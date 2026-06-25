using TeamConvention.Interfaces;
using UnityEngine;

public class Jewel : BaseInteractableObject
{
    private ItemData _itemData;
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
        if(interactor is IInventoryOwner inventoryOwner)
        {
            inventoryOwner.TryAcquireItem(_itemData, HoldType.Pocket);
            GameManager.Pool.DespawnToPool(this.gameObject);
        }
    }
}
