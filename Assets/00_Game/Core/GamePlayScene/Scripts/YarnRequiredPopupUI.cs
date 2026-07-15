using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class YarnRequiredPopupUI : MonoBehaviour, IIntroStep
{
    [SerializeField] private CanvasGroup panel;
    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI txtYarnRequired;

    private Vector3 imageOriginalPos;
    private Vector2 imageOriginalSize;

    public void Prepare(GameIntroConfig config)
    {
        imageOriginalPos = image.rectTransform.position;
        imageOriginalSize = image.rectTransform.sizeDelta;

        panel.alpha = 1f;
        txtYarnRequired.gameObject.SetActive(true);
        image.rectTransform.position = imageOriginalPos;
        image.rectTransform.sizeDelta = imageOriginalSize;

        GameScene.Instance.yarnRequiredUI.PrepareIntro();
        gameObject.SetActive(false);
    }

    public async UniTask Play(GameIntroConfig config)
    {
        var settings = config.yarnRequiredPopup;

        gameObject.SetActive(true);
        panel.alpha = 1f;

        int required = LevelController.Instance.CurrentYarnObj.TotalLen;
        await txtYarnRequired.CountTo(required, settings.countDuration, from: 0);
        await UniTask.Delay((int)(settings.holdDuration * 1000));

        txtYarnRequired.gameObject.SetActive(false);
        panel.alpha = 0f;

        RectTransform iconRect = (RectTransform)GameScene.Instance.yarnRequiredUI.Icon;
        RectTransform imgRect = image.rectTransform;

        Vector3 start = imgRect.position;
        Vector3 end = iconRect.position;
        Vector3 dir = end - start;
        Vector3 perpendicular = new Vector3(-dir.y, dir.x, 0f).normalized;
        Vector3 arcPoint = (start + end) / 2f + perpendicular * (dir.magnitude * settings.flyArcRatio);

        _ = imgRect.DOSizeDelta(iconRect.sizeDelta, settings.flyDuration);
        await imgRect.DOPath(new[] { arcPoint, end }, settings.flyDuration, PathType.CatmullRom)
            .SetEase(Ease.InOutSine)
            .AsyncWaitForCompletion();

        GameScene.Instance.yarnRequiredUI.gameObject.SetActive(true);

        await GameScene.Instance.yarnRequiredUI.PlayIntro(settings.infoMoveDuration);

        gameObject.SetActive(false);
    }
}
