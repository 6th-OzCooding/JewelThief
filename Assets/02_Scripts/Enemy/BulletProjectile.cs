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

    public void Initialize(Vector3 direction, float damage)
    {
        _damage = damage;
        _flyDirection = direction.normalized;
        transform.forward = _flyDirection;

        AutoDestroyRoutine(this.GetCancellationTokenOnDestroy()).Forget();
    }

    private async UniTaskVoid AutoDestroyRoutine(CancellationToken token)
    {
        bool isCanceled = await UniTask.Delay(TimeSpan.FromSeconds(_lifeTime), cancellationToken: token).SuppressCancellationThrow();
        if (isCanceled) return;

        if (gameObject != null)
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        float moveDistance = _speed * Time.deltaTime;

        if (Physics.Raycast(transform.position, _flyDirection, out RaycastHit hit, moveDistance))
        {
            if (hit.collider.CompareTag("Enemy"))
            {
                // 적이랑 부딪히면 바로 파괴되게
                Debug.Log("적 맞고 파괴됨");
                Destroy(gameObject);
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
                Destroy(gameObject);
                return;
            }
            else
            {
                // 벽이나 물체에 맞으면 바로 파괴되게
                Debug.Log("벽 맞고 파괴됨");
                Destroy(gameObject);
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

        Destroy(gameObject);
    }
}