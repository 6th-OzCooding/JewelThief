using UnityEngine;
public enum BuffType 
{
    None =0,
    Hp,
    Speed,
    Attack,
    Stealth
}
public class PotionBase : ItemBase
{
    public BuffType CurrentBuffType { get; private set; }
    public float Value { get; private set; }
    public float Duration { get; private set; }
    public override void InitItem(ItemData data)
    {
        if (data == null)
        {
            Debug.LogError("데이터 없음");
            return;
        }

        if (data is PotionData potionData) 
        {   
            base.InitItem(data);
            CurrentBuffType= potionData.CurrentBuffType;
            Value = potionData.Value;
            Duration = potionData.Duration;
        }
        else
        {
            Debug.LogError("potionData데이터가 아님");
            return;
        }
    }
}
