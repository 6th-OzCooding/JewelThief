using Cysharp.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine;

public class StageSelectController : MonoBehaviour
{
    [Header("카메라")]
    [SerializeField] private CinemachineCamera _stageSelectCamera; // 워킹용 카메라
    [SerializeField] private Transform _screenViewPoint; // 워킹이 도착하는 지점
    [SerializeField] private float _walkDuration = 1.5f;

    [Header("UI")]
    [SerializeField] private GameObject _stageSelectUI; // TODO: 스테이지 선택 UI 구현 시 삭제, Extension메서드 호출방식으로 변경

    private const int ACTIVE_PRIORITY = 20;
    private const int INACTIVE_PRIORITY = 0;

    private Transform _playerCameraTransform;
    private PlayerInputHandler _playerInputHandler;

    private void Awake()
    {
        SetStageSelectCameraActive(false);

        if (_stageSelectUI != null)
            _stageSelectUI.SetActive(false);
    }

    public void SetPlayerInputHandler(PlayerInputHandler inputHandler)
    {
        _playerInputHandler = inputHandler;
    }

    public void SetPlayerCameraTransform(Transform playerCameraTransform)
    {
        _playerCameraTransform = playerCameraTransform;
    }

    public void EnterStageSelect()
    {
        if (_playerInputHandler != null)
            _playerInputHandler.SetMode(PlayerInputMode.UIOnly);

        EnterStageSelectAsync().Forget();
    }

    private async UniTaskVoid EnterStageSelectAsync()
    {
        if (_stageSelectCamera == null)
        {
            Debug.LogError("StageSelectController: _stageSelectCamera가 연결되지 않았습니다.");
            return;
        }

        if (_screenViewPoint == null)
        {
            Debug.LogError("StageSelectController: _screenViewPoint가 연결되지 않았습니다.");
            return;
        }

        if (_playerCameraTransform == null)
        {
            Debug.LogError("StageSelectController: _playerCameraTransform이 연결되지 않았습니다.");
            return;
        }

        Transform camTransform = _stageSelectCamera.transform;
        camTransform.SetPositionAndRotation(_playerCameraTransform.position, _playerCameraTransform.rotation);

        SetStageSelectCameraActive(true);

        await TweenCameraToScreenAsync(camTransform);

        OpenStageSelectUI();
    }

    private async UniTask TweenCameraToScreenAsync(Transform camTransform)
    {
        Vector3 startPos = camTransform.position;
        Quaternion startRot = camTransform.rotation;

        float elapsed = 0f;
        while (elapsed < _walkDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / _walkDuration);

            camTransform.position = Vector3.Lerp(startPos, _screenViewPoint.position, t);
            camTransform.rotation = Quaternion.Slerp(startRot, _screenViewPoint.rotation, t);

            await UniTask.Yield();
        }

        camTransform.SetPositionAndRotation(_screenViewPoint.position, _screenViewPoint.rotation);
    }

    public void ExitStageSelect()
    {
        CloseStageSelectUI();

        SetStageSelectCameraActive(false);

        if (_playerInputHandler != null)
            _playerInputHandler.SetMode(PlayerInputMode.Gameplay);
    }

    private void OpenStageSelectUI()
    {
        if (_stageSelectUI == null)
        {
            Debug.LogError("StageSelectUI가 연결되지 않았습니다.");
            return;
        }

        _stageSelectUI.SetActive(true); // TODO(김경훈 - 06.22): 스테이지 선택창 구현시 Extension메서드 호출방식으로 변경
    }

    private void CloseStageSelectUI()
    {
        if (_stageSelectUI == null) return;

        _stageSelectUI.SetActive(false); // TODO(김경훈 - 06.22): 스테이지 선택창 구현시 Extension메서드 호출방식으로 변경
    }

    private void SetStageSelectCameraActive(bool isActive)
    {
        if (_stageSelectCamera == null)
        {
            Debug.LogError("StageSelectCamera가 연결되지 않았습니다.");
            return;
        }

        _stageSelectCamera.Priority = isActive ? ACTIVE_PRIORITY : INACTIVE_PRIORITY;
    }
}