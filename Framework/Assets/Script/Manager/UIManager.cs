using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
[System.Serializable]
public class RadialSegment
{
    public string segmentName;
    public RectTransform segmentTransform;

    private Vector3 defaultScale = Vector3.one;
    private Vector3 highlightScale = new Vector3(1.1f, 1.1f, 1f);

    public RadialMenuType menuType; // 추가: 어떤 메뉴인지

    public void SetHighlight(bool isOn)
    {
        if (segmentTransform == null) return;
        segmentTransform.localScale = isOn ? highlightScale : defaultScale;
    }

    public void ExecuteAction(int index = 0)
    {
        switch (menuType)
        {
            case RadialMenuType.Emotion:
                Debug.Log($"[감정] {segmentName} 표현 실행");
                // 감정 애니메이션 재생
                UIManager.Instance.PlayEmotionAnimation(segmentName);
                break;

            case RadialMenuType.Item:
                Debug.Log($"[아이템] {segmentName} 사용 시도");
                UIManager.Instance.UseQuickItem(index);
                break;

            case RadialMenuType.Soul:
                Debug.Log($"[영혼] {segmentName} 선택");
                UIManager.Instance.ChangeSoulType(segmentName);
                break;
        }
    }
}

public enum RadialMenuType
{
    None,
    Emotion,
    Item,
    Soul
}

public class UIManager : MonoBehaviour
{
    GameManager gameManager;
    DialogueManager dialogueManager;
    private SoundManager soundManager; // SoundManager 인스턴스를 필드로 추가
    private BattleManager BattleManager; // BattleManager 인스턴스를 필드로 추가

    private static UIManager instance;
    public GameObject[] ui_positions; // Weapon : 0
    public GameObject top;
    public GameObject down;


    [SerializeField]
    private GameObject ammos_td;
    [SerializeField]
    private GameObject[] ui_ammo;
    public GameObject[] pedestal;
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
    public Slider hpBar; // 체력바
    public TextMeshProUGUI hpBar_text;

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

    public Button mouseShotToggle;
    public Button mouseRollToggle;

    public Sprite onSprite;
    public Sprite offSprite;

    public bool isFullScreen = true;
    public bool isVSyncOn = true;
    public bool isCursorVisible = true;
    public bool isMouseShot = true;
    public bool isMouseRoll = true;

    [SerializeField]
    private int currentButtonIndex = 0;

    [SerializeField]
    private int currentRadialIndex = -1;

    //Camera 관련
    [SerializeField]
    private int curRIndex = 6; // curResolutionsIndex
    private List<Resolution> predefinedResolutions;
    public PixelPerfectCamera pixelPerfectCamera;


    public GameObject[] Interface;
    public bool isUserInterface = false;
    /// <summary>
    /// Up: 0 
    /// Down: 1 
    /// Left: 2 
    /// Right: 3 
    /// Shot: 4 
    /// Roll: 5 
    /// Check: 6 
    /// Inventroy, Quick Item: 7 
    /// Quick Soul: 8 
    ///  Quick Emotion: 9
    /// </summary>
    private KeyCode[] keyBindings = new KeyCode[10]; // 10개의 키 설정을 위한 배열
    private bool isWaitingForKey = false; // 키 입력 대기 상태를 나타내는 플래그
    private int currentKeyIndex = 0; // 현재 설정 중인 키의 인덱스

    [SerializeField] float baseBarWidth = 100f;   // MaxHP 20 기준 기본 넓이
    [SerializeField] float extraWidthPerHp = 10f;    // 21 이상부터 HP 1칸당 추가될 픽셀

    RectTransform hpRect;
    public Slider reloadSlider; // 재장전 슬라이더 UI

    public RectTransform crosshairTransform;

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
    public GameObject gameover_soul;
    public Sprite[] soul_sprites;
    // heartbreak sound 87->89
    // heartbreak_c sound 88->90
    // 4,5,6,7 -> 하트 조각
    // 3-> 부서진 

    // Save UI
    public GameObject savePanel;
    /// <summary>
    /// 0. 저장완료
    /// 1. 이름
    /// 2. 레벨
    /// 3. 시간
    /// 4. 장소
    /// 5. 돌아가기
    /// 6. 저장
    /// </summary>
    public TextMeshProUGUI[] savePanel_texts;
    public GameObject savePanel_soul;
    public GameObject[] save_points;
    public bool isSavePanel = false;
    public int saveNum = 0;
    public bool isSaveDelay = false;
    //inventory panel
    /// <summary>
    /// 0. 인벤토리
    /// 1. 아이템
    /// 2. 스텟 
    /// 3. 전화
    /// </summary>
    public int inventroy_panelNum = 0;
    private int item_prevNum = 0;
    public int inventroy_curNum = 0;
    public bool isInventroy = false;

    public GameObject inventroy_panel;
    public GameObject inventroy_simple_panel;
    public GameObject item_panel;
    public GameObject stat_panel;
    public GameObject call_panel;

    public TextMeshProUGUI[] inventroy_texts;
    public TextMeshProUGUI[] inventroy_simple_texts;
    public TextMeshProUGUI[] item_texts;
    public TextMeshProUGUI[] stat_texts;
    public TextMeshProUGUI[] call_texts;
    public TextMeshProUGUI[] interaction_texts;

    public GameObject[] inventroy_points;
    public GameObject[] item_points;
    public GameObject[] call_points;
    public GameObject[] interaction_points;
    public Image inventroy_soul;

    [Header("Radial Menu References")]
    public RadialMenuType currentRadialMenu = RadialMenuType.None;
    public GameObject emotionRadialPanel;
    public GameObject itemRadialPanel;
    public GameObject soulRadialPanel;
    public RectTransform centerPoint;       // 라디얼 메뉴의 중심(마우스 기준점)

    public List<RadialSegment> emotionSegments;
    public List<RadialSegment> itemSegments;
    public List<RadialSegment> soulSegments;

    [Header("Radial Segment Prefabs")]
    public GameObject emotionSegmentPrefab;
    public GameObject itemSegmentPrefab;
    public GameObject soulSegmentPrefab;

    public Slider laserAmmoSlider;

    [SerializeField] private float cancelRadius = 50f; // 중앙 취소 반경
    [SerializeField] private float confirmCooldown = 0.3f;
    private float lastConfirmTime = -999f;


    public bool isRadialMenuActive = false; // 현재 라디얼 메뉴 활성화 여부
    private int current_segment_Index = -1;          // 현재 하이라이트된 세그먼트 인덱스

    public int save_segment_index;

    public QuickTextBar[] quickTextBars;
    public QuickTextBar[] quickEmotionBars;

    private int nextBarIndex = 0;
    private bool isQuickItemDelay = false;
    private bool isQuickEmotionDelay = false;

    public void ShowQuickText(string msg, string itemName = "아이템", int txtId = 0, float charPerSec = 10f)
    {
        if (isQuickItemDelay)
        {
            Debug.Log("[ShowQuickText] 사용 제한 중입니다.");
            return;
        }
        // 사용 가능한 바 찾기
        for (int i = 0; i < quickTextBars.Length; i++)
        {
            if (!quickTextBars[i].IsBusy)
            {
                quickTextBars[i].txtId = txtId;
                quickTextBars[i].charPerSec = charPerSec;
                quickTextBars[i].ShowMessage(msg, 0.2f);
                return;
            }
        }

            isQuickItemDelay = true;
            quickTextBars[0].txtId = 1;
            quickTextBars[0].charPerSec = 7; // 경고는 일부러 느리게
            quickTextBars[0].ShowErrorMessage($"<color=red><b>* 빠르게 아이템을 사용하려 했지만 실패했다.");

            // 제한 해제 타이머
            StartCoroutine(ResetQuickItemDelay());
        
    }
    private IEnumerator ResetQuickItemDelay()
    {
        yield return new WaitForSeconds(5f); // 경고 메시지 출력 동안 제한
        isQuickItemDelay = false;
    }
    
    private IEnumerator ResetQuickEmotionDelay()
    {
        yield return new WaitForSeconds(5f); // 경고 메시지 출력 동안 제한
        isQuickEmotionDelay = false;
    }

    public void ShowQuickEmotion(string msg, string emotion = "감정표현", int txtId = 0, float charPerSec = 10f)
    {
        if (isQuickEmotionDelay)
        {
            Debug.Log("[ShowQuickEmotion] 사용 제한 중입니다.");
            return;
        }
        // 사용 가능한 바 찾기
        for (int i = 0; i < quickEmotionBars.Length; i++)
        {
            if (!quickEmotionBars[i].IsBusy)
            {
                quickEmotionBars[i].txtId = txtId;
                quickEmotionBars[i].charPerSec = charPerSec;
                quickEmotionBars[i].ShowMessage(msg,0.2f);
                return;
            }
        }

        isQuickEmotionDelay = true;
        quickEmotionBars[0].txtId = 1;
        quickEmotionBars[0].charPerSec = 7;
        quickEmotionBars[0].ShowErrorMessage("<color=yellow><b>* 감정이 너무 많아… \n정신이 혼란스럽다..</b></color>", 1f);

        // 제한 해제 타이머
        StartCoroutine(ResetQuickEmotionDelay());

    }



    #region unity_code
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

    }

    private void Start()
    {
        soundManager = SoundManager.Instance;
        BattleManager = BattleManager.Instance;
        dialogueManager = DialogueManager.Instance;
        gameManager = GameManager.Instance;
        gameover_Object.SetActive(false);
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
        keyBindings[7] = KeyCode.C;
        keyBindings[8] = KeyCode.Tab;
        keyBindings[9] = KeyCode.E;

        int screenWidth = 1920;
        int screenHeight = 1080;
        float screenRatio = (float)screenWidth / screenHeight;
        UpdateResolution(screenWidth, screenHeight);

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
        ShowPanel("Game");
        OptionInput();
        UpdateUI();
        SaveOff();
        OffPlayerUI(); // 비전투 상태에서는 UI를 숨김

        hpRect = hpBar.GetComponent<RectTransform>();
        ResizeHpBar(gameManager.GetPlayerData().Maxhealth);

        for (int i = 0; i < soulSegments.Count; i++)
        {
            Debug.Log($"soulSegments[{i}] = {soulSegments[i].segmentName}");
        }

    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            OffPlayerUI();
        }
        else if (Input.GetKeyDown(KeyCode.U))
        {
            OnPlayerUI();
        }
        if (isUserInterface)
        {
            Time.timeScale = 0f;
        }
        else
        {
            gameManager.ResumeGame();
        }

        if (currentButtonIndex > currentButtons.Length)
        {
            OnButtonHover(0);
        }
        if (isWaitingForKey)
        {
            DetectKeyInput();
        }
        UpdateUI();
        OptionInput();

        HandleInput();
        UpdateSoulPosition();
        UpdateInventoryUI();

        //MouseCusor
        Vector2 mousePosition = Input.mousePosition;
        crosshairTransform.position = mousePosition;
    

        if (isSavePanel)
        {
            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                soundManager.SFXPlay("move_sound", 185);
                saveNum--;
                if (saveNum < 0)
                {
                    saveNum = save_points.Length - 1; // 배열의 마지막 인덱스로 순환
                }
            }
            else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                soundManager.SFXPlay("move_sound", 185);
                saveNum++;
                if (saveNum >= save_points.Length)
                {
                    saveNum = 0; // 배열의 처음으로 순환
                }
            }

            savePanel_soul.gameObject.transform.localPosition = save_points[saveNum].transform.localPosition;

            switch (saveNum)
            {
                case 0:
                    if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Space) && !dialogueManager.currentNPC.isTalking)
                    {
                        SaveComplete();
                    }
                    break;

                case 1:
                    if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Space))
                    {
                        soundManager.SFXPlay("select_sound", 173);
                        isSaveDelay = true;
                        SaveOff();
                        StartCoroutine(SaveDalay());
                    }
                    break;
            }
        }
        else
        {
            if (saveNum == -1)
            {
                if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Space))
                {
                    isSaveDelay = true;
                    SaveOff();
                    StartCoroutine(SaveDalay());
                }
            }
        }
        // 감정 표현 (E 키 홀드)
        if (Input.GetKeyDown(GetKeyCode(9)))
            ToggleRadialMenu(RadialMenuType.Emotion);
        if (Input.GetKeyUp(GetKeyCode(9)))
            ConfirmRadialSelectionAndClose();

        //// 아이템 (Q 키 홀드)
        //if (Input.GetKeyDown(GetKeyCode(9)))
        //    `(RadialMenuType.Item);
        //if (Input.GetKeyUp(GetKeyCode(9)))
        //    ConfirmRadialSelectionAndClose();

        if (gameManager.GetPlayerData().player.GetComponent<PlayerMovement>().isSoulActive)
        {
            // 소울 (Tab 키 홀드)
            if (Input.GetKeyDown(GetKeyCode(8)))
                ToggleRadialMenu(RadialMenuType.Soul);
            if (Input.GetKeyUp(GetKeyCode(8)))
                ConfirmRadialSelectionAndClose();
        }

        // 라디얼 메뉴가 열려 있을 때 마우스 선택 처리
        if (isRadialMenuActive)
        {
            UpdateRadialSelection();

            if (Input.GetMouseButtonUp(0) && currentRadialIndex >= 0)
            {
                OnSelectSegment(currentRadialIndex);
            }
        }

        // 2) 메뉴 활성 상태라면 마우스 위치와 클릭 여부 판단
        if (isRadialMenuActive)
        {
            UpdateRadialSelection();

            // 마우스 왼쪽 버튼 떼는 순간 해당 감정표현/아이템 선택 확정
            if (Input.GetMouseButtonUp(0) && currentRadialIndex >= 0)
            {
                OnSelectSegment(currentRadialIndex);
            }
        }
    }

    #endregion

    #region savePanel
    public void SaveOpen()
    {
        saveNum = 0;
        isSavePanel = true;
        isSaveDelay = false;
        savePanel.SetActive(true);
        savePanel_soul.SetActive(true);

        savePanel_texts[1].color = new Color(255, 255, 255);
        savePanel_texts[2].color = new Color(255, 255, 255);
        savePanel_texts[3].color = new Color(255, 255, 255);
        savePanel_texts[4].color = new Color(255, 255, 255);

        savePanel_texts[1].text = gameManager.GetPlayerData().player_Name;
        savePanel_texts[3].text = gameManager.GetElapsedTimeInMinutes();
        savePanel_texts[0].gameObject.SetActive(false);
        savePanel_texts[5].gameObject.SetActive(true);
        savePanel_texts[6].gameObject.SetActive(true);
    }

    public void SaveOff()
    {
        saveNum = 0;
        isSavePanel = false;
        savePanel.SetActive(false);
    }

    public void SaveComplete()
    {
        savePanel_soul.SetActive(false);
        saveNum = -1; //야메
        isSavePanel = false;
        isSaveDelay = true;

        TextYellow();
        SoundManager.Instance.SFXPlay("save_sound", 171);

        savePanel_texts[5].gameObject.SetActive(false);
        savePanel_texts[6].gameObject.SetActive(false);
        savePanel_texts[0].gameObject.SetActive(true);
        gameManager.Save();
        gameManager.SaveGameTime();
    }
    public void TextYellow()
    {
        savePanel_texts[1].color = new Color(255, 255, 0);
        savePanel_texts[2].color = new Color(255, 255, 0);
        savePanel_texts[3].color = new Color(255, 255, 0);
        savePanel_texts[4].color = new Color(255, 255, 0);

        savePanel_texts[3].text = gameManager.GetElapsedTimeInMinutes().ToString();
        savePanel_texts[4].text = gameManager.GetMapName();
    }
    #endregion

    #region textBar
    public void SetTextBar()
    {
        // 플레이어 위치 가져오기
        Vector3 playerPosition = gameManager.GetPlayerData().position;
        Vector3 none = inventroy_simple_panel.gameObject.transform.localPosition;

        // 음 추가로 
        inventroy_simple_texts[0].text = gameManager.GetPlayerData().player_Name;
        inventroy_simple_texts[1].text = "HP " + gameManager.GetPlayerData().health + "/" + gameManager.GetPlayerData().Maxhealth;
        inventroy_simple_texts[2].text = "G  " + gameManager.GetPlayerData().GOLD;
        inventroy_simple_texts[3].text = "LV " + gameManager.GetPlayerData().LEVEL;

        // 카메라 위치 가져오기
        Vector3 cameraPosition = Camera.main.transform.position;
        if (playerPosition.y > cameraPosition.y)
        {
            // 플레이어가 카메라보다 높이 있는 경우
            textbar.transform.position = textbarPos[1].transform.position;
            inventroy_simple_panel.gameObject.transform.localPosition = new Vector3(none.x, 320);

        }
        else
        {
            // 플레이어가 카메라보다 낮게 있는 경우
            textbar.transform.position = textbarPos[0].transform.position;
            inventroy_simple_panel.gameObject.transform.localPosition = new Vector3(none.x, -220);
        }
        inventroy_soul.gameObject.SetActive(true);
    }
    public void TextBarOpen()
    {
        SetTextBar();
        textbar.SetActive(true);
    }
    public void CloseTextbar()
    {
        textbar.SetActive(false);
    }
    #endregion

    #region ToggleRadialMenu
    /// <summary>
    /// 라디얼 메뉴 열기/닫기
    /// </summary>
    public void ToggleRadialMenu(RadialMenuType type)
    {
        if (isRadialMenuActive && currentRadialMenu == type)
        {
            CloseAllRadialMenus();
            return;
        }

        CloseAllRadialMenus();
        currentRadialMenu = type;
        // 1. 가장 가까운 적의 감정 리스트 확보
        var closestEnemy = BattleManager.Instance.GetClosestEnemy();
        var emotions = (closestEnemy != null) ? closestEnemy.GetReactableEmotions() : new List<string>();

        // 2. EmotionInfo로 변환 (아이콘 포함)
        var emotionInfoList = new List<EmotionInfo>();
        foreach (var e in emotions)
        {
            emotionInfoList.Add(new EmotionInfo
            {
                name = e,
                icon = GameManager.Instance.GetEmotionSprite(e)
            });
        }
        // 3. 상황별 감정 세팅
        GameManager.Instance.SetCurrentSituationEmotions(emotionInfoList);

        UpdateSegmentValues(type); // **여기 추가**

        GameObject targetPanel = type switch
        {
            RadialMenuType.Emotion => emotionRadialPanel,
            RadialMenuType.Item => itemRadialPanel,
            RadialMenuType.Soul => soulRadialPanel,
            _ => null
        };

        if (targetPanel != null)
        {
            targetPanel.SetActive(true);
            StartCoroutine(ScaleIn(targetPanel.transform)); 
            // **이모션 메뉴 열 때 한 번만 호출**
            if (type == RadialMenuType.Emotion)
                BattleManager.Instance.HighlightClosestEnemyIndicator();

        }

        isRadialMenuActive = true;
    }
   

    private IEnumerator ScaleIn(Transform panelTransform)
    {
        panelTransform.localScale = Vector3.zero;
        float duration = 0.15f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            panelTransform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        panelTransform.localScale = Vector3.one;
    }

    private List<RadialSegment> GetSegmentList(RadialMenuType type)
    {
        return type switch
        {
            RadialMenuType.Item => itemSegments,
            RadialMenuType.Soul => soulSegments,
            RadialMenuType.Emotion => emotionSegments,
            _ => new List<RadialSegment>()
        };
    }

   
    private void UpdateSegmentValues(RadialMenuType type)
    {
        var playerMovement = GameManager.Instance.GetPlayerData().player.GetComponent<PlayerMovement>();
        switch (type)
        {
            case RadialMenuType.Item:
                var inventory = GameManager.Instance.GetPlayerData().inventory;
                for (int i = 0; i < itemSegments.Count; i++)
                {
                    string label = i < inventory.Count ? inventory[i].itemName : "";
                    BindSegment(itemSegments[i], label);
                }
                break;

            case RadialMenuType.Soul:
                playerMovement = gameManager.GetPlayerData().player.GetComponent<PlayerMovement>();
                var playerWeapons = playerMovement.playerWeapons;
                for (int i = 0; i < soulSegments.Count; i++)
                {
                    string label = i < playerWeapons.Count ? playerWeapons[i].WeaponName.ToString() : "";
                    BindSegment(soulSegments[i], label);
                }
                break;

            case RadialMenuType.Emotion:
                var emotionDataList = GameManager.Instance.GetCurrentSituationEmotions();

                for (int i = 0; i < emotionSegments.Count; i++)
                {
                    var segment = emotionSegments[i];

                    if (i < emotionDataList.Count)
                    {
                        // 활성 슬롯: 이름·아이콘 세팅, 버튼 활성화
                        var info = emotionDataList[i];
                        emotionSegments[i].segmentName = info.name; 
                        BindSegment(segment, info.name, info.icon);
                        SetSegmentInteractable(segment, true);
                    }
                    else
                    {
                        // 비활성 슬롯
                        emotionSegments[i].segmentName = ""; 
                        BindSegment(segment, "", null);
                        SetSegmentInteractable(segment, false);
                    }
                }
                break;
        }
    }
    private void BindSegment(RadialSegment segment, string label, Sprite icon)
    {
        // Text 세팅
        var textComp = segment.segmentTransform.GetComponentInChildren<TextMeshProUGUI>();
        if (textComp != null)
            textComp.text = label;

        // Icon 세팅
        var iconImage = segment.segmentTransform.Find("IconImage")?.GetComponent<Image>();
        if (iconImage != null)
        {
            if (!string.IsNullOrEmpty(label) && icon != null)
            {
                iconImage.sprite = icon;
                iconImage.gameObject.SetActive(true);
            }
            else
            {
                iconImage.gameObject.SetActive(false);
            }
        }
    }
    private void SetSegmentInteractable(RadialSegment segment, bool isInteractable)
    {
        // 버튼 컴포넌트 활성/비활성
        var btn = segment.segmentTransform.GetComponent<Button>();
        if (btn != null)
            btn.interactable = isInteractable;

    }
    private void BindSegment(RadialSegment segment, string label)
    {
        // 텍스트 설정
        TextMeshProUGUI textComponent = segment.segmentTransform.GetComponentInChildren<TextMeshProUGUI>();
        if (textComponent != null)
        {
            textComponent.text = label;
        }

        // 이미지 설정
        Image[] images = segment.segmentTransform.GetComponentsInChildren<Image>(true);
        foreach (var img in images)
        {
            if (img.gameObject.name == "IconImage") // 자식 아이콘만 제어
            {
                img.gameObject.SetActive(!string.IsNullOrEmpty(label));
            }
        }
    }

    private void CloseAllRadialMenus()
    {
        emotionRadialPanel.SetActive(false);
        itemRadialPanel.SetActive(false);
        soulRadialPanel.SetActive(false);

        // **메뉴 닫힐 때 전체 끄기**
        BattleManager.Instance.ClearAllEnemyIndicators();

        isRadialMenuActive = false;
        currentRadialMenu = RadialMenuType.None;
    }
    public void PlayEmotionAnimation(string emotionName)
    {
        GameObject player = gameManager.GetPlayerData().player;
        // 감정 표현 후 메시지 표시
        switch (emotionName)
        {
            case "분노":
            case "Anger":
                ShowQuickEmotion("* 당신은 분노에 가득 찼다.", emotionName);
                break;
            case "자비":
            case "Mercy":
                ShowQuickEmotion("* 당신은 자비로운 마음을 전했다.", emotionName);
                break;
            case "긍정":
            case "Affirm":
                ShowQuickEmotion("* 당신은 긍정적으로 대답했다.", emotionName);
                break;
            case "부정":
            case "Deny":
                ShowQuickEmotion("* 당신은 부정적으로 대답했다.", emotionName);
                break;
            case "사랑":
            case "Love":
                ShowQuickEmotion("* 당신은 따뜻한 마음을 보였다.", emotionName);
                break;
            case "반감":
            case "Disgust":
                ShowQuickEmotion("* 당신은 강한 반감을 드러냈다.", emotionName);
                break;
            case "유혹":
            case "Flirt":
                ShowQuickEmotion("* 당신은 살짝 유혹적인 모습을 보였다.", emotionName);
                break;
            case "무시":
            case "Ignore":
                ShowQuickEmotion("* 당신은 무관심한 태도를 취했다.", emotionName);
                break;
            case "존중":
            case "Respect":
                ShowQuickEmotion("* 당신은 상대를 존중했다.", emotionName);
                break;
            case "냉소":
            case "Sarcasm":
                ShowQuickEmotion("* 당신은 비꼬는 말을 했다.", emotionName);
                break;
            case "행복":
            case "Joy":
                ShowQuickEmotion("* 당신은 기쁨을 표현했다.", emotionName);
                break;
            case "슬픔":
            case "Sorrow":
                ShowQuickEmotion("* 당신은 슬픈 표정을 지었다.", emotionName);
                break;
            case "기도":
            case "Pray":
                ShowQuickEmotion("* 당신은 진심으로 기도했다.", emotionName);
                break;
            case "당황":
            case "Flustered":
                ShowQuickEmotion("* 당신은 당황했다.", emotionName);
                break;
            case "혼란":
            case "Confused":
                ShowQuickEmotion("* 당신은 혼란스러워했다.", emotionName);
                break;
            case "공포":
            case "Fear":
                ShowQuickEmotion("* 당신은 공포에 떨었다.", emotionName);
                break;
            case "진실":
            case "Truth":
                ShowQuickEmotion("* 당신은 진실을 말했다.", emotionName);
                break;
            case "용기":
            case "Bravery":
                ShowQuickEmotion("* 당신은 용기를 냈다.", emotionName);
                break;
            case "친절":
            case "Kindness":
                ShowQuickEmotion("* 당신은 친절을 베풀었다.", emotionName);
                break;
            case "고결":
            case "Integrity":
                ShowQuickEmotion("* 당신은 고결한 행동을 보였다.", emotionName);
                break;
            case "인내":
            case "Patience":
                ShowQuickEmotion("* 당신은 인내심을 발휘했다.", emotionName);
                break;
            case "끈기":
            case "Perseverance":
                ShowQuickEmotion("* 당신은 끝까지 끈기 있게 버텼다.", emotionName);
                break;
            case "의지":
            case "Determination":
                ShowQuickEmotion("* 당신의 눈에 의지가 빛났다.", emotionName);
                break;
            case "정의":
            case "Justice":
                ShowQuickEmotion("* 당신은 정의를 외쳤다.", emotionName);
                break;
            default:
                ShowQuickEmotion("* 당신의 감정이 명확하지 않다.", emotionName);
                break;
        }
    }


    public void UseQuickItem(int id)
    {
        if (isQuickItemDelay)
        {
            Debug.Log("[퀵아이템] 사용 지연 중입니다.");
            return;
        }

        gameManager.QuickUseItem(id);
    }


    public void ChangeSoulType(string soulName)
    {
        if (Enum.TryParse<WeaponType>(soulName, out var newWeaponType))
        {
            var playerMovement = GameManager.Instance.GetPlayerData().player.GetComponent<PlayerMovement>();

            playerMovement.SelectWeapon(save_segment_index);

            Debug.Log($"[소울] {newWeaponType} 으로 변경됨");

            UpdateSoulUIColor(); // 색상 동기화
        }
        else
        {
            Debug.LogWarning($"[소울] {soulName} 을 WeaponType 으로 변환 실패");
        }
    }

    /// <summary>
    /// 마우스 각도에 따라 하이라이트할 세그먼트 계산
    /// </summary>
    // 감정별 중심 각도 (위쪽부터 시계 방향, 자비가 0도 기준)
    private readonly float[] customAngles = { 0f, 60f, 120f, 180f, 240f, 300f };

    // 감정 순서: 자비, 긍정, 무시, 유혹, 분노, 부정

    private void UpdateRadialSelection()
    {
        Vector2 dir = Input.mousePosition - centerPoint.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        angle = (360f - angle + 90f) % 360f;

        List<RadialSegment> activeSegments = null;

        switch (currentRadialMenu)
        {
            case RadialMenuType.Emotion:
                activeSegments = emotionSegments;
                break;
            case RadialMenuType.Item:
                activeSegments = itemSegments;
                break;
            case RadialMenuType.Soul:
                activeSegments = soulSegments;
                break;
        }

        if (activeSegments == null || activeSegments.Count == 0) return;

        int closestIndex = GetClosestIndex(angle, activeSegments.Count);

        for (int i = 0; i < activeSegments.Count; i++)
        {
            activeSegments[i].SetHighlight(i == closestIndex);
        }

        currentRadialIndex = closestIndex;
    }

    private int GetClosestIndex(float angle, int count)
    {
        float segmentSize = 360f / count;
        float minDiff = 999f;
        int closestIndex = 0;

        for (int i = 0; i < count; i++)
        {
            float centerAngle = i * segmentSize;
            float diff = Mathf.Abs(Mathf.DeltaAngle(angle, centerAngle));
            if (diff < minDiff)
            {
                minDiff = diff;
                closestIndex = i;
            }
        }
        return closestIndex;
    }



    /// <summary>
    /// 세그먼트 선택이 확정되었을 때 실행
    /// </summary>
    private void OnSelectSegment(int index)
    {
        RadialSegment seg = null;
        switch (currentRadialMenu)
        {
            case RadialMenuType.Emotion:
                seg = emotionSegments[index];
                break;
            case RadialMenuType.Item:
                seg = itemSegments[index];
                break;
            case RadialMenuType.Soul:
                seg = soulSegments[index];
                save_segment_index = index;

                break;
        }

        if (seg != null)
        {
            Debug.Log($"[{currentRadialMenu}] Segment {index} 선택됨 - {seg.segmentName}");
            if(seg.segmentName != "")
                seg.ExecuteAction(index);
        }

        ToggleRadialMenu(currentRadialMenu); // 닫기
    }
    public void ConfirmRadialSelectionAndClose()
    {
        // 쿨타임 검사
        if (Time.unscaledTime - lastConfirmTime < confirmCooldown)
            return;

        lastConfirmTime = Time.unscaledTime;

        // 중앙에서 놓으면 취소
        float dist = Vector2.Distance(Input.mousePosition, centerPoint.position);
        if (dist < cancelRadius)
        {
            Debug.Log("중앙에서 놓았으므로 취소됨");
            CloseAllRadialMenus();
            return;
        }

        // 세그먼트 확정
        if (isRadialMenuActive && currentRadialIndex >= 0)
        {
            OnSelectSegment(currentRadialIndex);
        }

        CloseAllRadialMenus();
    }
    #endregion

    #region InventroyUi
    void UpdateInventoryUI()
    {
        // 인벤토리의 첫 번째 텍스트 비활성화 및 색상 변경 로직
        if (gameManager.GetPlayerData().inventory.Count == 0)
        {
            inventroy_texts[0].color = Color.gray;
        }
        else
        {
            inventroy_texts[0].color = Color.white;
        }
        inventroy_texts[2].gameObject.SetActive(gameManager.GetPlayerData().isPhone);
        // 아이템 없을때 돌아가는 코드
        if ((inventroy_panelNum == 4 || inventroy_panelNum == 1) && gameManager.GetPlayerData().inventory.Count == 0)
        {
            OnPanel(-1);
        }
    }

    public void ChangeInventroy()
    {
        if (!isInventroy)
        {
            inventroy_panelNum = 0;
            isInventroy = true;
            inventroy_panel.SetActive(true);
            soundManager.SFXPlay("move_sound", 185);
        }
        else
        {
            item_panel.SetActive(false);
            stat_panel.SetActive(false);
            call_panel.SetActive(false);
            inventroy_panel.SetActive(false);
            isInventroy = false;
        }

    }
    void OnPanel(int i)
    {
        item_panel.SetActive(false);
        stat_panel.SetActive(false);
        call_panel.SetActive(false);
        inventroy_soul.gameObject.SetActive(true);
        inventroy_curNum = 0;
        switch (i)
        {
            case 0:
                if (gameManager.GetPlayerData().inventory.Count > 0)
                {
                    soundManager.SFXPlay("select_sound", 173);
                    item_panel.SetActive(true);
                    inventroy_panelNum = 1;

                    foreach (var r in item_texts)
                    {
                        r.gameObject.SetActive(false); // 모든 item_texts 비활성화
                    }

                    int inventoryCount = gameManager.GetPlayerData().inventory.Count;
                    int maxDisplayCount = Mathf.Min(inventoryCount, item_texts.Length); // 최대 item_texts 배열 크기만큼 표시

                    for (int c = 0; c < maxDisplayCount; c++)
                    {
                        item_texts[c].gameObject.SetActive(true); // 플레이어 인벤토리 크기만큼 item_texts 활성화
                        item_texts[c].text = gameManager.GetPlayerData().inventory[c].itemName; // 아이템 이름 표시
                    }
                }
                break;

            case 1:
                soundManager.SFXPlay("select_sound", 173);
                stat_panel.SetActive(true);
                inventroy_panelNum = 2;
                inventroy_soul.gameObject.SetActive(false);
                StatUpdate();
                break;

            case 2:
                soundManager.SFXPlay("select_sound", 173);
                call_panel.SetActive(true);
                inventroy_panelNum = 3;
                break;

            case 3: // 선택창
                soundManager.SFXPlay("select_sound", 173);
                item_panel.SetActive(true);
                inventroy_panelNum = 4;
                break;
            default:
                if (inventroy_panelNum == 0)
                    ChangeInventroy();
                else
                    soundManager.SFXPlay("move_sound", 185);


                inventroy_panelNum = 0;
                break;
        }
    }
    void StatUpdate()
    {
        stat_texts[0].text = gameManager.GetPlayerData().player_Name;
        stat_texts[1].text = "LV " + gameManager.GetPlayerData().LEVEL;
        stat_texts[2].text = "HP " + gameManager.GetPlayerData().health + "/" + gameManager.GetPlayerData().Maxhealth;
        stat_texts[3].text = "AT " + gameManager.GetPlayerData().AT_level + " (" + gameManager.GetPlayerData().AT + ")";
        stat_texts[4].text = "DF " + gameManager.GetPlayerData().DF_level + " (" + gameManager.GetPlayerData().DF + ")";
        stat_texts[5].text = "EXP: " + gameManager.GetPlayerData().EXP;
        stat_texts[6].text = "NEXT: " + gameManager.GetPlayerData().NextEXP;
        stat_texts[7].text = "무기: " + gameManager.GetPlayerData().curWeapon.itemName;
        stat_texts[8].text = "갑옷: " + gameManager.GetPlayerData().curArmor.itemName;
        stat_texts[9].text = "GOLD: " + gameManager.GetPlayerData().GOLD;

    }
    void HandleInput()
    {
        if (!isInventroy || isUserInterface)
            return;
        // W 입력 시 inventroy_curNum 감소
        if (inventroy_panelNum != 4)
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
                inventroy_curNum--;
                soundManager.SFXPlay("move_sound", 185);
                if (inventroy_curNum < 0)
                {
                    inventroy_curNum = GetCurrentPanelTextLength() - 1; // 현재 패널의 끝으로 이동
                }
            }

            // S 입력 시 inventroy_curNum 증가
            if (Input.GetKeyDown(KeyCode.S))
            {
                soundManager.SFXPlay("move_sound", 185);
                inventroy_curNum++;
                if (inventroy_curNum >= GetCurrentPanelTextLength())
                {
                    inventroy_curNum = 0; // 현재 패널의 처음으로 이동
                }
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                inventroy_curNum--;
                soundManager.SFXPlay("move_sound", 185);
                if (inventroy_curNum < 0)
                {
                    inventroy_curNum = GetCurrentPanelTextLength() - 1; // 현재 패널의 끝으로 이동
                }
            }

            // S 입력 시 inventroy_curNum 증가
            if (Input.GetKeyDown(KeyCode.D))
            {
                soundManager.SFXPlay("move_sound", 185);
                inventroy_curNum++;
                if (inventroy_curNum >= GetCurrentPanelTextLength())
                {
                    inventroy_curNum = 0; // 현재 패널의 처음으로 이동
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.Z) && inventroy_panelNum != 2)
        {
            if (!item_panel.activeSelf)
                OnPanel(inventroy_curNum);

            // 인벤토리/아이템
            else if (inventroy_panelNum == 1)
            {
                item_prevNum = inventroy_curNum;
                OnPanel(3);
            }
            // 선택
            else if (inventroy_panelNum == 4)
            {
                // 아이템 사용
                if (inventroy_curNum >= 0 && inventroy_curNum < 3) // 또는 interaction_texts.Length
                {
                    switch (inventroy_curNum)
                    {
                        case 0:
                            gameManager.UseItem(item_prevNum);
                            break;
                        case 1:
                            gameManager.InfoItem(item_prevNum);
                            break;
                        case 2:
                            gameManager.DropItem(item_prevNum);
                            break;
                    }
                    OnPanel(-1);
                }
                else
                {
                    Debug.LogWarning($"잘못된 inventroy_curNum 값: {inventroy_curNum}");
                }

            }


        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            if (inventroy_panelNum != 4)
                OnPanel(-1);
            else
            {
                OnPanel(0);
                inventroy_curNum = 0;
            }
        }
    }
    int GetCurrentPanelTextLength()
    {
        int phoneValue = gameManager.GetPlayerData().isPhone ? 0 : 1;
        int result = gameManager.GetPlayerData().inventory.Count;


        switch (inventroy_panelNum)
        {
            case 0: // 인벤토리 패널
                return inventroy_texts.Length - phoneValue;
            case 1: // 아이템 패널
                return Mathf.Min(result, item_texts.Length);
            case 3: // 전화 패널
                return call_texts.Length;

            case 4: // 선택 패널
                return interaction_texts.Length;
            default:
                return 0;
        }
    }

    void UpdateSoulPosition()
    {
        GameObject[] currentPoints = null;

        // 패널에 맞는 포인트 배열을 선택
        switch (inventroy_panelNum)
        {
            case 0:
                currentPoints = inventroy_points;
                break;
            case 1:
                currentPoints = item_points;
                break;
            case 3: // 스탯 제외
                currentPoints = call_points;
                break;
            case 4: // 선텍
                currentPoints = interaction_points;
                break;
        }

        // 현재 선택된 포인트 위치로 inventroy_soul 이동
        if (currentPoints != null && currentPoints.Length > 0 && inventroy_curNum < currentPoints.Length)
        {
            inventroy_soul.transform.position = currentPoints[inventroy_curNum].transform.position;
        }
    }
    #endregion
    IEnumerator SaveDalay()
    {
        yield return new WaitForSeconds(0.5f);
        isSaveDelay = false;

    }
    public void LoadIntro()
    {
        SceneManager.LoadScene("IntroScene");
    }
    public void UpdateSoulUIColor()
    {
        var weapon = GameManager.Instance.GetWeaponData();
        Color soulColor = weapon.GetColor(weapon.weaponType);

        // gameover_soul 색상 설정
        if (gameover_soul != null)
            gameover_soul.GetComponent<Image>().color = soulColor;

        // 인벤토리 소울 색상 설정
        if (inventroy_soul != null)
            inventroy_soul.GetComponent<Image>().color = soulColor;

        // 세이브 패널 소울 색상 설정
        if (savePanel_soul != null)
            savePanel_soul.GetComponent<Image>().color = soulColor;

        // soulObject가 존재하면 색상 적용
        var player = GameManager.Instance.GetPlayerData().player;
        if (player != null)
        {
            var pm = player.GetComponent<PlayerMovement>();
            if (pm != null && pm.soulObject != null)
            {
                var soulRenderer = pm.soulObject.GetComponent<SpriteRenderer>();
                if (soulRenderer != null)
                {
                    soulRenderer.color = soulColor;
                }
            }
        }
    }


    public void UpdateLaserSlider(float current, float max)
    {
        if (laserAmmoSlider != null)
        {
            laserAmmoSlider.maxValue = max;
            laserAmmoSlider.value = current;
        }
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
        soundManager.SFXPlay("select_sound", 173);
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

    public void ResizeHpBar(float maxHealth)
    {
        // 안전장치 (Null 예외 방지)
        if (hpBar == null)
        {
            Debug.LogWarning("UIManager : hpBar reference missing!");
            return;
        }
        if (hpRect == null) hpRect = hpBar.GetComponent<RectTransform>();
        if (hpRect == null) return;

        // ── 가로 길이 계산 ─────────────────────────────
        float newWidth = baseBarWidth +
                         Mathf.Max(0, maxHealth - 20) * extraWidthPerHp;

        Vector2 size = hpRect.sizeDelta;       // 기존 높이 그대로 두고
        hpRect.sizeDelta = new Vector2(newWidth, size.y);   // 가로(x)만 조정
    }
    void InitHeart() // 체력 새팅
    {
        // 체력이 많아질수록 점점더 길어지도록
        PlayerData player = gameManager.GetPlayerData();
        float currentHealth = player.health;
        float maxHealth = player.Maxhealth;

        // 체력 이미지 업데이트
        hpBar.value = Mathf.Clamp((float)currentHealth / (float)maxHealth, 0, 1);
        hpBar_text.text = ((int)currentHealth).ToString() + " / " + ((int)maxHealth).ToString();
    }




    public void ShowDamageText(Vector3 worldPosition, float damageAmount)
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
        PlayerData player = GameManager.Instance.GetPlayerData();
        float currentHealth = player.health;
        float maxHealth = player.Maxhealth;

        ResizeHpBar(maxHealth);

        // 체력 이미지 업데이트
        hpBar.value = Mathf.Clamp((float)currentHealth / (float)maxHealth, 0, 1);

        hpBar_text.text = currentHealth.ToString() + " / " + maxHealth.ToString();

        // 무기 데이터 가져오기 및 총알 이미지 업데이트
        Weapon weapon = GameManager.Instance.GetWeaponData();
        float current_magazine = weapon.current_magazine;

        // 총알 이미지 업데이트f
        for (int i = 0; i < ui_ammo.Length; i++)
        {
            int spriteIndex = (i < current_magazine) ? 0 : 1; // 총알 개수에 따른 스프라이트 인덱스 계산
            ui_ammo[i].GetComponent<ImageScript>().SetImage(spriteIndex);
        }

        // 무기 탄약 텍스트 업데이트
        if (weapon.current_Ammo == -1)
        {
            ui_ammoText.text = "∞";
            ui_ammoText.fontSize = 80;
        }
        else
        {
            ui_ammoText.text = weapon.current_Ammo + "/" + weapon.maxAmmo;
            ui_ammoText.fontSize = 36;

        }
    }
    // 1) 무기 교체 시 호출할 메서드
    public void OnWeaponChanged(Weapon newWeapon)
    {
        // (1) 기존 UI 아이콘 정리
        if (ui_ammo != null)
        {
            for (int i = 0; i < ui_ammo.Length; i++)
            {
                if (ui_ammo[i] != null)
                    Destroy(ui_ammo[i]);
            }
        }
        ui_ammo = null;  // 배열 크기를 0 또는 null로 초기화

        // (2) 새 무기의 current_magazine 개수에 맞춰 다시 생성
        CreateAmmoUI(newWeapon);
    }

    // 2) 실제 아이콘 생성 로직을 분리해 둔다
    private void CreateAmmoUI(Weapon weapon)
    {
        // 현재 탄알 수량(남은 탄알)을 기준으로 아이콘을 만들려면 current_magazine 사용
        int ammoCount = (int)weapon.magazine;
        // 만약 “최대 용량”(Capacity)을 보여주려면 weapon.magazine으로 바꿔도 괜찮다
        Vector3 poss = ui_positions[0].transform.position;
        Vector3 posss = ui_positions[0].transform.position;

        ui_ammo = new GameObject[ammoCount];
        for (int i = 0; i < ammoCount; i++)
        {
            // Resources/Prefabs/Ammo.prefab 경로에 프리팹이 있어야 한다
            GameObject prefab = Resources.Load<GameObject>("Prefabs/Ammo");
            GameObject instance = Instantiate(prefab, ui_positions[0].transform);

            // 위치 세팅 (예시)
            float sizeY = instance.GetComponent<RectTransform>().sizeDelta.y;
            Vector3 pos = ui_positions[0].transform.position;
            pos.x += i * sizeY * 1.25f;
            instance.transform.position = pos;
            poss.x = pos.x + 200;
            posss.x = pos.x + 25;
            ui_ammo[i] = instance;
        }
        ui_ammoText.gameObject.transform.position = poss;
        top.gameObject.transform.position = posss;

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
        ui_ammoText.gameObject.SetActive(false);
        hpBar.gameObject.SetActive(false);
        ammos_td.gameObject.SetActive(false);
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
        ui_ammoText.gameObject.SetActive(true);
        hpBar.gameObject.SetActive(true);
        ammos_td.gameObject.SetActive(true);
    }
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
    public void SetAmmoUIVisible(bool isVisible)
    {
        foreach (var ammo in ui_ammo)
        {
            ammo?.SetActive(isVisible);
        }
        ui_ammoText?.gameObject.SetActive(isVisible);
        ammos_td?.SetActive(isVisible);
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

        // 마우스 조준 사용
        PlayerPrefs.SetInt("IsMouseShot", isMouseShot ? 1 : 0);

        // 마우스로 돌진
        PlayerPrefs.SetInt("isMouseRoll", isMouseRoll ? 1 : 0);


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
        // 마우스로 쏘기 불러오기
        if (PlayerPrefs.HasKey("isMouseShot"))
        {
            isMouseShot = PlayerPrefs.GetInt("isMouseShot") == 1;
            mouseShotToggle.image.sprite = isMouseShot ? onSprite : offSprite;
        }

        // 마우스로 구르기
        if (PlayerPrefs.HasKey("isMouseRoll"))
        {
            isMouseRoll = PlayerPrefs.GetInt("isMouseRoll") == 1;
            mouseRollToggle.image.sprite = isMouseRoll ? onSprite : offSprite;
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
        isMouseRoll = true;
        isMouseShot = true;
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
        keyBindings[7] = KeyCode.C;
        keyBindings[8] = KeyCode.Tab;
        keyBindings[9] = KeyCode.E;
        // UI 업데이트
        Screen.fullScreen = isFullScreen;
        fullScreenToggle.image.sprite = isFullScreen ? onSprite : offSprite;
        QualitySettings.vSyncCount = isVSyncOn ? 1 : 0;
        vSyncToggle.image.sprite = isVSyncOn ? onSprite : offSprite;
        Cursor.visible = isCursorVisible;
        cusorToggle.image.sprite = isCursorVisible ? onSprite : offSprite;

        mouseRollToggle.image.sprite = isMouseRoll ? onSprite : offSprite;
        mouseShotToggle.image.sprite = isMouseShot ? onSprite : offSprite;
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
        currentButtonIndex = 0; // 패널 변경 시 인덱스 초기화
        UpdateSelection();
    }
    void Navigate(int direction)
    {
        currentButtonIndex = (currentButtonIndex + direction + currentButtons.Length) % currentButtons.Length;
        UpdateSelection();
    }

    void OptionInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !GameManager.Instance.GetPlayerData().isDie)
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
                SaveSettings();
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
            soundManager.SFXPlay("move_sound", 185);
        }

        if (isUserInterface)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                Navigate(-1);
                soundManager.SFXPlay("move_sound", 185);
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            {
                Navigate(1);
                soundManager.SFXPlay("move_sound", 185);
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            {
                soundManager.SFXPlay("move_sound", 185);
                AdjustValue(-1);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            {
                soundManager.SFXPlay("move_sound", 185);
                AdjustValue(1);
            }
            else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Z))
            {
                if (currentButtons != null && currentButtons.Length > 0)
                {
                    if (currentPanel == "Option")
                    {
                        if (currentButtonIndex == 1 || currentButtonIndex == 3 || currentButtonIndex == 4)
                            ToggleValue();  // ToggleValue 호출
                        else
                        {
                            currentButtons[currentButtonIndex].onClick.Invoke();

                            if (currentButtonIndex != 2 && currentButtonIndex != 5 && currentButtonIndex != 6 && currentButtonIndex != 7 && currentButtonIndex != 8 && currentButtonIndex != 0)
                                soundManager.SFXPlay("select_sound", 173);
                        }
                    }
                    else
                    {
                        currentButtons[currentButtonIndex].onClick.Invoke();
                        soundManager.SFXPlay("select_sound", 173);
                    }
                }
            }
        }
    }

    public void soundSfx()
    {
        soundManager.SFXPlay("select_sound", 173);
    }
    public void AdjustValue(int direction)
    {
        switch (currentButtonIndex)
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
    void UpdateResolution(int width, int height)
    {

        CanvasScaler scaler = uicanvas.GetComponent<CanvasScaler>();
        if (scaler != null && scaler.uiScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize)
        {
            scaler.referenceResolution = new Vector2(width, height);
        }
        else
        {
            Debug.LogWarning("CanvasScaler가 없거나 올바른 모드가 아닙니다.");
        }

        Screen.SetResolution(width, height, Screen.fullScreen); // 옵션
    }

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
    public void ToggleValue()
    {
        switch (currentButtonIndex)
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
        soundManager.SFXPlay("select_sound", 173);
        StartCoroutine(ForceToggleUpdate());
    }
    public void ToggleKeyValue()
    {

        switch (currentButtonIndex)
        {
            case 11: // 활성화 : 마우스 조준, 비활성화 : 가장 가까운적 조준
                ToggleMouseShot();
                Debug.Log("마우스 조준 만들어");
                break;
            case 12: // 활성화 : 마우스로 돌진, 비활성화 : 방향키로 이동
                ToggleMouseRoll();
                Debug.Log("마우스로 돌진 만들어");
                break;
        }
        soundManager.SFXPlay("select_sound", 173);
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
        crosshairTransform.gameObject.SetActive(!isCursorVisible);
        Cursor.visible = isCursorVisible;
        UpdateButtonState(cusorToggle, isCursorVisible);
    }
    void ToggleMouseShot()
    {
        isMouseShot = !isMouseShot;
        UpdateButtonState(mouseShotToggle, isMouseShot);
    }

    void ToggleMouseRoll()
    {
        isMouseRoll = !isMouseRoll;
        UpdateButtonState(mouseRollToggle, isMouseRoll);
    }

    IEnumerator ForceToggleUpdate()
    {
        yield return null; // Wait for one frame
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(currentButtons[currentButtonIndex].gameObject);
    }
    void UpdateSelection()
    {
        for (int i = 0; i < currentButtons.Length; i++)
        {
            ColorBlock colors = currentButtons[i].colors;
            colors.normalColor = Color.white; // 기본 색상
            colors.highlightedColor = Color.white; // 강조 색상
            colors.pressedColor = Color.gray; // 클릭 시 색상
            colors.selectedColor = (i == currentButtonIndex) ? Color.white : Color.white; // 선택된 색상
            currentButtons[i].colors = colors;

            // 텍스트 색상 변경
            TextMeshProUGUI buttonText = currentButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.color = (i == currentButtonIndex) ? Color.white : Color.gray; // 강조 색상
            }
        }
    }

    public void OnButtonClick(int index)
    {
        currentButtonIndex = index;
        UpdateSelection();
    }
    public void ChangeResolution(int direction)
    {
        curRIndex = (curRIndex + direction + predefinedResolutions.Count) % predefinedResolutions.Count;
        Resolution selectedResolution = predefinedResolutions[curRIndex];
        UpdateResolution(selectedResolution.width, selectedResolution.height);
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
        currentButtonIndex = index;
        UpdateSelection();
    }
    #endregion

    /// <summary>
    /// Up: 0 
    /// Down: 1 
    /// Left: 2 
    /// Right: 3 
    /// Shot: 4 
    /// Roll: 5 
    /// Check: 6 
    /// Inventroy, Quick Item: 7 
    /// Quick Soul: 8 
    ///  Quick Emotion: 9
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
        soundManager.BGSoundPlayDelayed(4, 3);
        OffPlayerUI(); // 비전투 상태에서는 UI를 숨김

        //gameover_Object;
        //gameover_text;
        //gameover_image;
        StartCoroutine(Okdo());
        StartCoroutine(Okdso());
        StartCoroutine(FadeIn());
        StartCoroutine(gameoverTextOn());
        BattleManager.DestroyActiveBullets();

    }
    public void End_And_Load()
    {
        StartCoroutine(FadeOut());
        StartCoroutine(Load_SavePoint());
        soundManager.FadeOutBGSound(3f);  // 게임오버 브금 끄기
        // Off gameover
        // last load -> go!

    }
    private IEnumerator Load_SavePoint()
    {
        yield return new WaitForSeconds(5f);
        gameover_Object.SetActive(false);
        gameManager.Load();

    }
    private IEnumerator gameoverTextOn()
    {
        yield return new WaitForSeconds(4f);
        gameover_text.gameObject.SetActive(true);
        dialogueManager.StartGameOverDialogue(0);

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
    private IEnumerator FadeOut()
    {
        yield return new WaitForSeconds(0.8f);
        gameover_text.gameObject.SetActive(false);
        float duration = 2f; // 2초 동안 진행
        float currentTime = 0f;

        // 현재 이미지 색상 가져오기
        Color color = gameover_image.color;
        color.a = 1f; // 시작할 때 알파값을 1로 설정
        gameover_image.color = color;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(1 - (currentTime / duration)); // 알파 값을 1에서 0으로

            // 이미지 색상의 알파값 업데이트
            color.a = alpha;
            gameover_image.color = color;

            yield return null;
        }

        // 반복문이 끝난 후 알파 값을 0으로 완전히 설정
        color.a = 0f;
        gameover_image.color = color;
    }

    private IEnumerator Okdo()
    {
        yield return new WaitForSeconds(0.1f);

        gameover_soul.GetComponent<Image>().sprite = soul_sprites[1];
        soundManager.SFXPlay("heartbreak1", 87);
    }
    private IEnumerator Okdso()
    {
        yield return new WaitForSeconds(1f);
        Color breakon_color = gameover_soul.GetComponent<Image>().color;
        gameover_soul.GetComponent<Image>().color = new Color(1, 1, 1, 0);
        soundManager.SFXPlay("heartbreak2", 89);
        RectTransform transform = gameover_soul.GetComponent<RectTransform>();
        gameover_soul.GetComponent<PieceShooter>().ShootPieces(transform, breakon_color);

    }

    #endregion
}
