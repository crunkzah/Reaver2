using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;
using Photon.Realtime;
//using Photon.Pun;
using ExitGames.Client.Photon;

public enum GlobalCommand : byte
{
    Explode_QTS,
    SetDifficulty,
    SetInfernoCircle,
    BounceHit_RevolverBlue,
    AddEnemiesAlive,
    ShowRunStats,
    SetSavePoint,
    LoadLevel,
    SavePointActivated
}

[System.Serializable]
public struct NetObjectPrefab
{
    public SceneNetObject netEnum;
    public GameObject prefab;
}


public class NetworkObjectsManager : MonoBehaviour, IOnEventCallback//MonoBehaviourPunCallbacks
{

    #region  Singleton
    static NetworkObjectsManager instance;
    public static NetworkObjectsManager Singleton()
    {
        if(instance == null)
            instance = FindObjectOfType<NetworkObjectsManager>();
                
        return instance;
    }
    #endregion
    
    PhotonView photonView;

    public NetObjectPrefab[] netObjectsPredefined;
    Dictionary<int, GameObject> prefabs =  new Dictionary<int, GameObject>();
    public Dictionary<int, NetworkObject> runtimePool = new Dictionary<int, NetworkObject>();
    

    
    public int lastDynamicNetId = 4096;
    public int lastStaticNetId = 0;

    public void ResetStaticRegisteredObjects()
    {
        // REMEMBER RUNTIMEPOOL NOT SERIALIZED IN EDITOR !
        //runtimePool.Clear();
        PhotonMessageInfo info;
        
        lastStaticNetId = 0;
    }

    void Awake()
    {
        if(instance != null && instance.GetInstanceID() != this.GetInstanceID())
        {
            // InGameConsole.Log(string.Format("<color=yellow>Current instance is </color><color=red>{0}</color>", instance.gameObject.name));
            InGameConsole.Log("<color=yellow>Destroying excess <color=red>NetworkObjectsManager</color>.</color>");
            Destroy(this.gameObject);
            //this.enabled = false;
            
        }
        ExludeParticularNetworkCommands();
        photonView = GetComponent<PhotonView>();
    }

    void Start()
    {
        for(int i = 0; i < netObjectsPredefined.Length; i++)
        {
            prefabs.Add((int)netObjectsPredefined[i].netEnum, netObjectsPredefined[i].prefab);
        }
    }
    
    void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }   
    
    public void OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;
        
        switch(eventCode)
        {
            case(EventCodes.GiveItem):
            {
                object[] data = (object[])photonEvent.CustomData;

                GunType gunToGive = (GunType)data[0];   
                PlayerInventory.Singleton().GiveWeaponLocally(gunToGive);
                
                break;
            }
            case(EventCodes.Command_Unreliable):
            {
                object[] data = (object[])photonEvent.CustomData; //networkId, Command, args...

                int _net_id = (int)data[0];
                NetworkCommand _command = (NetworkCommand)data[1];
                if(data.Length == 2)
                {
                    CallFuncRPC(_net_id, _command);
                }
                else
                {
                    switch(_command)
                    {
                        case(NetworkCommand.Move):
                        {
                            Vector3 movePos = (Vector3)data[2];
                            
                            CallFuncRPC(_net_id, _command, movePos);
                            
                            break;
                        }
                        default:
                        {
                            break;
                        }
                    }
                }
                
                break;
            }
        }
    }
    

    public struct PackedNetworkCommand 
    {
        public int networkId;
        public NetworkCommand command;
        
        //public object[] args;
        public List<object> args;
    }
    
    [System.Serializable]
    public struct ScheduledNetworkCommand
    {
        public int networkId;
        public NetworkCommand command;
        public double timeWhenExecute;
        public List<object> args;
    }
    
    static List<PackedNetworkCommand> packedCommands = new List<PackedNetworkCommand>();
    
    public float NetworkPackedRPC_timer = 0f;
    static void ExecutePackedCommands()
    {
        if(packedCommands.Count > 0)
        {
            CallNetworkFunctionsMultiple(ref packedCommands);
            //InGameConsole.Log(string.Format("Executing <color=orange>{0}</color><color=cyan> packed</color> commands!", packedCommands.Count));   
            packedCommands.Clear();
        }
    }
    
    
    public static void PackNetworkCommand(int networkId, NetworkCommand command, params object[] args)
    {
        // if(PhotonNetwork.IsMasterClient == false)
        // {
        //     return;
        // }
     
        
        PackedNetworkCommand packedNetworkCommand = new PackedNetworkCommand();
        
        packedNetworkCommand.networkId = networkId;
        packedNetworkCommand.command = command;
        packedNetworkCommand.args = new List<object>(args);
        
        packedCommands.Add(packedNetworkCommand);
        
        // InGameConsole.LogFancy("New packedNetworkCommand: " + packedNetworkCommand.command.ToString());
    }
    
    // public List<ScheduledNetworkCommand> debugCommands;
    public static List<ScheduledNetworkCommand> scheduledCommands = new List<ScheduledNetworkCommand>();
    
    
    public static void ScheduleCommandLocally(int networkId, double timeSessionWhenExecute, NetworkCommand command, params object[] args)
    {
        Singleton().ScheduleCommandRPC(networkId, timeSessionWhenExecute, (byte)command, args);
    }
    
    public static void ScheduleCommand(int networkId, double timeSessionWhenExecute, NetworkCommand command, params object[] args)
    {
        // InGameConsole.Log(string.Format("<color=yellow>Scheduling! {0} seconds to go.</color>", (timeSessionWhenExecute - PhotonNetwork.Time).ToString("f")));
        Singleton().photonView.RPC("ScheduleCommandRPC", RpcTarget.AllViaServer, networkId, timeSessionWhenExecute, (byte)command, args);
    }
    
    [PunRPC]
    public void ScheduleCommandRPC(int networkId, double timeSessionWhenExecute, byte command, params object[] args)
    {
        ScheduledNetworkCommand snc = new ScheduledNetworkCommand();
        snc.networkId = networkId;
        snc.timeWhenExecute = timeSessionWhenExecute; 
        snc.command = (NetworkCommand)command;
        if(args != null)
        {
            snc.args = new List<object>(args);
            
            // for(int i = 0; i < args.Length; i++)
            // {
            //     snc.args.Add(args[i]);
            // }
        }
        
        scheduledCommands.Add(snc);
    }
    
    public void ClearCommands()
    {
        scheduledCommands.Clear();
        packedCommands.Clear();
    }

    void Update()
    {
        
        //@Incomplete : Consider case when NetworkPackedRPC_timer is much greater than ai update rate
        if(NetworkPackedRPC_timer > Globals.AI_update_rate)
        {
            NetworkPackedRPC_timer -= Globals.AI_update_rate;
            ExecutePackedCommands();
            // if(PhotonNetwork.IsMasterClient)
            // {
                // ExecutePackedCommands();
                //InGameConsole.Log("Time to execute packed network commands!");
            // }
        }
        
        NetworkPackedRPC_timer += UberManager.DeltaTime();
        
        // debugCommands = new List<ScheduledNetworkCommand>(scheduledCommands);
        
        int scheduledCommandsCount = scheduledCommands.Count;
        
        for(int i = 0; i < scheduledCommandsCount; i++)
        {
            if(scheduledCommands[i].timeWhenExecute <= PhotonNetwork.Time)
            {
                int networkId = scheduledCommands[i].networkId;
                
                if(!Singleton().runtimePool.ContainsKey(networkId))
                {
                    InGameConsole.LogWarning(string.Format("runtimePool does not contain netId: <color=yellow>{0}</color>. Skipping command.", networkId));
                }
                else
                {
                        
                    NetworkCommand command = scheduledCommands[i].command;
                    
                    NetworkObject netObj = Singleton().runtimePool[networkId];

                    INetworkObject inetworkObject = netObj.GetComponent<INetworkObject>();
                    
                    
                    if(scheduledCommands[i].args != null)
                    {
                        int cnt =  scheduledCommands[i].args.Count;
                        object[] command_args = new object[cnt];
                        for(int j = 0; j < command_args.Length; j++)
                        {
                            command_args[j] =scheduledCommands[i].args[j];
                        }
                        
                        if(inetworkObject == null)
                        {
                            InGameConsole.LogWarning("INetworkObject is null on networkId " + networkId.ToString());
                            InGameConsole.LogWarning("netObj is " + netObj.gameObject.name.ToString());
                            InGameConsole.LogWarning("Command is " + command.ToString());
                        }
                        
                        inetworkObject.ReceiveCommand(command, command_args);
                    }
                    else
                    {
                        inetworkObject.ReceiveCommand(command);
                    }
                    
                    
                    if(inetworkObject == null)
                    {
                        InGameConsole.LogError(string.Format("<color=red>{0} does not have INetworkObject on it !!!</color>", netObj.gameObject.name));
                    }

                    string netId_and_obj_name = string.Format("<color=yellow>{0} ({1}) </color>", networkId.ToString(), netObj.gameObject.name);

                    double scheduledDelay = PhotonNetwork.Time - scheduledCommands[i].timeWhenExecute;
                    int scheduledDelayMs = (int)(scheduledDelay * 1000);

                    InGameConsole.Log(string.Format("<color=orange><b>{0}</b></color><color=red>(Scheduled)</color> was called on <b>{1}</b> after <color=yellow>{2}</color> ms", command.ToString(), netId_and_obj_name, scheduledDelayMs));
                }
                
                scheduledCommands.RemoveAt(i);        
            }
        }
    }

    public static void RegisterNetObject(NetworkObject netComponent)
    {
        // if(netComponent == )
        
        netComponent.networkId = Singleton().lastDynamicNetId;
        if(!Singleton().runtimePool.ContainsKey(Singleton().lastDynamicNetId))
        {
            Singleton().runtimePool.Add(Singleton().lastDynamicNetId, netComponent);
            Debug.Log("<color=orange>RuntimePool</color> added " + netComponent.gameObject.name + " netId: <color=cyan>" + netComponent.networkId + "</color>");
            Singleton().lastDynamicNetId++;
        }
        else
        {

            InGameConsole.Log("runtimePool already contains key: " + Singleton().lastDynamicNetId + " on " + netComponent.gameObject.name);
            Debug.LogError("runtimePool already contains key: " + Singleton().lastDynamicNetId + " on " + netComponent.gameObject.name);
        }
        
        
    }

    

    public static void AssignStaticNetId(NetworkObject netComponent)
    {
        netComponent.networkId = Singleton().lastStaticNetId;
        Singleton().lastStaticNetId++;
    }

    public static void RegisterStaticNetObject(NetworkObject netComponent)
    {
        if(!Singleton().runtimePool.ContainsKey(netComponent.networkId))
        {
            Singleton().runtimePool.Add(netComponent.networkId, netComponent);
            // Debug.Log("<color=orange>RuntimePool (static)</color> added " + netComponent.gameObject.name + " netId: " + netComponent.networkId);
        }
        else
        {
            InGameConsole.Log("runtimePool already contains key: " + netComponent.networkId + " on " + netComponent.gameObject.name);
            Debug.LogError("runtimePool already contains key: " + netComponent.networkId + " on " + netComponent.gameObject.name);
        }
    }

    public static void UnregisterNetObject(NetworkObject netComponent)
    {   
        if(Singleton() != null)
        {
            if(Singleton().runtimePool.ContainsKey(netComponent.networkId))
            {
                Singleton().runtimePool.Remove(netComponent.networkId);
            }
        }
    }

    
    public static void DestroyAllDynamicNetworkObjects()
    {
        Singleton().photonView.RPC("DestroyAllDynamicObjects", RpcTarget.AllViaServer);
    }

    [PunRPC]
    public void DestroyAllDynamicObjects()
    {
        List<GameObject> dynamic_objects_to_destroy = new List<GameObject>();
        List<int> dynamic_objectKeys_to_destroy = new List<int>();

        foreach(KeyValuePair<int, NetworkObject> pair in runtimePool)
        {
            if(pair.Key > 1024)
            {
                dynamic_objects_to_destroy.Add(runtimePool[pair.Key].gameObject);
                dynamic_objectKeys_to_destroy.Add(pair.Key);

            }
        }

        for(int i = 0; i < dynamic_objects_to_destroy.Count; i++)
        {
            runtimePool.Remove(dynamic_objectKeys_to_destroy[i]);
            Debug.Log("<color=orange>Destroying</color> " + dynamic_objects_to_destroy[i].name);
            Destroy(dynamic_objects_to_destroy[i]);


        }
    }

    public static void CreateNetworkObjectsMultiple(int[] names, Vector3[] positions, Quaternion[] rotations, RpcTarget target = RpcTarget.AllViaServer)
    {
        Singleton().photonView.RPC("CreateObjectsMultiple", target, names, positions, rotations);

    }

    [PunRPC]
    void CreateObjectsMultiple(int[] prefabKeys, Vector3[] positions, Quaternion[] rotations)
    {
        if(prefabKeys != null)
        {
            for(int i = 0; i < prefabKeys.Length; i++)
            {
                int prefabKey = prefabKeys[i];
                Vector3 pos = positions[i];
                Quaternion rot = rotations[i];
                
                if (Singleton().prefabs.ContainsKey(prefabKey))
                {
                    Transform newObject = (Instantiate(Singleton().prefabs[prefabKey], pos, rot)).transform;
                    newObject.name = newObject.name.Replace("(Clone)", " (Net)").Trim();

                    InGameConsole.Log(string.Format("Creating <color=orange>{0}</color> at <color=cyan>{1}</color>",
                        ((SceneNetObject)prefabKey).ToString(), pos.ToString()));
                }
                else
                {
                    InGameConsole.LogError("prefabs does not contain item for key " + ((SceneNetObject)prefabKey).ToString());
                }
            }
        }
        else
        {
            Debug.LogError("PrefabKeys are null !!");

        }
    }
    
    public static void DisableObject(int networkId)
    {
        Singleton().photonView.RPC("DisableNetObject", RpcTarget.AllViaServer, networkId);
    }
    
    [PunRPC]
    public void DisableNetObject(int networkId)
    {
        if(Singleton().runtimePool.ContainsKey(networkId))
        {
            NetworkObject netObj = Singleton().runtimePool[networkId];
            
            netObj.networkId = -1;
            netObj.transform.position = new Vector3(0, 2000f, 0f);
            netObj.gameObject.SetActive(false);
        }
        else
        {
            InGameConsole.LogWarning("<color=green> DisableNetObject()</color>runtimePool doesn't contain " + networkId + " key");
        }
    }

    public static void CreateNetworkObject(SceneNetObject name, Vector3 pos, Quaternion rot, RpcTarget target = RpcTarget.AllViaServer)
    {
        if(PhotonNetwork.IsMasterClient == false)
        {
            return;
        }
        
        int nameAsKey = (int)name;
        if(Singleton().prefabs.ContainsKey(nameAsKey))
        {
            Singleton().photonView.RPC("CreateObject", target, nameAsKey, pos, rot);
        }
        else
        {
            Debug.LogError("No such prefab " + name.ToString() + " in prefabs.");
        }
    }

    [PunRPC]
    public void CreateObject(int prefabKey, Vector3 pos, Quaternion rot)
    {
        if(Singleton().prefabs.ContainsKey(prefabKey))
        {
            Transform newObject = (Instantiate(Singleton().prefabs[prefabKey], pos, rot)).transform;
            newObject.name = newObject.name.Replace("(Clone)", " (Net)").Trim();

            // InGameConsole.Log("<color=red>InteractNet</color> called on <color=cyan>" + ((SceneNetObject)prefabKey).ToString() + "</color>");
            InGameConsole.Log(string.Format("Creating <color=orange>{0}</color> at <color=cyan>{1}</color>",
                ((SceneNetObject)prefabKey).ToString(), pos.ToString()));
        }
        else
            Debug.LogError("prefabs does not contain item for key " + ((SceneNetObject)prefabKey).ToString());
    }

    public static void InteractWithNetObject(GameObject obj)
    {
        Singleton().photonView.RPC("InteractNet", RpcTarget.AllViaServer, obj.GetComponent<NetworkObject>().networkId);
    }

    [PunRPC]
    void InteractNet(int networkId)
    {
        if(Singleton().runtimePool.ContainsKey(networkId))
        {
            NetworkObject netObj = Singleton().runtimePool[networkId];
            
            Interactable interactable = netObj.GetComponent<Interactable>();
            if(interactable != null)
            {
                interactable.Interact();
            }
            else
            {
                netObj.SendMessage("Interact", SendMessageOptions.DontRequireReceiver);
            }
            

            InGameConsole.Log(string.Format("<color=orange>InteractNet</color> was called on <color=cyan><b>{0}</b></color> ({1})", networkId.ToString(), netObj.gameObject.name));
        }
        else
        {
            InGameConsole.LogError("runtimePool doesn't contain " + networkId + " key");
        }
    }
    
    
    
    public static void Activate(int networkId, RpcTarget rpcTarget = RpcTarget.AllViaServer)
    {
        Singleton().photonView.RPC("ActivateNet", rpcTarget, networkId);
    }
    
    [PunRPC]
    public void ActivateNet(int networkId)
    {
        if(Singleton().runtimePool.ContainsKey(networkId))
        {
            NetworkObject netObj = Singleton().runtimePool[networkId];
            IActivatable activatable = netObj.GetComponent<IActivatable>();
            
            InGameConsole.LogFancy(string.Format("<color=green>Activated</color> object with netId {0}", networkId));
            
            activatable.Activate();
        }
        else
        {
            InGameConsole.LogError("<color=yellow>ActivateNet</color> runtimePool doesn't contain " + networkId + " key");
        }
    }
    
    public static void Deactivate(int networkId, RpcTarget rpcTarget = RpcTarget.AllViaServer)
    {
        Singleton().photonView.RPC("DeactivateNet", rpcTarget, networkId);
    }
    
    [PunRPC]
    public void DeactivateNet(int networkId)
    {
        if(Singleton().runtimePool.ContainsKey(networkId))
        {
            NetworkObject netObj = Singleton().runtimePool[networkId];
            IActivatable activatable = netObj.GetComponent<IActivatable>();
            
            InGameConsole.LogFancy(string.Format("<color=yellow>Dectivated</color> object with netId {0}", networkId));
            
            activatable.Deactivate();
        }
        else
        {
            InGameConsole.LogError("<color=yellow>DeactivateNet</color> runtimePool doesn't contain " + networkId + " key");
        }
    }
    
    public static void DestroyNetObject(GameObject obj)
    {
        Singleton().photonView.RPC("DestroyObject", RpcTarget.AllViaServer, obj.GetComponent<NetworkObject>().networkId);
    }

    [PunRPC]
    public void DestroyObject(int networkId)
    {
        if(Singleton().runtimePool.ContainsKey(networkId))
        {
            NetworkObject netObj = Singleton().runtimePool[networkId];
            Destroy(netObj.gameObject);

            InGameConsole.Log(string.Format("<color=orange>DestroyObject</color> was called on <color=cyan><b>{0}</b> ({1})</color>", networkId.ToString(), netObj.gameObject.name));
        }
        else
            InGameConsole.LogError("runtimePool doesn't contain " + networkId + " key");
    }
    
    static void CallNetworkFunctionsMultiple(ref List<PackedNetworkCommand> cmds)
    {
        int commands_count = cmds.Count;
            
        int[] networkIds = new int[commands_count];
        int[] commands = new int[commands_count];
        object[][] commands_args = new object[commands_count][];
        
        for(int i = 0; i < commands_count; i++)
        {
            networkIds[i]   = cmds[i].networkId;
            commands[i]     = (byte)cmds[i].command;
            
            commands_args[i] = cmds[i].args.ToArray();
            
        }
        
        Singleton().photonView.RPC("CallFunctionsMultiple", RpcTarget.AllViaServer, networkIds, commands, PhotonNetwork.Time, commands_args);
    }
    
    
    [PunRPC]
    void CallFunctionsMultiple(int[] networkIds, int[] commands, double timeStamp, params object[][] args)
    {
        for(int i = 0; i < networkIds.Length; i++)
        {
            int networkId = networkIds[i];
            NetworkCommand command = (NetworkCommand)commands[i];
            object[] values = args[i];
            
            if(Singleton().runtimePool.ContainsKey(networkId))
            {
                NetworkObject netObj = Singleton().runtimePool[networkId];

                INetworkObject inetworkObject = netObj.GetComponent<INetworkObject>();
                
                // if(inetworkObject != null)
                // {
                //     InGameConsole.Log(string.Format("<color=red>{0} does not have INetworkObject on it !!!</color>", netObj.gameObject.name));
                // }
                
                
                
                // if(values != null)
                // {
                if(inetworkObject == null)
                {
                    InGameConsole.LogError(string.Format("INetwork object is null ! on <color=yellow>{0}</color>. Command is  <color=yellow>{1}</color> netId: <color=cyan>{2}</color>", netObj.gameObject.name, command.ToString(), netObj.networkId));
                }
                    
                inetworkObject.ReceiveCommand(command, values);
                // }
                // else
                //     inetworkObject.ReceiveCommand(command);
                // if(true || command != NetworkCommand.Move)
                // {
                    string netId_and_obj_name = string.Format("<color=cyan>{0} ({1}) </color>", networkId.ToString(), netObj.gameObject.name);

                    InGameConsole.Log(string.Format("<color=orange><b>{0}</b></color> was called on <b>{1}</b>", command.ToString(), netId_and_obj_name));
                // }
            }
            else
            {
                InGameConsole.LogWarning("<color=yellow>CallFunctionsMultiple():</color> runtimePool doesn't contain " + networkId + " key. Skipping command.");
            }
        }
    }
    
    public static int SpawnNewItem(ObjectPoolKey key, Vector3 pos, Vector3 velocity)
    {
        if(!PhotonNetwork.IsMasterClient)
        {
            InGameConsole.LogError(string.Format("<color=yellow>SpawnNewItem():</color> Non-master call {0}", key));
        }
        
        int newDynamicNetId = Singleton().lastDynamicNetId + 1;
        
        // InGameConsole.LogFancy("newDynamicNetId: " + newDynamicNetId);
        
        Singleton().lastDynamicNetId = newDynamicNetId;
        int netId = Singleton().lastDynamicNetId;
        Singleton().photonView.RPC("SpawnItem", RpcTarget.AllViaServer, (int)key, newDynamicNetId, pos, velocity);
        
        //InGameConsole.LogOrange(string.Format("Spawning <color=red>LOCAL</color> <color=green>{0}</color> with netId: <color=cyan>{1}</color>"));
        
        return newDynamicNetId;
    }
    
    
    public static void AskMasterToSpawnNewItem(ObjectPoolKey key, Vector3 pos, Vector3 velocity)
    {
        Singleton().photonView.RPC("SpawnNewItemFromMasterRPC", RpcTarget.MasterClient, key, pos, velocity);
    }
    
    [PunRPC]
    public void SpawnNewItemFromMasterRPC(ObjectPoolKey key, Vector3 pos, Vector3 velocity)
    {
        SpawnNewItem(key, pos, velocity);
    }
    
    [PunRPC]
    public void SpawnItem(int objectKey, int netId, Vector3 pos, Vector3 velocity)
    {
        ObjectPoolKey poolKey = (ObjectPoolKey)objectKey;
        
        GameObject obj = ObjectPool.s().Get(poolKey);
        
        NetworkObject net_comp = obj.GetComponent<NetworkObject>();
        
        if(runtimePool.ContainsKey(netId))
        {
            NetworkObject collisionNet = runtimePool[netId];
            InGameConsole.LogWarning(string.Format("RuntimePool contains key {0} {1}", netId, collisionNet.gameObject.name));
        }
        
        if(net_comp)
        {
            if(runtimePool.ContainsKey(net_comp.networkId))
            {
                runtimePool.Remove(net_comp.networkId);
            }
            net_comp.networkId = netId;
            runtimePool.Add(netId, net_comp);
            
            obj.transform.SetPositionAndRotation(pos, Quaternion.identity);
            
            DroppedItem droppedItem = obj.GetComponent<DroppedItem>();
            
            if(droppedItem)
            {
                droppedItem.LaunchItem(velocity);
            }
            else
            {
                InGameConsole.LogWarning(string.Format("DroppedItem not found on {0}", net_comp.name));
            }
            
            obj.SetActive(true);
            
            InGameConsole.LogFancy(string.Format("Spawning item <color=green>{0}</color> with netId: <color=cyan>{1}</color>", obj.name, netId));
        }
    }
    
    public static int SpawnNewObject(ObjectPoolKey key, Vector3 pos, Vector3 dir)
    {
        if(!PhotonNetwork.IsMasterClient)
        {
            InGameConsole.LogError(string.Format("<color=yellow>SpawnNewObject():</color> Non-master call {0}", key));
        }
        
        int newDynamicNetId = Singleton().lastDynamicNetId + 1;
        
        Singleton().lastDynamicNetId = newDynamicNetId;
        int netId = Singleton().lastDynamicNetId;
        Singleton().photonView.RPC("SpawnObject", RpcTarget.AllViaServer, (int)key, netId, pos, dir);
        
        //InGameConsole.LogOrange(string.Format("Spawning <color=red>LOCAL</color> <color=green>{0}</color> with netId: <color=cyan>{1}</color>"));
        
        return newDynamicNetId;
    }
    
    [PunRPC]
    public void SpawnObject(int objectKey, int netId, Vector3 pos, Vector3 dir)
    {
        ObjectPoolKey poolKey = (ObjectPoolKey)objectKey;
        
        GameObject obj = ObjectPool.s().Get(poolKey);
        
        NetworkObject netComp = obj.GetComponent<NetworkObject>();
        
        if(netComp)
        {
            if(runtimePool.ContainsKey(netComp.networkId))
            {
                runtimePool.Remove(netComp.networkId);
            }
            netComp.networkId = netId;
            runtimePool.Add(netId, netComp);
            
            obj.transform.SetPositionAndRotation(pos, Quaternion.identity);
            ParticlesManager.PlayPooled(ParticleType.npc_spawned_1_ps, pos, Vector3.forward);
            LightOnSpawn(pos);
            AudioManager.PlayClip(SoundType.spawn_npc_1_sound, 0.5f, 1);
            
            ISpawnable ispawnable = obj.GetComponent<ISpawnable>();
            if(ispawnable != null)
            {
                ispawnable.SetSpawnPosition(pos);
            }
            
            if(PhotonNetwork.IsMasterClient)
            {
                IRemoteAgent iremoteAgent = obj.GetComponent<IRemoteAgent>();
                if(iremoteAgent != null)
                {
                    iremoteAgent.RemoteAgentOnSpawn(pos);
                }
            }
            
            obj.SetActive(true);
            
            InGameConsole.LogFancy(string.Format("Spawning <color=green>{0}</color> with netId: <color=cyan>{1}</color>", obj.name, netId));
        }
    }

    public static void CallNetworkFunction(int networkId, NetworkCommand command, params object[] values)
    {
        if(networkId != -1)
        {
            Singleton().photonView.RPC("CallFuncRPC", RpcTarget.AllViaServer, networkId, command, values);
        }
        else
        {
            Debug.LogError("Trying to call network function on entity with -1 netId !");
        }
    }
    
    public static void CallNetworkFunctionUnreliable(int networkId, NetworkCommand command, params object[] values)
    {
        if(networkId != -1)
        {
            if(PhotonNetwork.IsMasterClient)
            {
                switch(command)
                {
                    case(NetworkCommand.Move):
                    {
                        object[] content = new object[] { (int)networkId, (byte)command, (Vector3)values[0] }; // Array contains the target position and the IDs of the selected units
                        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All }; // You would have to set the Receivers to All in order to receive this event on the local client as well
                        PhotonNetwork.RaiseEvent(EventCodes.Command_Unreliable, content, raiseEventOptions, SendOptions.SendUnreliable);
                        break;
                    }
                    default:
                    {
                        break;
                    }
                }
                //Singleton().photonView.RPC("CallFuncRPC", RpcTarget.AllViaServer, networkId, command, values);
            }
        }
        else
        {
            Debug.LogError("Trying to call network function unreliably on entity with -1 netId !");
        }
    }
    
    [PunRPC]
    void CallFuncRPCUnreliable(int networkId, NetworkCommand command, params object[] values)
    {
        if(Singleton().runtimePool.ContainsKey(networkId))
        {
            NetworkObject netObj = Singleton().runtimePool[networkId];

            INetworkObject inetworkObject = netObj.GetComponent<INetworkObject>();
            inetworkObject.ReceiveCommand(command, values);
            
            if(!commands_to_exclude_from_log.Contains(command))
            {
                string netId_and_obj_name = string.Format("<color=#03e3fc>{0} ({1}) </color>", networkId.ToString(), netObj.gameObject.name);
                InGameConsole.Log(string.Format("<color=orange><b>{0}</b></color> was called on <b>{1}</b>", command.ToString(), netId_and_obj_name));
            }
        }
        else
        {
            //InGameConsole.LogWarning("runtimePool doesn't contain " + networkId + " key; Command: " + command);
        }
    }
    
    [Header("NPC Originals:")]
    public GameObject Sinclaire_npc;
    public GameObject Padla_npc;
    public GameObject Stepa_npc;
    //public GameObject SniperGirl_npc;
   // public GameObject CatLady_npc;
    public GameObject Olios_npc;
    public GameObject PadlaLong_npc;
    public GameObject Scourge_npc;
    public GameObject ScourgeCool_npc;
    public GameObject SinclaireCool_npc;
    
    
    public void SpawnNPC2(byte _npc, Vector3 pos, Vector3 forward, int playerId = -1)
    {
        if(!PhotonNetwork.IsMasterClient)
        {
            return;
        }
        
        lastDynamicNetId++;
        int netId = lastDynamicNetId;
        
        if(playerId != -1)
        {
            
        }
        else
        {
            photonView.RPC("_SpawnNPC2", RpcTarget.AllViaServer, _npc, pos, forward, netId);
        }
    }
    
    public void SpawnNPC_Spawner(byte _npc, Vector3 pos, Vector3 forward, int spawnerID)
    {
        if(!PhotonNetwork.IsMasterClient)
        {
            return;
        }
        
        lastDynamicNetId++;
        int netId = lastDynamicNetId;
        
       
        photonView.RPC("_SpawnNPC_Spawner", RpcTarget.AllViaServer, _npc, pos, forward, netId, spawnerID);
    }
    
    [PunRPC]
    public void _SpawnNPC_Spawner(byte _npc, Vector3 pos, Vector3 forward, int net_id, int spawnerID)
    {
        NPCType npc = (NPCType)_npc;
        GameObject npc_original = null;
        
        SoundType spawn_soundType = SoundType.spawn_npc_1_sound;
        
        switch(npc)
        {
            case(NPCType.Sinclaire):
            {
                npc_original = Sinclaire_npc;
                spawn_soundType = SoundType.spawn_npc_2_sound;
                break;
            }
            case(NPCType.Padla):
            {
                npc_original = Padla_npc;
                break;
            }
            case(NPCType.Stepa):
            {
                npc_original = Stepa_npc;
                break;
            }
            case(NPCType.SniperGirl):
            {
                //npc_original = SniperGirl_npc;
                break;
            }
            case(NPCType.CatLady):
            {
                //npc_original = CatLady_npc;
                break;
            }
            case(NPCType.Olios):
            {
                npc_original = Olios_npc;
                spawn_soundType = SoundType.spawn_npc_2_sound;
                break;
            }
            case(NPCType.PadlaLong):
            {
                npc_original = PadlaLong_npc;
                spawn_soundType = SoundType.spawn_npc_2_sound;
                break;
            }
            case(NPCType.Scourge):
            {
                npc_original = Scourge_npc;
                break;
            }
            case(NPCType.ScourgeCool):
            {
                npc_original = ScourgeCool_npc;
                spawn_soundType = SoundType.spawn_npc_2_sound;
                break;
            }
            case(NPCType.SinclaireCool):
            {
                npc_original = SinclaireCool_npc;
                spawn_soundType = SoundType.None;
                break;
            }
            default:
            {
                break;
            }
        }
        NetworkObjectsManager netManager = NetworkObjectsManager.Singleton();
        
        Quaternion rot = Quaternion.LookRotation(forward, Vector3.up);
        
        GameObject spawned_npc = Instantiate(npc_original, pos, rot);
        
        SpawnedObject spawnedObject_comp = spawned_npc.GetComponent<SpawnedObject>();
        spawnedObject_comp.SetEnemySpawnerID(spawnerID);
        
        NetworkObject net_comp = spawned_npc.GetComponentInChildren<NetworkObject>();
        
        LightOnSpawn(pos);
        float spawn_pitch = Random.Range(0.9f, 1.05f);
        if(spawn_soundType != SoundType.None)
        {
            ParticlesManager.PlayPooled(ParticleType.npc_spawned_1_ps, pos, forward);
            AudioManager.Play3D(spawn_soundType, pos, spawn_pitch, 1f);
        }
        
        if(net_comp)
        {
            //InGameConsole.LogWarning(string.Format("
            net_comp.networkId = net_id;
            
            if(!netManager.runtimePool.ContainsKey(net_id))
            {
                netManager.runtimePool.Add(net_id, net_comp);
            }
            else
            {
                NetworkObject net_comp_collision = netManager.runtimePool[net_id];
                InGameConsole.LogWarning(string.Format("NetworkObjectsManager already contains <color=cyan>{0}</color> key. Object: <color=cyan>{1}</color>", net_id, net_comp_collision.gameObject.name));
            }
        }
        
        //Singleton().photonView.RPC("SpawnObject", RpcTarget.AllViaServer, (int)key, netId, pos, dir);
    }
    
    public void LightOnSpawn(Vector3 pos)
    {
        pos.y += 0.1f;
        GameObject light_obj = ObjectPool2.s().Get(ObjectPoolKey.LightPooled, false);
        LightPooled light =  light_obj.GetComponent<LightPooled>();
        Color color = new Color(0.9f, 0.2f, 0.6f);
        
        light.DoLight(pos, color, 1, 6, 5, 6);
    }
    
    [PunRPC]
    public void _SpawnNPC2(byte _npc, Vector3 pos, Vector3 forward, int net_id)
    {
        NPCType npc = (NPCType)_npc;
        GameObject npc_original = null;
        
        switch(npc)
        {
            case(NPCType.Sinclaire):
            {
                npc_original = Sinclaire_npc;
                break;
            }
            case(NPCType.Padla):
            {
                npc_original = Padla_npc;
                break;
            }
            case(NPCType.Stepa):
            {
                npc_original = Stepa_npc;
                break;
            }
            case(NPCType.SniperGirl):
            {
                //npc_original = SniperGirl_npc;
                break;
            }
            case(NPCType.CatLady):
            {
                //npc_original = CatLady_npc;
                break;
            }
            case(NPCType.Olios):
            {
                npc_original = Olios_npc;
                pos.y += 3F;
                break;
            }
            case(NPCType.PadlaLong):
            {
                npc_original = PadlaLong_npc;
                break;
            }
            case(NPCType.Scourge):
            {
                npc_original =Scourge_npc;
                break;
            }
            case(NPCType.ScourgeCool):
            {
                npc_original = ScourgeCool_npc;
                break;
            }
            case(NPCType.SinclaireCool):
            {
                npc_original = SinclaireCool_npc;
                break;
            }
            default:
            {
                break;
            }
        }
        NetworkObjectsManager netManager = NetworkObjectsManager.Singleton();
        
        Quaternion rot = Quaternion.LookRotation(forward, Vector3.up);
        
        GameObject spawned_npc = Instantiate(npc_original, pos, rot);
        NetworkObject net_comp = spawned_npc.GetComponentInChildren<NetworkObject>();
        
        ParticlesManager.PlayPooled(ParticleType.npc_spawned_1_ps, pos, forward);
        LightOnSpawn(pos);
        
        
        AudioManager.Play3D(SoundType.spawn_npc_1_sound, pos, 2, 0.25f);
        
        if(net_comp)
        {
            net_comp.networkId = net_id;
            
            if(!netManager.runtimePool.ContainsKey(net_id))
            {
                netManager.runtimePool.Add(net_id, net_comp);
            }
            else
            {
                NetworkObject net_comp_collision = netManager.runtimePool[net_id];
                InGameConsole.LogWarning(string.Format("NetworkObjectsManager already contains <color=cyan>{0}</color> key. Object: <color=cyan>{1}</color>", net_id, net_comp_collision.gameObject.name));
            }
        }
        
        //Singleton().photonView.RPC("SpawnObject", RpcTarget.AllViaServer, (int)key, netId, pos, dir);
    }
    
    
    HashSet<NetworkCommand> commands_to_exclude_from_log = new HashSet<NetworkCommand>();
    
    void ExludeParticularNetworkCommands()
    {
        commands_to_exclude_from_log.Add(NetworkCommand.Move);
        commands_to_exclude_from_log.Add(NetworkCommand.Shoot);
        commands_to_exclude_from_log.Add(NetworkCommand.Attack);
        commands_to_exclude_from_log.Add(NetworkCommand.Flee);
        commands_to_exclude_from_log.Add(NetworkCommand.TakeDamageLimbWithForce);
        commands_to_exclude_from_log.Add(NetworkCommand.TakeDamageLimbNoForce);
        commands_to_exclude_from_log.Add(NetworkCommand.TakeDamageExplosive);
        commands_to_exclude_from_log.Add(NetworkCommand.SetTarget);
        
        commands_to_exclude_from_log.Add(NetworkCommand.SetState);
        commands_to_exclude_from_log.Add(NetworkCommand.LaunchAirborne);
        commands_to_exclude_from_log.Add(NetworkCommand.LaunchAirborneUp);
        
        //commands_to_exclude_from_log.Add(NetworkCommand.Ability1);
       // commands_to_exclude_from_log.Add(NetworkCommand.Ability2);
        //commands_to_exclude_from_log.Add(NetworkCommand.Ability3);
        
        //commands_to_exclude_from_log.Add(NetworkCommand.OpenGates);
        //commands_to_exclude_from_log.Add(NetworkCommand.LockGates);
    }

    [PunRPC]
    void CallFuncRPC(int networkId, NetworkCommand command, params object[] values)
    {
        if(Singleton().runtimePool.ContainsKey(networkId))
        {
            NetworkObject netObj = Singleton().runtimePool[networkId];

            INetworkObject inetworkObject = netObj.GetComponent<INetworkObject>();
            inetworkObject.ReceiveCommand(command, values);
            
            if(!commands_to_exclude_from_log.Contains(command))
            {
                string netId_and_obj_name = string.Format("<color=#03e3fc>{0} ({1}) </color>", networkId.ToString(), netObj.gameObject.name);
                InGameConsole.Log(string.Format("<color=orange><b>{0}</b></color> was called on <b>{1}</b>", command.ToString(), netId_and_obj_name));
            }
        }
        else
        {
            //InGameConsole.LogWarning("runtimePool doesn't contain " + networkId + " key; Command: " + command);
        }
    }
    
    public static void CallGlobalCommand(GlobalCommand command, RpcTarget rpcTarget, params object[] args)
    {
        //PhotonMessageInfo info;
        
        byte command_byte = (byte)command;
        Singleton().photonView.RPC(nameof(GC_RPC), rpcTarget, command_byte, args);
    }
    
    [PunRPC]
    public void GC_RPC(byte global_command_byte, params object[] args)
    {
        GlobalCommand global_command  = (GlobalCommand)global_command_byte;
        
        switch(global_command)
        {
            case(GlobalCommand.Explode_QTS):
            {
                GameObject obj = ObjectPool.s().Get(ObjectPoolKey.Kaboom1, false);
                float explosionRadius = 6;
                float explosionForce = 40;
                int explosionDamage = 1200;
                bool isMine = false;
                
                obj.GetComponent<Kaboom1>().ExplodeDamageHostile(transform.localPosition, explosionRadius, explosionForce, explosionDamage, isMine, false, 0);
                
                break;
            }
            case(GlobalCommand.ShowRunStats):
            {
                int restarts = (int)args[0];
                float time = (float)args[1];
                int diff = (int)args[2];
                
                UberManager.Singleton().InGameTimer = time;
                UberManager.Singleton().RestartsOnThisLevel = restarts;
                UberManager.Singleton().difficulty = diff;
                
                GameStats.SetStats(restarts, time, diff);
                GameStats.Show();
                
                
                
                break;
            }
            case(GlobalCommand.SetDifficulty):
            {
                int _difficulty = (int)args[0];
                UberManager.Singleton().difficulty = _difficulty;
                
                break;
            }
            case(GlobalCommand.SetInfernoCircle):
            {
                int _infernoCircle = (int)args[0];
                UberManager.Singleton().infernoCircle = _infernoCircle;
                break;
            }
            case(GlobalCommand.BounceHit_RevolverBlue):
            {
                Vector3 startPos    = (Vector3)args[0];
                Vector3 endPos      = (Vector3)args[1];
                
                
                // if(Physics.Raycast(ray, out hit, revolverShotMaxDistance, bulletMask))
                // {
                //     lineEnd = hit.point;
                //     OnHitScanBounce(hit.point, hitScanDirection, hit.normal, revolverDmg * 2, hit.collider, null, 2.5f, 2, 1650);
                // }
                // else
                // {
                //     OnHitScan(shotPos + hitScanDirection * revolverShotMaxDistance, hitScanDirection, -hitScanDirection, revolverDmg, null);
                // }
                //GameObject bulletFX = ObjectPool.s().Get(ObjectPoolKey.RevolverBullet);
                GameObject bulletFX = ObjectPool.s().Get(ObjectPoolKey.Revolver_bullet_ult);
                bulletFX.GetComponent<BulletControllerHurtless>().Launch2(startPos, endPos);
                
                break;
            }
            case(GlobalCommand.AddEnemiesAlive):
            {
                int enemiesToAdd = (int)args[0];
                AudioManager.AddEnemiesAlive(enemiesToAdd);
                break;
            }
            case(GlobalCommand.SetSavePoint):
            {
                int savePoint = (int)args[0];
                UberManager.SetSavePointPriority(savePoint);
                //AudioManager.AddEnemiesAlive(enemiesToAdd);
                break;
            }
            case(GlobalCommand.LoadLevel):
            {
                int levelToLoad = (int)args[0];
                
                PhotonNetwork.LoadLevel(levelToLoad);
                break;
            }
            case(GlobalCommand.SavePointActivated):
            {
                // if(PhotonNetwork.IsMasterClient)
                // {
                //     ref List<PlayerController> players = ref UberManager.Singleton().playerControllers;
                    
                //     for(int i = 0; i < players.Count; i++)
                //     {
                //         if(players[i] && !players[i].isAlive)
                //         {
                //             players[i].pv.RPC("ReviveWithPenalty", RpcTarget.)
                //         }
                //     }
                // }
                
                PlayerController myPlayer = PhotonManager.GetLocalPlayer();
                if(myPlayer)
                {
                    if(!myPlayer.isAlive)
                    {
                        myPlayer.ReviveWithPenalty(0.25f);
                    }
                }
                
                break;
            }
            default:
            {
                break;
            }
        }
    }
}
