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

    private List<MapGrid> _grids;

    private int _count = 0;

    private void Awake()
    {
        _grids = new();

        InitGrid();
    }

    private void InitGrid()
    {
        for (int y = 0; y < _mapSize; y++)
        {
            for (int x = 0; x < _mapSize; x++)
            {
                MapGrid newGrid = Instantiate(_mapGridObject, new Vector3(x * _gridSpacing, 0, y * _gridSpacing), Quaternion.identity);
                newGrid.CreateMapGrid(false, _tileObjects);
                _grids.Add(newGrid);
            }
        }

        StartCoroutine(CheckEntropy());
    }

    private IEnumerator CheckEntropy()
    {
        List<MapGrid> lowEntropyGrids = new(_grids);

        lowEntropyGrids.RemoveAll(grid => grid.IsCollapsed);
        lowEntropyGrids.Sort((a, b) => a.TileOptions.Length - b.TileOptions.Length);
        lowEntropyGrids.RemoveAll(grid => grid.TileOptions.Length != lowEntropyGrids[0].TileOptions.Length);

        yield return new WaitForSeconds(0.01f);

        CollapseGrid(lowEntropyGrids);
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
        List<MapGrid> neighborsGrid = GetNeighbors(currentGrid);

        for (int i = 0; i < neighborsGrid.Count; i++)
        {
            if (neighborsGrid[i] == null || neighborsGrid[i].IsCollapsed)
            {
                continue;
            }

            MapTile[] updatedOptions = null;
            if (i == 0)         // 위
            {
                updatedOptions = CheckValidation(neighborsGrid[i].TileOptions, seletedTile.GetUpTiles);
            }
            else if (i == 1)    // 아래
            {
                updatedOptions = CheckValidation(neighborsGrid[i].TileOptions, seletedTile.GetDownTiles);
            }
            else if (i == 2)    // 오른쪽
            {
                updatedOptions = CheckValidation(neighborsGrid[i].TileOptions, seletedTile.GetRightTiles);
            }
            else if (i == 3)    // 왼쪽
            {
                updatedOptions = CheckValidation(neighborsGrid[i].TileOptions, seletedTile.GetLeftTiles);
            }

            neighborsGrid[i].SetTileOptions(updatedOptions);
        }

        _count++;
        if (_count < _mapSize * _mapSize)
        {
            StartCoroutine(CheckEntropy());
        }
    }

    // 위, 아래, 오른쪽, 왼쪽 순서로 반환.
    private List<MapGrid> GetNeighbors(MapGrid currentGrid)
    {
        List<MapGrid> neighbors = new();

        // O(n)의 시간, 최적화를 위해선 데이터 관리 방식을 바꿔야함.
        int currentGridIndex = Array.IndexOf(_grids.ToArray(), currentGrid);

        int upIndex = (currentGridIndex + _mapSize < _grids.Count) ? currentGridIndex + _mapSize : -1;
        int downIndex = (currentGridIndex - _mapSize >= 0) ? currentGridIndex - _mapSize : -1;
        int rightIndex = (currentGridIndex % _mapSize < _mapSize - 1) ? currentGridIndex + 1 : -1;
        int leftIndex = (currentGridIndex % _mapSize > 0) ? currentGridIndex - 1 : -1;

        neighbors.Add(upIndex != -1 ? _grids[upIndex] : null);
        neighbors.Add(downIndex != -1 ? _grids[downIndex] : null);
        neighbors.Add(rightIndex != -1 ? _grids[rightIndex] : null);
        neighbors.Add(leftIndex != -1 ? _grids[leftIndex] : null);

        return neighbors;
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
