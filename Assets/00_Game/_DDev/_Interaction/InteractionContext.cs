using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionContext
{
    public readonly Camera Cam;
    public readonly float HoldDuration;

    public IInteractable Current;
    public float Timer;

    public InteractionContext(Camera cam, float holdDuration)
    {
        Cam = cam;
        HoldDuration = holdDuration;
    }

    public bool TryPick(out IInteractable interactable)
    {
        interactable = null;
        Vector2 pos = Pointer.current.position.ReadValue();
        Ray ray = Cam.ScreenPointToRay(pos);
        if (Physics.Raycast(ray, out RaycastHit hit))
            return hit.collider.TryGetComponent(out interactable);
        return false;
    }

    public bool CurrentIsAlive =>
        Current is MonoBehaviour mb && mb && mb.gameObject.activeSelf;

    public void Clear()
    {
        Current = null;
        Timer = 0f;
    }
}
