using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DataTable
{
    public Dictionary<string, PoolingObjectData> GetPoolingObjectDataTable() => PoolingObjectDataTable;
    public Dictionary<string, InteractableObject> GetInteractableObjectDataTable() => InteractableObjectDataTable;
    public Dictionary<string, InventoryTypeData> GetInventoryTypeDataTable() => InventoryTypeDataTable;
    public Dictionary<string, ItemData> GetItemDataTable() => ItemDataTable;
    public Dictionary<string, SoundData> GetSoundDataTable() => SoundDataTable;
    public Dictionary<string, StageData> GetStageDataTable() => StageDataTable;

    Dictionary<string, PoolingObjectData> PoolingObjectDataTable { get; set; } = new();
    Dictionary<string, InteractableObject> InteractableObjectDataTable { get; set; } = new();
    Dictionary<string, InventoryTypeData> InventoryTypeDataTable { get; set; } = new();
    Dictionary<string, ItemData> ItemDataTable { get; set; } = new();
    Dictionary<string, SoundData> SoundDataTable { get; set; } = new();
    Dictionary<string, StageData> StageDataTable { get; set; } = new();

    [Serializable]
    class SerializationWrapper<T>
    {
        public List<T> items;
    }

    
    public void LoadAllData()
    {
        // PoolingObjectDataTable = LoadData<PoolingObjectData>("PoolingObject");
        InteractableObjectDataTable = LoadData<InteractableObject>("InteractableObject");
        ItemDataTable = LoadData<ItemData>("ItemData");

        // TODO (김경훈 - 06.20: 아이템 데이터로 통합 후 삭제)
        InventoryTypeDataTable = LoadData<InventoryTypeData>("InventoryTypeData");

        SoundDataTable = LoadData<SoundData>("SoundData");
        StageDataTable = LoadData<StageData>("StageData");
    }

    #region Getters

    public PoolingObjectData GetPoolingObjectData(string id)
    {
        if (null == PoolingObjectDataTable || string.IsNullOrEmpty(id)) return null;

        return PoolingObjectDataTable.TryGetValue(id, out var data) ? data : null;
    }

    public InteractableObject GetInteractableObjectData(string id)
    {
        if (null == InteractableObjectDataTable || string.IsNullOrEmpty(id)) return null;

        return InteractableObjectDataTable.TryGetValue(id, out var data) ? data : null;
    }

    public ItemData GetItemData(string id)
    {
        if (null == ItemDataTable || string.IsNullOrEmpty(id)) return null;
        return ItemDataTable.TryGetValue(id, out var data) ? data : null;
    }

    // TODO (김경훈 - 06.20: 아이템 데이터로 통합 후 삭제)
    public InventoryTypeData GetInventoryTypeData(string id)
    {
        if (null == InventoryTypeDataTable || string.IsNullOrEmpty(id)) return null;
        return InventoryTypeDataTable.TryGetValue(id, out var data) ? data : null;
    }

    public SoundData GetSoundData(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        return SoundDataTable.TryGetValue(id, out var data) ? data : null;
    }

    public StageData GetStageData(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        return StageDataTable.TryGetValue(id, out var data) ? data : null;
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
