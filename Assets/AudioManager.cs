using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Sound
{
    Bgm,        // 배경음악
    Effect,     // 효과음
    MaxCount,   // 아무것도 아님. enum Sound의 개수를 세기 위함
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    
    AudioSource[] audioSources = new AudioSource[(int)Sound.MaxCount];
    public List<AudioClip> audioClips = new List<AudioClip>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            audioSources[(int)Sound.Bgm] = GameObject.Find("BGM").GetComponent<AudioSource>();
            audioSources[(int)Sound.Bgm].loop = true; // BGM은 반복 재생
            
            audioSources[(int)Sound.Effect] = GameObject.Find("Effect").GetComponent<AudioSource>();
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }

    public void Play(string clipName, Sound type)
    {
        AudioClip audioClip = audioClips.Find(clip => clip.name == clipName);
        
        switch (type)
        {
            case Sound.Bgm:     // BGM 배경음악 재생
                AudioSource bgmSource = audioSources[(int)Sound.Bgm];
                if (bgmSource.isPlaying)
                    bgmSource.Stop();

                bgmSource.clip = audioClip;
                bgmSource.Play();
                break;
            case Sound.Effect:  // Effect 효과음 재생
                AudioSource effectSource = audioSources[(int)Sound.Effect];
                if (effectSource.isPlaying)
                    effectSource.Stop();

                effectSource.clip = audioClip;
                effectSource.Play();
                break;
            default:
                Debug.LogError($"Unhandled sound type '{type}'.");
                break;
        }
    }

    public void EffectPlay(string clipName)
    {
        AudioClip audioClip = audioClips.Find(clip => clip.name == clipName);
        AudioSource effectSource = audioSources[(int)Sound.Effect];
        audioSources[(int)Sound.Effect].loop = false;
        effectSource.PlayOneShot(audioClip);
    }
    
    /*public IEnumerator StartFailSound()
    {
        yield return new WaitForSeconds(1f);
        
        Play("Snicker", Sound.Effect);
    }

    public void EffectLoopPlay(string clipName)
    {
        AudioClip audioClip = audioClips.Find(clip => clip.name == clipName);
        AudioSource effectSource = audioSources[(int)Sound.Effect];
        effectSource.loop = true;
        effectSource.clip = audioClip;
        effectSource.Play();
    }

    public void EffectStop()
    {
        AudioSource effectSource = audioSources[(int)Sound.Effect];
        if (effectSource.clip != null)
        {
            effectSource.Stop(); 
            effectSource.loop = false; 
            effectSource.clip = null; 
        }
    }*/
}
