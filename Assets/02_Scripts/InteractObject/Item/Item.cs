using TeamConvention.Interfaces;
using UnityEngine;

public class Item : BaseInteractableObject
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
        base.OnInitalized();

        _objectId = _itemData.Id;
        _objectName = _itemData.Name;

        Weight = _itemData.Weight;
        Price = _itemData.Price;
        ItemGrade = _itemData.GetItemGrade();

        _meshFilter.sharedMesh = GameManager.Resource.GetLoadedAsset<Mesh>(_itemData.MeshPath);
        if (_itemData.MeshCollider != "")
            _meshCollider.sharedMesh = GameManager.Resource.GetLoadedAsset<Mesh>(_itemData.MeshCollider);
        else
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
        if (interactor is not IInventoryOwner inventoryOwner)
        {
            Debug.LogError($"{_itemData.Name}을(를) 획득할 수 없습니다. 상호작용 대상이 인벤토리를 가지고 있지 않습니다.");
            return;
        }

        if (!inventoryOwner.TryAcquireItem(_itemData, _itemData.GetHoldType()))
        {
            return;
        }

        GameManager.Pool.DespawnToPool(this.gameObject);
    }
}
