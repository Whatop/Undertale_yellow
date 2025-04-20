using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using UnityEngine.U2D;

using UnityEngine.SceneManagement;
using System;
[System.Serializable]
public class RadialSegment
{
    public string segmentName;
    public Image icon;
    public GameObject highlightObj;

    public void SetHighlight(bool isOn)
    {
        if (highlightObj != null)
            highlightObj.SetActive(isOn);
        // 아이콘 크기/색상 변경 로직 등 가능
    }

    public void ExecuteAction()
    {
        // 감정표현 실행(애니메이션, 사운드 재생, 네트워크 전송 등)
        Debug.Log($"감정표현 [{segmentName}] 실행");
    }
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
    public bool isMouseShot= true;
    public bool isMouseRoll= true;

    [SerializeField]
    private int currentIndex = 0;

    //Camera 관련
    [SerializeField]
    private int curRIndex = 6; // curResolutionsIndex
    private List<Resolution> predefinedResolutions;
    public PixelPerfectCamera pixelPerfectCamera;


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
    public GameObject radialMenuPanel;      // 라디얼 메뉴 전체를 담고 있는 Panel
    public RectTransform centerPoint;       // 라디얼 메뉴의 중심(마우스 기준점)
    public List<RadialSegment> segments;    // 세그먼트 리스트 (감정표현/아이템 등)

    [Header("Radial Menu Settings")]
    public KeyCode toggleKey = KeyCode.G;   // 라디얼 메뉴 열기/닫기 키
    public bool isRadialMenuActive = false; // 현재 라디얼 메뉴 활성화 여부

    private int current_segment_Index = -1;          // 현재 하이라이트된 세그먼트 인덱스

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
        BattleManager = BattleManager.Instance;
        dialogueManager = DialogueManager.Instance;
    }

    private void Start()
    {
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
        keyBindings[7] = KeyCode.E;
        keyBindings[8] = KeyCode.Tab;
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
        InitWeapon();
        ShowPanel("Game");
        OptionInput();
        UpdateUI();
        SaveOff();
        OffPlayerUI(); // 비전투 상태에서는 UI를 숨김

        hpRect = hpBar.GetComponent<RectTransform>();
        ResizeHpBar(gameManager.GetPlayerData().Maxhealth);
    }

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

        HandleInput();
        UpdateSoulPosition();
        UpdateInventoryUI();

        //MouseCusor
        Vector2 mousePosition = Input.mousePosition;
        crosshairTransform.position = mousePosition;

        if (Input.GetKeyDown(KeyCode.E))
        {
            RectTransform transform = gameover_soul.GetComponent<RectTransform>();
            gameover_soul.GetComponent<PieceShooter>().ShootPieces(transform);

        }
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
        // 1) 라디얼 메뉴 토글 처리
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleRadialMenu();
        }

        // 2) 메뉴 활성 상태라면 마우스 위치와 클릭 여부 판단
        if (isRadialMenuActive)
        {
            UpdateRadialSelection();

            // 마우스 왼쪽 버튼 떼는 순간 해당 감정표현/아이템 선택 확정
            if (Input.GetMouseButtonUp(0) && currentIndex >= 0)
            {
                OnSelectSegment(currentIndex);
            }
        }
    }
    /// <summary>
      /// 라디얼 메뉴 열기/닫기
      /// </summary>
    public void ToggleRadialMenu()
    {
        isRadialMenuActive = !isRadialMenuActive;
        radialMenuPanel.SetActive(isRadialMenuActive);

        // 열 때, 중앙 위치 설정 (마우스 위치 등에)
        if (isRadialMenuActive)
        {
            Vector3 mousePos = Input.mousePosition;
            centerPoint.position = mousePos;
        }
    }

    /// <summary>
    /// 마우스 각도에 따라 하이라이트할 세그먼트 계산
    /// </summary>
    private void UpdateRadialSelection()
    {
        Vector2 dir = Input.mousePosition - centerPoint.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        angle = (angle + 360f) % 360f; // 0~360 범위로 맞춤

        float segmentAngle = 360f / segments.Count;
        int newIndex = (int)(angle / segmentAngle);

        if (newIndex != current_segment_Index)
        {
            // 이전 하이라이트 제거
            if (current_segment_Index >= 0 && current_segment_Index < segments.Count)
            {
                segments[current_segment_Index].SetHighlight(false);
            }

            // 새 인덱스 하이라이트
            current_segment_Index = newIndex;
            segments[current_segment_Index].SetHighlight(true);
        }
    }

    /// <summary>
    /// 세그먼트 선택이 확정되었을 때 실행
    /// </summary>
    private void OnSelectSegment(int index)
    {
        RadialSegment seg = segments[index];
        Debug.Log($"Segment {index} 선택됨 - {seg.segmentName}");
        seg.ExecuteAction();

        // 선택 후 메뉴 닫을 수도 있음
        ToggleRadialMenu();
    }

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
        stat_texts[8].text = "갑옷: " + gameManager.GetPlayerData().curAmmor.itemName;
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
                switch (inventroy_curNum)
                {
                    case 0:
                        //사용
                        gameManager.UseItem(item_prevNum);
                        OnPanel(-1);
                        break;

                    case 1:
                        // 정보
                        gameManager.InfoItem(item_prevNum);
                        OnPanel(-1);
                        break;
                    case 2:
                        //버리기
                        Debug.Log(inventroy_curNum);
                        gameManager.DropItem(item_prevNum);
                        OnPanel(-1);
                        break;
                }
                // gameManager.GetPlayerData().inventory[0].id = 
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

    public void ResizeHpBar(int maxHealth)
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
        int currentHealth = player.health;
        int maxHealth = player.Maxhealth;

        // 체력 이미지 업데이트
        hpBar.value = Mathf.Clamp((float)currentHealth / (float)maxHealth, 0, 1);
        hpBar_text.text = currentHealth.ToString() + " / " + maxHealth.ToString();
    }

    void InitWeapon() // 총 새팅
    {
        Weapon weapon = gameManager.GetWeaponData();

        int ammo_count = weapon.magazine;
        ui_ammo = new GameObject[ammo_count];

        for (int i = 0; i < ammo_count; i++)
        {
            GameObject weaponPrefab = Resources.Load<GameObject>("Prefabs/Ammo");
            GameObject instance = Instantiate(weaponPrefab, ui_positions[0].transform);

            float sizeY = instance.GetComponent<RectTransform>().sizeDelta.y;
            Vector3 newPosition = instance.transform.position;
            newPosition.x = ui_positions[0].transform.position.x + i * sizeY * 1.25f; // 세로 방향으로 위치 설정
            instance.transform.position = newPosition;
            if(ammo_count == i + 1)
            {
            newPosition.x *= 1.05f; // 세로 방향으로 위치 설정
                top.transform.position = newPosition;
            }
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
        PlayerData player = gameManager.GetPlayerData();
        int currentHealth = player.health;
        int maxHealth = player.Maxhealth;

        ResizeHpBar(maxHealth);

        // 체력 이미지 업데이트
        hpBar.value = Mathf.Clamp((float)currentHealth / (float)maxHealth, 0, 1);

        hpBar_text.text = currentHealth.ToString() + " / " + maxHealth.ToString();

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
        PlayerPrefs.SetInt("IsMouseShot", isMouseShot? 1 : 0);

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
        keyBindings[7] = KeyCode.E;
        keyBindings[8] = KeyCode.Tab;
        // UI 업데이트
        Screen.fullScreen = isFullScreen;
        fullScreenToggle.image.sprite = isFullScreen ? onSprite : offSprite;
        QualitySettings.vSyncCount = isVSyncOn ? 1 : 0;
        vSyncToggle.image.sprite = isVSyncOn ? onSprite : offSprite;
        Cursor.visible = isCursorVisible;
        cusorToggle.image.sprite = isCursorVisible ? onSprite : offSprite;

        mouseRollToggle.image.sprite = isMouseRoll ? onSprite : offSprite;
        mouseShotToggle.image.sprite = isMouseShot? onSprite : offSprite;
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
        if (Input.GetKeyDown(KeyCode.Escape)&& !GameManager.Instance.GetPlayerData().isDie)
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
                        if (currentIndex == 1 || currentIndex == 3 || currentIndex == 4)
                            ToggleValue();  // ToggleValue 호출
                        else
                        {
                            currentButtons[currentIndex].onClick.Invoke();
                            
                         if(currentIndex != 2&& currentIndex != 5 && currentIndex != 6 && currentIndex != 7 && currentIndex != 8 && currentIndex != 0)
                            soundManager.SFXPlay("select_sound", 173);
                        }
                    }
                    else
                    {
                        currentButtons[currentIndex].onClick.Invoke();
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
    void UpdateResolution(int width, int height)
    {
        // Pixel Perfect 카메라 해상도 설정
        pixelPerfectCamera.refResolutionX = width;
        pixelPerfectCamera.refResolutionY = height;

        // Canvas Scaler와 연동
        CanvasScaler scaler = uicanvas.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.referenceResolution = new Vector2(width, height);
        }
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
        soundManager.SFXPlay("select_sound", 173);
        StartCoroutine(ForceToggleUpdate());
    }
    public void ToggleKeyValue()
    {
        switch (currentIndex)
        {
            case 9: // 마우스 조준
                ToggleMouseShot();
                Debug.Log("마우스 조준 만들어");
                break;
            case 10: // 마우스로 돌진
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
        isMouseRoll= !isMouseRoll;
        UpdateButtonState(mouseRollToggle, isMouseRoll);
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

        gameover_soul.GetComponent<Image>().color = new Color(1, 1, 1, 0);
        soundManager.SFXPlay("heartbreak2", 89);
        RectTransform transform = gameover_soul.GetComponent<RectTransform>();
        gameover_soul.GetComponent<PieceShooter>().ShootPieces(transform);

    }

    #endregion
}
