using TeamConvention.Interfaces;
using UnityEngine;

public class ShopItemDisplay : BaseInteractableObject
{
    private ItemData _itemData;

    [SerializeField] private MeshFilter _meshFilter;
    [SerializeField] private MeshRenderer _meshRenderer;
    [SerializeField] private MeshCollider _meshCollider;

    protected override void LoadData(string id)
    {
        _itemData = GameManager.DataTable.GetItemData(id);
    }

    protected override void OnInitalized()
    {
        _objectId = _itemData.Id;
        _objectName = _itemData.Name;

        _meshFilter.sharedMesh = GameManager.Resource.GetLoadedAsset<Mesh>(_itemData.MeshPath);
        _meshCollider.sharedMesh = _meshFilter.sharedMesh;

        var materialPath = _itemData.MaterialPaths;
        Material[] materials = new Material[materialPath.Count];
        for (int i = 0; i < materialPath.Count; i++)
        {
            var material = GameManager.Resource.GetLoadedAsset<Material>(materialPath[i]);
            materials[i] = material;
        }

        _meshRenderer.sharedMaterials = materials;
    }

    protected override bool CheckCanInteract()
    {
        return _itemData != null;
    }

    protected override void OnInteract(IInteractor interactor)
    {
        if (interactor is not IInventoryOwner inventoryOwner)
        {
            Debug.LogWarning("인벤토리가 누락되었습니다.");
            return;
        }

        bool isPurchased = GameManager.Shop.TryBuyItem(inventoryOwner, _objectId);

        if (!isPurchased)
        {
            GameManager.Sound.PlaySFX(SoundId.SFX_Error01);
            Debug.Log($"{_objectName} 구매에 실패했습니다. (골드 부족 또는 습득 실패)");
            return;
        }

        GameManager.Sound.PlaySFX(SoundId.SFX_Gain01);
        Debug.Log($"{_objectName}을(를) 구매했습니다. 남은 골드: {GameManager.Instance.Gold}");
    }
}