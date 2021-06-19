using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;

public enum KateState : byte
{
    Sitting,
}

public class KateController : MonoBehaviour,  INetworkObject
{
    Transform thisTransform;
    NetworkObject net_comp;
    
    public Transform head;
    TrackTo head_tracking;
    
    
    public KateState state;
    
    void Awake()
    {
        thisTransform = transform;
        net_comp = GetComponent<NetworkObject>();
        head_tracking = head.GetComponent<TrackTo>();
    }
    
    public void ReceiveCommand(NetworkCommand command, params object[] args)
    {
        switch(command)
        {
            default:
            {
                break;
            }
        }
    }
    
    
    void UpdateBrain(float dt)
    {
        switch(state)
        {
            case(KateState.Sitting):
            {
                break;
            }
        }
    }
    
    //[Range(-1F, 1F)]
    float maxDotValue = 0.45f;
    public float currentDot = 0;
    
    void UpdateBrainLocally(float dt)
    {
        switch(state)
        {
            case(KateState.Sitting):
            {
                ref List<PlayerController> pcs = ref UberManager.Singleton().players_controller;
                
                float closestDistanceSqr = float.MaxValue;
                
                PlayerController player_to_lookAt = null;
                
                int len = pcs.Count;
                for(int i = 0; i < len; i++)
                {
                    if(pcs[i])
                    {
                        Vector3 pc_headPos = pcs[i].GetHeadPosition();
                        Vector3 dirToPlayer = (pc_headPos - head.position).normalized;
                        
                        currentDot = Vector3.Dot(head.forward, dirToPlayer);
                        
                        if(Math.Abs(currentDot) < maxDotValue)
                        {
                            float sqrDistance = Math.SqrDistance(pc_headPos, head.position);
                            if(sqrDistance < closestDistanceSqr)
                            {
                                closestDistanceSqr = sqrDistance;
                                player_to_lookAt = pcs[i];
                            }
                        }
                    }
                }
                
                if(player_to_lookAt)
                {
                    head_tracking.SetMode(TrackingMode.LookAt);
                    head_tracking.LookAtPos(player_to_lookAt.GetHeadPosition());
                }
                else
                {
                    head_tracking.SetMode(TrackingMode.None);
                }
                
                break;
            }
        }
    }
    
    public float dot;
    
    void Update()
    {
        float dt = UberManager.DeltaTime();
        
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
        
        //if(PhotonNetwork.IsMasterClient)
        //{
        //    UpdateBrain(dt);
        //}
        
        //UpdateBrainLocally(dt);
    }
}
