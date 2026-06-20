using System;
using System.Collections.Generic;

public enum SoundType
{
    BGM,
    SFX,
    Voice
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

public class InventoryTypeData : BaseData
{
    public string Name;
    public HoldType CurrentHoldType;
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
public class SoundData : BaseData
{
    public string Name;
    public float Volume;
    public SoundType SoundType;
    public bool IsLoop;
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