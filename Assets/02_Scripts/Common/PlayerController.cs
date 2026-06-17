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
    [SerializeField] private float _mouseSensitivity = 10f; // 마우스 민감도
    [SerializeField] private float _rotationSmoothTime = 0.05f; //부드러운 회전을 위한 지연시간
    [SerializeField] private float _minLookAngle = -40f; // 카메라 상단 제한 각도
    [SerializeField] private float _maxLookAngle = 70f;  // 카메라 하단 제한 각도
    private float _verticalRotation = 0f;
    private float _horizontalRotation = 0f;
    // 카메라 회전 누적 값
    private float _targetHorizontalRotation = 0f;
    private float _targetVerticalRotation = 0f;
    // 카메라 회전 최종 값
    private float _currentVerticalVelocity = 0f;
    private float _currentHorizontalVelocity = 0f;
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


        CalculateTargetRotation();
    }
    void FixedUpdate()
    {
        RotatePlayerAndCamera();
        Move();

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

    private void CalculateTargetRotation() //카메라 회전값 계산
    {
        if (_inputHandler == null) return;

        // 1. 마우스 입력값 누적 (Time.deltaTime을 곱해 독립적인 프레임 보정)
        float mouseX = _inputHandler.LookVector.x * _mouseSensitivity * Time.deltaTime;
        float mouseY = _inputHandler.LookVector.y * _mouseSensitivity * Time.deltaTime;

        // 2. 목표로 하는 '최종 각도'를 먼저 더해줍니다.
        _targetHorizontalRotation += mouseX;
        _targetVerticalRotation -= mouseY;
        _targetVerticalRotation = Mathf.Clamp(_targetVerticalRotation, _minLookAngle, _maxLookAngle);

        // 3. [핵심 수정] '현재 각도'에서 '목표 각도'로 SmoothDamp를 흘려보내 미세 떨림을 잡습니다.
        _horizontalRotation = Mathf.SmoothDamp(_horizontalRotation, _targetHorizontalRotation, ref _currentHorizontalVelocity, _rotationSmoothTime);
        _verticalRotation = Mathf.SmoothDamp(_verticalRotation, _targetVerticalRotation, ref _currentVerticalVelocity, _rotationSmoothTime);
    }

    
    private void RotatePlayerAndCamera()
    {
        // 좌우 회전 (Rigidbody를 사용해 물리 기반으로 쭉 밀어줍니다)
        if (_rigidbody_Player != null)
        {
            Quaternion targetBodyRotation = Quaternion.Euler(0f, _horizontalRotation, 0f);
            _rigidbody_Player.MoveRotation(targetBodyRotation);
        }

        // 상하 고개 회전 (카메라 릭은 물리 연산 대상이 아니므로 트랜스폼으로 부드럽게 연동)
        if (_tranform_cameraRig != null)
        {
            _tranform_cameraRig.localRotation = Quaternion.Euler(_verticalRotation, 0f, 0f);
        }
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
