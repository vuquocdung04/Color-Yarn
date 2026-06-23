using UnityEngine;
using UnityEngine.InputSystem;

public interface IPointerState
{
    IPointerState Update(InteractionContext ctx);
}

public static class PointerStates
{
    public static readonly IPointerState Idle = new IdleState();
    public static readonly IPointerState Pressing = new PressingState();
    public static readonly IPointerState Holding = new HoldingState();
}

public class IdleState : IPointerState
{
    public IPointerState Update(InteractionContext ctx)
    {
        if (Pointer.current.press.wasPressedThisFrame && ctx.TryPick(out var interactable))
        {
            ctx.Current = interactable;
            ctx.Timer = 0f;
            return PointerStates.Pressing;
        }
        return this;
    }
}

public class PressingState : IPointerState
{
    public IPointerState Update(InteractionContext ctx)
    {
        if (!ctx.CurrentIsAlive)
        {
            ctx.Clear();
            return PointerStates.Idle;
        }

        if (Pointer.current.press.wasReleasedThisFrame)
        {
            ctx.Current.OnTap();
            ctx.Clear();
            return PointerStates.Idle;
        }

        ctx.Timer += Time.deltaTime;
        if (ctx.Timer >= ctx.HoldDuration)
        {
            ctx.Current.OnHoldStart();
            return PointerStates.Holding;
        }
        return this;
    }
}

public class HoldingState : IPointerState
{
    public IPointerState Update(InteractionContext ctx)
    {
        if (!ctx.CurrentIsAlive)
        {
            ctx.Clear();
            return PointerStates.Idle;
        }

        if (Pointer.current.press.wasReleasedThisFrame)
        {
            ctx.Current.OnHoldEnd();
            ctx.Clear();
            return PointerStates.Idle;
        }
        return this;
    }
}
