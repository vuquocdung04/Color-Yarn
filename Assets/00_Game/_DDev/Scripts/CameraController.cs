using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Zoom")]
    public Camera cam;
    public float scrollZoomSpeed = 0.01f;
    public float minZoom = 2f;
    public float maxZoom = 15f;

    [Header("Rotate")]
    public GameObject rotateTarget; // tam thoi public de xu ly viec xoay
    public float rotationSpeed = 0.2f;

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
                cam.orthographicSize - scrollValue * scrollZoomSpeed, minZoom, maxZoom);
        }
    }

    private void HandleRotate()
    {
        if (rotateTarget == null) return;

        if (Pointer.current != null && Pointer.current.press.isPressed)
        {
            Vector2 delta = Pointer.current.delta.ReadValue();

            rotateTarget.transform.Rotate(Vector3.up, -delta.x * rotationSpeed, Space.World);
            rotateTarget.transform.Rotate(Vector3.right, delta.y * rotationSpeed, Space.World);
        }
    }
}
