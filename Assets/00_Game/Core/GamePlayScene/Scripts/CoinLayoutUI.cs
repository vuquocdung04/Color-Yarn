using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CoinLayoutUI : MonoBehaviour, IIntroStep
{
    [SerializeField] private Button btnCoin;
    [SerializeField] private CanvasGroup canvasGroup;

    public void Init()
    {
        btnCoin.OnClicked(delegate
        {
            _ = ShopBox.Setup(GameScene.GetPopupHolder(), box => box.Show());
        });
    }

    public void Prepare(GameIntroConfig config) => canvasGroup.SetCanvasState(false, 0f);

    public UniTask Play(GameIntroConfig config)
    {
        canvasGroup.SetCanvasState(true);
        canvasGroup.DOFade(1f, config.reveal.duration);
        return UniTask.CompletedTask;
    }

    public Transform GetCoinBar() => btnCoin.transform;
}
