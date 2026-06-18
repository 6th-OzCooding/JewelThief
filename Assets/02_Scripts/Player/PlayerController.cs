using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("이동 설정")]
    [SerializeField] private float _moveSpeed = 8f;
    [SerializeField] private Rigidbody _rigidbody_Player;
    private Vector3 _moveDirection; // 플레이어 이동하는 방향

    [Header("점프관련 설정")]
    [SerializeField] private float _jumpForce = 7f; // 점프 힘 (높이 조절)
    [SerializeField] private Transform _groundCheck;    // 발 밑에 배치할 빈 오브젝트
    [SerializeField] private float _groundCheckRadius = 0.5f; // 체크 범위
    [SerializeField] private LayerMask _groundLayer;    // 지면으로 인식할 레이어 (Platforms 등)
    private bool _isGrounded = false; // 바닥에 붙어있는지 여부

    [Header("카메라 회전 설정")] 
    [SerializeField] private Transform _tranform_cameraRig; // 플레이어 자식으로 있는 CameraRig 트랜스폼
    [SerializeField] private Camera Camera_FPS;

    
    //카메라 회전에 사용되는 가속도 변수

    [Header("입력부 가져오기")]
    [SerializeField] private PlayerInputHandler _inputHandler;


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

        if(_inputHandler == null)
        {
            _inputHandler = GetComponent<PlayerInputHandler>();
        }
    }

    void Update()
    {


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

        if (_inputHandler.InteractRequested)
        {
            PushKeyOne();
        }

    }

   


    private void Move()
    {
        Vector3 input = _inputHandler.InputVector;
        _moveDirection = (transform.forward * input.z + transform.right * input.x).normalized;

        // 2. 물리 속도(Velocity) 적용
        _rigidbody_Player.linearVelocity = new Vector3(
            _moveDirection.x * _moveSpeed,
            _rigidbody_Player.linearVelocity.y,
            _moveDirection.z * _moveSpeed
            );
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
        _inputHandler.JumpRequested = false; // 플래그 초기화
    }

    private void PushKeyOne()
    {
        Debug.Log("E키 입력됨!");
        _inputHandler.InteractRequested = false;
    }

    private void OnDrawGizmos() //시각적으로 _groundCheck 그리기
    {
        if (_groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_groundCheck.position, _groundCheckRadius);
        }
    }
}
