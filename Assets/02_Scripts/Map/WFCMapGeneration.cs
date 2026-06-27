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
    private int _mapSizeSetting = 10;
    private float _gridSpacing = 10f;

    // cache
    private MapTile _boundaryTile;
    private MapGrid _mapGridObject;
    private List<MapGrid> _grids;
    private List<MapTile> _tileObjects;
    private List<MapTile> _generatedTiles;

    // 재활용 Buffer를
    private readonly List<MapGrid> _lowEntropyGrids = new();
    private readonly MapGrid[] _collapsedNeighbors = new MapGrid[4];
    private Queue<MapGrid> _propagateQueue = new();
    private HashSet<int> _propagateQueueIndexes = new();
    private Queue<MapGrid> _checkReachableTileQueue = new();
    private HashSet<int> _checkReachableTileVisitedIndexs = new();

    // Map Object Spawner
    private Transform _mapRoot;
    private MapObjectSpawner _mapObjectSpawner = new();
    private RunTimeBakeNavMesh _runTimeBakeNavMesh = new();

    // Generation Map Exception
    private MapGrid _startGrid;
    private int _maxGenerateRetryCount;    // 맵 생성 실패 시 재시도 횟수
    private int _minReachableTileCount;    // 도달 가능한 타일 수 최소값
    private int _minReachableRoomCount;     // 도달 가능한 방 개수 최소값

    public async UniTask StartGenerateMap(NavMeshSurface navMeshSurface, Transform mapRoot, Action<float> onProgress = null)
    {
        _mapRoot = mapRoot;
        _runTimeBakeNavMesh.Init(navMeshSurface);

        _tileObjects = new();
        _generatedTiles = new();
        _grids = new();
        _mapSize = _mapSizeSetting + 2;

        await LoadAssets();

        bool success = false;
        for (int retryCount = 0; retryCount < _maxGenerateRetryCount; retryCount++)
        {
            Debug.Log($"WFC 맵 생성 시도: {retryCount + 1}/{_maxGenerateRetryCount}");

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

            Debug.Log("WFC 맵 생성 성공");
            break;
        }

        if (!success)
        {
            Debug.LogError($"WFC 맵 생성 실패. 최대 재시도 횟수 초과: {_maxGenerateRetryCount}");
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

        DestroyGrid();
        DestroyTile();
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

        _generatedTiles.Add(newTile);

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
                    continue;

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
        _generatedTiles.Add(newBoundaryTile);
    }

    private async UniTask LoadAssets()
    {
        _mapGridObject = GameManager.Resource.GetLoadedAsset<GameObject>("MapGrid").GetComponent<MapGrid>();

        // TODO(김익환, 26-06-24): 어떤 스테이지에 따라 아래 하드 코딩된 것을 등록 하면 될 듯 - 그건 데이터 드리븐으로 가져오고.
        _boundaryTile = GameManager.Resource.GetLoadedAsset<GameObject>("BoundaryTile").GetComponent<MapTile>();

        _tileObjects.Add(GameManager.Resource.GetLoadedAsset<GameObject>("All Direction Corridor").GetComponent<MapTile>());
        _tileObjects.Add(GameManager.Resource.GetLoadedAsset<GameObject>("Horizontal Corridor").GetComponent<MapTile>());
        _tileObjects.Add(GameManager.Resource.GetLoadedAsset<GameObject>("Vertical Corridor").GetComponent<MapTile>());
        _tileObjects.Add(GameManager.Resource.GetLoadedAsset<GameObject>("RightTop Corridor").GetComponent<MapTile>());
        _tileObjects.Add(GameManager.Resource.GetLoadedAsset<GameObject>("RightDown Corridor").GetComponent<MapTile>());
        _tileObjects.Add(GameManager.Resource.GetLoadedAsset<GameObject>("LeftTop Corridor").GetComponent<MapTile>());
        _tileObjects.Add(GameManager.Resource.GetLoadedAsset<GameObject>("LeftDown Corridor").GetComponent<MapTile>());
        _tileObjects.Add(GameManager.Resource.GetLoadedAsset<GameObject>("T Corridor1").GetComponent<MapTile>());
        _tileObjects.Add(GameManager.Resource.GetLoadedAsset<GameObject>("T Corridor2").GetComponent<MapTile>());
        _tileObjects.Add(GameManager.Resource.GetLoadedAsset<GameObject>("T Corridor3").GetComponent<MapTile>());
        _tileObjects.Add(GameManager.Resource.GetLoadedAsset<GameObject>("T Corridor4").GetComponent<MapTile>());

        _tileObjects.Add(GameManager.Resource.GetLoadedAsset<GameObject>("Room1").GetComponent<MapTile>());
        _tileObjects.Add(GameManager.Resource.GetLoadedAsset<GameObject>("Demo Room1").GetComponent<MapTile>());
        _tileObjects.Add(GameManager.Resource.GetLoadedAsset<GameObject>("Demo Room2").GetComponent<MapTile>());
        _tileObjects.Add(GameManager.Resource.GetLoadedAsset<GameObject>("Demo Room3").GetComponent<MapTile>());
        _tileObjects.Add(GameManager.Resource.GetLoadedAsset<GameObject>("Demo Room4").GetComponent<MapTile>());
        _tileObjects.Add(GameManager.Resource.GetLoadedAsset<GameObject>("Demo Room5").GetComponent<MapTile>());
        _tileObjects.Add(GameManager.Resource.GetLoadedAsset<GameObject>("Demo Room6").GetComponent<MapTile>());
        _tileObjects.Add(GameManager.Resource.GetLoadedAsset<GameObject>("Demo Room7").GetComponent<MapTile>());


        SOTilePreset = await GameManager.Resource.LoadAssetAsync<SOTilePreset>("SOTilePreset");

        _maxGenerateRetryCount = SOTilePreset.MaxGenerateRetryCount;
        _minReachableTileCount = SOTilePreset.MinReachableTileCount;
        _minReachableRoomCount = SOTilePreset.MinReachableRoomCount;
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
            currentGrid.IsCollapsed = true;

            if (presetTile.IsStartTile)
                _startGrid = currentGrid;

            MapTile tile = presetTile.tilePrefab;

            currentGrid.SetTileOptions(new MapTile[] { tile });

            _generationCount++;

            var newTile = GameObject.Instantiate(tile,
            currentGrid.transform.position + tile.transform.position,
            tile.transform.rotation
            , _mapRoot);

#if UNITY_EDITOR
            newTile.SetStartTile();
#endif
            _generatedTiles.Add(newTile);

            bool sucess = Propagate(currentGrid);

            if (!sucess)
            {
                Debug.Log("프리셋 타일 전파 중 모순 발생");
                return false;
            }
        }

        return true;
    }

    public void Release()
    {
        Initialize();

        _tileObjects.Clear();
        _startGrid = null;
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
        foreach (var tile in _generatedTiles)
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

        Debug.Log($"시작 타일 기준 도달 가능 타일 수: {reachableCount}/{_grids.Count}, 필요 개수: {_minReachableTileCount}");

        return reachableCount >= _minReachableTileCount;
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
        if (_startGrid == null)
        {
            Debug.LogError("시작 Grid가 없습니다.");
            return false;
        }

        int reachableRoomCount = CountReachableRooms(_startGrid);

        Debug.Log($"시작 타일 기준 도달 가능 방 개수: {reachableRoomCount}, 필요 개수: {_minReachableRoomCount}");

        return reachableRoomCount >= _minReachableRoomCount;
    }

    private int CountReachableRooms(MapGrid startGrid)
    {
        Queue<MapGrid> queue = new();
        HashSet<int> visited = new();

        queue.Enqueue(startGrid);
        visited.Add(startGrid.Index);

        int roomCount = 0;

        while (queue.Count > 0)
        {
            MapGrid currentGrid = queue.Dequeue();

            if (TryGetCollapsedTile(currentGrid, out MapTile currentTile))
            {
                if (currentTile.TileType == MapTileType.Room)
                    roomCount++;
            }

            foreach (Direction direction in _directions)
            {
                MapGrid neighborGrid = GetNeighbor(currentGrid, direction);

                if (neighborGrid == null)
                    continue;

                if (visited.Contains(neighborGrid.Index))
                    continue;

                if (!neighborGrid.IsCollapsed)
                    continue;

                if (!CheckConnected(currentGrid, neighborGrid, direction))
                    continue;

                visited.Add(neighborGrid.Index);
                queue.Enqueue(neighborGrid);
            }
        }

        return roomCount;
    }
}
