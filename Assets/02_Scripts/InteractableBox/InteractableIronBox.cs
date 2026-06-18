using UnityEngine;

public class InteractableIronBox : InteractableBox
{
    // TODO(안우재 2026-6-15) : 매개변수로 어떠한 형식으로 데이터를 받아올지 확인 및 대입 필요
    public override void InitBox()
    {
        base.InitBox();
        _isLocking = true;
    }

    // 아이템 List에 존재하는 아이템들 더 높은 단계 잘나오도록 추출(LcokPick으로 상자 오픈 시)
    private string UseLockPickGradingItem()
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
            totalWeight += i + 1;
        }

        int rand = Random.Range(0, totalWeight);
        int current = 0;

        for (int i = 0; i < count; i++)
        {
            current += i + 1;

            if (rand < current)
            {
                return _hadItemList[i];
            }
        }

        return _hadItemList[count - 1];
    }
}
