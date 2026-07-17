using DG.Tweening;
using Spine.Unity;
using UnityEngine;

public class Bloom : MonoBehaviour
{
    [SerializeField] private SkeletonAnimation skeletonAnimation;
    [SerializeField] private ParticleSystem particle;
    [SerializeField] private string animationName = "Brush2";
    [SerializeField, Range(0f, 3f)] private float holdStartDuration = 0f;
    [SerializeField, Range(0f, 3f)] private float moveDuration = 0.5f;
    [SerializeField, Range(0f, 3f)] private float holdEndDuration = 0f;

    public void Activate(System.Action onComplete = null)
    {
        var rolls = HolesTemp.Instance.ClearAll();

        gameObject.SetActive(true);

        Vector3 p = transform.position;
        p.x = HolesTemp.Instance.FirstActiveHoleX;
        transform.position = p;

        skeletonAnimation.AnimationState.SetAnimation(0, animationName, false);

        AudioManager.Instance.PlaySfx("Bloom");

        DOTween.Sequence()
            .AppendInterval(holdStartDuration)
            .AppendCallback(() => particle.Play())
            .Append(transform.DOMoveX(HolesTemp.Instance.LastActiveHoleX, moveDuration).SetEase(Ease.Linear))
            .AppendInterval(holdEndDuration)
            .OnComplete(() =>
            {
                particle.Stop();
                gameObject.SetActive(false);
                AweSomeBox.Instance.Store(rolls, () => onComplete?.Invoke());
            });
    }
}
