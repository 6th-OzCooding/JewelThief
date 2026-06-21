using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;

public class CreditPopupUI : UIBase
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ClosePopup();
        }
    }

    private void ClosePopup()
    {
        UIManager.Instance.ClosePopupUI(UIType.CreditPopup);
    }
}
