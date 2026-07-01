using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class BulletProjectile : MonoBehaviour
{
    [SerializeField] private float _speed = 20f;
    [SerializeField] private float _lifeTime = 10f;

    private Vector3 _flyDirection;
    private CancellationTokenSource _cts;

    public void Initialize(Vector3 direction)
    {
        _flyDirection = direction.normalized;
        transform.forward = _flyDirection;
        // Dispose로 메모리 누수문제 해결
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
        }
        _cts = new CancellationTokenSource();
        AutoDespawnRoutine(_cts.Token).Forget();
    }

    private void OnDisable()
    {
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }
    }

    private async UniTaskVoid AutoDespawnRoutine(CancellationToken token)
    {
        bool isCanceled = await UniTask.Delay(TimeSpan.FromSeconds(_lifeTime), cancellationToken: token).SuppressCancellationThrow();
        if (isCanceled) return; // 중간에 다른 곳에 부딪혀서 토큰이 취소되면 무시

        ReturnToPool();
    }

    // Destroy 대신 Pool로 반납하는 메서드
    private void ReturnToPool()
    {
        if (gameObject.activeInHierarchy)
        {
            GameManager.Pool.DespawnToPool(gameObject);
        }
    }

    private void Update()
    {
        float moveDistance = _speed * Time.deltaTime;
        transform.position += _flyDirection * moveDistance;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger) return;

        if (other.CompareTag("Enemy"))
        {
            return;
        }

        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.OnPlayerHit();
                Debug.Log("총알: 플레이어가 총알에 맞았습니다.");
            }

            if (other.TryGetComponent(out IDebuffable debuffTarget))
            {
                debuffTarget.ApplyDebuff(DebuffType.MoveSpeed, 0.01f, 1.5f);
                Debug.Log("[디버프] 테이저건 효과: 이동 속도 99% 감소 (1.5초)");
            }

            ReturnToPool();
            return;
        }
        ReturnToPool();
    }
}