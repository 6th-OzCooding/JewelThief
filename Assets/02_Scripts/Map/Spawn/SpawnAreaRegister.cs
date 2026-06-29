using System.Collections.Generic;
using UnityEngine;

public class SpawnAreaRegister
{
    private readonly Dictionary<AreaType, List<SpawnArea>> _areasByType = new();

    public void Clear()
    {
        _areasByType.Clear();
    }

    public void Register(SpawnArea area)
    {
        if (area == null)
            return;

        if (!_areasByType.TryGetValue(area.AreaType, out List<SpawnArea> list))
        {
            list = new List<SpawnArea>();
            _areasByType.Add(area.AreaType, list);
        }

        list.Add(area);
    }

    public void RegisterFromRoot(IEnumerable<MapTile> tiles)
    {
        if (tiles == null)
            return;

        foreach (MapTile tile in tiles)
        {
            if (tile == null)
                continue;

            SpawnArea[] areas = tile.GetComponentsInChildren<SpawnArea>();

            foreach (SpawnArea area in areas)
                Register(area);
        }
    }

    public IReadOnlyList<SpawnArea> GetAreas(AreaType areaType)
    {
        if (_areasByType.TryGetValue(areaType, out List<SpawnArea> list))
            return list;

        return System.Array.Empty<SpawnArea>();
    }
}