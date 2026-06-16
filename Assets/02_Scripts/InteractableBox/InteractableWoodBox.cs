using UnityEngine;

public class InteractableWoodBox : InteractableBox
{
    // 아이템 List에 존재하는 아이템들 낮은 아이템들 많이 나오도록 출력
    protected override string GradingItem()
    {
        if (_hadItemList == null || _hadItemList.Count == 0)
        {
            Debug.LogError("아이템 리스트가 비어 있습니다.");
            return default;
        }

        int count = _hadItemList.Count;

        int totalWeight = 0;

        for (int i = 0; i < count; i++)
        {
            totalWeight += count - i;
        }

        int rand = Random.Range(0, totalWeight);
        int current = 0;

        for (int i = 0; i < count; i++)
        {
            current += count - i;

            if (rand < current)
            {
                return _hadItemList[i];
            }
        }

        return _hadItemList[0];
    }
}
