using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public enum InGameMenuState : byte
{
    Hidden,
    MainPanel,
    Settings
}

public class InGameMenu : MonoBehaviour
{
    static InGameMenu _instance;
    public static InGameMenu Singleton()
    {
        if(_instance == null)
        {
            _instance = FindObjectOfType<InGameMenu>();
        }
        
        return _instance;
    }
    
    
    public Toggle fogs_toggle;
    public Toggle coats_toggle;
    public Toggle gore_toggle;
    
    
  

    public InGameMenuState state;
    
    bool canBeShown = true;
    bool wasCursorVisibleBeforeMenu = true;
    
    public GameObject canvas;
    public GameObject main;
    public GameObject settings;
    
    public void InGameMenuBackButton()
    {
        Hide();
    }
    
    public void InGameMenuSettingsBackButton()
    {
        SaveSettingsFromInGameMenu();
        settings.SetActive(false);
        main.SetActive(true);
    }
    
    public void InGameMenuSettingsButton()
    {
        settings.SetActive(true);
        main.SetActive(false);
    }
    
    
    public static bool IsVisible()
    {
        
        return Singleton().canvas.activeSelf;
    }
    
    public static void Lock()
    {
        Singleton().canBeShown = false;
        Hide();
    }
    
    public static void Allow()
    {
        Singleton().canBeShown = true;
    }
    
    public static void Show()
    {
        Singleton()._ShowInGameMenu();
    }
    
    void _ShowInGameMenu()
    {
        InGameConsole.LogFancy("ShowInGameMenu");
        UberManager.PauseGame();
        
        if(PlayerPrefs.GetInt("Fogs", 1) == 1)
            fogs_toggle.isOn = true;
        else
            fogs_toggle.isOn = false;
            
        
        currentMusicVolumeOnSlider = PlayerPrefs.GetFloat("MV", AudioManager.defaultMV);
        musicScrollbar.value = currentMusicVolumeOnSlider;
        
        currentEffectsVolumeOnSlider = PlayerPrefs.GetFloat("EV", AudioManager.defaultEV);
        effectsScrollbar.value = currentEffectsVolumeOnSlider;
        
        if(!canBeShown)
        {
            return;
        }
        
        ReleaseCursor();
        
        
        Singleton().canvas.SetActive(true);
    }
    
    public static void Hide()
    {
        Singleton()._HideInGameMenu();
    }
    
    public void LockCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    
    public void ReleaseCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    
    public void RestartCurrentLevelButton()
    {
        Hide();
        if(PhotonNetwork.IsMasterClient)
            NetworkObjectsManager.CallGlobalCommand(GlobalCommand.SetSavePoint, RpcTarget.All, -1);
        UberManager.SetSavePointPriority(-1);
        UberManager.Singleton().ReloadLevel();
    }
    
    
    void SaveSettingsFromInGameMenu()
    {
        InGameConsole.LogOrange("Saving settings form HideInGameMenu()");
            
            if(fogs_toggle.isOn)
            {
                PlayerPrefs.SetInt("Fogs", 1);
            }
            else
            {
                PlayerPrefs.SetInt("Fogs", 0);
            }
            currentMusicVolumeOnSlider = musicScrollbar.value;
            currentMusicVolumeOnSlider = Mathf.Clamp(currentMusicVolumeOnSlider, 0.0001f, 1f);
            PlayerPrefs.SetFloat("MV", currentMusicVolumeOnSlider);
            
            currentEffectsVolumeOnSlider = effectsScrollbar.value;
            currentEffectsVolumeOnSlider = Mathf.Clamp(currentEffectsVolumeOnSlider, 0.0001f, 1f);
            PlayerPrefs.SetFloat("EV", currentEffectsVolumeOnSlider);
    }
    
    void _HideInGameMenu()
    {
        // InGameConsole.LogFancy("HideInGameMenu");
        UberManager.ResumeGame();
        
        if(settings.activeSelf)
        {
            SaveSettingsFromInGameMenu();
        }
        
        if(PhotonManager.GetLocalPlayer() != null)
        {
            LockCursor();
        }
        
        //PlayerPrefs.SetInt("Fogs", (UberManager.UseFogs ? 1 : 0));
        //InGameConsole.LogFancy("SetInt fogs " + (UberManager.UseFogs ? 1 : 0));
        
        
        Singleton().canvas.SetActive(false);
    }
    
    public void GoToMainMenu()
    {
        
        if(PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
        
        
        UberManager.Load_Level_Locally(0);
    }
    
    bool GetEscapeKeyDown()
    {
        return Input.GetKeyDown(KeyCode.Escape);
    }
    
    
    public void CloseInGameMenuButton()
    {
        Hide();
    }
    
    
    void Awake()
    {
        if(_instance != null)
        {
            if(_instance != this)
            {
                Destroy(this.gameObject);
            }
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(this.gameObject);
            DontDestroyOnLoad(canvas);
            int currentSceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
            if(currentSceneIndex == 0 || currentSceneIndex == 1)
            {
                Lock();
            }
            else
            {
                Hide();
            }
        }
    }
    
    public void FogsToggle()
    {
        UberManager.Singleton().ToggleUseFogs();
    }
    
    public void OnMouseSensChanged()
    {
        
    }
    
    public void OnFovChanged()
    {
        
    }
    
    public void OnMasterVolumeChanged()
    {
        
    }
    
    public Scrollbar musicScrollbar;
    public Scrollbar effectsScrollbar;
    
    float currentEffectsVolumeOnSlider;
    float currentMusicVolumeOnSlider;
    
    public void OnMusicVolumeChanged()
    {
        musicScrollbar.value = Mathf.Clamp(musicScrollbar.value, 0.0001f, 1f);
        currentMusicVolumeOnSlider = musicScrollbar.value;
        float db = Mathf.Log10(currentMusicVolumeOnSlider) * 20f;
        AudioManager.Singleton().musicMasterMixer.audioMixer.SetFloat("MV", db);
        
//        InGameConsole.LogFancy("OnMusicVolumeChanged db: " + db.ToString("f"));
    }
    
    public void OnEffectsVolumeChanged()
    {
        effectsScrollbar.value = Mathf.Clamp(effectsScrollbar.value, 0.0001f, 1f);
        currentEffectsVolumeOnSlider = effectsScrollbar.value;
        float db = Mathf.Log10(currentEffectsVolumeOnSlider) * 20f;
        AudioManager.Singleton().effectsMixer.audioMixer.SetFloat("EV", db);
        
        //InGameConsole.LogFancy("OnEffectsVolumeChanged " + effectsScrollbar.value.ToString("f"));
    }
    
    void Update()
    {
        if(GetEscapeKeyDown() && !GameStats.IsShowing())
        {
            int level_index = UberManager.GetCurrentLevelIndex();
            if(level_index == 0 || level_index == 1)
            {
                return;
            }
            
            
            // switch(state)
            // {
            //     case(InGameMenuState.Hidden):
            //     {
            //         break;
            //     }
            //     case(InGameMenuState.MainPanel):
            //     {
            //         break;
            //     }
            //     case(InGameMenuState.Settings):
            //     {
            //         break;
            //     }
            // }
            if(Singleton().canvas.activeSelf)
            {
                if(settings.activeSelf)
                {
                    InGameMenuSettingsBackButton();
                }
                else
                {
                    Hide();
                }
            }
            else
            {
                Show();
            }
        }
    }
    
   
}
