using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TopBar : MonoBehaviour
{
    public static TopBar Instance { get; private set; }

    public void InitInstance() => Instance = this;

    public TextMeshProUGUI txtLevelDisplay;

    [Header("Button")]
    public Button btnSetting;
    public Button btnCoin;

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
    }

    public Transform GetCoinBar() => btnCoin.transform;
}
