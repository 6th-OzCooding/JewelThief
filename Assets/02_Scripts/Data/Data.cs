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
