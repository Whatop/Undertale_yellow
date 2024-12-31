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
    private Queue<AudioSource> sfxPool = new Queue<AudioSource>(); // SFX 풀
    public int initialPoolSize = 10; // 초기 풀 크기

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;

            // 초기 SFX 풀 생성
            for (int i = 0; i < initialPoolSize; i++)
            {
                GameObject go = new GameObject("SFXSource");
                AudioSource audioSource = go.AddComponent<AudioSource>();
                audioSource.outputAudioMixerGroup = mixer.FindMatchingGroups("SFX")[0];
                go.transform.SetParent(transform);
                go.SetActive(false);
                sfxPool.Enqueue(audioSource);
            }
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
            return instance;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        // 씬에 맞는 배경 음악 재생
        for (int i = 0; i < bglist.Length; i++)
        {
            if (scene.name == bglist[i].name)
            {
                BGSoundSave(bglist[i]);
            }
        }
    }

    // 사운드 이펙트(SFX) 재생
    public void SFXPlay(string sfxName, int sfxNum)
    {
        AudioSource audioSource = GetAudioSourceFromPool();
        audioSource.clip = sfxlist[sfxNum];
        audioSource.volume = 0.1f;
        audioSource.Play();
        StartCoroutine(DeactivateAfterPlay(audioSource));
    }  
    // 사운드 이펙트(SFX) 재생
    public void SFXPlay(string sfxName, int sfxNum, float volume)
    {
        AudioSource audioSource = GetAudioSourceFromPool();
        audioSource.clip = sfxlist[sfxNum];
        audioSource.volume = volume;
        audioSource.Play();
        StartCoroutine(DeactivateAfterPlay(audioSource));
    }

    // 텍스트 관련 SFX 재생
    public void SFXTextPlay(string textName, int textNum)
    {
        AudioSource audioSource = GetAudioSourceFromPool();
        audioSource.clip = txtlist[textNum];
        audioSource.volume = 0.1f;
        audioSource.Play();
        StartCoroutine(DeactivateAfterPlay(audioSource));
    }

    private AudioSource GetAudioSourceFromPool()
    {
        if (sfxPool.Count > 0)
        {
            AudioSource source = sfxPool.Dequeue();
            source.gameObject.SetActive(true);
            return source;
        }
        else
        {
            GameObject go = new GameObject("SFXSource");
            AudioSource audioSource = go.AddComponent<AudioSource>();
            audioSource.outputAudioMixerGroup = mixer.FindMatchingGroups("SFX")[0];
            go.transform.SetParent(transform);
            return audioSource;
        }
    }

    private IEnumerator DeactivateAfterPlay(AudioSource audioSource)
    {
        yield return new WaitForSeconds(audioSource.clip.length);
        audioSource.gameObject.SetActive(false);
        sfxPool.Enqueue(audioSource);
    }

    public void BGSoundSave(AudioClip clip)
    {
        bgSound.clip = clip;
    }

    // 배경 음악 재생
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

    public void BGSoundPlay(int textNum)
    {
        if (bgSound.clip != null)
        {
            bgSound.clip = bglist[textNum];
            bgSound.outputAudioMixerGroup = mixer.FindMatchingGroups("BG")[0];
            bgSound.loop = true;
            bgSound.volume = 0.1f;
            bgSound.Play();
        }
    }

    public void BGSoundPlayDelayed(int bgNum, float delay)
    {
        StartCoroutine(PlayBGAfterDelay(bgNum, delay));
    }

    private IEnumerator PlayBGAfterDelay(int bgNum, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (bgNum >= 0 && bgNum < bglist.Length)
        {
            bgSound.clip = bglist[bgNum];
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

    // 배경음 볼륨 조절
    public void BGSoundVolume(float val)
    {
        val = Mathf.Clamp(val, 0f, 1f); // 값이 항상 0과 1 사이에 있도록 보장
        mixer.SetFloat("BGSound", LinearToDecibel(val));
    }

    // 효과음 볼륨 조절
    public void SFXSoundVolume(float val)
    {
        val = Mathf.Clamp(val, 0f, 1f); // 값이 항상 0과 1 사이에 있도록 보장
        mixer.SetFloat("SFXSound", LinearToDecibel(val));
    }

    private float LinearToDecibel(float linearValue)
    {
        if (linearValue == 0)
        {
            return -80f; // -80 dB는 무음으로 간주
        }
        else
        {
            return 20f * Mathf.Log10(linearValue);
        }
    }

    // 배경음 일시 정지 및 재개
    public void PauseBGSound()
    {
        Debug.Log("배경음 멈춤");
        if (bgSound.isPlaying)
            bgSound.Pause();
    }

    public void ResumeBGSound()
    {
        if (!bgSound.isPlaying)
        {
            Debug.Log("배경음 재개");
            bgSound.UnPause();
        }
    }

    // 배경음 페이드 아웃
    public void FadeOutBGSound(float duration)
    {
        StartCoroutine(FadeOutCoroutine(duration));
    }

    private IEnumerator FadeOutCoroutine(float duration)
    {
        float currentTime = 0;
        float startVolume;

        // 현재 볼륨 값을 가져옵니다 (데시벨 값)
        mixer.GetFloat("BGSound", out startVolume);
        // 데시벨 값을 선형 값(0~1)으로 변환합니다.
        startVolume = Mathf.Pow(10, startVolume / 20);

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            // 볼륨을 선형적으로 감소시킵니다.
            float newVolume = Mathf.Lerp(startVolume, 0f, currentTime / duration);
            BGSoundVolume(newVolume);
            yield return null;
        }

        // 볼륨을 완전히 0으로 설정하고 배경음을 정지합니다.
        BGSoundVolume(0f);
        StopBGSound();
    }

    // 배경 음악을 느리게 조정하는 메서드
    public void SlowDownMusic()
    {
        if (bgSound != null)
        {
            bgSound.pitch = 0.5f; // 음속을 느리게 설정 (기본 1.0에서 0.5로 설정)
            Debug.Log("음악 속도를 느리게 변경했습니다.");
        }
    }

    // 배경 음악을 변경하는 메서드
    public void PlayMusic(string musicName)
    {
        for (int i = 0; i < bglist.Length; i++)
        {
            if (bglist[i].name == musicName)
            {
                BGSoundPlay(i);
                Debug.Log($"{musicName} 음악을 재생합니다.");
                break;
            }
        }
    }
}
