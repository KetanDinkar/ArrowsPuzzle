using UnityEngine;
using UnityEngine.InputSystem;

public class TapInput : MonoBehaviour
{
    void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            Tap(Mouse.current.position.ReadValue());

        if (Touchscreen.current != null &&
            Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            Tap(Touchscreen.current.primaryTouch.position.ReadValue());
    }

    void Tap(Vector2 screenPos)
    {
        Vector2 world = Camera.main.ScreenToWorldPoint(screenPos);
        var hit = Physics2D.OverlapPoint(world);
        if (hit == null) return;
        var head = hit.GetComponent<ArrowheadRef>();
        if (head != null && head.owner != null)
            head.owner.Tap(hit.transform);
    }
}