using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;
using TMPro;

public enum KikiState : byte
{
    Sitting,
}

public class KikiController : MonoBehaviour, INetworkObject
{
    
    NetworkObject net_comp;
    public KikiState state;
    
    public void ReceiveCommand(NetworkCommand command, params object[] args)
    {
        switch(command)
        {
            case(NetworkCommand.Ability1):
            {
                
                break;
            }
            default:
            {
                break;    
            }
        }
    }
    public TextMeshProUGUI tmp;
    
    void Start()
    {
        
    }
    void UpdateBrainLocally(float dt)
    {
        
    }
    
    TrackTo head_tracking;
    public Transform head;
    Transform thisTransform;
    
    void Awake()
    {
        thisTransform = transform;
        net_comp = GetComponent<NetworkObject>();
        head_tracking = head.GetComponent<TrackTo>();
    }
    
    float maxDotValue = 0.45f;
    public float currentDot = 0;
    
    
    public float dot;
    
    void Update()
    {
        PlayerController local_pc = PhotonManager.GetLocalPlayer();
        if(local_pc)
        {
            Vector3 dir = local_pc.GetHeadPosition() - head.position;
            dir.Normalize(); 
            dot = Vector3.Dot(thisTransform.forward, dir);
            
            if(dot > maxDotValue)
            {
                head_tracking.SetMode(TrackingMode.LookAt);
                head_tracking.LookAtPos(local_pc.GetHeadPosition());
            }
            else
            {
                head_tracking.SetMode(TrackingMode.None);
            }
        }
    }
    
    
}
