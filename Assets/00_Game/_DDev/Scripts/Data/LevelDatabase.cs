using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;


[System.Serializable]
public class DataLevel
{
    public string levelName;
    public float minZoomCamera;
    public float maxZoomCamera;
    public int requiredLen;

    [Range(0f, 1f)] public float boxDifficulty;
    [Range(0f, 10f)] public float yarnDepthSpread;

    [HorizontalGroup("Preview"), PreviewField(70), HideLabel]
    public GameObject levelPrefab;

    [HorizontalGroup("Preview"), PreviewField(70), HideLabel]
    public Sprite levelSprite;
}

[CreateAssetMenu(fileName = "LevelDatabase", menuName = "Data/Level Database")]
public class LevelDatabase : ScriptableObject
{
    [SerializeField] private List<DataLevel> levels;
}
