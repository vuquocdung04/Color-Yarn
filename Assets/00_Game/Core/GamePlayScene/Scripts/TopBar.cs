using EventDispatcher;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TopBar : MonoBehaviour
{
    public static TopBar Instance { get; private set; }

    public void InitInstance() => Instance = this;

    public TextMeshProUGUI txtLevelDisplay;
    public TextMeshProUGUI txtYarnProgress;

    [Header("Button")]
    public Button btnSetting;
    public Button btnCoin;

    private int currentYarn;
    private int requiredYarn;

    public void Init()
    {
        btnSetting.OnClicked(delegate
        {
            _ = SettingGameBox.Setup(GameScene.GetPopupHolder(), box => box.Show());
        });

        btnCoin.OnClicked(delegate
        {
            _ = ShopBox.Setup(GameScene.GetPopupHolder(), box => box.Show());
        });

        txtLevelDisplay.text = $"Level {UseProfile.Level.Value}";

        this.RegisterListener(EventID.YARN_COLLECTED, OnYarnCollected);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
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

    public Transform GetCoinBar() => btnCoin.transform;
}
