using UnityEngine;

public abstract class InputMode
{
    protected InputController Controller { get; private set; }

    public virtual void OnEnter(InputController controller)
    {
        Controller = controller;
    }

    public virtual void OnExit()
    {
    }

    public virtual void OnTap3D(RaycastHit hit) { }
    public virtual void OnHoldStart(RaycastHit hit) { }
    public virtual void OnHoldEnd(RaycastHit hit) { }
    public virtual void OnClick2D(Collider2D collider) { }
}
