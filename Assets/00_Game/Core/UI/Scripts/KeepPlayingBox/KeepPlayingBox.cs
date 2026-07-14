using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class KeepPlayingBox : BaseBox<KeepPlayingBox>
{
    public Button btnClose;
    public Button btnBuyWithCoin;
    public TextMeshProUGUI txtCoinDisplay;
    public TextMeshProUGUI txtDes;
    public Image image;

    private Action _onBuy;
    private Action _onGiveUp;

    public void SetupAndShow(Sprite sprite, string des, int price, Action onBuy, Action onGiveUp)
    {
        image.sprite = sprite;
        txtDes.text = des;
        txtCoinDisplay.text = price.ToString();
        _onBuy = onBuy;
        _onGiveUp = onGiveUp;
        Show();
    }

    protected override void Init()
    {
        btnClose.OnClicked(() => _onGiveUp?.Invoke());
        btnBuyWithCoin.OnClicked(() => _onBuy?.Invoke());
    }

    protected override void InitState()
    {
    }
}
