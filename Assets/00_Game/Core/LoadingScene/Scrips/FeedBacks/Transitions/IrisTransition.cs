using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class IrisTransition : MonoBehaviour, ISceneTransition
{
    [SerializeField] private Canvas wipeCanvas;
    [SerializeField] private float durationOut = 1f;
    [SerializeField] private float durationIn = 1f;

    private Material cachedMat;

    private Material Mat
    {
        get
        {
            if (cachedMat == null)
                cachedMat = wipeCanvas.GetComponentInChildren<RawImage>().material;
            return cachedMat;
        }
    }

    public TransType Type => TransType.Iris;

    private void Awake()
    {
        wipeCanvas.enabled = false;
    }

    public void SetCamera(Camera cam)
    {
        wipeCanvas.enabled = true;
        if (cam != null)
            wipeCanvas.worldCamera = cam;
    }

    public async UniTask CoverAsync()
    {
        SetState(0f, 0f);
        await Mat.DOFloat(1.2f, "_Radius", durationOut).ToUniTask();
    }

    public void CoverInstant() => SetState(1f, 0f);

    public async UniTask RevealAsync()
    {
        SetState(1f, 0f);
        await Mat.DOFloat(1.2f, "_Radius", durationIn).ToUniTask();
        wipeCanvas.enabled = false;
    }

    private void SetState(float isInvert, float radius)
    {
        Mat.SetFloat("_IsInvert", isInvert);
        Mat.SetFloat("_Radius", radius);
    }
}
