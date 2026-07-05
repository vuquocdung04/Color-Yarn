using UnityEngine;
using Sirenix.OdinInspector;

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

    [Header("Anim")]
    private float duration = 2f;
    [SerializeField, Range(0.05f, 0.6f)] private float reachEnd = 0.35f;
    [SerializeField, Range(0.6f, 0.97f)] private float pullStart = 0.85f;

    // B set luc runtime qua Setup (khong keo tay), chi dung de raycast bam soi
    private Transform bObject;
    private Renderer  bRenderer;

    private float elapsed;
    private bool  running;

    // ================= PUBLIC API =================
    public void Setup(Renderer targetB, string colorKey)
    {
        bRenderer = targetB;
        bObject   = targetB != null ? targetB.transform : null;

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

    public void SetLineMaterial(Material m)
    {
        if (line != null) line.material = m;
    }

    public void SetColor(Color c)
    {
        if (line != null)
        {
            line.startColor = c;
            line.endColor   = c;
            ApplyColor(line.material, c);
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

    // ================= LOOP =================
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
        for (int i = 0; i < subdivisions; i++)
        {
            float f = i / (float)(subdivisions - 1);
            line.SetPosition(i, Vector3.Lerp(from, to, f));
        }
    }

    static readonly RaycastHit[] _hitBuf = new RaycastHit[32];

    Vector3 _lastBPoint;
    bool    _hasLastB;

    Vector3 GetPointOnBound(float t)
    {
        // B da bi destroy -> giu diem cuoi cung, khong nhay ve aTop
        if (bRenderer == null)
            return _hasLastB ? _lastBPoint : (aTop != null ? aTop.position : transform.position);

        Bounds bnd = bRenderer.bounds;
        Vector3 center = bnd.center;
        float y = Mathf.Lerp(bnd.max.y, bnd.min.y, t);

        float u   = (t * turns) % 1f;
        float ang = u * Mathf.PI * 2f;
        Vector3 dir = new Vector3(Mathf.Cos(ang), 0f, Mathf.Sin(ang));

        float maxR = Mathf.Max(bnd.extents.x, bnd.extents.z) * 2f + 0.01f;
        Vector3 axisP  = new Vector3(center.x, y, center.z);
        Vector3 origin = axisP + dir * maxR;

        Vector3 result;
        // ban tia tu ngoai vao truc -> be mat ngoai cung cua B
        if (RaycastSurface(origin, -dir, maxR, out Vector3 surf, out Vector3 nrm))
            result = surf - nrm * inset; // inset > 0: lun vao trong chut
        else
        {
            // fallback: chu vi AABB neu tia truot
            Vector3 xz = PerimeterPoint(u, bnd.min.x, bnd.max.x, bnd.min.z, bnd.max.z);
            result = new Vector3(xz.x, y, xz.z);
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

    // ================= EDITOR TEST =================
    [Button("Play Wrap", ButtonSizes.Large), GUIColor(0.4f, 1f, 0.5f)]
    void Btn_Play()
    {
        elapsed = 0f;
        running = true;
    }

    [Button("Reset"), GUIColor(1f, 0.8f, 0.4f)]
    void Btn_Reset()
    {
        running = false;
        elapsed = 0f;
        UpdateLine(0f);
    }

    [PropertyRange(0f, 1f), OnValueChanged(nameof(Scrub)), ShowInInspector]
    private float scrub = 0f;

    void Scrub() => UpdateLine(scrub);
}
