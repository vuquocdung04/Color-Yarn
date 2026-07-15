using Cysharp.Threading.Tasks;
using DG.Tweening;
using EventDispatcher;
using TMPro;
using UnityEngine;

public class YarnRequiredUI : MonoBehaviour
{
    [SerializeField] private Transform icon;
    [SerializeField] private Transform info;
    [SerializeField] private TextMeshProUGUI txtYarnProgress;

    private Vector3 originalPosition;
    private int currentYarn;
    private int requiredYarn;

    public Transform Icon => icon;

    public void Init()
    {
        originalPosition = info.localPosition;

        this.RegisterListener(EventID.YARN_COLLECTED, OnYarnCollected);
    }

    public void PrepareIntro()
    {
        info.localPosition = originalPosition + new Vector3(-150f, 0f, 0f);
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        this.RemoveListener(EventID.YARN_COLLECTED, OnYarnCollected);
    }

    public void SetRequiredYarn(int total)
    {
        requiredYarn = total;
        currentYarn = 0;
        UpdateYarnProgressText();
    }

    private void OnYarnCollected(object param)
    {
        currentYarn += 3;
        UpdateYarnProgressText();

        if (currentYarn >= requiredYarn)
            this.PostEvent(EventID.LEVEL_COMPLETE);
    }

    private void UpdateYarnProgressText()
    {
        if (txtYarnProgress != null) txtYarnProgress.text = $"{currentYarn}/{requiredYarn}";
    }

    public async UniTask PlayIntro(float duration)
    {
        await info.DOLocalMove(originalPosition, duration).AsyncWaitForCompletion();
    }
}
