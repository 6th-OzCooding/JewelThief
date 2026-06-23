using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DataTable
{
    public Dictionary<string, PreLoadAssetData> GetPreLoadAssetDataTable() => _preLoadAssetDataTable;
    public Dictionary<string, PoolingObjectData> GetPoolingObjectDataTable() => _poolingObjectDataTable;
    public Dictionary<string, InteractableContainerData> GetInteractableContainerDataTable() => _interactableContainerDataTable;
    public Dictionary<string, InventoryTypeData> GetInventoryTypeDataTable() => _inventoryTypeDataTable;
    public Dictionary<string, ItemData> GetItemDataTable() => _itemDataTable;
    public Dictionary<string, SoundData> GetSoundDataTable() => _soundDataTable;
    public Dictionary<string, StageData> GetStageDataTable() => _stageDataTable;


    private Dictionary<string, PreLoadAssetData> _preLoadAssetDataTable { get; set; } = new();
    private Dictionary<string, PoolingObjectData> _poolingObjectDataTable { get; set; } = new();
    private Dictionary<string, InteractableContainerData> _interactableContainerDataTable { get; set; } = new();
    private Dictionary<string, InventoryTypeData> _inventoryTypeDataTable { get; set; } = new();
    private Dictionary<string, ItemData> _itemDataTable { get; set; } = new();
    private Dictionary<string, SoundData> _soundDataTable { get; set; } = new();
    private Dictionary<string, StageData> _stageDataTable { get; set; } = new();

    [Serializable]
    class SerializationWrapper<T>
    {
        public List<T> items;
    }

    
    public void LoadAllData()
    {
        _preLoadAssetDataTable = LoadData<PreLoadAssetData>("PreLoadAsset");
        // PoolingObjectDataTable = LoadData<PoolingObjectData>("PoolingObject");
        _interactableContainerDataTable = LoadData<InteractableContainerData>("InteractableContainer");
        _itemDataTable = LoadData<ItemData>("ItemData");

        // TODO (김경훈 - 06.20: 아이템 데이터로 통합 후 삭제)
        _inventoryTypeDataTable = LoadData<InventoryTypeData>("InventoryTypeData");

        _soundDataTable = LoadData<SoundData>("SoundData");
        _stageDataTable = LoadData<StageData>("StageData");
    }

    #region Getters

    public PreLoadAssetData GetPreLoadAssetData(string id)
    {
        if (null == _preLoadAssetDataTable || string.IsNullOrEmpty(id)) return null;
        return _preLoadAssetDataTable.TryGetValue(id, out var data) ? data : null;
    }

    public PoolingObjectData GetPoolingObjectData(string id)
    {
        if (null == _poolingObjectDataTable || string.IsNullOrEmpty(id)) return null;

        return _poolingObjectDataTable.TryGetValue(id, out var data) ? data : null;
    }

    public InteractableContainerData GetInteractableObjectData(string id)
    {
        if (null == _interactableContainerDataTable || string.IsNullOrEmpty(id)) return null;

        return _interactableContainerDataTable.TryGetValue(id, out var data) ? data : null;
    }

    public ItemData GetItemData(string id)
    {
        if (null == _itemDataTable || string.IsNullOrEmpty(id)) return null;
        return _itemDataTable.TryGetValue(id, out var data) ? data : null;
    }

    // TODO (김경훈 - 06.20: 아이템 데이터로 통합 후 삭제)
    public InventoryTypeData GetInventoryTypeData(string id)
    {
        if (null == _inventoryTypeDataTable || string.IsNullOrEmpty(id)) return null;
        return _inventoryTypeDataTable.TryGetValue(id, out var data) ? data : null;
    }

    public SoundData GetSoundData(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        return _soundDataTable.TryGetValue(id, out var data) ? data : null;
    }

    public StageData GetStageData(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        return _stageDataTable.TryGetValue(id, out var data) ? data : null;
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
