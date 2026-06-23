using System.Collections.Generic;
using UnityEngine;
public enum ItemType
{
    None = 0,
    Potion,
    Tool,
    Jewel
}
public enum ItemGrade
{
    None = 0,
    Rare,
    Epic,
    Unique,
    Legendary
}
public class ItemBase : MonoBehaviour
{
    [SerializeField] private MeshFilter meshFilter;
    [SerializeField] private MeshRenderer meshRenderer;
    public string Id { get; private set; }
    public ItemType CurrentItemType { get; private set; }
    public ItemGrade CurrentItemGrade { get; private set; }

    public float Weight { get; private set; }
    public int Price { get; private set; }
    public string MeshPath;
    public List<string> MaterialPaths = new();


    public virtual void InitItem(ItemData data)
    {
        if (data == null)
        {
            Debug.LogError("데이터 없음");
            return;
        }
        Id = data.Id;
        CurrentItemType = data.CurrentItemType;
        CurrentItemGrade = data.CurrentItemGrade;
        Weight = data.Weight;
        Price = data.Price;
        MeshPath = data.MeshPath;
        MaterialPaths = data.MaterialPaths;
        if (!string.IsNullOrEmpty(MeshPath))
        {
            ChangeAppearance(MeshPath, MaterialPaths);//매쉬와 메테리얼 변경
        }
    }
    private void ChangeAppearance(string meshPath, List<string> materialPaths)
    {
        if (!string.IsNullOrEmpty(meshPath))
        {
            Mesh targetMesh = GameManager.Resource.GetLoadedAsset<Mesh>(meshPath);
            if (targetMesh != null)
            {
                meshFilter.sharedMesh = targetMesh;
            }
            else
            {
                Debug.LogError($"메쉬 로드 실패: {meshPath}");
            }
        }
        if (materialPaths != null && materialPaths.Count > 0)
        {
            Material[] targetMaterials = new Material[materialPaths.Count];

            for (int i = 0; i < materialPaths.Count; i++)
            {
                string path = materialPaths[i];
                Material material = GameManager.Resource.GetLoadedAsset<Material>(path);

                if (material != null)
                {
                    targetMaterials[i] = material;
                }
                else
                {
                    Debug.LogError($" 마테리얼 로드 실패: {path}");
                }
            }
            meshRenderer.sharedMaterials = targetMaterials;
        }
    }
}
