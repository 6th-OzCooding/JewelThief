using UnityEngine;

public class SoundManager
{
    private AudioSource SFXSourcePlayer;
    private AudioSource BGMSourcePlayer;

    public void Init(GameObject gameManager)
    {
        SFXSourcePlayer = Utils.GetOrAddComponentInChild<AudioSource>(gameManager, "SFXSourcePlayer");
        BGMSourcePlayer = Utils.GetOrAddComponentInChild<AudioSource>(gameManager, "BGMSourcePlayer");
    }

    public void PlaySFX(string soundDataId)
    {
        SoundData data = GameManager.DataTable.GetSoundData(soundDataId);
        if (null == data)
        {
            Debug.LogError($"사운드 데이터를 찾을 수 없습니다: {soundDataId}");
            return;
        }

        Utils.LoadAndPlayAudioClip(SFXSourcePlayer, data.Name, data.IsLoop, data.Volume).Forget();
    }

    public void PlayBGM(string soundDataId)
    {
        SoundData data = GameManager.DataTable.GetSoundData(soundDataId);
        if (null == data)
        {
            Debug.LogError($"사운드 데이터를 찾을 수 없습니다: {soundDataId}");
            return;
        }

        Utils.LoadAndPlayAudioClip(BGMSourcePlayer, data.Name, data.IsLoop, data.Volume).Forget();
    }

    public void SetBGMPitch(float pitch)
    {
        if (null == BGMSourcePlayer) return;

        BGMSourcePlayer.pitch = Mathf.Clamp(pitch, 0f, 2f);
    }

}
