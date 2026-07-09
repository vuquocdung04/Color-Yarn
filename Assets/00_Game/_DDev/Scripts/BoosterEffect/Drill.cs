using System;
using DG.Tweening;
using UnityEngine;

public class Drill : MonoBehaviour
{
    [SerializeField] private ParticleSystem particle;
    [SerializeField] private float holdDuration = 0.2f;
    [SerializeField] private float moveDuration = 0.3f;
    [SerializeField] private float duration = 0.7f;

    public void MoveIn(float targetX, Action onArrived)
    {
        gameObject.SetActive(true);
        transform.localScale = Vector3.one;

        DOVirtual.DelayedCall(holdDuration, () =>
        {
            transform.DOMoveX(targetX, moveDuration).SetEase(Ease.OutQuad)
                .OnComplete(() => onArrived?.Invoke());
        });
    }

    public void StartDrilling(Action onDrillStart)
    {
        onDrillStart?.Invoke();

        transform.DORotate(new Vector3(0f, -35f, 0f), duration);
        transform.DOScale(1.15f, duration * 0.5f).SetLoops(2, LoopType.Yoyo);

        if (particle != null) particle.Play();

        DOVirtual.DelayedCall(duration, () => gameObject.SetActive(false));
    }
}
