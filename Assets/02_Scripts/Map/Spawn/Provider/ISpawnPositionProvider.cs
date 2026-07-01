using System.Collections.Generic;
using UnityEngine;

public interface ISpawnPositionProvider
{
    SpawnInfo GetSpawnInfo(IReadOnlyList<SpawnArea> spawnAreas);
}

public struct SpawnInfo
{
    public Vector3 Position;
    public Quaternion Rotation;

    public SpawnInfo(Vector3 position, Quaternion rotation)
    {
        Position = position;
        Rotation = rotation;
    }
}