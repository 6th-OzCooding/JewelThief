using System;
using System.Collections.Generic;
using UnityEngine;


public class PoolManager
{
    Dictionary<string, Queue<GameObject>> objectPools = new();
    Dictionary<string, List<GameObject>> activedObjects = new();
    private Transform _poolRoot;

    public void Init(Transform poolRoot)
    {
        _poolRoot = poolRoot;
        var poolDataTable = GameManager.DataTable.GetPoolDataTable();

        foreach (string poolId in poolDataTable.Keys)
        {
            objectPools.Add(poolId, new Queue<GameObject>());

            var gameObject = LoadGameObject(poolId);

            for (int i = 0; i < poolDataTable[poolId].InitSize; i++)
            {
                CreateNewObject(poolId, gameObject);
            }
        }
    }

    public GameObject SpawnFromPool(string poolId, Vector3 position)
        => GetFromPool(poolId, position, Quaternion.identity);
    public GameObject SpawnFromPool(string poolId, Vector3 position, Quaternion rotation)
        => GetFromPool(poolId, position, rotation);

    public GameObject SpawnFromPool(string pooId, Vector3 position, Quaternion rotation, Transform parent)
        => GetFromPool(pooId, position, rotation, parent);

    public GameObject SpawnFromPool(string pooId, Transform parent, bool orginTF)
        => GetFromPool(pooId, Vector3.zero, Quaternion.identity, parent, orginTF);

    public T SpawnFromPool<T>(string poolId, Vector3 position) where T : Component
    {
        GameObject obj = GetFromPool(poolId, position, Quaternion.identity);
        if (obj.TryGetComponent<T>(out T component))
            return component;
        else
            throw new Exception($"Pool Id({poolId})에 올바른 컴포넌트가 존재하지 않습니다. component: {typeof(T)}.");
    }
    public T SpawnFromPool<T>(string poolId, Vector3 position, Quaternion rotation) where T : Component
    {
        GameObject obj = GetFromPool(poolId, position, rotation);
        if (obj.TryGetComponent<T>(out T component))
            return component;
        else
            throw new Exception($"Pool Id({poolId})에 올바른 컴포넌트가 존재하지 않습니다. component: {typeof(T)}.");
    }

    public void DespawnToPool(GameObject obj)
    {
        objectPools[obj.name].Enqueue(obj);
        activedObjects[obj.name].Remove(obj);

        obj.SetActive(false);
    }

    public void AllDespawnToPool()
    {
        foreach (var activedList in activedObjects.Values)
        {
            for (int i = activedList.Count - 1; i >= 0; i--)
            {
                DespawnToPool(activedList[i]);
            }
        }

        activedObjects.Clear();
    }

    private GameObject GetFromPool(string poolId, Vector3 position, Quaternion rotation, Transform parent = null, bool orginTF = false)
    {
        if (!objectPools.ContainsKey(poolId))
            throw new Exception($"Pool Id({poolId})에 해당하는 풀은 존재하지 않습니다.");

        Queue<GameObject> poolQueue = objectPools[poolId];
        if (poolQueue.Count <= 0)
        {
            CreateNewObject(poolId, LoadGameObject(poolId));
        }

        GameObject obj = poolQueue.Dequeue();

        if (!orginTF)
        {
            obj.transform.position = position;
            obj.transform.rotation = rotation;
        }

        if (null != parent)
        {
            obj.transform.SetParent(parent, false);
        }
        obj.gameObject.SetActive(true);

        if (!activedObjects.ContainsKey(poolId))
        {
            activedObjects.Add(poolId, new());
        }

        activedObjects[poolId].Add(obj);

        return obj;
    }

    private GameObject CreateNewObject(string poolId, GameObject gameObject)
    {
        var obj = GameObject.Instantiate(gameObject, _poolRoot);
        obj.name = poolId;
        objectPools[poolId].Enqueue(obj);
        obj.SetActive(false);

        return obj;
    }

    private GameObject LoadGameObject(string address)
    {
        GameObject gameObject = GameManager.Resource.GetLoadedAsset<GameObject>(address);

        if (null == gameObject)
        {
            // 그냥 프리펩 로드
            Debug.Log($"Pool Address({address})에 에셋이 ResourceManager에서 로드되어 있지 않습니다.\n Resources 로드를 합니다.");
            gameObject = Resources.Load<GameObject>($"Poo/{address}");
        }
        else
        {
            Debug.Log($"Pool Address({address})에 에셋이 ResourceManager에서 로드되어 있습니다.");
        }

        return gameObject;
    }
}
