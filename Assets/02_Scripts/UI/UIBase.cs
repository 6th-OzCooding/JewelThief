using UnityEngine;

/// <summary>
/// 모든 UI 프리팹이 공통으로 상속받는 기본 UI 클래스입니다.
/// </summary>
public class UIBase : MonoBehaviour
{
    private bool _isInitialized;

    /// <summary>
    /// UI가 처음 생성된 뒤 한 번만 초기화합니다.
    /// </summary>
    public void Initialize()
    {
        if (_isInitialized)
            return;

        _isInitialized = true;
        OnInitialize();
    }

    /// <summary>
    /// UI를 화면에 표시합니다.
    /// </summary>
    public void Open()
    {
        gameObject.SetActive(true);
        OnOpen();
    }

    /// <summary>
    /// UI를 화면에서 숨깁니다.
    /// </summary>
    public void Close()
    {
        OnClose();
        gameObject.SetActive(false);
    }

    /// <summary>
    /// UI별 초기화 코드가 필요할 때 자식 클래스에서 재정의합니다.
    /// </summary>
    protected virtual void OnInitialize()
    {
    }

    /// <summary>
    /// UI가 열릴 때 실행할 코드가 필요할 때 자식 클래스에서 재정의합니다.
    /// </summary>
    protected virtual void OnOpen()
    {
    }

    /// <summary>
    /// UI가 닫힐 때 실행할 코드가 필요할 때 자식 클래스에서 재정의합니다.
    /// </summary>
    protected virtual void OnClose()
    {
    }
}
