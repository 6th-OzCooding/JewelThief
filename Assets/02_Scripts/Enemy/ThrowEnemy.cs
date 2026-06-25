using UnityEngine;
using System;
using Cysharp.Threading.Tasks;

public class ThrowEnemy : MonoBehaviour
{
    [SerializeField] private GameObject _policeBar; // 껐다 켤 경찰봉 오브젝트
    [SerializeField] private string _barProjectilePoolId = "Pool_Throw"; // 던질 투사체 프리팹 pool Manager에서 가져오기
    [SerializeField] private Transform _firePoint; // 투사체의 생성 위치

    [SerializeField] private float _throwDelay = 0.1f;
    [SerializeField] private float _respawnTime = 3f;

    private EnemyBase _enemyBase;
    private void Awake()
    {
        // 곤봉을 던지는 캐릭터의 공격력을 가져온다.
        _enemyBase = GetComponent<EnemyBase>();
    }

    public void ThrowWeapon()
    {
        // 곤봉이 할당되어 있지 않거나 투사체 프리팹이 할당되어 있지 않은 경우 중지
        if (_policeBar == null || _barProjectilePoolId == null) return;

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
            // 던질 투사체 생성
            Vector3 spawnPos = _firePoint != null ? _firePoint.position : transform.position + Vector3.up * 1f;
            // PoolManager에서 곤봉 데이터 가져오기
            GameObject projectile = GameManager.Pool.SpawnFromPool(_barProjectilePoolId, spawnPos, transform.rotation);

            // 앞으로 던져지는 것과 데미지 설정
            BarProjectile projScript = projectile.GetComponent<BarProjectile>();
            if (projScript != null && _enemyBase != null)
            {
                projScript.Initialize(transform.forward, _enemyBase.AttackDamage);
            }

            // 공격 대기 (쿨타임)
            await UniTask.Delay(TimeSpan.FromSeconds(_respawnTime), cancellationToken: token);

            // 곤봉 다시 활성화
            _policeBar.SetActive(true);
        }
        catch (OperationCanceledException)
        {
            // 예외상황에서 토큰 반환
            Debug.Log("던지는 행동 종료");
        }
    }
}