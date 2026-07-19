using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class LevelHardIntroUI : MonoBehaviour, IIntroStep
{
    [SerializeField] private Image bg;
    [SerializeField] private float bgFadeDuration = 0.2f;

    [SerializeField] private Image image;
    [SerializeField] private Color fromColor = Color.red;
    [SerializeField] private Color toColor = Color.white;
    [SerializeField] private float colorDuration = 0.4f;
    [SerializeField] private int loops = 4;

    public void Prepare(GameIntroConfig config)
    {
        gameObject.SetActive(false);
    }

    public async UniTask Play(GameIntroConfig config)
    {
        if (LevelController.Instance == null || !LevelController.Instance.IsHard) return;

        gameObject.SetActive(true);
        image.color = fromColor;

        if (AudioManager.Instance)
            AudioManager.Instance.PlaySfx("hard_level");

        SetBgAlpha(0f);
        await bg.DOFade(0.95f, bgFadeDuration).AsyncWaitForCompletion();

        await image.DOColor(toColor, colorDuration)
            .SetLoops(loops, LoopType.Yoyo)
            .AsyncWaitForCompletion();

        gameObject.SetActive(false);
    }

    private void SetBgAlpha(float a)
    {
        Color c = bg.color;
        c.a = a;
        bg.color = c;
    }
}
