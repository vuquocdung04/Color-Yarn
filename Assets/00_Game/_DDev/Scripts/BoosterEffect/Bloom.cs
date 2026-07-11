using Spine;
using Spine.Unity;
using UnityEngine;

public class Bloom : MonoBehaviour
{
    [SerializeField] private SkeletonAnimation skeletonAnimation;
    [SerializeField] private string animationName = "Brush2";

    public void Activate()
    {
        var rolls = HolesTemp.Instance.ClearAll();
        Debug.LogError("ewqewq");
        gameObject.SetActive(true);
        skeletonAnimation.AnimationState.SetAnimation(0, animationName, false);
        skeletonAnimation.AnimationState.Complete += OnBrushComplete;

        void OnBrushComplete(TrackEntry entry)
        {
            skeletonAnimation.AnimationState.Complete -= OnBrushComplete;
            gameObject.SetActive(false);
            AweSomeBox.Instance.Store(rolls, () => BoosterController.Instance.OnBoosterActionSuccess());
        }
    }
}
