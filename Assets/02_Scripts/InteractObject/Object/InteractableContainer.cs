using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

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
    [SerializeField] private AudioSource _audioSource;

    private SpawnObjectType _interactableObjectType;
    private string _visualPrefabPath;
    private BoxDropData _rarityRateData = new BoxDropData();
    private List<string> _itemList = new List<string>();
    private List<string> _spawnedRarityList = new List<string>();
    private int _maxSpawnItemCount;
    private List<string> _spawnedItemList = new List<string>();

    private GameObject _meshObject;

    private void OnDisable()
    {
        Destroy(_meshObject);
    }

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
        if (data == null)
        {
            Debug.LogError("InteractableContainerData가 없습니다.");
            return;
        }

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

    private void SpawnMeshBox()
    {
        if (string.IsNullOrEmpty(_visualPrefabPath))
        {
            Debug.LogError("Visual 프리팹 경로 없음");
            return;
        }

        GameObject prefab = GameManager.Resource.GetLoadedAsset<GameObject>(_visualPrefabPath);

        if (prefab == null)
        {
            Debug.LogError($"Visual 프리팹이 ResourceManager에 로드되어 있지 않습니다: {_visualPrefabPath}");
            return;
        }

        GameObject obj = Instantiate(prefab, transform);

        obj.transform.localPosition = prefab.transform.position;
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localScale = Vector3.one;

        _meshObject = obj;

        if (_animController != null)
        {
            _animController.InitMeshAnime(obj);
            _audioSource = _meshObject.GetComponentInChildren<AudioSource>();
        }
    }

    private void DestroyMeshBox()
    {
        if (_meshObject == null)
            return;

        Destroy(_meshObject);
        _meshObject = null;
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
            return;
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

        foreach (string checkItemDataId in _itemList)
        {
            ItemData itemData = GameManager.DataTable.GetItemData(checkItemDataId);

            if (itemData == null)
            {
                Debug.LogError($"존재하지 않는 ItemData입니다 : {checkItemDataId}");
                continue;
            }

            if (itemData.GetItemGrade() == spanwAbleItemList)
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
        if (shootingObject == null)
            return;

        if (!shootingObject.TryGetComponent(out Rigidbody shootObjRigid))
            return;

        // 튀어오르는 값 여기서 조절 가능
        float minImpulsePower = 3f;
        float maxImpulsePower = 7f;
        float sideRandomPower = 1.5f;

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

    private void PlayDropItemParticle(int dropItemCount)
    {
        string poolId = GetDropParticlePoolId(dropItemCount);

        if (string.IsNullOrEmpty(poolId))
            return;

        GameManager.Pool.SpawnFromPool(
            poolId,
            transform.position,
            Quaternion.identity
        );
    }

    private string GetDropParticlePoolId(int dropItemCount)
    {
        if (dropItemCount <= 1)
            return "Effect_Low";

        if (dropItemCount <= 3)
            return "Effect_Normal";

        return "Effect_High";
    }

    private void OpenBox()
    {
        _animController.SetStat(InteractableObjectAnimState.Open);
        PickItemId();
        PlayDropItemParticle(_spawnedItemList.Count);

        foreach (string spawnItemId in _spawnedItemList)
        {
            ItemData itemData = GameManager.DataTable.GetItemData(spawnItemId);

            if (itemData == null)
            {
                Debug.LogError($"아이템 데이터 없음: {spawnItemId}");
                continue;
            }

            GameObject spawnedObject = SpawnItemObjectFromPool(itemData, spawnItemId);

            if (spawnedObject == null)
                continue;

            ShootItem(spawnedObject);
        }
        _audioSource.Play();
    }

    private GameObject SpawnItemObjectFromPool(ItemData itemData, string itemId)
    {
        string poolId = GetItemPoolId(itemData);

        if (string.IsNullOrEmpty(poolId))
            return null;

        GameObject spawnedObject = GameManager.Pool.SpawnFromPool(
            poolId,
            transform.position + Vector3.up * 0.7f,
            Quaternion.identity
        );

        spawnedObject.TryGetComponent<BaseInteractableObject>(out BaseInteractableObject spawnInteractableObject);
        if(spawnInteractableObject == null) 
        {
            Debug.LogError($"{poolId} 프리팹에 BaseInteractableObject를 상속한 컴포넌트가 없습니다.");
            GameManager.Pool.DespawnToPool(spawnedObject);
            return null;
        }

        spawnInteractableObject.InitFromSpawner(itemId);
        
        return spawnedObject;
    }

    private string GetItemPoolId(ItemData itemData)
    {
        switch (itemData.GetItemType()) 
        {
            case ItemType.Jewel:
                return "ItemObject";

            case ItemType.Tool:
                return "Pool_Tool";

            case ItemType.Junk:
                return "ItemObject";

            default:
                Debug.LogError($"지원하지 않는 아이템 타입입니다.");
                return null;
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
            // 시간 감소 리스트의 2번째가 줄어드는 것이므로 1로 설정
            GameManager.Alert.ReduceTimer(_timeReductionAmountList[1]);
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
