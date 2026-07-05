using UnityEngine;
using UnityEngine.InputSystem;

public class ObjectInteractionManager : MonoBehaviour
{
    public static ObjectInteractionManager Instance { get; private set; }

    public Camera cam;
    public float holdDuration = 0.5f;
    public Material transparentMat;

    private InteractionContext context;
    private IPointerState state;

    private void Awake()
    {
        Instance = this;
        context = new InteractionContext(cam, holdDuration);
        state = PointerStates.Idle;
    }

    private void Update()
    {
        if (Pointer.current == null) return;
        state = state.Update(context);
    }
}
