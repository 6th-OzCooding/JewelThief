using System.Collections.Generic;
using TeamConvention.Interfaces;
using UnityEngine;
using UnityEngine.AddressableAssets;

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

public class InteractableBox : MonoBehaviour, IInteractable //IDisarmable
{
    [Header("컴포넌트")]
    [SerializeField] private InteractableBoxAnimeController _animController;

    private string _interactableBoxDataId;
    private string _interactableName;
    private string _interactableBoxComment;
    private bool _isLocking;
    private string _meshPrefabPath;
    private Dictionary<ItemGrade, List<string>> _itemPoolByRarity = new Dictionary<ItemGrade, List<string>>();
    private BoxDropData _rarityRateData = new BoxDropData();

    public string Name => _interactableName;
    public bool CanInteract() => !_isLocking;

    private void OnEnable()
    {
        // TODO(안우재 2026-6-17) : 테스트 코드(addresasable 연동 확인)용 추후 삭제(데이터 클래스 작성 시)
        // _meshPrefabPath = "Assets/03_Prefabs/Object/Mesh_IronBox_Prefab.prefab";
        
        // 초기화 부분
        // InitBox("Object_03");
        // SpawnMeshBox();
    }

    private void Start()
    {
        // TODO(안우재 2026-6-17) : 테스트 코드 스폰 매니저 또는 게임매니저에 의해 생겨날 경우 삭제 필요
        InitBox("Object_03");
        SpawnMeshBox();
    }


    // TODO(안우재 2026-6-15) : 매개변수로 어떠한 형식으로 데이터를 받아올지 확인 및 대입 필요
    private void InitBox(string dataId)
    {
        InteractableObject data = GameManager.DataTable.GetInteractableObjectData(dataId);
        _interactableBoxDataId = data.Id;
        _interactableName = data.ObjName;
        _interactableBoxComment = data.ObjectComment;
        _isLocking = data.IsLock;
        _meshPrefabPath = data.ObjMeshPrefabPath;
        // InitItemList(데이터 클래스의 ItemIdList를 매개변수로 함)
        // InitRarityRateData
    }

    private void InitItemList(List<string> itemIdList)
    {
        // TODO(안우재 2026-6-17) : 아이템 등급, 종류에 따라 따로 _itemPoolByRarity에 할당
    }

    private void InitRarityRateData(List<int> rateList)
    {
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

        if (!_itemPoolByRarity.TryGetValue(pickedRarity, out List<string> itemIdList))
            return null;

        if (itemIdList == null || itemIdList.Count == 0)
            return null;

        int randomIndex = UnityEngine.Random.Range(0, itemIdList.Count);
        return itemIdList[randomIndex];
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
        // TODO(안우재 2026-6-15) : Player 조준 시 띄울 HUD 제작 필요 및 적용 필요


    }

}

