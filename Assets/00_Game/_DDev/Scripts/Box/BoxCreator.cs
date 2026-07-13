using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class BoxCreator : MonoBehaviour, IIntroStep
{
    public static BoxCreator Instance { get; private set; }

    public void InitInstance() => Instance = this;

    [SerializeField] private List<BoxSlot> boxSlots;
    [SerializeField] private YarnRoll yarnRollPrefab;

    [SerializeField] private float coverUpOffset = 6f;
    [SerializeField] private float boxHopOffset = 7f;
    [SerializeField] private float boxAnimDuration = 0.3f;

    [SerializeField] private float boxDipOffset = 0.5f;
    [SerializeField] private float boxDipDuration = 0.1f;
    [SerializeField] private float boxHoldDuration = 0.1f;

    public float CoverUpOffset => coverUpOffset;
    public float BoxHopOffset => boxHopOffset;
    public float BoxAnimDuration => boxAnimDuration;

    public float BoxDipOffset => boxDipOffset;
    public float BoxDipDuration => boxDipDuration;
    public float BoxHoldDuration => boxHoldDuration;

    public bool HasAnyBoxClosing => boxSlots.Exists(b => b.IsClosing);

    public void Init()
    {
    }

    public void SetInitialColors()
    {
        var yarnObj = LevelController.Instance.CurrentYarnObj;
        var rootColors = yarnObj.GetRootColorKeys();
        var used = new HashSet<string>();

        for (int i = 0; i < boxSlots.Count && i < 2; i++)
        {
            string key = FindUnused(rootColors, used);
            if (key == null) key = FindUnused(yarnObj.TotalByColor.Keys, used);

            if (key == null) continue;
            used.Add(key);
            GameAlgorithm.Instance.Reserve(key);
            boxSlots[i].SetColor(key);
        }
    }

    private static string FindUnused(IEnumerable<string> candidates, HashSet<string> used)
    {
        foreach (var c in candidates)
            if (!used.Contains(c)) return c;
        return null;
    }

    public void Prepare(GameIntroConfig config)
    {
        for (int i = 0; i < boxSlots.Count; i++)
        {
            bool isLeft = i < boxSlots.Count / 2;
            boxSlots[i].PrepareIntro(isLeft ? -config.box.offsetX : config.box.offsetX);
        }
    }

    public async UniTask Play(GameIntroConfig config)
    {
        float moveDuration = config.box.moveDuration;
        int innerLeft = boxSlots.Count / 2 - 1;
        int innerRight = boxSlots.Count / 2;

        for (int i = 0; i < boxSlots.Count; i++)
            if (i == innerLeft || i == innerRight)
                boxSlots[i].PlayIntroMove(moveDuration);

        await UniTask.Delay((int)(config.box.waveDelay * 1000));

        for (int i = 0; i < boxSlots.Count; i++)
            if (i != innerLeft && i != innerRight)
                boxSlots[i].PlayIntroMove(moveDuration);

        await UniTask.Delay((int)(moveDuration * 1000));
    }

    public bool TryInstantFill(BoxSlot box) => box.TryInstantFill(yarnRollPrefab);

    public bool TrySpawn(InteractableObject target)
    {
        string key = target.ColorKey;
        float duration = HolesTemp.Instance != null ? HolesTemp.Instance.Duration : 2f;

        foreach (var box in boxSlots)
        {
            if (box.CanAccept(key))
            {
                box.Spawn(target, yarnRollPrefab, duration);
                return true;
            }
        }

        return HolesTemp.Instance != null && HolesTemp.Instance.TrySpawn(target);
    }
}
