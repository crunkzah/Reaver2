
using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;

public enum Civ2State : byte
{
    Idle,
    Holding,
}

public class Civ2Controller : MonoBehaviour, INetworkObject
{
    Transform thisTransform;
    NetworkObject net_comp;
    
    public Transform head;
    TrackTo head_tracking;
    
    public Civ2State state;
    
    Animator anim;
    
    public Transform revolver_holding;
    
    void Awake()
    {
        thisTransform = transform;
        net_comp = GetComponent<NetworkObject>();
        head_tracking = head.GetComponent<TrackTo>();
        anim = GetComponent<Animator>();
    }
    
    
    void Start()
    {
        SetState(Civ2State.Holding);
    }
    
    bool canSendCommands = true;
    
    void UnlockSendingCommands()
    {
        canSendCommands = true;
    }
    
    void LockSendingCommands()
    {
        canSendCommands = false;
    }
    
    public void ReceiveCommand(NetworkCommand command, params object[] args)
    {
        switch(command)
        {
            case(NetworkCommand.SetState):
            {
                UnlockSendingCommands();
                                
                byte _state = (byte)args[0];
                
                SetState((Civ2State)_state);
                
                break;
            }
            default:
            {
                break;
            }
        }
    }
    
    public GatesController[] gates_to_open;
    
    void UpdateBrain(float dt)
    {
        switch(state)
        {
            case(Civ2State.Holding):
            {
                if(canSendCommands)
                {
                    //PlayerController masterPlayer = PhotonManager.GetLocalPlayer();
                    ref List<PlayerController> pcs = ref UberManager.Singleton().playerControllers;
                    int len = pcs.Count;                    
                    for(int i = 0; i < len; i++)
                    {
                        PlayerController playerTarget = pcs[i];
                        if(playerTarget)
                        {
                            float distance_to_revolver = Vector3.Distance(revolver_holding.position, playerTarget.GetHeadPosition());
                            //InGameConsole.LogFancy("distance_to_revolver is " + distance_to_revolver.ToString("f"));
                            if(distance_to_revolver < 2.1f)
                            {
                                LockSendingCommands();
                                NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.SetState, (byte)Civ2State.Idle);
                                PlayerInventory.Singleton().RaiseEventGiveWeaponToAllPlayers(GunType.Revolver);
                            }
                        }
                    }
                }
                break;
            }
            case(Civ2State.Idle):
            {
                break;
            }
        }
    }
    
    //[Range(-1F, 1F)]
    float maxDotValue = 0.45f;
    public float currentDot = 0;
    
    public ParticleSystem revolver_holding_ps;
    public ParticleSystem revolver_burst_ps;
    public AudioSource audio_src;
    
    public void SetState(Civ2State _state)
    {
        state = _state;
        switch(state)
        {
            case(Civ2State.Holding):
            {
                anim.SetTrigger("Hold");
                revolver_holding.gameObject.SetActive(true);
                break;
            }
            case(Civ2State.Idle):
            {
                anim.SetTrigger("Idle");
                DoLight(revolver_holding.transform.position);
                Vector3 lightPos = revolver_holding.position;
                GameObject g = ObjectPool2.s().Get(ObjectPoolKey.LightPooled, false);
                LightPooled light = g.GetComponent<LightPooled>();
                Color color = new Color(1f, 0.2f, 0f, 1f);
                audio_src.Stop();
                
                float decay_speed = 8 / 0.33f;
                light.DoLight(lightPos, color, 2f, 8, 8, decay_speed);
                revolver_holding.gameObject.SetActive(false);
                //ParticleSystem.EmissionModule module = revolver_holding_ps.emission;
                revolver_holding_ps.loop = false;
                revolver_holding_ps.GetComponent<Light>().enabled = false;
                revolver_burst_ps.Play();
               // module.rateOverTime = 0;
                
                
                // for(int i = 0; i < gates_to_open.Length; i++)
                // {
                //     gates_to_open[i].Unlock();
                // }
                Invoke(nameof(RevolverGiven_OpenGates), 1.0f);
                break;
            }
        }
    }
    
    public void RevolverGiven_OpenGates()
    {
        for(int i = 0; i < gates_to_open.Length; i++)
        {
            gates_to_open[i].Unlock();
        }
    }
    
    void DoLight(Vector3 pos)
    {
        GameObject g = ObjectPool2.s().Get(ObjectPoolKey.LightPooled, false);
        LightPooled light = g.GetComponent<LightPooled>();
        Color color = new Color(1f, 0.71f, 0.1f, 1f);
        
        float decay_speed = 5;
        light.DoLight(pos, color, 3f, 14, 3, decay_speed);
    }
    
    void UpdateBrainLocally(float dt)
    {
        switch(state)
        {
            case(Civ2State.Holding):
            {
                // ref List<PlayerController> pcs = ref UberManager.Singleton().players_controller;
                
                // float closestDistanceSqr = float.MaxValue;
                
                // PlayerController player_to_lookAt = null;
                
                // int len = pcs.Count;
                // for(int i = 0; i < len; i++)
                // {
                //     if(pcs[i])
                //     {
                //         Vector3 pc_headPos = pcs[i].GetHeadPosition();
                //         Vector3 dirToPlayer = (pc_headPos - head.position).normalized;
                        
                //         currentDot = Vector3.Dot(head.forward, dirToPlayer);
                        
                //         if(Math.Abs(currentDot) < maxDotValue)
                //         {
                //             float sqrDistance = Math.SqrDistance(pc_headPos, head.position);
                //             if(sqrDistance < closestDistanceSqr)
                //             {
                //                 closestDistanceSqr = sqrDistance;
                //                 player_to_lookAt = pcs[i];
                //             }
                //         }
                //     }
                // }
                
                // if(player_to_lookAt)
                // {
                //     head_tracking.SetMode(TrackingMode.LookAt);
                //     head_tracking.LookAtPos(player_to_lookAt.GetHeadPosition());
                // }
                // else
                // {
                //     head_tracking.SetMode(TrackingMode.None);
                // }
                
                break;
            }
            case(Civ2State.Idle):
            {
                break;
            }
        }
    }
    
    public float dot;
    
    void Update()
    {
        float dt = UberManager.DeltaTime();
        
        // PlayerController local_pc = PhotonManager.GetLocalPlayer();
        // if(local_pc)
        // {
        //     Vector3 dir = local_pc.GetHeadPosition() - head.position;
        //     dir.Normalize(); 
        //     dot = Vector3.Dot(thisTransform.forward, dir);
            
        //     if(dot > maxDotValue)
        //     {
        //         head_tracking.SetMode(TrackingMode.LookAt);
        //         head_tracking.LookAtPos(local_pc.GetHeadPosition());
        //     }
        //     else
        //     {
        //         head_tracking.SetMode(TrackingMode.None);
        //     }
        // }
        
        if(PhotonNetwork.IsMasterClient)
        {
           UpdateBrain(dt);
        }
        
        UpdateBrainLocally(dt);
    }
}
