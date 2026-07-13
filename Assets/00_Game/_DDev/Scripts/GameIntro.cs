using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

public class GameIntro : MonoBehaviour
{
    public static GameIntro Instance { get; private set; }

    public void InitInstance() => Instance = this;

    [SerializeField] private GameIntroConfig config;
    [SerializeField] private List<MonoBehaviour> introSteps;

    public void Init()
    {
        foreach (var step in introSteps)
            ((IIntroStep)step).Prepare(config);
    }

    public async UniTask PlayIntro()
    {
        foreach (var step in introSteps)
            await ((IIntroStep)step).Play(config);

        BoosterController.Instance.Intro(config.reveal.duration);
        TopBar.Instance.Intro(config.reveal.duration);
        GameScene.Instance.coinLayout.Intro(config.reveal.duration);
    }

    [Button("Test All Intro")]
    private void TestAllIntro()
    {
        Init();
        PlayIntro().Forget();
    }

    [Button("Auto Collect Intro Steps")]
    private void AutoCollectIntroSteps()
    {
        introSteps = new List<MonoBehaviour>();
        foreach (var mono in FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (mono is IIntroStep)
                introSteps.Add(mono);
    }
}
