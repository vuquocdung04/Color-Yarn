using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public partial class FXManager
{
    [SerializeField] private TransType transitionType = TransType.Iris;
    private ISceneTransition[] transitions;
    [HideInInspector] public bool isNextSceneReady;

    private void InitTransitions()
    {
        transitions = GetComponentsInChildren<ISceneTransition>(true);
    }

    public void LoadScene(string sceneName, bool skipOutPhase = false)
    {
        RunAsync(sceneName, Pick(), skipOutPhase).Forget();
    }

    private ISceneTransition Pick()
    {
        foreach (var t in transitions)
            if (t.Type == transitionType) return t;
        return null;
    }

    private async UniTaskVoid RunAsync(string sceneName, ISceneTransition t, bool skipOutPhase)
    {
        if (t == null) return;

        isNextSceneReady = false;
        t.SetCamera(GetSceneCamera());

        if (skipOutPhase) t.CoverInstant();
        else await t.CoverAsync();

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone)
            await UniTask.Yield();

        t.SetCamera(GetSceneCamera());

        await UniTask.WaitUntil(() => isNextSceneReady);
        await t.RevealAsync();
    }

    private Camera GetSceneCamera()
    {
        string scene = SceneManager.GetActiveScene().name;

        if (scene == SceneName.LOBBY_SCENE)
            return LobbyController.Instance != null ? LobbyController.Instance.mainCamera : null;

        if (scene == SceneName.GAME_PLAY)
            return GamePlayController.Instance != null ? GamePlayController.Instance.cameraUI : null;

        return null;
    }
}
