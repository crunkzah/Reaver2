using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

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
        UberManager.Singleton().ReloadLevel();
    }
    
    void _HideInGameMenu()
    {
        InGameConsole.LogFancy("HideInGameMenu");
        
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
