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

    public InGameMenuState state;
    
    bool canBeShown = true;
    bool wasCursorVisibleBeforeMenu = true;
    
    public GameObject canvas;
    
    
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
        UberManager.SetSavePointPriority(-1);
        UberManager.Singleton().ReloadLevel();
    }
    
    void _HideInGameMenu()
    {
        InGameConsole.LogFancy("HideInGameMenu");
        UberManager.ResumeGame();
        
        if(PhotonManager.GetLocalPlayer() != null)
        {
            LockCursor();
        }
        
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
    
    void Update()
    {
        if(GetEscapeKeyDown())
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
                Hide();
            }
            else
            {
                Show();
            }
        }
    }
    
   
}
