using System.Collections.Generic;
using UnityEngine;

public class MapObjectSpawner
{
    private FloorSpawner _floorSpawner = new(FloorSpawnerDirection.Down);
    private WallSpawner _wallSpawner = new();

    private readonly SpawnAreaRegister _registry = new();

    public void ObjectSpawnAfterMapGenerated(Transform mapRoot)
    {
        _registry.Clear();
        _registry.RegisterFromRoot(mapRoot);

        IReadOnlyList<SpawnArea> floorAreas = _registry.GetAreas(AreaType.Floor);
        IReadOnlyList<SpawnArea> wallAreas = _registry.GetAreas(AreaType.Wall);

        _floorSpawner.SpawnObjectFromFloor(floorAreas);
        _wallSpawner.SpawnObjectFromWall(wallAreas);
    }
}