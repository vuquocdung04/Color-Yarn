using UnityEngine;

public class Booster1InputMode : InputMode
{
    public override void OnClick2D(Collider2D collider)
    {
        var box = collider.GetComponentInParent<BoxSlot>();
        if (box == null || box.IsLocked || box.IsBusy) return;

        BoxCreator.Instance.Booster1Fill(box);
        BoosterController.Instance.OnBoosterActionSuccess();
    }
}
