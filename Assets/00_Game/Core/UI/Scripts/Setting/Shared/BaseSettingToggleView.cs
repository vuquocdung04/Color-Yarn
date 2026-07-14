using UnityEngine;

public abstract class BaseSettingToggleView : MonoBehaviour
{
    protected IToggleSetting setting;

    public void Bind(IToggleSetting toggleSetting)
    {
        setting = toggleSetting;
        HookInput();
        Refresh();
    }

    protected abstract void HookInput();
    protected abstract void Refresh();

    public virtual void AutoAssignRefs() { }
}
