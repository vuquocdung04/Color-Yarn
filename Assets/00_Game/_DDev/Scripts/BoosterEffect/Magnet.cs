using System;
using Sirenix.OdinInspector;
using Spine;
using Spine.Unity;
using UnityEngine;

public class Magnet : MonoBehaviour
{
    private const string APPEAR_IN = "Apear_in";
    private const string SUCK = "Suck";
    private const string APPEAR_OUT = "Apear_out";

    [SerializeField] private GameObject root;
    [SerializeField] private SkeletonAnimation rope;
    [SerializeField] private SkeletonAnimation vacuum;
    [SerializeField] private string ropePrefix = "Rope";
    [SerializeField] private string vacuumPrefix = "Vacuum";

    public void Activate(Action onSuck = null, Action onDone = null)
    {
        root.SetActive(true);
        PlayBoth(APPEAR_IN, () =>
        {
            onSuck?.Invoke();
            PlayBoth(SUCK, () =>
                PlayBoth(APPEAR_OUT, () =>
                {
                    root.SetActive(false);
                    onDone?.Invoke();
                }));
        });
    }

    private void PlayBoth(string anim, Action onComplete)
    {
        int remaining = 2;
        void Done() { if (--remaining == 0) onComplete?.Invoke(); }

        PlayOne(rope, $"{ropePrefix}/{anim}", Done);
        PlayOne(vacuum, $"{vacuumPrefix}/{anim}", Done);
    }

    private void PlayOne(SkeletonAnimation skeleton, string anim, Action onComplete)
    {
        TrackEntry entry = skeleton.AnimationState.SetAnimation(0, anim, false);
        entry.Complete += _ => onComplete();
    }

    [Button("Test Magnet")]
    private void TestMagnet() => Activate();
}
