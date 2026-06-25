using UnityEngine;

public class BarProjectile : MonoBehaviour
{
    [SerializeField] private float _speed = 7f;
    [SerializeField] private float _lifeTime = 3f;

    private float _damage;

    public void Initialize(Vector3 direction, float damage)
    {
        _damage = damage;
        transform.forward = direction;

        Destroy(gameObject, _lifeTime);
    }

    private void Update()
    {
        float moveDistance = _speed * Time.deltaTime;
        Vector3 nextPosition = transform.position + transform.forward * moveDistance;

        // 이동하기 전에 현재 위치에서 앞방향으로 이동할 거리(moveDistance)만큼 레이저를 쏴서 막히는 게 있는지 검사합니다.
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, moveDistance))
        {
            // 부딪힌 대상이 적(자신이나 동료)이라면 무시
            if (hit.collider.CompareTag("Enemy"))
            {
                Debug.Log("적 맞고 파괴됨");
                Destroy(gameObject);
                return;
            }
            // 부딪힌 대상이 플레이어라면 데미지!
            else if (hit.collider.CompareTag("Player"))
            {
                PlayerController player = hit.collider.GetComponent<PlayerController>();
                if (player != null)
                {
                    player.TakePlayerSpDamage(_damage);
                    Debug.Log($"플레이어에게 곤봉 적중! 데미지: {_damage}");
                }
                Destroy(gameObject); // 맞췄으니 파괴
                return; // 더 이상 이동하지 않도록 함수 종료
            }
            // 부딪힌 대상이 그 외의 것(벽, 바닥, 장애물 등)이라면 무조건 파괴!
            else
            {
                Debug.Log("벽 맞고 파괴됨");
                Destroy(gameObject);
                return;
            }
        }

        transform.position = nextPosition;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy")) return; // 적과 부딪힌다면 무시

        // 플레이어에게 맞았을 경우, 기절시켜야함
        if (other.CompareTag("Player"))
        {
            PlayerController player = GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakePlayerSpDamage(_damage);
                Debug.Log("플레이어가 곤봉에 맞았습니다.");
            }
        }
        // 다른 것에 부딪히면 투사체를 없앤다
        Destroy(gameObject);
    }
}