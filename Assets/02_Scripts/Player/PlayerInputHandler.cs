using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    public Vector3 InputVector { get; private set; } // WASD 키보드 이동값을 받는 벡터
    public Vector2 LookVector { get; private set; } //카메라 회전값을 받는 벡터
    public bool JumpRequested { get; set; }  // 점프 입력이 들어왔는지 확인하는 플래그
    public bool InteractRequested { get; set; } // interact입력이 들어왔는지 확인하는 플래그

    public event Action OnInteractEvent;

    private void OnMove(InputValue value)
    {
        Vector2 rawInput = value.Get<Vector2>();

        InputVector = new Vector3(rawInput.x, 0f, rawInput.y);
    }

    private void OnLook(InputValue value)
    {
        LookVector = value.Get<Vector2>();
    }
    private void OnJump(InputValue value)
    {
        // 버튼을 누른 순간에 호출됩니다.
        if (value.isPressed)
        {
            JumpRequested = true;
        }
    }

    private void OnInteract(InputValue value) => OnInteractEvent?.Invoke();
}
