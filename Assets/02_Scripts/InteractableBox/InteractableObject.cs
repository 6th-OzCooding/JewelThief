using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using TeamConvention.Interfaces;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.AddressableAssets;

public enum SpawnObjectType
{
    None,
    Cabinet,
    LockBox,
    WoodBox
}

[System.Serializable]
public class RarityWeight
{
    public ItemGrade Rarity;
    public int Weight;
}

[System.Serializable]
public class BoxDropData
{
    public List<RarityWeight> RarityWeights = new();
}

public class InteractableObject : MonoBehaviour, IInteractable //IDisarmable
{
    [Header("컴포넌트")]
    [SerializeField] private InteractableBoxAnimeController _animController;

    private string _interactableBoxDataId;
    private string _interactableName;
    private SpawnObjectType _interactableObjectType;
    private string _interactableBoxComment;
    private bool _isLocking;
    private string _meshPrefabPath;
    private Dictionary<ItemGrade, List<string>> _itemPoolByRarity = new Dictionary<ItemGrade, List<string>>();
    private BoxDropData _rarityRateData = new BoxDropData();
    private List<string> _ItemList = new List<string>();
    private List<string> _spawnedList = new List<string>();

    public string Name => _interactableName;
    public bool CanInteract() => !_isLocking;

    private void OnEnable()
    {
        // 초기화 부분 테스트
        InitBox("Object_03");
        SpawnMeshBox();
    }

    private void Start()
    {
        // TODO(안우재 2026-6-17) : 테스트 코드 스폰 매니저 또는 게임매니저에 의해 생겨날 경우 삭제 필요
        InitBox("Object_03");
        SpawnMeshBox();
    }


    public void InitBox(string dataId)
    {
        InteractableObjectData data = GameManager.DataTable.GetInteractableObjectData(dataId);
        _interactableBoxDataId = data.Id;
        InitObjectSpawnType(data.SpawnObjectTypeData);
        _interactableName = data.ObjName;
        _interactableBoxComment = data.ObjectComment;
        _isLocking = data.IsLock;
        _meshPrefabPath = data.ObjMeshPrefabPath;
        InitItemList(data.ItemIdList);
        InitRarityRateData(data.RateList);
    }

    private void InitObjectSpawnType(string typeStr)
    {
        if (Enum.TryParse<SpawnObjectType>(typeStr, out SpawnObjectType returnObjType))
        {
            _interactableObjectType = returnObjType;
        }
        else
        {
            Debug.LogError("잘못된 소환 형식");
            _interactableObjectType = SpawnObjectType.None;
        }
    }

    private void InitItemList(List<string> itemList)
    {
        if (itemList == null) return;
        _ItemList = itemList;
    }

    private void InitRarityRateData(List<int> rateList)
    {
        if(rateList == null) return;
        for (int i = 0; i < rateList.Count; i++)
        {
            ItemGrade rarity = (ItemGrade)(i + 1);

            _rarityRateData.RarityWeights.Add(new RarityWeight
            {
                Rarity = rarity,
                Weight = rateList[i]
            });
        }
    }

    private async void SpawnMeshBox()
    {
        if (_meshPrefabPath == null || _meshPrefabPath == "")
        {
            Debug.LogError("Mesh 프리팹 경로 없음");
            return;
        }
           
        GameObject obj = await Addressables.InstantiateAsync(_meshPrefabPath).Task;
        if (obj == null) return;
        obj.transform.SetParent(transform, false);

        _animController.InitMeshAnime(obj);
    }

    private string OpenBox()
    {
        if(_isLocking)
        {

            return string.Empty;
        }

        string itemId = PickItemId();

        if (string.IsNullOrEmpty(itemId))
        {
            Debug.Log("아이템 뽑기 실패");
            return null;
        }

        return itemId;
    }

    private void OpenLockedBox()
    {
        // TODO(안우재 2026-6-18) : 잠겨있는경우 도구를 사용할건지 안할건지 확인하는 단계 또는 기타 행동 들어가야함

    }

    private string PickItemId()
    {
        ItemGrade pickedRarity = PickRarity();

        if (pickedRarity == ItemGrade.None)
            return null;

        InitSpawnedItemList(pickedRarity);
        if(_spawnedList.Count == 0)
        {
            Debug.LogError("InteratableObject.cs 스크립트의 InitSpawnedItemList메서드 문제");
        }

        int randomIndex = UnityEngine.Random.Range(0, _spawnedList.Count);
        return _spawnedList[randomIndex];
    }

    private void InitSpawnedItemList(ItemGrade spanwAbleItemList)
    {
        _spawnedList.Clear();
        if (spanwAbleItemList == ItemGrade.None) return;

        foreach(string checkItemDataId in _ItemList)
        {
            if(GameManager.DataTable.GetItemData(checkItemDataId).CurrentItemGrade == spanwAbleItemList)
            {
                _spawnedList.Add(checkItemDataId);
            }
        }
    }

    private ItemGrade PickRarity()
    {
        if (_rarityRateData == null)
            return ItemGrade.None;

        if (_rarityRateData.RarityWeights == null || _rarityRateData.RarityWeights.Count == 0)
            return ItemGrade.None;

        int totalWeight = 0;

        foreach (RarityWeight rarityWeight in _rarityRateData.RarityWeights)
        {
            if (rarityWeight.Weight <= 0)
                continue;

            totalWeight += rarityWeight.Weight;
        }

        if (totalWeight <= 0)
            return ItemGrade.None;

        int randomValue = UnityEngine.Random.Range(0, totalWeight);

        int currentWeight = 0;

        foreach (RarityWeight rarityWeight in _rarityRateData.RarityWeights)
        {
            if (rarityWeight.Weight <= 0)
                continue;

            currentWeight += rarityWeight.Weight;

            if (randomValue < currentWeight)
                return rarityWeight.Rarity;
        }

        return ItemGrade.None;
    }


    public void Interact(IInteractor interactor)
    {
        // TODO(안우재 2026-6-22) : Player와 상호작용 시 행동 정의 예시) OpenBox 메서드 호출 등


    }

}

