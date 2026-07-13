using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using EventDispatcher;
using UnityEngine;

public class HolesTemp : MonoBehaviour
{
    public static HolesTemp Instance { get; private set; }

    public void InitInstance() => Instance = this;

    [SerializeField] private List<HoleTemp> holes;
    [SerializeField] private YarnRoll yarnRollPrefab;
    [SerializeField] private float duration = 2f;

    [Header("Drill Booster")]
    [SerializeField] private Drill drill;
    [SerializeField] private int initialActiveHoleCount = 5;
    [SerializeField] private float shiftDuration = 0.3f;
    [SerializeField] private float waveDelay = 0.06f;
    [SerializeField] private float scaleInDuration = 0.3f;

    [Header("Bloom Booster")]
    [SerializeField] private Bloom bloom;

    public float Duration => duration;

    private YarnRoll[] occupants;
    private int activeHoleCount;

    public bool IsFull
    {
        get
        {
            for (int i = 0; i < activeHoleCount; i++)
                if (occupants[i] == null) return false;
            return true;
        }
    }

    public bool HasAnyOccupant
    {
        get
        {
            for (int i = 0; i < activeHoleCount; i++)
                if (occupants[i] != null) return true;
            return false;
        }
    }

    public void Init()
    {
        occupants = new YarnRoll[holes != null ? holes.Count : 0];
        activeHoleCount = Mathf.Min(initialActiveHoleCount, occupants.Length);

        this.RegisterListener(EventID.BOOSTER_USE_REQUEST, OnUseRequest);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        this.RemoveListener(EventID.BOOSTER_USE_REQUEST, OnUseRequest);
    }

    private void OnUseRequest(object param)
    {
        var type = (BoosterType)param;
        if (type == BoosterType.Booster0) ActivateExtraHole();
        else if (type == BoosterType.Booster2 && HasAnyOccupant) bloom.Activate();
    }

    public void PrepareIntro()
    {
        for (int i = 0; i < activeHoleCount; i++)
            holes[i].PrepareIntro();
    }

    public async UniTask PlayIntro(float scaleDuration, float waveDelay)
    {
        for (int i = 0; i < activeHoleCount; i++)
        {
            int index = i;
            DOVirtual.DelayedCall(index * waveDelay, () => holes[index].ScaleIn(scaleDuration));
        }

        float total = (activeHoleCount - 1) * waveDelay + scaleDuration;
        await UniTask.Delay((int)(total * 1000));
    }

    public void ActivateExtraHole()
    {
        if (activeHoleCount >= holes.Count) return;
        if (drill == null) return;

        float spacing = holes[1].transform.position.x - holes[0].transform.position.x;
        Vector3 shift = new Vector3(-spacing / 2f, 0f, 0f);

        HoleTemp lastHole = holes[initialActiveHoleCount - 1];
        HoleTemp extraHole = holes[initialActiveHoleCount];
        float targetX = lastHole.transform.position.x + spacing / 2f;

        drill.MoveIn(targetX, () =>
        {
            for (int i = 0; i < initialActiveHoleCount; i++)
                holes[i].ShiftWave(shift, shiftDuration, i * waveDelay);

            Vector3 extraPos = extraHole.transform.position;
            extraPos.x = targetX;
            extraHole.transform.position = extraPos;

            float waveTotal = shiftDuration + (initialActiveHoleCount - 1) * waveDelay;
            DOVirtual.DelayedCall(waveTotal, () => drill.StartDrilling(() => extraHole.ScaleIn(scaleInDuration)));
        });

        activeHoleCount = holes.Count;
    }

    public bool TrySpawn(InteractableObject target)
    {
        if (target == null || yarnRollPrefab == null || holes == null || holes.Count == 0) return false;

        int idx = FindEmptySlot();
        if (idx < 0) return false;

        Transform anchor = holes[idx].Anchor;
        YarnRoll yr = Instantiate(yarnRollPrefab);
        yr.Setup(target.GetComponent<Renderer>(), target.ColorKey);
        yr.Play(duration);
        yr.PlaceInSlot(anchor);

        occupants[idx] = yr;
        holes[idx].SetOccupied(true);

        if (IsFull)
        {
            InputController.Instance.SetWaitingMode();
            if (!BoxCreator.Instance.HasAnyBoxClosing)
                GameFlow.Instance?.TriggerLose();
        }

        return true;
    }

    public void RecheckLose()
    {
        if (IsFull) GameFlow.Instance?.TriggerLose();
        else InputController.Instance.RestoreNormalMode();
    }

    private int FindEmptySlot()
    {
        for (int i = 0; i < activeHoleCount; i++)
            if (occupants[i] == null) return i;
        return -1;
    }

    public Dictionary<string, int> GetParkedColorCounts()
    {
        var result = new Dictionary<string, int>();
        for (int i = 0; i < activeHoleCount; i++)
        {
            var o = occupants[i];
            if (o == null || string.IsNullOrEmpty(o.ColorKey)) continue;
            result[o.ColorKey] = result.TryGetValue(o.ColorKey, out int c) ? c + 1 : 1;
        }
        return result;
    }

    public List<YarnRoll> TakeMatching(string key, int max)
    {
        var result = new List<YarnRoll>();
        for (int i = 0; i < activeHoleCount && result.Count < max; i++)
        {
            var r = occupants[i];
            if (r == null) continue;
            if (r.ColorKey == key)
            {
                result.Add(r);
                occupants[i] = null;
                holes[i].SetOccupied(false);
            }
        }
        return result;
    }

    public List<YarnRoll> ClearAll()
    {
        var result = new List<YarnRoll>();
        for (int i = 0; i < activeHoleCount; i++)
        {
            if (occupants[i] == null) continue;
            result.Add(occupants[i]);
            occupants[i] = null;
            holes[i].SetOccupied(false);
        }
        return result;
    }
}
