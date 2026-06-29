using TeamConvention.Interfaces;
using UnityEngine;

public class Statue : BaseInteractableObject
{
    private ItemData _itemData;

    public ItemData Data
    {
        get { return _itemData; }
    }

    public float Weight { get; private set; }
    public int Price { get; private set; }
    public ItemGrade ItemGrade { get; private set; }

    [SerializeField] private MeshFilter _meshFilter;
    [SerializeField] private MeshRenderer _meshRenderer;
    [SerializeField] private MeshCollider _meshCollider;

    protected override void OnInitalized()
    {
        _objectId = _itemData.Id;
        _objectName = _itemData.Name;

        Weight = _itemData.Weight;
        Price = _itemData.Price;
        ItemGrade = _itemData.GetItemGrade();

        _meshFilter.sharedMesh = GameManager.Resource.GetLoadedAsset<Mesh>(_itemData.MeshPath);
        // _meshCollider.sharedMesh = _meshFilter.sharedMesh;
        BoxCollider boxCollider = gameObject.GetComponent<BoxCollider>();
        if (boxCollider == null)
        {
            boxCollider = gameObject.AddComponent<BoxCollider>();
        }

        if (_meshFilter.sharedMesh != null)
        {
            boxCollider.center = _meshFilter.sharedMesh.bounds.center;
            boxCollider.size = _meshFilter.sharedMesh.bounds.size;
        }

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

        // 조각상은 Hold 아이템으로 취급하므로 획득 가능 여부와 손 장착은 PlayerInventory가 판단합니다.
        if (!inventoryOwner.TryAcquireItem(_itemData, _itemData.GetHoldType())) return;

        GameManager.Pool.DespawnToPool(this.gameObject);
    }
}
