using UnityEngine;
using Photon.Pun;

[System.Serializable]
public struct ObjectAndCommand
{
    public NetworkObject net_comp;
    public NetworkCommand commandToSend;
}

public class TriggerBox : MonoBehaviour, INetworkObject
{
    public ObjectAndCommand[] targets;
    public GameObject[] interactables_to_call;
    
    public bool triggerOnce = true;
    public double triggerDelay = 0.15d;
    
    // public float progressiveDelay = 1.3f;
    // public bool addProgressiveDelay = true;
    
    NetworkObject net_comp;
    bool canTrigger = true;
    
    bool canSendCommands = true;
    
    
    void Awake()
    {
        BoxCollider box = GetComponent<BoxCollider>();
        bounds = box.bounds;
        
        Destroy(box);
        
        net_comp = GetComponent<NetworkObject>();
    }
    
    
    public void ReceiveCommand(NetworkCommand cmd, params object[] args)
    {
        switch(cmd)
        {
            case NetworkCommand.DoTrigger:
            {
                UnlockSendingCommands();
                DoTrigger();
                
                break;
            }
        }
    }
    
    bool CanTrigger()
    {
        
        return canTrigger && PhotonNetwork.IsMasterClient;
    }
    
    void LockSendingCommands()
    {
        canSendCommands = false;
    }
    
    void UnlockSendingCommands()
    {
        canSendCommands = true;
    }
    
    public void DoTrigger()
    {
        if(!PhotonNetwork.IsMasterClient)
        {
            return;
        }
        
        if(canTrigger)
        {
            if(triggerOnce)
            {
                canTrigger = false;
            }
            
            if(targets != null && targets.Length > 0)
            {
                int len = targets.Length;
                
                for(int i = 0; i < len; i++)
                {
                    int net_id = targets[i].net_comp.networkId;
                    NetworkCommand cmd = targets[i].commandToSend;
                    double timeToExecute = triggerDelay + PhotonNetwork.Time;
                    
                    NetworkObjectsManager.ScheduleCommand(net_id, timeToExecute, cmd);
                }
            }
            
            if(interactables_to_call != null && interactables_to_call.Length > 0)
            {
                int len = interactables_to_call.Length;
                
                for(int i = 0; i < len; i++)
                {
                    interactables_to_call[i].GetComponent<Interactable>().Interact();
                }
            }
        }
    }
    
    Bounds bounds;
    public LayerMask layerMask; 
    
    void Update()
    {
        if(canTrigger && PhotonNetwork.IsMasterClient && canSendCommands)
        {
        
            if(Physics.CheckBox(bounds.center, bounds.extents, Quaternion.identity, layerMask))
            {
                LockSendingCommands();
                NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.DoTrigger);
            }
        }
    }
    
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if(bounds == null)
        {
            bounds = GetComponent<BoxCollider>().bounds;
        }
        
        //Gizmos.color = CanTrigger() ? Color.cyan : Color.red;
        Gizmos.color = Color.cyan;
        
        
        Gizmos.DrawWireCube(bounds.center, bounds.size);
    }  
#endif
}
