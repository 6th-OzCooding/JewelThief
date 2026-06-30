using Cysharp.Threading.Tasks;
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
    private const float DUCKING_DURATION = 1f;  // SFX 출력 시 BGM 볼륨 감소 지속 시간
    private const float DUCKED_VOLUME = 0.3f;    // SFX 출력 시 BGM 볼륨 감소 비율

    private float _timeLimit;
    private float _remainingTime;
    private AlertLevel _currentLevel = AlertLevel.Low;
    private bool _isTimeUp = false;
    private bool _isInitialized = false;
    private bool _isPaused = false;

    // 경계 단계 변경 이벤트 (변경된 단계 전달)
    public event Action<AlertLevel> OnAlertLevelChanged;

    // 제한시간 소진 이벤트
    public event Action OnTimeUp;

    public void Init(float timeLimit)
    {
        _timeLimit = 5f;
        _remainingTime = 5f;
        _currentLevel = AlertLevel.Low;
        _isTimeUp = false;
        _isPaused = false;
        _isInitialized = true;

        RaiseAlertLevelChanged(_currentLevel, playSfx: false);
    }

    // GameManager의 Update에서 호출
    public void OnUpdate()
    {
        if (!_isInitialized) return;
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

        RaiseAlertLevelChanged(_currentLevel, playSfx: false);
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
        RaiseAlertLevelChanged(_currentLevel, playSfx: true);
    }

    // 남은 시간 비율에 따른 경계 단계 계산
    private AlertLevel CalcAlertLevel()
    {
        float ratio = _remainingTime / _timeLimit;

        if (ratio > 2f / 3f) return AlertLevel.Low;
        if (ratio > 1f / 3f) return AlertLevel.Mid;
        return AlertLevel.High;
    }

    // 경계 단계 변경 이벤트 발행 + 내부 연출(BGM) 처리
    private void RaiseAlertLevelChanged(AlertLevel level, bool playSfx)
    {
        UpdateBGMPitch(level);

        if (playSfx)
        {
            PlayLevelUpSfxWithDucking().Forget();
        }

        OnAlertLevelChanged?.Invoke(level);
    }

    #endregion

    #region Presentation

    // 경계 레벨에 따라 BGM 재생 속도 조정
    private void UpdateBGMPitch(AlertLevel level)
    {
        float pitch;

        switch (level)
        {
            case AlertLevel.Low:    pitch = 1.0f;
                break;
            case AlertLevel.Mid:    pitch = 1.25f;
                break;
            case AlertLevel.High:   pitch = 1.5f;
                break;
            default: pitch = 1.0f;
                break;
        }

        Debug.Log($" 경계 레벨: {level}, BGM Pitch: {pitch}");
        GameManager.Sound.SetBGMPitch(pitch);
    }

    // 레벨업 SFX 재생 + 재생 동안 BGM 볼륨 덕킹(낮춤) 후 복구
    private async UniTaskVoid PlayLevelUpSfxWithDucking()
    {
        GameManager.Sound.PlaySFX(SoundId.SFX_AlertUp);
        GameManager.Sound.SetBGMVolume(SoundId.BGM_PlayTheme, DUCKED_VOLUME);

        await UniTask.Delay(TimeSpan.FromSeconds(DUCKING_DURATION));

        if (_isTimeUp) return;

        GameManager.Sound.SetBGMVolume(SoundId.BGM_PlayTheme, 1f);
    }

    #endregion

    // 제한시간 소진 처리
    private void HandleTimeUp()
    {
        _isTimeUp = true;
        _currentLevel = AlertLevel.High;

        GameManager.Sound.StopBGM();
        GameManager.Sound.PlaySFX(SoundId.SFX_TimeUp01);

        RaiseAlertLevelChanged(_currentLevel, playSfx: false);
        OnTimeUp?.Invoke();
    }
}