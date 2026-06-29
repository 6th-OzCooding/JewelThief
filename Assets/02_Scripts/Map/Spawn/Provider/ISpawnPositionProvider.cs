using System.Collections.Generic;
using UnityEngine;

public interface ISpawnPositionProvider
{
    bool TryGetTransform(IReadOnlyList<SpawnArea> spawnAreas, out SpawnTransform transform);
}

public struct SpawnTransform
{
    public Vector3 Position;
    public Quaternion Rotation;

    public SpawnTransform(Vector3 position, Quaternion rotation)
    {
        Position = position;
        Rotation = rotation;
    }
}