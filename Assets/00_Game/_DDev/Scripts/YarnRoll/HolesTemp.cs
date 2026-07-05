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

    private int _index;
    private readonly List<YarnRoll> parked = new();

    public void Init() => _index = 0;

    public void Spawn(InteractableObject target)
    {
        if (target == null || yarnRollPrefab == null || holes == null || holes.Count == 0) return;

        Transform hole = holes[_index];
        _index = (_index + 1) % holes.Count;

        YarnRoll yr = Instantiate(yarnRollPrefab, hole.position, hole.rotation, hole);
        yr.Setup(target.GetComponent<Renderer>(), target.ColorKey);
        yr.Play(duration);

        parked.Add(yr);
    }

    public List<YarnRoll> TakeMatching(string key, int max)
    {
        var result = new List<YarnRoll>();
        for (int i = parked.Count - 1; i >= 0 && result.Count < max; i--)
        {
            var r = parked[i];
            if (r == null) { parked.RemoveAt(i); continue; }
            if (r.ColorKey == key)
            {
                result.Add(r);
                parked.RemoveAt(i);
            }
        }
        return result;
    }
}
