using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class BarProjectile : MonoBehaviour
{
    [SerializeField] private float _speed = 7f;
    [SerializeField] private float _lifeTime = 3f;
    [SerializeField] private float _spinSpeed = 1000f; // 회전 속도

    private float _damage;
    private Vector3 _flyDirection;
    private CancellationTokenSource _cts;

    public void Initialize(Vector3 direction, float damage)
    {
        _damage = damage;
        _flyDirection = direction.normalized;

        transform.forward = _flyDirection;
        // Dispose로 메모리 누수 문제 해결
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }
        _cts = new CancellationTokenSource();
        AutoDespawnRoutine(_cts.Token).Forget();
    }

    private async UniTaskVoid AutoDespawnRoutine(CancellationToken token)
    {
        bool isCanceled = await UniTask.Delay(TimeSpan.FromSeconds(_lifeTime), cancellationToken: token).SuppressCancellationThrow();
        if (isCanceled) return;

        ReturnToPool();
    }

    private void ReturnToPool()
    {
        // Dispose를 사용하여 메모리 누수 문제 해결
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
        transform.Rotate(Vector3.right * _spinSpeed * Time.deltaTime, Space.Self);
        float moveDistance = _speed * Time.deltaTime;

        if (Physics.Raycast(transform.position, _flyDirection, out RaycastHit hit, moveDistance))
        {
            if (hit.collider.CompareTag("Enemy"))
            {
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
                    Debug.Log($"플레이어에게 곤봉 적중! 데미지: {_damage}");
                }
                ReturnToPool();
                return;
            }
            else
            {
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
                Debug.Log("플레이어가 곤봉에 맞았습니다.");
            }
        }
        ReturnToPool();
    }
}