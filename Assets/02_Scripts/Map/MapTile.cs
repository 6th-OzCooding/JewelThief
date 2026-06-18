using UnityEngine;

public class MapTile : MonoBehaviour
{
    [SerializeField] private MapTile[] _upTiles;
    [SerializeField] private MapTile[] _downTiles;
    [SerializeField] private MapTile[] _leftTiles;
    [SerializeField] private MapTile[] _rightTiles;

    public MapTile[] GetUpTiles => _upTiles;
    public MapTile[] GetDownTiles => _downTiles;
    public MapTile[] GetLeftTiles => _leftTiles;
    public MapTile[] GetRightTiles => _rightTiles;
}
