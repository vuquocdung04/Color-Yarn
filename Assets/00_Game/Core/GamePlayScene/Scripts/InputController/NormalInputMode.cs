using EventDispatcher;
using UnityEngine;

public class NormalInputMode : InputMode
{
    public override void HandleClick(RaycastHit hit)
    {
    }

    public override void HandleClick2D(RaycastHit2D hit)
    {
        var box = hit.collider.GetComponentInParent<BoxSlot>();
        if (box != null) box.OnTapped();
    }
}