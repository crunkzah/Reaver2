using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using ExitGames.Client.Photon;

public class PhotonManager : MonoBehaviourPunCallbacks, IInRoomCallbacks
{
    static PhotonManager _instance;
    
    public static PhotonManager Singleton()
    {
        if(_instance == null)
        {
            _instance = FindObjectOfType<PhotonManager>();
        }
        
        return _instance;
    }
    
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        Debug.Log("<color=yellow>OnMasterClientSwitched()</color>");
        if(UberManager.GetCurrentLevelIndex() != 0)
        {
            if(PhotonNetwork.InRoom)
            {
                PhotonNetwork.LeaveRoom();    
            }
            UberManager.Load_Level_Locally(0);
        }
    }
    
    
    public static PlayerController GetLocalPlayer()
    {
        if(_instance == null)
        {
            return null;
        }
        
        return Singleton().local_controller;
    }
    

    void Awake()
    {
        PhotonNetwork.UseRpcMonoBehaviourCache = true;
        PhotonNetwork.SerializationRate = 20;
        PhotonNetwork.SendRate = 20;
        
        //TODO : Maybe sync scene maybe not - 20.08.2019
        PhotonNetwork.AutomaticallySyncScene = true;
        
        if(_instance == null)
        {
            _instance = this;
            //DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            if(_instance != this)
            {
                InGameConsole.Log("<color=yellow>Destroying excess PhotonManager</color>");
                Destroy(this.gameObject);
            }
        }
        
    }

    float timeConnectToMasterStarted = 0f;
    
    void ConfigureConnection()
    {
        PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = "eu";
    }
    
    void Start()
    {
        InGameConsole.Log("<color=#FF6347>Press <color=yellow>'F'      </color> to spawn your player.</color>");
        InGameConsole.Log("<color=#FF6347>Press <color=yellow>'N'      </color> to sync objects on scene.</color>");
        InGameConsole.Log("<color=#FF6347>Press <color=yellow>'M'      </color> to reload whole scene.</color>");
        InGameConsole.Log("<color=#FF6347>Press <color=yellow>'Shift+Y'</color> to toggle lag simulation hud.</color>");
        InGameConsole.Log(string.Format("<color=#FF6347>SerializationRate:  <color=yellow>{0}</color>.</color>", PhotonNetwork.SerializationRate));
        InGameConsole.Log(string.Format("<color=#FF6347>SendRate:  <color=yellow>{0}</color>.</color>", PhotonNetwork.SendRate));
        
        PhotonNetwork.UseRpcMonoBehaviourCache = true;
        
        
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        SetupMyNickName();
    }
    
    
    public void ConnectToPhotonNetwork()
    {
        if(PhotonNetwork.OfflineMode)
        {
            if(PhotonNetwork.InRoom)
            {
                PhotonNetwork.LeaveRoom();
            }
            PhotonNetwork.Disconnect();
            PhotonNetwork.OfflineMode = false;
        }
        
        if(!PhotonNetwork.IsConnected)
        {
            timeConnectToMasterStarted = Time.time;
            ConfigureConnection();
            PhotonNetwork.ConnectUsingSettings();
        }
    }
    
    public List<GameObject> sceneObjectsToSynchronize;
    
    [PunRPC]
    public void SetTimeToSyncScene(double photonTime)
    {
        timeWhenToSync = photonTime;
        shouldSync = true;
        
        
        InGameConsole.Log(string.Format("<color=#FF8000>------- Syncing scene in {0:0} seconds-------</color>", (float)(photonTime - PhotonNetwork.Time)));
    }
    
    void OnSceneSync()
    {
        if(sceneObjectsToSynchronize != null)
        {
            for(int i = 0; i < sceneObjectsToSynchronize.Count; i++)
            {
                sceneObjectsToSynchronize[i].GetComponent<ISceneSyncable>().OnSceneSynchronization();
            }
        }
        InGameConsole.Log("<color=#FF8000>------- Synchronizing scene -------</color>");
    }
    
    
    public void SubscribeToOnSceneSync(GameObject obj)
    {
        if(sceneObjectsToSynchronize == null)
        {
            sceneObjectsToSynchronize = new List<GameObject>();
        }
        
        ISceneSyncable sceneSyncable = obj.GetComponent<ISceneSyncable>();
        
        if(sceneSyncable == null)
        {
            InGameConsole.LogError("<color=red>ISceneSyncable is null on " + obj.name + "</color>");
        }
        else
        {
            sceneObjectsToSynchronize.Add(obj);
        }
    }

    [Header("Testing vars:")]    
    public string playerPrefabName;
    
    public string roomNameToCreateOrJoin = "Room 1";
    
    public static string CreateRandomName()
    {
        string name = "";

        for (int counter = 1; counter <= 4; ++counter)
        {
            bool upperCase = (Random.Range(0, 2) == 1);

            int rand = 0;
            if (upperCase)
            {
                rand = Random.Range(65, 91);
            }
            else
            {
                rand = Random.Range(97, 123);
            }

            name += (char)rand;
        }

        return name;
    }
    
    public void LeaveRoom()
    {
        if(PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
    }
    
    
    public void CreateRoom()
    {
        roomNameToCreateOrJoin = CreateRandomName();
        
        // print("Creating room");
        InGameConsole.LogFancy("CreateRoom() " + roomNameToCreateOrJoin);
        
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 4;
        roomOptions.IsOpen = true;
        roomOptions.IsVisible = false;
        
                
        //In RoomOptions we should expect some user probably
        PhotonNetwork.CreateRoom(roomNameToCreateOrJoin, roomOptions);
    }
    
    
    public void JoinRoom(string roomId)
    {
        roomNameToCreateOrJoin = roomId;
        
        if(string.IsNullOrEmpty(roomNameToCreateOrJoin))
        {
            return;
        }
        
        PhotonNetwork.JoinRoom(roomId);
    }
    
    public override void OnCreateRoomFailed(short code, string msg)
    {
        Debug.Log(string.Format("<color=yellow>OnCreateRoomFailed(), code: {0}, msg: {1}</color>", code, msg));
        roomNameToCreateOrJoin = CreateRandomName();
    }
    
    public Transform spawnPlace;
    
    int spawnPlaceIndex = 0;

    public GameObject LocalDwarf;
    public PlayerController local_controller;
    
    bool wantGoSingleplayer = false;
    
    public static void GoSingleplayerMode()
    {
        if(PhotonNetwork.IsConnectedAndReady)
        {
            Singleton().wantGoSingleplayer = true;
            PhotonNetwork.Disconnect();
        }
        else
        {
            Singleton().wantGoSingleplayer = true;
        }
    }
    
    void StartSingleplayerMode()
    {
        PhotonNetwork.OfflineMode = true;
            
        if(!PhotonNetwork.InRoom)
        {
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.MaxPlayers = 1;
            roomOptions.IsOpen = false;
            roomOptions.IsVisible = false;
            
            
            PhotonNetwork.JoinOrCreateRoom("Offline room", roomOptions, TypedLobby.Default);
        }
    }
    
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("<color=yellow>OnDisconnected(), Cause:</color> " + cause.ToString());
    }
    

    public void SpawnMyPlayer()
    {
        int sceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
        
        if(sceneIndex == 0 || sceneIndex == 1)
            return;
        
        if(LocalDwarf != null)
            return;
            
        if(PhotonNetwork.InRoom)
        {
            InGameConsole.Log("<color=#B7C231>Spawning local player !</color>");
            
            SpawnPlace[] spawns = FindObjectsOfType<SpawnPlace>();
            
            if(spawns != null)
            {
                // spawnPlace = spawns.transform;
                spawnPlaceIndex++;
                if(spawnPlaceIndex >= spawns.Length)
                {
                    spawnPlaceIndex = 0;
                }
                
                spawnPlace = spawns[spawnPlaceIndex].transform;
            }
            
            
            Vector3 playerSpawnPosition = (spawnPlace == null) ? new Vector3(3.38f, 1.16f, -8f) : spawnPlace.position;
            
            LocalDwarf = PhotonNetwork.Instantiate(playerPrefabName, playerSpawnPosition, Quaternion.identity);
            //LocalDwarf = PhotonNetwork.Instantiate(playerPrefabName, playerSpawnPosition, spawnPlace.rotation);
            
            local_controller = LocalDwarf.GetComponent<PlayerController>();
            
            
            PlayerManager.Singleton().SetLocalPlayer(LocalDwarf);
            LocalDwarf.name = "Local_Dwarf";
            
            //FollowingCamera.Singleton().transform.position = playerSpawnPosition;
            StartUpScreen.Singleton().FadeIn(1.5f);
            //AudioManager.PlayClip(SoundType.PlayerSpawn, 0.5f, Random.Range(0.2f, 0.25f));
        }
    }
    
    public bool isConnected = false;
        
    public bool shouldSync = false;
    public double timeWhenToSync;
    
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if(scene.buildIndex == 0 || scene.buildIndex == 1)
        {
            InGameMenu.Lock();
            if(scene.buildIndex == 0)
            {
                if(PhotonNetwork.IsMasterClient && PhotonNetwork.InRoom)
                {
                    PhotonNetwork.CurrentRoom.IsOpen = true;
                }
            }
        }
        else
        {
            InGameMenu.Allow();
            if(PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.CurrentRoom.IsOpen = false;
            }
        }
        
        InGameConsole.Log(string.Format("Level '{0}' loaded!", scene.name));
        AudioManager.PlayClip(SoundType.level_start1, 0.09f, 1);
        
        //sceneObjectsToSynchronize.Clear();
        
        UberManager.Singleton().readyToSwitchLevel = true;
        
        ObjectPool.s().ResetPool();
        ObjectPool2.s().ResetPool();
        
        //WeaponManager.Singleton().ResetItemsOnGround();
        
        // if(PhotonNetwork.IsMasterClient)
        // {
        //     SyncSceneOnAllClients();
        // }
        
    }
    
    void SyncSceneOnAllClients()
    {
        photonView.RPC("SetTimeToSyncScene", RpcTarget.AllViaServer, PhotonNetwork.Time + 3d);
    }
    
    public Photon.Realtime.ClientState networkClientState;
    

    void Update()
    {
        if(wantGoSingleplayer)
        {
            if(PhotonNetwork.IsConnected == false)
            {
                wantGoSingleplayer = false;
                StartSingleplayerMode();
            }
        }
        
        networkClientState = PhotonNetwork.NetworkClientState;
        
        
        isConnected = PhotonNetwork.IsConnectedAndReady;
        
        if(shouldSync)
        {
            if(PhotonNetwork.Time > timeWhenToSync)
            {
                shouldSync = false;
                
                OnSceneSync();
            }
        }
        
        if(Input.GetKeyDown(KeyCode.V))
        {
            if(!InGameMenu.IsVisible())
            {
                if(LocalDwarf == null)
                    SpawnMyPlayer();
                else
                {
                    DestroyMyPlayer();      
                }
            }
            else
            {
                Debug.Log("<color=yellow>Can't spawn player!</color>");
            }
        }
        
        // if(Input.GetKeyDown(KeyCode.N) && PhotonNetwork.IsMasterClient)   
        // {
        //     photonView.RPC("SetTimeToSyncScene", RpcTarget.AllViaServer, PhotonNetwork.Time + 3d);
        // }
    }
    
    
    void SendPlayerInfo()
    {
        // object[] content = new object[] { new Vector3(10.0f, 2.0f, 5.0f), 1, 2, 5, 10 }; // Array contains the target position and the IDs of the selected units
        // RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All }; // You would have to set the Receivers to All in order to receive this event on the local client as well
        // PhotonNetwork.RaiseEvent(MoveUnitsToTargetPositionEventCode, content, raiseEventOptions, SendOptions.SendReliable);
    }
    
    
    public void DestroyMyPlayer()
    {
        if(LocalDwarf)
        {
            LocalDwarf.GetComponent<PlayerController>().OnDestroyCustom();
            PhotonNetwork.Destroy(LocalDwarf.GetComponent<PhotonView>());
        }
    }

    void OnGUI()
    {
        string text = string.Format("<color=blue>{0}</color>", networkClientState.ToString());
        GUIStyle style = GUIStyle.none;
        
        style.alignment = TextAnchor.MiddleCenter;
        
        float rectWidth = 250;
        
        GUI.Label(new Rect((Screen.width - rectWidth)/2, Screen.height - 34, rectWidth, 50), text, style);
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("<color=yellow>OnCreatedRoom(), roomId: </color>" + PhotonNetwork.CurrentRoom.Name);
    }

    public override void OnJoinedRoom()
    {
        if(PhotonNetwork.OfflineMode == false && MainMenuManager.Singleton())
        {
            MainMenuManager.Singleton().GoLobbyPage();
        }
        Debug.Log("<color=yellow>OnJoinedRoom(), roomId: </color>" + PhotonNetwork.CurrentRoom.Name);
    }
    
    public override void OnJoinRoomFailed(short code, string msg)
    {
        Debug.Log(string.Format("<color=yellow>OnJoinRoomFailed(), code: {0}, msg: {1}</color>", code, msg));
    }
    
    public override void OnLeftRoom()
    {
        if(MainMenuManager.Singleton())
        {
            MainMenuManager.Singleton().OnRoomLeftSelf();
        }
        Debug.Log("<color=yellow>OnLeftRoom()</color>");
    }
    
    
    // float reloadLevelTimer = 0f;
    // bool waitingToLoadLevel = false;
    
    public void DisconnectFromPhoton()
    {
        InGameConsole.LogOrange("<color=yellow>DisconnectFromPhoton()</color> was called");
        
        
        PhotonNetwork.Disconnect();
    }

    [PunRPC]
    public void ReloadLevel()
    {
        int currentSceneBuildIndex = SceneManager.GetActiveScene().buildIndex;
        
        PhotonNetwork.LoadLevel(currentSceneBuildIndex);
    }

    public override void OnPlayerEnteredRoom(Player player)
    {
        NetworkObjectsManager.Singleton().ClearCommands();
        
        string msg = string.Format("<color=blue> <color=yellow>{1}</color> has joined the room! </color>. Are we a master ? <color=orange>{0}</color>", PhotonNetwork.IsMasterClient, player.NickName);
        InGameConsole.Log(msg);
    }
    
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        //Master has left!
        
        Debug.Log(string.Format("<color=yellow>OnPlayerleftRoom(): <color=green>{0}</color> has left room!</color>", otherPlayer.NickName));
        //Debug.Log("<color=yellow>Are we Master? </color>" + PhotonNetwork.IsMasterClient);
        // if(otherPlayer.IsMasterClient)
        // {
        //     Debug.Log(string.Format("<color=yellow>OnPlayerleftRoom(): <color=green>Now leaving room and going to main menu since we are not Master!</color></color>", otherPlayer.NickName));
        //     PhotonNetwork.LeaveRoom();
        //     if(UberManager.GetCurrentLevelIndex() != 0)
        //     {
        //         UberManager.Load_Level_Locally(0);
        //     }
        //     else
        //     {
        //         MainMenuManager.Singleton().OnRoomLeftOther();
        //     }
        // }
    }
    
    public override void OnConnected()
    {
        InGameConsole.Log("OnConnected()");
    }

    public override void OnConnectedToMaster()
    {
        InGameConsole.Log("OnConnectedToMaster()");
        
        if(PhotonNetwork.OfflineMode)
        {
            string msg = string.Format("<color=green> Connected in offline mode.</color>");
            InGameConsole.Log(msg);
        }
        else
        {
            string msg = string.Format("<color=green> Connected to master.</color>Time spent connecting: {0}", (Time.time - timeConnectToMasterStarted).ToString("F"));
            
            InGameConsole.Log(msg);
        }
        
        
        
        //JoinOrCreateRoom();
    }

    void SetupMyNickName()
    {
        //Steam, gog, xbox stuff
        char c = (char)('A' + Random.Range (0,26));
        PhotonNetwork.NickName = "Crunkz_" + c;
    }
    
    public enum NetworkEvent : byte
    {
        Default = 1
    }
    

}
