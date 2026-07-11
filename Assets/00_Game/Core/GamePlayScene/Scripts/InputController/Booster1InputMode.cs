using UnityEngine;

public class Booster1InputMode : InputMode
{
    public override void HandleClick(RaycastHit hit)
    {
    }

    public override void HandleClick2D(RaycastHit2D hit)
    {
        var box = hit.collider.GetComponentInParent<BoxSlot>();
        if (box == null || box.IsLocked) return;

        if (BoxCreator.Instance.TryInstantFill(box))
            BoosterController.Instance.OnBoosterActionSuccess();
    }
}
