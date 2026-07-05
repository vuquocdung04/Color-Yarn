using UnityEngine;

public partial class YarnRoll
{
    private static readonly int DissolveId = Shader.PropertyToID("_DissolveAmount");
    private MaterialPropertyBlock _mpb;

    // B (cube): tan dan khi thao soi
    private void UpdateDissolveB(float t)
    {
        float amount = pullStart > 0f ? Mathf.Clamp01(t / pullStart) : t;
        SetDissolve(bRenderer, amount);
    }

    // A (goc cuon): hien dan khi line leo tu aBottom -> aTop
    private void UpdateDissolveA(float t)
    {
        float climb = t < reachEnd ? 0f : Mathf.Clamp01((t - reachEnd) / (1f - reachEnd));
        SetDissolve(aRenderer, 1f - climb);
    }

    private void SetDissolve(Renderer r, float amount)
    {
        if (r == null) return;
        _mpb ??= new MaterialPropertyBlock();
        r.GetPropertyBlock(_mpb);
        _mpb.SetFloat(DissolveId, Mathf.Clamp01(amount));
        r.SetPropertyBlock(_mpb);
    }
}
