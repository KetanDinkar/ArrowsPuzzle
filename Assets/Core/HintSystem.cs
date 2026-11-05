using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HintSystem : MonoBehaviour
{
    public float hitRadius = 0.4f;   // use the same value as your collision radius
    public float step = 0.05f;       // simulation time step in seconds
    public float maxTime = 20f;      // safety limit

    ArrowLine lastHint;

    public void ShowHint()
    {
        var all = FindObjectsByType<ArrowLine>(FindObjectsSortMode.None);
        foreach (var l in all) l.ClearHighlight();

        var moving = all.Where(l => l.IsMoving).ToList();
        var legal = new List<ArrowLine>();

        foreach (var l in all)
        {
            if (l.state != ArrowLine.State.Idle) continue;
            if (IsLegal(l, false, moving) || IsLegal(l, true, moving))
                legal.Add(l);
        }

        if (legal.Count == 0) return;

        // avoid showing the same line twice in a row when there is a choice
        if (legal.Count > 1 && lastHint != null) legal.Remove(lastHint);

        lastHint = legal[Random.Range(0, legal.Count)];
        lastHint.Highlight();
    }

    // Simulates starting 'cand' now and checks it against every line already moving
    bool IsLegal(ArrowLine cand, bool fromB, List<ArrowLine> moving)
    {
        for (float t = 0; t <= maxTime; t += step)
        {
            float cd = cand.Speed * t;
            if (cd >= cand.Total) return true;       // finished with no hit
            Vector2 cp = cand.PosAt(cd, fromB);

            foreach (var m in moving)
            {
                float md = m.CurrentDist + m.Speed * t;
                if (md >= m.Total) continue;         // that line has already finished
                Vector2 mp = m.PosAt(md, m.Reversed);
                if (Vector2.Distance(cp, mp) < hitRadius) return false;
            }
        }
        return true;
    }
}