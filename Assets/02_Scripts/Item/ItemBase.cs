using UnityEngine;
public enum ItemType 
{
    None =0,
    Consumeable,
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
    public string ItemId { get; private set; }
    public float Weight { get; private set; }
    public int Price { get; private set; }
    public ItemType Type { get; private set; }
    public ItemGrade Grade { get; private set; }
}
