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

    public void Initialize(Vector3 direction, float damage)
    {
        _damage = damage;
        _flyDirection = direction.normalized;

        transform.forward = _flyDirection;

        // [수정됨] 풀링용 토큰 대신 파괴 시 자동 취소되는 토큰 사용
        AutoDestroyRoutine(this.GetCancellationTokenOnDestroy()).Forget();
    }

    private async UniTaskVoid AutoDestroyRoutine(CancellationToken token)
    {
        bool isCanceled = await UniTask.Delay(TimeSpan.FromSeconds(_lifeTime), cancellationToken: token).SuppressCancellationThrow();
        if (isCanceled) return;

        if (gameObject != null)
        {
            Destroy(gameObject); // [수정됨] Destroy 사용
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
                Destroy(gameObject); // [수정됨]
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
                Destroy(gameObject); // [수정됨]
                return;
            }
            else
            {
                Debug.Log("벽 맞고 파괴됨");
                Destroy(gameObject); // [수정됨]
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

        Destroy(gameObject);
    }
}