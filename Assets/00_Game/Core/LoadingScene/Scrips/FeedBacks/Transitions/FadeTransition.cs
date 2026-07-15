using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class FadeTransition : MonoBehaviour, ISceneTransition
{
    [SerializeField] private Canvas wipeCanvas;
    [SerializeField] private Image image;
    [SerializeField] private float durationOut = 0.5f;
    [SerializeField] private float durationIn = 0.5f;

    public TransType Type => TransType.Fade;

    private void Awake()
    {
        SetAlpha(0f);
        if (wipeCanvas.gameObject.activeInHierarchy)
            wipeCanvas.gameObject.SetActive(false);
    }

    public void SetCamera(Camera cam) { }

    public async UniTask CoverAsync()
    {
        wipeCanvas.gameObject.SetActive(true);
        await image.DOFade(1f, durationOut).ToUniTask();
    }

    public void CoverInstant()
    {
        wipeCanvas.gameObject.SetActive(true);
        SetAlpha(1f);
    }

    public async UniTask RevealAsync()
    {
        await image.DOFade(0f, durationIn).ToUniTask();
        wipeCanvas.gameObject.SetActive(false);
    }

    private void SetAlpha(float a)
    {
        Color c = image.color;
        c.a = a;
        image.color = c;
    }
}
