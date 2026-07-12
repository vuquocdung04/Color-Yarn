using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using EventDispatcher;
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

    [SerializeField] private ParticleSystem doneFX;

    private readonly List<YarnRoll> spawnedRolls = new();
    private readonly List<YarnRoll> reservedRolls = new();
    private int currentYarnRoll;
    private bool hasNextColor;

    private SpriteRenderer[] slotPlaceholders;

    public bool IsClosing { get; private set; }
    public bool IsLocked => isLocked;

    public string ColorKey => colorKey;
    public bool CanAccept(string key) => !isLocked && colorKey == key && currentYarnRoll < MaxCapacity;

    private void Awake()
    {
        if (spriteCoverRenderer != null) spriteCoverRenderer.gameObject.SetActive(false);
        if (lockObject != null) lockObject.SetActive(isLocked);

        slotPlaceholders = new SpriteRenderer[slots.Count];
        for (int i = 0; i < slots.Count; i++)
            slotPlaceholders[i] = slots[i].GetComponentInChildren<SpriteRenderer>();
    }

    private void HideSlotPlaceholder(int index)
    {
        if (index >= 0 && index < slotPlaceholders.Length && slotPlaceholders[index] != null)
            slotPlaceholders[index].gameObject.SetActive(false);
    }

    private void ShowAllSlotPlaceholders()
    {
        foreach (var sr in slotPlaceholders)
            if (sr != null) sr.gameObject.SetActive(true);
    }

    private void PlayBounce()
    {
        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOScale(0.95f, 0.1f).SetEase(Ease.InOutQuad));
        seq.Append(transform.DOScale(1f, 0.1f).SetEase(Ease.InOutQuad));
    }

    public void OnTapped()
    {
        if (!isLocked) return;
        Debug.Log($"[BoxSlot] tapped locked box {name} - popup here later");
        Unlock();
    }

    private void Unlock()
    {
        string key = GameAlgorithm.Instance.PickRescueColor();
        if (string.IsNullOrEmpty(key))
        {
            Debug.LogError($"[BoxSlot] {name} unlock nhung khong con mau nao de chon");
            return;
        }

        isLocked = false;
        SetColor(key);
        if (lockObject != null) lockObject.SetActive(false);
        ReserveFromHoles();
        PlaceReservedRolls();
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

    private string NextColorKey() => GameAlgorithm.Instance.PickNextColor();

    private void AdvanceToNextColor()
    {
        this.PostEvent(EventID.YARN_COLLECTED);

        string next = NextColorKey();
        hasNextColor = !string.IsNullOrEmpty(next);
        if (hasNextColor)
        {
            colorKey = next;
            ReserveFromHoles();
        }
    }

    private void ReserveFromHoles()
    {
        int needed = MaxCapacity - reservedRolls.Count;
        if (needed <= 0) return;

        var rolls = AweSomeBox.Instance.TakeMatching(colorKey, needed);
        reservedRolls.AddRange(rolls);

        needed -= rolls.Count;
        if (needed > 0)
            reservedRolls.AddRange(HolesTemp.Instance.TakeMatching(colorKey, needed));
    }

    public void Spawn(InteractableObject target, YarnRoll prefab, float duration)
    {
        if (currentYarnRoll >= MaxCapacity || currentYarnRoll >= slots.Count) return;

        Transform slot = slots[currentYarnRoll];
        YarnRoll yr = Instantiate(prefab);
        yr.Setup(target.GetComponent<Renderer>(), target.ColorKey);
        yr.Play(duration);
        yr.PlaceInSlot(slot, PlayBounce);
        HideSlotPlaceholder(currentYarnRoll);

        spawnedRolls.Add(yr);
        currentYarnRoll++;

        if (currentYarnRoll >= MaxCapacity)
        {
            AdvanceToNextColor();
            yr.Finished += OnLastRollFinished;
        }
    }

    public bool TryInstantFill(YarnRoll prefab)
    {
        if (isLocked) return false;

        int needed = MaxCapacity - currentYarnRoll;
        if (needed <= 0) return false;

        int taken = LevelController.Instance.CurrentYarnObj.ConsumeRandomByColor(colorKey, needed);

        for (int i = 0; i < taken && currentYarnRoll < slots.Count; i++)
        {
            Transform slot = slots[currentYarnRoll];
            YarnRoll yr = Instantiate(prefab);
            yr.Setup(null, colorKey);
            yr.ShowCompleted();
            yr.PlaceInSlot(slot, PlayBounce);
            HideSlotPlaceholder(currentYarnRoll);

            spawnedRolls.Add(yr);
            currentYarnRoll++;
        }

        if (currentYarnRoll >= MaxCapacity)
        {
            AdvanceToNextColor();
            CloseAndResetAsync(this.GetCancellationTokenOnDestroy()).Forget();
        }

        return true;
    }

    private void OnLastRollFinished()
    {
        CloseAndResetAsync(this.GetCancellationTokenOnDestroy()).Forget();
    }

    private async UniTaskVoid CloseAndResetAsync(CancellationToken token)
    {
        IsClosing = true;
        try
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

                if (doneFX != null) doneFX.Play();
            }

            if (token.IsCancellationRequested) return;

            float targetY = hasNextColor ? boxRestPos.y + BoxCreator.Instance.BoxHopOffset : boxRestPos.y;

            Sequence boxSeq = DOTween.Sequence();
            boxSeq.Append(transform.DOMoveY(boxRestPos.y - BoxCreator.Instance.BoxDipOffset, BoxCreator.Instance.BoxDipDuration).SetEase(Ease.InOutQuad));
            boxSeq.AppendInterval(BoxCreator.Instance.BoxHoldDuration);
            boxSeq.Append(transform.DOMoveY(targetY, duration).SetEase(Ease.InOutQuad));

            await boxSeq.AsyncWaitForCompletion();

            if (token.IsCancellationRequested || !hasNextColor) return;

            foreach (var yr in spawnedRolls)
                if (yr != null) Destroy(yr.gameObject);
            spawnedRolls.Clear();

            currentYarnRoll = 0;
            ShowAllSlotPlaceholders();
            if (spriteCoverRenderer != null) spriteCoverRenderer.gameObject.SetActive(false);

            ApplySprites();

            await transform.DOMoveY(boxRestPos.y, duration)
                .SetEase(Ease.OutCubic).AsyncWaitForCompletion();

            if (token.IsCancellationRequested) return;

            PlaceReservedRolls();
        }
        finally
        {
            IsClosing = false;
            HolesTemp.Instance.RecheckLose();
        }
    }

    private void PlaceReservedRolls()
    {
        ReserveFromHoles();

        while (reservedRolls.Count > 0 && currentYarnRoll < MaxCapacity && currentYarnRoll < slots.Count)
        {
            YarnRoll r = reservedRolls[0];
            reservedRolls.RemoveAt(0);
            if (r == null) continue;

            Transform slot = slots[currentYarnRoll];
            r.PlaceInSlot(slot, PlayBounce);
            HideSlotPlaceholder(currentYarnRoll);
            spawnedRolls.Add(r);
            currentYarnRoll++;
        }

        if (currentYarnRoll >= MaxCapacity)
        {
            AdvanceToNextColor();
            CloseAndResetAsync(this.GetCancellationTokenOnDestroy()).Forget();
        }
    }
}
