using NUnit.Framework;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class InteractableBox : MonoBehaviour
{
    protected string _instId;
    protected string _interactableBoxName;
    protected List<string> _hadItemList = new List<string>();
    protected bool _isLocking;

    // TODO(안우재 2026-6-15) : 매개변수로 어떠한 형식으로 데이터를 받아올지 확인 및 대입 필요
    public virtual void InitBox()
    {
        _isLocking = false;
    }

    protected void PopUpInteractUI()
    {
        // TODO(안우재 2026-6-15) : UIManager의 PopUpUI를 꺼내와 해당장비의 "이름 [F]"이 가능하도록 추가
    }

    protected virtual string OpenBox()
    {
        string returnItemId = "";

        // TODO(안우재 2026-6-15) : 상자 오픈 시 아이템 할당 로직 추가 필요
        if(!_isLocking)
        {
            // TODO(안우재 2026-6-15) : 상자 오픈 시 아이템 할당 로직 추가 필요
            return returnItemId;
        }

        return returnItemId;
    }

    // 아이템 List에 존재하는 아이템들 균등하게 추출
    protected virtual string GradingItem()
    {
        if (_hadItemList == null || _hadItemList.Count == 0)
        {
            Debug.LogError("아이템 리스트가 비어 있습니다.");
            return default;
        }

        int randomIndex = Random.Range(0, _hadItemList.Count);

        return _hadItemList[randomIndex];
    }

    public void InteractCloserPlayer()
    {
        // TODO(안우재 2026-6-15) : Player와 가까이 왔을 시 상호작용(HUD 띄우기) 함수 등록
        // 추가로 Collider로 Player를 구분할것인지, 거리에 따라 구분할 것인지, 플레이어의 시야 정 중앙일때 일지
        // 정해야함

    }
}
