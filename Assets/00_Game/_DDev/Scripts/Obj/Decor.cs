using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class Decor : MonoBehaviour
{
    [Header("Fake Physics")]
    [SerializeField] private LayerMask groundMask = ~0;
    [SerializeField] private float impulseSpeed = 1.5f;
    [SerializeField] private float gravity = 9.81f;
    [SerializeField, Range(0f, 20f)] private float friction = 5f;
    [SerializeField] private float lifeTime = 4f;

    private InteractableObject parentShell;
    private Vector3 velocity;
    private float radius;

    private void Awake()
    {
        parentShell = GetComponentInParent<InteractableObject>();
        if (parentShell != null)
            parentShell.OnDissolveStart += HandleDissolveStart;

        var rend = GetComponentInChildren<Renderer>();
        Vector3 e = rend != null ? rend.bounds.extents : Vector3.one * 0.05f;
        radius = (e.x + e.y + e.z) / 3f;
    }

    private void OnDestroy()
    {
        if (parentShell != null)
            parentShell.OnDissolveStart -= HandleDissolveStart;
    }

    private void HandleDissolveStart(float dur)
    {
        parentShell.OnDissolveStart -= HandleDissolveStart;

        Vector3 outward = transform.position - parentShell.transform.position;
        outward = outward.sqrMagnitude > 1e-6f ? outward.normalized : Vector3.up;

        FallRoutine(dur * PercentFromTop, outward, this.GetCancellationTokenOnDestroy()).Forget();
    }

    private async UniTaskVoid FallRoutine(float delay, Vector3 outward, CancellationToken token)
    {
        if (delay > 0f)
            await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: token);

        transform.SetParent(null, true);
        velocity = outward * impulseSpeed;

        float remaining = Mathf.Max(0f, lifeTime - delay);
        float t = 0f;
        while (t < remaining)
        {
            Step(Time.deltaTime);
            t += Time.deltaTime;
            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }

        Destroy(gameObject);
    }

    private void Step(float dt)
    {
        velocity += Vector3.down * gravity * dt;

        Vector3 move = velocity * dt;
        float dist = move.magnitude;
        Vector3 dir = dist > 1e-5f ? move / dist : Vector3.down;

        if (Physics.Raycast(transform.position, dir, out RaycastHit hit, dist + radius, groundMask, QueryTriggerInteraction.Ignore))
        {
            transform.position = hit.point + hit.normal * radius;
            velocity = Vector3.ProjectOnPlane(velocity, hit.normal);
            velocity *= Mathf.Clamp01(1f - friction * dt);
        }
        else
        {
            transform.position += move;
        }
    }

    public float PercentFromTop
    {
        get
        {
            if (parentShell == null) return 0f;

            Bounds localBounds = GetLocalBounds(parentShell);
            float localY = parentShell.transform.InverseTransformPoint(transform.position).y;
            float vertY = Mathf.InverseLerp(localBounds.min.y, localBounds.max.y, localY);
            return 1f - vertY;
        }
    }

    private static Bounds GetLocalBounds(InteractableObject obj)
    {
        var mf = obj.GetComponent<MeshFilter>();
        return mf != null && mf.sharedMesh != null ? mf.sharedMesh.bounds : new Bounds(Vector3.zero, Vector3.one);
    }

    private void OnDrawGizmos()
    {
        if (parentShell != null)
        {
            Bounds localBounds = GetLocalBounds(parentShell);
            Gizmos.color = Color.green;
            Gizmos.matrix = parentShell.transform.localToWorldMatrix;
            Gizmos.DrawWireCube(localBounds.center, localBounds.size);
            Gizmos.matrix = Matrix4x4.identity;
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, 0.03f);

#if UNITY_EDITOR
        UnityEditor.Handles.color = Color.yellow;
#endif
    }
}
