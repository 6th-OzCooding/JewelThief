using UnityEngine;

public class SoundManager
{
    private AudioSource SFXSourcePlayer;
    private AudioSource BGMSourcePlayer;

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

}
