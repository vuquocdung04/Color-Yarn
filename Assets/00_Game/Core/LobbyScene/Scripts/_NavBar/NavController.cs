using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class NavController : MonoBehaviour
{
    public static NavController Instance { get; private set; }

    [SerializeField] private RectTransform selector;
    [SerializeField] private float selectorMoveDuration = 0.25f;

    public List<NavButton> navButtons;
    private NavButton currentNavSelected;

    public void Init()
    {
        Instance = this;

        foreach (var nav in navButtons)
        {
            nav.Init();
            nav.SetupClick(delegate
            {
                if (nav != currentNavSelected)
                {
                    UpdateNavButtonState(nav);
                }
            });
        }
        InitAfterLayoutAsync().Forget();
    }
    private async UniTaskVoid InitAfterLayoutAsync()
    {
        await UniTask.WaitForEndOfFrame(this);
        InitNavButtonStateWith(ENavType.Lobby);
    }
    public void NavigateTo(ENavType type)
    {
        var target = navButtons.Find(n => n.navType == type);
        if (target == null || target == currentNavSelected) return;
        UpdateNavButtonState(target);
    }
    private void InitSize()
    {
        int count = navButtons.Count;
        if (count == 0) return;

        float totalWidth = GetComponent<RectTransform>().rect.width;
        float height = 250f;
        int middle = count / 2;

        for (int i = 0; i < count; i++)
        {
            float percent = i == middle ? 0.34f : 0.33f;
            navButtons[i].SetSize(new Vector2(totalWidth * percent, height));
        }
    }
    private void InitNavButtonStateWith(ENavType type)
    {
        InitSize();
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);

        foreach (var t in navButtons)
            t.HandleSelected(t.navType == type);

        currentNavSelected = navButtons.Find(n => n.navType == type);
        MoveSelectorTo(currentNavSelected, true);
    }
    private void UpdateNavButtonState(NavButton navButton)
    {
        foreach (var t in navButtons)
        {
            t.HandleSelected(false);
        }
        HandleScreenSliding(navButton);
        currentNavSelected = navButton;
        navButton.HandleSelected(true);
        MoveSelectorTo(navButton, false);
    }
    private void MoveSelectorTo(NavButton button, bool instant)
    {
        if (selector == null || button == null) return;

        float targetX = button.IconWorldX;
        if (instant)
        {
            Vector3 pos = selector.position;
            pos.x = targetX;
            selector.position = pos;
        }
        else
        {
            selector.DOMoveX(targetX, selectorMoveDuration).SetEase(Ease.OutCubic);
        }
    }
    private void HandleScreenSliding(NavButton clicked)
    {
        bool clickedIsRight = clicked.transform.localPosition.x > currentNavSelected.transform.localPosition.x;

        var outAnim = clickedIsRight ? BoxAnimationFactory.SlideToLeft : BoxAnimationFactory.SlideToRight;
        var inAnim = clickedIsRight ? BoxAnimationFactory.SlideFromRight : BoxAnimationFactory.SlideFromLeft;

        ClosePrevBox(currentNavSelected.navType, outAnim);
        OpenCurrentBox(clicked.navType, inAnim);
    }
    private void OpenCurrentBox(ENavType type, IShowAnimation anim)
    {
        switch (type)
        {
            case ENavType.Shop:
                ShopBox.Instance.Show(anim);
                break;
            case ENavType.Lobby:
                LobbyBox.Instance.Show(anim);
                break;
            case ENavType.Rank:
                RankBox.Instance.Show(anim);
                break;
        }
    }
    private void ClosePrevBox(ENavType type, IShowAnimation anim)
    {
        switch (type)
        {
            case ENavType.Shop:
                if (ShopBox.Instance != null) ShopBox.Instance.Close(anim);
                break;
            case ENavType.Lobby:
                if (LobbyBox.Instance != null) LobbyBox.Instance.Close(anim);
                break;
            case ENavType.Rank:
                if (RankBox.Instance != null) RankBox.Instance.Close(anim);
                break;
        }
    }

    [ContextMenu("Setup Nav button")]
    private void Setup()
    {
        navButtons.Clear();
        navButtons = GetComponentsInChildren<NavButton>().ToList();

        foreach (var t in navButtons)
        {
            t.InitSetup();
        }
    }
}
