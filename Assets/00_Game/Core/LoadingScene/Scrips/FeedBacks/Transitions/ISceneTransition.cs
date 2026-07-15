using Cysharp.Threading.Tasks;
using UnityEngine;

public interface ISceneTransition
{
    TransType Type { get; }
    void SetCamera(Camera cam);
    UniTask CoverAsync();
    void CoverInstant();
    UniTask RevealAsync();
}
