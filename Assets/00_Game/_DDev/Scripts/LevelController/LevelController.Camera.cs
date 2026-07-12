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
