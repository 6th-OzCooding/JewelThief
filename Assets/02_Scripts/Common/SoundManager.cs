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

        Utils.LoadAndPlayAudioClip(SFXSourcePlayer, soundDataId).Forget();
    }

    public void PlayBGM(string soundDataId)
    {
        Utils.LoadAndPlayAudioClip(BGMSourcePlayer, soundDataId, isLoop: true).Forget();
    }

    public void SetBGMPitch(float pitch)
    {
        if (null == BGMSourcePlayer) return;

        BGMSourcePlayer.pitch = Mathf.Clamp(pitch, 0f, 2f);
    }

}
