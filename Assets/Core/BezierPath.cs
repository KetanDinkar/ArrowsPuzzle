using System.Collections.Generic;
using UnityEngine;

public static class BezierPath
{
    public static List<Vector2> Sample(List<Vector2> cp, int perSegment = 30)
    {
        var result = new List<Vector2>();
        if (cp == null || cp.Count < 4) return result;

        int segments = (cp.Count - 1) / 3;
        for (int s = 0; s < segments; s++)
        {
            Vector2 p0 = cp[s * 3];
            Vector2 p1 = cp[s * 3 + 1];
            Vector2 p2 = cp[s * 3 + 2];
            Vector2 p3 = cp[s * 3 + 3];

            int start = (s == 0) ? 0 : 1;
            for (int i = start; i <= perSegment; i++)
            {
                float t = i / (float)perSegment;
                float u = 1f - t;
                result.Add(u * u * u * p0
                         + 3 * u * u * t * p1
                         + 3 * u * t * t * p2
                         + t * t * t * p3);
            }
        }
        return result;
    }
}