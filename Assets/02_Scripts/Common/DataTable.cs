using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DataTable
{
    public Dictionary<string, PreLoadAssetData> GetPreLoadAssetDataTable() => _preLoadAssetDataTable;
    public Dictionary<string, PoolData> GetPoolDataTable() => _poolDataTable;
    public Dictionary<string, ItemData> GetItemDataTable() => _itemDataTable;
    public Dictionary<string, StageData> GetStageDataTable() => _stageDataTable;
    public Dictionary<string, MapSpawnData> GetMapSpawnDataTable() => _mapSpawnDataTable;

    private Dictionary<string, PreLoadAssetData> _preLoadAssetDataTable { get; set; } = new();
    private Dictionary<string, PoolData> _poolDataTable { get; set; } = new();
    private Dictionary<string, InteractableContainerData> _interactableContainerDataTable { get; set; } = new();
    private Dictionary<string, ItemData> _itemDataTable { get; set; } = new();
    private Dictionary<string, Door> _doorDataTable { get; set; } = new();
    private Dictionary<string, PopupViewData> _popupViewDataTable { get; set; } = new();
    private Dictionary<string, SoundData> _soundDataTable { get; set; } = new();
    private Dictionary<string, StageData> _stageDataTable { get; set; } = new();
    private Dictionary<string, EnemyData> _enemyDataTable { get; set; } = new();
    private Dictionary<string, MapSpawnData> _mapSpawnDataTable { get; set; } = new();
    private Dictionary<string, TrapData> _trapDataTable { get; set; } = new();
    private Dictionary<string, StageEnemyData> _stageEnemyDataTable { get; set; } = new();

    [Serializable]
    class SerializationWrapper<T>
    {
        public List<T> items;
    }


    public void LoadAllData()
    {
        _preLoadAssetDataTable = LoadData<PreLoadAssetData>("PreLoadAsset");
        _poolDataTable = LoadData<PoolData>("Pool");
        _interactableContainerDataTable = LoadData<InteractableContainerData>("InteractableContainer");
        _doorDataTable = LoadData<Door>("Door");
        _itemDataTable = LoadData<ItemData>("ItemData");
        _popupViewDataTable = LoadData<PopupViewData>("PopupViewData");
        _soundDataTable = LoadData<SoundData>("SoundData");
        _stageDataTable = LoadData<StageData>("StageData");
        _enemyDataTable = LoadData<EnemyData>("Enemy");
        _mapSpawnDataTable = LoadData<MapSpawnData>("MapSpawn");
        _trapDataTable = LoadData<TrapData>("Trap");
        _stageEnemyDataTable = LoadData<StageEnemyData>("StageEnemyData");
    }

    #region Getters

    public PreLoadAssetData GetPreLoadAssetData(string id)
    {
        if (null == _preLoadAssetDataTable || string.IsNullOrEmpty(id)) return null;
        return _preLoadAssetDataTable.TryGetValue(id, out var data) ? data : null;
    }

    public PoolData GetPoolData(string id)
    {
        if (null == _poolDataTable || string.IsNullOrEmpty(id)) return null;

        return _poolDataTable.TryGetValue(id, out var data) ? data : null;
    }

    public InteractableContainerData GetInteractableContainerData(string id)
    {
        if (null == _interactableContainerDataTable || string.IsNullOrEmpty(id)) return null;

        return _interactableContainerDataTable.TryGetValue(id, out var data) ? data : null;
    }

    public Door GetDoorData(string id)
    {
        if (null == _doorDataTable || string.IsNullOrEmpty(id)) return null;

        return _doorDataTable.TryGetValue(id, out var data) ? data : null;
    }

    public ItemData GetItemData(string id)
    {
        if (null == _itemDataTable || string.IsNullOrEmpty(id)) return null;
        return _itemDataTable.TryGetValue(id, out var data) ? data : null;
    }

    public PopupViewData GetPopupViewData(string id)
    {
        if (null == _popupViewDataTable || string.IsNullOrEmpty(id)) return null;
        return _popupViewDataTable.TryGetValue(id, out var data) ? data : null;
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

    public EnemyData GetEnemyData(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        return _enemyDataTable.TryGetValue(id, out var data) ? data : null;
    }

    public MapSpawnData GetMapSpawnData(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        return _mapSpawnDataTable.TryGetValue(id, out var data) ? data : null;
    }

    public TrapData GetTrapData(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        return _trapDataTable.TryGetValue(id, out var data) ? data : null;
    }
    public StageEnemyData GetStageEnemyData(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        return _stageEnemyDataTable.TryGetValue(id, out var data) ? data : null;
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
