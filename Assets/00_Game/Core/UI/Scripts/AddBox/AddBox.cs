using UnityEngine.UI;

public class AddBox : BaseBox<AddBox>
{
    public Button btnClose;
    public Button btnBuyWithCoin;

    private BoxSlot _target;

    public void SetupAndShow(BoxSlot box)
    {
        _target = box;
        Show();
    }

    protected override void Init()
    {
        btnClose.OnClicked(Close);
        btnBuyWithCoin.OnClicked(() => OnBuy(0));
    }

    protected override void InitState()
    {
    }

    private void OnBuy(int cost)
    {
        _target.Unlock();
        Close();
    }
}
