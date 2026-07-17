using UnityEngine;
using UnityEngine.InputSystem;

public class InputController : MonoBehaviour
{
    public static InputController Instance { get; private set; }

    public void InitInstance() => Instance = this;

    [SerializeField] private float holdDuration = 0.5f;
    [SerializeField] private Material transparentMat;

    public Material TransparentMat => transparentMat;

    private Camera cam;
    private InputMode _currentMode;
    private InputMode _normalMode;
    private InputMode _booster0Mode;
    private InputMode _booster1Mode;
    private InputMode _booster2Mode;
    private InputMode _disabledMode;

    private RaycastHit _pressedHit;
    private bool _pressed3D;
    private bool _holding;
    private float _holdTimer;

    public bool CanInteract { get; private set; }

    public void Init()
    {
        cam = GamePlayController.Instance.cameraGameplay;

        _normalMode = new NormalInputMode();
        _booster0Mode = new Booster0InputMode();
        _booster1Mode = new Booster1InputMode();
        _booster2Mode = new Booster2InputMode();
        _disabledMode = new DisabledInputMode();

        SetMode(_normalMode);

        if (GameFlow.Instance != null)
        {
            CanInteract = GameFlow.Instance.CurrentState == GameState.Playing;
            GameFlow.Instance.OnStateEntered += HandleGameStateEntered;
            GameFlow.Instance.OnStateExited += HandleGameStateExited;
        }
    }

    private void HandleGameStateEntered(GameState state)
    {
        if (state == GameState.Playing) CanInteract = true;
    }

    private void HandleGameStateExited(GameState state)
    {
        if (state == GameState.Playing) CanInteract = false;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;

        if (GameFlow.Instance != null)
        {
            GameFlow.Instance.OnStateEntered -= HandleGameStateEntered;
            GameFlow.Instance.OnStateExited -= HandleGameStateExited;
        }
    }

    public void SetMode(InputMode newMode)
    {
        _currentMode?.OnExit();
        _currentMode = newMode;
        _currentMode?.OnEnter(this);
    }

    public void SetBooster0Mode() => SetMode(_booster0Mode);
    public void SetBooster1Mode() => SetMode(_booster1Mode);
    public void SetBooster2Mode() => SetMode(_booster2Mode);
    public void SetWaitingMode() => SetMode(_disabledMode);
    public void RestoreNormalMode() => SetMode(_normalMode);

    private void Update()
    {
        if (Pointer.current == null) return;

        if (!CanInteract)
        {
            CancelHold();
            return;
        }

        if (Pointer.current.press.wasPressedThisFrame)
            BeginPress();
        else if (_pressed3D)
            TrackPress();
    }

    private void BeginPress()
    {
        Vector2 screenPos = Pointer.current.position.ReadValue();

        Ray ray = cam.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            _pressedHit = hit;
            _pressed3D = true;
            _holding = false;
            _holdTimer = 0f;
            return;
        }

        RaycastHit2D hit2D = Physics2D.GetRayIntersection(ray);
        if (hit2D.collider != null)
            _currentMode.OnClick2D(hit2D.collider);
    }

    private void TrackPress()
    {
        if (_pressedHit.collider == null)
        {
            ResetPress();
            return;
        }

        if (Pointer.current.press.wasReleasedThisFrame)
        {
            if (_holding) _currentMode.OnHoldEnd(_pressedHit);
            else _currentMode.OnTap3D(_pressedHit);
            ResetPress();
            return;
        }

        if (!_holding)
        {
            _holdTimer += Time.deltaTime;
            if (_holdTimer >= holdDuration)
            {
                _holding = true;
                _currentMode.OnHoldStart(_pressedHit);
            }
        }
    }

    private void CancelHold()
    {
        if (_pressed3D && _holding && _pressedHit.collider != null)
            _currentMode.OnHoldEnd(_pressedHit);
        ResetPress();
    }

    private void ResetPress()
    {
        _pressed3D = false;
        _holding = false;
        _holdTimer = 0f;
        _pressedHit = default;
    }
}
