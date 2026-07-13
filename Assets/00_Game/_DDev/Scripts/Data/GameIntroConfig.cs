using UnityEngine;

[CreateAssetMenu(fileName = "GameIntroConfig", menuName = "Data/Game Intro Config")]
public class GameIntroConfig : ScriptableObject
{
    [System.Serializable]
    public class BoxSettings
    {
        public float offsetX = 3f;
        public float moveDuration = 0.4f;
        public float waveDelay = 0.12f;
    }

    [System.Serializable]
    public class HoleSettings
    {
        public float scaleDuration = 0.3f;
        public float waveDelay = 0.05f;
    }

    [System.Serializable]
    public class YarnSettings
    {
        public float rotateY = 720f;
        public float spinDuration = 0.8f;
        public float tiltX = -10f;
        public float tiltDuration = 0.3f;
        public float zoomPercent = 0.3f;
    }

    [System.Serializable]
    public class YarnRequiredPopupSettings
    {
        public float countDuration = 0.6f;
        public float holdDuration = 0.3f;
        public float flyDuration = 0.5f;
        public float flyArcRatio = 0.3f;
        public float infoMoveDuration = 0.4f;
    }

    [System.Serializable]
    public class RevealSettings
    {
        public float duration = 0.3f;
    }

    public BoxSettings box;
    public HoleSettings hole;
    public YarnSettings yarn;
    public YarnRequiredPopupSettings yarnRequiredPopup;
    public RevealSettings reveal;
}
