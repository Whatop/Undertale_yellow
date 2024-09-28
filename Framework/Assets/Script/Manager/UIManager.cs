using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    GameManager gameManager;

    private static UIManager instance;
    public GameObject[] ui_positions; //health : 0, Weapon : 1, hp2 : 0

    [SerializeField]
    private GameObject[] ui_healths;

    [SerializeField]
    private GameObject[] ui_ammo;
    public GameObject[] pedestal;
    public Image ui_weaponImage;
    public Image ui_weaponBackImage;
    public GameObject ui_healthImage;
    public TextMeshProUGUI ui_ammoText;
    public Canvas uicanvas;
    public Camera mainCamera;
    public TextMeshProUGUI damageTextPrefab; // DamageText 프리팹

    // Option
    public GameObject optionPanel;
    public GameObject puasePanel;
    public GameObject keyChangePanel;
    public GameObject YN_ResetPanel;
    public GameObject KeyCheckPanel;

    public Button[] mainButtons;
    public Button[] optionButtons;
    public Button[] keyChangeButtons;
    public Button[] YNButtons;

    public Button[] keyChangefunctions;

    [SerializeField]
    private Button[] currentButtons;
    public Button fullScreenToggle;
    public Scrollbar brightnessScrollbar;
    public Button vSyncToggle;
    public Button cusorToggle;
    public Scrollbar bgmScrollbar;
    public Scrollbar sfxScrollbar;
    public Scrollbar cameraShakeScrollbar;
    public Scrollbar miniMapSizeScrollbar;

    public Sprite onSprite;
    public Sprite offSprite;

    public bool isFullScreen = false;
    public bool isVSyncOn = false;
    public bool isCursorVisible = true;

    [SerializeField]
    private int currentIndex = 0;

    [SerializeField]
    private int curRIndex = 6; // curResolutionsIndex
    private List<Resolution> predefinedResolutions;

    public GameObject[] Interface;
    public bool isUserInterface = false;

    /// <summary>
    /// 0 : Up
    /// 1 : Down
    /// 2 : Left
    /// 3 : Right
    /// 4 : Shot
    /// 5 : Roll
    /// 6 : Check
    /// 7 : Inventroy
    /// 8 : Map
    /// </summary>
    private KeyCode[] keyBindings = new KeyCode[9]; // 9개의 키 설정을 위한 배열
    private bool isWaitingForKey = false; // 키 입력 대기 상태를 나타내는 플래그
    private int currentKeyIndex = 0; // 현재 설정 중인 키의 인덱스

    //PlayerUI
    public Slider reloadSlider; // 재장전 슬라이더 UI


    public TextMeshProUGUI currentResolutionText;

    [SerializeField]
    private string currentPanel = "Default"; // 현재 패널을 추적
    private string prevPanel = "Default"; // 이전 패널을 추적

    //panel
    /// <summary>
    /// 0 : Up
    /// 1 : Down
    /// </summary>
    public GameObject[] textbarPos;
    public GameObject textbar;
    public TextMeshProUGUI text;
    public Image npcFaceImage;
    
    //gameover UI
    public GameObject gameover_Object;
    public TextMeshProUGUI gameover_text;
    public Image gameover_image;
    // heartbreak sound 87->89
    // heartbreak_c sound 88->90

    private SoundManager soundManager; // SoundManager 인스턴스를 필드로 추가
    public static UIManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<UIManager>();
                if (instance == null)
                {
                    GameObject obj = new GameObject("UIManager");
                    instance = obj.AddComponent<UIManager>();
                }
            }
            return instance;
        }
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        gameManager = GameManager.Instance;
        soundManager = SoundManager.Instance;
    }

    private void Start()
    {
        LoadSettings();
        predefinedResolutions = new List<Resolution>
    {
        new Resolution { width = 640, height = 480 },
        new Resolution { width = 720, height = 480 },
        new Resolution { width = 800, height = 600 },
        new Resolution { width = 1024, height = 768 },
        new Resolution { width = 1280, height = 720 },
        new Resolution { width = 1440, height = 1080 },
        new Resolution { width = 1920, height = 1080 },
        new Resolution { width = 2560, height = 1440 }
    };
        keyBindings[0] = KeyCode.W;
        keyBindings[1] = KeyCode.S;
        keyBindings[2] = KeyCode.A;
        keyBindings[3] = KeyCode.D;
        keyBindings[4] = KeyCode.Mouse0;
        keyBindings[5] = KeyCode.Mouse1;
        keyBindings[6] = KeyCode.F;
        keyBindings[7] = KeyCode.E;
        keyBindings[8] = KeyCode.Tab;
        int screenWidth = 1920;
        int screenHeight = 1080;
        float screenRatio = (float)screenWidth / screenHeight;

        Screen.SetResolution(screenWidth, screenHeight, Screen.fullScreen);

        for (int i = 0; i < mainButtons.Length; i++)
        {
            int index = i;  // 클로저를 위한 로컬 복사본
            mainButtons[i].onClick.AddListener(() => OnButtonClick(index));
            AddEventTriggerListener(mainButtons[i].gameObject, EventTriggerType.PointerEnter, () => OnButtonHover(index));
        }
        for (int i = 0; i < optionButtons.Length; i++)
        {
            int index = i;  // 클로저를 위한 로컬 복사본
            optionButtons[i].onClick.AddListener(() => OnButtonClick(index));
            AddEventTriggerListener(optionButtons[i].gameObject, EventTriggerType.PointerEnter, () => OnButtonHover(index));
        }
        for (int i = 0; i < keyChangeButtons.Length; i++)
        {
            int index = i;  // 클로저를 위한 로컬 복사본
            keyChangeButtons[i].onClick.AddListener(() => OnButtonClick(index));
            AddEventTriggerListener(keyChangeButtons[i].gameObject, EventTriggerType.PointerEnter, () => OnButtonHover(index));
        }
        for (int i = 0; i < YNButtons.Length; i++)
        {
            int index = i;  // 클로저를 위한 로컬 복사본
            YNButtons[i].onClick.AddListener(() => OnButtonClick(index));
            AddEventTriggerListener(YNButtons[i].gameObject, EventTriggerType.PointerEnter, () => OnButtonHover(index));
        }
        InitHeart();
        InitWeapon();
        ShowPanel("Game");
        OptionInput();
        UpdateUI();
    }

    public void TextBarOpen()
    {
        // 플레이어 위치 가져오기
        Vector3 playerPosition = gameManager.GetPlayerData().position;

        // 카메라 위치 가져오기
        Vector3 cameraPosition = Camera.main.transform.position;
        if (playerPosition.y > cameraPosition.y)
        {
            // 플레이어가 카메라보다 높이 있는 경우
            textbar.transform.position = textbarPos[1].transform.position;
        }
        else 
        {
            // 플레이어가 카메라보다 낮게 있는 경우
            textbar.transform.position = textbarPos[0].transform.position;
        }
        textbar.SetActive(true);
    }
    public void CloseTextbar()
    {
        textbar.SetActive(false);
    }
    private void Update()
    {
        if (isUserInterface)
        {
            Time.timeScale = 0f;
        }
        else
        {
            gameManager.ResumeGame();
        }

        if (currentIndex > currentButtons.Length)
        {
            OnButtonHover(0);
        }
        if (isWaitingForKey)
        {
            DetectKeyInput();
        }
        UpdateUI();
        OptionInput();
    }
    public void LoadIntro()
    {
        SceneManager.LoadScene("IntroScene");
    }
    #region KeyBoardUi
    private void DetectKeyInput()
    {
        foreach (KeyCode keyCode in System.Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(keyCode))
            {
                if (keyCode == KeyCode.Escape)
                {
                    // ESC를 눌렀을 때 입력 취소
                    isWaitingForKey = false;
                    Debug.Log("Key binding cancelled.");
                }
                else
                {
                    // 다른 키를 눌렀을 때 해당 키로 저장
                    // 동일한 키가 있는지 확인
                    for (int i = 0; i < keyBindings.Length; i++)
                    {
                        if (keyBindings[i] == keyCode)
                        {
                            keyBindings[i] = KeyCode.None; // 기존 키를 None으로 설정
                            break;
                        }
                    }

                    keyBindings[currentKeyIndex] = keyCode;
                    isWaitingForKey = false;
                    SaveKeyBindings();
                    Debug.Log("Key binding set to: " + keyCode);
                }
                CloseKeyCheck();
                UpdateKeyBindingUI();
                break;
            }
        }
    }
    public void StartKeyBinding(int index)
    {
        currentKeyIndex = index;
        isWaitingForKey = true;
        Debug.Log("Press any key to bind to action " + index);
    }

    private void SaveKeyBindings()
    {
        for (int i = 0; i < keyBindings.Length; i++)
        {
            PlayerPrefs.SetString("KeyBinding" + i, keyBindings[i].ToString());
        }
        PlayerPrefs.Save();
    }

    private void LoadKeyBindings()
    {
        for (int i = 0; i < keyBindings.Length; i++)
        {
            string keyString = PlayerPrefs.GetString("KeyBinding" + i, KeyCode.None.ToString());
            keyBindings[i] = (KeyCode)System.Enum.Parse(typeof(KeyCode), keyString);
        }
        UpdateKeyBindingUI();
    }

    private void UpdateKeyBindingUI()
    {
        for (int i = 0; i < keyChangefunctions.Length; i++)
        {
            TextMeshProUGUI buttonText = keyChangefunctions[i].GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                if (keyBindings[i].ToString() == "Mouse0")
                    buttonText.text = "Left Mouse Button";
                else if (keyBindings[i].ToString() == "Mouse1")
                    buttonText.text = "Right Mouse Button";
                else
                    buttonText.text = keyBindings[i].ToString();
            }
        }
    }

    public void OpenKeyCheck()
    {
        KeyCheckPanel.SetActive(true);
    }
    public void CloseKeyCheck()
    {
        KeyCheckPanel.SetActive(false);
    }

    #endregion

    #region playerUi

    void InitHeart() // 체력 새팅
    {
        PlayerData player = gameManager.GetPlayerData();
        int heart_count = player.Maxhealth / 2; // 총 체력에 맞는 하트 개수 설정
        ui_healths = new GameObject[heart_count];

        // 체력바 전체 길이를 설정하기 위한 부모 UI RectTransform
        RectTransform parentRect = ui_positions[0].GetComponent<RectTransform>();

        // 각 하트의 가로 간격 (여백 포함)을 결정합니다.
        float totalWidth = parentRect.rect.width; // 부모 UI의 너비
        float padding = 10f; // 하트 간의 간격 (필요에 따라 조정)
        float heartWidth = (totalWidth - padding * (heart_count - 1)) / heart_count; // 각 하트의 너비
        ui_healthImage.SetActive(true);

        for (int i = 0; i < heart_count; i++)
        {
            GameObject heartPrefab = Resources.Load<GameObject>("Prefabs/Heart");
            GameObject instance = Instantiate(heartPrefab, ui_positions[0].transform);

            RectTransform heartRect = instance.GetComponent<RectTransform>();

            // 하트의 크기를 부모 UI 너비에 맞춰 조정합니다.
            heartRect.sizeDelta = new Vector2(heartWidth, heartRect.sizeDelta.y);

            // 하트를 가로 방향으로 배치합니다.
            Vector3 newPosition = instance.transform.position;

            if (!GameManager.Instance.isBattle)
            {
                newPosition.x = parentRect.position.x - totalWidth / 2 + (heartWidth + padding) * i + heartWidth / 2; // 가로 배치
            }
            else
            {
                newPosition.x = ui_positions[2].transform.position.x + (heartWidth + padding) * i; // 전투 상태일 때 위치 조정
            }

            instance.transform.position = newPosition;
            ui_healths[i] = instance;
        }
    }

    void InitWeapon() // 총 새팅
    {
        Weapon weapon = gameManager.GetWeaponData();

        int ammo_count = weapon.magazine;
        ui_ammo = new GameObject[ammo_count];

        for (int i = 0; i < ammo_count; i++)
        {
            GameObject weaponPrefab = Resources.Load<GameObject>("Prefabs/Ammo");
            GameObject instance = Instantiate(weaponPrefab, ui_positions[1].transform);

            float sizeY = instance.GetComponent<RectTransform>().sizeDelta.y;
            Vector3 newPosition = instance.transform.position;
            newPosition.y = ui_positions[1].transform.position.y + i * sizeY * 1.25f; // 세로 방향으로 위치 설정
            instance.transform.position = newPosition;
            ui_ammo[i] = instance;
        }
    }

    public void ShowDamageText(Vector3 worldPosition, int damageAmount)
    { // 월드 좌표를 스크린 좌표로 변환
        Vector2 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);

        // 스크린 좌표를 Canvas 안에서 사용 가능한 위치로 변환
        RectTransformUtility.ScreenPointToLocalPointInRectangle(uicanvas.transform as RectTransform, screenPosition, uicanvas.worldCamera, out Vector2 canvasPosition);

        // 텍스트 생성
        TextMeshProUGUI damageText = Instantiate(damageTextPrefab, uicanvas.transform);
        damageText.rectTransform.localPosition = canvasPosition;
        damageText.text = damageAmount.ToString();
        damageText.GetComponent<DamageText>().Initialize(damageAmount);
    }

    public void UpdateUI()
    {
        SetCombatMode();
        PlayerData player = gameManager.GetPlayerData();
        int currentHealth = player.health;

        // 체력 이미지 업데이트
        RectTransform parentRect = ui_positions[0].GetComponent<RectTransform>();
        float totalWidth = parentRect.rect.width; // 부모 UI의 너비
        float padding = 10f; // 하트 간의 간격
        int heart_count = ui_healths.Length;
        float heartWidth = (totalWidth - padding * (heart_count - 1)) / heart_count; // 각 하트의 너비

        for (int i = 0; i < heart_count; i++)
        {
            // 체력에 따른 스프라이트 인덱스 계산
            int spriteIndex = Mathf.Clamp(currentHealth - i * 2, 0, 2);
            ui_healths[i].GetComponent<ImageScript>().SetImage(spriteIndex);

            // 하트의 위치를 다시 설정 (체력 UI가 갱신될 때마다 위치를 다시 조정)
            RectTransform heartRect = ui_healths[i].GetComponent<RectTransform>();
            Vector3 newPosition = ui_healths[i].transform.position;

            if (!GameManager.Instance.isBattle)
            {
                newPosition.x = parentRect.position.x - totalWidth / 2 + (heartWidth + padding) * i + heartWidth / 2; // 가로 배치
                ui_healths[i].GetComponent<Image>().color = new Color(1, 1, 1, 0.75f);
            }
            else
            {
                newPosition.x = ui_positions[2].transform.position.x + (heartWidth + padding) * i; // 전투 상태일 때 위치 조정
                ui_healths[i].GetComponent<Image>().color = new Color(1, 1, 1, 1);
            }

            // 위치 적용
            ui_healths[i].transform.position = newPosition;
        }

        // 무기 데이터 가져오기 및 총알 이미지 업데이트
        Weapon weapon = gameManager.GetWeaponData();
        int current_magazine = weapon.current_magazine;

        // 총알 이미지 업데이트
        for (int i = 0; i < ui_ammo.Length; i++)
        {
            int spriteIndex = (i < current_magazine) ? 0 : 1; // 총알 개수에 따른 스프라이트 인덱스 계산
            ui_ammo[i].GetComponent<ImageScript>().SetImage(spriteIndex);
        }

        // 무기 탄약 텍스트 업데이트
        ui_ammoText.text = weapon.current_Ammo + "/" + weapon.maxAmmo;
    }

    public void SetCombatMode()
    {
        if (gameManager.isBattle || GameManager.Instance.GetPlayerData().isInvincible)
        {
            OnPlayerUI(); // 전투 상태에서는 UI를 보여줌

        }
        else
        {
            OffPlayerUI(); // 비전투 상태에서는 UI를 숨김
        }
    }
    // 체력, 무기 등의 UI를 끄는 함수
    public void OffPlayerUI()
    {
        foreach (var ui in ui_ammo)
        {
            ui.gameObject.SetActive(false);
        }
        foreach (var ui in pedestal)
        {
            ui.gameObject.SetActive(false);
        }
        ui_weaponImage.gameObject.SetActive(false);
        ui_ammoText.gameObject.SetActive(false);
        ui_weaponBackImage.gameObject.SetActive(false);
        ui_healthImage.gameObject.SetActive(false);
    }

    // 체력, 무기 등의 UI를 켜는 함수
    public void OnPlayerUI()
    {
        foreach (var ui in ui_ammo)
        {
            ui.gameObject.SetActive(true);
        }
        foreach (var ui in pedestal)
        {
            ui.gameObject.SetActive(true);
        }
        ui_weaponImage.gameObject.SetActive(true);
        ui_ammoText.gameObject.SetActive(true);
        ui_weaponBackImage.gameObject.SetActive(true);
        ui_healthImage.gameObject.SetActive(true);
    }
    #endregion

    #region optionUI

    public void OpenYNReset()
    {
        YN_ResetPanel.SetActive(true);
        ShowPanel("YNCheck");

    }
    public void CloseYNReset()
    {
        YN_ResetPanel.SetActive(false);
    }

    public void SaveSettings()
    {
        // 볼륨 설정 저장
        PlayerPrefs.SetFloat("BGMVolume", bgmScrollbar.value);
        PlayerPrefs.SetFloat("SFXVolume", sfxScrollbar.value);

        // 전체화면 설정 저장
        PlayerPrefs.SetInt("IsFullScreen", isFullScreen ? 1 : 0);

        // VSync 설정 저장
        PlayerPrefs.SetInt("IsVSyncOn", isVSyncOn ? 1 : 0);

        // 커서 설정 저장
        PlayerPrefs.SetInt("IsCursorVisible", isCursorVisible ? 1 : 0);

        // 밝기 설정 저장
        PlayerPrefs.SetFloat("Brightness", brightnessScrollbar.value);

        // 기타 설정 저장
        PlayerPrefs.SetFloat("CameraShake", cameraShakeScrollbar.value);
        PlayerPrefs.SetFloat("MiniMapSize", miniMapSizeScrollbar.value);

        // 해상도 인덱스 저장
        PlayerPrefs.SetInt("ResolutionIndex", curRIndex);

        PlayerPrefs.Save();
    }
    public void LoadSettings()
    {
        // 볼륨 설정 불러오기
        if (PlayerPrefs.HasKey("BGMVolume"))
        {
            bgmScrollbar.value = PlayerPrefs.GetFloat("BGMVolume");
        }

        if (PlayerPrefs.HasKey("SFXVolume"))
        {
            sfxScrollbar.value = PlayerPrefs.GetFloat("SFXVolume");
        }

        // 전체화면 설정 불러오기
        if (PlayerPrefs.HasKey("IsFullScreen"))
        {
            isFullScreen = PlayerPrefs.GetInt("IsFullScreen") == 1;
            Screen.fullScreen = isFullScreen;
            fullScreenToggle.image.sprite = isFullScreen ? onSprite : offSprite;
        }

        // VSync 설정 불러오기
        if (PlayerPrefs.HasKey("IsVSyncOn"))
        {
            isVSyncOn = PlayerPrefs.GetInt("IsVSyncOn") == 1;
            QualitySettings.vSyncCount = isVSyncOn ? 1 : 0;
            vSyncToggle.image.sprite = isVSyncOn ? onSprite : offSprite;
        }

        // 커서 설정 불러오기
        if (PlayerPrefs.HasKey("IsCursorVisible"))
        {
            isCursorVisible = PlayerPrefs.GetInt("IsCursorVisible") == 1;
            Cursor.visible = isCursorVisible;
            cusorToggle.image.sprite = isCursorVisible ? onSprite : offSprite;
        }

        // 밝기 설정 불러오기
        if (PlayerPrefs.HasKey("Brightness"))
        {
            brightnessScrollbar.value = PlayerPrefs.GetFloat("Brightness");
            // 여기서 밝기 조정 코드를 추가하세요
        }

        // 기타 설정 불러오기
        if (PlayerPrefs.HasKey("CameraShake"))
        {
            cameraShakeScrollbar.value = PlayerPrefs.GetFloat("CameraShake");
        }

        if (PlayerPrefs.HasKey("MiniMapSize"))
        {
            miniMapSizeScrollbar.value = PlayerPrefs.GetFloat("MiniMapSize");
        }

        // 해상도 인덱스 불러오기
        if (PlayerPrefs.HasKey("ResolutionIndex"))
        {
            curRIndex = PlayerPrefs.GetInt("ResolutionIndex");
            // 해상도 설정 변경 코드 추가
        }
    }
    public void ResetSettings()
    {
        PlayerPrefs.DeleteAll();
        // 기본값 설정
        bgmScrollbar.value = 0.5f; // 기본 볼륨 값
        sfxScrollbar.value = 0.5f;  // 기본 볼륨 값
        isFullScreen = true;
        isVSyncOn = true;
        isCursorVisible = true;
        brightnessScrollbar.value = 0.5f; // 기본 밝기 값
        cameraShakeScrollbar.value = 0.5f;  // 기본 카메라 흔들림 값
        miniMapSizeScrollbar.value = 0.5f;  // 기본 미니맵 크기 값
        curRIndex = 0; // 기본 해상도 인덱스

        keyBindings[0] = KeyCode.W;
        keyBindings[1] = KeyCode.S;
        keyBindings[2] = KeyCode.A;
        keyBindings[3] = KeyCode.D;
        keyBindings[4] = KeyCode.Mouse0;
        keyBindings[5] = KeyCode.Mouse1;
        keyBindings[6] = KeyCode.F;
        keyBindings[7] = KeyCode.E;
        keyBindings[8] = KeyCode.Tab;
        // UI 업데이트
        Screen.fullScreen = isFullScreen;
        fullScreenToggle.image.sprite = isFullScreen ? onSprite : offSprite;
        QualitySettings.vSyncCount = isVSyncOn ? 1 : 0;
        vSyncToggle.image.sprite = isVSyncOn ? onSprite : offSprite;
        Cursor.visible = isCursorVisible;
        cusorToggle.image.sprite = isCursorVisible ? onSprite : offSprite;

        // 저장
        SaveSettings();
    }
    public void exitGame()
    {
        Debug.Log("Game is exiting...");
        Application.Quit();
    }
    public void ShowPanel(string panelName)
    {
        switch (panelName)
        {
            case "Main":
                currentPanel = "Main";
                isUserInterface = true;
                currentButtons = mainButtons;
                puasePanel.SetActive(true);
                optionPanel.SetActive(false);
                keyChangePanel.SetActive(false);
                break;
            case "Option":
                currentPanel = "Option";
                currentButtons = optionButtons;
                isUserInterface = true;
                puasePanel.SetActive(false);
                optionPanel.SetActive(true);
                keyChangePanel.SetActive(false);
                prevPanel = currentPanel;
                break;
            case "KeyChange":
                currentPanel = "KeyChange";
                currentButtons = keyChangeButtons;
                isUserInterface = true;
                puasePanel.SetActive(false);
                optionPanel.SetActive(false);
                keyChangePanel.SetActive(true);
                prevPanel = currentPanel;
                break;
            case "YNCheck":
                currentPanel = "YNCheck";
                currentButtons = YNButtons;
                break;
            case "Return":
                ShowPanel(prevPanel);
                break;
            default:
                currentPanel = "Game";
                isUserInterface = false;
                currentButtons = mainButtons;
                puasePanel.SetActive(false);
                optionPanel.SetActive(false);
                keyChangePanel.SetActive(false);
                break;
        }
        currentIndex = 0; // 패널 변경 시 인덱스 초기화
        UpdateSelection();
    }
    void Navigate(int direction)
    {
        currentIndex = (currentIndex + direction + currentButtons.Length) % currentButtons.Length;
        UpdateSelection();
    }

    void OptionInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentPanel == "KeyChange")
            {
                ShowPanel("Option");
                CloseYNReset();
                SaveSettings();
            }
            else if (currentPanel == "Option")
            {
                ShowPanel("Main");
                CloseYNReset();
                SaveSettings();
            }
            else if (currentPanel == "Main")
            {
                ShowPanel("Game");
                gameManager.ResumeGame();
                Time.timeScale = 1f;
                soundManager.ResumeBGSound();
                CloseYNReset();
            }
            else if (currentPanel == "YNCheck")
            {
                CloseYNReset();
                ShowPanel("Return");
            }
            else
            {
                ShowPanel("Main");
                Time.timeScale = 0f;
                soundManager.PauseBGSound();
            }
            soundManager.SFXPlay("mus_piano1", 32);
        }

        if (isUserInterface)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                Navigate(-1);
                soundManager.SFXPlay("snd_piano3", 34);
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            {
                Navigate(1);
                soundManager.SFXPlay("snd_piano4", 35);
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            {
                soundManager.SFXPlay("snd_piano5", 36);
                AdjustValue(-1);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            {
                soundManager.SFXPlay("snd_piano5", 36);
                AdjustValue(1);
            }
            else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
            {
                if (currentButtons != null && currentButtons.Length > 0)
                {
                    if (currentPanel == "Option")
                    {
                        if (currentIndex == 1 || currentIndex == 3 || currentIndex == 4)
                            ToggleValue();  // ToggleValue 호출
                        else
                        {
                            currentButtons[currentIndex].onClick.Invoke();
                            soundManager.SFXPlay("snd_piano6", 37);
                        }
                    }
                    else
                    {
                        currentButtons[currentIndex].onClick.Invoke();
                        soundManager.SFXPlay("snd_piano6", 37);
                    }
                }
            }
        }
    }

    void AdjustValue(int direction)
    {
        switch (currentIndex)
        {
            case 2: // 밝기 조절
                brightnessScrollbar.value = Mathf.Clamp(brightnessScrollbar.value + direction * 0.1f, 0, 1);
                break;
            case 5: // BGM 볼륨 조절
                bgmScrollbar.value = Mathf.Clamp(bgmScrollbar.value + direction * 0.1f, 0, 1);
                soundManager.BGSoundVolume(bgmScrollbar.value);
                break;
            case 6: // SFX 볼륨 조절
                sfxScrollbar.value = Mathf.Clamp(sfxScrollbar.value + direction * 0.1f, 0, 1);
                soundManager.SFXSoundVolume(sfxScrollbar.value);
                break;
            case 7: // 카메라 흔들림 조절
                cameraShakeScrollbar.value = Mathf.Clamp(cameraShakeScrollbar.value + direction * 0.1f, 0, 1);
                break;
            case 8: // 미니맵 크기 조절
                miniMapSizeScrollbar.value = Mathf.Clamp(miniMapSizeScrollbar.value + direction * 0.1f, 0, 1);
                break;
            case 0: // 해상도 변경
                ChangeResolution(direction);
                break;
        }
    }

    // 5
    public void SetBGVolume()
    {
        if (soundManager != null)
        {
            soundManager.BGSoundVolume(bgmScrollbar.value);
        }
    }

    public void SetSFXVolume()
    {
        if (soundManager != null)
        {
            soundManager.SFXSoundVolume(sfxScrollbar.value);
        }
    }
    //
    void ToggleValue()
    {
        switch (currentIndex)
        {
            case 1: // 전체 화면 토글
                ToggleFullScreen();
                break;
            case 3: // 수직 동기화 토글
                ToggleVSync();
                break;
            case 4: // 커서 토글
                ToggleCursorVisibility();
                break;
        }
        StartCoroutine(ForceToggleUpdate());
    }
    void UpdateButtonState(Button button, bool state)
    {
        SpriteState spriteState = button.spriteState;

        if (state)
        {
            button.image.sprite = onSprite;
            spriteState.highlightedSprite = onSprite;
            spriteState.pressedSprite = onSprite;
            spriteState.selectedSprite = onSprite;
            spriteState.disabledSprite = onSprite;
        }
        else
        {
            button.image.sprite = offSprite;
            spriteState.highlightedSprite = offSprite;
            spriteState.pressedSprite = offSprite;
            spriteState.selectedSprite = offSprite;
            spriteState.disabledSprite = offSprite;
        }

        button.spriteState = spriteState;
    }

    void ToggleFullScreen()
    {
        isFullScreen = !isFullScreen;
        Screen.fullScreen = isFullScreen;
        UpdateButtonState(fullScreenToggle, isFullScreen);
    }

    void ToggleVSync()
    {
        isVSyncOn = !isVSyncOn;
        QualitySettings.vSyncCount = isVSyncOn ? 1 : 0;
        UpdateButtonState(vSyncToggle, isVSyncOn);
    }

    void ToggleCursorVisibility()
    {
        isCursorVisible = !isCursorVisible;
        Cursor.visible = isCursorVisible;
        UpdateButtonState(cusorToggle, isCursorVisible);
    }

    IEnumerator ForceToggleUpdate()
    {
        yield return null; // Wait for one frame
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(currentButtons[currentIndex].gameObject);
    }
    void UpdateSelection()
    {
        for (int i = 0; i < currentButtons.Length; i++)
        {
            ColorBlock colors = currentButtons[i].colors;
            colors.normalColor = Color.white; // 기본 색상
            colors.highlightedColor = Color.white; // 강조 색상
            colors.pressedColor = Color.gray; // 클릭 시 색상
            colors.selectedColor = (i == currentIndex) ? Color.white : Color.white; // 선택된 색상
            currentButtons[i].colors = colors;

            // 텍스트 색상 변경
            TextMeshProUGUI buttonText = currentButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.color = (i == currentIndex) ? Color.white : Color.gray; // 강조 색상
            }
        }
    }

    public void OnButtonClick(int index)
    {
        currentIndex = index;
        UpdateSelection();
        soundManager.SFXPlay("snd_piano2", 33);
    }
    public void ChangeResolution(int direction)
    {
        curRIndex = (curRIndex + direction + predefinedResolutions.Count) % predefinedResolutions.Count;
        Resolution selectedResolution = predefinedResolutions[curRIndex];
        Screen.SetResolution(selectedResolution.width, selectedResolution.height, Screen.fullScreen);
        Debug.Log("Resolution changed to: " + selectedResolution.width + " x " + selectedResolution.height);

        // 현재 해상도 텍스트 업데이트
        UpdateCurrentResolutionText();
    }

    void UpdateCurrentResolutionText()
    {
        currentResolutionText.text = Screen.width + " x " + Screen.height;
    }

    //마우스 호버 이벤트 리스너
    void AddEventTriggerListener(GameObject target, EventTriggerType eventType, System.Action action)
    {
        EventTrigger trigger = target.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = target.AddComponent<EventTrigger>();
        }

        EventTrigger.Entry entry = new EventTrigger.Entry { eventID = eventType };
        entry.callback.AddListener((eventData) => action());
        trigger.triggers.Add(entry);
    }
    ///<summary>
    /// OnClickEvnet와 똑같은 코드지만 , 이친구는 마우스가 위에 올려졌을때 작동됨
    ///</summary>

    void OnButtonHover(int index)
    {
        currentIndex = index;
        UpdateSelection();
    }
    #endregion
    //player UI
    public void ShowReloadSlider(bool show)
    {
        reloadSlider.gameObject.SetActive(show);
    }

    public void SetReloadSliderMaxValue(float maxValue)
    {
        reloadSlider.maxValue = maxValue;
    }

    public void SetReloadSliderValue(float value)
    {
        reloadSlider.value = value;
    }

    /// <summary>
    /// 0 : Up
    /// 1 : Down
    /// 2 : Left
    /// 3 : Right
    /// 4 : Shot
    /// 5 : Roll
    /// 6 : Check
    /// 7 : Inventory
    /// 8 : Map
    /// </summary>
    public KeyCode GetKeyCode(int i)
    {
        // 인덱스 범위를 확인하여 유효한 경우에만 반환
        if (i >= 0 && i < keyBindings.Length)
        {
            return keyBindings[i];
        }

        // 유효하지 않은 인덱스일 경우 기본값(KeyCode.None) 반환
        return KeyCode.None;
    }


    #region gameOver
    public void playGameover()
    {
        gameover_Object.SetActive(true);
        soundManager.StopBGSound();
        soundManager.SFXPlayDelayed("heartbreak1", 87, 0.1f);
        soundManager.SFXPlayDelayed("heartbreak2", 89, 1f);
        soundManager.BGSoundPlayDelayed(4, 3);
        //gameover_Object;
        //gameover_text;
        //gameover_image;
        StartCoroutine(FadeIn());
        StartCoroutine(gameoverTextOn());


    } 
    private IEnumerator gameoverTextOn()
    {
        yield return new WaitForSeconds(3f);
        gameover_text.text = "you cannot aruound dont give up. . .";

        gameover_text.GetComponent<TypeEffect>().SetMsg(gameover_text.text, OnSentenceComplete);
    }
    private IEnumerator FadeIn()
    {
        yield return new WaitForSeconds(3f);
        float duration = 2f; // 2 seconds
        float currentTime = 0f;

        // Get the current color of the image
        Color color = gameover_image.color;
        color.a = 0f; // Start with alpha at 0
        gameover_image.color = color;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(currentTime / duration); // Alpha value from 0 to 1

            // Update the alpha of the image color
            color.a = alpha;
            gameover_image.color = color;

            yield return null;
        }

        // Ensure the alpha is fully set to 1 after the loop
        color.a = 1f;
        gameover_image.color = color;
    }
    private void OnSentenceComplete()
    {
        Debug.Log("문장이 완료되었습니다.");
    }
    #endregion
}
