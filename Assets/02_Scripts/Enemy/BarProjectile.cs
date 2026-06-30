using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class BarProjectile : MonoBehaviour
{
    [SerializeField] private float _speed = 7f;
    [SerializeField] private float _lifeTime = 3f;
    [SerializeField] private float _spinSpeed = 1000f; // 회전 속도

    private Vector3 _flyDirection;
    private CancellationTokenSource _cts;

    public void Initialize(Vector3 direction)
    {
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
        if (isCanceled) return;

        ReturnToPool();
    }

    private void ReturnToPool()
    {
        if (gameObject.activeInHierarchy)
        {
            GameManager.Pool.DespawnToPool(gameObject);
        }
    }

    private void Update()
    {
        transform.Rotate(Vector3.right * _spinSpeed * Time.deltaTime, Space.Self);

        float moveDistance = _speed * Time.deltaTime;
        transform.position += _flyDirection * moveDistance;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Debug.Log("곤봉: 적 맞고 파괴됨");
            ReturnToPool();
            return;
        }

        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.OnPlayerHit();
                Debug.Log("곤봉: 플레이어가 곤봉에 맞았습니다.");
            }
            ReturnToPool();
            return;
        }

        ReturnToPool();
    }
}