using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TopBar : MonoBehaviour, IIntroStep
{
    public static TopBar Instance { get; private set; }

    public void InitInstance() => Instance = this;

    [SerializeField] private bool isTest;

    public TextMeshProUGUI txtLevelDisplay;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Level Nodes")]
    [SerializeField] private List<LevelNodeUI> levelNodes;

    [Header("Button")]
    public Button btnSetting;

    public void Init()
    {
        btnSetting.OnClicked(delegate
        {
            _ = SettingGameBox.Setup(GameScene.GetPopupHolder(), box => box.Show());
        });

        txtLevelDisplay.text = $"Level {UseProfile.Level.Value}";
        SetupLevelNodes();
    }

    public void Prepare(GameIntroConfig config) => canvasGroup.SetCanvasState(false, 0f);

    [Button("Auto Wire Level Nodes")]
    private void AutoWireLevelNodes()
    {
        levelNodes = new List<LevelNodeUI>(GetComponentsInChildren<LevelNodeUI>());
        foreach (var node in levelNodes)
            node.AutoAssignRefs();
    }

    private void SetupLevelNodes()
    {
        int currentLevel = isTest ? LevelController.Instance.CurrentLevel : UseProfile.Level.Value;
        int zoneStart = (currentLevel - 1) / levelNodes.Count * levelNodes.Count + 1;

        for (int i = 0; i < levelNodes.Count; i++)
        {
            int levelNum = zoneStart + i;
            DataLevel data = LevelController.Instance.GetLevelData(levelNum);

            levelNodes[i].SetIcon(data.levelSprite);
            levelNodes[i].SetHard(data.isHard);
            levelNodes[i].SetCurrent(levelNum == currentLevel);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public UniTask Play(GameIntroConfig config)
    {
        canvasGroup.SetCanvasState(true);
        canvasGroup.DOFade(1f, config.reveal.duration);
        return UniTask.CompletedTask;
    }
}
