using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;


public class MainMenuManager : MonoBehaviour
{
    static MainMenuManager _instance;
    public static MainMenuManager Singleton()
    {
        if(_instance == null)
        {
            _instance = FindObjectOfType<MainMenuManager>();
        }
        
        return _instance;
    }
    
    [Header("Pages:")]
    public GameObject MainMenu_Page;
    public GameObject Play_Page;
    public GameObject Settings_Page;
    public GameObject Credits_Page;
    public GameObject Singleplayer_Page;
    public GameObject Multiplayer_Page;
    public GameObject Lobby_Page;
    [Header("Elements:")]
    public TMP_InputField roomId_inputField;
    
    public List<GameObject> pages = new List<GameObject>();
    
    public TextMeshProUGUI isConnected_tmp;
    public TextMeshProUGUI connect_tmp_btn;
    
    static string connectString = "Connect";
    static string disconnectString = "Disconnect";
    
    static string connectedString = "Connected";
    static string not_connectedString = "Not connected";
    
    public ParticleSystem[] menu_pses;
    
    void Awake()
    {
        pages.Add(MainMenu_Page);
        pages.Add(Play_Page);
        pages.Add(Settings_Page);
        pages.Add(Credits_Page);
        pages.Add(Singleplayer_Page);
        pages.Add(Multiplayer_Page);
        pages.Add(Lobby_Page);
    }
    
    void Start()
    {
        GoMainMenu();
        // if(UberManager.Singleton() != null)
        //     AudioManager.SetMusicMainMenu();
    }
    
    
    void Update()
    {
        if(isConnected_tmp)
        {
            if(PhotonNetwork.IsConnectedAndReady)
            {
                if(!isConnected_tmp.text.Equals(connectedString))
                {
                    
                    if(PhotonNetwork.IsMasterClient)
                    {
                        if(PhotonNetwork.OfflineMode)
                        {
                            isConnected_tmp.SetText(connectedString + " (Singleplayer)");
                        }
                        else
                            isConnected_tmp.SetText(connectedString + " (Master)");
                    }
                    else
                        isConnected_tmp.SetText(connectedString);
                    
                    isConnected_tmp.color = Color.green;
                    
                    
                    
//                    connect_tmp_btn.SetText(disconnectString);
                }
            }
            else
            {
                if(!isConnected_tmp.text.Equals(not_connectedString))
                {
                    isConnected_tmp.SetText(not_connectedString);
                    isConnected_tmp.color = Color.yellow;
                    
                    
                    
                    
//                    connect_tmp_btn.SetText(connectString);
                }
            }
        }
        
        
    }
    
    public void JoinRoomButton()
    {
        string roomId = roomId_inputField.text;
        
        PhotonManager.Singleton().JoinRoom(roomId);
    }
    
    public void CreateRoomButton()
    {
        if(PhotonNetwork.IsConnectedAndReady)
        {
        //PhotonManager.Singleton().LeaveRoom();
            Debug.Log("CreateRoomButton()");
            PhotonManager.Singleton().CreateRoom();
        }
    }
    
    // public void SingleplayerButton()
    // {
    //     GoSinglePlayerPage();
    //     PhotonManager.GoSingleplayerMode();
    //     // InGameConsole.Log("<color=yellow>Singleplayer</color> Button was pressed !");
    // }
    
    // public void MultiplayerButton()
    // {
    //     GoMultiPlayerPage();
    //     // InGameConsole.Log("<color=yellow>Multiplayer</color> Button was pressed !");
    // }
    
    public void OptionsButton()
    {
        GoSettingsMenu();
        InGameConsole.Log("<color=yellow>Options</color> Button was pressed !");
    }
    
    public void PlayButton()
    {
        GoPlayPage();
    }
    
    public void CreditsButton()
    {
        GoCreditsPage();
        // InGameConsole.Log("<color=yellow>Credits</color> Button was pressed !");
    }
    
    public void QuitButton()
    {
        Application.Quit();
    }
    
    public void OfflineMode()
    {
        if(PhotonNetwork.OfflineMode == false && PhotonNetwork.IsConnected)
        {
            return;
        }
        
        PhotonNetwork.OfflineMode = !PhotonNetwork.OfflineMode;
    }
    
    public void LeaveRoomButton()
    {
        PhotonManager.Singleton().LeaveRoom();
    }
    
    public void OnRoomLeftSelf()
    {
        Debug.Log("<color=yellow>MainMenuManager(): OnRoomLeft()");
        if(Lobby_Page.activeSelf)
        {
            GoPlayPage();
        }
    }
    
    public void OnRoomLeftOther()
    {
        if(Lobby_Page.activeSelf)
        {
            GoMultiPlayerPage();
        }
    }
    
    public void DisconnectButton()
    {
        if(PhotonNetwork.IsConnected)
        {
            PhotonManager.Singleton().DisconnectFromPhoton();
            InGameConsole.LogFancy("Disconnecting from Photon network...");
        }
        else
        {
            PhotonManager.Singleton().ConnectToPhotonNetwork();
        }
    }
    
    void EnableMenuParticleSystems()
    {
        for(int i = 0; i < menu_pses.Length; i++)
        {
            menu_pses[i].gameObject.SetActive(true);
            //menu_pses[i].Play();
        }
    }
    
    void DisableMenuParticleSystems()
    {
        for(int i = 0; i < menu_pses.Length; i++)
        {
            menu_pses[i].gameObject.SetActive(false);
        }
    }
    
    public void GoMainMenu()
    {
        DisableAllPages();
        MainMenu_Page.SetActive(true);
        EnableMenuParticleSystems();
        // Debug.Log("Go Main Menu");
    }
    
    public void GoSinglePlayerPage()
    {
        DisableAllPages();
        PhotonManager.GoSingleplayerMode();
        Singleplayer_Page.SetActive(true);
        DisableMenuParticleSystems();
    }
    
    public void GoSettingsMenu()
    {
        DisableAllPages();
        Settings_Page.SetActive(true);
        EnableMenuParticleSystems();
    }
    
    public void GoMultiPlayerPage()
    {
        DisableAllPages();
        Multiplayer_Page.SetActive(true);
        PhotonManager.Singleton().ConnectToPhotonNetwork();
        EnableMenuParticleSystems();
        if(PhotonNetwork.OfflineMode == false && PhotonNetwork.InRoom)
        {
            GoLobbyPage();
        }
    }
    
    public void GoCreditsPage()
    {
        DisableAllPages();
        Credits_Page.SetActive(true);
        EnableMenuParticleSystems();
    }
    
    public void GoPlayPage()
    {
        DisableAllPages();
        Play_Page.SetActive(true);
        EnableMenuParticleSystems();
    }
    
    public void GoLobbyPage()
    {
        DisableAllPages();
        Lobby_Page.SetActive(true);
        DisableMenuParticleSystems();
    }
    
    void DisableAllPages()
    {
        for(int i = 0; i < pages.Count; i++)
        {
            // Debug.Log(string.Format("Disabling <color=yellow>{0}</color>", pages[i].name));
            pages[i].SetActive(false);
        }
    }
    
    
    public void Level1_Button()
    {
        UberManager.Load_Level(3);
    }
    
    public void Level2_Button()
    {
        UberManager.Load_Level(4);
    }
    
    public void Level3_Button()
    {
        UberManager.Load_Level(5);
    }
    
    public void Level4_Button()
    {
        UberManager.Load_Level(6);
    }
}
