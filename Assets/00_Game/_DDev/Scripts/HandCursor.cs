using UnityEngine;
using UnityEngine.InputSystem;

public class HandCursor : MonoBehaviour
{
    public Camera cam;
    public Transform hand;
    public SpriteRenderer handRenderer;
    public Sprite spriteClick;
    public Sprite spriteUnClick;
    public float depth = 10f;

    private void Awake()
    {
        if (cam == null) cam = Camera.main;
        if (handRenderer == null && hand != null) handRenderer = hand.GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        SetSprite(spriteUnClick);
    }

    private void Update()
    {
        if (Pointer.current == null || cam == null || hand == null) return;

        FollowMouse();
        UpdatePressState();
    }

    private void FollowMouse()
    {
        Vector3 screenPos = Pointer.current.position.ReadValue();
        screenPos.z = depth;
        hand.position = cam.ScreenToWorldPoint(screenPos);
    }

    private void UpdatePressState()
    {
        if (Pointer.current.press.wasPressedThisFrame)
            SetSprite(spriteClick);
        else if (Pointer.current.press.wasReleasedThisFrame)
            SetSprite(spriteUnClick);
    }

    private void SetSprite(Sprite sprite)
    {
        if (handRenderer != null && sprite != null)
            handRenderer.sprite = sprite;
    }
}
