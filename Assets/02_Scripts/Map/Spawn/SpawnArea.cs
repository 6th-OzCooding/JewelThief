using UnityEngine;

public enum AreaType
{
    Floor,
    Wall,
    Ceiling,
    COUNT,
}

public class SpawnArea : MonoBehaviour
{
    [SerializeField] private AreaType _areaType;
    private BoxCollider _collider;

    public AreaType AreaType => _areaType;

    private void Awake()
    {
        _collider = GetComponent<BoxCollider>();
    }

    public Vector3 GetRandomPosition()
    {
        Bounds bounds = _collider.bounds;

        return new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y),
            Random.Range(bounds.min.z, bounds.max.z)
            );
    }

    private void OnDrawGizmos()
    {
        if (null != _collider)
        {
            switch (_areaType)
            {
                case AreaType.Floor:
                    Gizmos.color = Color.yellow;
                    break;
                case AreaType.Wall:
                    Gizmos.color = Color.cyan;
                    break;
                case AreaType.Ceiling:
                    Gizmos.color = Color.pink;
                    break;
                default:
                    Gizmos.color = Color.white;
                    break;
            }

            Gizmos.DrawWireCube(_collider.bounds.center, _collider.bounds.size);
        }
    }
}
