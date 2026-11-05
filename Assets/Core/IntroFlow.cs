using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class IntroFlow : MonoBehaviour
{
    // Stays true after the intro, so Retry skips it
    public static bool SkipIntro;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatic() { SkipIntro = false; }

    public Canvas canvas;
    public TapInput tapInput;
    public Sprite playButton, heartFull, heartBroken, heartEmpty, youWon, youLost, titleLogo;

    static readonly Color Ink = new Color(0.13f, 0.15f, 0.22f);
    static readonly Color Bg = new Color(0.96f, 0.97f, 1f);
    static readonly Color Blue = new Color(0.10f, 0.50f, 1f);
    static readonly Color Red = new Color(1f, 0.15f, 0.15f);
    static readonly Color Green = new Color(0.18f, 0.74f, 0.36f);

    Sprite circle, tri, rounded;
    RectTransform root, startScreen, controls, playRT;
    RectTransform[] pages = new RectTransform[3];
    Image[] pageDots = new Image[3];
    TextMeshProUGUI nextLabel;
    int page;

    Track t1, t2a, t2b;
    Image tapRing, burst;
    TextMeshProUGUI tapLabel;
    Image[] demoHearts = new Image[3];
    RectTransform banLost, banWon;



    // ---------------------------------------------------------------- setup
    void Awake()
    {
        if (SkipIntro) return;
        if (tapInput == null) tapInput = GetComponent<TapInput>();
        if (tapInput != null) tapInput.enabled = false;
    }

    void Start()
    {
        if (SkipIntro) return;
        if (canvas == null) canvas = FindFirstObjectByType<Canvas>();
        circle = MakeCircle();
        tri = MakeTriangle();
        rounded = MakeRounded();
        Build();
        ShowStart();
    }

    void Build()
    {
        root = NewRect("IntroRoot", canvas.transform);
        Stretch(root);
        root.gameObject.AddComponent<Image>().color = Bg;
        root.SetAsLastSibling();

        // Start screen
        startScreen = NewRect("StartScreen", root);
        Stretch(startScreen);
        var logo = NewImage("Logo", startScreen, titleLogo, Color.white, new Vector2(900, 615), new Vector2(0, 360));
        logo.preserveAspect = true;
        var play = NewImage("PlayButton", startScreen, playButton, Color.white, new Vector2(400, 400), new Vector2(0, -300));
        play.raycastTarget = true;
        var pb = play.gameObject.AddComponent<Button>();
        pb.targetGraphic = play;
        pb.onClick.AddListener(() => ShowPage(0));
        playRT = play.rectTransform;

        // Pages
        string[] titles = { "Tap and Go", "Avoid Collisions", "Win or Lose" };
        string[] descs =
        {
            "Tap an arrowhead. It follows its line to the other end and clears the line.",
            "If two moving arrowheads collide, you lose a life.",
            "Lose all your lives and you lose. Clear every line and you win."
        };
        for (int i = 0; i < 3; i++)
        {
            var p = NewRect("Page" + i, root);
            Stretch(p);
            pages[i] = p;
            var tt = NewText("Title", p, titles[i], 96, new Vector2(0, 730), new Vector2(950, 150));
            tt.fontStyle = FontStyles.Bold; tt.color = Ink;
            var stage = NewImage("Stage", p, rounded, Color.white, new Vector2(900, 760), new Vector2(0, 90));
            stage.type = Image.Type.Sliced;
            var dd = NewText("Desc", p, descs[i], 54, new Vector2(0, -490), new Vector2(920, 260));
            dd.color = Ink;
            if (i == 0) BuildDemo1(stage.rectTransform);
            else if (i == 1) BuildDemo2(stage.rectTransform);
            else BuildDemo3(stage.rectTransform);
        }

        // Shared controls (Next, Skip, page dots)
        controls = NewRect("Controls", root);
        Stretch(controls);
        var next = NewButton("Next", controls, "Next", new Vector2(0, -740), new Vector2(520, 150), Green, Color.white, 64);
        next.onClick.AddListener(OnNext);
        nextLabel = next.GetComponentInChildren<TextMeshProUGUI>();
        var skip = NewButton("Skip", controls, "Skip", Vector2.zero, new Vector2(230, 100), new Color(0.88f, 0.9f, 0.96f), new Color(0.35f, 0.4f, 0.5f), 44);
        skip.onClick.AddListener(Finish);
        var srt = (RectTransform)skip.transform;
        srt.anchorMin = srt.anchorMax = srt.pivot = new Vector2(1, 1);
        srt.anchoredPosition = new Vector2(-40, -60);
        for (int i = 0; i < 3; i++)
            pageDots[i] = NewImage("Dot" + i, controls, circle, Color.gray, new Vector2(28, 28), new Vector2((i - 1) * 56, -885));
    }

    void ShowStart()
    {
        startScreen.gameObject.SetActive(true);
        controls.gameObject.SetActive(false);
        foreach (var p in pages) p.gameObject.SetActive(false);
        StartCoroutine(Pulse(playRT));
    }

    void ShowPage(int i)
    {
        page = i;
        StopAllCoroutines();
        startScreen.gameObject.SetActive(false);
        controls.gameObject.SetActive(true);
        for (int k = 0; k < pages.Length; k++)
        {
            pages[k].gameObject.SetActive(k == i);
            pageDots[k].color = k == i ? Blue : new Color(0.8f, 0.84f, 0.92f);
        }
        nextLabel.text = i == pages.Length - 1 ? "Let's Play" : "Next";
        if (i == 0) StartCoroutine(RunDemo1());
        else if (i == 1) StartCoroutine(RunDemo2());
        else StartCoroutine(RunDemo3());
    }

    void OnNext()
    {
        if (page < pages.Length - 1) ShowPage(page + 1);
        else Finish();
    }

    void Finish()
    {
        StopAllCoroutines();
        SkipIntro = true;
        root.gameObject.SetActive(false);
        if (tapInput != null) tapInput.enabled = true;
    }

    // ---------------------------------------------------------------- demos
    void BuildDemo1(RectTransform stage)
    {
        var lines = NewRect("Lines", stage); Stretch(lines);
        var heads = NewRect("Heads", stage); Stretch(heads);
        t1 = MakeTrack(lines, heads, Blue, new[]
        {
            new Vector2(-330, -170), new Vector2(-120, 230),
            new Vector2(120, -290), new Vector2(330, 170)
        });
        tapRing = NewImage("TapRing", heads, circle, Color.clear, new Vector2(110, 110), Vector2.zero);
        tapLabel = NewText("TapLabel", heads, "TAP", 54, Vector2.zero, new Vector2(300, 80));
        tapLabel.fontStyle = FontStyles.Bold; tapLabel.color = Blue;
    }

    IEnumerator RunDemo1()
    {
        while (true)
        {
            t1.Reset();
            tapRing.color = Color.clear;
            tapLabel.gameObject.SetActive(false);
            Vector2 a = t1.pts[0];
            tapRing.rectTransform.anchoredPosition = a;
            tapLabel.rectTransform.anchoredPosition = a + new Vector2(0, -90);
            yield return new WaitForSeconds(0.8f);

            tapLabel.gameObject.SetActive(true);
            for (float t = 0; t < 0.5f; t += Time.deltaTime)
            {
                float k = t / 0.5f;
                tapRing.rectTransform.localScale = Vector3.one * Mathf.Lerp(0.4f, 1.8f, k);
                tapRing.color = new Color(Blue.r, Blue.g, Blue.b, 1f - k);
                yield return null;
            }
            tapRing.color = Color.clear;
            tapLabel.gameObject.SetActive(false);

            float dur = 2.4f;
            for (float t = 0; t < dur; t += Time.deltaTime)
            {
                t1.Progress(Mathf.SmoothStep(0, 1, t / dur) * t1.total, true);
                yield return null;
            }
            t1.Progress(t1.total, true);

            for (float t = 0; t < 0.5f; t += Time.deltaTime)
            {
                float k = t / 0.5f;
                t1.SetAlpha(1f - k);
                t1.headA.rectTransform.localScale = t1.headB.rectTransform.localScale = Vector3.one * (1f + 0.6f * k);
                yield return null;
            }
            t1.SetAlpha(0f);
            yield return new WaitForSeconds(0.9f);
        }
    }

    void BuildDemo2(RectTransform stage)
    {
        var lines = NewRect("Lines", stage); Stretch(lines);
        var heads = NewRect("Heads", stage); Stretch(heads);
        t2a = MakeTrack(lines, heads, Blue, Straight(new Vector2(-280, -60), new Vector2(280, -60)));
        t2b = MakeTrack(lines, heads, Red, Straight(new Vector2(0, -340), new Vector2(0, 220)));
        burst = NewImage("Burst", heads, circle, Color.clear, new Vector2(170, 170), new Vector2(0, -60));
        for (int i = 0; i < 3; i++)
            demoHearts[i] = NewImage("Heart" + i, heads, heartFull, Color.white, new Vector2(90, 90), new Vector2((i - 1) * 130, 305));
    }

    IEnumerator RunDemo2()
    {
        while (true)
        {
            t2a.Reset(); t2b.Reset();
            foreach (var h in demoHearts) { h.sprite = heartFull; h.rectTransform.localScale = Vector3.one; }
            burst.color = Color.clear;
            yield return new WaitForSeconds(0.9f);

            float meet = 262f, dur = 1.8f;
            for (float t = 0; t < dur; t += Time.deltaTime)
            {
                float d = Mathf.Lerp(0, meet, t / dur);
                t2a.Progress(d, true);
                t2b.Progress(d, true);
                yield return null;
            }

            for (float t = 0; t < 0.4f; t += Time.deltaTime)
            {
                float k = t / 0.4f;
                burst.rectTransform.localScale = Vector3.one * Mathf.Lerp(0.3f, 2f, k);
                burst.color = new Color(1f, 0.6f, 0.1f, 1f - k);
                yield return null;
            }
            burst.color = Color.clear;

            var h3 = demoHearts[2];
            h3.sprite = heartBroken;
            for (float t = 0; t < 0.6f; t += Time.deltaTime)
            {
                h3.rectTransform.localScale = Vector3.one * (1f + 0.4f * Mathf.Sin(t / 0.6f * Mathf.PI));
                yield return null;
            }
            h3.rectTransform.localScale = Vector3.one;
            h3.sprite = heartEmpty;

            t2a.Reset(); t2b.Reset();
            yield return new WaitForSeconds(1.6f);
        }
    }

    void BuildDemo3(RectTransform stage)
    {
        for (int i = 0; i < 3; i++)
            NewImage("Heart" + i, stage, heartEmpty, Color.white, new Vector2(100, 100), new Vector2((i - 1) * 140, 300));
        banLost = NewImage("Lost", stage, youLost, Color.white, new Vector2(540, 203), new Vector2(0, 140)).rectTransform;
        banLost.GetComponent<Image>().preserveAspect = true;
        var line = NewImage("Divider", stage, rounded, new Color(0.88f, 0.9f, 0.95f), new Vector2(700, 8), new Vector2(0, 0));
        line.type = Image.Type.Sliced;
        var lbl = NewText("AllClear", stage, "All lines cleared", 54, new Vector2(0, -70), new Vector2(800, 80));
        lbl.fontStyle = FontStyles.Bold; lbl.color = Green;
        banWon = NewImage("Won", stage, youWon, Color.white, new Vector2(540, 203), new Vector2(0, -210)).rectTransform;
        banWon.GetComponent<Image>().preserveAspect = true;
    }

    IEnumerator RunDemo3()
    {
        float t = 0;
        while (true)
        {
            t += Time.deltaTime;
            banLost.localScale = Vector3.one * (1f + 0.04f * Mathf.Sin(t * 3f));
            banWon.localScale = Vector3.one * (1f + 0.04f * Mathf.Sin(t * 3f + Mathf.PI));
            yield return null;
        }
    }

    IEnumerator Pulse(RectTransform rt)
    {
        float t = 0;
        while (true)
        {
            t += Time.deltaTime;
            rt.localScale = Vector3.one * (1f + 0.06f * Mathf.Sin(t * 3f));
            yield return null;
        }
    }

    // ---------------------------------------------------------------- track (a line made of dots + 2 arrowheads)
    class Track
    {
        public Color col;
        public Vector2[] pts;
        public float[] cum;
        public float total;
        public List<Image> dots = new List<Image>();
        public List<float> dotD = new List<float>();
        public Image headA, headB;

        public Vector2 PosAt(float d, out Vector2 dir)
        {
            d = Mathf.Clamp(d, 0f, total);
            int i = 1;
            while (i < pts.Length - 1 && cum[i] < d) i++;
            float seg = cum[i] - cum[i - 1];
            float k = seg > 0.0001f ? (d - cum[i - 1]) / seg : 0f;
            dir = (pts[i] - pts[i - 1]).normalized;
            return Vector2.Lerp(pts[i - 1], pts[i], k);
        }

        public static void Face(Image head, Vector2 dir)
        {
            head.rectTransform.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f);
        }

        public void Reset()
        {
            Vector2 dir;
            foreach (var dot in dots) dot.enabled = true;
            PosAt(0f, out dir);
            headA.rectTransform.anchoredPosition = pts[0];
            Face(headA, dir);
            PosAt(total, out dir);
            headB.rectTransform.anchoredPosition = pts[pts.Length - 1];
            Face(headB, -dir);
            headA.rectTransform.localScale = headB.rectTransform.localScale = Vector3.one;
            SetAlpha(1f);
        }

        public void SetAlpha(float a)
        {
            var c = col; c.a = a;
            foreach (var dot in dots) dot.color = c;
            headA.color = c; headB.color = c;
        }

        public void Progress(float d, bool fromA)
        {
            var mover = fromA ? headA : headB;
            Vector2 dir;
            Vector2 p = PosAt(fromA ? d : total - d, out dir);
            mover.rectTransform.anchoredPosition = p;
            Face(mover, fromA ? dir : -dir);
            for (int i = 0; i < dots.Count; i++)
                dots[i].enabled = fromA ? dotD[i] > d : dotD[i] < total - d;
        }
    }

    Track MakeTrack(RectTransform lines, RectTransform heads, Color col, Vector2[] ctrl)
    {
        var t = new Track { col = col };
        t.pts = BezierPath.Sample(new List<Vector2>(ctrl), 40).ToArray();
        t.cum = new float[t.pts.Length];
        for (int i = 1; i < t.pts.Length; i++)
            t.cum[i] = t.cum[i - 1] + Vector2.Distance(t.pts[i], t.pts[i - 1]);
        t.total = t.cum[t.cum.Length - 1];
        for (float d = 0; d <= t.total; d += 9f)
        {
            Vector2 dir;
            Vector2 p = t.PosAt(d, out dir);
            t.dots.Add(NewImage("dot", lines, circle, col, new Vector2(15, 15), p));
            t.dotD.Add(d);
        }
        t.headA = NewImage("A", heads, tri, col, new Vector2(64, 64), t.pts[0]);
        t.headB = NewImage("B", heads, tri, col, new Vector2(64, 64), t.pts[t.pts.Length - 1]);
        t.Reset();
        return t;
    }

    static Vector2[] Straight(Vector2 a, Vector2 b)
    {
        return new[] { a, a + (b - a) / 3f, a + (b - a) * 2f / 3f, b };
    }

    // ---------------------------------------------------------------- UI helpers
    static RectTransform NewRect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        return rt;
    }

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }

    static Image NewImage(string name, Transform parent, Sprite s, Color c, Vector2 size, Vector2 pos)
    {
        var rt = NewRect(name, parent);
        rt.sizeDelta = size; rt.anchoredPosition = pos;
        var img = rt.gameObject.AddComponent<Image>();
        img.sprite = s; img.color = c; img.raycastTarget = false;
        return img;
    }

    static TextMeshProUGUI NewText(string name, Transform parent, string text, float size, Vector2 pos, Vector2 box)
    {
        var rt = NewRect(name, parent);
        rt.sizeDelta = box; rt.anchoredPosition = pos;
        var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
        t.text = text; t.fontSize = size;
        t.alignment = TextAlignmentOptions.Center;
        t.color = Ink; t.raycastTarget = false;
        return t;
    }

    Button NewButton(string name, Transform parent, string label, Vector2 pos, Vector2 size, Color bg, Color fg, float fontSize)
    {
        var img = NewImage(name, parent, rounded, bg, size, pos);
        img.type = Image.Type.Sliced;
        img.raycastTarget = true;
        var b = img.gameObject.AddComponent<Button>();
        b.targetGraphic = img;
        var t = NewText("Label", img.transform, label, fontSize, Vector2.zero, size);
        t.color = fg; t.fontStyle = FontStyles.Bold;
        return b;
    }

    // ---------------------------------------------------------------- procedural sprites (no art files needed)
    static Sprite MakeCircle()
    {
        int n = 64; float r = n / 2f;
        var tex = NewTex(n);
        for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(r, r));
                tex.SetPixel(x, y, new Color(1, 1, 1, Mathf.Clamp01(r - d)));
            }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), 100f);
    }

    static Sprite MakeTriangle()
    {
        int n = 64;
        var a = new Vector2(n * 0.5f, n * 0.92f);
        var b = new Vector2(n * 0.08f, n * 0.10f);
        var c = new Vector2(n * 0.92f, n * 0.10f);
        var tex = NewTex(n);
        for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                int hit = 0;
                for (int sy = 0; sy < 4; sy++)
                    for (int sx = 0; sx < 4; sx++)
                        if (InTri(new Vector2(x + (sx + 0.5f) / 4f, y + (sy + 0.5f) / 4f), a, b, c)) hit++;
                tex.SetPixel(x, y, new Color(1, 1, 1, hit / 16f));
            }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), 100f);
    }

    static Sprite MakeRounded()
    {
        int n = 64, rad = 22;
        var tex = NewTex(n);
        for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float px = Mathf.Abs(x + 0.5f - n / 2f) - (n / 2f - rad);
                float py = Mathf.Abs(y + 0.5f - n / 2f) - (n / 2f - rad);
                float dist = Mathf.Sqrt(Mathf.Max(px, 0) * Mathf.Max(px, 0) + Mathf.Max(py, 0) * Mathf.Max(py, 0)) - rad;
                tex.SetPixel(x, y, new Color(1, 1, 1, Mathf.Clamp01(0.5f - dist)));
            }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), 100f, 0,
            SpriteMeshType.FullRect, new Vector4(rad, rad, rad, rad));
    }

    static Texture2D NewTex(int n)
    {
        var t = new Texture2D(n, n, TextureFormat.RGBA32, false);
        t.filterMode = FilterMode.Bilinear;
        t.wrapMode = TextureWrapMode.Clamp;
        return t;
    }

    static float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
    }

    static bool InTri(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        float d1 = Sign(p, a, b), d2 = Sign(p, b, c), d3 = Sign(p, c, a);
        bool neg = d1 < 0 || d2 < 0 || d3 < 0;
        bool pos = d1 > 0 || d2 > 0 || d3 > 0;
        return !(neg && pos);
    }
}