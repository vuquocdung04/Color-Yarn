using UnityEngine;

public partial class LevelController : MonoBehaviour, IIntroStep
{
    public static LevelController Instance { get; private set; }

    public void InitInstance() => Instance = this;

    [Header("Level")]
    [SerializeField] private LevelDatabase levelDatabase;
    [SerializeField] private int currentLevel = 1;

    public int CurrentLevel => currentLevel;
    public YarnObj CurrentYarnObj { get; private set; }
    public GameObject CurrentLevelGO { get; private set; }
    public float MinZoomScale { get; private set; }
    public float MaxZoomScale { get; private set; }
    public bool IsHard { get; private set; }

    public void Init()
    {
        SpawnCurrentLevel();
        InitCameraControls();
    }

    public GameObject SpawnCurrentLevel()
    {
        DataLevel data = levelDatabase.Levels[currentLevel - 1];
        GameObject go = Instantiate(data.levelPrefab);

        CurrentYarnObj = go.GetComponent<YarnObj>();
        CurrentYarnObj.Init(data.requiredLen, data.yarnDepthSpread);

        GameAlgorithm.Instance.SetDifficulty(data.boxDifficulty);

        CurrentLevelGO = go;
        MinZoomScale = data.minZoomScale;
        MaxZoomScale = data.maxZoomScale;
        IsHard = data.isHard;

        return go;
    }

    public DataLevel GetLevelData(int levelNum) => levelDatabase.Levels[levelNum - 1];
}
