using UnityEngine;

public class MapTile : MonoBehaviour
{
    [SerializeField] private MapTile[] _upTiles;
    [SerializeField] private MapTile[] _downTiles;
    [SerializeField] private MapTile[] _leftTiles;
    [SerializeField] private MapTile[] _rightTiles;

    [SerializeField] public GameObject CeilingGO;

    [Header("베이스 타일 스폰 포인트")]
    [SerializeField] private Transform _spawnPoint;

    public MapTile[] GetUpTiles => _upTiles;
    public MapTile[] GetDownTiles => _downTiles;
    public MapTile[] GetLeftTiles => _leftTiles;
    public MapTile[] GetRightTiles => _rightTiles;
    public Transform SpawnPoint => _spawnPoint;

    public void SetCeilingGO(bool active)
    {
        if (CeilingGO != null)
        {
            CeilingGO.SetActive(active);
        }
    }
}

