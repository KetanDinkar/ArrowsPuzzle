using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowLine : MonoBehaviour
{
    public enum State { Idle, Moving, Done }

    public State state = State.Idle;
    public Transform headA, headB;
    public float speed = 1.6f;
    public float vanishTime = 0.5f;

    List<Vector2> pts;
    float[] cum;
    float total;
    LineRenderer lr;

    Transform mover;
    bool reversed;
    float dist;

    public static System.Action<ArrowLine> OnCompleted;
    public Vector2 HeadPos => mover != null ? (Vector2)mover.position : Vector2.zero;

    public bool IsMoving => state == State.Moving;
    public float Total => total;
    public float Speed => speed;
    public float CurrentDist => dist;
    public bool Reversed => reversed;

    public void Build(LineData data, GameObject headPrefab)
    {
        pts = BezierPath.Sample(data.controlPoints);
        if (pts.Count < 2) return;

        // cumulative distance along the path
        cum = new float[pts.Count];
        for (int i = 1; i < pts.Count; i++)
            cum[i] = cum[i - 1] + Vector2.Distance(pts[i], pts[i - 1]);
        total = cum[pts.Count - 1];

        lr = gameObject.AddComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = lr.endColor = data.color;
        lr.startWidth = lr.endWidth = 0.036f;
        lr.numCapVertices = 4;
        lr.numCornerVertices = 4;
        lr.useWorldSpace = true;
        DrawFull();

        headA = MakeHead(headPrefab, data.color);
        headB = MakeHead(headPrefab, data.color);
        PlaceHeadsAtEnds();
    }

    Transform MakeHead(GameObject prefab, Color color)
    {
        var go = Instantiate(prefab, transform);
        go.GetComponent<SpriteRenderer>().color = color;
        go.GetComponent<ArrowheadRef>()?.SetOwner(this);
        return go.transform;
    }

    void PlaceHeadsAtEnds()
    {
        headA.position = pts[0];
        headA.up = (pts[1] - pts[0]).normalized;
        headB.position = pts[pts.Count - 1];
        headB.up = (pts[pts.Count - 2] - pts[pts.Count - 1]).normalized;
    }

    void DrawFull()
    {
        lr.positionCount = pts.Count;
        for (int i = 0; i < pts.Count; i++) lr.SetPosition(i, pts[i]);
    }

    // Returns position and direction at distance d from the start of the path
    Vector2 Sample(float d, out Vector2 dir)
    {
        d = Mathf.Clamp(d, 0, total);
        int i = 1;
        while (i < pts.Count - 1 && cum[i] < d) i++;
        float segLen = cum[i] - cum[i - 1];
        float t = segLen > 0 ? (d - cum[i - 1]) / segLen : 0;
        dir = (pts[i] - pts[i - 1]).normalized;
        return Vector2.Lerp(pts[i - 1], pts[i], t);
    }

    // Called when the player taps one of the heads
    public void Tap(Transform head)
    {
        if (state != State.Idle) return;

        ClearHighlight();

        state = State.Moving;
        mover = head;
        reversed = (head == headB);
        dist = 0;
        // the other head is disabled while this one moves
        (reversed ? headA : headB).GetComponent<Collider2D>().enabled = false;
    }

    void Update()
    {
        if (state != State.Moving) return;

        dist += speed * Time.deltaTime;
        if (dist >= total)
        {
            state = State.Done;
            StartCoroutine(Vanish());
            return;
        }

        float d = reversed ? total - dist : dist;
        Vector2 pos = Sample(d, out Vector2 dir);
        mover.position = pos;
        mover.up = reversed ? -dir : dir;
        RedrawRemaining(d);
    }

    // Hides the part of the line the head already covered
    void RedrawRemaining(float d)
    {
        var rest = new List<Vector3>();
        Vector2 headPos = Sample(d, out _);
        if (!reversed)
        {
            rest.Add(headPos);
            for (int i = 0; i < pts.Count; i++)
                if (cum[i] > d) rest.Add(pts[i]);
        }
        else
        {
            for (int i = 0; i < pts.Count; i++)
                if (cum[i] < d) rest.Add(pts[i]);
            rest.Add(headPos);
        }
        lr.positionCount = rest.Count;
        lr.SetPositions(rest.ToArray());
    }

    public void ResetLine()
    {
        if (state != State.Moving) return;
        state = State.Idle;
        mover = null;
        dist = 0;
        DrawFull();
        PlaceHeadsAtEnds();
        headA.GetComponent<Collider2D>().enabled = true;
        headB.GetComponent<Collider2D>().enabled = true;
    }

    public Vector2 PosAt(float travelled, bool fromB)
    {
        float d = fromB ? total - travelled : travelled;
        return Sample(d, out _);
    }

    // ---------- Vanish (same as the tutorial: heads grow to 1.6x and fade) ----------
    IEnumerator Vanish()
    {
        var srA = headA.GetComponent<SpriteRenderer>();
        var srB = headB.GetComponent<SpriteRenderer>();
        headA.GetComponent<Collider2D>().enabled = false;
        headB.GetComponent<Collider2D>().enabled = false;

        // snap the moving head onto the far end so both heads sit together
        mover.position = reversed ? (Vector3)pts[0] : (Vector3)pts[pts.Count - 1];
        lr.enabled = false;

        Vector3 sA = headA.localScale, sB = headB.localScale;
        Color cA = srA.color, cB = srB.color;

        for (float t = 0; t < vanishTime; t += Time.deltaTime)
        {
            float k = t / vanishTime;
            float s = 1f + 0.6f * k;
            headA.localScale = sA * s;
            headB.localScale = sB * s;
            srA.color = new Color(cA.r, cA.g, cA.b, 1f - k);
            srB.color = new Color(cB.r, cB.g, cB.b, 1f - k);
            yield return null;
        }

        OnCompleted?.Invoke(this);   // win check happens after the vanish
        Destroy(gameObject);
    }

    // ---------- Hint highlight (one line at a time) ----------
    Coroutine pulse;
    Vector3 scaleA, scaleB;
    float baseWidth;
    bool scalesSaved;

    public void Highlight()
    {
        if (state != State.Idle) return;
        ClearHighlight();
        if (!scalesSaved)
        {
            scaleA = headA.localScale;
            scaleB = headB.localScale;
            baseWidth = lr.startWidth;
            scalesSaved = true;
        }
        pulse = StartCoroutine(Pulse());
    }

    public void ClearHighlight()
    {
        if (pulse != null) { StopCoroutine(pulse); pulse = null; }
        if (!scalesSaved) return;
        if (headA) headA.localScale = scaleA;
        if (headB) headB.localScale = scaleB;
        if (lr) lr.startWidth = lr.endWidth = baseWidth;
    }

    IEnumerator Pulse()
    {
        float t = 0;
        while (true)
        {
            t += Time.deltaTime;
            float s = 1f + 0.4f * Mathf.Abs(Mathf.Sin(t * 6f));
            headA.localScale = scaleA * s;
            headB.localScale = scaleB * s;
            lr.startWidth = lr.endWidth = baseWidth * s;
            yield return null;
        }
    }
}