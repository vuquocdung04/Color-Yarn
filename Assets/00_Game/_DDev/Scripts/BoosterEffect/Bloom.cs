using Spine;
using Spine.Unity;
using UnityEngine;

public class Bloom : MonoBehaviour
{
    [SerializeField] private SkeletonAnimation skeletonAnimation;
    [SerializeField] private string animationName = "Brush2";

    public void Activate(System.Action onComplete = null)
    {
        var rolls = HolesTemp.Instance.ClearAll();
        gameObject.SetActive(true);
        skeletonAnimation.AnimationState.SetAnimation(0, animationName, false);
        skeletonAnimation.AnimationState.Complete += OnBrushComplete;

        void OnBrushComplete(TrackEntry entry)
        {
            skeletonAnimation.AnimationState.Complete -= OnBrushComplete;
            gameObject.SetActive(false);
            AweSomeBox.Instance.Store(rolls, () => onComplete?.Invoke());
        }
    }
}
