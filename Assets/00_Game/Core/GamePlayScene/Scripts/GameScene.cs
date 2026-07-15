using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class GameScene : MonoBehaviour
{
    public static GameScene Instance { get; private set; }

    public void InitInstance()
    {
        Instance = this;
        boosterController.InitInstance();
        topBar.InitInstance();
    }

    public Transform popupHolder;
    public Image darkPanel;

    [Header("Booster")]
    public BoosterController boosterController;

    [Header("Top Bar")]
    public TopBar topBar;

    [Header("Yarn Required")]
    public YarnRequiredUI yarnRequiredUI;

    [Header("Coin")]
    public CoinLayoutUI coinLayout;

    public void Init()
    {
        boosterController.Init();
        topBar.Init();

        yarnRequiredUI.Init();
        yarnRequiredUI.SetRequiredYarn(LevelController.Instance.CurrentYarnObj.TotalLen);

        coinLayout.Init();
    }

    public static void EnableDarkPanel(bool state)
    {
        Instance.darkPanel.DOKill();
        if (state)
        {
            Instance.darkPanel.gameObject.SetActive(true);

            Color color = Instance.darkPanel.color;
            color.a = 0f;
            Instance.darkPanel.color = color;

            float targetAlpha = 0.9f;
            float fadeDuration = 0.15f;

            Instance.darkPanel.DOFade(targetAlpha, fadeDuration).SetUpdate(true);
        }
        else
        {
            Instance.darkPanel.gameObject.SetActive(false);
        }
    }

    public static Transform GetCoinBar() => Instance.coinLayout.GetCoinBar();
    public static Transform GetPopupHolder() => Instance.popupHolder;
}
