using System;

[Serializable]
public class BaseData
{
    public string Id;
}

[Serializable]
public class PoolingObjectData : BaseData
{
    public int InitSize;
}
[Serializable]
public class ItemData : BaseData 
{
    public string Name;
    public string Description;
    public ItemType CurrentItemType;
    public ItemGrade CurrentItemGrade;
    public float Weight;
    public int Price;
    public string IconPath;
    public string PrefabPath;
}
[Serializable]
public class ToolData : ItemData 
{
    public int Durability;
}
[Serializable]
public class PotionData : ItemData
{
    public BuffType CurrentBuffType;
    public float Value;
    public float Duration;
}
[Serializable]
public class JewelData : ItemData
{
    
}