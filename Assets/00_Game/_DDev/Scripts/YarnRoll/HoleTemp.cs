using DG.Tweening;
using UnityEngine;

public class HoleTemp : MonoBehaviour
{
    [SerializeField] private Transform anchor;

    public Transform Anchor => anchor != null ? anchor : transform;

    public void ShiftWave(Vector3 offset, float duration, float delay)
    {
        transform.DOMove(transform.position + offset, duration).SetDelay(delay).SetEase(Ease.OutQuad);
    }

    public void ScaleIn(float duration)
    {
        gameObject.SetActive(true);
        transform.localScale = Vector3.zero;
        transform.DOScale(1f, duration);
    }
}
