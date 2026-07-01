using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using Sirenix.OdinInspector;

public class LineController : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public Transform targetB;
    public Transform startPoint;
    public Transform endPoint;
    public float durationThread = 1f;
    public int lineResolution = 10; 

    private CancellationTokenSource _cts;

    [Button]
    public void PlayEffect()
    {
        ResetEffect();
        _cts = new CancellationTokenSource();

        var combinedToken = CancellationTokenSource.CreateLinkedTokenSource(
            _cts.Token, 
            this.GetCancellationTokenOnDestroy()
        ).Token;

        RunLineLogicAsync(combinedToken).Forget();
    }

    private void ResetEffect()
    {
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }

        lineRenderer.positionCount = lineResolution;

        if (startPoint != null)
        {
            UpdateLinePositions(startPoint.position);
        }
    }

    private async UniTaskVoid RunLineLogicAsync(CancellationToken token)
    {
        float timer = 0f;

        while (!token.IsCancellationRequested)
        {
            Vector3 currentEndPos = Vector3.zero;

            if (timer < durationThread)
            {
                timer += Time.deltaTime;
                float t = Mathf.Clamp01(timer / durationThread);

                if (startPoint != null && endPoint != null)
                {
                    currentEndPos = Vector3.Lerp(startPoint.position, endPoint.position, t);
                }
            }
            else
            {
                if (endPoint != null)
                {
                    currentEndPos = endPoint.position;
                }
            }

            UpdateLinePositions(currentEndPos);

            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }
    }

    private void UpdateLinePositions(Vector3 endPos)
    {
        if (targetB == null) return;

        for (int i = 0; i < lineResolution; i++)
        {
            float t = i / (float)(lineResolution - 1);
            Vector3 pointPos = Vector3.Lerp(targetB.position, endPos, t);
            lineRenderer.SetPosition(i, pointPos);
        }
    }
}