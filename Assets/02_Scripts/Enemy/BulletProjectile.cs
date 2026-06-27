using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class BulletProjectile : MonoBehaviour
{
    [SerializeField] private float _speed = 20f;
    [SerializeField] private float _lifeTime = 2f;

    private float _damage;
    private Vector3 _flyDirection;
    private CancellationTokenSource _cts;

    public void Initialize(Vector3 direction, float damage)
    {
        _damage = damage;
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

    private async UniTaskVoid AutoDespawnRoutine(CancellationToken token)
    {
        bool isCanceled = await UniTask.Delay(TimeSpan.FromSeconds(_lifeTime), cancellationToken: token).SuppressCancellationThrow();
        if (isCanceled) return; // 중간에 다른 곳에 부딪혀서 토큰이 취소되면 무시

        ReturnToPool();
    }
    // Destroy 대신 Pool로 반납하는 메서드
    private void ReturnToPool()
    {
        // Dispose로 메모리 누수 문제 해결
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }
        if (gameObject.activeInHierarchy)
        {
            GameManager.Pool.DespawnToPool(gameObject);
        }
    }

    private void Update()
    {
        float moveDistance = _speed * Time.deltaTime;

        if (Physics.Raycast(transform.position, _flyDirection, out RaycastHit hit, moveDistance))
        {
            if (hit.collider.CompareTag("Enemy"))
            {
                // 적이랑 부딪히면 바로 반납
                Debug.Log("적 맞고 파괴됨");
                ReturnToPool();
                return;
            }
            else if (hit.collider.CompareTag("Player"))
            {
                PlayerController player = hit.collider.GetComponent<PlayerController>();
                if (player != null)
                {
                    player.TakePlayerSpDamage(_damage);
                    // 데미지를 받았는 지 확인
                    Debug.Log($"플레이어에게 총알 적중! 데미지: {_damage}");
                }
                ReturnToPool();
                return;
            }
            else
            {
                // 벽이나 물체에 맞으면 바로 파괴되게
                Debug.Log("벽 맞고 파괴됨");
                ReturnToPool();
                return;
            }
        }

        transform.position += _flyDirection * moveDistance;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy")) return;

        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakePlayerSpDamage(_damage);
                Debug.Log("플레이어가 총알에 맞았습니다.");
            }
        }
        ReturnToPool();
    }
}