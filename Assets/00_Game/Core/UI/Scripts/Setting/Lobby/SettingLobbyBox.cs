using UnityEngine.UI;

public class SettingLobbyBox : BaseBox<SettingLobbyBox>
{
    public Button btnClose;
    public Button btnCloseByPanel;

    public SettingsToggleGroup settings;

    protected override void Init()
    {
        btnClose.OnClicked(Close);
        btnCloseByPanel.OnClicked(Close);
        settings.Init();
    }

    protected override void InitState()
    {
    }
}
