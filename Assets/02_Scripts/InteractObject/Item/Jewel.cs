using TeamConvention.Interfaces;
using UnityEngine;

public class Jewel : BaseInteractableObject
{
    private ItemData _itemData;

    public ItemData Data
    {
        get { return _itemData; }
    }

    public float Weight { get; private set; }
    public int Price { get; private set; }

    [SerializeField] private MeshFilter _meshFilter;
    [SerializeField] private MeshRenderer _meshRenderer;
    [SerializeField] private MeshCollider _meshCollider;
    public ItemGrade ItemGrade { get; private set; }

    protected override void OnInitalized()
    {
        _objectId = _itemData.Id;
        _objectName = _itemData.Name;
        ItemGrade = _itemData.GetItemGrade();

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
        return true;
    }

    protected override void LoadData(string id)
    {
        _itemData = GameManager.DataTable.GetItemData(id);
    }

    protected override void OnInteract(IInteractor interactor)
    {
        if (interactor is not IInventoryOwner inventoryOwner) return;

        HoldType holdType = _itemData.GetHoldType();

        if (holdType == HoldType.Pocket)
        {
            if (JewelInventoryManager.Instance == null) return;

            if (!JewelInventoryManager.Instance.CanPickupJewel(_itemData)) return;
        }

        // 월드 보석 획득도 PlayerInventory를 먼저 통과해 가방 용량/무게 상태에 반영합니다.
        if (!inventoryOwner.TryAcquireItem(_itemData, holdType)) return;

        if (holdType == HoldType.Hold)
        {
            GameManager.Pool.DespawnToPool(this.gameObject);
            return;
        }

        JewelInventoryManager.Instance.AddJewelToTempQueue(this);
    }
}
