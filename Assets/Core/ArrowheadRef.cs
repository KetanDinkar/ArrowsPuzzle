using UnityEngine;

public class ArrowheadRef : MonoBehaviour
{
    public ArrowLine owner;
    public void SetOwner(ArrowLine line) { owner = line; }
}