using TeamConvention.Interfaces;
using UnityEngine;

public abstract class BaseInteractableObject : MonoBehaviour, IInteractable
{
    protected string _objectId;
    protected string _objectName;
    protected string _objectComment;

    private bool _isInitialized = false;
    private bool _isKinematic = false;

    private Animator _animator;

    private void Awake()
    {
        if (null == _animator)
            TryGetComponent(out _animator);
    }


    /// <summary>
    /// 애니메이션 구체화된 로직이 필요한 경우 override 해서 사용합니다.
    /// </summary>
    protected virtual void PlayInteractionAnimation()
    {
        if (_animator == null)
            return;

        _animator.SetTrigger("Interact");
    }



    /// <summary>
    /// 데이터 데이블에서 정보를 활용하여 초기화 하는 작업을 여기에 작성합니다.
    /// 또한 다른 초기화도 여기서 작업합니다.
    /// </summary>
    protected virtual void OnInitalized() 
    {
        _isInitialized = false;

        if (_isKinematic)
        {
            GetComponent<Rigidbody>().isKinematic = _isKinematic;
        }
    }


    /// <summary>
    /// 상호작용이 가능한 상태인지 체크하는 로직을 여기에 작성합니다.
    /// </summary>
    protected abstract bool CheckCanInteract();


    /// <summary>
    /// 상호작용시 실행되는 로직을 여기에 작성합니다.
    /// </summary>
    protected abstract void OnInteract(IInteractor interactor);


    /// <summary>
    /// id를 이용해 데이터 테이블에서 정보를 불러옵니다.
    /// ex) GameManager.DataTalbe.Get???Data(id);
    /// 해당 데이터로 초기화 하는 작업은 OnInitalized 함수에서 수행합니다.
    /// </summary>
    protected abstract void LoadData(string id);



    public string GetId => _objectId;

    public string GetName => _objectName;

    public void InitFromSpawner(string id, bool isKinematic = false)
    {
        _objectId = id;
        _isKinematic = isKinematic;
        LoadData(id);
        OnInitalized();
        _isInitialized = true;
    }

    public bool CanInteract()
    {
        return CheckCanInteract();
    }

    public void Interact(IInteractor interactor)
    {
        if (!CanInteract()) return;
        OnInteract(interactor);
        PlayInteractionAnimation();
    }
}
