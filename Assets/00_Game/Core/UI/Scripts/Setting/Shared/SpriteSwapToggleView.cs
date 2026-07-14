using UnityEngine;
using UnityEngine.UI;

public class SpriteSwapToggleView : BaseSettingToggleView
{
    [SerializeField] private Button button;
    [SerializeField] private Image icon;
    [SerializeField] private Sprite sprOn;
    [SerializeField] private Sprite sprOff;

    protected override void HookInput()
    {
        button.OnClicked(() =>
        {
            setting.Toggle();
            Refresh();
        });
    }

    protected override void Refresh()
    {
        icon.SetSprite(setting.Value ? sprOn : sprOff);
    }

    public override void AutoAssignRefs()
    {
        button = GetComponentInChildren<Button>();
    }
}
