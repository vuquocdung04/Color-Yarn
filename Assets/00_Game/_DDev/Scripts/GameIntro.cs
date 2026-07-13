using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameIntro : MonoBehaviour
{
    public static GameIntro Instance { get; private set; }

    public void InitInstance() => Instance = this;

    [Header("Box Intro")]
    [SerializeField] private float boxOffsetX = 3f;
    [SerializeField] private float boxMoveDuration = 0.4f;
    [SerializeField] private float boxWaveDelay = 0.12f;

    [Header("Hole Intro")]
    [SerializeField] private float holeScaleDuration = 0.3f;
    [SerializeField] private float holeWaveDelay = 0.05f;

    [Header("Yarn Intro")]
    [SerializeField] private float yarnRotateY = 720f;
    [SerializeField] private float yarnSpinDuration = 0.8f;
    [SerializeField] private float yarnTiltX = -10f;
    [SerializeField] private float yarnTiltDuration = 0.3f;
    [SerializeField] private float yarnZoomPercent = 0.3f;

    [Header("Yarn Required Popup")]
    [SerializeField] private GameObject yarnRequiredGO;
    [SerializeField] private CanvasGroup yarnRequiredPanel;
    [SerializeField] private Image yarnRequiredImage;
    [SerializeField] private TextMeshProUGUI txtYarnRequired;
    [SerializeField] private float yarnCountDuration = 0.6f;
    [SerializeField] private float yarnHoldDuration = 0.3f;
    [SerializeField] private float yarnFlyDuration = 0.5f;
    [SerializeField] private float yarnFlyArcRatio = 0.3f;
    [SerializeField] private float yarnInfoMoveDuration = 0.4f;

    private Vector3 yarnImageOriginalPos;
    private Vector2 yarnImageOriginalSize;

    public void Init()
    {
        BoxCreator.Instance.PrepareIntro(boxOffsetX);
        HolesTemp.Instance.PrepareIntro();
        LevelController.Instance.PrepareYarnIntro();

        yarnImageOriginalPos = yarnRequiredImage.rectTransform.position;
        yarnImageOriginalSize = yarnRequiredImage.rectTransform.sizeDelta;
        ResetYarnRequiredPopup();
    }

    private void ResetYarnRequiredPopup()
    {
        yarnRequiredGO.SetActive(false);
        yarnRequiredPanel.alpha = 1f;
        txtYarnRequired.gameObject.SetActive(true);
        yarnRequiredImage.rectTransform.position = yarnImageOriginalPos;
        yarnRequiredImage.rectTransform.sizeDelta = yarnImageOriginalSize;
    }

    public async UniTask PlayIntro()
    {
        await LevelController.Instance.PlayYarnIntro(yarnRotateY, yarnSpinDuration, yarnTiltX, yarnTiltDuration, yarnZoomPercent);

        await PlayYarnRequiredPopup();

        await BoxCreator.Instance.PlayIntro(boxMoveDuration, boxWaveDelay);

        await HolesTemp.Instance.PlayIntro(holeScaleDuration, holeWaveDelay);

        BoosterController.Instance.Intro();
        TopBar.Instance.Intro();
        GameScene.Instance.coinLayout.Intro();
    }

    [Button("Test Box Intro")]
    private void TestBoxIntro()
    {
        BoxCreator.Instance.PrepareIntro(boxOffsetX);
        BoxCreator.Instance.PlayIntro(boxMoveDuration, boxWaveDelay).Forget();
    }

    [Button("Test Hole Intro")]
    private void TestHoleIntro()
    {
        HolesTemp.Instance.PrepareIntro();
        HolesTemp.Instance.PlayIntro(holeScaleDuration, holeWaveDelay).Forget();
    }

    [Button("Test Yarn Intro")]
    private void TestYarnIntro()
    {
        LevelController.Instance.PrepareYarnIntro();
        LevelController.Instance.PlayYarnIntro(yarnRotateY, yarnSpinDuration, yarnTiltX, yarnTiltDuration, yarnZoomPercent).Forget();
    }

    [Button("Test All Intro")]
    private void TestAllIntro()
    {
        Init();
        PlayIntro().Forget();
    }

    private async UniTask PlayYarnRequiredPopup()
    {
        yarnRequiredGO.SetActive(true);
        yarnRequiredPanel.alpha = 1f;

        int required = LevelController.Instance.CurrentYarnObj.TotalLen;
        await txtYarnRequired.CountTo(required, yarnCountDuration, from: 0);
        await UniTask.Delay((int)(yarnHoldDuration * 1000));

        txtYarnRequired.gameObject.SetActive(false);
        yarnRequiredPanel.alpha = 0f;

        RectTransform iconRect = (RectTransform)GameScene.Instance.yarnRequiredUI.Icon;
        RectTransform imgRect = yarnRequiredImage.rectTransform;

        Vector3 start = imgRect.position;
        Vector3 end = iconRect.position;
        Vector3 dir = end - start;
        Vector3 perpendicular = new Vector3(-dir.y, dir.x, 0f).normalized;
        Vector3 arcPoint = (start + end) / 2f + perpendicular * (dir.magnitude * yarnFlyArcRatio);

        imgRect.DOSizeDelta(iconRect.sizeDelta, yarnFlyDuration);
        await imgRect.DOPath(new[] { arcPoint, end }, yarnFlyDuration, PathType.CatmullRom)
            .SetEase(Ease.InOutSine)
            .AsyncWaitForCompletion();

        GameScene.Instance.yarnRequiredUI.gameObject.SetActive(true);

        await GameScene.Instance.yarnRequiredUI.PlayIntro(yarnInfoMoveDuration);

        yarnRequiredGO.SetActive(false);
    }

    [Button("Test Yarn Required Popup")]
    private void TestYarnRequiredPopup()
    {
        ResetYarnRequiredPopup();
        PlayYarnRequiredPopup().Forget();
    }
}
