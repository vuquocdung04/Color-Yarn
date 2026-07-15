using Cysharp.Threading.Tasks;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public DataRepo dataRepo;
    public FXManager fxManager;
    public AudioManager audioManager;
    public LocalizationManager localizationManager;
    public HeartManager heartManager;
    public CurrencyManager currencyManager;
    public LoadingBox loadingBox;
    public ToastManager toastManager;

    public bool isSkipOutPhase;
    public float loadingStepDuration = 1f;
    public float loadingFadeOutDuration = 1f;

    protected override void OnAwake()
    {
        Init().Forget();
    }
    private async UniTaskVoid Init()
    {
        Application.targetFrameRate = 60;
        loadingBox.Init();
        var load50Task = loadingBox.LoadingAsync(0.5f, loadingStepDuration);
        await GamePrefs.Init();

        InitInstance();

        Test();
        //firebaseSetup.Init();
        //await UniTask.WaitUntil(() => firebaseSetup.IsActiveRemote);
        InitManagers();
        await load50Task;
        await loadingBox.LoadingAsync(1f, loadingStepDuration);
        fxManager.PrepareWipeClosed();
        await loadingBox.CloseAsync(loadingFadeOutDuration);

        //Init final
        fxManager.LoadScene(SceneName.GAME_PLAY, isSkipOutPhase);
    }

    private void InitInstance()
    {
        dataRepo.InitInstance();
        fxManager.InitInstance();
        audioManager.InitInstance();
        heartManager.InitInstance();
        currencyManager.InitInstance();
        toastManager.InitInstance();
    }

    private void InitManagers()
    {
        dataRepo.Init();
        fxManager.Init();
        audioManager.Init();
        heartManager.Init();
        currencyManager.Init();
        toastManager.Init();
    }

    private void Test()
    {
        UseProfile.Heart.Value = 4;
        UseProfile.TimeLastOverHeart = TimeManager.GetCurrentTime();
    }
}