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
    Tool
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
public class PoolingObjectData : BaseData
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
    public ItemType ItemType;
    public ItemGrade ItemGrade;
    public float Weight;
    public int Price;
    public string IconPath;
    public string MeshPath;
    public List<string> MaterialPaths = new List<string>();
    public int ChargeCount;
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
public class SoundData : BaseData
{
    public string Name;
    public float Volume;
    public string SoundType;
    public bool IsLoop;

    public global::SoundType GetSoundType()   // 추가: 사용처에서 enum으로 변환해서 사용 (필드명과 타입명이 같아 global:: 명시)
    => Enum.TryParse<global::SoundType>(SoundType, out var result) ? result : global::SoundType.SFX;
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
}

[Serializable]
public class PopupViewData : BaseData
{
    public string PopupType;
    public string DefaultPrompt;
    public string LockedPrompt;
    public string MasterKeyPrompt;
    public string MasterKeyLimitPrompt;
    public string OverweightPrompt;
    public string NotEnoughMoneyPrompt;
    public string PurchaseSuccessPrompt;

    public global::PopupType GetPopupType()
        => Enum.TryParse<global::PopupType>(PopupType, out var result) ? result : global::PopupType.Simple;
}