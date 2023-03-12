using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] List<AudioData> sfxList;

    [SerializeField] AudioSource musicPlayer;
    [SerializeField] AudioSource sfxPlayer;

    [SerializeField] float fadeDuration = 0.75f;

    float originalMusicVol;
    Dictionary<AudioId, AudioData> sfxLookUp;

    public static AudioManager i { get; private set; }

    private void Awake()
    {
        i = this;
    }

    private void Start()
    {
        originalMusicVol = musicPlayer.volume;

        sfxLookUp = sfxList.ToDictionary(x => x.id);
    }

    public void PlaySfx(AudioClip clip)
    {
        if (clip == null)
        {
            return;
        }

        sfxPlayer.PlayOneShot(clip);
    }

    public void PlaySfx(AudioId audioId)
    {
        if (!sfxLookUp.ContainsKey(audioId))
        {
            return;
        }

        var audioData = sfxLookUp[audioId];
        PlaySfx(audioData.clip);
    }

    public void PlayMusic(AudioClip clip, bool loop = true, bool fade = false)
    {
        if (clip == null)
        {
            return;
        }

        StartCoroutine(PlayMusicAsync(clip, loop, fade));
    }

    IEnumerator PlayMusicAsync(AudioClip clip, bool loop, bool fade)
    {
        if (fade)
        {
            yield return musicPlayer.DOFade(0, fadeDuration).WaitForCompletion();
        }

        musicPlayer.clip = clip;
        musicPlayer.loop = loop;
        musicPlayer.Play();

        if (fade)
        {
            yield return musicPlayer.DOFade(originalMusicVol, fadeDuration).WaitForCompletion();
        }
    }
}

public enum AudioId
{
    UISelect, OpenPip, Hit, Faint, ExpGain
}

[System.Serializable]
public class AudioData
{
    public AudioId id;
    public AudioClip clip;
}
