using System.Net;
using TeamConvention.Interfaces;
using UnityEngine;

public class Tool : BaseInteractableObject
{
    private ItemData _itemData;
    public int ChargeCount { get; set; }

    [SerializeField] private MeshRenderer _meshRenderer;
    [SerializeField] private MeshFilter _meshFilter;

    protected override void OnInitalized()
    {
        _objectId = _itemData.Id;
        _objectName = _itemData.Name;
        ChargeCount = _itemData.ChargeCount;

        _meshFilter.sharedMesh = GameManager.Resource.GetLoadedAsset<Mesh>(_itemData.MeshPath);

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
        // 무언가를 해제하는 용도인데, 이는 사용부에서 조건 처리를 하고 있음.
        // 만약 그렇지 않다면 추가 작업 필요
    }
}
