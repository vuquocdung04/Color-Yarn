using UnityEngine;

public class DisabledInputMode : InputMode
{
    public override void HandleClick(RaycastHit hit)
    {
    }

    public override void HandleClick2D(RaycastHit2D hit)
    {
    }
}