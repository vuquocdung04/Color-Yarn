using UnityEngine;

public class NormalInputMode : InputMode
{
    public override void OnTap3D(RaycastHit hit)
    {
        if (hit.collider.TryGetComponent(out IInteractable interactable))
            interactable.OnTap();
    }

    public override void OnHoldStart(RaycastHit hit)
    {
        if (hit.collider.TryGetComponent(out IInteractable interactable))
            interactable.OnHoldStart();
    }

    public override void OnHoldEnd(RaycastHit hit)
    {
        if (hit.collider.TryGetComponent(out IInteractable interactable))
            interactable.OnHoldEnd();
    }

    public override void OnClick2D(Collider2D collider)
    {
        var box = collider.GetComponentInParent<BoxSlot>();
        if (box != null) box.OnTapped();
    }
}
