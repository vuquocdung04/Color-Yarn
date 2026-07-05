using System.Collections.Generic;
using UnityEngine;

public class BoxCreator : MonoBehaviour
{
    public static BoxCreator Instance { get; private set; }

    public void InitInstance() => Instance = this;

    [SerializeField] private List<BoxSlot> boxSlots;

    public void Init()
    {
        if (boxSlots.Count > 0) boxSlots[0].SetColor("Crimson");
    }

    public void Spawn(InteractableObject target)
    {
        string key = target.ColorKey;
        float duration = HolesTemp.Instance != null ? HolesTemp.Instance.Duration : 2f;

        foreach (var box in boxSlots)
        {
            if (box.CanAccept(key))
            {
                box.Spawn(target, duration);
                return;
            }
        }

        HolesTemp.Instance?.Spawn(target);
    }
}
