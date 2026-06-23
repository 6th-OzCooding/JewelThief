using System;
using TeamConvention.Interfaces;
using UnityEngine;

/// <summary>
/// 손에 든 아이템을 즉시 골드로 환전하는 판매소(돈 세탁기)
/// 데이터 테이블 의존 없이 로비에 고정 배치되는 독립 오브젝트
/// </summary>
public class MoneyLaundry : MonoBehaviour, IInteractable
{
    public string GetId => "MoneyLaundry";
    public string GetName => "돈 세탁기";

    public static event Action OnSellRequested;
    public bool CanInteract() => true;

    public void Interact(IInteractor interactor)
    {
        OnSellRequested?.Invoke();
    }
}