using DG.Tweening;
using UnityEngine;

namespace UIJuice
{
    public enum IconAnimationType
    {
        Pulse,
        Wiggle
    }

    public class IconAnimator : MonoBehaviour
    {
        [SerializeField] private IconAnimationType animationType = IconAnimationType.Pulse;

        [Header("Pulse")]
        [SerializeField] private float pulseScale = 1.1f;
        [SerializeField] private float pulseDuration = 0.6f;

        [Header("Wiggle")]
        [SerializeField] private float wiggleAngle = 10f;
        [SerializeField] private float wiggleDuration = 0.4f;
        [SerializeField] private float wiggleInterval = 2f;

        private Tween tween;
        private Vector3 baseScale;

        private void OnEnable()
        {
            baseScale = transform.localScale;

            switch (animationType)
            {
                case IconAnimationType.Pulse:
                    PlayPulse();
                    break;
                case IconAnimationType.Wiggle:
                    PlayWiggle();
                    break;
            }
        }

        private void OnDisable()
        {
            tween?.Kill();
            transform.localScale = baseScale;
            transform.localRotation = Quaternion.identity;
        }

        private void PlayPulse()
        {
            tween = transform.DOScale(baseScale * pulseScale, pulseDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }

        private void PlayWiggle()
        {
            tween = DOTween.Sequence()
                .AppendInterval(wiggleInterval)
                .Append(transform.DOPunchRotation(new Vector3(0f, 0f, wiggleAngle), wiggleDuration, 10, 1f))
                .SetLoops(-1);
        }
    }
}
