using UnityEngine.UI;

public class SettingGameBox : BaseBox<SettingGameBox>
{
    public Button btnClose;
    public Button btnReturnHome;
    public Button btnRestart;
    public Button btnCheat;

    public SettingsToggleGroup settings;

    protected override void Init()
    {
        btnClose.OnClicked(Close);

        btnReturnHome.OnClicked(delegate
        {
            _ = QuitLevelBox.Setup(transform.parent, box => box.SetupAndShow(QuitLevelBox.Mode.Leave));
        });

        btnRestart.OnClicked(delegate
        {
            _ = QuitLevelBox.Setup(transform.parent, box => box.SetupAndShow(QuitLevelBox.Mode.Restart));
        });

        btnCheat.OnClicked(delegate
        {
            _ = CheatBox.Setup(transform.parent, box => box.Show());
        });

        settings.Init();
    }

    protected override void InitState()
    {
    }
}
