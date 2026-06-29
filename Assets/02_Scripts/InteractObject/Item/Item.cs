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
        // TODO(김익환, 26.06.27): 인벤토리 시스템 구현 후 아이템 획득 로직 추가
        GameManager.Pool.DespawnToPool(this.gameObject);
    }
}
