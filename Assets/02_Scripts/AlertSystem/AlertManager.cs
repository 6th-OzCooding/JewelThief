using System;
using UnityEngine;

public enum AlertLevel
{
    None,
    Low,
    Mid,
    High,
}

public class AlertManager
{
    private float _timeLimit;
    private float _remainingTime;
    private AlertLevel _currentLevel = AlertLevel.Low;
    private bool _isTimeUp = false;
    private bool _isPaused = false;

    // 경계 단계 변경 이벤트 (변경된 단계 전달)
    public event Action<AlertLevel> OnAlertLevelChanged;

    // 제한시간 소진 이벤트
    public event Action OnTimeUp;

    public void Init(float timeLimit)
    {
        _timeLimit = timeLimit;
        _remainingTime = timeLimit;
        _currentLevel = AlertLevel.Low;
        _isTimeUp = false;
        _isPaused = false;
    }

    // GameManager의 Update에서 호출
    public void OnUpdate()
    {
        if (_isTimeUp) return;
        if (_isPaused) return;

        ReduceTimer(Time.deltaTime);
    }

    #region Timer

    // 타이머 증가 요청 (소모품 아이템 등)
    public void AddTimer(float amount)
    {
        if (_isTimeUp) return;
        if (amount <= 0f) return;

        _remainingTime += amount;

        // TODO(김경훈 2026-06-13): 타이머 증가 시 경계레벨 감소 여부 논의 필요
    }

    // 타이머 감소 요청 (함정 발동, 플레이어 행동 등)
    public void ReduceTimer(float amount)
    {
        if (_isTimeUp) return;
        if (amount <= 0f) return;

        _remainingTime -= amount;

        if (_remainingTime <= 0f)
        {
            _remainingTime = 0f;
            HandleTimeUp();
            return;
        }

        UpdateAlertLevel();
    }

    public void PauseTimer()
    {
        if (_isTimeUp) return;

        _isPaused = true;
    }

    public void ResumeTimer()
    {
        if (_isTimeUp) return;

        _isPaused = false;
    }

    public void ResetTimer()
    {
        _remainingTime = _timeLimit;
        _currentLevel = AlertLevel.Low;
        _isTimeUp = false;
        _isPaused = false;

        OnAlertLevelChanged?.Invoke(_currentLevel);
    }

    public float GetRemainingTime()
    {
        return _remainingTime;
    }

    #endregion

    #region Alert Level

    public AlertLevel GetCurrentLevel()
    {
        return _currentLevel;
    }

    // 경계 단계 업데이트 (타이머 감소 시)
    private void UpdateAlertLevel()
    {
        AlertLevel newLevel = CalcAlertLevel();

        if (newLevel <= _currentLevel) return;

        _currentLevel = newLevel;
        OnAlertLevelChanged?.Invoke(_currentLevel);
    }

    // 남은 시간 비율에 따른 경계 단계 계산
    private AlertLevel CalcAlertLevel()
    {
        float ratio = _remainingTime / _timeLimit;

        if (ratio > 2f / 3f) return AlertLevel.Low;
        if (ratio > 1f / 3f) return AlertLevel.Mid;
        return AlertLevel.High;
    }

    #endregion

    // 제한시간 소진 처리
    private void HandleTimeUp()
    {
        _isTimeUp = true;
        _currentLevel = AlertLevel.High;

        OnAlertLevelChanged?.Invoke(_currentLevel);
        OnTimeUp?.Invoke();
    }
}