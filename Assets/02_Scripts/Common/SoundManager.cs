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
}
