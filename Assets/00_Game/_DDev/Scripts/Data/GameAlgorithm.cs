using System.Collections.Generic;
using UnityEngine;

public class GameAlgorithm : MonoBehaviour
{
    public static GameAlgorithm Instance { get; private set; }

    public void InitInstance() => Instance = this;

    [SerializeField] private int targetDifficultyMin = 3;
    [SerializeField] private int targetDifficultyMax = 6;

    private readonly Dictionary<string, int> remainingByColor = new();

    public void Init()
    {
    }

    public void RegisterTotals(IReadOnlyDictionary<string, int> totals)
    {
        remainingByColor.Clear();
        foreach (var kv in totals)
            remainingByColor[kv.Key] = kv.Value;
    }

    public void Reserve(string key)
    {
        if (string.IsNullOrEmpty(key)) return;
        if (remainingByColor.TryGetValue(key, out int c))
            remainingByColor[key] = Mathf.Max(0, c - 3);
    }

    public string PickNextColor()
    {
        var layersByColor = YarnObj.Instance.GetLayersByColor();
        var parkedByColor = HolesTemp.Instance.GetParkedColorCounts();

        var candidates = new List<string>(remainingByColor.Keys);
        Shuffle(candidates);

        string best = null;
        int bestDiff = int.MaxValue;

        foreach (var color in candidates)
        {
            if (remainingByColor[color] < 3) continue;

            int cost = ComputeColorCost(color, layersByColor, parkedByColor);
            int diff = cost < targetDifficultyMin ? targetDifficultyMin - cost
                     : cost > targetDifficultyMax ? cost - targetDifficultyMax
                     : 0;

            if (diff < bestDiff)
            {
                bestDiff = diff;
                best = color;
            }
        }

        if (best != null) Reserve(best);
        return best;
    }

    private static int ComputeColorCost(string color, Dictionary<string, List<int>> layersByColor, Dictionary<string, int> parkedByColor)
    {
        var costs = new List<int>();

        int parkedCount = parkedByColor.TryGetValue(color, out int p) ? p : 0;
        for (int i = 0; i < parkedCount; i++) costs.Add(0);

        if (layersByColor.TryGetValue(color, out var layers))
            costs.AddRange(layers);

        costs.Sort();

        int sum = 0;
        for (int i = 0; i < Mathf.Min(3, costs.Count); i++) sum += costs[i];
        return sum;
    }

    public string PickRescueColor()
    {
        var parked = HolesTemp.Instance.GetParkedColorCounts();
        var candidates = new List<string>(parked.Keys);
        Shuffle(candidates);

        string best = null;
        int bestCount = 0;
        foreach (var color in candidates)
        {
            if (!remainingByColor.TryGetValue(color, out int remaining) || remaining < 3) continue;

            int count = parked[color];
            if (count > bestCount)
            {
                bestCount = count;
                best = color;
            }
        }

        if (best != null)
        {
            Reserve(best);
            return best;
        }

        return PickNextColor();
    }

    private static void Shuffle(List<string> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
