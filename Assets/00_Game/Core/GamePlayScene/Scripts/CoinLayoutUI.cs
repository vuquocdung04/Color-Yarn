using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CoinLayoutUI : MonoBehaviour
{
    [SerializeField] private Button btnCoin;
    [SerializeField] private CanvasGroup canvasGroup;

    public void Init()
    {
        btnCoin.OnClicked(delegate
        {
            _ = ShopBox.Setup(GameScene.GetPopupHolder(), box => box.Show());
        });

        canvasGroup.SetCanvasState(false, 0f);
    }

    public void Intro(float duration = 0.3f)
    {
        canvasGroup.SetCanvasState(true);
        canvasGroup.DOFade(1f, duration);
    }

    public Transform GetCoinBar() => btnCoin.transform;
}
