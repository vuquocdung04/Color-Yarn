
using Cysharp.Threading.Tasks;
using EventDispatcher;
using UnityEngine;

public class GamePlayController : Singleton<GamePlayController>
{
    public Camera cameraUI;
    public Camera cameraGameplay;
    public GameScene gameScene;
    public HandAnimation handAnimation;
    public GameFlow gameFlow;
    public InputController inputController;
    public HolesTemp holesTemp;
    public ColorRepo colorRepo;
    public BoxCreator boxCreator;
    public GameAlgorithm gameAlgorithm;
    public AweSomeBox aweSomeBox;
    public LevelController levelController;
    public GameIntro gameIntro;
    public KeepPlayingController keepPlayingController;

    protected override void OnAwake()
    {
        base.OnAwake();
        Init().Forget();
    }

    private async UniTaskVoid Init()
    {
        InitInstance();
        InitManagers();
        gameFlow.RequestPause();

        AudioManager.Instance?.PlayMusic("Normal Level Music (Cover) 1");

        await UniTask.WaitForEndOfFrame(this);
        boxCreator.SetInitialColors();
        await UniTask.Delay(500);
        if (FXManager.Instance)
            FXManager.Instance.isNextSceneReady = true;
        await UniTask.Delay(500);
        await gameIntro.PlayIntro();
        gameFlow.RequestResume();
    }

    private void InitInstance()
    {
        gameScene.InitInstance();
        handAnimation.InitInstance();
        inputController.InitInstance();
        gameFlow.InitInstance();
        holesTemp.InitInstance();
        colorRepo.InitInstance();
        boxCreator.InitInstance();
        gameAlgorithm.InitInstance();
        aweSomeBox.InitInstance();
        levelController.InitInstance();
        gameIntro.InitInstance();
        keepPlayingController.InitInstance();
    }

    private void InitManagers()
    {
        levelController.Init();
        gameScene.Init();
        handAnimation.Init();
        inputController.Init();
        gameFlow.Init();
        holesTemp.Init();
        colorRepo.Init();
        boxCreator.Init();
        gameAlgorithm.Init();
        aweSomeBox.Init();
        keepPlayingController.Init();

        gameIntro.Init();
    }
}
