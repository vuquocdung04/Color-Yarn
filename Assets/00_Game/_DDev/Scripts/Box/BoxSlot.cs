using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using EventDispatcher;
using UnityEngine;
using UnityEngine.Serialization;

public class BoxSlot : MonoBehaviour
{
    private const int MaxCapacity = 3;

    private enum BoxState { Locked, Available, Closing, BoosterActive }

    [SerializeField] private string colorKey;
    [SerializeField] private List<Transform> slots;
    [SerializeField] private SpriteRenderer spriteBoxRenderer;
    [SerializeField] private SpriteRenderer spriteCoverRenderer;

    [FormerlySerializedAs("isLocked")]
    [SerializeField] private bool startLocked;
    [SerializeField] private GameObject lockObject;

    [SerializeField] private ParticleSystem doneFX;
    [SerializeField] private Magnet magnet;

    private readonly List<YarnRoll> spawnedRolls = new();
    private readonly List<YarnRoll> reservedRolls = new();
    private int currentYarnRoll;
    private bool hasNextColor;

    private SpriteRenderer[] slotPlaceholders;
    private Vector3 introRestPosition;

    private BoxState state;

    public bool IsLocked => state == BoxState.Locked;
    public bool IsBusy => state == BoxState.Closing || state == BoxState.BoosterActive;
    public bool CanAccept(string key) => state == BoxState.Available && colorKey == key && currentYarnRoll < MaxCapacity;

    private void SetState(BoxState next)
    {
        state = next;
        if (lockObject != null) lockObject.SetActive(next == BoxState.Locked);
    }

    private void Awake()
    {
        if (spriteCoverRenderer != null) spriteCoverRenderer.gameObject.SetActive(false);
        SetState(startLocked ? BoxState.Locked : BoxState.Available);

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

    public void PrepareIntro(float offsetX)
    {
        introRestPosition = transform.position;
        transform.position = introRestPosition + new Vector3(offsetX, 0f, 0f);
    }

    public void PlayIntroMove(float duration)
    {
        transform.DOMove(introRestPosition, duration).SetEase(Ease.OutCubic);
    }

    public void OnTapped()
    {
        if (!IsLocked) return;
        _ = AddBox.Setup(GameScene.GetPopupHolder(), box => box.SetupAndShow(this));
    }

    public void Unlock()
    {
        string key = GameAlgorithm.Instance.PickRescueColor();
        if (string.IsNullOrEmpty(key))
        {
            Debug.LogError($"[BoxSlot] {name} unlock nhung khong con mau nao de chon");
            return;
        }

        SetState(BoxState.Available);
        SetColor(key);
        ReserveFromHolesFirst();
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

    private void ReserveFromHolesFirst()
    {
        int needed = MaxCapacity - reservedRolls.Count;
        if (needed <= 0) return;

        reservedRolls.AddRange(HolesTemp.Instance.TakeMatching(colorKey, needed));

        needed = MaxCapacity - reservedRolls.Count;
        if (needed > 0)
            reservedRolls.AddRange(AweSomeBox.Instance.TakeMatching(colorKey, needed));
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

    public void Booster1Fill(YarnRoll prefab)
    {
        SetState(BoxState.BoosterActive);
        bool filled = false;
        magnet.Activate(
            onSuck: () => filled = FillInstant(prefab),
            onDone: () =>
            {
                if (filled)
                {
                    CloseLid();
                }
                else
                {
                    SetState(BoxState.Available);
                    HolesTemp.Instance.RecheckLose();
                }
            });
    }

    private bool FillInstant(YarnRoll prefab)
    {
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
            return true;
        }

        return false;
    }

    public void CloseLid() => CloseAndResetAsync(this.GetCancellationTokenOnDestroy()).Forget();

    private void OnLastRollFinished()
    {
        CloseAndResetAsync(this.GetCancellationTokenOnDestroy()).Forget();
    }

    private async UniTaskVoid CloseAndResetAsync(CancellationToken token)
    {
        SetState(BoxState.Closing);
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
            _ = boxSeq.Append(transform.DOMoveY(boxRestPos.y - BoxCreator.Instance.BoxDipOffset, BoxCreator.Instance.BoxDipDuration).SetEase(Ease.InOutQuad));
            _ = boxSeq.AppendInterval(BoxCreator.Instance.BoxHoldDuration);
            _ = boxSeq.Append(transform.DOMoveY(targetY, duration).SetEase(Ease.InOutQuad));

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
            SetState(BoxState.Available);
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
