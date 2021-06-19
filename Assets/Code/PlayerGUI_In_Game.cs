using UnityEngine;
using UnityEngine.UI;
using TMPro;



public class PlayerGUI_In_Game : MonoBehaviour
{

#region  Singleton
    static PlayerGUI_In_Game _instance;
    public static PlayerGUI_In_Game Singleton()
    {
        if(_instance == null)
            _instance = FindObjectOfType<PlayerGUI_In_Game>();
        return _instance;
    }
#endregion

    void Awake()
    {
        DontDestroyOnLoad(transform.parent.gameObject);
        
    }
    
    
    
    PlayerController localPlayer;
    
    public RectTransform staminaBar;
    public TextMeshProUGUI staminaText;
    
    int hp = 0;
    
    public RectTransform hpBar;
    public TextMeshProUGUI hpText;
    
    
    const float barFullWidth = 250f;
    int playerHp;
    float playerStamina;
    
    
    public GameObject crosshair;
    
    
    Camera fpsCamera;
    
    public void AssignPlayerGUI(PlayerController _player, Camera _fpsCamera)
    {
        localPlayer = _player;
        fpsCamera = _fpsCamera;
    }
    
    
    
    void Update()
    {
        if(localPlayer)
        {
            if(!hpBar.gameObject.activeSelf)
            {
                hpBar.gameObject.SetActive(true);
                hpText.gameObject.SetActive(true);
            }
            if(!staminaBar.gameObject.activeSelf)
            {
                staminaBar.gameObject.SetActive(true);
                staminaText.gameObject.SetActive(true);
            }
            if(!crosshair.activeSelf)
            {
                crosshair.SetActive(true);
            }
            
            if(playerHp != localPlayer.HitPoints)
            {
                ProcessHealth();
            }
            
            if(playerStamina != localPlayer.Stamina)
            {
                ProcessStamina();
            }
            
            
        }
        else
        {
            if(hpBar.gameObject.activeSelf)
            {
                hpBar.gameObject.SetActive(false);
                hpText.gameObject.SetActive(false);
                staminaBar.gameObject.SetActive(false);
                staminaText.gameObject.SetActive(false);
            }
            if(crosshair.activeSelf)
            {
                crosshair.SetActive(false);
            }
            
        }
        
    }
    
    void ProcessHealth()
    {
        playerHp = localPlayer.HitPoints;
        
        // int maxHealth = PhotonManager.Singleton().local_controller.GetMaxHealth();
        // float t = Mathf.InverseLerp(0, maxHealth, playerHp);
        // t = Mathf.Clamp(t, 0f, 1f);
        // float hp_bar_width = Mathf.Lerp(0, barFullWidth, t);
        // hpBar.sizeDelta = new Vector2(hp_bar_width,  hpBar.sizeDelta.y);
        
        hpText.SetText(playerHp.ToString());
    }
    
    void ProcessStamina()
    {
        playerStamina = localPlayer.Stamina;
                
        // float maxStamina = 100f;
        // float t = Mathf.InverseLerp(0, maxStamina, playerStamina);
        // t = Mathf.Clamp(t, 0f, 1f);
        // float stamina_bar_width = Mathf.Lerp(0, barFullWidth, t);
        // staminaBar.sizeDelta = new Vector2(stamina_bar_width,  hpBar.sizeDelta.y);
        
        int stamina_int = Mathf.FloorToInt(playerStamina);
           
        staminaText.SetText((stamina_int).ToString());
    }
    
    public void Hide()
    {
        transform.parent.gameObject.SetActive(false);
    }

    void OnDisable()
    {
        
        
    }
    
    void Start()
    {
        // Hide();
    }
}
