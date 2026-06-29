using System.Collections.Generic;

public class CeilingProvider : ISpawnPositionProvider
{
    public bool GetSpawnInfo(IReadOnlyList<SpawnArea> spawnAreas, out SpawnInfo transform)
    {
        transform = default;
        return false;
    }
}
