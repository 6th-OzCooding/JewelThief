using System.Collections.Generic;
using UnityEngine;

public interface ISpawnPositionProvider
{
    bool GetSpawnInfo(IReadOnlyList<SpawnArea> spawnAreas, out SpawnInfo spawnInfo);
}

public struct SpawnInfo
{
    public Vector3 Position;
    public Quaternion Rotation;
    public bool IsGravityOff;

    public SpawnInfo(Vector3 position, Quaternion rotation, bool isGravityOff = false)
    {
        Position = position;
        Rotation = rotation;
        IsGravityOff = isGravityOff;
    }
}