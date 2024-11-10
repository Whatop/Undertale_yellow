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
        for (int i = 0; i < bglist.Length; i++)
        {
            if (scene.name == bglist[i].name)
            {
                BGSoundSave(bglist[i]);
            }
        }
    }

    public void SFXPlay(string sfxName, int sfxNum)
    {
        AudioSource audioSource = GetAudioSourceFromPool();
        audioSource.clip = sfxlist[sfxNum];
        audioSource.volume = 0.1f;
        audioSource.Play();
        StartCoroutine(DeactivateAfterPlay(audioSource));
    }

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
        if (bgSound.isPlaying)
            bgSound.Pause();
    }

    public void ResumeBGSound()
    {
        if (!bgSound.isPlaying)
            bgSound.UnPause();
    }

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

}
