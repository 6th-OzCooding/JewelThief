using UnityEngine;
using System;
using Cysharp.Threading.Tasks;

public class ThrowEnemy : MonoBehaviour
{
    [SerializeField] private GameObject _policeBar; // 껐다 켤 경찰봉 오브젝트
    // [수정됨] Pool ID 문자열 대신 프리팹을 직접 받도록 변경
    [SerializeField] private GameObject _barProjectilePrefab;
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
        // 프리팹 할당 체크
        if (_policeBar == null || _barProjectilePrefab == null) return;

        ThrowRoutine().Forget();
    }

    private async UniTaskVoid ThrowRoutine()
    {
        var token = this.GetCancellationTokenOnDestroy();

        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(_throwDelay), cancellationToken: token);

            await UniTask.WaitWhile(() => GameManager.Instance != null && GameManager.Instance.IsPaused, cancellationToken: token);

            _policeBar.SetActive(false);

            Vector3 spawnPos = _firePoint != null ? _firePoint.position : transform.position + Vector3.up * 1f;

            // [수정됨] 풀 대신 Instantiate를 사용하여 곤봉 생성
            GameObject projectile = Instantiate(_barProjectilePrefab, spawnPos, transform.rotation);

            BarProjectile projScript = projectile.GetComponent<BarProjectile>();
            if (projScript != null && _enemyBase != null)
            {
                projScript.Initialize(transform.forward, _enemyBase.AttackDamage);
            }

            await UniTask.Delay(TimeSpan.FromSeconds(_respawnTime), cancellationToken: token);

            _policeBar.SetActive(true);
        }
        catch (OperationCanceledException)
        {
            Debug.Log("던지는 행동 종료");
        }
    }
}