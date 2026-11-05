using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LineData
{
    public string id = "line";
    public Color color = Color.red;

    // Bezier path. Every 3 control points after the first make one curve segment:
    // P0, (C1, C2, P1), (C1, C2, P2), ...
    // Count must be 1 + 3*n. This is the data the spline is built from.
    public List<Vector2> controlPoints = new List<Vector2>();
}

[CreateAssetMenu(menuName = "Arrows/Level Data", fileName = "Level_01")]
public class LevelData : ScriptableObject
{
    public List<LineData> lines = new List<LineData>();
}