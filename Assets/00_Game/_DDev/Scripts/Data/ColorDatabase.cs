using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class ColorEntry
{
    public string key;

    [HorizontalGroup("Preview"), PreviewField(60), HideLabel]
    public Material material;

    [HorizontalGroup("Preview"), PreviewField(60), HideLabel]
    public Sprite spriteBox;

    [HorizontalGroup("Preview"), PreviewField(60), HideLabel]
    public Sprite spriteCover;
}

[CreateAssetMenu(fileName = "ColorDatabase", menuName = "Data/Color Database")]
public class ColorDatabase : ScriptableObject
{
    [SerializeField] private List<ColorEntry> entries;

    [Button("Auto Fill Keys")]
    private void AutoFillKeys()
    {
        foreach (var e in entries)
        {
            if (e.material != null) e.key = e.material.name;
        }
    }

    public ColorEntry GetSet(string key) => entries.Find(e => e.key == key);

    public ColorEntry GetRandom() => entries.Count > 0 ? entries[Random.Range(0, entries.Count)] : null;
}
