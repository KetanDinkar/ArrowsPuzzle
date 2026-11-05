using UnityEngine;

public class LevelLoader : MonoBehaviour
{
    public LevelData level;
    public GameObject headPrefab;

    void Awake()
    {
        foreach (var data in level.lines)
        {
            var go = new GameObject("Line_" + data.id);
            go.transform.SetParent(transform);
            var line = go.AddComponent<ArrowLine>();
            line.Build(data, headPrefab);
        }
    }
}