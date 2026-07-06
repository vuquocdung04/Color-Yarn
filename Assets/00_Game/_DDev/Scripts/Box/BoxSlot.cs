using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class BoxSlot : MonoBehaviour
{
    private const int MaxCapacity = 3;

    [SerializeField] private string colorKey;
    [SerializeField] private List<Transform> slots;
    [SerializeField] private SpriteRenderer spriteBoxRenderer;
    [SerializeField] private SpriteRenderer spriteCoverRenderer;

    [SerializeField] private bool isLocked;
    [SerializeField] private GameObject lockObject;

    [SerializeField] private ParticleSystem particleSystem;

    private readonly List<YarnRoll> spawnedRolls = new();
    private int currentYarnRoll;

    public string ColorKey => colorKey;
    public bool CanAccept(string key) => !isLocked && colorKey == key && currentYarnRoll < MaxCapacity;

    private void Awake()
    {
        if (spriteCoverRenderer != null) spriteCoverRenderer.gameObject.SetActive(false);
        if (lockObject != null) lockObject.SetActive(isLocked);
    }

    public void OnTapped()
    {
        if (!isLocked) return;
        Debug.Log($"[BoxSlot] tapped locked box {name} - popup here later");
        Unlock();
    }

    private void Unlock()
    {
        isLocked = false;
        SetColor(NextColorKey());
        if (lockObject != null) lockObject.SetActive(false);
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
        var entry = ColorRepo.Instance.GetSet(colorKey);
        if (entry != null) SetSprites(entry.spriteBox, entry.spriteCover);
    }

    private string NextColorKey()
    {
        var entry = ColorRepo.Instance.GetRandom();
        return entry != null ? entry.key : colorKey;
    }

    public void Spawn(InteractableObject target, YarnRoll prefab, float duration)
    {
        if (currentYarnRoll >= MaxCapacity || currentYarnRoll >= slots.Count) return;

        Transform slot = slots[currentYarnRoll];
        YarnRoll yr = Instantiate(prefab, slot.position, slot.rotation, slot);
        yr.Setup(target.GetComponent<Renderer>(), target.ColorKey);
        yr.Play(duration);

        spawnedRolls.Add(yr);
        currentYarnRoll++;

        if (currentYarnRoll >= MaxCapacity)
        {
            colorKey = NextColorKey();
            yr.Finished += OnLastRollFinished;
        }
    }

    private void OnLastRollFinished()
    {
        CloseAndResetAsync(this.GetCancellationTokenOnDestroy()).Forget();
    }

    private async UniTaskVoid CloseAndResetAsync(CancellationToken token)
    {
        float duration = BoxCreator.Instance.BoxAnimDuration;
        Vector3 boxRestPos = transform.position;

        if (spriteCoverRenderer != null)
        {
            Transform coverT = spriteCoverRenderer.transform;
            Vector3 coverRestPos = coverT.position;
            coverT.position = coverRestPos + Vector3.up * BoxCreator.Instance.CoverUpOffset;
            spriteCoverRenderer.gameObject.SetActive(true);

            await coverT.DOMove(coverRestPos, duration).SetEase(Ease.Linear).AsyncWaitForCompletion();

            if (particleSystem != null) particleSystem.Play();
        }

        if (token.IsCancellationRequested) return;

        Sequence boxSeq = DOTween.Sequence();
        boxSeq.Append(transform.DOMoveY(boxRestPos.y - BoxCreator.Instance.BoxDipOffset, BoxCreator.Instance.BoxDipDuration).SetEase(Ease.InOutQuad));
        boxSeq.AppendInterval(BoxCreator.Instance.BoxHoldDuration);
        boxSeq.Append(transform.DOMoveY(boxRestPos.y + BoxCreator.Instance.BoxHopOffset, duration).SetEase(Ease.InOutQuad));

        await boxSeq.AsyncWaitForCompletion();

        if (token.IsCancellationRequested) return;

        foreach (var yr in spawnedRolls)
            if (yr != null) Destroy(yr.gameObject);
        spawnedRolls.Clear();

        currentYarnRoll = 0;
        if (spriteCoverRenderer != null) spriteCoverRenderer.gameObject.SetActive(false);

        ApplySprites();

        await transform.DOMoveY(boxRestPos.y, duration)
            .SetEase(Ease.OutCubic).AsyncWaitForCompletion();

        if (token.IsCancellationRequested) return;

        PullFromHoles();
    }

    private void PullFromHoles()
    {
        int empty = MaxCapacity - currentYarnRoll;
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

        if (currentYarnRoll >= MaxCapacity)
        {
            colorKey = NextColorKey();
            CloseAndResetAsync(this.GetCancellationTokenOnDestroy()).Forget();
        }
    }
}
