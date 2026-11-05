using System.Collections.Generic;
using UnityEngine;

public class CollisionSystem : MonoBehaviour
{
    public LifeSystem life;
    public float hitRadius = 0.3f;

    void Update()
    {
        var moving = new List<ArrowLine>();
        foreach (var l in FindObjectsByType<ArrowLine>(FindObjectsSortMode.None))
            if (l.state == ArrowLine.State.Moving) moving.Add(l);

        var toReset = new HashSet<ArrowLine>();
        int hits = 0;

        for (int i = 0; i < moving.Count; i++)
            for (int j = i + 1; j < moving.Count; j++)
                if (Vector2.Distance(moving[i].HeadPos, moving[j].HeadPos) < hitRadius)
                {
                    toReset.Add(moving[i]);
                    toReset.Add(moving[j]);
                    hits++;
                }

        foreach (var l in toReset) l.ResetLine();
        if (hits > 0) life.Lose(hits);
    }
}