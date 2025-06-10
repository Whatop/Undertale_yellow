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

    [System.Serializable]
    public class SFXInfo
    {
        public int id;           // 예: 63, 226 등
        public AudioClip clip;
        // 생성자 추가 (있어도 되고 없어도 됩니다)
        public SFXInfo(int _id, AudioClip _clip)
        {
            id = _id;
            clip = _clip;
        }
    }
    [SerializeField]
    private SFXInfo[] sfxList;   // Inspector에서 사운드 ID별 AudioClip 연결

    // 재생 중인 looping SFX를 ID별로 관리하기 위한 Dictionary
    private Dictionary<int, AudioSource> loopingSources = new Dictionary<int, AudioSource>();

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
            // sfxList 길이를 sfxClips.Length만큼 잡고, 한 칸씩 초기화
            sfxList = new SFXInfo[sfxlist.Length];
            for (int i = 0; i < sfxlist.Length; i++)
            {
                // 방법1: 생성자 사용
                sfxList[i] = new SFXInfo(i, sfxlist[i]);
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
     private void InitializeSFXPool()
    {
        // 초기 풀 생성: AudioSource 오브젝트를 initialPoolSize만큼 만들어서 큐에 넣어둠
        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject go = new GameObject("SFXSource");
            go.transform.SetParent(transform);
            AudioSource a = go.AddComponent<AudioSource>();
            a.outputAudioMixerGroup = mixer.FindMatchingGroups("SFX")[0];
            a.playOnAwake = false;
            go.SetActive(false);
            sfxPool.Enqueue(a);
        }
    }

    /// <summary>
    /// 1회용 SFX 재생 (풀에서 AudioSource 꺼내서 Play → 끝나면 풀로 복귀)
    /// </summary>
    public void SFXPlay(string sfxName, int sfxNum)
    {
        AudioSource audioSource = GetAudioSourceFromPool();
        audioSource.clip = sfxlist[sfxNum];
        audioSource.volume = 1f;
        audioSource.loop = false;
        audioSource.gameObject.SetActive(true);
        audioSource.Play();
        StartCoroutine(DeactivateAfterPlay(audioSource));
    }

    /// <summary>
    /// 1회용 SFX 재생(볼륨 지정 버전)
    /// </summary>
    public void SFXPlay(string sfxName, int sfxNum, float volume)
    {
        AudioSource audioSource = GetAudioSourceFromPool();
        audioSource.clip = sfxlist[sfxNum];
        audioSource.volume = volume;
        audioSource.loop = false;
        audioSource.gameObject.SetActive(true);
        audioSource.Play();
        StartCoroutine(DeactivateAfterPlay(audioSource));
    }

    /// <summary>
    /// 텍스트(나레이션 등) 재생할 때, 1회용으로 사용
    /// </summary>
    public void SFXTextPlay(string textName, int textNum, float volume = 1)
    {
        AudioSource audioSource = GetAudioSourceFromPool();
        audioSource.clip = txtlist[textNum];
        audioSource.volume = volume;
        audioSource.loop = false;
        audioSource.gameObject.SetActive(true);
        audioSource.Play();
        StartCoroutine(DeactivateAfterPlay(audioSource));
    }

    /// <summary>
    /// 1회용 SFX용 AudioSource를 풀에서 꺼내거나, 풀에 남아있는 게 없으면 새로 생성
    /// </summary>
    private AudioSource GetAudioSourceFromPool()
    {
        if (sfxPool.Count > 0)
        {
            AudioSource source = sfxPool.Dequeue();
            return source;
        }
        else
        {
            GameObject go = new GameObject("SFXSource");
            go.transform.SetParent(transform);
            AudioSource audioSource = go.AddComponent<AudioSource>();
            audioSource.outputAudioMixerGroup = mixer.FindMatchingGroups("SFX")[0];
            audioSource.playOnAwake = false;
            return audioSource;
        }
    }
    /// <summary>
    /// 루프 사운드 재생 시작 (ID로 식별) 
    /// 이미 재생 중이라면 무시
    /// </summary>
    public void SFXPlayLoop(int sfxNum, float volume = 1f)
    {
        // 이미 재생 중이면 중복 재생 금지
        if (loopingSources.ContainsKey(sfxNum))
            return;

        // 풀에서 AudioSource 꺼내기
        AudioSource source = GetAudioSourceFromPool();
        source.clip = sfxlist[sfxNum];
        source.volume = volume;
        source.loop = true;
        source.gameObject.SetActive(true);
        source.Play();

        loopingSources.Add(sfxNum, source);
    }

    /// <summary>
    /// 루프 사운드 정지 (ID로 식별하여 해당 AudioSource를 풀로 반환)
    /// </summary>
    public void SFXStopLoop(int sfxNum)
    {
        if (!loopingSources.ContainsKey(sfxNum))
            return;

        AudioSource source = loopingSources[sfxNum];
        source.Stop();
        source.loop = false;
        source.gameObject.SetActive(false);

        // 풀에 되돌리기
        sfxPool.Enqueue(source);
        loopingSources.Remove(sfxNum);
    }
    /// <summary>
    /// 클립이 끝나면 AudioSource를 꺼서 풀에 재등록
    /// </summary>
    private IEnumerator DeactivateAfterPlay(AudioSource audioSource)
    {
        // 클립 길이만큼 대기
        yield return new WaitForSeconds(audioSource.clip.length);
        audioSource.Stop();
        audioSource.gameObject.SetActive(false);
        sfxPool.Enqueue(audioSource);
    }
    public void BGSoundSave(AudioClip clip)
    {
        bgSound.clip = clip;
    }

    // 배경 음악 재생
    public void BGSoundPlay(float volume =0.6f)
    {
        if (bgSound.clip != null)
        {
            bgSound.outputAudioMixerGroup = mixer.FindMatchingGroups("BG")[0];
            bgSound.loop = true;
            bgSound.volume = volume;
            bgSound.Play();
        }
    }

    public void BGSoundPlay(int textNum, float volume = 0.6f)
    {
        if (bgSound.clip != null)
        {
            bgSound.clip = bglist[textNum];
            bgSound.outputAudioMixerGroup = mixer.FindMatchingGroups("BG")[0];
            bgSound.loop = true;
            bgSound.volume = volume;
            bgSound.Play();
        }
    }

    public void BGSoundPlayDelayed(int bgNum, float delay, float volume = 0.6f)
    {
        StartCoroutine(PlayBGAfterDelay(bgNum, delay, volume));
    }

    private IEnumerator PlayBGAfterDelay(int bgNum, float delay, float volume = 0.6f)
    {
        yield return new WaitForSeconds(delay);

        if (bgNum >= 0 && bgNum < bglist.Length)
        {
            bgSound.clip = bglist[bgNum];
            bgSound.outputAudioMixerGroup = mixer.FindMatchingGroups("BG")[0];
            bgSound.loop = true;
            bgSound.volume = volume;
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
