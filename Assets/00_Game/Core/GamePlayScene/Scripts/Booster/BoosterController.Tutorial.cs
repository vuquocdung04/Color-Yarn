using UnityEngine;

public partial class BoosterController
{
    public readonly int[] TutorialLevels = { 3, 6, 9 };

    public int GetCurrentTutorialBoosterIndex()
        => System.Array.IndexOf(TutorialLevels, currentLevel);

    private bool IsTutorialDone(BoosterType type) => GetConfig(type).tutorialDone;
    private void SetTutorialDone(BoosterType type, bool value) => GetConfig(type).tutorialDone = value;

    public void CheckTutorialHighlight()
    {
        BoosterItem target = null;

        void TryAssign(int index)
        {
            if (target != null) return;
            if (TutorialLevels.Length > index
                && currentLevel == TutorialLevels[index]
                && !IsTutorialDone((BoosterType)index)
                && index < items.Count)
            {
                target = items[index];
            }
        }

        TryAssign(0);
        TryAssign(1);
        TryAssign(2);

        if (target != null)
        {
            _ = BoosterUnlockBox.Setup(GameScene.GetPopupHolder(), box => box.Show());
        }
    }

    private void CheckAndClearTutorialPhase1(BoosterType type, BoosterItem item)
    {
        if (IsTutorialDone(type)) return;

        HandAnimation.Instance.RemoveHighlightUI(item.gameObject);
        HandAnimation.Instance.KillUI();

        if (type == BoosterType.Booster0) SetTutorialDone(type, true);
    }

    private void SetupPhase2Tutorial(BoosterType type)
    {
        if (type == BoosterType.Booster1
            && currentLevel == 6
            && !IsTutorialDone(BoosterType.Booster1))
        {
            // Transform targetObj = GamePlayController.Instance.gameScene.GetTutorialTarget();
            // if (targetObj != null) HandAnimation.Instance.PlayAnimObj(targetObj);
        }
    }

    private void CompletePhase2Tutorial(BoosterType type)
    {
        if (type == BoosterType.Booster1 && !IsTutorialDone(BoosterType.Booster1))
        {
            SetTutorialDone(BoosterType.Booster1, true);
            HandAnimation.Instance.KillObj();
        }
    }

    private void HandleTutorialCancel(BoosterType type)
    {
        int level = currentLevel;

        if (type == BoosterType.Booster1 && level == 6 && !IsTutorialDone(BoosterType.Booster1))
        {
            HandAnimation.Instance.KillObj();
            SetTutorialDone(BoosterType.Booster1, true);
        }

        if (type == BoosterType.Booster2 && level == 9 && !IsTutorialDone(BoosterType.Booster2))
        {
            HandAnimation.Instance.KillObj();
            SetTutorialDone(BoosterType.Booster2, true);
        }
    }
}
