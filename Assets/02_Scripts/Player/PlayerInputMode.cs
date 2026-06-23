using UnityEngine;

public enum PlayerInputMode
{
    Gameplay,         // 평소: 이동 + 시선 + 커서 잠금
    CinematicLook,   // 워킹/스테이지 선택 등: 카메라 워킹 중 시점 이동이 가능한 경우
    UIOnly              // 완전 UI 모드: 이동/시선 모두 잠금, 커서만 노출
}
