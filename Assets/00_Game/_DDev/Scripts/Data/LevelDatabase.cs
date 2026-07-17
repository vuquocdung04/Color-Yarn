using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;


[System.Serializable]
public class DataLevel
{
    public string levelName;
    [FormerlySerializedAs("minZoomCamera")] public float minZoomScale = 0.6f;
    [FormerlySerializedAs("maxZoomCamera")] public float maxZoomScale = 2f;
    public int requiredLen;

    [Range(0f, 1f)] public float boxDifficulty;
    [Range(0f, 10f)] public float yarnDepthSpread;
    public bool isHard;

    [HorizontalGroup("Preview"), PreviewField(70), HideLabel]
    public GameObject levelPrefab;

    [HorizontalGroup("Preview"), PreviewField(70), HideLabel]
    public Sprite levelSprite;
}

[CreateAssetMenu(fileName = "LevelDatabase", menuName = "Data/Level Database")]
public class LevelDatabase : ScriptableObject
{
    [SerializeField] private List<DataLevel> levels;

    public List<DataLevel> Levels => levels;
}
