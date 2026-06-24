using System.Collections.Generic;
using TeamConvention.Interfaces;
using UnityEngine;

public class PlayerController : MonoBehaviour, IInteractor, IInventoryOwner
{
    [Header("이동 설정")]
    [SerializeField] private float _moveSpeed = 8f;
    [SerializeField] private float _sprintScale = 2f; //스프린트 속도 배율
    [SerializeField] private float _crouchScale = 0.3f; //앉을 때 속도 배율
    [SerializeField] private float _overweightScale = 0.5f; //무게 초과했을 때 속도 배율 
    [SerializeField] private float _overweightMoveSpeed = 1f;
    [SerializeField] private Rigidbody _rigidbody_Player;
    [SerializeField] private CapsuleCollider _playerCollider;
    private Vector3 _moveDirection; // 플레이어 이동하는 방향
    private bool _isOverweight;
    private bool _isCrouching;
    private bool _isStaminaCooling = false; //스태미나 고갈을 표현하는 상태변수

    [Header("점프관련 설정")]
    [SerializeField] private float _jumpForce = 7f; // 점프 힘 (높이 조절)
    [SerializeField] private Transform _groundCheck;    // 발 밑에 배치할 빈 오브젝트
    [SerializeField] private float _groundCheckRadius = 0.5f; // 체크 범위
    [SerializeField] private LayerMask _groundLayer;    // 지면으로 인식할 레이어 (Platforms 등)
    private bool _isGrounded = false; // 바닥에 붙어있는지 여부

    [Header("머리 관련 체크 설정")]
    [SerializeField] private Transform _headCheck; // 아까 만든 머리 위 빈 오브젝트 할당
    [SerializeField] private float _headCheckRadius = 0.5f;       // 체크할 구체의 반지름
    [SerializeField] private LayerMask _headLayer;
    private bool _isHeading = false; // 머리에 무언가 부딪혔는지 여부

    [Header("카메라 회전 및 위치 설정")]
    [SerializeField] private Transform _tranform_CameraRig; // 플레이어 자식으로 있는 CameraRig 트랜스폼
    [SerializeField] private Camera Camera_FPS;
    [SerializeField] private Transform _tranform_CrouchCamera; //앉았을 때 카메라 위치 트랜스폼
    [SerializeField] private float _float_MoveColliderY = 0.5f; //앉았을 때 콜라이더 높이 변경값
    private float _standCameraLocalY; //서있을 때의 카메라 높이 저장한 변수

    [Header("입력부 가져오기")]
    [SerializeField] private PlayerInputHandler _inputHandler;

    [Header("인벤토리")]
    [SerializeField] private PlayerInventory _playerInventory;

    public IReadOnlyList<InventoryItem> BagItems => _playerInventory != null ? _playerInventory.BagItems : null;
    public InventoryItem LeftHandItem => _playerInventory != null ? _playerInventory.LeftHandItem : null;
    public InventoryItem RightHandItem => _playerInventory != null ? _playerInventory.RightHandItem : null;

    public InventoryItem RemoveBagItem(InventoryItem inventoryItem) => _playerInventory?.RemoveBagItem(inventoryItem);
    public InventoryItem ClearHandItem(PlayerHandType handType) => _playerInventory?.ClearHandItem(handType);

    public bool TryAcquireItem(ItemData itemData, HoldType holdType, out InventoryItem acquiredItem, out string resultMessage)
    {
        if (_playerInventory == null)
        {
            acquiredItem = null;
            resultMessage = "PlayerInventory가 연결되지 않았습니다.";
            return false;
        }

        return _playerInventory.TryAcquireItem(itemData, holdType, out acquiredItem, out resultMessage);
    }

    [Header("상호작용")]
    [SerializeField] private InteractionHoverDetector _hoverDetector;

    [Header("플레이어 스탯")]
    [SerializeField] private float _playerSp = 100;
    [SerializeField] private float _spintSpUsePerSecond = 5; //스프린트 시 초당 소모되는 스태미나
    [SerializeField] private float _spintSpAddPerSecond = 3; //평소 초당 회복되는 스태미나
    private float _playerMaxSp;

    public Vector3 Position => this.transform.position;
    public Transform CameraTransform => Camera_FPS != null ? Camera_FPS.transform : null;
    public PlayerInventory Inventory => _playerInventory;

    void Awake()
    {
        // 인스펙터에서 깜빡하고 할당 안 했을 때를 대비해 자동으로 리지드바디 넣기
        if (_rigidbody_Player == null)
        {
            _rigidbody_Player = GetComponent<Rigidbody>();
        }

        if (_groundCheck == null)
        {
            _groundCheck = this.transform;
        }

        if (_inputHandler == null)
        {
            _inputHandler = GetComponent<PlayerInputHandler>();
        }

        if (_hoverDetector == null)
        {
            _hoverDetector = GetComponentInParent<InteractionHoverDetector>();
        }

        if (_playerCollider == null)
        {
            _playerCollider = GetComponent<CapsuleCollider>();
        }

        _playerMaxSp = _playerSp; //최대 스태미나 지정

        _standCameraLocalY = _tranform_CameraRig.localPosition.y; //서있을 때의 카메라 높이 저장

    }

    void OnEnable()
    {
        if (_inputHandler != null)
        {
            _inputHandler.OnInteractEvent += TryInteract;
        }

        if (_inputHandler != null)
        {
            _inputHandler.OnCrouchChanged += CrouchAndStand;
        }

    }

    void OnDisable()
    {
        if (_inputHandler != null)
        {
            _inputHandler.OnInteractEvent -= TryInteract;
        }

        if (_inputHandler != null)
        {
            _inputHandler.OnCrouchChanged -= CrouchAndStand;
        }
    }

    void FixedUpdate()
    {
        Move();
        RotatePlayer();

        if (_groundCheck != null)
        {
            _isGrounded = Physics.CheckSphere(_groundCheck.position, _groundCheckRadius, _groundLayer);
        }

        if (_inputHandler.JumpRequested)
        {
            Jump();
        }

        if (_headCheck != null)
        {
            _isHeading = Physics.CheckSphere(_headCheck.position, _headCheckRadius, _headLayer);
          
        }

        if (IsSprint())
        {
            //스프린트 상태가 아니고, 스태미나가 최대가 아닐 때 회복한다
            TakePlayerSpDamagePerSecond(_spintSpUsePerSecond); //스태미나를 초당 정해진 값만큼 깎음
            if (_playerSp < 0f) {

                _playerSp = 0f;
                _isStaminaCooling = true;
            }
        }
        else  //스프린트 상태가 아니고, 스태미나가 최대가 아닐 때 회복한다
        {
            if(_playerSp < _playerMaxSp)
            {
                AddPlayerSpPerSecond(_spintSpAddPerSecond);

                if (_playerSp > _playerMaxSp)
                    _playerSp = _playerMaxSp;
            }

            if (_isStaminaCooling && _playerSp >= _playerMaxSp)
            {
                _isStaminaCooling = false;
            }
        }

    }

    private bool IsSprint() //스프린트 입력되고, 좌표 변경되는 중, 스태미나 0이상, 앉기가 입력되지 않을때 true
    {
        bool hasInput = _inputHandler.SprintRequested;
        bool isMoving = _moveDirection.magnitude > 0.1f;
        bool hasStamina = _playerSp > 0f&& !_isStaminaCooling; ;
        bool isNotCrouching = !_isCrouching;

        return hasInput && isMoving && hasStamina && isNotCrouching;
    }

    private void Move()
    {
        if (_inputHandler == null) return;
        if (_inputHandler.CurrentMode != PlayerInputMode.Gameplay) return;

        Vector3 input = _inputHandler.InputVector;
        _moveDirection = (transform.forward * input.z + transform.right * input.x).normalized;
        float currentMoveSpeed = GetCurrentMoveSpeed();

        // 2. 물리 속도(Velocity) 적용
        _rigidbody_Player.linearVelocity = new Vector3(
            _moveDirection.x * currentMoveSpeed,
            _rigidbody_Player.linearVelocity.y,
            _moveDirection.z * currentMoveSpeed
            );
    }

    private float GetCurrentMoveSpeed()
    {
        if (_isCrouching) //웅크린 상태일때
        {
            return _moveSpeed * _crouchScale;
        }

        if (IsSprint()) //스프린트 상태일때
        {
            return _moveSpeed * _sprintScale;

        }
        //나중에 아래로 옮겨야 함

        if (_playerInventory == null)
            return _moveSpeed;

        float currentWeight = _playerInventory.GetTotalCarryWeight();
        float maxWeight = _playerInventory.MaxCarryWeight;
        bool isOverweight = currentWeight > maxWeight;


        if (_isOverweight != isOverweight)
        {
            _isOverweight = isOverweight;

            if (_isOverweight)
            {
                Debug.Log($"무게 초과 상태. 이동속도를 {_overweightMoveSpeed:0.##}. 현재 보유 아이템 무게: {currentWeight:0.##}/{maxWeight:0.##}");
            }

            else
            {
                Debug.Log($"무게 초과 상태가 해제. 이동속도를 {_moveSpeed:0.##}. 현재 보유 아이템 무게: {currentWeight:0.##}/{maxWeight:0.##}");
            }
        }

        if (_isOverweight)
        {
            return _moveSpeed * _overweightScale;
        }

        /* if (_inputHandler.SprintRequested) //스프린트 키가 눌렸을 때
         {
             return _moveSpeed * _sprintScale;

         }*/
        // _playerInventory이 완성되면 위에 있는 걸 지우고 여기로 옮겨야 함

        return _moveSpeed;

    }

    private void RotatePlayer()
    {
        float cameraYaw = Camera_FPS.transform.eulerAngles.y;

        Quaternion targetRotation = Quaternion.Euler(0f, cameraYaw, 0f);

        // _rigidbody_Player.MoveRotation(targetRotation); //리지드바디로 회전시키기
        this.transform.rotation = targetRotation; //트랜스폼으로 회전시키기
    }
    private void CrouchAndStand()
    {
        Vector3 camPos = _tranform_CameraRig.localPosition;

        if (!_isCrouching)
        {
            _playerCollider.height -= _float_MoveColliderY; //콜라이더의 길이 감소
            _playerCollider.center = new Vector3(_playerCollider.center.x, _playerCollider.center.y - _float_MoveColliderY / 2f, _playerCollider.center.z);
            //콜라이더 위치를 길이 감소값의 절반만큼 감소
            //이렇게 해야 바닥이 그대로 유지된다
            camPos.y = _tranform_CrouchCamera.localPosition.y;

            _isCrouching = true;
        }
        else if (_isCrouching && !_isHeading) //머리에 부딪히지 않았다면
        {
            _playerCollider.height += _float_MoveColliderY;
            _playerCollider.center = new Vector3(_playerCollider.center.x, _playerCollider.center.y + _float_MoveColliderY / 2f, _playerCollider.center.z);

            camPos.y = _standCameraLocalY;
            _isCrouching = false;
        }

        _tranform_CameraRig.localPosition = camPos;
    }

    private void Jump()
    {
        // 바닥에 닿아있을 때만 점프 가능하도록 제한 (무한 점프 방지)
        if (_isGrounded)
        {
            // Y축 방향으로 순간적인 힘을 빡 꽂아넣어 '딱딱하게' 뛰어오르게 합니다.
            _rigidbody_Player.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
        }

        _inputHandler.JumpRequested = false;
    }

    public void TryInteract()
    {
        if (_hoverDetector == null)
        {
            Debug.LogWarning("InteractionHoverDetector가 연결되지 않았습니다.");
            return;
        }

        var interactable = _hoverDetector.CurrentTarget;
        if (interactable == null)
        {
            Debug.Log("상호작용할 대상이 없습니다.");
            return;
        }

        interactable.Interact(this);
    }

    private void OnDrawGizmos() //시각적으로 _groundCheck 그리기
    {
        if (_groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_groundCheck.position, _groundCheckRadius);
        }

        if (_headCheck != null)
        {
            Gizmos.DrawWireSphere(_headCheck.position, _headCheckRadius);
        }
    }


    public void TakePlayerSpDamage(float damage)
    {
        _playerSp -= damage;
        Debug.Log($"플레이어 Sp: {_playerSp}");
    }

    public void TakePlayerSpDamagePerSecond(float damage)
    {
        _playerSp -= damage * Time.fixedDeltaTime;
        Debug.Log($"플레이어 Sp: {_playerSp}");
    }

    public void AddPlayerSp(float sp)
    {
        _playerSp += sp;
        Debug.Log($"플레이어 Sp: {_playerSp}");

    }

    public void AddPlayerSpPerSecond(float sp)
    {

        _playerSp += sp * Time.fixedDeltaTime;
        Debug.Log($"플레이어 Sp: {_playerSp}");

    }

    public void TakePlayerMoveSpeedDamage(float damage)
    {
        _moveSpeed -= damage;
        Debug.Log($"플레이어 속도: {_moveSpeed}");
    }

    public void AddPlayerMoveSpeed(float speed)
    {
        _moveSpeed += speed;
        Debug.Log($"플레이어 속도: {_moveSpeed}");
    }

    private void PlayerDie()
    {
        Debug.Log("플레이어가 죽었습니다.");
    }
}