using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class DistanceTrigger : MonoBehaviour, INetworkObject
{
    
    public static List<TriggerTrackable> targets = new List<TriggerTrackable>();
    
    public GameObject[] objectsToTrigger;
    
    ITriggerable[] triggerables;
    
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
        triggerables = new ITriggerable[objectsToTrigger.Length];
        
        for(int i = 0; i < objectsToTrigger.Length; i++)
        {
            ITriggerable itriggerable = objectsToTrigger[i].GetComponent<ITriggerable>();
#if UNITY_EDITOR            
            if(itriggerable == null)
            {
                string msg = string.Format("{0} has no 'ITriggerable' component!", objectsToTrigger[i].gameObject.name);
                InGameConsole.LogError(msg);
            }
#endif
            
            triggerables[i] = itriggerable;
        }
    }
   
    
    public void ReceiveCommand(NetworkCommand command, params object[] values)
    {
        // canSendCommands = true;
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
            triggerables[i].OnTrigger();
        }
        
        if(triggerOnce)
        {
#if DEBUG_BUILD
            GetComponent<Renderer>().material.color = Color.grey;
#endif
            this.enabled = false;
        }
        
    }
    
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
                if ((targets[i].transform.position - thisTransform.position).sqrMagnitude < distance * distance)
                {
                    LockSendingCommands();
                    NetworkObjectsManager.CallNetworkFunction(netComponent.networkId, NetworkCommand.DoTrigger);
                }
            }
        }
    }
    
    
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(3f/255f, 198f/255f, 252f/255f);
        Gizmos.DrawWireSphere(transform.position, distance);
    }
    
#endif
    
}
