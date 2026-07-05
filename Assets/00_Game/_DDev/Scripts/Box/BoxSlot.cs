using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class BoxSlot : MonoBehaviour
{
    [SerializeField] private string colorKey;
    [SerializeField] private List<Transform> slots;
    [SerializeField] private SpriteRenderer spriteBoxRenderer;
    [SerializeField] private SpriteRenderer spriteCoverRenderer;
    [SerializeField] private YarnRoll yarnRollPrefab;
    [SerializeField] private int maxCapacity = 3;

    private readonly List<YarnRoll> spawnedRolls = new();
    private int currentYarnRoll;

    public string ColorKey => colorKey;
    public bool CanAccept(string key) => colorKey == key && currentYarnRoll < maxCapacity;

    private void Awake()
    {
        if (spriteCoverRenderer != null) spriteCoverRenderer.gameObject.SetActive(false);
    }

    public void SetSprites(Sprite box, Sprite cover)
    {
        if (spriteBoxRenderer != null) spriteBoxRenderer.sprite = box;
        if (spriteCoverRenderer != null) spriteCoverRenderer.sprite = cover;
    }

    public void SetColor(string key)
    {
        colorKey = key;

        var entry = ColorRepo.Instance != null ? ColorRepo.Instance.GetSet(key) : null;
        if (entry != null) SetSprites(entry.spriteBox, entry.spriteCover);
    }

    public void Spawn(InteractableObject target, float duration)
    {
        if (currentYarnRoll >= maxCapacity || currentYarnRoll >= slots.Count) return;

        Transform slot = slots[currentYarnRoll];
        YarnRoll yr = Instantiate(yarnRollPrefab, slot.position, slot.rotation, slot);
        yr.Setup(target.GetComponent<Renderer>(), target.ColorKey);
        yr.Play(duration);

        spawnedRolls.Add(yr);
        currentYarnRoll++;

        if (currentYarnRoll >= maxCapacity)
            yr.Finished += OnLastRollFinished;
    }

    private void OnLastRollFinished()
    {
        CloseAndResetAsync(this.GetCancellationTokenOnDestroy()).Forget();
    }

    private void CloseLid()
    {
        if (spriteCoverRenderer != null) spriteCoverRenderer.gameObject.SetActive(true);
    }

    private async UniTaskVoid CloseAndResetAsync(CancellationToken token)
    {
        CloseLid();

        foreach (var yr in spawnedRolls)
            if (yr != null) Destroy(yr.gameObject);
        spawnedRolls.Clear();

        await UniTask.Delay(System.TimeSpan.FromSeconds(1f), cancellationToken: token);

        OnBoxReset();

        if (spriteCoverRenderer != null) spriteCoverRenderer.gameObject.SetActive(false);
        currentYarnRoll = 0;
    }

    public void OnBoxReset()
    {
        SetColor("White");
    }
}
