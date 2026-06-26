using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    public Vector3 InputVector { get; private set; } // WASD 키보드 이동값을 받는 벡터
    public Vector2 LookVector { get; private set; } //카메라 회전값을 받는 벡터
    public bool JumpRequested { get; set; }  // 점프 입력이 들어왔는지 확인하는 플래그
    public bool InteractRequested { get; set; } // interact입력이 들어왔는지 확인하는 플래그
    public bool SprintRequested { get; private set; } // Sprint 입력이 들어왔는지 확인하는 플래그
    public bool CrouchRequested { get; private set; }//Crouch 입력이 들어왔는지 확인하는 플래그
    public PlayerInputMode CurrentMode { get; private set; } = PlayerInputMode.Gameplay;

    public bool JewelryInventoryRequested { get; private set; }

    public event Action OnInteractEvent;

    public event Action OnCrouchChanged;

    public event Action OnJewelryInventoryToggleEvent; // B키 (보석 인벤토리 열기/ 닫기)

    // 추가: 입력 모드 전환 (이동/시선 허용 여부 + 커서 잠금 상태를 함께 제어)
    public void SetMode(PlayerInputMode mode)
    {
        CurrentMode = mode;

        bool isCursorVisible = mode != PlayerInputMode.Gameplay;
        Cursor.lockState = isCursorVisible ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isCursorVisible;
    }

    private void OnMove(InputValue value)
    {
        Vector2 rawInput = value.Get<Vector2>();

        InputVector = new Vector3(rawInput.x, 0f, rawInput.y);
    }

    private void OnLook(InputValue value)
    {
        if (CurrentMode == PlayerInputMode.UIOnly) return;
        LookVector = value.Get<Vector2>();
    }
    private void OnJump(InputValue value)
    {
        if (CurrentMode != PlayerInputMode.Gameplay) return;

        if (value.isPressed)
        {
            JumpRequested = true;
        }
    }

    private void OnInteract(InputValue value) => OnInteractEvent?.Invoke();

    private void OnSprint(InputValue value)
    {
        SprintRequested = value.isPressed;
    }

    private void OnCrouch(InputValue value)
    {
        if (value.isPressed)
        {
            //CrouchRequested = !CrouchRequested;
            OnCrouchChanged?.Invoke();
        }
       
    }

    // B키 보석 인벤토리
    private void OnJewelryInventory(InputValue value)
    {
        JewelryInventoryRequested = value.isPressed;

        if (value.isPressed)
        {
            OnJewelryInventoryToggleEvent?.Invoke();
        }
    }
}
