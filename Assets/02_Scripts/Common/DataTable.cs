using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DataTable
{
    public Dictionary<string, PoolingObjectData> GetPoolingObjectDataTable() => PoolingObjectDataTable;
    public Dictionary<string, InteractableObject> GetPoolingInteractableObjectDataTable() => PoolingInteractableObjectDataTable;
    public Dictionary<string, InventoryTypeData> GetPoolingInventoryTypeDataTable() => PoolingInventoryTypeDataTable;
    public Dictionary<string, ItemData> GetPoolingItemDataTable() => PoolingItemDataTable;

    Dictionary<string, PoolingObjectData> PoolingObjectDataTable { get; set; } = new();
    Dictionary<string, InteractableObject> PoolingInteractableObjectDataTable { get; set; } = new();
    Dictionary<string, InventoryTypeData> PoolingInventoryTypeDataTable { get; set; } = new();
    Dictionary<string, ItemData> PoolingItemDataTable { get; set; } = new();

    [Serializable]
    class SerializationWrapper<T>
    {
        public List<T> items;
    }

    
    public void LoadAllData()
    {
        // PoolingObjectDataTable = LoadData<PoolingObjectData>("PoolingObject");
        PoolingInteractableObjectDataTable = LoadData<InteractableObject>("InteractableObject");
        PoolingItemDataTable = LoadData<ItemData>("ItemData");

        // TODO: ItemData와 통합 후 삭제 예정
        PoolingInventoryTypeDataTable = LoadData<InventoryTypeData>("InventoryTypeData");
        
    }

    #region Getters

    public PoolingObjectData GetPoolingObjectData(string id)
    {
        if (null == PoolingObjectDataTable || string.IsNullOrEmpty(id)) return null;

        return PoolingObjectDataTable.TryGetValue(id, out var data) ? data : null;
    }

    public InteractableObject GetPoolingInteractableObjectData(string id)
    {
        if (null == PoolingObjectDataTable || string.IsNullOrEmpty(id)) return null;

        return PoolingInteractableObjectDataTable.TryGetValue(id, out var data) ? data : null;
    }

    #endregion

    Dictionary<string, T> LoadData<T>(string tableNmae) where T : BaseData
    {
        string resourcePath = $"JsonOutput/{tableNmae}";
        TextAsset textAsset = Utils.ResourcesLoad<TextAsset>(resourcePath);
        if (null == textAsset)
        {
            Debug.LogError($"리소스를 찾을 수 없습니다: Resources/{resourcePath}");
            return new Dictionary<string, T>();
        }

        try
        {
            string jsonString = textAsset.text;

            string wrappedJson = "{\"items\":" + jsonString + "}";

            SerializationWrapper<T> wrapper = JsonUtility.FromJson<SerializationWrapper<T>>(wrappedJson);

            if (wrapper == null || wrapper.items == null)
            {
                Debug.LogError($"[{typeof(T).Name}] JSON 파싱 결과가 비어 있습니다.");
            }

            if (null != wrapper && null != wrapper.items)
            {
                Debug.Log($"{typeof(T).Name} 데이터를 {wrapper.items.Count}개 로드했습니다.");
                return wrapper.items.ToDictionary(value => value.Id.ToString());
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[{typeof(T).Name} JSON 로드 오류] {ex.Message}");
        }

        return new Dictionary<string, T>();
    }
}
