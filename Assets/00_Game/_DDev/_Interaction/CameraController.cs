using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public Camera cam;
    public float scrollZoomSpeed = 0.01f; 
    public float minZoom = 2f;
    public float maxZoom = 15f;

    private void Update()
    {
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