using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
public interface IDebuffable
{
    void ApplyDebuff(DebuffType type,float debuffValue, float duration);
}
public enum DebuffType 
{
    None=0,
    MoveSpeed,
    Weight

}
public interface IStatModifiable
{
    // 어떤 타입의 스탯 배율을 몇으로 바꿀지 매개변수로 전달.
    void SetStatMultiplier(DebuffType type, float value);
    void ResetStatMultiplier(DebuffType type);
}

public class PlayerDebuffHandler : MonoBehaviour, IDebuffable
{
    private IStatModifiable statTarget;
   

    private Dictionary<DebuffType, CancellationTokenSource> debuffTokens = new Dictionary<DebuffType, CancellationTokenSource>();
    void Awake()
    {
        statTarget = GetComponent<IStatModifiable>();
    }

    public void ApplyDebuff(DebuffType type, float debuffValue, float duration)
    {
        //if (statTarget == null) return;
        if (debuffTokens.TryGetValue(type, out var existingCts))
        {
            // 같은 트랩을 또 밟은 경우이므로, 기존 해당 타이머만 취소합니다.
            // (예: 이동속도 저하 걸린 상태에서 또 이동속도 저하 맞으면 타이머 리셋)
            existingCts?.Cancel();
            existingCts?.Dispose();
        }

        
        var newCts = new CancellationTokenSource();
        debuffTokens[type] = newCts; 

        StatDebuffRoutine(type, debuffValue, duration, newCts.Token).Forget();
    }

    private async UniTaskVoid StatDebuffRoutine(DebuffType type, float debuffValue, float duration, CancellationToken token)
    {
        try
        {
            Debug.Log($"디버프시작{type}");
            statTarget.SetStatMultiplier(type, debuffValue);
            //플레이어의 인터페이스에 있는 디버프 적용 함수 (임시)
            await UniTask.Delay(System.TimeSpan.FromSeconds(duration), cancellationToken: token);

            
            Debug.Log($"디버프끝{type}");
            statTarget.ResetStatMultiplier(type);
            if (debuffTokens.TryGetValue(type, out var currentCts) && currentCts.Token == token)
            {
                debuffTokens.Remove(type);
            }
        }
        catch (System.OperationCanceledException)
        {
            // 중첩 시 해제 안 함 (새로운 타이머가 제어권을 가져감)
        }
    }
}
