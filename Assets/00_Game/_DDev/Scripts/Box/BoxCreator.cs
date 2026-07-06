using System.Collections.Generic;
using UnityEngine;

public class BoxCreator : MonoBehaviour
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

    public void Init()
    {
        if (boxSlots.Count > 0) boxSlots[0].SetColor("Crimson");
    }

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
