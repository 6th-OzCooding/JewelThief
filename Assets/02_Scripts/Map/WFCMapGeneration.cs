using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WFCMapGeneration : MonoBehaviour
{
    [Header("Map Generation Settings")]
    [SerializeField] private float _gridSpacing = 1f;
    [SerializeField] private int _mapSize = 10;

    [Header("Objects")]
    [SerializeField] private MapTile[] _tileObjects;
    [SerializeField] private MapGrid _mapGridObject;
    [SerializeField] private MapTile _backUpTile;

    // cache
    private List<MapGrid> _grids;

    // Buffer를 만들어 재활용
    private readonly List<MapGrid> _lowEntropyGrids = new();
    private readonly MapGrid[] _collapsedNeighbors = new MapGrid[4];

    private int _generationCount = 0;

    private void Awake()
    {
        _grids = new();

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
                newGrid.CreateMapGrid(false, _tileObjects, index);
                _grids.Add(newGrid);

                index++;
            }
        }

        StartCoroutine(CheckEntropy());
    }

    private IEnumerator CheckEntropy()
    {
        _lowEntropyGrids.Clear();

        int lowestEntropy = int.MaxValue;

        for(int i = 0; i < _grids.Count; i++)
        {
            MapGrid grid = _grids[i];

            if (_grids[i].IsCollapsed)
            {
                continue;
            }

            int optionCount = grid.TileOptions.Length;

            if(optionCount == 0)
            {
                grid.SetTileOptions(new MapTile[] { _backUpTile });
                optionCount = 1;
            }

            if (optionCount < lowestEntropy)
            {
                lowestEntropy = optionCount;
                _lowEntropyGrids.Clear();
                _lowEntropyGrids.Add(grid);
            }
            else if(optionCount == lowestEntropy)
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

        Instantiate(selectedTile, currentGrid.transform.position, selectedTile.transform.rotation);

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
        if (_generationCount < _mapSize * _mapSize)
        {
            StartCoroutine(CheckEntropy());
        }
    }

    // 위, 아래, 오른쪽, 왼쪽 순서로 반환.
    private void UpdateCollapsedNeighbors(MapGrid currentGrid)
    {
        int currentGridIndex = currentGrid.Index;

        int upIndex = (currentGridIndex + _mapSize < _grids.Count) ? currentGridIndex + _mapSize : -1;
        int downIndex = (currentGridIndex - _mapSize >= 0) ? currentGridIndex - _mapSize : -1;
        int rightIndex = (currentGridIndex % _mapSize < _mapSize - 1) ? currentGridIndex + 1 : -1;
        int leftIndex = (currentGridIndex % _mapSize > 0) ? currentGridIndex - 1 : -1;

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
}
