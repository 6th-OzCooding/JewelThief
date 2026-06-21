using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("이동 설정")]
    [SerializeField] private float _moveSpeed = 8f;
    [SerializeField] private Rigidbody _rigidbody_Player;
    private Vector3 _moveDirection; // 플레이어 이동하는 방향
    private Vector2 _inputVector; // WASD 키보드 이동값을 받는 벡터

    [Header("점프관련 설정")]
    [SerializeField] private float _jumpForce = 7f; // 점프 힘 (높이 조절)
    [SerializeField] private Transform _groundCheck;    // 발 밑에 배치할 빈 오브젝트
    [SerializeField] private float _groundCheckRadius = 0.5f; // 체크 범위
    [SerializeField] private LayerMask _groundLayer;    // 지면으로 인식할 레이어 (Platforms 등)
    private bool _isGrounded = false; // 바닥에 붙어있는지 여부
    private bool _jumpRequested = false; // 점프 입력이 들어왔는지 확인하는 플래그

    [Header("카메라 회전 설정")]
    [SerializeField] private Transform _cameraRig; // 플레이어 자식으로 있는 CameraRig 오브젝트 등록
    [SerializeField] private float _mouseSensitivity = 10f; // 마우스 민감도
    [SerializeField] private float _minLookAngle = -40f; // 카메라 상단 제한 각도
    [SerializeField] private float _maxLookAngle = 70f;  // 카메라 하단 제한 각도
    private Vector2 _lookVector; //카메라 회전값을 받는 벡터
    private float _verticalRotation = 0f; // 카메라 상하 회전 누적 값

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
       


    }

    // 세팅 UI에서 쓰려고 추가
    void Start()
    {
        _mouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 50f);
    }

    // 세팅 UI에서 쓰려고 추가
    public void SetMouseSensitivity(float sensitivity)
    {
        _mouseSensitivity = sensitivity;
    }

    void Update()
    {
        Vector3 forwardMovement = transform.forward * _inputVector.y;
        Vector3 rightMovement = transform.right * _inputVector.x;
        _moveDirection = (forwardMovement + rightMovement).normalized;

        RotatePlayerAndCamera();

       
    }
    void FixedUpdate()
    {
        if (_groundCheck != null)
        {
            _isGrounded = Physics.CheckSphere(_groundCheck.position, _groundCheckRadius, _groundLayer);
        }

        Move();

        if (_jumpRequested)
        {
            Jump();
        }
    }


   

    private void Move()
    {
       

        _rigidbody_Player.linearVelocity = new Vector3(
             _moveDirection.x * _moveSpeed,
             _rigidbody_Player.linearVelocity.y,
             _moveDirection.z * _moveSpeed
         );
    }


   
    // 마우스 움직임에 따른 회전 로직
    private void RotatePlayerAndCamera()
    {
        // 1. Time.deltaTime을 곱해 1초당 회전량으로 보정합니다.
        // (이때 감도가 너무 낮아지면 _mouseSensitivity 기본값을 인스펙터에서 키워주세요)
        float horizontalRotation = _lookVector.x * _mouseSensitivity * Time.deltaTime;
        this.transform.Rotate(Vector3.up * horizontalRotation);

        // 2. 상하 회전 역시 Time.deltaTime을 곱해줍니다.
        _verticalRotation -= _lookVector.y * _mouseSensitivity * Time.deltaTime;

        // 목이 뒤로 꺾이거나 땅 뚫고 들어가지 않도록 각도 제한 (Clamp)
        _verticalRotation = Mathf.Clamp(_verticalRotation, _minLookAngle, _maxLookAngle);

        if (_cameraRig != null)
        {
            _cameraRig.localRotation = Quaternion.Euler(_verticalRotation, 0f, 0f);
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
        _jumpRequested = false; // 플래그 초기화
    }

    private void OnMove(InputValue value)
    {
        _inputVector = value.Get<Vector2>();
    }

    private void OnLook(InputValue value)
    {
        _lookVector = value.Get<Vector2>();
    }
    private void OnJump(InputValue value)
    {
        // 버튼을 누른 순간에 호출됩니다.
        if (value.isPressed)
        {
            _jumpRequested = true;
        }
    }

    private void OnInteract(InputValue value)
    {
        if (value.isPressed)
        {
            Debug.Log("E 키 입력: 상호작용을 시작합니다.");
            // 여기에 아이템과 상호작용할 코드 입력( 레이케스트를 화면상으로 쏴서 물건을 줍는다던가, 오브젝트를 체크해서 등등)
        }
    }
}
