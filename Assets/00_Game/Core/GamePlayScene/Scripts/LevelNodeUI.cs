using UnityEngine;
using UnityEngine.UI;

public class LevelNodeUI : MonoBehaviour
{
    [SerializeField] private Image imageIcon;
    [SerializeField] private Transform iconHard;
    [SerializeField] private CanvasGroup canvasGroup;

    public void SetIcon(Sprite sprite)
    {
        imageIcon.sprite = sprite;
    }

    public void SetHard(bool isHard)
    {
        iconHard.gameObject.SetActive(isHard);
    }

    public void SetCurrent(bool isCurrent)
    {
        canvasGroup.alpha = isCurrent ? 1f : 0.4f;
        transform.localScale = isCurrent ? Vector3.one * 1.2f : Vector3.one;
    }

    public void AutoAssignRefs()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        imageIcon = transform.GetChild(0).GetComponent<Image>();
        iconHard = transform.GetChild(1);
    }
}
