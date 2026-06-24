using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WFCMapGeneration
{
    // Map Generation Settings
    private float _gridSpacing = 10f;
    private int _mapSizeSetting = 10;
    private int _mapSize;
    private int _generationCount = 0;

    private SOTilePreset _soTilePreset;

    // Objects
    private List<MapTile> _tileObjects;
    private MapGrid _mapGridObject;
    private MapTile _backUpTile;
    private MapTile _boundaryTile;

    // cache
    private List<MapGrid> _grids;
    private List<MapTile> _generatedTiles;

    // Buffer를 만들어 재활용
    private readonly List<MapGrid> _lowEntropyGrids = new();
    private readonly MapGrid[] _collapsedNeighbors = new MapGrid[4];


    public async UniTask StartGenerateMap(Action<float> onProgress = null)
    {
        _tileObjects = new();
        _generatedTiles = new();
        _grids = new();
        _mapSize = _mapSizeSetting + 2;
        _generationCount = 0;

        onProgress?.Invoke(0.0f);

        await LoadAssets();

        onProgress?.Invoke(0.1f);

        GenerateGrids();
        PresetTileGenerate();

        await GenerateMapAsync(onProgress);

        onProgress?.Invoke(1.0f);
    }

    private void GenerateGrids()
    {
        int index = 0;

        for (int y = 0; y < _mapSize; y++)
        {
            for (int x = 0; x < _mapSize; x++)
            {
                MapGrid newGrid = GameObject.Instantiate(_mapGridObject, new Vector3(x * _gridSpacing, 0, y * _gridSpacing), Quaternion.identity);

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

    private async UniTask GenerateMapAsync(Action<float> onProgress)
    {
        float totalCount = _mapSizeSetting * _mapSizeSetting;

        while(_generationCount < totalCount)
        {
            if(CheckEntropy())
            {
                float progess = _generationCount / totalCount;
                onProgress?.Invoke(progess);

                await UniTask.Yield();
            }
            else
            {
                Debug.Log("모순 발생 맵 재성성 시작!");
                break;
            }

        }
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
                Debug.Log("백업 타일 설정!");
                grid.SetTileOptions(new MapTile[] { _backUpTile });
                optionCount = 1;
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

        CollapseGrid(_lowEntropyGrids);
        return true;
    }

    private void CollapseGrid(List<MapGrid> collapseCandidatetGrids)
    {
        int randomGridIndex = UnityEngine.Random.Range(0, collapseCandidatetGrids.Count);

        MapGrid currentGrid = collapseCandidatetGrids[randomGridIndex];

        currentGrid.IsCollapsed = true;

        MapTile selectedTile = currentGrid.TileOptions[UnityEngine.Random.Range(0, currentGrid.TileOptions.Length)];
        currentGrid.TileOptions = new MapTile[] { selectedTile };

        var newTile = GameObject.Instantiate(selectedTile
                    , currentGrid.transform.position + selectedTile.transform.position
                    , selectedTile.transform.rotation);

        _generatedTiles.Add(newTile);
        UpdateGeneration(currentGrid, selectedTile);
    }

    private void UpdateGeneration(MapGrid currentGrid, MapTile seletedTile)
    {
        UpdateCollapsedNeighbors(currentGrid);

        for (int i = 0; i < _collapsedNeighbors.Length; i++)
        {
            if (_collapsedNeighbors[i] == null || _collapsedNeighbors[i].IsCollapsed)
                continue;

            MapTile[] updatedOptions = null;
            if (i == 0)         // 위
            {
                updatedOptions = CheckValidation(_collapsedNeighbors[i].TileOptions, seletedTile.GetUpTiles);
            }
            else if (i == 1)    // 아래
            {
                updatedOptions = CheckValidation(_collapsedNeighbors[i].TileOptions, seletedTile.GetDownTiles);
            }
            else if (i == 2)    // 오른쪽
            {
                updatedOptions = CheckValidation(_collapsedNeighbors[i].TileOptions, seletedTile.GetRightTiles);
            }
            else if (i == 3)    // 왼쪽
            {
                updatedOptions = CheckValidation(_collapsedNeighbors[i].TileOptions, seletedTile.GetLeftTiles);
            }

            _collapsedNeighbors[i].SetTileOptions(updatedOptions);
        }

        _generationCount++;
    }

    private void UpdateCollapsedNeighbors(MapGrid currentGrid)
    {
        int currentGridIndex = currentGrid.Index;

        int upIndex = (currentGridIndex + _mapSizeSetting < _grids.Count) ? currentGridIndex + _mapSizeSetting : -1;
        int downIndex = (currentGridIndex - _mapSizeSetting >= 0) ? currentGridIndex - _mapSizeSetting : -1;
        int rightIndex = (currentGridIndex % _mapSizeSetting < _mapSizeSetting - 1) ? currentGridIndex + 1 : -1;
        int leftIndex = (currentGridIndex % _mapSizeSetting > 0) ? currentGridIndex - 1 : -1;

        _collapsedNeighbors[0] = upIndex != -1 ? _grids[upIndex] : null;
        _collapsedNeighbors[1] = downIndex != -1 ? _grids[downIndex] : null;
        _collapsedNeighbors[2] = rightIndex != -1 ? _grids[rightIndex] : null;
        _collapsedNeighbors[3] = leftIndex != -1 ? _grids[leftIndex] : null;
    }

    private MapTile[] CheckValidation(MapTile[] neighborOptionsList, MapTile[] validOptions)
    {
        List<MapTile> updateOptions = new(neighborOptionsList);
        for (int i = updateOptions.Count - 1; i >= 0; i--)
        {
            if (!validOptions.Contains(updateOptions[i]))
            {
                updateOptions.RemoveAt(i);
            }
        }

        return updateOptions.ToArray();
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
        var newBoundaryTile = GameObject.Instantiate(_boundaryTile, newGrid.transform.position + dir * _gridSpacing / 2, rotation);
        _generatedTiles.Add(newBoundaryTile);
    }

    private async UniTask LoadAssets()
    {
        _mapGridObject = GameManager.Resource.GetLoadedAsset<GameObject>("MapGrid").GetComponent<MapGrid>();

        _backUpTile = GameManager.Resource.GetLoadedAsset<GameObject>("BackUpTile").GetComponent<MapTile>();
        _boundaryTile = GameManager.Resource.GetLoadedAsset<GameObject>("BoundaryTile").GetComponent<MapTile>();

        _tileObjects.Add(GameManager.Resource.GetLoadedAsset<GameObject>("Vertical Corridor").GetComponent<MapTile>());
        _tileObjects.Add(GameManager.Resource.GetLoadedAsset<GameObject>("Room1").GetComponent<MapTile>());
        _tileObjects.Add(GameManager.Resource.GetLoadedAsset<GameObject>("RightTop Corridor").GetComponent<MapTile>());
        _tileObjects.Add(GameManager.Resource.GetLoadedAsset<GameObject>("RightDown Corridor").GetComponent<MapTile>());
        _tileObjects.Add(GameManager.Resource.GetLoadedAsset<GameObject>("LeftTop Corridor").GetComponent<MapTile>());
        _tileObjects.Add(GameManager.Resource.GetLoadedAsset<GameObject>("LeftDown Corridor").GetComponent<MapTile>());
        _tileObjects.Add(GameManager.Resource.GetLoadedAsset<GameObject>("Horizontal Corridor").GetComponent<MapTile>());
        _tileObjects.Add(GameManager.Resource.GetLoadedAsset<GameObject>("Demo Room4").GetComponent<MapTile>());
        _tileObjects.Add(GameManager.Resource.GetLoadedAsset<GameObject>("Demo Room3").GetComponent<MapTile>());
        _tileObjects.Add(GameManager.Resource.GetLoadedAsset<GameObject>("Demo Room2").GetComponent<MapTile>());
        _tileObjects.Add(GameManager.Resource.GetLoadedAsset<GameObject>("Demo Room1").GetComponent<MapTile>());
        _tileObjects.Add(GameManager.Resource.GetLoadedAsset<GameObject>("All Direction Corridor").GetComponent<MapTile>());


        _soTilePreset = await GameManager.Resource.LoadAssetAsync<SOTilePreset>("SOTilePreset");
        Debug.Log("맵 에셋 할당 완료!");
    }

    private void PresetTileGenerate()
    {
        if (null == _soTilePreset || _soTilePreset.presetTiles.Count == 0)
            return;

        foreach (TilePresetData presetTile in _soTilePreset.presetTiles)
        {
            Vector2Int pos = presetTile.position;

            if (pos.x < 0 || pos.x >= _mapSizeSetting || pos.y < 0 || pos.y >= _mapSizeSetting)
                continue;

            if (null == presetTile.tilePrefab)
                continue;

            int gridIndex = (int)(pos.x + pos.y * _mapSizeSetting);
            MapGrid currentGrid = _grids[gridIndex];
            currentGrid.IsCollapsed = true;

            MapTile tile = presetTile.tilePrefab;

            currentGrid.SetTileOptions(new MapTile[] { tile });

            var newTile = GameObject.Instantiate(tile,
            currentGrid.transform.position + tile.transform.position,
            tile.transform.rotation
            );

            _generatedTiles.Add(newTile);

            UpdateGeneration(currentGrid, tile);
        }
    }

    public void Release()
    {
        DestroyGrid();
        DestroyTile();

        _tileObjects.Clear();
        _lowEntropyGrids.Clear();

        _generationCount = 0;
    }

    private void DestroyGrid()
    {
        foreach(var grid in _grids)
        {
            GameObject.Destroy(grid.gameObject);
        }
        _grids.Clear();
    }

    private void DestroyTile()
    {
        foreach(var tile in _generatedTiles)
        {
            GameObject.Destroy(tile.gameObject);
        }
        _generatedTiles.Clear();
    }
}
