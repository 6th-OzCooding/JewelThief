using UnityEngine;
using System;
using Cysharp.Threading.Tasks;

public class ShootEnemy : MonoBehaviour
{
    [SerializeField] private GameObject _taserEffect; // 기존에 쓰시던 이펙트 오브젝트
    [SerializeField] private float _shootDelay = 0.1f; // 총을 뻗는 애니메이션 타이밍 (켜지는 시간)
    [SerializeField] private float _effectDuration = 0.8f; // 이펙트가 터지고 유지되는 시간 (꺼지는 시간)
    [SerializeField] private Transform _firePoint; // 투사체의 생성 위치 (스크린샷의 Bullet 빈 오브젝트)

    private EnemyBase _enemyBase;

    private void Awake()
    {
        _enemyBase = GetComponent<EnemyBase>();
    }

    private void Start()
    {
        // 시작할 때 이펙트 꺼두기 (기존 로직)
        if (_taserEffect != null)
        {
            _taserEffect.SetActive(false);
        }
    }

    public void Shoot()
    {
        // 인스펙터 할당 체크 안전 장치 (기존 로직)
        if (_taserEffect == null)
        {
            Debug.LogError("ShootEnemy: 테이저 이펙트가 할당되지 않았습니다!");
            return;
        }

        if (_firePoint == null)
        {
            Debug.LogError("ShootEnemy: 발사 위치(FirePoint)가 할당되지 않았습니다!");
            return;
        }

        ShootRoutine().Forget();
    }

    private async UniTaskVoid ShootRoutine()
    {
        // 오브젝트가 파괴되면 진행 중인 대기를 취소하기 위한 토큰
        var token = this.GetCancellationTokenOnDestroy();

        try
        {
            // 잠시 기다리기 (애니메이션 타이밍)
            await UniTask.Delay(TimeSpan.FromSeconds(_shootDelay), cancellationToken: token);
            // 총알을 쏘기 직전에 멈추게 하는 코드
            await UniTask.WaitWhile(() => GameManager.Instance != null && GameManager.Instance.IsPaused, cancellationToken: token);
            // 투사체(총알) 생성 및 발사
            GameObject projectile = GameManager.Pool.SpawnFromPool("Pool_Bullet", _firePoint.position, _firePoint.rotation);
            BulletProjectile projScript = projectile.GetComponent<BulletProjectile>();

            if (projScript != null && _enemyBase != null)
            {
                Vector3 shootDirection = transform.forward;
                if (_enemyBase.TargetPlayer != null)
                {
                    Collider playerCol = _enemyBase.TargetPlayer.GetComponent<Collider>();
                    // 플레이어의 콜리더 중심점을 찾거나, 없다면 발끝에서 1만큼 올린 위치를 타겟으로 잡습니다.
                    Vector3 targetCenter = playerCol != null ? playerCol.bounds.center : _enemyBase.TargetPlayer.transform.position + Vector3.up * 1f;

                    shootDirection = (targetCenter - _firePoint.position).normalized;
                }
                projScript.Initialize(shootDirection);
            }

            if (_taserEffect != null)
            {
                _taserEffect.SetActive(true);
                var ps = _taserEffect.GetComponent<ParticleSystem>();
                if (ps != null) ps.Play(true);
            }

            // 이펙트가 재생될 시간만큼 대기
            await UniTask.Delay(TimeSpan.FromSeconds(_effectDuration), cancellationToken: token);

            // 기다린 후 이펙트 끄기
            if (_taserEffect != null)
            {
                _taserEffect.SetActive(false);
            }
        }
        catch (OperationCanceledException)
        {
            // 예외 발생 시 안전하게 이펙트 끄기
            if (_taserEffect != null)
            {
                _taserEffect.SetActive(false);
            }
        }
    }
}