using TeamConvention.Interfaces;
using UnityEngine;

public class Escape : BaseInteractableObject
{
    public ItemData Data
    {
        get { return _itemData; }
    }

    [SerializeField] private MeshFilter _meshFilter;
    [SerializeField] private MeshRenderer _meshRenderer;
    [SerializeField] private MeshCollider _meshCollider;

    private ItemData _itemData;


    protected override void OnInitalized()
    {
        base.OnInitalized();

        _objectId = _itemData.Id;
        _objectName = _itemData.Name;

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
        //return GameManager.Alert.GetRemainingTime() <= 0f;
    }

    protected override void LoadData(string id) 
    {
        _itemData = GameManager.DataTable.GetItemData(id);
    }

    protected override void OnInteract(IInteractor interactor)
    {
        GameManager.Instance.Escape();
    }
}
