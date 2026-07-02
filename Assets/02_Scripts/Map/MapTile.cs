using UnityEngine;

public enum  MapTileType
{
    Room,
    Corridor
}

public class MapTile : MonoBehaviour
{
    [SerializeField] private bool _openUp;
    [SerializeField] private bool _openDown;
    [SerializeField] private bool _openLeft;
    [SerializeField] private bool _openRight;

    [SerializeField] private MapTileType _tileType;
    [SerializeField] private int _weight = 1;

    public bool OpenUp => _openUp;
    public bool OpenDown => _openDown;
    public bool OpenLeft => _openLeft;
    public bool OpenRight => _openRight;

    public MapTileType TileType => _tileType;
    public int Weight => _weight;
}

