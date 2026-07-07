using System.Collections.Generic;
using UnityEngine;

public class HolesTemp : MonoBehaviour
{
    public static HolesTemp Instance { get; private set; }

    public void InitInstance() => Instance = this;

    [SerializeField] private List<Transform> holes;
    [SerializeField] private YarnRoll yarnRollPrefab;
    [SerializeField] private float duration = 2f;

    public float Duration => duration;

    private YarnRoll[] occupants;

    public bool IsFull
    {
        get
        {
            foreach (var o in occupants) if (o == null) return false;
            return true;
        }
    }

    public void Init()
    {
        occupants = new YarnRoll[holes != null ? holes.Count : 0];
    }

    public bool TrySpawn(InteractableObject target)
    {
        if (target == null || yarnRollPrefab == null || holes == null || holes.Count == 0) return false;

        int idx = FindEmptySlot();
        if (idx < 0) return false;

        Transform hole = holes[idx];
        YarnRoll yr = Instantiate(yarnRollPrefab, hole.position, hole.rotation, hole);
        yr.Setup(target.GetComponent<Renderer>(), target.ColorKey);
        yr.Play(duration);

        occupants[idx] = yr;

        GameFlow.Instance?.CheckLose();
        return true;
    }

    private int FindEmptySlot()
    {
        for (int i = 0; i < occupants.Length; i++)
            if (occupants[i] == null) return i;
        return -1;
    }

    public Dictionary<string, int> GetParkedColorCounts()
    {
        var result = new Dictionary<string, int>();
        foreach (var o in occupants)
        {
            if (o == null || string.IsNullOrEmpty(o.ColorKey)) continue;
            result[o.ColorKey] = result.TryGetValue(o.ColorKey, out int c) ? c + 1 : 1;
        }
        return result;
    }

    public List<YarnRoll> TakeMatching(string key, int max)
    {
        var result = new List<YarnRoll>();
        for (int i = 0; i < occupants.Length && result.Count < max; i++)
        {
            var r = occupants[i];
            if (r == null) continue;
            if (r.ColorKey == key)
            {
                result.Add(r);
                occupants[i] = null;
            }
        }
        return result;
    }
}
