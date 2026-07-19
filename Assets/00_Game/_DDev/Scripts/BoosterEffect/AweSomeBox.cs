using System;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class AweSomeBox : MonoBehaviour
{
    public static AweSomeBox Instance { get; private set; }

    public void InitInstance() => Instance = this;

    [SerializeField] private float flyDuration = 0.5f;
    [SerializeField] private float jumpPower = 1f;
    [SerializeField] private GameObject visual;

    [ShowInInspector, ReadOnly]
    public List<YarnRoll> stored = new();

    public void Init()
    {
        UpdateVisual();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void Store(List<YarnRoll> rolls, Action onAllArrived)
    {
        var valid = rolls.FindAll(r => r != null);
        if (valid.Count == 0)
        {
            onAllArrived?.Invoke();
            return;
        }

        int remaining = valid.Count;
        foreach (var r in valid)
        {
            stored.Add(r);
            r.transform.DOJump(transform.position, jumpPower, 1, flyDuration).OnComplete(() =>
            {
                r.gameObject.SetActive(false);
                remaining--;
                if (remaining == 0) onAllArrived?.Invoke();
            });
        }

        UpdateVisual();
    }

    public Dictionary<string, int> GetStoredColorCounts()
    {
        var result = new Dictionary<string, int>();
        foreach (var r in stored)
        {
            if (r == null || string.IsNullOrEmpty(r.ColorKey)) continue;
            result[r.ColorKey] = result.TryGetValue(r.ColorKey, out int c) ? c + 1 : 1;
        }
        return result;
    }

    public List<YarnRoll> TakeMatching(string key, int max)
    {
        var result = new List<YarnRoll>();
        for (int i = stored.Count - 1; i >= 0 && result.Count < max; i--)
        {
            var r = stored[i];
            if (r == null || r.ColorKey != key) continue;

            result.Add(r);
            stored.RemoveAt(i);
            r.gameObject.SetActive(true);
        }

        UpdateVisual();
        return result;
    }

    private void UpdateVisual()
    {
        if (visual != null) visual.SetActive(stored.Count > 0);
    }
}
