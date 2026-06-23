using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

public class CubeDissolve : MonoBehaviour
{
    public float duration = 0.5f;
    public bool deactivateInsteadOfDestroy = true;

    private Renderer _renderer;
    private Material _mat;
    private bool _running;

    public bool IsRunning => _running;

    void Awake()
    {
        Init();
    }

    private void Init()
    {
        if (_mat != null) return;
        _renderer = GetComponent<Renderer>();
        _mat = _renderer.material;
        _mat.SetFloat("_DissolveAmount", 0f);
    }

    [Button("Test Dissolve")]
    public void Play()
    {
        if (!Application.isPlaying) return;
        Init();
        if (_running) return;
        _running = true;
        StartCoroutine(Run());
    }

    [Button("Reset")]
    public void ResetDissolve()
    {
        Init();
        StopAllCoroutines();
        _running = false;
        _mat.SetFloat("_DissolveAmount", 0f);
        gameObject.SetActive(true);
    }

    private IEnumerator Run()
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            _mat.SetFloat("_DissolveAmount", Mathf.Clamp01(t / duration));
            yield return null;
        }
        _mat.SetFloat("_DissolveAmount", 1f);

        if (deactivateInsteadOfDestroy)
            gameObject.SetActive(false);
        else
            Destroy(gameObject);
    }
}
