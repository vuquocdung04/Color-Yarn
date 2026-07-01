using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class YarnLine : MonoBehaviour
{
    [Header("References")]
    public Transform cube;
    public Transform target;

    [Header("Loop")]
    public float speed = 2f;
    public float heightOffset = 0f;
    public float inset = 0f;

    [Header("State")]
    public bool playing = false;

    private LineRenderer _lr;
    private float _t;
    private readonly Vector3[] _pts = new Vector3[2];

    void Awake()
    {
        _lr = GetComponent<LineRenderer>();
        _lr.positionCount = 2;
    }

    [Button("Play")]
    public void Play()
    {
        if (_lr == null) _lr = GetComponent<LineRenderer>();
        _lr.positionCount = 2;
        _t = 0f;
        playing = true;
    }

    [Button("Stop")]
    public void Stop()
    {
        playing = false;
        if (_lr != null) _lr.positionCount = 0;
    }

    void Update()
    {
        if (!playing || cube == null || target == null) return;

        _t += Time.deltaTime * speed;
        float u = Mathf.Repeat(_t, 1f);

        _pts[0] = target.position;
        _pts[1] = PointOnBounds(u);
        _lr.SetPositions(_pts);
    }

    private Vector3 PointOnBounds(float u)
    {
        Bounds b = cube.GetComponent<Renderer>().bounds;
        Vector3 c = b.center;
        float ex = b.extents.x - inset;
        float ez = b.extents.z - inset;
        float y = c.y + heightOffset;

        Vector3 c0 = new Vector3(c.x - ex, y, c.z - ez);
        Vector3 c1 = new Vector3(c.x + ex, y, c.z - ez);
        Vector3 c2 = new Vector3(c.x + ex, y, c.z + ez);
        Vector3 c3 = new Vector3(c.x - ex, y, c.z + ez);

        float seg = u * 4f;
        int i = (int)seg;
        float f = seg - i;

        switch (i)
        {
            case 0: return Vector3.Lerp(c0, c1, f);
            case 1: return Vector3.Lerp(c1, c2, f);
            case 2: return Vector3.Lerp(c2, c3, f);
            default: return Vector3.Lerp(c3, c0, f);
        }
    }
}