using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class UIManager : MonoBehaviour
{
    GameManager gameManager;

    private static UIManager instance;
    public GameObject[] ui_positions; //health : 0, Weapon : 1
    
    [SerializeField]
    private GameObject[] ui_healths;

    [SerializeField]
    private GameObject[] ui_ammo;
    public Image ui_weaponImage;
    public TextMeshProUGUI ui_ammoText;
    float screenRatio;

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
    }
    private void Start()
    {
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;
        float screenRatio = screenWidth / screenHeight;

        InitHeart();
        InitWeapon();
    }
    void InitHeart() // 체력 새팅
    {
        PlayerData player = gameManager.GetPlayerData();

        int heart_count = player.Maxhealth / 2;
        ui_healths = new GameObject[heart_count];

        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        for (int i = 0; i < heart_count; i++)
        {
            GameObject heartPrefab = Resources.Load<GameObject>("Prefabs/Heart");
            GameObject instance = Instantiate(heartPrefab, ui_positions[0].transform);

            float sizeX = instance.GetComponent<RectTransform>().sizeDelta.x;

            Vector3 newPosition = instance.transform.position;
            newPosition.x = ui_positions[0].transform.position.x + i * sizeX; // 가로 방향으로 위치 설정
            instance.transform.position = newPosition;
            ui_healths[i] = instance;
        }
    }
    void InitWeapon() // 총 새팅
    {
        Weapon weapon = gameManager.GetWeaponData();

        int ammo_count = weapon.currentAmmo;
        ui_ammo = new GameObject[ammo_count];

        float screenWidth = Screen.width;
        float screenHeight = Screen.height;
         screenRatio = screenWidth / screenHeight;

        for (int i = 0; i < ammo_count; i++)
        {
            GameObject weaponPrefab = Resources.Load<GameObject>("Prefabs/Ammo");
            GameObject instance = Instantiate(weaponPrefab, ui_positions[1].transform);

            float sizeY = instance.GetComponent<RectTransform>().sizeDelta.y;
            Debug.Log(sizeY);
            Vector3 newPosition = instance.transform.position;
            newPosition.y = ui_positions[1].transform.position.y + i * sizeY; // 세로 방향으로 위치 설정
            instance.transform.position = newPosition;
            ui_ammo[i] = instance;
        }
    }
    private void Update()
    {
        PlayerData player = gameManager.GetPlayerData();
        int currentHealth = player.health;

        // 체력 이미지 업데이트
        for (int i = 0; i < ui_healths.Length; i++)
        {
            int spriteIndex = Mathf.Clamp(currentHealth - i * 2, 0, 2); // 체력에 따른 스프라이트 인덱스 계산
            ui_healths[i].GetComponent<HeartScript>().SetImage(spriteIndex);
        }
        Weapon weapon = gameManager.GetWeaponData();
        int currentAmmo = weapon.currentAmmo;

        // 총알 이미지 업데이트
        for (int i = 0; i < ui_ammo.Length; i++)
        {
            // 총알이 남아있는 경우
            if (i < currentAmmo)
            {
                ui_ammo[i].SetActive(true); // 총알 UI 활성화
            }
            else
            {
                ui_ammo[i].SetActive(false); //y 총알 UI 비활성화
            }
        }
    }
}
