using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

public class WFCMapGeneration
{
    public SOTilePreset SOTilePreset { get; private set; }

    private enum Direction
    {
        Up,
        Down,
        Right,
        Left
    }

    private readonly Direction[] _directions =
    {
        Direction.Up,
        Direction.Down,
        Direction.Right,
        Direction.Left
    };

    // Map Generation Settings
    private int _mapSize;
    private int _generationCount = 0;
    private int _mapSizeSetting;
    private float _gridSpacing = 10f;

    // cache
    private MapTile _boundaryTile;
    private MapGrid _mapGridObject;
    private List<MapGrid> _grids = new();
    private List<MapTile> _tileObjects = new();
    private Dictionary<int, MapTile> _generatedTiles = new();   // 키는 grid index, 바운더리 타일은 음수

    // 재활용 Buffer를
    private readonly List<MapGrid> _lowEntropyGrids = new();
    private readonly MapGrid[] _collapsedNeighbors = new MapGrid[4];
    private Queue<MapGrid> _propagateQueue = new();
    private HashSet<int> _propagateQueueIndexes = new();
    private Queue<MapGrid> _checkReachableTileQueue = new();
    private HashSet<int> _checkReachableTileVisitedIndexs = new();
    private List<int> _removeTiles = new();

    // Map Object Spawner
    private Transform _mapRoot;
    private MapObjectSpawner _mapObjectSpawner = new();
    private RunTimeBakeNavMesh _runTimeBakeNavMesh = new();

    // Generation Map Exception
    private MapGrid _startGrid;


    public void Release()
    {
        ClearGeneratedMap();

        _tileObjects?.Clear();
        _startGrid = null;

        _removeTiles.Clear();

        Initialize();
    }

    public async UniTask StartGenerateMap(NavMeshSurface navMeshSurface, Transform mapRoot, Action<float> onProgress = null)
    {
        await LoadAssets();

        _mapRoot = mapRoot;
        _runTimeBakeNavMesh.Init(navMeshSurface);

        _mapSize = _mapSizeSetting + 2;

        bool success = false;
        for (int retryCount = 0; retryCount < SOTilePreset.MaxGenerateRetryCount; retryCount++)
        {
            Debug.Log($"WFC 맵 생성 시도: {retryCount + 1}/{SOTilePreset.MaxGenerateRetryCount}");

            ClearGeneratedMap();
            Initialize();

            GenerateGrids();

            bool presetSuccess = PresetTileGenerate();

            if (!presetSuccess)
            {
                Debug.LogWarning("프리셋 배치 중 모순 발생. 맵 생성 중단.");
                return;
            }

            success = await GenerateMapAsync(onProgress);

            if (!success)
            {
                Debug.LogWarning("WFC 모순 발생. 맵 재생성 시도.");
                await UniTask.Yield();
                continue;
            }

            if (!CheckReachableTileCount())
            {
                Debug.LogWarning("시작 타일 기준 도달 가능한 타일 수가 부족. 맵 재생성 시도.");
                success = false;
                await UniTask.Yield();
                continue;
            }

            if(!CheckReachableRoomCount())
            {
                Debug.LogWarning("도달 가능한 방 개수가 부족. 맵 재생성 시도.");
                success = false;
                await UniTask.Yield();
                continue;
            }

            DeleteUnreacheableTile();
            Debug.Log("WFC 맵 생성 성공");
            break;
        }

        if (!success)
        {
            Debug.LogError($"WFC 맵 생성 실패. 최대 재시도 횟수 초과: {SOTilePreset.MaxGenerateRetryCount}");
            return;
        }

        _mapObjectSpawner.ObjectSpawnAfterMapGenerated(_mapRoot);
        await _runTimeBakeNavMesh.BakeAfterMapGeneratedAsync();

        onProgress?.Invoke(1.0f);
    }


    private void Initialize()
    {
        _generationCount = 0;

        _propagateQueue.Clear();
        _propagateQueueIndexes.Clear();

        _checkReachableTileQueue.Clear();
        _checkReachableTileVisitedIndexs.Clear();

        _grids.Clear();
        _generatedTiles.Clear();
        _lowEntropyGrids.Clear();
    }

    private void ClearGeneratedMap()
    {
        DestroyGrid();
        DestroyTile();
    }

    private void GenerateGrids()
    {
        int index = 0;

        for (int y = 0; y < _mapSize; y++)
        {
            for (int x = 0; x < _mapSize; x++)
            {
                MapGrid newGrid = GameObject.Instantiate(_mapGridObject, new Vector3(x * _gridSpacing, 0, y * _gridSpacing), Quaternion.identity, _mapRoot);

                if (x == 0 || y == 0 || x == _mapSize - 1 || y == _mapSize - 1)
                {
                    int pos = x + y * _mapSize;
                    CreateBoundary(newGrid, pos);
                }
                else
                {
                    newGrid.CreateMapGrid(false, _tileObjects, index);
                    _grids.Add(newGrid);
                    index++;
                }
            }
        }
    }

    private async UniTask<bool> GenerateMapAsync(Action<float> onProgress)
    {
        float totalCount = _mapSizeSetting * _mapSizeSetting;

        while (_generationCount < totalCount)
        {
            if (CheckEntropy())
            {
                float progess = _generationCount / totalCount;
                onProgress?.Invoke(progess);

                await UniTask.Yield();
            }
            else
            {
                Debug.Log("모순 발생 맵 재성성 시작!");
                return false;
            }
        }

        return true;
    }

    private bool CheckEntropy()
    {
        _lowEntropyGrids.Clear();

        int lowestEntropy = int.MaxValue;

        for (int i = 0; i < _grids.Count; i++)
        {
            MapGrid grid = _grids[i];

            if (_grids[i].IsCollapsed)
            {
                continue;
            }

            int optionCount = grid.TileOptions.Length;

            if (optionCount == 0)
            {
                Debug.Log("WFC 모순 발생");
                return false;
            }

            if (optionCount < lowestEntropy)
            {
                lowestEntropy = optionCount;
                _lowEntropyGrids.Clear();
                _lowEntropyGrids.Add(grid);
            }
            else if (optionCount == lowestEntropy)
            {
                _lowEntropyGrids.Add(grid);
            }
        }

        if (_lowEntropyGrids.Count == 0)
            return false;

        return CollapseGrid(_lowEntropyGrids);
    }

    private bool CollapseGrid(List<MapGrid> collapseCandidatetGrids)
    {
        int randomGridIndex = UnityEngine.Random.Range(0, collapseCandidatetGrids.Count);

        MapGrid currentGrid = collapseCandidatetGrids[randomGridIndex];

        currentGrid.IsCollapsed = true;

        MapTile selectedTile = null;

        int totalWeight = 0;
        for (int i = 0; i < currentGrid.TileOptions.Length; i++)
        {
            totalWeight += currentGrid.TileOptions[i].Weight;
        }

        int randomWeight = UnityEngine.Random.Range(0, totalWeight);
        int currentWeight = 0;
        for (int i = 0; i < currentGrid.TileOptions.Length; i++)
        {
            currentWeight += currentGrid.TileOptions[i].Weight;
            if(randomWeight < currentWeight)
            {
                selectedTile = currentGrid.TileOptions[i];
                break;
            }
        }

        if(selectedTile == null)
        {
            Debug.LogError("CollapseGrid: 타일 선택 실패");
            return false;
        }
        currentGrid.TileOptions = new MapTile[] { selectedTile };

        _generationCount++;

        bool success = Propagate(currentGrid);
        if (!success)
        {
            Debug.Log("모순 발생 맵 재성성 시작!");
            return false;
        }

        var newTile = GameObject.Instantiate(selectedTile
                    , currentGrid.transform.position + selectedTile.transform.position
                    , selectedTile.transform.rotation
                    , _mapRoot);

        _generatedTiles[currentGrid.Index] = newTile;

        return true;
    }

    private bool Propagate(MapGrid collapseGrid)
    {
        _propagateQueue.Clear();
        _propagateQueueIndexes.Clear();

        EnqueueNeighbors(collapseGrid, _propagateQueue, _propagateQueueIndexes);

        while (_propagateQueue.Count > 0)
        {
            MapGrid neighborGrid = _propagateQueue.Dequeue();
            _propagateQueueIndexes.Remove(neighborGrid.Index);

            if (neighborGrid.IsCollapsed)
                continue;

            MapTile[] oldOptions = neighborGrid.TileOptions;
            MapTile[] newOptions = UpdateOptionsByNeighbors(neighborGrid);

            if (newOptions.Length == 0)
            {
                Debug.Log("모순 발생");
                return false;
            }

            if (newOptions.Length != oldOptions.Length)
            {
                neighborGrid.SetTileOptions(newOptions);
                EnqueueNeighbors(neighborGrid, _propagateQueue, _propagateQueueIndexes);
            }
        }

        return true;
    }

    private void EnqueueNeighbors(MapGrid grid, Queue<MapGrid> queue, HashSet<int> queueIndexes)
    {
        foreach (Direction dir in _directions)
        {
            MapGrid neighbor = GetNeighbor(grid, dir);
            if (neighbor != null && !neighbor.IsCollapsed && !queueIndexes.Contains(neighbor.Index))
            {
                queue.Enqueue(neighbor);
                queueIndexes.Add(neighbor.Index);
            }
        }
    }

    private MapGrid GetNeighbor(MapGrid currentGrid, Direction direction)
    {
        int currentIndex = currentGrid.Index;
        int neighborIndex = -1;
        switch (direction)
        {
            case Direction.Up:
                neighborIndex = (currentIndex + _mapSizeSetting < _grids.Count) ? currentIndex + _mapSizeSetting : -1;
                break;
            case Direction.Down:
                neighborIndex = (currentIndex - _mapSizeSetting >= 0) ? currentIndex - _mapSizeSetting : -1;
                break;
            case Direction.Right:
                neighborIndex = (currentIndex % _mapSizeSetting < _mapSizeSetting - 1) ? currentIndex + 1 : -1;
                break;
            case Direction.Left:
                neighborIndex = (currentIndex % _mapSizeSetting > 0) ? currentIndex - 1 : -1;
                break;
        }
        return neighborIndex != -1 ? _grids[neighborIndex] : null;
    }

    private MapTile[] UpdateOptionsByNeighbors(MapGrid updatingGrid)
    {
        List<MapTile> validOptions = new();

        foreach (MapTile candidateTile in updatingGrid.TileOptions)
        {
            bool isValid = true;

            foreach (Direction direction in _directions)
            {
                MapGrid neighborGrid = GetNeighbor(updatingGrid, direction);

                if (null == neighborGrid)
                {
                    if (IsOpen(candidateTile, direction))
                    {
                        isValid = false;
                        break;
                    }

                    continue;
                }

                if (!HasCompatibleNeighborOption(candidateTile, neighborGrid, direction))
                {
                    isValid = false;
                    break;
                }
            }

            if (isValid)
            {
                validOptions.Add(candidateTile);
            }
        }

        return validOptions.ToArray();
    }

    private bool HasCompatibleNeighborOption(MapTile candidateTile, MapGrid neighborGrid, Direction direction)
    {
        foreach (MapTile neighborTile in neighborGrid.TileOptions)
        {
            if (IsCompatible(candidateTile, neighborTile, direction))
            {
                return true;
            }
        }

        return false;
    }

    private bool IsCompatible(MapTile candidateTile, MapTile neighborTile, Direction direction)
    {
        bool currentOpen = IsOpen(candidateTile, direction);
        bool neighborOpen = IsOpen(neighborTile, GetOppositeDirection(direction));

        return currentOpen == neighborOpen;
    }

    private bool IsOpen(MapTile tile, Direction direction)
    {
        return direction switch
        {
            Direction.Up => tile.OpenUp,
            Direction.Down => tile.OpenDown,
            Direction.Right => tile.OpenRight,
            Direction.Left => tile.OpenLeft,
            _ => false
        };
    }

    private void CreateBoundary(MapGrid newGrid, int pos)
    {
        Quaternion rotation = Quaternion.identity;
        Vector3 dir = Vector3.zero;

        if (pos < _mapSize)                             // 아래 경계
        {
            rotation = Quaternion.Euler(0, 0, 0);
            dir = Vector3.forward;
        }
        else if (pos >= _mapSize * (_mapSize - 1))      // 위쪽 경계
        {
            rotation = Quaternion.Euler(0, 180, 0);
            dir = Vector3.back;
        }
        else if (pos % _mapSize == 0)                   // 왼쪽 경계
        {
            rotation = Quaternion.Euler(0, 90, 0);
            dir = Vector3.right;
        }
        else if (pos % _mapSize == _mapSize - 1)        // 오른쪽 경계
        {
            rotation = Quaternion.Euler(0, -90, 0);
            dir = Vector3.left;
        }

        newGrid.CreateMapGrid(true, new List<MapTile> { _boundaryTile }, -1);
        var newBoundaryTile = GameObject.Instantiate(_boundaryTile, newGrid.transform.position + dir * _gridSpacing / 2, rotation, _mapRoot);
        _generatedTiles[-1 * (pos + 1)] = newBoundaryTile;
    }

    private async UniTask LoadAssets()
    {
        _mapGridObject = GameManager.Resource.GetLoadedAsset<GameObject>("MapGrid").GetComponent<MapGrid>();
        _boundaryTile = GameManager.Resource.GetLoadedAsset<GameObject>("BoundaryTile").GetComponent<MapTile>();

        StageData stageData = GameManager.DataTable.GetStageData(GameManager.Instance.SelectedStageId);

        foreach (string tileAddress in stageData.TileAddress)
            _tileObjects.Add(GameManager.Resource.GetLoadedAsset<GameObject>(tileAddress).GetComponent<MapTile>());

        SOTilePreset = await GameManager.Resource.LoadAssetAsync<SOTilePreset>("SOTilePreset");
        _mapSizeSetting = SOTilePreset.MapWidth;
    }

    private bool PresetTileGenerate()
    {
        if (null == SOTilePreset || SOTilePreset.presetTiles.Count == 0)
            return true;

        foreach (TilePresetData presetTile in SOTilePreset.presetTiles)
        {
            Vector2Int pos = presetTile.position;

            if (pos.x < 0 || pos.x >= _mapSizeSetting || pos.y < 0 || pos.y >= _mapSizeSetting)
                continue;

            if (null == presetTile.tilePrefab)
                continue;

            int gridIndex = pos.x + pos.y * _mapSizeSetting;
            MapGrid currentGrid = _grids[gridIndex];

            if (currentGrid.IsCollapsed)
            {
                Debug.LogError($"프리셋 좌표 중복 또는 이미 확정된 Grid입니다. Pos: {pos}, Index: {gridIndex}");
                return false;
            }

            currentGrid.IsCollapsed = true;

            MapTile tile = presetTile.tilePrefab;

            currentGrid.SetTileOptions(new MapTile[] { tile });

            _generationCount++;

            var newTile = GameObject.Instantiate(tile,
            currentGrid.transform.position + tile.transform.position,
            tile.transform.rotation
            , _mapRoot);

            if (presetTile.IsStartTile)
            {
                _startGrid = currentGrid;
#if UNITY_EDITOR
                newTile.SetStartTile();
#endif
            }

            _generatedTiles[currentGrid.Index] = newTile;

            bool sucess = Propagate(currentGrid);

            if (!sucess)
            {
                Debug.Log("프리셋 타일 전파 중 모순 발생");
                return false;
            }
        }

        return true;
    }

    private void DestroyGrid()
    {
        foreach (var grid in _grids)
        {
            GameObject.Destroy(grid.gameObject);
        }
        _grids.Clear();
    }

    private void DestroyTile()
    {
        foreach (var tile in _generatedTiles.Values)
        {
            GameObject.Destroy(tile.gameObject);
        }
        _generatedTiles.Clear();
    }

    private bool CheckReachableTileCount()
    {
        if (_startGrid == null)
        {
            Debug.LogError("시작 Grid가 없습니다.");
            return false;
        }

        _checkReachableTileQueue.Clear();
        _checkReachableTileVisitedIndexs.Clear();

        _checkReachableTileQueue.Enqueue(_startGrid);
        _checkReachableTileVisitedIndexs.Add(_startGrid.Index);

        while (_checkReachableTileQueue.Count > 0)
        {
            MapGrid currentGrid = _checkReachableTileQueue.Dequeue();

            foreach (Direction direction in _directions)
            {
                MapGrid neighborGrid = GetNeighbor(currentGrid, direction);

                if (neighborGrid == null)
                    continue;

                if (_checkReachableTileVisitedIndexs.Contains(neighborGrid.Index))
                    continue;

                if (!neighborGrid.IsCollapsed)
                    continue;

                if (!CheckConnected(currentGrid, neighborGrid, direction))
                    continue;

                _checkReachableTileVisitedIndexs.Add(neighborGrid.Index);
                _checkReachableTileQueue.Enqueue(neighborGrid);
            }
        }

        int reachableCount = _checkReachableTileVisitedIndexs.Count;

        Debug.Log($"시작 타일 기준 도달 가능 타일 수: {reachableCount}/{_grids.Count}, 필요 개수: {SOTilePreset.MinReachableTileCount}");

        return reachableCount >= SOTilePreset.MinReachableTileCount;
    }

    private bool CheckConnected(MapGrid currentGrid, MapGrid neighborGrid, Direction direction)
    {
        if (!TryGetCollapsedTile(currentGrid, out MapTile currentTile))
            return false;

        if (!TryGetCollapsedTile(neighborGrid, out MapTile neighborTile))
            return false;

        bool currentOpen = IsOpen(currentTile, direction);
        bool neighborOpen = IsOpen(neighborTile, GetOppositeDirection(direction));

        return currentOpen && neighborOpen;
    }

    private bool TryGetCollapsedTile(MapGrid grid, out MapTile tile)
    {
        tile = null;

        if (grid == null)
            return false;

        if (!grid.IsCollapsed)
            return false;

        if (grid.TileOptions == null || grid.TileOptions.Length != 1)
            return false;

        tile = grid.TileOptions[0];
        return tile != null;
    }

    private Direction GetOppositeDirection(Direction direction)
    {
        return direction switch
        {
            Direction.Up => Direction.Down,
            Direction.Down => Direction.Up,
            Direction.Right => Direction.Left,
            Direction.Left => Direction.Right,
            _ => direction
        };
    }

    private bool CheckReachableRoomCount()
    {
        int reachableRoomCount = 0;

        foreach (int gridIndex in _checkReachableTileVisitedIndexs)
        {
            MapGrid grid = _grids[gridIndex];

            if (!TryGetCollapsedTile(grid, out MapTile tile))
                continue;

            if (tile.TileType == MapTileType.Room)
                reachableRoomCount++;
        }

        Debug.Log($"시작 타일 기준 도달 가능 방 개수: {reachableRoomCount}, 필요 개수: {SOTilePreset.MinReachableRoomCount}");

        float currentRoomRatio = (float)reachableRoomCount / (_mapSizeSetting * _mapSizeSetting);
        Debug.Log($"방 비율: {currentRoomRatio}, 최대 방 비율: {SOTilePreset.MaxRoomRatio}");


        return (reachableRoomCount >= SOTilePreset.MinReachableRoomCount) && (currentRoomRatio <= SOTilePreset.MaxRoomRatio);
    }

    private void DeleteUnreacheableTile()
    {
        _removeTiles.Clear();

        foreach (var pair in _generatedTiles)
        {
            int gridIndex = pair.Key;

            // BoundaryTile은 음수 Key를 쓰므로 삭제 대상에서 제외
            if (gridIndex < 0)
                continue;

            if (_checkReachableTileVisitedIndexs.Contains(gridIndex))
                continue;

            MapTile tile = pair.Value;

            if (tile != null)
                GameObject.Destroy(tile.gameObject);

            _removeTiles.Add(gridIndex);
        }

        for (int i = 0; i < _removeTiles.Count; i++)
        {
            _generatedTiles.Remove(_removeTiles[i]);
        }

        Debug.Log($"갈 수 없는 타일 삭제 완료. 삭제 개수: {_removeTiles.Count}");
    }
}
