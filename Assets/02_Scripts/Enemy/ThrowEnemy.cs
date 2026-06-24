using UnityEngine;
using System;
using Cysharp.Threading.Tasks;

public class ThrowEnemy : MonoBehaviour
{
    [SerializeField] private GameObject _policeBar; // 껐다 켤 경찰봉 오브젝트

    [SerializeField] private float _throwDelay = 0.1f;
    [SerializeField] private float _respawnTime = 1.5f;

    public void ThrowWeapon()
    {
        // 곤봉이 할당되어 있지 않으면 에러를 띄우고 중지
        if (_policeBar == null) return;

        ThrowRoutine().Forget();
    }

    private async UniTaskVoid ThrowRoutine()
    {
        // 오브젝트가 파괴되면 진행 중인 대기를 취소하기 위한 토큰
        var token = this.GetCancellationTokenOnDestroy();

        try
        {
            // 잠시 기다리기
            await UniTask.Delay(TimeSpan.FromSeconds(_throwDelay), cancellationToken: token);

            // 기다린 후 곤봉 비활성화
            _policeBar.SetActive(false);
            Debug.Log("곤봉을 비활성화 합니다!");

            // 1초 정도 대기
            await UniTask.Delay(TimeSpan.FromSeconds(_respawnTime), cancellationToken: token);

            // 곤봉 다시 활성화
            _policeBar.SetActive(true);
            Debug.Log("곤봉이 다시 나타났습니다!");
        }
        catch (OperationCanceledException)
        {
            // 예외상황에서 토큰 반환
            Debug.Log("경찰봉 던지기 루틴 취소");
        }
    }
}