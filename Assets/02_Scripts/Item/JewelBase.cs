using UnityEngine;

public class JewelBase : ItemBase
{
    public override void InitItem(ItemData data)
    {
        if (data == null)
        {
            Debug.LogError("데이터 없음");
            return;
        }
        if (data is JewelData jewelData)
        {
            base.InitItem(data);
        }
        else 
        {
            Debug.LogError("jewelData데이터가 아님");
            return;
        }
    }
    
}
