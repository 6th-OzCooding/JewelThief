using TeamConvention.Interfaces;
using UnityEngine;

public class PlayerController : MonoBehaviour, IInteractor
{
    [Header("이동 설정")]
    [SerializeField] private float _moveSpeed = 8f;
    [SerializeField] private float _overweightMoveSpeed = 1f;
    [SerializeField] private Rigidbody _rigidbody_Player;
    private Vector3 _moveDirection; // 플레이어 이동하는 방향
    private bool _isOverweight;

    [Header("점프관련 설정")]
    [SerializeField] private float _jumpForce = 7f; // 점프 힘 (높이 조절)
    [SerializeField] private Transform _groundCheck;    // 발 밑에 배치할 빈 오브젝트
    [SerializeField] private float _groundCheckRadius = 0.5f; // 체크 범위
    [SerializeField] private LayerMask _groundLayer;    // 지면으로 인식할 레이어 (Platforms 등)
    private bool _isGrounded = false; // 바닥에 붙어있는지 여부

    [Header("카메라 회전 설정")]
    [SerializeField] private Transform _tranform_cameraRig; // 플레이어 자식으로 있는 CameraRig 트랜스폼
    [SerializeField] private Camera Camera_FPS;

    [Header("입력부 가져오기")]
    [SerializeField] private PlayerInputHandler _inputHandler;

    [Header("인벤토리")]
    [SerializeField] private PlayerInventory _playerInventory;

    [Header("상호작용")]
    [SerializeField] private float _interactDistance = 3f;
    [SerializeField] private LayerMask _interactLayerMask = ~0;
    [SerializeField] private QueryTriggerInteraction _interactTriggerInteraction = QueryTriggerInteraction.Ignore;

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

        if (_playerInventory == null)
        {
            _playerInventory = GetComponent<PlayerInventory>();
        }
    }

    void OnEnable()
    {
        if (_inputHandler != null)
        {
            _inputHandler.OnInteractEvent += TryInteract;
        }
    }

    void OnDisable()
    {
        if (_inputHandler != null)
        {
            _inputHandler.OnInteractEvent -= TryInteract;
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
                Debug.Log($"무게 초과 상태입니다. 이동속도를 {_overweightMoveSpeed:0.##}(으)로 조정합니다. 현재 보유 아이템 무게: {currentWeight:0.##}/{maxWeight:0.##}");
            }
            else
            {
                Debug.Log($"무게 초과 상태가 해제되었습니다. 이동속도를 {_moveSpeed:0.##}(으)로 되돌립니다. 현재 보유 아이템 무게: {currentWeight:0.##}/{maxWeight:0.##}");
            }
        }

        if (!isOverweight)
            return _moveSpeed;

        return _overweightMoveSpeed;
    }

    private void RotatePlayer()
    {
        float cameraYaw = Camera_FPS.transform.eulerAngles.y;

        Quaternion targetRotation = Quaternion.Euler(0f, cameraYaw, 0f);

        // _rigidbody_Player.MoveRotation(targetRotation); //리지드바디로 회전시키기
        this.transform.rotation = targetRotation; //트랜스폼으로 회전시키기
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
        var interactable = DetectInteractable();
        if (interactable == null)
        {
            Debug.Log("상호작용할 대상이 없습니다.");
            return;
        }

        interactable.Interact(this);
        
    }

    // 카메라 중앙에서 레이캐스트를 쏴서 IInteractable이 있는지 감지하는 함수
    // TODO(김경훈 2026-06-22): InventoryPickupItem도 IInteractable로 통합해야함.
    private IInteractable DetectInteractable()
    {
        if (Camera_FPS == null)
            return null;

        Ray ray = Camera_FPS.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (!Physics.Raycast(ray, out RaycastHit hit, _interactDistance, _interactLayerMask, _interactTriggerInteraction))
            return null;

        if (hit.collider.TryGetComponent(out IInteractable interactable))
        {
            return interactable;
        }
            
        return hit.collider.GetComponentInParent<IInteractable>();
    }

    //// 카메라 중앙에서 레이캐스트를 쏴서 InventoryPickupItem이 있는지 감지하는 함수
    //private InventoryPickupItem DetectInventoryPickupItem()
    //{
    //    if (Camera_FPS == null)
    //        return null;

    //    Ray ray = Camera_FPS.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
    //    if (!Physics.Raycast(ray, out RaycastHit hit, _interactDistance, _interactLayerMask, _interactTriggerInteraction))
    //        return null;

    //    if (hit.collider.TryGetComponent(out InventoryPickupItem pickupItem))
    //        return pickupItem;

    //    return hit.collider.GetComponentInParent<InventoryPickupItem>();
    //}

    private void OnDrawGizmos() //시각적으로 _groundCheck 그리기
    {
        if (_groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_groundCheck.position, _groundCheckRadius);
        }
    }
}
