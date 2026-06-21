using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WFCMapGeneration : MonoBehaviour
{
    [Header("Map Generation Settings")]
    [SerializeField] private float _gridSpacing = 1f;
    [SerializeField] private int _mapSizeSetting = 10;
    private int _mapSize;

    [Header("Objects")]
    [SerializeField] private MapTile[] _tileObjects;
    [SerializeField] private MapGrid _mapGridObject;
    [SerializeField] private MapTile _backUpTile;
    [SerializeField] private MapTile _boundaryTile;

    [Header("Debug")]
    [SerializeField] private DebugTileInspector _debugTileInspector;

    // cache
    private List<MapGrid> _grids;

    // Buffer를 만들어 재활용
    private readonly List<MapGrid> _lowEntropyGrids = new();
    private readonly MapGrid[] _collapsedNeighbors = new MapGrid[4];

    private int _generationCount = 0;

    private void Awake()
    {
        _grids = new();
        _mapSize = _mapSizeSetting + 2;

        InitGrid();
    }

    private void InitGrid()
    {
        int index = 0;

        for (int y = 0; y < _mapSize; y++)
        {
            for (int x = 0; x < _mapSize; x++)
            {
                MapGrid newGrid = Instantiate(_mapGridObject, new Vector3(x * _gridSpacing, 0, y * _gridSpacing), Quaternion.identity);

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

        StartCoroutine(CheckEntropy());
    }

    private IEnumerator CheckEntropy()
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

        yield return new WaitForSeconds(0.01f);

        CollapseGrid(_lowEntropyGrids);
    }

    private void CollapseGrid(List<MapGrid> collapseCandidatetGrids)
    {
        int randomGridIndex = UnityEngine.Random.Range(0, collapseCandidatetGrids.Count);

        MapGrid currentGrid = collapseCandidatetGrids[randomGridIndex];

        currentGrid.IsCollapsed = true;

        MapTile selectedTile = currentGrid.TileOptions[UnityEngine.Random.Range(0, currentGrid.TileOptions.Length)];
        currentGrid.TileOptions = new MapTile[] { selectedTile };

        _debugTileInspector.AddTile(
            Instantiate(selectedTile
            , currentGrid.transform.position + selectedTile.transform.position
            , selectedTile.transform.rotation)
            );

        UpdateGeneration(currentGrid, selectedTile);
    }

    private void UpdateGeneration(MapGrid currentGrid, MapTile seletedTile)
    {
        UpdateCollapsedNeighbors(currentGrid);

        for (int i = 0; i < _collapsedNeighbors.Length; i++)
        {
            if (_collapsedNeighbors[i] == null || _collapsedNeighbors[i].IsCollapsed)
            {
                continue;
            }

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
        if (_generationCount < _mapSizeSetting * _mapSizeSetting)
        {
            StartCoroutine(CheckEntropy());
        }
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

        newGrid.CreateMapGrid(true, new MapTile[] { _boundaryTile }, -1);
        Instantiate(_boundaryTile, newGrid.transform.position + dir * _gridSpacing / 2, rotation);

    }
}
