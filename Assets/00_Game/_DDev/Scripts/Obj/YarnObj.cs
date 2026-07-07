using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class YarnObj : MonoBehaviour
{
    public static YarnObj Instance { get; private set; }

    [SerializeField] private Transform root;
    [SerializeField] private int requiredYarn = 20;

    [Header("Grow")]
    [SerializeField] private float growDuration = 0.35f;
    [SerializeField] private Ease growEase = Ease.OutBack;

    private List<InteractableObject> interactableObjects;
    private readonly Dictionary<string, int> totalByColor = new();

    public Transform Root => root != null ? root : transform;
    public float GrowDuration => growDuration;
    public Ease GrowEase => growEase;

    public int TotalLen { get; private set; }
    public int CurrentLen { get; private set; }
    public IReadOnlyDictionary<string, int> TotalByColor => totalByColor;

    private void Start() => Init();

    private void Init()
    {
        Instance = this;
        interactableObjects = new List<InteractableObject>(GetComponentsInChildren<InteractableObject>());
        GenerateChildRuntime(requiredYarn);
    }

    public void GenerateChildRuntime(int requiredYarn)
    {
        int remainder = requiredYarn % 3;
        if (remainder != 0) requiredYarn += 3 - remainder;

        foreach (var obj in interactableObjects)
            if (obj != null) obj.totalCube = 0;

        int extra = requiredYarn - interactableObjects.Count;
        while (extra > 0 && interactableObjects.Count > 0)
        {
            var obj = interactableObjects[Random.Range(0, interactableObjects.Count)];
            if (obj == null) continue;
            obj.totalCube++;
            extra--;
        }

        TotalLen = 0;
        foreach (var obj in interactableObjects)
        {
            if (obj == null) continue;
            obj.Init();
            obj.BuildCore();
            TotalLen += obj.totalCube + 1;
        }

        AssignColorsInGroups();
    }

    private void AssignColorsInGroups()
    {
        totalByColor.Clear();

        var roots = new HashSet<InteractableObject>(interactableObjects);
        var allObjects = new List<InteractableObject>(GetComponentsInChildren<InteractableObject>());

        foreach (var obj in allObjects)
            if (obj != null) obj.Init();

        var children = new List<InteractableObject>();
        foreach (var obj in allObjects)
        {
            if (obj == null) continue;
            if (roots.Contains(obj))
                AddColor(totalByColor, obj.ColorKey);
            else
                children.Add(obj);
        }

        for (int i = children.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (children[i], children[j]) = (children[j], children[i]);
        }

        int idx = 0;
        while (idx < children.Count)
        {
            var entry = ColorRepo.Instance.GetRandom();
            if (entry == null) break;

            int groupSize = Mathf.Min(3, children.Count - idx);
            for (int k = 0; k < groupSize; k++, idx++)
                children[idx].ApplyColor(entry.key);

            totalByColor[entry.key] = totalByColor.TryGetValue(entry.key, out int c) ? c + groupSize : groupSize;
        }

        GameAlgorithm.Instance.RegisterTotals(totalByColor);
    }

    public List<string> GetRootColorKeys()
    {
        var result = new List<string>();
        foreach (var obj in interactableObjects)
        {
            if (obj == null || string.IsNullOrEmpty(obj.ColorKey)) continue;
            if (!result.Contains(obj.ColorKey)) result.Add(obj.ColorKey);
        }
        return result;
    }

    public Dictionary<string, List<int>> GetLayersByColor()
    {
        var result = new Dictionary<string, List<int>>();
        foreach (var obj in GetComponentsInChildren<InteractableObject>())
        {
            if (obj == null || string.IsNullOrEmpty(obj.ColorKey)) continue;
            if (!result.TryGetValue(obj.ColorKey, out var list))
            {
                list = new List<int>();
                result[obj.ColorKey] = list;
            }
            list.Add(obj.Layer);
        }
        return result;
    }

    public Dictionary<string, int> GetReachableColorCounts()
    {
        var result = new Dictionary<string, int>();
        foreach (var obj in interactableObjects)
        {
            if (obj == null) continue;
            AddColor(result, obj.ColorKey);

            if (obj.HasCore)
            {
                var childObj = obj.Core.GetComponent<InteractableObject>();
                if (childObj != null) AddColor(result, childObj.ColorKey);
            }
        }
        return result;
    }

    private static void AddColor(Dictionary<string, int> dict, string key)
    {
        if (string.IsNullOrEmpty(key)) return;
        dict[key] = dict.TryGetValue(key, out int c) ? c + 1 : 1;
    }

    public void AddLen(InteractableObject obj)
    {
        if (obj != null) interactableObjects.Add(obj);
    }

    public void RemoveLen(InteractableObject obj)
    {
        interactableObjects.Remove(obj);
        CurrentLen++;
    }
}
