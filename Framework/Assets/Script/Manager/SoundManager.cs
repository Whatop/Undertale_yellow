using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

//이 코드는 Unity 게임에서 사운드를 관리하는 스크립트입니다
//. 배경 음악과 효과음을 처리하며,
//씬이 로드될 때마다 해당 씬에 맞는 배경 음악을 재생합니다.
//또한 볼륨 조절 및 효과음 재생 기능이 구현되어 있습니다.
//코드는 Singleton 패턴을 사용하여 하나의 인스턴스만 생성되도록 되어 있고,
//씬이 바뀔 때마다 배경 음악을 자동으로 변경합니다.
//또한 오디오 믹서를 활용하여 볼륨을 조절합니다.
//효과음은 동적으로 생성된 GameObject에 AudioSource를 추가하여 재생하고 일정 시간 후에 파괴됩니다.






public class SoundManager: MonoBehaviour
{
    public AudioSource bgSound;
    public AudioMixer mixer;
    public AudioClip[] bglist;
    public AudioClip[] sfxlist;
    public AudioClip[] txtlist;

    private static SoundManager instance;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(instance);
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
    // 이렇게 하면 씬마다 하지만..
    // 언더테일은 방마다 하니 안됨!
    // 그래서 스테이지에서 게임 매니저로 방있으면 저거하는..ㅋㅋ..
    // 아마 방 번호를 따로 지정해줘야겠지! 음 음..
    private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        for(int i = 0; i < bglist.Length; i++)
        {
            if (arg0.name == bglist[i].name)
            {
                BGSoundSave(bglist[i]);
            }
            //방코드로 else if()  
        }
    }

    public void BGSoundVolume(float val)
    {
        mixer.SetFloat("BGSound", Mathf.Log10(val)*20);
    }
    public void SFXSoundVolume(float val)
    {
        mixer.SetFloat("SFXSound", Mathf.Log10(val) * 20);
    }

    // SoundManager.instance.SFXPlay(string,int);
    public void SFXPlay(string sfxName, int sfxNum)
    {
        GameObject go = new GameObject(sfxName + "Sound");
        AudioSource audioSource = go.AddComponent<AudioSource>();

        audioSource.clip = sfxlist[sfxNum];
        audioSource.outputAudioMixerGroup = mixer.FindMatchingGroups("SFX")[0];
        audioSource.volume = 0.1f;
        audioSource.Play();
        Destroy(go, sfxlist[sfxNum].length);
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
}
