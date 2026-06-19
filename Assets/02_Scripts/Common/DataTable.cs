using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DataTable
{
    public Dictionary<string, PoolingObjectData> GetPoolingObjectDataTable() => PoolingObjectDataTable;
    public Dictionary<string, ItemData> GetItemDataTable() => ItemDataTable;
    public Dictionary<string, InventoryTypeData> GetInventoryTypeDataTable() => InventoryTypeDataTable;

    Dictionary<string, PoolingObjectData> PoolingObjectDataTable { get; set; } = new Dictionary<string, PoolingObjectData>();
    Dictionary<string, ItemData> ItemDataTable { get; set; } = new Dictionary<string, ItemData>();
    Dictionary<string, InventoryTypeData> InventoryTypeDataTable { get; set; } = new Dictionary<string, InventoryTypeData>();

    [Serializable]
    class SerializationWrapper<T>
    {
        public List<T> items;
    }

    
    public void LoadAllData()
    {
        //PoolingObjectDataTable = LoadData<PoolingObjectData>("PoolingObject");
        ItemDataTable = LoadData<ItemData>("ItemData");
        InventoryTypeDataTable = LoadData<InventoryTypeData>("InventoryTypeData");
    }

    #region Getters

    public PoolingObjectData GetPoolingObjectData(string id)
    {
        if (null == PoolingObjectDataTable || string.IsNullOrEmpty(id)) return null;

        return PoolingObjectDataTable.TryGetValue(id, out var data) ? data : null;
    }

    /// <summary>
    /// 아이템 기본 데이터를 반환합니다.
    /// </summary>
    public ItemData GetItemData(string id)
    {
        if (null == ItemDataTable || string.IsNullOrEmpty(id)) return null;

        return ItemDataTable.TryGetValue(id, out ItemData data) ? data : null;
    }

    /// <summary>
    /// 아이템의 인벤토리 보관 타입 데이터를 반환합니다.
    /// </summary>
    public InventoryTypeData GetInventoryTypeData(string id)
    {
        if (null == InventoryTypeDataTable || string.IsNullOrEmpty(id)) return null;

        return InventoryTypeDataTable.TryGetValue(id, out InventoryTypeData data) ? data : null;
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
                return wrapper.items
                    .Where(value => value != null && !string.IsNullOrEmpty(value.Id))
                    .ToDictionary(value => value.Id);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[{typeof(T).Name} JSON 로드 오류] {ex.Message}");
        }

        return new Dictionary<string, T>();
    }
}
