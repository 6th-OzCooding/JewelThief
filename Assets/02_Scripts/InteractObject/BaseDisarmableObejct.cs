using System.Collections.Generic;
using TeamConvention.Interfaces;
using UnityEngine;

public abstract class BaseDisarmableObejct : MonoBehaviour, IInteractable, IDisarmable
{
    protected string _disarmObjId;
    protected string _disarmObjName;
    protected List<string> _requiredToolIdList;

    protected List<float> _timeReductionAmountList;

    protected bool _requiresTool;
    protected bool _isDisarmed = false;
    protected bool _isInitialized = false;

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


    /// <summary>
    /// 데이터 데이블에서 정보를 활용하여 초기화 하는 작업을 여기에 작성합니다.
    /// 또한 다른 초기화도 여기서 작업합니다.
    /// </summary>
    protected virtual void OnInitalized()
    {
        _isDisarmed = false;
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

    public bool CanInteract()
    {
        return CanDisarm();
    }

    public void Interact(IInteractor interactor)
    {
        Disarm(interactor);
    }

    public bool CanDisarm()
    {
        if (_isDisarmed)
            return false;

        if (_requiresTool)
        {
            // 도구가 필요한 경우 도구가 있는지 체크하는 로직 작성
            // ex) inventory.HasToolForDisarming(_requiredToolId); // 이경우 _requiredToolId에 해당하는 도구가 있는지 체크
        }

        return true;
    }

    public void Disarm(IInteractor interactor)
    {
        if (!CanDisarm())
            return;

        _isDisarmed = true;
        PlayInteractionAnimation();
        OnDisarm();
    }
}