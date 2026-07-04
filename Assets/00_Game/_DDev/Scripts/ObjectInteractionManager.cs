using UnityEngine;
using UnityEngine.InputSystem;

public class ObjectInteractionManager : MonoBehaviour
{
    public Camera cam;
    public float holdDuration = 0.5f;

    private InteractionContext context;
    private IPointerState state;

    private void Awake()
    {
        context = new InteractionContext(cam, holdDuration);
        state = PointerStates.Idle;
    }

    private void Update()
    {
        if (Pointer.current == null) return;
        state = state.Update(context);
    }
}
