using UnityEngine;

public class KeepPlayingController : MonoBehaviour
{
    public static KeepPlayingController Instance { get; private set; }

    public void InitInstance() => Instance = this;

    [SerializeField] private Sprite sprAddBox;
    [SerializeField] private Sprite sprClean;
    [SerializeField] private int[] prices = { 500, 900, 1500 };

    private int showIndex;
    private bool cleanUsed;
    private bool cleanMode;
    private bool deciding;

    public void Init()
    {
        showIndex = 0;
        cleanUsed = false;
        deciding = false;
    }

    public void OnLoseCondition()
    {
        if (deciding) return;

        if (BoxCreator.Instance.CountLocked() > 0)
        {
            cleanMode = false;
            ShowPopup(sprAddBox, "Add 1 box and continue?");
        }
        else if (!cleanUsed)
        {
            cleanMode = true;
            ShowPopup(sprClean, "Clean HolesTemp and Continue?");
        }
        else
        {
            GameFlow.Instance.TriggerLose();
        }
    }

    private void ShowPopup(Sprite sprite, string des)
    {
        deciding = true;
        int price = prices[Mathf.Min(showIndex, prices.Length - 1)];
        _ = KeepPlayingBox.Setup(GameScene.GetPopupHolder(),
            box => box.SetupAndShow(sprite, des, price, OnBuy, OnGiveUp));
    }

    private void OnBuy()
    {
        deciding = false;
        showIndex++;

        if (cleanMode)
        {
            cleanUsed = true;
            HolesTemp.Instance.CleanToAweSome();
        }
        else
        {
            string color = HolesTemp.Instance.GetDominantColor();
            BoxCreator.Instance.GetFirstLocked()?.Unlock(color);
        }

        KeepPlayingBox.Instance.Close();
        HolesTemp.Instance.RecheckLose();
    }

    private void OnGiveUp()
    {
        KeepPlayingBox.Instance.Close();
        GameFlow.Instance.TriggerLose();
    }
}
