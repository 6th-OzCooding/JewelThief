using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
public interface IDebuffable
{
    void ApplySlowDebuff(float slowRatio, float duration);
}

public class PlayerDebuffHandler : MonoBehaviour, IDebuffable
{
    private PlayerController playerController;
    private CancellationTokenSource cts;
    void Awake()
    {
        playerController = GetComponent<PlayerController>();
    }

    public void ApplySlowDebuff(float slowRatio, float duration)
    {
        cts?.Cancel();
        cts = new CancellationTokenSource();

        SlowRoutine(slowRatio, duration, cts.Token).Forget();
    }

    private async UniTaskVoid SlowRoutine(float slowRatio, float duration, CancellationToken token)
    {
        try
        {
            // 플레이어의 이동 속도 배율을 낮춥니다 (예: 50% 디버프면 0.5f로 변경)
            //플레이어 컨트롤러에 SpeedMultiplier 필요
            // playerController.SpeedMultiplier = 1f - slowRatio;

            await UniTask.Delay(System.TimeSpan.FromSeconds(duration), cancellationToken: token);

            
            Debug.Log("디버프끝");
            //playerController.SpeedMultiplier = 1f;
        }
        catch (System.OperationCanceledException)
        {
            // 중첩 시 해제 안 함 (새로운 타이머가 제어권을 가져감)
        }
    }
}
