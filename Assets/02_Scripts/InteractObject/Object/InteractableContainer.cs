using System.Collections.Generic;
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

public class InteractableContainer : BaseDisarmableObejct
{
    [Header("컴포넌트")]
    [SerializeField] private InteractableContainerAnimeController _animController;

    private SpawnObjectType _interactableObjectType;
    private string _visualPrefabPath;
    private BoxDropData _rarityRateData = new BoxDropData();
    private List<string> _itemList = new List<string>();
    private List<string> _spawnedRarityList = new List<string>();
    private int _maxSpawnItemCount;
    private List<string> _spawnedItemList = new List<string>();

    private GameObject _meshObject;

    protected override void OnInitalized()
    {
        base.OnInitalized();
        SpawnMeshBox();
    }

    protected override void LoadData(string dataId)
    {
        InteractableContainerData data = GameManager.DataTable.GetInteractableContainerData(dataId);
        ApplyData(data);
    }

    private void ApplyData(InteractableContainerData data)
    {
        if (data == null) return;
        _disarmObjId = data.Id;
        _maxSpawnItemCount = data.MaxItemCount;
        _interactableObjectType = data.GetPopupType();
        _isDisarmed = data.IsContainerDisarm;
        _visualPrefabPath = data.ContainerMeshPrefabPath;
        InitStringListData(_itemList, data.ItemIdList);
        InitRarityRateData(data.RateList);
        InitFloatListData(_timeReductionAmountList, data.TimeReductionAmountList);
        InitStringListData(_requiredToolIdList, data.RequiresToolIdList);
        _isInteractable = true;
        _hasRequiresTool = false;
    }

    private void InitStringListData(List<string> requierInitList, List<string> loadDataList)
    {
        if (requierInitList == null || loadDataList == null) return;

        requierInitList.Clear();

        foreach (string data in loadDataList)
        {
            requierInitList.Add(data);
        }
    }

    private void InitFloatListData(List<float> requierInitList, List<float> loadDataList)
    {
        if (requierInitList == null || loadDataList == null) return;

        requierInitList.Clear();

        foreach (float data in loadDataList)
        {
            requierInitList.Add(data);
        }
    }

    private void InitRarityRateData(List<int> rateList)
    {
        if(rateList == null) return;

        _rarityRateData.RarityWeights.Clear();

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
        if (_visualPrefabPath == null || _visualPrefabPath == "")
        {
            Debug.LogError("Mesh 프리팹 경로 없음");
            return;
        }
           
        GameObject obj = await Addressables.InstantiateAsync(_visualPrefabPath).Task;
        if (obj == null) return;

        obj.transform.SetParent(transform, false);
        _meshObject = obj;
        _animController.InitMeshAnime(obj);
    }

    private void DestroyMeshBox()
    {
        Destroy(_meshObject);
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
        if(_itemList.Count == 0)
        {
            Debug.LogError("할당된 데이터 아이템 Id가 없습니다.");
        }

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

    private void ShootItem(GameObject shootingObject)
    {
        if(shootingObject == null || shootingObject.GetComponent<Rigidbody>() == null) return;

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

    private async void PlayDropItemParitcle(int dropItemCount)
    {
        // Addresabble로 파티클 구현 dropItemCount 1=Low, 2,3=normal / 4,5 = High
        if(dropItemCount <= 1)
        {
            GameObject effect = await Addressables.InstantiateAsync(
            "Prefabs/Paticle/Effect_Low",
            transform.position,
            Quaternion.identity
            ).Task;
        }
        else if(dropItemCount >= 2 && dropItemCount <= 3)
        {
            GameObject effect = await Addressables.InstantiateAsync(
            "Prefabs/Paticle/Effect_Normal",
            transform.position,
            Quaternion.identity
            ).Task;
        }
        else if(dropItemCount >=4)
        {
            GameObject effect = await Addressables.InstantiateAsync(
            "Prefabs/Paticle/Effect_High",
            transform.position,
            Quaternion.identity
            ).Task;
        }
    }

    private void OpenBox()
    {
        _animController.SetStat(BoxState.Open);
        PickItemId();
        PlayDropItemParitcle(_spawnedItemList.Count);

        foreach (string spawnItemId in _spawnedItemList)
        {
            // TODO(안우재 2026-6-24) : GameObejct 생성(pooling 구현 후 가능) 후 ShootItem() 을이용하여 발사
            /*
            ItemData itemData = GameManager.DataTable.GetItemData(spawnItemId);

            if (itemData == null)
            {
                Debug.LogError($"아이템 데이터 없음: {spawnItemId}");
                continue;
            }

            GameObject itemPrefab = GameManager.Resource.GetLoadedAsset<GameObject>(
                itemData.ItemPrefabPath // 실제 필드명으로 수정
            );

            if (itemPrefab == null)
            {
                Debug.LogError($"아이템 프리팹 로드 안 됨: {spawnItemId}");
                continue;
            }

            GameObject itemObj = Instantiate(
                itemPrefab,
                spawnPos,
                Quaternion.identity
            );

            ShootItem(itemObj);
            */
        }
    }

    // 안잠긴 것을 열 때 전용
    protected override void OnDisarm()
    {
        OpenBox();
        _isInteractable = false;
    }

    // 잠긴것을 풀때 전용
    protected override void OnDisarm(bool isCollectToolUse)
    {
        InteractableContainerData data = GameManager.DataTable.GetInteractableContainerData(_disarmObjId);
        if (isCollectToolUse)
        {
            ChangeStat(data.CollectOpenDataId);
        }
        else
        {
            // TODO(안우재 2026-6-24) : 강제로 열었기에 ChangeStat 전에 차감 시간을 적용해야함
            //                          시간 차감 로직 성준님께 여쭤보기
            ChangeStat(data.ForceOpenDataId);
        }
    }

    private void ChangeStat(string dataId)
    {
        InteractableContainerData data = GameManager.DataTable.GetInteractableContainerData(dataId);
        ApplyData(data);
        DestroyMeshBox();
        SpawnMeshBox();
    }
}

