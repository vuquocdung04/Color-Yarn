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
        ApplySprites();
    }

    private void ApplySprites()
    {
        var entry = ColorRepo.Instance != null ? ColorRepo.Instance.GetSet(colorKey) : null;
        if (entry != null) SetSprites(entry.spriteBox, entry.spriteCover);
    }

    private string NextColorKey() => "White";

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
        {
            colorKey = NextColorKey();
            yr.Finished += OnLastRollFinished;
        }
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

        currentYarnRoll = 0;
        if (spriteCoverRenderer != null) spriteCoverRenderer.gameObject.SetActive(false);

        ApplySprites();
        PullFromHoles();
    }

    private void PullFromHoles()
    {
        if (HolesTemp.Instance == null) return;

        int empty = maxCapacity - currentYarnRoll;
        if (empty <= 0) return;

        var rolls = HolesTemp.Instance.TakeMatching(colorKey, empty);
        foreach (var r in rolls)
        {
            if (r == null || currentYarnRoll >= slots.Count) continue;
            Transform slot = slots[currentYarnRoll];
            r.transform.SetParent(slot);
            r.transform.localPosition = Vector3.zero;
            r.transform.localRotation = Quaternion.identity;
            r.transform.localScale = Vector3.one;
            spawnedRolls.Add(r);
            currentYarnRoll++;
        }

        if (currentYarnRoll >= maxCapacity)
        {
            colorKey = NextColorKey();
            CloseAndResetAsync(this.GetCancellationTokenOnDestroy()).Forget();
        }
    }
}
