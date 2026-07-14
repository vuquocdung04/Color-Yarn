public interface IToggleSetting
{
    bool Value { get; }
    void Toggle();
}

public class SoundSetting : IToggleSetting
{
    public bool Value => UseProfile.OnSound;
    public void Toggle() => AudioManager.Instance.ToggleSound();
}

public class MusicSetting : IToggleSetting
{
    public bool Value => UseProfile.OnMusic;
    public void Toggle() => AudioManager.Instance.ToggleMusic();
}

public class VibSetting : IToggleSetting
{
    public bool Value => UseProfile.OnVib;
    public void Toggle() => UseProfile.OnVib.Value = !UseProfile.OnVib;
}
