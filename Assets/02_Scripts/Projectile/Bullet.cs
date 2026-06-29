using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float _speed = 10f;
    [SerializeField] private float _lifeTime = 5f;

    Vector3 _moveDirection;

    public void Init(Vector3 direction)
    {
        _moveDirection = direction.normalized;
        transform.rotation = Quaternion.LookRotation(_moveDirection);
        Destroy(gameObject, _lifeTime);
    }

    private void Update()
    {
        transform.position += _moveDirection * _speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            Debug.Log($"Bullet hit: {other.gameObject.name}");
            Destroy(gameObject);
        }
    }
}
