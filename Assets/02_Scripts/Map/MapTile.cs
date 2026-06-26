using UnityEngine;

public class MapTile : MonoBehaviour
{
    [SerializeField] private bool _openUp;
    [SerializeField] private bool _openDown;
    [SerializeField] private bool _openLeft;
    [SerializeField] private bool _openRight;

    public bool OpenUp => _openUp;
    public bool OpenDown => _openDown;
    public bool OpenLeft => _openLeft;
    public bool OpenRight => _openRight;


    private bool _isStartTile = false;


    public void SetStartTile()
    {
        _isStartTile = true;
    }

    private void OnDrawGizmos()
    {
        if (_isStartTile)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position, new Vector3(5, 10, 5));
        }
    }
}

