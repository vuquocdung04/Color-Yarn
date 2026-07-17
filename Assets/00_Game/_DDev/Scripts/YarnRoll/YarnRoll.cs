using DG.Tweening;
using UnityEngine;

public partial class YarnRoll : MonoBehaviour
{
    [Header("A (goc cuon)")]
    [SerializeField] private Transform aTop;
    [SerializeField] private Transform aBottom;
    [SerializeField] private Renderer  aRenderer;

    [Header("Quan")]
    [SerializeField] private float turns = 3f;
    [SerializeField] private float inset = 0f;

    [Header("Line")]
    [SerializeField] private LineRenderer line;
    [SerializeField, Min(2)] private int subdivisions = 24;

    [Header("Line - Wave")]
    [SerializeField] private float waveAmplitude = 0.05f;
    [SerializeField] private float waveCount = 2f;
    [SerializeField] private float waveSpeed = 3f;

    [Header("Anim")]
    private float duration = 2f;
    [SerializeField, Range(0.05f, 0.6f)] private float reachEnd = 0.35f;
    [SerializeField, Range(0.6f, 0.97f)] private float pullStart = 0.85f;

    [Header("Drop")]
    [SerializeField] private float dropStartY = 0.6f;
    [SerializeField] private float dropDuration = 0.2f;
    [SerializeField] private float dropWobbleAngle = 7f;

    private Transform bObject;
    private Renderer  bRenderer;

    private float elapsed;
    private bool  running;

    public string ColorKey { get; private set; }

    public void Setup(Renderer targetB, string colorKey)
    {
        bRenderer = targetB;
        bObject   = targetB != null ? targetB.transform : null;
        ColorKey  = colorKey;

        var entry = ColorRepo.Instance != null ? ColorRepo.Instance.GetSet(colorKey) : null;
        if (entry != null) SetColor(GetColorFromMaterial(entry.material));
    }

    private static Color GetColorFromMaterial(Material m)
    {
        if (m == null) return Color.white;
        if (m.HasProperty("_BaseColor")) return m.GetColor("_BaseColor");
        return m.color;
    }

    public void Play(float dur)
    {
        duration = dur;
        elapsed = 0f;
        running = true;
    }

    public void ShowCompleted() => UpdateLine(1f);

    private System.Action pendingDropCallback;

    public void PlaceInSlot(Transform slot, System.Action onDropComplete = null)
    {
        transform.SetParent(slot);
        transform.localPosition = new Vector3(0f, dropStartY, 0f);
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;

        if (running)
        {
            pendingDropCallback = onDropComplete;
            Finished += OnReadyToDrop;
        }
        else
        {
            PlayDropAnimation(onDropComplete);
        }
    }

    private void OnReadyToDrop()
    {
        Finished -= OnReadyToDrop;
        PlayDropAnimation(pendingDropCallback);
        pendingDropCallback = null;
    }

    private void PlayDropAnimation(System.Action onComplete)
    {
        transform.DOLocalMoveY(0f, dropDuration).OnComplete(() => onComplete?.Invoke());
        transform.DOPunchRotation(new Vector3(0f, 0f, dropWobbleAngle), dropDuration);
    }

    public void SetColor(Color c)
    {
        if (line != null)
        {
            line.startColor = c;
            line.endColor   = c;
        }

        if (aRenderer != null)
            ApplyColor(aRenderer.material, c);
    }

    private static void ApplyColor(Material m, Color c)
    {
        if (m == null) return;
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        else if (m.HasProperty("_Color")) m.SetColor("_Color", c);
        else m.color = c;
    }

    public event System.Action Finished;

    void Awake() => UpdateLine(0f);

    void Update()
    {
        if (!running) return;
        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / duration);
        UpdateLine(t);
        if (t >= 1f)
        {
            running = false;
            Finished?.Invoke();
        }
    }

    void UpdateLine(float t)
    {
        Vector3 headA;
        if (t < reachEnd)
        {
            float k = t / reachEnd;
            headA = Vector3.Lerp(GetPointOnBound(0f), aBottom.position, k);
        }
        else
        {
            float k = Mathf.Clamp01((t - reachEnd) / (1f - reachEnd));
            headA = Vector3.Lerp(aBottom.position, aTop.position, k);
        }

        Vector3 headB;
        if (t < pullStart)
        {
            headB = GetPointOnBound(t / pullStart);
        }
        else
        {
            float k = (t - pullStart) / (1f - pullStart);
            headB = Vector3.Lerp(GetPointOnBound(1f), aTop.position, k);
        }

        SetStraightLine(headA, headB);
        UpdateDissolveA(t);
    }

    void SetStraightLine(Vector3 from, Vector3 to)
    {
        if (line == null) return;
        line.positionCount = subdivisions;

        Vector3 dir = to - from;
        float length = dir.magnitude;
        Vector3 axis = length > 1e-5f ? dir / length : Vector3.forward;
        Vector3 perp = Vector3.Cross(axis, Vector3.up);
        if (perp.sqrMagnitude < 1e-6f) perp = Vector3.Cross(axis, Vector3.right);
        perp.Normalize();

        float lengthFactor = Mathf.Clamp01(length);

        for (int i = 0; i < subdivisions; i++)
        {
            float f = i / (float)(subdivisions - 1);
            Vector3 point = Vector3.Lerp(from, to, f);

            float envelope = Mathf.Sin(Mathf.PI * f);
            float wave = Mathf.Sin(f * Mathf.PI * 2f * waveCount + Time.time * waveSpeed);

            point += perp * (wave * envelope * waveAmplitude * lengthFactor);
            line.SetPosition(i, point);
        }
    }

    static readonly RaycastHit[] _hitBuf = new RaycastHit[32];

    Vector3 _lastBPoint;
    bool    _hasLastB;

    static Bounds GetLocalBounds(Renderer r)
    {
        if (r.TryGetComponent(out MeshFilter mf) && mf.sharedMesh != null)
            return mf.sharedMesh.bounds;

        if (r is SkinnedMeshRenderer smr)
            return smr.localBounds;

        return r.bounds;
    }

    Vector3 GetPointOnBound(float t)
    {
        if (bRenderer == null || bObject == null)
            return _hasLastB ? _lastBPoint : (aTop != null ? aTop.position : transform.position);

        Bounds bnd = GetLocalBounds(bRenderer);
        Vector3 center = bnd.center;
        float y = Mathf.Lerp(bnd.max.y, bnd.min.y, t);

        float u   = (t * turns) % 1f;
        float ang = u * Mathf.PI * 2f;
        Vector3 localDir = new Vector3(Mathf.Cos(ang), 0f, Mathf.Sin(ang));

        float maxR = Mathf.Max(bnd.extents.x, bnd.extents.z) * 2f + 0.01f;
        Vector3 localAxisP = new Vector3(center.x, y, center.z);
        Vector3 localOrigin = localAxisP + localDir * maxR;

        Vector3 worldOrigin = bObject.TransformPoint(localOrigin);
        Vector3 worldDir = bObject.TransformDirection(-localDir).normalized;
        float worldMaxR = maxR * Mathf.Max(bObject.lossyScale.x, Mathf.Max(bObject.lossyScale.y, bObject.lossyScale.z));

        Vector3 result;
        if (RaycastSurface(worldOrigin, worldDir, worldMaxR, out Vector3 surf, out Vector3 nrm))
            result = surf - nrm * inset;
        else
        {
            Vector3 xz = PerimeterPoint(u, bnd.min.x, bnd.max.x, bnd.min.z, bnd.max.z);
            result = bObject.TransformPoint(new Vector3(xz.x, y, xz.z));
        }

        _lastBPoint = result;
        _hasLastB = true;
        return result;
    }

    bool RaycastSurface(Vector3 origin, Vector3 dir, float dist, out Vector3 point, out Vector3 normal)
    {
        point = default; normal = Vector3.up;
        int n = Physics.RaycastNonAlloc(origin, dir, _hitBuf, dist);
        float best = float.MaxValue;
        bool found = false;
        for (int i = 0; i < n; i++)
        {
            var h = _hitBuf[i];
            if (bObject != null && !h.collider.transform.IsChildOf(bObject)) continue;
            if (h.distance < best)
            {
                best = h.distance;
                point = h.point;
                normal = h.normal;
                found = true;
            }
        }
        return found;
    }

    Vector3 PerimeterPoint(float u, float minX, float maxX, float minZ, float maxZ)
    {
        float seg = u * 4f;
        int side = Mathf.FloorToInt(seg) % 4;
        float f = seg - Mathf.Floor(seg);

        switch (side)
        {
            case 0:  return new Vector3(Mathf.Lerp(minX, maxX, f), 0, minZ);
            case 1:  return new Vector3(maxX, 0, Mathf.Lerp(minZ, maxZ, f));
            case 2:  return new Vector3(Mathf.Lerp(maxX, minX, f), 0, maxZ);
            default: return new Vector3(minX, 0, Mathf.Lerp(maxZ, minZ, f));
        }
    }
}
