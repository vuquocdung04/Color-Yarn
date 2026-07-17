using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public partial class InteractableObject : MonoBehaviour, IInteractable
{
    public event Action<float> OnDissolveStart;

    private float duration = 0.5f;
    private float growDuration;
    private Ease  growEase;

    private YarnObj _controller;
    private YarnObj Owner
    {
        get
        {
            if (_controller == null) _controller = GetComponentInParent<YarnObj>();
            return _controller;
        }
    }
    private Transform Root => Owner != null ? Owner.Root : null;

    private Renderer _renderer;
    private Material _mat;
    private int _dissolveAmountPropId;
    private Vector3 savedScale;

    [SerializeField] private string colorKey;
    public string ColorKey => colorKey;

    public int Layer { get; private set; }

    public void DecrementLayer()
    {
        if (Layer > 1) Layer--;
    }

    public bool IsBusy { get; private set; }

    public void Init()
    {
        if (_mat != null) return;
        _renderer = GetComponent<Renderer>();

        if (string.IsNullOrEmpty(colorKey))
            colorKey = CleanMaterialName(_renderer.sharedMaterial.name);

        _mat = _renderer.material;
        _dissolveAmountPropId = Shader.PropertyToID("_DissolveAmount");
        if (_mat.HasProperty(_dissolveAmountPropId))
            _mat.SetFloat(_dissolveAmountPropId, 0f);

        if (Owner != null)
        {
            growDuration = Owner.GrowDuration;
            growEase = Owner.GrowEase;
        }

        var parentObj = transform.parent != null ? transform.parent.GetComponentInParent<InteractableObject>() : null;
        savedScale = parentObj != null ? parentObj.savedScale : transform.localScale;
        Layer = parentObj != null ? parentObj.Layer + 1 : 1;
    }

    private static string CleanMaterialName(string matName)
    {
        if (string.IsNullOrEmpty(matName)) return matName;
        int idx = matName.IndexOf(" (Instance)");
        return idx >= 0 ? matName.Substring(0, idx) : matName;
    }

    public void OnTap()
    {
        if (IsBusy) return;
        if (BoxCreator.Instance == null || !BoxCreator.Instance.TrySpawn(this)) return;
        AudioManager.Instance.PlaySfx("Yarn");
        DissolveSequence(this.GetCancellationTokenOnDestroy()).Forget();
    }

    public void OnHoldStart() => SetSeeThrough(true);
    public void OnHoldEnd()   => SetSeeThrough(false);

    private async UniTaskVoid DissolveSequence(CancellationToken token)
    {
        IsBusy = true;

        GameObject coreGo = core;

        Owner?.RemoveLen(this);

        if (coreGo != null)
        {
            Owner?.AddLen(coreGo.GetComponent<InteractableObject>());
            coreGo.transform.SetParent(Root, true);

            foreach (var obj in coreGo.GetComponentsInChildren<InteractableObject>())
                if (obj != null) obj.DecrementLayer();
        }

        Init();
        float dur = HolesTemp.Instance != null ? HolesTemp.Instance.Duration : duration;
        OnDissolveStart?.Invoke(dur);
        float t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            _mat.SetFloat(_dissolveAmountPropId, Mathf.Clamp01(t / dur));
            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }
        _mat.SetFloat(_dissolveAmountPropId, 1f);

        if (coreGo != null)
            _ = coreGo.transform.DOScale(savedScale, growDuration).SetEase(growEase);

        Destroy(gameObject);
    }

    public void InstantConsume()
    {
        GameObject coreGo = core;
        var parentObj = transform.parent != null ? transform.parent.GetComponent<InteractableObject>() : null;

        if (parentObj == null)
        {
            Owner?.RemoveLen(this);

            if (coreGo != null)
            {
                Owner?.AddLen(coreGo.GetComponent<InteractableObject>());
                coreGo.transform.SetParent(Root, true);

                foreach (var obj in coreGo.GetComponentsInChildren<InteractableObject>())
                    if (obj != null) obj.DecrementLayer();

                _ = coreGo.transform.DOScale(savedScale, growDuration).SetEase(growEase);
            }
        }
        else
        {
            if (coreGo != null)
            {
                coreGo.transform.SetParent(parentObj.transform, false);
                coreGo.transform.localPosition = Vector3.zero;
                coreGo.transform.localRotation = Quaternion.identity;
                coreGo.transform.localScale = Vector3.one * scaleRatio;

                foreach (var obj in coreGo.GetComponentsInChildren<InteractableObject>())
                    if (obj != null) obj.DecrementLayer();
            }

            parentObj.core = coreGo;
        }

        Destroy(gameObject);
    }
}