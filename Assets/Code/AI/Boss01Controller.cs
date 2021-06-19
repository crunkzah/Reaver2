using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

public class Boss01Controller : MonoBehaviour, INetworkObject
{
    NetworkObject netComponent;
    NavMeshAgent agent;
    Animator anim;
    Transform thisTransform;
    
    Transform target;
    
    public enum Boss01State : int
    {
        Sleeping,
        Idle,
        Chasing,
        Stomping,
        Dead
    }
    
    
    [Header("Variables:")]
    public float sqrFatalVisionRadius = 15 * 15;
    public float idleVisionRadiusSqr = 20 * 20;
    public float idleVisionFovCos = 0.17364817766F;    
    const float eyesOffsetY = 5f;
    
    public Boss01State state = Boss01State.Idle;
    
    Vector3 spawnPos;
    
    
    int moveSpeedHash;
    
    
    void InitOnce()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        netComponent = GetComponent<NetworkObject>();
        thisTransform = transform;
        
        ActionHash = Animator.StringToHash("Action");
        
        moveSpeedHash = Animator.StringToHash("moveSpeed");
        DeadHash = Animator.StringToHash("isDead");
        
        
        groundMask = LayerMask.GetMask("Ground", "Fadable");
    }
    
    void Start()
    {
        InitOnce();
        InitialState();
    }
    
    void InitialState()
    {
        NavMeshHit hit;
        NavMesh.SamplePosition(thisTransform.position, out hit, 50, NavMesh.AllAreas);
        spawnPos = hit.position;
        canSendCommands = true;
        
        stompTimer = 0f;
        
        HitPoints = MaxHealth;
        
        anim.SetBool(DeadHash, false);
        anim.SetLayerWeight(1, 0);
    }
    
    public bool canSendCommands;
    
    void LockSendingCommands()
    {
        canSendCommands = false;
    }
    
    const int MaxHealth = 1500;
    int HitPoints;
    
    public void ReceiveCommand(NetworkCommand command, params object[] args)
    {
        switch(command)
        {
            case(NetworkCommand.TakeDamage):
            {
                int dmg = (int)args[0];
                
                TakeDamage(dmg);
                
                break;
            }
            case(NetworkCommand.SetTarget):
            {
                canSendCommands = true;
                
                
                int photonViewId = (int)args[0];
                
                PhotonView pv = PhotonNetwork.GetPhotonView(photonViewId);
                
                SetTarget(pv.transform);
                SetState(Boss01State.Chasing);
                
                
                stompTimer = 0f;
                stompTimer -= stompMinTime;
                
                
                
                
                break;
            }
            case(NetworkCommand.ClearTarget):
            {
                canSendCommands = true;
                
                ClearTarget();
                
                break;
            }
            case(NetworkCommand.Ability1):
            {
                canSendCommands = true;
                
                
                agent.enabled = false;
                
                
                anim.SetLayerWeight(1, 1f);
                anim.SetInteger(ActionHash, 1);
                
                
                SetState(Boss01State.Stomping);
                
                
                InGameConsole.LogFancy("Stomping.");
                                
                break;                
            }
            default:
            {
                InGameConsole.LogError(string.Format("Unhandled command <color=blue>{0}</color> for <color=blue>{1}</color> !", command.ToString(), gameObject.name));
                break;
            }
        }
    }
    
    double setTargetDelay = 0.2d;
    
    
    public float stompTimer = 0f;
    const float stompCooldown = 6f;
    public float stompMinTime = 0f;
    public float stompRadiusSqr = 6 * 6;
    public int stompDamage = 75;
    
    
    
    void UpdateBrain()
    {
        switch(state)
        {
            case(Boss01State.Idle):
            {
                for(int i = 0; i < NPCManager.Singleton().aiTargets.Count; i++)
                {
                    Transform potentialTarget = NPCManager.Singleton().aiTargets[i];
                    
                     if(NPCManager.CheckTargetVisibility(thisTransform, potentialTarget,
                                                     sqrFatalVisionRadius, idleVisionRadiusSqr,
                                                     idleVisionFovCos,
                                                     eyesOffsetY, Globals.playerEyesOffset.y))
                    {
                        int photonViewId = potentialTarget.GetComponent<PhotonView>().ViewID;
                        double timeToExecute = setTargetDelay + PhotonNetwork.Time;
                        NetworkObjectsManager.ScheduleCommand(netComponent.networkId,  timeToExecute, NetworkCommand.SetTarget, photonViewId);
                        
                        LockSendingCommands();
                        break;
                    }
                    
                }
                
                
                break;
            }
            case(Boss01State.Chasing):
            {
                if(target == null)
                {
                    LockSendingCommands();
                    NetworkObjectsManager.CallNetworkFunction(netComponent.networkId, NetworkCommand.ClearTarget);
                }   
                
                
                
                
                stompTimer += UberManager.DeltaTime();
                if(stompTimer >= stompCooldown)
                {
                    stompTimer = 0f;            
                    
                    NetworkObjectsManager.ScheduleCommand(netComponent.networkId, PhotonNetwork.Time + 0.2d, NetworkCommand.Ability1);
                }
                             
                break;
            }
        }
    }
    
    void UpdateBrainLocally()
    {
        switch(state)
        {
            case(Boss01State.Chasing):
            {
                if(target != null)
                {
                    agent.SetDestination(target.position);
                }
                    
                break;
            }
        }
    }
    
    
    void TakeDamage(int dmg)
    {
        HitPoints -= dmg;
        if(HitPoints < 0)
        {
            HitPoints = 0;
        }
        
        if(state != Boss01State.Dead && HitPoints <= 0)
        {
            Die();
        }
    }
    
    void Die()
    {
        InGameConsole.LogOrange(this.gameObject.name + " has died.");
        agent.enabled = false;
        anim.SetBool(DeadHash, true);
        anim.SetLayerWeight(1, 0);
        
        SetState(Boss01State.Dead);
    }
    
    void SetTarget(Transform t)
    {
        target = t;
    }
    
    void ClearTarget()
    {
        target = null;
        SetState(Boss01State.Idle);
    }
    
    void SetState(Boss01State newState)
    {
        state = newState;
        
    }
    
    void Update()
    {
        if(PhotonNetwork.IsMasterClient && canSendCommands)
            UpdateBrain();
        
        UpdateBrainLocally();
        
        Animation();
        // Debug();
    }
    
    
    
    
    [Header("Animations:")]
    int ActionHash; // 0 - empty, 1 - stomp
    int DeadHash;
    
    [Header("Footsteps:")]
    public AudioSource footStepSource;
    public Transform footL;
    public Transform footR;
    float footStepRayLength = 1f;
    
    int groundMask;
    float footStepCd = 0.33f;    
    public float timeNextFootStep = 0f;
    
    float lastFootStepTime = 0;
    
    bool footGroundedL;
    bool footGroundedR;
    
    
    
    void FootStep()
    {
        // InGameConsole.LogFancy("Footstep.");
        footStepSource.pitch = Random.Range(0.8f, 0.9f);
        footStepSource.Play();
        timeNextFootStep = UberManager.TimeSinceStart() + footStepCd;
        FollowingCamera.ShakeY(Random.Range(8, 9));
        
        
        // InGameConsole.LogFancy("Time since last step: " + (UberManager.TimeSinceStart() - lastFootStepTime).ToString());
        lastFootStepTime = UberManager.TimeSinceStart();
    }
    
    void Animation()
    {
        float moveSpeed = Math.Magnitude(Math.GetXZ(agent.velocity));
        
        
        Ray footRay = new Ray(footL.position, Vector3.down);        
        
        if(state == Boss01State.Chasing && UberManager.TimeSinceStart() > timeNextFootStep)
        {
            //Debug.DrawRay(footRay.origin, footRay.direction * footStepRayLength, Color.red);
            
            //Left foot:
            if(Physics.Raycast(footRay, footStepRayLength, groundMask))
            {
                if(!footGroundedL)
                {
                    FootStep();
                }
                footGroundedL = true;
            }
            else
            {
                footGroundedL = false;
            }
            
            //Right foot:
            footRay.origin = footR.position;
            if(Physics.Raycast(footRay, footStepRayLength, groundMask))
            {
                if(!footGroundedR)
                {
                    FootStep();
                }
                footGroundedR = true;
            }
            else
            {
                footGroundedR = false;
            }
        }
        
        anim.SetFloat(moveSpeedHash, moveSpeed);
    }
    
    void OnStompHappened()
    {
        if(state == Boss01State.Dead)
            return;
        
        // InGameConsole.LogFancy("OnStompHappened()");
        AudioManager.PlayGlobalClip(0, 0.55f, 1);
        FollowingCamera.ShakeY(15);
        
        
        for(int i = 0; i < NPCManager.Singleton().aiTargets.Count; i++)
        {
            Transform t = NPCManager.Singleton().aiTargets[i];
            if(Math.SqrDistance(t.position, thisTransform.position) < stompRadiusSqr)
            {
                t.GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.AllViaServer, stompDamage);
                // _target.GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.AllViaServer, slamDamage);
            }
        }
        
        
        Vector3 pos = thisTransform.position;
        
        float magnitude = 6;
        
        ParticlesManager.Play(2, pos + Math.GetXZ(Random.onUnitSphere) * magnitude, Vector3.up);
        ParticlesManager.Play(2, pos + Math.GetXZ(Random.onUnitSphere) * magnitude, Vector3.up);
        ParticlesManager.Play(2, pos + Math.GetXZ(Random.onUnitSphere) * magnitude, Vector3.up);
        ParticlesManager.Play(2, pos + Math.GetXZ(Random.onUnitSphere) * magnitude, Vector3.up);
        
    }
    
    void OnStompEnded()
    {
        
        if(state == Boss01State.Dead)
            return;
        
        
        // InGameConsole.LogFancy("OnStompEnded()");
        anim.SetInteger(ActionHash, 0);
        anim.SetLayerWeight(1, 0f);
        
        agent.enabled = true;
        
        if(state == Boss01State.Stomping)
        {
            SetState(Boss01State.Chasing);
        }
        else
        {
            InGameConsole.LogWarning("OnStompEnded was called but boss not in 'Stomping' state.");
        }
    }
    
    
#if UNITY_EDITOR

    GUIStyle style = new GUIStyle();

    void OnDrawGizmos()
    {
        if(Application.isPlaying && thisTransform != null)
        {
            
            string txt = string.Format("{0}\nHP: {1}", state, HitPoints);
            
            style.alignment = TextAnchor.MiddleCenter;
            style.richText = true;
            style.fontStyle = FontStyle.Bold;
            style.normal.textColor = Colors.Orange;
            
            UnityEditor.Handles.Label(thisTransform.position + Vector3.up * eyesOffsetY, txt, style);
        }
        // Gizmos.color = Color.red;
        // Gizmos.DrawSphere(destination, 0.8f);
    }
#endif
}
