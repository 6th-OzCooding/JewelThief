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
    public string MeshPath;
    public List<string> MaterialPaths = new List<string>();
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

[Serializable]
public class InteractableObject : BaseData
{
    public string ObjName;
    public string ObjectComment;
    public bool IsLock;
    public List<string> ItemIdList;
    public List<int> RateList;
    public string ObjMeshPrefabPath;
}

[Serializable]
public class Door : BaseData
{
    public string DoorName;
    public string DoorComment;
    public bool IsLock;
    public List<string> ItemIdList;
    public List<int> RateList;
    public string DoorMeshPrefabPath;
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
