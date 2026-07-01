using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public partial class InteractableObject : MonoBehaviour, IInteractable
{
    [Header("Dissolve / Grow")]
    public float duration = 0.5f;
    public float growDuration = 0.35f;
    public Ease  growEase = Ease.OutBack;

    private CubePhaseController _controller;
    private Transform Root
    {
        get
        {
            if (_controller == null) _controller = GetComponentInParent<CubePhaseController>();
            return _controller != null ? _controller.Root : null;
        }
    }

    private Renderer _renderer;
    private Material _mat;
    private int _dissolveAmountPropId;

    public bool IsBusy { get; private set; }

    private void Awake() => Init();

    private void Init()
    {
        if (_mat != null) return;
        _renderer = GetComponent<Renderer>();
        _mat = _renderer.material;
        _dissolveAmountPropId = Shader.PropertyToID("_DissolveAmount");
        if (_mat.HasProperty(_dissolveAmountPropId))
            _mat.SetFloat(_dissolveAmountPropId, 0f);
    }

    public void OnTap()
    {
        if (IsBusy) return;
        DissolveSequence(this.GetCancellationTokenOnDestroy()).Forget();
    }

    public void OnHoldStart() => SetSeeThrough(true);
    public void OnHoldEnd()   => SetSeeThrough(false);

    // Ca chuoi tu chay: tach loi -> dissolve minh -> huy minh -> loi phinh
    private async UniTaskVoid DissolveSequence(CancellationToken token)
    {
        IsBusy = true;

        GameObject coreGo = core;

        // 1) tach loi ra Root, giu nguyen world position/scale
        if (coreGo != null)
            coreGo.transform.SetParent(Root, true);

        // 2) dissolve vo minh
        Init();
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            _mat.SetFloat(_dissolveAmountPropId, Mathf.Clamp01(t / duration));
            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }
        _mat.SetFloat(_dissolveAmountPropId, 1f);

        // 3) loi phinh ve 1 ngay tai cho (loi cung la InteractableObject, tu co nest cua no)
        if (coreGo != null)
            _ = coreGo.transform.DOScale(Vector3.one, growDuration).SetEase(growEase);

        // 4) huy vo
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