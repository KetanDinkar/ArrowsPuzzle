using UnityEditor;
using UnityEngine;

public static class GenerateLevel01
{
    [MenuItem("Tools/Arrows/Generate Level 01")]
    static void Generate()
    {
        var level = AssetDatabase.LoadAssetAtPath<LevelData>("Assets/Levels/Level_01.asset");
        if (level == null) { Debug.LogError("Assets/Levels/Level_01.asset not found"); return; }

        level.lines.Clear();

        var pink = new Color(0.98f, 0.35f, 0.85f);
        var blue = new Color(0.10f, 0.50f, 1.00f);
        var orange = new Color(0.82f, 0.50f, 0.33f);
        var green = new Color(0.15f, 0.85f, 0.15f);
        var red = new Color(1.00f, 0.05f, 0.05f);
        var purple = new Color(0.55f, 0.10f, 1.00f);

        // Pink loops
        Add(level, "pink_top", pink, -1.31f, 1.59f, -1.57f, 1.83f, -1.59f, 2.33f, -1.19f, 2.37f, -0.79f, 2.38f, -0.74f, 1.93f, -1.09f, 1.65f);
        Add(level, "pink_right", pink, -0.94f, 1.37f, -0.59f, 1.53f, -0.14f, 1.38f, -0.24f, 0.98f, -0.34f, 0.68f, -0.69f, 0.73f, -1.01f, 1.13f);
        Add(level, "pink_left", pink, -1.52f, 1.33f, -1.89f, 1.43f, -1.99f, 0.93f, -1.69f, 0.82f, -1.44f, 0.78f, -1.29f, 0.98f, -1.29f, 1.13f);

        // Blue
        Add(level, "blue_long", blue, -1.11f, 0.78f, -1.19f, 0.13f, -0.89f, -0.67f, -0.12f, -1.28f);
        Add(level, "blue_loop", blue, -0.02f, -0.39f, -0.14f, 0.23f, 0.21f, 0.75f, 0.61f, 0.74f, 1.06f, 0.71f, 1.16f, 0.03f, 0.86f, -0.59f);

        // Orange
        Add(level, "orange_top", orange, 1.94f, -0.29f, 1.94f, 0.21f, 1.16f, 0.28f, 0.82f, -0.15f);
        Add(level, "orange_low", orange, -0.08f, -1.35f, 0.13f, -1.57f, 0.26f, -1.77f, 0.23f, -2.00f);

        // Green
        Add(level, "green_right", green, 1.90f, -0.53f, 1.81f, -1.07f, 1.46f, -1.39f, 1.04f, -1.48f);
        Add(level, "green_leaf_top", green, -0.84f, 0.45f, -0.99f, 0.78f, -0.49f, 0.93f, -0.12f, 0.97f, -0.21f, 0.53f, -0.29f, 0.15f, -0.79f, 0.24f);
        Add(level, "green_leaf_left", green, -1.18f, -0.42f, -1.54f, -0.32f, -1.67f, -0.67f, -1.70f, -1.26f, -1.29f, -1.17f, -0.89f, -0.97f, -1.09f, -0.62f);

        // Red
        Add(level, "red_top", red, -1.09f, 0.25f, -0.89f, 0.35f, -0.69f, 0.38f, -0.54f, 0.55f);
        Add(level, "red_left", red, -1.38f, -0.77f, -1.24f, -0.52f, -1.04f, -0.47f, -0.84f, -0.37f);
        Add(level, "red_bottom", red, -0.24f, -2.05f, -0.37f, -2.37f, 0.06f, -2.47f, 0.15f, -2.22f);

        // Purple
        Add(level, "purple", purple, -0.06f, -2.00f, 0.31f, -1.92f, 0.71f, -1.69f, 0.95f, -1.51f);

        EditorUtility.SetDirty(level);
        AssetDatabase.SaveAssets();
        Debug.Log("Level_01 generated with " + level.lines.Count + " lines");
    }

    static void Add(LevelData level, string id, Color c, params float[] xy)
    {
        var d = new LineData { id = id, color = c };
        for (int i = 0; i < xy.Length; i += 2)
            d.controlPoints.Add(new Vector2(xy[i], xy[i + 1]));
        level.lines.Add(d);
    }
}