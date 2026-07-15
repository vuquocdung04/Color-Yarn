using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class YarnObj : MonoBehaviour
{
    [SerializeField] private Transform root;

    private float yarnDepthSpread;

    [Header("Grow")]
    [SerializeField] private float growDuration = 0.35f;
    [SerializeField] private Ease growEase = Ease.OutBack;

    private List<InteractableObject> interactableObjects;
    private readonly Dictionary<string, int> totalByColor = new();

    public Transform Root => root != null ? root : transform;
    public float GrowDuration => growDuration;
    public Ease GrowEase => growEase;

    public int TotalLen { get; private set; }
    public IReadOnlyDictionary<string, int> TotalByColor => totalByColor;

    public void Init(int requiredYarn, float yarnDepthSpread)
    {
        this.yarnDepthSpread = yarnDepthSpread;
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
        DistributeExtraCubes(extra);

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

    private void DistributeExtraCubes(int extra)
    {
        int n = interactableObjects.Count;
        if (n == 0 || extra <= 0) return;

        var weights = new float[n];
        float totalWeight = 0f;
        for (int i = 0; i < n; i++)
        {
            weights[i] = Mathf.Pow(Random.value, yarnDepthSpread);
            totalWeight += weights[i];
        }

        var shares = new float[n];
        var alloc = new int[n];
        int allocatedSum = 0;
        for (int i = 0; i < n; i++)
        {
            shares[i] = totalWeight > 0f ? weights[i] / totalWeight * extra : extra / (float)n;
            alloc[i] = Mathf.FloorToInt(shares[i]);
            allocatedSum += alloc[i];
        }

        var order = new List<int>();
        for (int i = 0; i < n; i++) order.Add(i);
        order.Sort((a, b) => (shares[b] - alloc[b]).CompareTo(shares[a] - alloc[a]));

        int remainder = Mathf.Min(extra - allocatedSum, n);
        for (int i = 0; i < remainder; i++)
            alloc[order[i]]++;

        for (int i = 0; i < n; i++)
            if (interactableObjects[i] != null) interactableObjects[i].totalCube = alloc[i];
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

    private static void AddColor(Dictionary<string, int> dict, string key)
    {
        if (string.IsNullOrEmpty(key)) return;
        dict[key] = dict.TryGetValue(key, out int c) ? c + 1 : 1;
    }

    public int ConsumeRandomByColor(string colorKey, int count)
    {
        var matches = new List<InteractableObject>();
        foreach (var obj in GetComponentsInChildren<InteractableObject>())
            if (obj != null && obj.ColorKey == colorKey && !obj.IsBusy) matches.Add(obj);

        for (int i = matches.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (matches[i], matches[j]) = (matches[j], matches[i]);
        }

        int take = Mathf.Min(count, matches.Count);
        for (int i = 0; i < take; i++)
            matches[i].InstantConsume();

        return take;
    }

    public async UniTask SpinIntro(float rotateY, float spinDuration)
    {
        await transform.DORotate(new Vector3(0f, rotateY, 0f), spinDuration, RotateMode.FastBeyond360)
            .SetEase(Ease.OutQuad)
            .AsyncWaitForCompletion();
    }

    public async UniTask TiltIntro(float tiltX, float tiltDuration)
    {
        await transform.DOLocalRotate(new Vector3(tiltX, 0f, 0f), tiltDuration)
            .AsyncWaitForCompletion();
    }

    public void AddLen(InteractableObject obj)
    {
        if (obj != null) interactableObjects.Add(obj);
    }

    public void RemoveLen(InteractableObject obj)
    {
        interactableObjects.Remove(obj);
    }
}
