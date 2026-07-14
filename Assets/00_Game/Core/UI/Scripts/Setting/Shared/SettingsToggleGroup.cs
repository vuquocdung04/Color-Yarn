using Sirenix.OdinInspector;
using UnityEngine;

public class SettingsToggleGroup : MonoBehaviour
{
    [SerializeField] private BaseSettingToggleView soundView;
    [SerializeField] private BaseSettingToggleView musicView;
    [SerializeField] private BaseSettingToggleView vibView;

    public void Init()
    {
        soundView.Bind(new SoundSetting());
        musicView.Bind(new MusicSetting());
        vibView.Bind(new VibSetting());
    }

    [Button]
    private void AutoWire()
    {
        var views = GetComponentsInChildren<BaseSettingToggleView>(true);
        if (views.Length >= 3)
        {
            soundView = views[0];
            musicView = views[1];
            vibView = views[2];
        }

        foreach (var v in views) v.AutoAssignRefs();
    }
}
