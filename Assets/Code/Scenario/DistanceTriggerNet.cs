using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class DistanceTriggerNet : MonoBehaviour, INetworkObject
{
    
    public static List<TriggerTrackable> targets = new List<TriggerTrackable>();
    
    
    
    public GameObject[] objectsToTrigger;
    
    INetworkObject[] triggerables;
    
    public bool triggerOnce = true;
    
    Transform thisTransform;
    
    public float distance = 1f;
    bool canSendCommands = true;
    
    NetworkObject netComponent;
    
    
    void Awake()
    {
        netComponent = GetComponent<NetworkObject>();
        thisTransform = transform;
        
        //note(marat): Not very fancy, but does not matter that much.        
        // if(targets != null && targets.Count > 0)
        // {
        //     targets.Clear();
        // }
    }
    
    
    void Start()
    {
        triggerables = new INetworkObject[objectsToTrigger.Length];
        
        for(int i = 0; i < objectsToTrigger.Length; i++)
        {
            INetworkObject inetworkObject = objectsToTrigger[i].GetComponent<INetworkObject>();
#if UNITY_EDITOR            
            if(inetworkObject == null)
            {
                string msg = string.Format("{0} has no 'ITriggerable' component!", objectsToTrigger[i].gameObject.name);
                InGameConsole.LogError(msg);
            }
#endif
            
            triggerables[i] = inetworkObject;
        }
    }
   
    
    public void ReceiveCommand(NetworkCommand command, params object[] values)
    {
        canSendCommands = true;
        switch(command)
        {
            case(NetworkCommand.DoTrigger):
            {
                DoTrigger();
                break;
            }
            default:
            {
                break;
            }

        }
    }
    
    void DoTrigger()
    {
        for(int i = 0; i < triggerables.Length; i++)
        {
            if(triggerables[i] != null)
                triggerables[i].ReceiveCommand(NetworkCommand.DoTrigger);
        }
        
        if(triggerOnce)
        {
#if DEBUG_BUILD
            GetComponent<Renderer>().material.color = Color.grey;
#endif
            this.enabled = false;
        }
        
    }
    
    public float delay = 1.5f;
    
    void LockSendingCommands()
    {
        canSendCommands = false;
    }
    
    void Update()
    {
        if(PhotonNetwork.IsMasterClient && canSendCommands)
        {
            for(int i = 0; i < targets.Count; i++)
            {   
                if (Math.SqrDistance(targets[i].transform.position, thisTransform.position) < distance * distance)
                {
                    LockSendingCommands();
                    NetworkObjectsManager.ScheduleCommand(netComponent.networkId, (double)delay + PhotonNetwork.Time, NetworkCommand.DoTrigger);
                    // NetworkObjectsManager.CallNetworkFunction(netComponent.networkId, NetworkCommand.DoTrigger);
                }
            }
        }
    }
    
    
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(35f/255f, 148f/255f, 252f/255f);
        Gizmos.DrawWireSphere(transform.position, distance);
    }
    
#endif
    
}
