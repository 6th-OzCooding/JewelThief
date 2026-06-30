using System.Collections.Generic;
using System.Net;
using TeamConvention.Interfaces;
using UnityEngine;

public abstract class BaseDisarmableObejct : MonoBehaviour, IInteractable, IDisarmable
{
    protected string _disarmObjId;
    protected string _disarmObjName;
    protected List<string> _requiredToolIdList = new();

    protected List<float> _timeReductionAmountList = new();

    protected bool _hasRequiresTool;
    protected bool _isDisarmed = false;
    protected bool _isInitialized = false;
    protected bool _isInteractable;

    protected Animator _animator;

    private void Awake()
    {
        if (null == _animator)
            TryGetComponent(out _animator);
    }

    /// <summary>
    /// 함정 제거시 구체화된 로직이 필요한 경우 override 해서 사용합니다.
    /// </summary>
    protected virtual void OnDisarm() { }
    protected virtual void OnDisarm(bool isCollectToolUse) { }

    /// <summary>
    /// 데이터 데이블에서 정보를 활용하여 초기화 하는 작업을 여기에 작성합니다.
    /// 또한 다른 초기화도 여기서 작업합니다.
    /// </summary>
    protected virtual void OnInitalized() 
    {
        // _isDisarmed = false;
        _isInitialized = false;
    }


    /// <summary>
    /// 애니메이션 추가 로직이 필요한 경우 override 해서 사용합니다.
    /// </summary>
    protected virtual void PlayInteractionAnimation()
    {
        if (_animator == null)
            return;

        _animator.SetTrigger("Interact");
    }


    /// <summary>
    /// id를 이용해 데이터 테이블에서 정보를 불러옵니다.
    /// ex) GameManager.DataTalbe.Get???Data(id);
    /// 해당 데이터로 초기화 하는 작업은 OnInitalized 함수에서 수행합니다.
    /// </summary>
    protected abstract void LoadData(string id);

    public bool IsDisarmed => _isDisarmed;

    public string GetId => _disarmObjId;

    public string GetName => _disarmObjName;

    public void InitFromSpawner(string id)
    {
        _disarmObjId = id;
        LoadData(id);
        OnInitalized();
        _isInitialized = true;
    }

    /// <summary>
    /// 상호작용 가능여부 체크 멤버변수 _isInteractable를 반환
    /// 상호작용이 불가능해지는 경우 _isInteractable를 false로 변환
    /// </summary>
    public bool CanInteract()
    {
        return _isInteractable;
    }

    public void Interact(IInteractor interactor)
    {
        if (!CanInteract())
            return;
        
        Disarm(interactor);
    }

    public bool CanDisarm()
    {
        if (_isDisarmed)
            return false;


        return true;
    }

    private void CheckRequireTools(IInteractor interactor)
    {
        _hasRequiresTool = false;

        if (interactor == null)
            return;

        if (_requiredToolIdList == null || _requiredToolIdList.Count == 0)
            return;

        if (_requiredToolIdList.Contains("None"))
            return;

        if (interactor is not IInventoryOwner inventoryOwner)
            return;

        _hasRequiresTool = inventoryOwner.TryUseSelectedTool(_requiredToolIdList, out _);
    }

    public void Disarm(IInteractor interactor)
    {
        if (CanDisarm())
        {
            CheckRequireTools(interactor);
            OnDisarm(_hasRequiresTool);
            return;
        }

        PlayInteractionAnimation();
        OnDisarm();
    }
}
