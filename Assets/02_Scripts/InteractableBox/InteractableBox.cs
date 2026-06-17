using NUnit.Framework;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AddressableAssets;

public enum ItemRarity
{
    None,
    Consumable,
    Normal,
    Rare,
    Legendary
}

[System.Serializable]
public class RarityWeight
{
    public ItemRarity Rarity;
    public int Weight;
}

[System.Serializable]
public class BoxDropData
{
    public List<RarityWeight> RarityWeights = new();
}

public class InteractableBox : MonoBehaviour
{
    [Header("컴포넌트")]
    [SerializeField] private InteractableBoxAnimeController _animController;

    private string _interactableBoxName;
    private string _interactableBoxComment;
    private bool _isLocking;
    private string _meshPrefabPath;
    private Dictionary<ItemRarity, List<string>> _itemPoolByRarity = new Dictionary<ItemRarity, List<string>>();
    private BoxDropData _rarityRateData = new BoxDropData();

    private void OnEnable()
    {
        // TODO(안우재 2026-6-17) : 테스트 코드(addresasable 연동 확인)용 추후 삭제(데이터 클래스 작성 시)
        _meshPrefabPath = "Assets/03_Prefabs/Object/Mesh_IronBox_Prefab.prefab";

        // InitBox();
        SpawnMeshBox();
    }

    private void Start()
    {

    }

    // TODO(안우재 2026-6-15) : 매개변수로 어떠한 형식으로 데이터를 받아올지 확인 및 대입 필요
    private void InitBox(/*데이터 클래스 매개변수*/)
    {
        /*
        _interactableBoxName = 
        _interactableBoxComment = 
        _isLocking = 
        _meshPrefabPath = 
        InitItemList(데이터 클래스의 ItemIdList를 매개변수로 함)
        InitRarityRateData
        */
    }

    private void InitItemList(List<string> itemIdList)
    {
        // TODO(안우재 2026-6-17) : 아이템 등급, 종류에 따라 따로 _itemPoolByRarity에 할당
    }

    private void InitRarityRateData(List<int> rateList)
    {
        for (int i = 0; i < rateList.Count; i++)
        {
            ItemRarity rarity = (ItemRarity)(i + 1);

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

    public void PopUpInteractUI()
    {
        // TODO(안우재 2026-6-15) : UIManager의 PopUpUI를 꺼내와 해당장비의 "이름 [F]"이 가능하도록 추가
    }

    private string OpenBox()
    {
        string itemId = PickItemId();

        if (string.IsNullOrEmpty(itemId))
        {
            Debug.Log("아이템 뽑기 실패");
            return null;
        }

        return itemId;
    }

    private string PickItemId()
    {
        ItemRarity pickedRarity = PickRarity();

        if (pickedRarity == ItemRarity.None)
            return null;

        if (!_itemPoolByRarity.TryGetValue(pickedRarity, out List<string> itemIdList))
            return null;

        if (itemIdList == null || itemIdList.Count == 0)
            return null;

        int randomIndex = UnityEngine.Random.Range(0, itemIdList.Count);
        return itemIdList[randomIndex];
    }

    private ItemRarity PickRarity()
    {
        if (_rarityRateData == null)
            return ItemRarity.None;

        if (_rarityRateData.RarityWeights == null || _rarityRateData.RarityWeights.Count == 0)
            return ItemRarity.None;

        int totalWeight = 0;

        foreach (RarityWeight rarityWeight in _rarityRateData.RarityWeights)
        {
            if (rarityWeight.Weight <= 0)
                continue;

            totalWeight += rarityWeight.Weight;
        }

        if (totalWeight <= 0)
            return ItemRarity.None;

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

        return ItemRarity.None;
    }


    public void InteractCloserPlayer()
    {
        // TODO(안우재 2026-6-15) : Player 조준 시 띄울 HUD 제작 필요 및 적용 필요


    }

}

