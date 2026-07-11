using UnityEngine;
using UnityEngine.InputSystem;

public class InputController : MonoBehaviour
{
    public static InputController Instance { get; private set; }

    public void InitInstance() => Instance = this;

    private Camera cam;
    private Camera camUI;
    private InputMode _currentMode;
    private InputMode _normalMode;
    private InputMode _booster0Mode;
    private InputMode _booster2Mode;
    private InputMode _disabledMode;

    public void Init()
    {
        cam = GamePlayController.Instance.cameraGameplay;
        camUI = GamePlayController.Instance.cameraUI;

        _normalMode = new NormalInputMode();
        _booster0Mode = new Booster0InputMode();
        _booster2Mode = new Booster2InputMode();
        _disabledMode = new DisabledInputMode();

        SetMode(_normalMode);

        GameFlow.Instance.OnStateEntered += OnGameStateChanged;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        GameFlow.Instance.OnStateEntered -= OnGameStateChanged;
    }

    private void OnGameStateChanged(GameState newState)
    {
        bool shouldDisable = newState == GameState.Win ||
                            newState == GameState.Lose ||
                            newState == GameState.Paused ||
                            newState == GameState.Tutorial;

        if (shouldDisable)
            SetMode(_disabledMode);
        else
            SetMode(_normalMode);
    }

    public void SetMode(InputMode newMode)
    {
        _currentMode?.OnExit();
        _currentMode = newMode;
        _currentMode?.OnEnter(this);
    }

    public void SetBooster0Mode() => SetMode(_booster0Mode);
    public void SetBooster2Mode() => SetMode(_booster2Mode);
    public void SetWaitingMode() => SetMode(_disabledMode);
    public void RestoreNormalMode() => SetMode(_normalMode);

    private void Update()
    {
        if (Pointer.current == null) return;
        if (!Pointer.current.press.wasPressedThisFrame) return;

        HandleClick();
    }

    private void HandleClick()
    {
        Vector2 screenPos = Pointer.current.position.ReadValue();
        Ray ray = cam.ScreenPointToRay(screenPos);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            _currentMode.HandleClick(hit);
            return;
        }

        Ray rayUI = camUI.ScreenPointToRay(screenPos);
        RaycastHit2D hit2D = Physics2D.Raycast(rayUI.origin, rayUI.direction);
        if (hit2D.collider != null)
            _currentMode.HandleClick2D(hit2D);
    }
}