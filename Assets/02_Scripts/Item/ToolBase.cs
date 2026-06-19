using UnityEngine;

public class ToolBase : ItemBase
{
    public int Durability { get; private set; }
    public int CurrentDurability { get; private set; }

    public override void InitItem(ItemData data)
    {
        if (data == null) {
            Debug.LogError("데이터 없음");
            return;
        }
       
        if (data is ToolData toolData)
        { 
            base.InitItem(data);
            Durability = toolData.Durability;
            CurrentDurability = Durability;
        }
        else
        {
            Debug.LogError("toolData데이터가 아님");
            return;
        }
    }
    public void UseTool(int amount) 
    {
        if (CurrentDurability <= 0) return;

        int newDurability = CurrentDurability - amount;

        if (newDurability > 0) 
        {
        CurrentDurability = newDurability;
        }
        else 
        { 
            CurrentDurability = 0;
            DestroyTool();
        }
    }
    public void DestroyTool() { }
}
