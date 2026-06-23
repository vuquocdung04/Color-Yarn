using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public Camera cam;
    public float rotationSpeed = 0.2f;
    public float scrollZoomSpeed = 0.2f; 
    public float minZoom = 2f;
    public float maxZoom = 15f;

    private float xRotation = 0f;
    private float yRotation = 0f;

    private void Start()
    {
        Vector3 angles = transform.eulerAngles;
        xRotation = angles.x;
        yRotation = angles.y;
    }

    private void Update()
    {
        if (Pointer.current != null && Pointer.current.press.isPressed)
        {
            Vector2 delta = Pointer.current.delta.ReadValue();

            yRotation += delta.x * rotationSpeed;
            xRotation -= delta.y * rotationSpeed; 

            transform.rotation = Quaternion.Euler(xRotation, yRotation, 0f);
        }

        if (Mouse.current != null)
        {
            float scrollValue = Mouse.current.scroll.ReadValue().y;
            
            if (scrollValue != 0)
            {
                cam.orthographicSize = Mathf.Clamp(cam.orthographicSize - scrollValue * scrollZoomSpeed, minZoom, maxZoom);
            }
        }
    }
}