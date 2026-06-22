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
    private SpawnObjectType _interactableObjectType;
    private bool _isLocking;
    private string _meshPrefabPath;
    private Dictionary<ItemGrade, List<string>> _itemPoolByRarity = new Dictionary<ItemGrade, List<string>>();
    private BoxDropData _rarityRateData = new BoxDropData();
    private List<string> _itemList = new List<string>();
    private List<string> _spawnedRarityList = new List<string>();
    private int _maxSpawnItemCount;
    private int _spawnItemCount;
    private List<string> _spawnedItemList = new List<string>();

    public string Name => _interactableBoxDataId;
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
        _maxSpawnItemCount = data.MaxItemCount;
        InitObjectSpawnType(data.SpawnObjectTypeData);
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
        _itemList = itemList;
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

    private void PickItemId()
    {
        ItemGrade pickedRarity = PickRarity();

        if (pickedRarity == ItemGrade.None)
            return;

        InitSpawnedItemList(pickedRarity);
        if(_spawnedRarityList.Count == 0)
        {
            Debug.LogError("InteratableObject.cs 스크립트의 InitSpawnedItemList메서드 문제");
        }

        // Random.Range 특성 상 최댓값을 포함하지 않아 +1을 추가하였음
        int randomItemSpawnCount = UnityEngine.Random.Range(1, _maxSpawnItemCount + 1);
        _spawnedItemList.Clear();

        for (int i = 0; i < randomItemSpawnCount; i++)
        {
            int randomCount = UnityEngine.Random.Range(0, _spawnedRarityList.Count);
            _spawnedItemList.Add(_spawnedRarityList[randomCount]);
        }
    }

    private void InitSpawnedItemList(ItemGrade spanwAbleItemList)
    {
        _spawnedRarityList.Clear();
        if (spanwAbleItemList == ItemGrade.None) return;

        foreach(string checkItemDataId in _itemList)
        {
            if(GameManager.DataTable.GetItemData(checkItemDataId).CurrentItemGrade == spanwAbleItemList)
            {
                _spawnedRarityList.Add(checkItemDataId);
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


    private void OpenBox()
    {
        PickItemId();

        // TODO(안우재 2026-6-22) : _spawnedItemList에 있는 아이템들을 생성하는데 아이템들을 위로 발사(Impulse)하여 생성
        //                          아이템 갯수(_spawnedItemList.Count)에 따라 파티클이 다르게 해야함

        // foreach문 들어가기 전 PlayDropItemParicle(_spawnedItemList.Count) 로 파티클 실행
        // 아이템 풀에서 Active한 gameObject activedItem이 있다고 침
        // 생성 로직 후 ShootItem(activedItem)을 수행, 이걸 foreach안에서 수행

    }

    private void ShootItem(GameObject shootingObject)
    {
        if(shootingObject == null) return;

        // 튀어오르는 값 여기서 조절 가능
        float minImpulsePower = 3f;
        float maxImpulsePower = 7f;
        float sideRandomPower = 1.5f;

        shootingObject.TryGetComponent<Rigidbody>(out Rigidbody shootObjRigid);

        shootObjRigid.linearVelocity = Vector3.zero;
        shootObjRigid.angularVelocity = Vector3.zero;

        Vector3 randomDir = new Vector3(
            UnityEngine.Random.Range(-sideRandomPower, sideRandomPower),
            1f,
            UnityEngine.Random.Range(-sideRandomPower, sideRandomPower)
        ).normalized;

        float power = UnityEngine.Random.Range(minImpulsePower, maxImpulsePower);

        shootObjRigid.AddForce(randomDir * power, ForceMode.Impulse);
    }

    private void PlayDropItemParicle(int dropItemCount)
    {
        // 파티클을 데이터 드리븐할지, 아니면 직접할당할지, 아니면 Addressable로 생성할지 확인 필요
    }

    private void OpenLockedBox()
    {
        // TODO(안우재 2026-6-18) : 잠겨있는 오브젝트 잠금해제에 따른 행동 정의 필요

    }

    public void Interact(IInteractor interactor)
    {
        // TODO(안우재 2026-6-22) : Player와 상호작용 시 행동 정의 예시) OpenBox 메서드 호출, 아이템 발사 관련 등
        //                          잠겨있는지 아닌지도 판단하는 로직 추가해야함
        OpenBox();
    }

}

