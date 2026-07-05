using UnityEngine;
using Sirenix.OdinInspector;

public partial class InteractableObject
{
    [Header("Nesting")]
    [SerializeField, Min(0)] public int totalCube = 2;
    [SerializeField, Range(0.1f, 0.95f)] public float scaleRatio = 0.7f;

    private GameObject core;
    private bool seeThrough;

    public GameObject Core => core;
    public int  TotalCube => totalCube;
    public bool HasCore   => core != null;

    [Button("Build Core", ButtonSizes.Large), GUIColor(0.4f, 0.8f, 1f)]
    public void BuildCore()
    {
        ClearCore();
        if (totalCube <= 0) return;

        core = Instantiate(gameObject, transform);
        core.name = $"{name}_core";

        StripChildren(core.transform);

        core.transform.localPosition = Vector3.zero;
        core.transform.localRotation = Quaternion.identity;
        core.transform.localScale = Vector3.one * scaleRatio;

        var child = core.GetComponent<InteractableObject>();
        child.totalCube = totalCube - 1;
        child.scaleRatio = scaleRatio;
        child.ApplyRandomColor();

        child.BuildCore();
    }

    private void StripChildren(Transform t)
    {
        for (int i = t.childCount - 1; i >= 0; i--)
        {
            var c = t.GetChild(i).gameObject;
            if (Application.isPlaying) Destroy(c);
            else DestroyImmediate(c);
        }
    }

    public void ClearCore()
    {
        if (core == null) return;
        if (Application.isPlaying) Destroy(core);
        else DestroyImmediate(core);
        core = null;
    }

    public void SetSeeThrough(bool on)
    {
        seeThrough = on;
        Init();
        if (on)
        {
            var mat = ObjectInteractionManager.Instance != null ? ObjectInteractionManager.Instance.transparentMat : null;
            if (mat != null) _renderer.material = mat;
        }
        else
        {
            _renderer.material = _mat;
        }
    }

    public void ApplyRandomColor()
    {
        var entry = ColorRepo.Instance != null ? ColorRepo.Instance.GetRandom() : null;
        if (entry == null) return;

        Init();
        if (entry.material != null)
        {
            _renderer.material = entry.material;
            _mat = _renderer.material;
        }
        colorKey = entry.key;
    }
}