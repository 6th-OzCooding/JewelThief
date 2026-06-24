using UnityEngine;
public enum ItemType 
{
    None =0,
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
    public string Id { get; private set; }
    public ItemType CurrentItemType { get; private set; }
    public ItemGrade CurrentItemGrade { get; private set; }
   
    public float Weight { get; private set; }
    public int Price { get; private set; }
    
    public virtual void  InitItem(ItemData data) 
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
    }
}
