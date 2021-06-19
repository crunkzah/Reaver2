using UnityEngine;
using Photon.Pun;

public class RoomController : MonoBehaviour, INetworkObject
{

    NetworkObject net_comp;
    
    
    
    bool canSendCommands = true;
    
    void LockSendingCommands()
    {
        canSendCommands = false;
    }
    
    
    void UnlockSendingCommands()
    {
        canSendCommands = true;
    }
    
    void Start()
    {
        isSpawning = false;
        wasActivated = false;
        UnlockSendingCommands();
    }
    
    bool wasActivated = false;
    
    
    public ObjectSpawner[] spawners;
    public float[] timings;
    const float spawnDelayPerNPC = 1.0F;
    
    int currentSpawner = 0;
    
    bool isSpawning = false;
    
    void OnAbility1()
    {
        wasActivated = true;
        timeWhenCalled = Time.time;
        
        for(int i = 0; i < timings.Length; i++)
        {
            timings[i] = timeWhenCalled + spawnDelayPerNPC * i;
        }
        
        currentSpawner = 0;
        isSpawning = true;
    }
    
    float timeWhenCalled = float.MaxValue;
    
    public void ReceiveCommand(NetworkCommand command, params object[] args)
    {
        switch(command)
        {
            case(NetworkCommand.Ability1):
            {
                UnlockSendingCommands();
                OnAbility1();
                
                break;
            }
            
            default:
            {
                break;
            }
        }
    }
    
    Bounds bounds;
    BoxCollider col;
    
    void Awake()
    {
        col = GetComponent<BoxCollider>();
        bounds = col.bounds;
        net_comp = GetComponent<NetworkObject>();
        
        timings = new float[spawners.Length];
        
        Destroy(col);
    }
    
    // void Update()
    // {
    //     return;
    //     // return;
    //     if(!PhotonNetwork.IsMasterClient)
    //     {
    //         return;
    //     }
        
        
    //     if(!wasActivated && canSendCommands)
    //     {
    //         // InGameConsole.LogFancy("Trying to check");
    //         int len = NPCManager.Singleton().aiTargets.Count;
            
    //         for(int i = 0; i < len; i++)
    //         {
    //             // InGameConsole.LogFancy("Check if in room " + NPCManager.AITargets()[i].name);
    //             Transform playerTransform = NPCManager.Singleton().aiTargets[i];
                
    //             if(playerTransform && bounds.Contains(playerTransform.localPosition))
    //             {
    //                 LockSendingCommands();
    //                 NetworkObjectsManager.ScheduleCommand(net_comp.networkId, UberManager.GetPhotonTimeDelayedBy(0.15F), NetworkCommand.Ability1);
    //             }
    //         }
    //     }
        
    //     if(isSpawning)
    //     {
    //         if(Time.time > timings[currentSpawner])
    //         {
    //             spawners[currentSpawner].Interact();
    //             currentSpawner++;
    //             if(currentSpawner >= spawners.Length)
    //             {
    //                 isSpawning = false;
    //             }
    //         }
    //     }
    // }
    
}
