using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    public AudioSource bgSound;
    public AudioMixer mixer;
    public AudioClip[] bglist;
    public AudioClip[] sfxlist;
    public AudioClip[] txtlist;
    private static SoundManager instance;
    private List<AudioSource> sfxSources = new List<AudioSource>(); // List to keep track of SFX sources

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static SoundManager Instance
    {
        get
        {
            if (null == instance)
            {
                return null;
            }
            return instance;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        for (int i = 0; i < bglist.Length; i++)
        {
            if (scene.name == bglist[i].name)
            {
                BGSoundSave(bglist[i]);
            }
        }
    }

    private float LinearToDecibel(float linearValue)
    {
        if (linearValue == 0)
        {
            return -80f; // -80 dB is usually considered as silent
        }
        else
        {
            return 20f * Mathf.Log10(linearValue);
        }
    }

    public void BGSoundVolume(float val)
    {
        val = Mathf.Clamp(val, 0f, 1f); // 값이 항상 0과 1 사이에 있도록 보장
        mixer.SetFloat("BGSound", LinearToDecibel(val));
    }

    public void SFXSoundVolume(float val)
    {
        val = Mathf.Clamp(val, 0f, 1f); // 값이 항상 0과 1 사이에 있도록 보장
        mixer.SetFloat("SFXSound", LinearToDecibel(val));
    }

    public void SFXPlay(string sfxName, int sfxNum)
    {
        GameObject go = new GameObject(sfxName + "Sound");
        AudioSource audioSource = go.AddComponent<AudioSource>();

        audioSource.clip = sfxlist[sfxNum];
        audioSource.outputAudioMixerGroup = mixer.FindMatchingGroups("SFX")[0];
        audioSource.volume = 0.1f;
        audioSource.Play();
        Destroy(go, sfxlist[sfxNum].length);
        sfxSources.Add(audioSource); // Add to the list of SFX sources
    }


    public void SFXTextPlay(string textName, int textNum)
    {
        GameObject go = new GameObject(textName + "Sound");
        AudioSource audioSource = go.AddComponent<AudioSource>();

        audioSource.clip = txtlist[textNum];
        audioSource.outputAudioMixerGroup = mixer.FindMatchingGroups("SFX")[0];
        audioSource.volume = 0.1f;
        audioSource.Play();
        Destroy(go, txtlist[textNum].length);
        sfxSources.Add(audioSource); // Add to the list of SFX sources
    }

    public void BGSoundSave(AudioClip clip)
    {
        bgSound.clip = clip;
    }

    public void BGSoundPlay()
    {
        if (bgSound.clip != null)
        {
            bgSound.outputAudioMixerGroup = mixer.FindMatchingGroups("BG")[0];
            bgSound.loop = true;
            bgSound.volume = 0.1f;
            bgSound.Play();
        }
    }

    public void StopBGSound()
    {
        if (bgSound.clip != null)
            bgSound.Stop();
    }

    // Pause the background sound
    public void PauseBGSound()
    {
        if (bgSound.isPlaying)
            bgSound.Pause();
    }

    // Resume the background sound
    public void ResumeBGSound()
    {
        if (!bgSound.isPlaying)
            bgSound.UnPause();
    }

    // Pause all currently playing SFX
    public void PauseAllSFX()
    {
        foreach (AudioSource source in sfxSources)
        {
            if (source.isPlaying)
                source.Pause();
        }
    }

    // Resume all paused SFX
    public void ResumeAllSFX()
    {
        foreach (AudioSource source in sfxSources)
        {
            if (source.clip != null && !source.isPlaying)
                source.UnPause();
        }
    }

    // Stop all currently playing SFX
    public void StopAllSFX()
    {
        foreach (AudioSource source in sfxSources)
        {
            if (source.isPlaying)
                source.Stop();
        }
        sfxSources.Clear(); // Clear the list of SFX sources
    }
}
