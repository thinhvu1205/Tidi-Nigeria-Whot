using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Globals;
using UnityEngine;

public class SoundManager : Singleton<SoundManager>
{
    [SerializeField] private AudioSource audioMusic, audioEffect;
    [SerializeField] private AudioClip audioClipLobby;
    private readonly List<AudioSource> audioSourcePool = new();
    private List<AudioSource> listCurrentAudio = new();

    public void PlayMusicLobby()
    {
        if (UIManager.Instance.gameView != null)
        {
            PlayMusicInGame(Sound.IN_GAME_COMMON);
        }
        else
        {
            if (Config.isOpenMusic)
            {
                if (audioMusic.clip == audioClipLobby && !audioMusic.isPlaying)
                {
                    audioMusic.volume = 0.5f;
                    audioMusic.clip = audioClipLobby;
                    audioMusic.Play();
                }
                else if (audioMusic.clip != audioClipLobby)
                {
                    audioMusic.Stop();
                    audioMusic.clip = audioClipLobby;
                    audioMusic.volume = 0.5f;
                    audioMusic.Play();
                }
            }
            else
            {
                audioMusic.Stop();
            }
        }

    }
    public void PlayMusicInGame(string pathAudio)
    {
        // AudioClip audioClip = BundleHandler.LoadAudioClip(pathAudio);
        AudioClip audioClip = Resources.Load<AudioClip>("BundlePack/" + pathAudio);
        if (Config.isOpenMusic)
        {
            audioMusic.Stop();
            audioMusic.clip = audioClip;
            audioMusic.Play();
        }
        else
        {
            audioMusic.Stop();
        }
    }

    public AudioSource PlayEffectFromPath(string pathAudio)
    {
        if (!Config.isOpenSound) return null;

        // AudioClip audioClip = BundleHandler.LoadAudioClip(pathAudio);
        AudioClip audioClip = Resources.Load<AudioClip>("BundlePack/" + pathAudio);

        AudioSource audioSrc;
        if (audioSourcePool.Count > 0 && audioSourcePool[0].isPlaying == false)
        {
            audioSrc = audioSourcePool[0];
            audioSourcePool.RemoveAt(0);
        }
        else
        {
            audioSrc = Instantiate(audioEffect);
            audioSrc.transform.SetParent(transform);
        }
        audioSrc.Stop();
        audioSrc.clip = audioClip;
        audioSrc.Play();
        listCurrentAudio.Add(audioSrc);

        DOTween.Sequence()
            .AppendInterval(audioSrc.time).AppendCallback(() =>
            {
                //audioSrc.Stop();
                audioSourcePool.Add(audioSrc);
                listCurrentAudio.Remove(audioSrc);
            });
        return audioSrc;
    }
    
    public void StopAllCurrentEffect()
    {
        listCurrentAudio.ForEach(auSrc =>
        {

            if (auSrc.isPlaying)
            {
                auSrc.Stop();
                audioSourcePool.Add(auSrc);
            }
        });
        audioSourcePool.ForEach(auSrc =>
        {

            if (auSrc.isPlaying)
            {
                auSrc.Stop();
            }
        });
        listCurrentAudio.Clear();
        audioEffect.Stop();
    }
}
