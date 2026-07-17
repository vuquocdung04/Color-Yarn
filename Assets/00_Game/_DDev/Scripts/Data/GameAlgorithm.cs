using System.Collections.Generic;
using UnityEngine;

public class GameAlgorithm : MonoBehaviour
{
    public static GameAlgorithm Instance { get; private set; }

    public void InitInstance() => Instance = this;

    private float boxDifficulty = 0.5f;

    private readonly Dictionary<string, int> remainingByColor = new();

    public void Init()
    {
    }

    public void SetDifficulty(float difficulty) => boxDifficulty = difficulty;

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
        var layersByColor = LevelController.Instance.CurrentYarnObj.GetLayersByColor();
        var parkedByColor = HolesTemp.Instance.GetParkedColorCounts();
        var storedByColor = AweSomeBox.Instance.GetStoredColorCounts();

        var candidates = new List<string>(remainingByColor.Keys);
        Shuffle(candidates);

        var costs = new Dictionary<string, int>();
        int minCost = int.MaxValue;
        int maxCost = int.MinValue;

        foreach (var color in candidates)
        {
            if (AvailableCount(color, parkedByColor, storedByColor) < 3) continue;

            int cost = ComputeColorCost(color, layersByColor, parkedByColor, storedByColor);
            costs[color] = cost;
            if (cost < minCost) minCost = cost;
            if (cost > maxCost) maxCost = cost;
        }

        if (costs.Count == 0) return null;

        float target = Mathf.Lerp(minCost, maxCost, boxDifficulty);

        string best = null;
        float bestDiff = float.MaxValue;

        foreach (var color in candidates)
        {
            if (!costs.TryGetValue(color, out int cost)) continue;

            float diff = Mathf.Abs(cost - target);
            if (diff < bestDiff)
            {
                bestDiff = diff;
                best = color;
            }
        }

        Reserve(best);
        return best;
    }

    private int AvailableCount(string color, Dictionary<string, int> parkedByColor, Dictionary<string, int> storedByColor)
    {
        int ledger = remainingByColor.TryGetValue(color, out int r) ? r : 0;
        int parked = parkedByColor.TryGetValue(color, out int p) ? p : 0;
        int stored = storedByColor.TryGetValue(color, out int s) ? s : 0;
        return Mathf.Max(ledger, parked + stored);
    }

    private static int ComputeColorCost(string color, Dictionary<string, List<int>> layersByColor, Dictionary<string, int> parkedByColor, Dictionary<string, int> storedByColor)
    {
        var costs = new List<int>();

        int parkedCount = parkedByColor.TryGetValue(color, out int p) ? p : 0;
        for (int i = 0; i < parkedCount; i++) costs.Add(0);

        int storedCount = storedByColor.TryGetValue(color, out int s) ? s : 0;
        for (int i = 0; i < storedCount; i++) costs.Add(0);

        if (layersByColor.TryGetValue(color, out var layers))
            costs.AddRange(layers);

        costs.Sort();

        int sum = 0;
        for (int i = 0; i < Mathf.Min(3, costs.Count); i++) sum += costs[i];
        return sum;
    }

    public string PickRescueColor()
    {
        string best = PickMostAvailable(HolesTemp.Instance.GetParkedColorCounts());
        if (best == null)
            best = PickMostAvailable(AweSomeBox.Instance.GetStoredColorCounts());

        if (best != null)
        {
            Reserve(best);
            return best;
        }

        return PickNextColor();
    }

    private string PickMostAvailable(Dictionary<string, int> counts)
    {
        string best = null;
        int bestCount = 0;
        foreach (var kv in counts)
        {
            if (!remainingByColor.TryGetValue(kv.Key, out int remaining) || remaining < 3) continue;
            if (kv.Value > bestCount)
            {
                bestCount = kv.Value;
                best = kv.Key;
            }
        }
        return best;
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
