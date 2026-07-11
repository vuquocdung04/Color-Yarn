using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public partial class InteractableObject : MonoBehaviour, IInteractable
{
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
        DissolveSequence(this.GetCancellationTokenOnDestroy()).Forget();
    }

    public void OnHoldStart() => SetSeeThrough(true);
    public void OnHoldEnd()   => SetSeeThrough(false);

    // Ca chuoi tu chay: tach loi -> dissolve minh -> huy minh -> loi phinh
    private async UniTaskVoid DissolveSequence(CancellationToken token)
    {
        IsBusy = true;

        GameObject coreGo = core;

        Owner?.RemoveLen(this);

        // 1) tach loi ra Root, giu nguyen world position/scale
        if (coreGo != null)
        {
            Owner?.AddLen(coreGo.GetComponent<InteractableObject>());
            coreGo.transform.SetParent(Root, true);

            foreach (var obj in coreGo.GetComponentsInChildren<InteractableObject>())
                if (obj != null) obj.DecrementLayer();
        }

        // 2) dissolve vo minh (chung duration voi YarnRoll qua HolesTemp)
        Init();
        float dur = HolesTemp.Instance != null ? HolesTemp.Instance.Duration : duration;
        float t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            _mat.SetFloat(_dissolveAmountPropId, Mathf.Clamp01(t / dur));
            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }
        _mat.SetFloat(_dissolveAmountPropId, 1f);

        // 3) loi phinh ve 1 ngay tai cho (loi cung la InteractableObject, tu co nest cua no)
        if (coreGo != null)
            _ = coreGo.transform.DOScale(savedScale, growDuration).SetEase(growEase);

        // 4) huy vo
        Destroy(gameObject);
    }

    public void InstantConsume()
    {
        GameObject coreGo = core;

        Owner?.RemoveLen(this);

        if (coreGo != null)
        {
            Owner?.AddLen(coreGo.GetComponent<InteractableObject>());
            coreGo.transform.SetParent(Root, true);

            foreach (var obj in coreGo.GetComponentsInChildren<InteractableObject>())
                if (obj != null) obj.DecrementLayer();

            _ = coreGo.transform.DOScale(savedScale, growDuration).SetEase(growEase);
        }

        Destroy(gameObject);
    }

    public void ResetDissolve()
    {
        Init();
        IsBusy = false;
        if (_mat.HasProperty(_dissolveAmountPropId))
            _mat.SetFloat(_dissolveAmountPropId, 0f);
        gameObject.SetActive(true);
    }
}