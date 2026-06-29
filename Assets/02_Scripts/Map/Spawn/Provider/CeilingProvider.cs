using System.Collections.Generic;

public class CeilingProvider : ISpawnPositionProvider
{
    public bool TryGetTransform(IReadOnlyList<SpawnArea> spawnAreas, out SpawnTransform transform)
    {
        transform = default;
        return false;
    }
}
