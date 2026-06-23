using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class InteractableObject : MonoBehaviour, IInteractable
{
    public float duration = 0.5f;
    public bool deactivateInsteadOfDestroy = true;

    private Renderer _renderer;
    private Material _mat;
    private bool _running;
    private int _dissolveAmountPropId;

    public bool IsRunning => _running;

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        if (_mat != null) return;
        
        _renderer = GetComponent<Renderer>();
        _mat = _renderer.material;
        _dissolveAmountPropId = Shader.PropertyToID("_DissolveAmount");

        if (_mat.HasProperty(_dissolveAmountPropId))
        {
            _mat.SetFloat(_dissolveAmountPropId, 0f);
        }
    }

    public void OnTap()
    {
        if (_running) return;
        DissolveAsync(this.GetCancellationTokenOnDestroy()).Forget();
    }

    public void OnHoldStart()
    {
    }

    public void OnHoldEnd()
    {
    }

    public void ResetDissolve()
    {
        Init();
        _running = false;
        
        if (_mat.HasProperty(_dissolveAmountPropId))
        {
            _mat.SetFloat(_dissolveAmountPropId, 0f);
        }
        gameObject.SetActive(true);
    }

    private async UniTaskVoid DissolveAsync(CancellationToken token)
    {
        _running = true;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            _mat.SetFloat(_dissolveAmountPropId, Mathf.Clamp01(t / duration));
            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }

        _mat.SetFloat(_dissolveAmountPropId, 1f);
        _running = false;

        if (deactivateInsteadOfDestroy)
            gameObject.SetActive(false);
        else
            Destroy(gameObject);
    }
}