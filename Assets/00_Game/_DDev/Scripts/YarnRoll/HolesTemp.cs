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

    public void Init() => _index = 0;

    // Sinh 1 YarnRoll tai hole ke tiep (0 -> cuoi list), nham target lam B
    public void Spawn(InteractableObject target)
    {
        if (target == null || yarnRollPrefab == null || holes == null || holes.Count == 0) return;

        Transform hole = holes[_index];
        _index = (_index + 1) % holes.Count;

        YarnRoll yr = Instantiate(yarnRollPrefab, hole.position, hole.rotation, hole);
        yr.Setup(target.GetComponent<Renderer>(), target.ColorKey);
        yr.Play(duration);
    }
}
