using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public partial class BoosterController
{
    [System.Serializable]
    public class BoosterConfig
    {
        [TableColumnWidth(90, resizable: false)]
        public BoosterType type;
        public int levelUnlock;
        public int quantity;
        public bool tutorialDone;
    }

    [Title("Items")]
    [SerializeField] private List<BoosterItem> items;

    [TableList(AlwaysExpanded = true, DrawScrollView = false)]
    [SerializeField] private List<BoosterConfig> configs;

    [Title("State")]
    [SerializeField] private int currentLevel;

    private BoosterConfig GetConfig(BoosterType type) => configs.Find(c => c.type == type);

    private void ApplyConfigToItems()
    {
        foreach (var item in items)
        {
            var cfg = GetConfig(item.Type);
            item.SetUnlockLevel(cfg.levelUnlock);

            bool unlocked = currentLevel >= cfg.levelUnlock;

            if (unlocked)
            {
                item.ChangeState(BoosterState.Available, force: true);
                item.SetData(cfg.quantity);
            }
            else
            {
                item.ChangeState(BoosterState.Locked, force: true);
            }
        }
    }

    private void OverrideConfig()
    {
        currentLevel = 1;

        var b0 = GetConfig(BoosterType.Booster0);
        b0.levelUnlock = 3;
        b0.quantity = 3;
        b0.tutorialDone = false;

        var b1 = GetConfig(BoosterType.Booster1);
        b1.levelUnlock = 6;
        b1.quantity = 3;
        b1.tutorialDone = false;

        var b2 = GetConfig(BoosterType.Booster2);
        b2.levelUnlock = 9;
        b2.quantity = 3;
        b2.tutorialDone = false;
    }

    private int GetQuantity(BoosterType type) => GetConfig(type).quantity;

    private void Consume(BoosterType type)
    {
        var cfg = GetConfig(type);
        cfg.quantity = Mathf.Max(0, cfg.quantity - 1);
    }

    public void AddQuantity(BoosterType type, int amount)
    {
        var cfg = GetConfig(type);
        cfg.quantity += amount;
        FindItem(type)?.SetData(cfg.quantity);
    }

    public BoosterItem FindItem(BoosterType type) => items.Find(i => i.Type == type);

    public BoosterItem GetItemByIndex(int index) =>
        (items != null && index >= 0 && index < items.Count) ? items[index] : null;
}
