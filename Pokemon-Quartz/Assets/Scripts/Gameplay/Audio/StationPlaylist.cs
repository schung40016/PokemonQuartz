using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StationPlaylist : MonoBehaviour
{
    #region Attributes

    [SerializeField] AudioSource radio;

    public List<AudioClip> musicClips = new List<AudioClip>();
    AudioClip currentTrack;

    private float length;

    private Coroutine musicLoop;

    private MusicQueue musicQueue;

    public bool musicSwitch = true;

    #endregion

    #region Monobehavior APi

    private void Start()
    {
        musicQueue = new MusicQueue(musicClips);
    }

    #endregion

    public void StopMusic()
    {
        if(musicLoop != null)
        {
            musicQueue.musicSwitch = false;
            StopCoroutine(musicLoop);
            //StopAllCoroutines();
            radio.Stop();
        }
    }

    public void StartMusic()
    {
        musicQueue.musicSwitch = true;
        musicLoop = StartCoroutine(musicQueue.LoopMusic(this, 0));
    }
}

public class MusicQueue
{
    private List<AudioClip> clips;

    public bool musicSwitch = true;

    public MusicQueue(List<AudioClip> clips)
    {
        this.clips = clips;
    }

    public IEnumerator LoopMusic(MonoBehaviour player, float delay)
    {
        while(musicSwitch)
        {
            yield return player.StartCoroutine(Run(RandomizeList(clips), delay));
        }
    }

    public IEnumerator Run(List<AudioClip> tracks, float delay)
    {
        foreach(AudioClip clip in tracks)
        {
            AudioManager.i.PlayMusic(clip);

            yield return new WaitForSeconds(clip.length + delay);
            // BUG: Can't play the next song in queue because the above code stops working.
        }
    }

    public List<AudioClip> RandomizeList(List<AudioClip> list)
    {
        List<AudioClip> copy = new List<AudioClip>(list);

        int n = copy.Count;

        while(n > 1)
        {
            n--;

            int k = Random.Range(0, n + 1);

            AudioClip value = copy[k];

            copy[k] = copy[n];
            copy[n] = value;
        }

        return copy;
    }
}
