using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public partial class LevelController
{
    [Header("Zoom")]
    [SerializeField] private float scrollZoomSpeed = 0.01f;
    [SerializeField] private Slider zoomSlider;

    [Header("Rotate")]
    [SerializeField] private float rotationSpeed = 0.2f;

    private float CurrentZoomScale => CurrentLevelGO != null ? CurrentLevelGO.transform.localScale.x : 1f;

    private void InitCameraControls()
    {
        if (zoomSlider != null) zoomSlider.onValueChanged.AddListener(OnZoomSliderChanged);

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
        if (GameFlow.Instance != null)
        {
            GameFlow.Instance.OnStateEntered -= HandleGameStateEntered;
            GameFlow.Instance.OnStateExited -= HandleGameStateExited;
        }
    }

    private void ApplyZoom(float scale)
    {
        if (CurrentLevelGO == null) return;
        CurrentLevelGO.transform.localScale = Vector3.one * scale;
    }

    private void SyncZoomSlider(float scale)
    {
        if (zoomSlider != null)
            zoomSlider.SetValueWithoutNotify(Mathf.InverseLerp(MinZoomScale, MaxZoomScale, scale));
    }

    private void OnZoomSliderChanged(float value)
    {
        ApplyZoom(Mathf.Lerp(MinZoomScale, MaxZoomScale, value));
    }

    private bool CanInteract { get; set; }

    private void Update()
    {
        if (!CanInteract) return;
        HandleZoom();
        HandleRotate();
    }

    private void HandleZoom()
    {
        if (Mouse.current == null || CurrentLevelGO == null) return;

        float scrollValue = Mouse.current.scroll.ReadValue().y;
        if (scrollValue != 0)
        {
            float scale = Mathf.Clamp(
                CurrentZoomScale + scrollValue * scrollZoomSpeed,
                MinZoomScale, MaxZoomScale);

            ApplyZoom(scale);
            SyncZoomSlider(scale);
        }
    }

    public void PlayIntroZoom(float percent, float duration)
    {
        if (CurrentLevelGO == null) return;

        float target = Mathf.Lerp(MinZoomScale, MaxZoomScale, percent);
        DOTween.To(() => CurrentZoomScale, v =>
        {
            ApplyZoom(v);
            SyncZoomSlider(v);
        }, target, duration);
    }

    public void Prepare(GameIntroConfig config)
    {
        if (CurrentYarnObj != null)
            CurrentYarnObj.transform.rotation = Quaternion.identity;

        ApplyZoom(MinZoomScale);
        if (zoomSlider != null) zoomSlider.SetValueWithoutNotify(0f);
    }

    public async UniTask Play(GameIntroConfig config)
    {
        await CurrentYarnObj.SpinIntro(config.yarn.rotateY, config.yarn.spinDuration);

        PlayIntroZoom(config.yarn.zoomPercent, config.yarn.tiltDuration);
        await CurrentYarnObj.TiltIntro(config.yarn.tiltX, config.yarn.tiltDuration);
    }

    private void HandleRotate()
    {
        if (CurrentLevelGO == null) return;

        if (Pointer.current != null && Pointer.current.press.isPressed)
        {
            Vector2 delta = Pointer.current.delta.ReadValue();

            CurrentLevelGO.transform.Rotate(Vector3.up, -delta.x * rotationSpeed, Space.World);
            CurrentLevelGO.transform.Rotate(Vector3.right, delta.y * rotationSpeed, Space.World);
        }
    }
}
