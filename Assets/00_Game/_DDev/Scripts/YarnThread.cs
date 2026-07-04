using UnityEngine;
using Sirenix.OdinInspector;

[RequireComponent(typeof(LineRenderer))]
public class YarnThread : MonoBehaviour
{
    public enum Axis { X, Y, Z }

    [Header("A (goc cuon)")]
    [SerializeField] private Transform aTop;
    [SerializeField] private Transform aBottom;

    [Header("B (cube)")]
    [SerializeField] private Transform bObject;
    [SerializeField] private Renderer  bRenderer;
    [SerializeField] private Axis       bAxis = Axis.Y;

    [Header("Quan")]
    [SerializeField] private float turns = 3f;
    [SerializeField] private float inset = 0f;

    [Header("Line")]
    [SerializeField, Min(2)] private int subdivisions = 24;

    [Header("Anim")]
    [SerializeField] private float duration = 2f;
    [SerializeField, Range(0.05f, 0.6f)] private float reachEnd = 0.35f;
    [SerializeField, Range(0.6f, 0.97f)] private float pullStart = 0.85f;

    private LineRenderer line;
    private float elapsed;
    private bool  running;

    void Awake()
    {
        line = GetComponent<LineRenderer>();
        EnsureRefs();
        UpdateLine(0f);
    }

    void Update()
    {
        if (!running) return;
        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / duration);
        UpdateLine(t);
        if (t >= 1f) running = false;
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
    }

    void SetStraightLine(Vector3 from, Vector3 to)
    {
        line.positionCount = subdivisions;
        for (int i = 0; i < subdivisions; i++)
        {
            float f = i / (float)(subdivisions - 1);
            line.SetPosition(i, Vector3.Lerp(from, to, f));
        }
    }

    Vector3 GetPointOnBound(float t)
    {
        Bounds bnd = bRenderer.bounds;
        float minX = bnd.min.x + inset, maxX = bnd.max.x - inset;
        float minZ = bnd.min.z + inset, maxZ = bnd.max.z - inset;

        Vector3 top = AxisExtreme(bnd, bAxis, true);
        Vector3 bot = AxisExtreme(bnd, bAxis, false);
        float y = Mathf.Lerp(top.y, bot.y, t);

        float u = (t * turns) % 1f;
        Vector3 xz = PerimeterPoint(u, minX, maxX, minZ, maxZ);

        return new Vector3(xz.x, y, xz.z);
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

    Vector3 AxisExtreme(Bounds b, Axis axis, bool max)
    {
        Vector3 p = b.center;
        switch (axis)
        {
            case Axis.X: p.x = max ? b.max.x : b.min.x; break;
            case Axis.Y: p.y = max ? b.max.y : b.min.y; break;
            case Axis.Z: p.z = max ? b.max.z : b.min.z; break;
        }
        return p;
    }

    void EnsureRefs()
    {
        if (line == null) line = GetComponent<LineRenderer>();
        if (bRenderer == null && bObject != null)
            bRenderer = bObject.GetComponentInChildren<Renderer>();
    }

    [Button("Play Wrap", ButtonSizes.Large), GUIColor(0.4f, 1f, 0.5f)]
    void Btn_Play()
    {
        EnsureRefs();
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

    void Scrub()
    {
        EnsureRefs();
        UpdateLine(scrub);
    }
}