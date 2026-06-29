using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class SoundManager
{
    private AudioSource SFXSourcePlayer;
    private AudioSource BGMSourcePlayer;

    private readonly Dictionary<string, CancellationTokenSource> _repeatingSfxCtsDic = new();

    private readonly Dictionary<string, float> _repeatingSfxIntervalDic = new();

    public void Init(GameObject gameManager)
    {
        SFXSourcePlayer = Utils.GetOrAddComponentInChild<AudioSource>(gameManager, "SFXSourcePlayer");
        BGMSourcePlayer = Utils.GetOrAddComponentInChild<AudioSource>(gameManager, "BGMSourcePlayer");

        // 세팅 UI에서 쓰려고 추가
        float savedVolume = PlayerPrefs.GetFloat("MasterVolume", 1.0f);
        SetMasterVolume(savedVolume);
    }

    // 세팅 UI에서 쓰려고 추가
    public void SetMasterVolume(float volume)
    {
        if (SFXSourcePlayer != null) SFXSourcePlayer.volume = volume;
        if (BGMSourcePlayer != null) BGMSourcePlayer.volume = volume;
    }

    public void PlaySFX(string soundDataId)
    {
        SoundData data = GameManager.DataTable.GetSoundData(soundDataId);
        if (null == data)
        {
            Debug.LogError($"사운드 데이터를 찾을 수 없습니다: {soundDataId}");
            return;
        }

        Utils.LoadAndPlayAudioClip(SFXSourcePlayer, data.Name, data.IsLoop, data.Volume);
    }

    public void PlayBGM(string soundDataId)
    {
        SoundData data = GameManager.DataTable.GetSoundData(soundDataId);
        if (null == data)
        {
            Debug.LogError($"사운드 데이터를 찾을 수 없습니다: {soundDataId}");
            return;
        }

        Utils.LoadAndPlayAudioClip(BGMSourcePlayer, data.Name, data.IsLoop, data.Volume);
    }

    public void StopBGM()
    {
        if (null == BGMSourcePlayer) return;

        BGMSourcePlayer.Stop();
    }

    public void SetBGMVolume(string soundDataId, float volumeRatio)
    {
        if (null == BGMSourcePlayer) return;

        SoundData data = GameManager.DataTable.GetSoundData(soundDataId);
        float baseVolume = data != null ? data.Volume : 1f;

        BGMSourcePlayer.volume = Mathf.Clamp01(volumeRatio) * baseVolume;
    }

    public void SetBGMPitch(float pitch)
    {
        if (null == BGMSourcePlayer) return;

        BGMSourcePlayer.pitch = Mathf.Clamp(pitch, 0f, 2f);
    }

    #region Repeating SFX

    public void PlayRepeatingSFX(string soundDataId, float interval)
    {
        if (string.IsNullOrEmpty(soundDataId))
        {
            Debug.LogError("반복 재생할 사운드 ID가 비어 있습니다.");
            return;
        }

        if (interval <= 0f)
        {
            Debug.LogError($"반복 재생 간격이 올바르지 않습니다. interval: {interval}");
            return;
        }

        if (_repeatingSfxCtsDic.ContainsKey(soundDataId)
            && _repeatingSfxIntervalDic.TryGetValue(soundDataId, out float currentInterval)
            && Mathf.Approximately(currentInterval, interval))
        {
            return;
        }

        StopRepeatingSFX(soundDataId);

        CancellationTokenSource cts = new CancellationTokenSource();
        _repeatingSfxCtsDic[soundDataId] = cts;
        _repeatingSfxIntervalDic[soundDataId] = interval;

        RepeatingSfxRoutine(soundDataId, interval, cts.Token).Forget();
    }

    public void StopRepeatingSFX(string soundDataId)
    {
        if (string.IsNullOrEmpty(soundDataId)) return;

        if (_repeatingSfxCtsDic.TryGetValue(soundDataId, out CancellationTokenSource cts))
        {
            cts.Cancel();
            cts.Dispose();
            _repeatingSfxCtsDic.Remove(soundDataId);
            _repeatingSfxIntervalDic.Remove(soundDataId);
        }
    }

    private async UniTaskVoid RepeatingSfxRoutine(string soundDataId, float interval, CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            PlaySFX(soundDataId);

            await UniTask.Delay(TimeSpan.FromSeconds(interval), cancellationToken: token).SuppressCancellationThrow();
        }
    }

    #endregion
}