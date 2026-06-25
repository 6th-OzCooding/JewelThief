using System.Net;
using TeamConvention.Interfaces;
using UnityEngine;

public class Tool : BaseInteractableObject
{
    private ItemData _itemData;
    public int ChargeCount { get; set; }

    [SerializeField] private MeshRenderer _meshRenderer;
    [SerializeField] private MeshFilter _meshFilter;
    [SerializeField] private MeshCollider _meshCollider;

    protected override void OnInitalized()
    {
        _objectId = _itemData.Id;
        _objectName = _itemData.Name;
        ChargeCount = _itemData.ChargeCount;

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
        return ChargeCount > 0;
    }

    protected override void LoadData(string id)
    {
        _itemData = GameManager.DataTable.GetItemData(id);
    }

    protected override void OnInteract(IInteractor interactor)
    {
        ChargeCount--;
        if(ChargeCount <= 0)
        {
            if(interactor is IInventoryOwner inventoryOwner)
            {
                inventoryOwner.ClearHandItem(PlayerHandType.Right);
                GameManager.Pool.DespawnToPool(this.gameObject);

            }
        }
    }
}
