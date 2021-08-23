using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AI;
using Photon.Pun;

public enum Level : int
{
    Lobby,
    UberManager,
    Prologue,
    Level_1,
    LabyrinthRadial,
    Level_2,
    Level_Flat,
    Level_Clouds
}



public enum NPCType : byte
{
    Sinclaire,
    Padla,
    Stepa,
    SniperGirl,
    PadlaLong,
    Olios,
    CatLady,
    Scourge,
    ScourgeCool
}

public enum Language : int
{
    English,
    Russian
}

public enum InGameTimerState : int
{
    NotCounting,
    Working
}

public class UberManager : MonoBehaviour
{
    public static UberManager instance;
    public static UberManager Singleton()
    {
        if(instance == null)
        {
            instance = FindObjectOfType<UberManager>();
        }
        return instance;
    }
    
    public static Language lang = Language.English;
    
    
    [Header("Mouse cursor:")]
    public Texture2D mouseTex;
    
    [Header("Debug purposes only:")]
    public Material redDemon;
    public Material blueDemon;
    
    public List<GameObject> players = new List<GameObject>(4);
    public List<PlayerController> players_controller = new List<PlayerController>(4);
    
    public static int GetCurrentLevelIndex()
    {
        return SceneManager.GetActiveScene().buildIndex;
    }
    
    public void AddPlayerToList(GameObject player)
    {
        for(int i = 0; i < players.Count; i++)
        {
            if(players[i] == null)
            {
                players[i] = player;
            }
        }
        
        if(!players.Contains(player))
        {
            players.Add(player);
            players_controller.Add(player.GetComponent<PlayerController>());
            
            InGameConsole.LogFancy("Added player !");
        }
    }
    
    
    
    
      
    void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            //InGameConsole.LogError("<color=red>UberManager collision!</color>");
            if(instance.GetInstanceID() != this.GetInstanceID())
            {
                PhotonView _pv = GetComponent<PhotonView>();
                if(_pv)
                {
                    this.enabled = false;
                    Destroy(this.gameObject);
                }
            }
        }
    }
    

    LocalSettings local_settings;

    void Start()
    {
       local_settings = new LocalSettings();
    //    local_settings.ReadLocalSettings();
    
       Application.targetFrameRate = 144;
    }
    
    void OnLevelChanged(Scene prev, Scene now)
    {
        InGameConsole.LogOrange(string.Format("OnLevelChanged: <color=yellow>{0}</color>-><color=blue>{1}</color>", prev.name, now.name));
        
        
        timeSinceLevelLoaded = TimeSinceStart();
    }
    
    public static float deltaTime = 0f;
    public static float time = 0f;
    public static float timeSinceLevelLoaded = 0f;
    
    static bool isDeltaTimeUpdated = false;
    static bool isTimeUpdated = false;
    
    public static float TimeSinceStart()
    {
        if(!isTimeUpdated)
        {
            time = Time.time;
            isTimeUpdated = true;
        }
        
        return time;
    }
    
    const double commandsDelay = 0.125d;
    
    public static double GetPhotonTimeDelayed()
    {
        return PhotonNetwork.Time + GetCommandsDelay();
    }
    
    public static bool IsOnlineMode()
    {
        return !PhotonNetwork.OfflineMode;
    }
   
    
    public static double GetPhotonTimeDelayedBy(float AddDelay)
    {
        return PhotonNetwork.Time + (double)AddDelay;
    }
    
    public static float GetPhotonTime()
    {
        return (float)PhotonNetwork.Time;
    }
    
    public static double GetCommandsDelay()
    {
        if(PhotonNetwork.OfflineMode)
        {
            return 0;
        }
        else
        {
            return commandsDelay;
        }
    }
    static bool isGamePaused = false;
    
    public static void PauseGame()
    {
        if(PhotonNetwork.OfflineMode)
            isGamePaused = true;
        else
            isGamePaused = false;
            
        Time.timeScale = 0;
        
        // PlayerController local_pc = PhotonManager.GetLocalPlayer();
        // if(local_pc)
        // {
        //     local_pc.ReleaseCursor();
        // }
    }
    
    public static void ResumeGame()
    {
        isGamePaused = false;
        Time.timeScale = 1;
        
        PlayerController local_pc = PhotonManager.GetLocalPlayer();
        // if(local_pc)
        // {
        //     local_pc.LockCursor();
        // }
    }
    
    public static float UnscaledDeltaTime()
    {
        return Time.unscaledDeltaTime;
        // if(!isDeltaTimeUpdated)
        // {
        //     deltaTime = Time.deltaTime;
        //     isDeltaTimeUpdated = true;
        // }
        
        // if(isGamePaused)
        // {
        //     return 0;
        // }
        
        // return deltaTime;
    }
    
    public static float DeltaTime()
    {
        if(!isDeltaTimeUpdated)
        {
            deltaTime = Time.deltaTime;
            isDeltaTimeUpdated = true;
        }
        
        if(isGamePaused)
        {
            return 0;
        }
        
        return deltaTime;
    }
    
    int screenshots = 0;
    
    public bool readyToSwitchLevel = true;
    
    int[] targetFps = {-1, 30, 60, 120};
    int fpsIndex = 0;
    
    public void ReloadLevel()
    {
        LoadLevel(SceneManager.GetActiveScene().buildIndex);
    }
    
    public void CommandLevel(string lvl)
    {
        if(lvl.Equals("menu"))
        {
            LoadLevel(0);
        }
    }
    
    public static void Load_Level_Locally(int level_index)
    {
        SceneManager.LoadScene(level_index);    
    }
    
    
    // public static void ReloadLevel()
    // {
    //     Singleton().ReloadLevel();
    // }
    
    // void ReloadLevel()
    // {
    //     int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
    // }
    
    public static void Load_Level(int level_index)
    {
        Singleton().LoadLevel(level_index);
    }
    
    public int currentSavePointPriorityLevel = -1;
    
    public static void SetSavePointPriority(int priority)
    {
        Singleton()._SetSavePointPriority(priority);
    }
    
    void _SetSavePointPriority(int priority)
    {
        currentSavePointPriorityLevel = priority;
    }
    
    public static int GetSavePointPriority()
    {
        return Singleton()._GetSavePointPriority();
    }
    
    int _GetSavePointPriority()
    {
        return currentSavePointPriorityLevel;
    }
    
    
    public void LoadLevel(int level_index)
    {
        if(PhotonNetwork.IsMasterClient == false)
        {
            return;
        }
        
        if(readyToSwitchLevel)
        {
            readyToSwitchLevel = false;
            
            LoadingScreen.SetOn(level_index);
            System.GC.Collect();
            
            if(SceneManager.GetActiveScene().buildIndex == level_index)
            {
                //This means we are reloading current level
            }
            PhotonNetwork.LoadLevel(level_index);
        }
        else
        {
            InGameConsole.LogFancy("<color=yellow>Can't switch level</color>, we are <color=red>not</color> done with previous <color=green>LoadLevel()</color> command.");
        }
    }
    
    // IEnumerator LoadLevelAsync(int level_index)
    // {
        
    //     if(readyToSwitchLevel)
    //     {
    //         readyToSwitchLevel = false;
            
    //         AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(level_index, LoadSceneMode.Additive);
            
            

    //         // Wait until the asynchronous scene fully loads
    //         while (!asyncLoad.isDone)
    //         {
    //             yield return null;
    //         }     
            
    //         //PhotonNetwork.LoadLevel(level_index);
    //     }
    //     else
    //     {
    //         InGameConsole.LogFancy("Can't switch level, we are <color=red>not</color> done with previous <color=green>LoadLevel()</color> command.");
    //     }

       
    // }
    
    
    
    // ObjectPoolKey[] spawn_items = {ObjectPoolKey.Chaser2, ObjectPoolKey.RobotBasicShooter_npc, ObjectPoolKey.RobotBasicShooter_shotgun_npc, ObjectPoolKey.RobotCharger};
    // ObjectPoolKey[] spawn_items = {ObjectPoolKey.Witch, ObjectPoolKey.Shooter2, ObjectPoolKey.Shooter3_shotgunner};
    
    NPCType[] spawn_npcs = {NPCType.Scourge, NPCType.PadlaLong, NPCType.ScourgeCool, NPCType.SniperGirl, NPCType.Sinclaire, NPCType.Padla, NPCType.Stepa, NPCType.CatLady, NPCType.Olios};
    int spawn_index = 0;
    
    void SpawnIndexIncrement()
    {
        spawn_index++;
        if(spawn_index >= spawn_npcs.Length)
        {
            spawn_index = 0;
        }
        
        InGameConsole.LogOrange(string.Format("UberManager: will spawn <color=yellow>{0}</color> on click", spawn_npcs[spawn_index].ToString()));
    }
    
    void DebugSpawnOnClick()
    {
        if(Inputs.GetKeyDown(KeyCode.J))
        {
            SpawnIndexIncrement();
        }
        
        if(Inputs.GetKeyDown(KeyCode.H))
        {
            Ray ray = FollowingCamera.Singleton().cam.ScreenPointToRay(Input.mousePosition);
            
            RaycastHit hit;
            int mask = LayerMask.GetMask("Ground");
            if(Physics.Raycast(ray, out hit, 1000f, mask))
            {
                Vector3 pos = hit.point;
                NavMeshHit navMeshHit;
                if(NavMesh.SamplePosition(pos, out navMeshHit, 0.2F, NavMesh.AllAreas))
                {
                    Vector3 dir = -ray.direction;
                    dir.y = 0;
                    dir.Normalize();
                    
                    NPCType npc_type = spawn_npcs[spawn_index];
                    pos = navMeshHit.position;
                    NetworkObjectsManager.Singleton().SpawnNPC2((byte)(npc_type), pos, dir);
                }
                else
                {
                    InGameConsole.LogOrange("Couldn't spawn <color=green>NPC</color> because couldn't hit NavMesh - <color=yellow>DebugSpawnOnClick()</color>");
                }
            }
        }
    }

    void OnGUI()
    {
        if(!PhotonNetwork.IsMasterClient)
        {
            return;
        }
        
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.alignment = TextAnchor.MiddleCenter;
        // style.alignment = TextAnchor.MiddleCenter;
        
        float rectWidth = 200;
        string restartsString = string.Format("<color=cyan>Restarts:<color=yellow><b>{0}</b></color></color>", this.RestartsOnThisLevel);
        GUI.Label(new Rect((Screen.width-rectWidth)/2, Screen.height - 100, rectWidth, 50), restartsString,style);
        
        string inGameTimerString = string.Format("<color=cyan>IGT:<color=yellow><b>{0}</b></color></color>", this.InGameTimer.ToString("f"));
        GUI.Label(new Rect((Screen.width-rectWidth)/2, Screen.height - 75, rectWidth, 50), inGameTimerString, style);
        
        // string text_currentSavePriority = string.Format("<color=cyan>currentSavePriority:<color=green>{0}</color></color>", this.currentSavePointPriorityLevel);
        // float rectWidth = 200;
        // GUI.Label(new Rect((Screen.width-rectWidth)/2, Screen.height - 72, rectWidth, 50), text_currentSavePriority, style);
        
        string text = string.Format("<color=yellow>Spawn NPC: <color=green>{0}</color></color>", spawn_npcs[spawn_index]);
        rectWidth = 200;
        GUI.Label(new Rect((Screen.width-rectWidth)/2, Screen.height - 52, rectWidth, 50), text, style);
        
    }

    
    public double deltaNetworkTime;
    public double deltaNetworkTimeMs;
    public double prev_photon_time = 0;
    bool first_time_photon_time_check = true;
    
    
    void CheckLocalDeltaTimeAndPhotonTime()
    {
        deltaNetworkTime = PhotonNetwork.Time - prev_photon_time;
        deltaNetworkTimeMs = deltaNetworkTime * 1000d;
        
        if(first_time_photon_time_check)
        {
            first_time_photon_time_check = false;
            return;
        }
        
        //more than a 150ms spike:
        if(deltaNetworkTime > 0.15f && TimeSinceStart() > 5f && deltaNetworkTime < 1f)
        {
            InGameConsole.LogWarning(string.Format("deltaNetworkTime is {0}ms", (deltaNetworkTime * 1000d).ToString("f")));
        }
    }
    
    void ChangeLevel()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        Level currentLevel = (Level)currentSceneIndex;
        InGameConsole.LogOrange(currentLevel.ToString());
        
        // if(Input.GetKey(KeyCode.LeftShift))
        // {
        //     LoadLevel((int)currentLevel);
        // }
        // else
        // {
            
            switch(currentLevel)
            {
                case Level.Lobby:
                    LoadLevel((int)Level.LabyrinthRadial);
                    
                    break;
                case Level.LabyrinthRadial:
                    LoadLevel((int)Level.Level_1);
                    
                    break;
                case Level.Level_1:
                    LoadLevel((int)Level.LabyrinthRadial);
                    
                    break;
                case Level.Level_Flat:
                {
                    LoadLevel((int)Level.Level_Flat);
                    break;
                }
            }
        // }
    }
    
    void HandlePlayerList()
    {
        int len = players.Count;
        for(int i = 0; i < len; i++)            
        {
            if(players[i] == null)
            {
                players.RemoveAt(i);
                players_controller.RemoveAt(i);
            }
        }
    }
    
    public float InGameTimer        = 0;
    public int RestartsOnThisLevel  = 0;
    public InGameTimerState timerState = InGameTimerState.NotCounting;
    
    public static void ResetRestartsCount()
    {
        Singleton().RestartsOnThisLevel = 0;
    }
    
    public static void AddRestartCount()
    {
        Singleton().RestartsOnThisLevel++;
    }
    
    
    public static void ResetInGameTimer()
    {
        ResetRestartsCount();
        Singleton()._ResetInGameTimer();
    }
    
    public static void StartInGameTimer()
    {
        Singleton()._StartInGameTimer();
    }
    
    public static void StopInGameTimer()
    {
        Singleton()._StopInGameTimer();
    }
    
    void _ResetInGameTimer()
    {
        InGameTimer = 0;
        timerState = InGameTimerState.NotCounting;
    }
    
    void _StartInGameTimer()
    {
        timerState = InGameTimerState.Working;
    }
    
    void _StopInGameTimer()
    {
        timerState = InGameTimerState.NotCounting;
    }
    
    void InGameTimerTick(float dt)
    {
        switch(timerState)
        {
            case(InGameTimerState.NotCounting):
            {
            break;
            }
            case(InGameTimerState.Working):
            {
                InGameTimer += dt;
            break;
            }
        }
    }
    
    void Update()
    {
        // HandlePlayerList();
        if(PhotonNetwork.IsMasterClient)
        {
            float dt = UberManager.DeltaTime();
            InGameTimerTick(dt);
        }
        
        isTimeUpdated       = false;
        isDeltaTimeUpdated  = false;
        
        CheckLocalDeltaTimeAndPhotonTime();
        prev_photon_time = PhotonNetwork.Time;
        
        if(PhotonNetwork.IsMasterClient)
        {
            DebugSpawnOnClick();
        }
        
        
        
        // if(Inputs.GetKeyDown(KeyCode.T) && PhotonNetwork.IsMasterClient)
        // {
        //     ChangeLevel();
            
        // }
        
        // if(Input.GetKey(KeyCode.LeftControl) && Input.GetMouseButtonDown(1))
        // {
        //     Ray ray = FollowingCamera.Singleton().cam.ScreenPointToRay(Input.mousePosition);
        //     int mask = LayerMask.GetMask("Ground");
            
        //     RaycastHit hit;
            
        //     if(Physics.Raycast(ray, out hit, 100f, mask))
        //     {
                    
        //         ParticlesManager.PlayPooled(ParticleType.player_spawn_ps, hit.point + Vector3.up * 0.1f, Vector3.forward);
        //         if(PhotonNetwork.IsMasterClient)
        //         {
        //             int net_id = GlobalShooter.Singleton().net_comp.networkId;
        //             NetworkObjectsManager.ScheduleCommand(net_id, GetPhotonTimeDelayedBy(1f), NetworkCommand.GlobalCommand, (int)GlobalShooter_ability.shoot_1_quad, hit.point + Vector3.up * 0.4f);
        //         }
        //     }
        // }
        if(Input.GetKeyDown(KeyCode.K))
        {
            ScreenCapture.CaptureScreenshot(string.Format("C:/Users/Marat/Desktop/SCREENSHOTS/Screenshot_{0}.png", screenshots), 2);
            ++screenshots;
            InGameConsole.Log("<color=blue>ScreenCaptured</color>");
        }
    }
}
