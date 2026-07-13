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
    private Camera cam;

    private void InitCameraControls()
    {
        cam = GamePlayController.Instance.cameraGameplay;
        if (zoomSlider != null) zoomSlider.onValueChanged.AddListener(OnZoomSliderChanged);
    }

    private void OnZoomSliderChanged(float value)
    {
        if (cam == null) return;
        cam.orthographicSize = Mathf.Lerp(MaxZoomCamera, MinZoomCamera, value);
    }

    private void Update()
    {
        HandleZoom();
        HandleRotate();
    }

    private void HandleZoom()
    {
        if (Mouse.current == null || cam == null) return;

        float scrollValue = Mouse.current.scroll.ReadValue().y;
        if (scrollValue != 0)
        {
            cam.orthographicSize = Mathf.Clamp(
                cam.orthographicSize - scrollValue * scrollZoomSpeed,
                MinZoomCamera, MaxZoomCamera);

            if (zoomSlider != null)
            {
                float normalized = Mathf.InverseLerp(MaxZoomCamera, MinZoomCamera, cam.orthographicSize);
                zoomSlider.SetValueWithoutNotify(normalized);
            }
        }
    }

    public void PlayIntroZoom(float percent, float duration)
    {
        if (cam == null) return;

        float target = Mathf.Lerp(MaxZoomCamera, MinZoomCamera, percent);
        DOTween.To(() => cam.orthographicSize, v =>
        {
            cam.orthographicSize = v;
            if (zoomSlider != null)
                zoomSlider.SetValueWithoutNotify(Mathf.InverseLerp(MaxZoomCamera, MinZoomCamera, v));
        }, target, duration);
    }

    public void PrepareYarnIntro()
    {
        if (CurrentYarnObj != null)
            CurrentYarnObj.transform.rotation = Quaternion.identity;

        if (cam != null)
        {
            cam.orthographicSize = MaxZoomCamera;
            if (zoomSlider != null) zoomSlider.SetValueWithoutNotify(0f);
        }
    }

    public async UniTask PlayYarnIntro(float rotateY, float spinDuration, float tiltX, float tiltDuration, float zoomPercent)
    {
        await CurrentYarnObj.SpinIntro(rotateY, spinDuration);

        PlayIntroZoom(zoomPercent, tiltDuration);
        await CurrentYarnObj.TiltIntro(tiltX, tiltDuration);
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
