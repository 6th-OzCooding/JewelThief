using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float _speed = 10f;
    [SerializeField] private float _lifeTime = 5f;

    private float _elapsedTime = 0f;

    Vector3 _moveDirection;

    public void Init(Vector3 direction)
    {
        _elapsedTime = 0f;
        _moveDirection = direction.normalized;
        transform.rotation = Quaternion.LookRotation(_moveDirection);
    }

    private void Update()
    {
        if(GameManager.Instance.IsPaused)
            return;

        _elapsedTime += Time.deltaTime;

        if (_elapsedTime >= _lifeTime)
        {
            GameManager.Pool.DespawnToPool(this.gameObject);
            return;
        }

        transform.position += _moveDirection * _speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            GameManager.Pool.DespawnToPool(this.gameObject);
        }
    }
}
