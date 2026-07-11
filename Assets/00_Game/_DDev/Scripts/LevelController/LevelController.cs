using UnityEngine;

public partial class LevelController : MonoBehaviour
{
    public static LevelController Instance { get; private set; }

    public void InitInstance() => Instance = this;

    [Header("Level")]
    [SerializeField] private LevelDatabase levelDatabase;
    [SerializeField] private int currentLevel = 1;

    public YarnObj CurrentYarnObj { get; private set; }
    public GameObject CurrentLevelGO { get; private set; }
    public float MinZoomCamera { get; private set; }
    public float MaxZoomCamera { get; private set; }

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
        MinZoomCamera = data.minZoomCamera;
        MaxZoomCamera = data.maxZoomCamera;

        return go;
    }
}
