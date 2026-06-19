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

/// <summary>
/// 아이템을 플레이어가 어떤 방식으로 들고 다니는지 구분합니다.
/// </summary>
public enum HoldType
{
    None = 0,
    Pocket,
    Hold
}

/// <summary>
/// 아이템별 인벤토리 보관 타입 데이터입니다.
/// </summary>
[Serializable]
public class InventoryTypeData : BaseData
{
    public string CurrentHoldType;

    /// <summary>
    /// 문자열로 로드된 보관 타입을 HoldType enum으로 변환합니다.
    /// </summary>
    public HoldType GetHoldType()
    {
        if (Enum.TryParse(CurrentHoldType, true, out HoldType holdType))
            return holdType;

        return HoldType.None;
    }
}
