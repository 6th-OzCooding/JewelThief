using System;
using System.Collections.Generic;

public enum SoundType
{
    BGM,
    SFX,
    Voice
}

public enum PopupType
{
    None = 0,
    Simple,
    ItemInfo,
    ShopInfo
}

public enum PopupTargetType
{
    None = 0,
    Item,
    Box,
    Trap,
    EscapePath,
    Door,
    Tool,
    StageSelectChair,
    Washer
}

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

[Serializable]
public class BaseData
{
    public string Id;
}

[Serializable]
public class PoolData : BaseData
{
    public int InitSize;
}

[Serializable]
public class PreLoadAssetData : BaseData
{
    public string Address;
    public string AssetType;
}

[Serializable]
public class ItemData : BaseData 
{
    public string Name;
    public string Description;
    public string StringItemType;
    public string StringItemGrade;
    public string StringHoldType;
    public float Weight;
    public int Price;
    public string IconPath;
    public string MeshPath;
    public List<string> MaterialPaths = new List<string>();
    public string MeshCollider;
    public int ChargeCount;
    public string Husks;

    public ItemGrade GetItemGrade()
        => Enum.TryParse<ItemGrade>(StringItemGrade, out var result) ? result : ItemGrade.None;
    public ItemType GetItemType()
        => Enum.TryParse<ItemType>(StringItemType, out var result) ? result : ItemType.None;
}

[Serializable]
public class InventoryTypeData : BaseData
{
    public string Name;
    public string CurrentHoldType;

    public HoldType GetHoldType()
        => Enum.TryParse<HoldType>(CurrentHoldType, out var result) ? result : HoldType.None;
}

[Serializable]
public class ToolData : ItemData 
{
    public int Durability;
}

[Serializable]
public class JewelData : ItemData
{
    
}

[Serializable]
public class InteractableContainerData : BaseData
{
    public string ContainerName;
    public string SpawnContainerTypeData;
    public string ContainerComment;
    public bool IsContainerDisarm;
    public List<string> RequiresToolIdList;
    public string CollectOpenDataId;
    public string ForceOpenDataId;
    public List<float> TimeReductionAmountList;
    public List<string> ItemIdList;
    public List<int> RateList;
    public int MaxItemCount;
    public string ContainerMeshPrefabPath;

    public SpawnObjectType GetPopupType()
        => Enum.TryParse<SpawnObjectType>(SpawnContainerTypeData, out var result) ? result : SpawnObjectType.None;
}

[Serializable]
public class Door : BaseData
{
    public string DoorName;
    public string DoorComment;
    public List<string> DoorRequiresToolIdList;
    public List<float> DoorTimeReductionAmountList;
    public bool IsDisarm;
    public string DoorMeshPrefabPath;
}

[Serializable]
public class PopupViewData : BaseData
{
    public string StringPopupType;
    public string DefaultPrompt;
    public string LockedPrompt;
    public string MasterKeyPrompt;
    public string MasterKeyLimitPrompt;
    public string OverweightPrompt;
    public string NotEnoughMoneyPrompt;
    public string PurchaseSuccessPrompt;

    public PopupType GetPopupType()
        => Enum.TryParse<PopupType>(StringPopupType, out var result) ? result : PopupType.Simple;
}

[Serializable]
public class SoundData : BaseData
{
    public string Name;
    public float Volume;
    public string StringSoundType;
    public bool IsLoop;

    public SoundType GetSoundType()
    => Enum.TryParse<SoundType>(StringSoundType, out var result) ? result : SoundType.SFX;
}

[Serializable]
public class StageData : BaseData
{
    public string Name;
    public int TimeLimit;
    public int MaxObject;
    public int MaxTrap;
    public int ExitCount;
    public int EnemyId;
    public List<string> TileAddress;
}

[Serializable]
public class EnemyData : BaseData
{
    public string Name;
    public float ViewRadius;
    public float ViewAngle;
    public float ViewHeight;
    public float DetectRadius;
    public float WalkSpeed;
    public float RunSpeed;
    public float AttackRadius;
    public float MinApproachDistance;
    public float AttackDelay;
    public float AttackDamage;
    public string PrefabAddress;
}