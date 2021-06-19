using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

public enum PadlaLongState : byte
{
    Idle,
    Chasing,
    Attacking1,
    Attacking2,
    Dead
}

public class PadlaLongController : MonoBehaviour, INetworkObject, IDamagableLocal
{
    public GameObject remoteAgent_prefab;
    NavMeshAgent remoteAgent;
    Transform remoteAgentTransform;
    
    Animator anim;
    public AudioSource audio_src;
    NetworkObject net_comp;
    Transform thisTransform;
    CapsuleCollider col;
    
    Rigidbody[] joint_rbs;
    
    static int MoveSpeedHash = -1;
    static int groundMask = -1;
    
    
    DamagableLimb[] limbs;
    
    
    //public TrailRendererController trail_arm;
    public TrailRendererController trail_spine;
    
    SpawnedObject spawnedObjectComp;
    
    void Awake()
    {
        spawnedObjectComp = GetComponent<SpawnedObject>();
        InitJoints();
        DisableSkeleton();
        
        col = GetComponent<CapsuleCollider>();
        if(audio_src == null)
            audio_src = GetComponentInChildren<AudioSource>();
        net_comp = GetComponent<NetworkObject>();
        anim = GetComponent<Animator>();
        thisTransform = transform;
        
        
        limbs = GetComponentsInChildren<DamagableLimb>();
        
        if(MoveSpeedHash == -1)
        {
            MoveSpeedHash = Animator.StringToHash("MoveSpeed");
        }
        
        if(groundMask == -1)
        {
            groundMask = LayerMask.GetMask("Ground", "Ceiling");
        }
        
    }
    
    void InitAsMaster()
    {
        remoteAgent = Instantiate(remoteAgent_prefab, thisTransform.localPosition, thisTransform.localRotation).GetComponent<NavMeshAgent>();
        remoteAgentTransform = remoteAgent.transform;
    }
    
    void Start()
    {
        if(PhotonNetwork.IsMasterClient)
        {
            InitAsMaster();
        }
        
        
        HitPoints = MaxHealth;
        SetMovePos(thisTransform.localPosition);
        WarpRemoteAgent(thisTransform.localPosition);
    }

    void InitJoints()
    {
        joint_rbs = GetComponentsInChildren<Rigidbody>();
    }
    
    void EnableSkeleton()
    {
        int len = joint_rbs.Length;
        for(int i = 0; i < len; i++)
        {
            if(joint_rbs[i])
                joint_rbs[i].isKinematic = false;
        }
    }
    
    public void DisableSkeleton()
    {
        int len = joint_rbs.Length;
        for(int i = 0; i < len; i++)
        {
            if(joint_rbs[i])
                joint_rbs[i].isKinematic = true;
        }
    }
    
    public bool canSendCommands = true;
    
    void LockSendingCommands()
    {
        canSendCommands = false;
    }
    
    void UnlockSendingCommands()
    {
        canSendCommands = true;
    }
    
    
    public void ReceiveCommand(NetworkCommand command, params object[] args)
    {
        switch(command)
        {
            case(NetworkCommand.SetTarget):
            {
                UnlockSendingCommands();
                int viewID = (int)args[0];
                
                PhotonView _pv = PhotonNetwork.GetPhotonView(viewID);
                if(_pv)
                {
                    PlayerController _pc = _pv.GetComponent<PlayerController>();
                    if(_pc)
                    {
                        SetTarget(_pc);
                        SetState(PadlaLongState.Chasing);
                    }
                }
                break;
            }
            case(NetworkCommand.Move):
            {
                UnlockSendingCommands();
                Vector3 pos = (Vector3)args[0];
                
                SetMovePos(pos);                
                break;
            }
            case(NetworkCommand.Attack):
            {
                UnlockSendingCommands();
                
                if(state == PadlaLongState.Dead)
                    return;
                
                Vector3 attackDashPos = (Vector3)args[0];
                
                brainTimer = 0;
                attack_timer = 0;
                
                SetDashPos(attackDashPos);
                
                SetMovePos(currentDestination);
                WarpRemoteAgent(currentDestination);
                UpdateRemoteAgentDestination(currentDestination);
                Kick();
                SetState(PadlaLongState.Attacking1);
                
                
                break;
            }
            case(NetworkCommand.SetState):
            {
                UnlockSendingCommands();
                
                if(state == PadlaLongState.Dead)
                {
                    return;
                }
                
                byte _state = (byte)args[0];
                
                SetState((PadlaLongState)_state);
                
                break;
            }
            case(NetworkCommand.DieWithForce):
            {
                
                Vector3 force = (Vector3)args[0];
                
                byte limb_id = 0;
                if(args.Length > 1)
                {
                    limb_id = (byte)args[1];
                }
                
                Die(force, limb_id);
                
                break;
            }
            case(NetworkCommand.TakeDamage):
            {
                int incomingDamage = (int)args[0];
                TakeDamage(incomingDamage);
                
                break;
            }
            default:
            {
                break;
            }
        }
    }
    
    void SetState(PadlaLongState _state)
    {
        if(state == PadlaLongState.Dead)
        {
            return;
        }
        
        
        switch(_state)
        {
            case(PadlaLongState.Attacking1):
            {
                canDoMeleeDamageToLocalPlayer = true;
                anim.SetFloat(MoveSpeedHash, 0);
                break;
            }
            case(PadlaLongState.Chasing):
            {
                distanceTravelledRunningSqr = 0;
                break;
            }
            default:
            {
                break;
            }
        }
        
        
        state = _state;
    }
    
    float damage_taken_timeStamp;
    
    void TakeDamage(int dmg)
    {
        HitPoints -= dmg;
        // if(HitPoints <= 0)
        // {
        //    Die(Vector3.zero);
        //    HitPoints = 0;
        // }
    }
    
    void Die(Vector3 force, byte limb_to_destroy)
    {
        if(state == PadlaLongState.Dead)
        {
            return;
        }
        
        SetState(PadlaLongState.Dead);
        
        if(spawnedObjectComp)
            spawnedObjectComp.OnObjectDied();
        
        HitPoints = -1;
        
        anim.enabled = false;
        col.enabled = false;
        
        NetworkObjectsManager.UnregisterNetObject(net_comp);
        if(remoteAgent)
            Destroy(remoteAgent.gameObject, 0.1f);
            
        int len = joint_rbs.Length;
        
        
        //audio_src.PlayOneShot(clipDeath, 0.5f);
        HitPoints = 0;
        
        EnableSkeleton();  
        AudioManager.Play3D(SoundType.death_impact_gib_distorted, thisTransform.localPosition);
        
        if(limb_to_destroy != 0)
        {
            if(limbs != null)
            {
                len = limbs.Length;
                for(int i = 0; i < len; i++)
                {
                    if(limbs[i].limb_id == limb_to_destroy)
                    {
                        Vector3 f = force;
                        //InGameConsole.LogOrange(string.Format("Apply force {0} to limb {1}", f, limb_to_destroy));
                        
                        limbs[i].ApplyForceToAdjacentLimbs(f);
                        if(!limbs[i].isRootLimb)
                        {
                            limbs[i].TakeDamageLimb(2500);
                        }
                        //limbs[i].AddForceToLimb(f);
                        
                        break;
                    }
                }
            }
        }
        DropHealthCrystals();
    }
    
    void DropHealthCrystals()
    {
        HealthCrystal hc = ObjectPool.s().Get(ObjectPoolKey.HealthCrystal).GetComponent<HealthCrystal>();
        hc.Launch(this.transform.localPosition + new Vector3(0, 1.25f, 0));
    }
    
    
    const int MaxHealth = 1800;
    public int HitPoints = MaxHealth;
    
    public int GetCurrentHP()
    {
        return HitPoints;
    }
    
    public void TakeDamageLocally(int dmg, Vector3 hitPos, Vector3 hitDir)
    {
        if(state != PadlaLongState.Dead && damage_taken_timeStamp + 0.15F < Time.time)
        {
            float vol = Random.Range(0.8f, 1f);
            //audio_src.PlayOneShot(clipHurt1, vol);
            
            damage_taken_timeStamp = Time.time;
        }
    }
    
    public bool IsDead()
    {
        if(state == PadlaLongState.Dead)
            return true;
        else
            return false;
    }
    
    public PadlaLongState state = PadlaLongState.Idle;
    
    const float PATH_UPDATE_CD = 0.125F * 2;
    float brainTimer = 0;
    
    Vector3 dashPos;
    
    void SetDashPos(Vector3 _dashPos)
    {
        dashPos = _dashPos;
    }
    
    void SetMovePos(Vector3 pos)
    {
        currentDestination = pos;
    }
    
    Vector3 currentDestination;
    static readonly Vector3 vUp = new Vector3(0, 1, 0);
    Quaternion deriv;
    
    const float moveSpeed = 8F;
    const float rotateTime = 0.1F;
    float speedMult = 1;
    
    const float rotationDistanceEpsilon = 0.01F;
    
    
     void RotateToLookAt(Vector3 lookAtPointXZ, float timeToRotate, bool lookAtTargetIfNear = true)
    {
        Vector3 ourPos = thisTransform.localPosition;
        lookAtPointXZ.y = ourPos.y;
        
        if(Math.SqrDistance(lookAtPointXZ, ourPos) > rotationDistanceEpsilon * rotationDistanceEpsilon)
        {
            Vector3 lookDir = Math.Normalized(lookAtPointXZ - ourPos);
            Quaternion targetRotation = Quaternion.LookRotation(lookDir, vUp);
            thisTransform.localRotation = QuaternionUtil.SmoothDamp(thisTransform.localRotation, targetRotation, ref deriv, timeToRotate);
        }
        else
        {
            if(lookAtTargetIfNear && target_pc)
            {
                Vector3 targetPos = target_pc.GetGroundPosition();
                targetPos.y = ourPos.y;
                
                Vector3 lookDir = targetPos - ourPos;
                if(Math.SqrMagnitude(lookDir) > rotationDistanceEpsilon * rotationDistanceEpsilon)
                {
                    lookDir = Math.Normalized(lookDir);
                    Quaternion targetRotation = Quaternion.LookRotation(lookDir, vUp);
                    thisTransform.localRotation = QuaternionUtil.SmoothDamp(thisTransform.localRotation, targetRotation, ref deriv, timeToRotate);
                }
            }
        }
    }
    
    void WarpRemoteAgent(Vector3 pos)
    {
        if(remoteAgent)
        {
            if(!remoteAgent.gameObject.activeSelf)
            {
                remoteAgent.gameObject.SetActive(true);
            }
            NavMeshHit navMeshHit;
            NavMesh.SamplePosition(pos, out navMeshHit, 1, NavMesh.AllAreas);
            remoteAgent.ResetPath();
            remoteAgent.Warp(navMeshHit.position);
        }
    }
    
    
    void DisableRemoteAgent()
    {
        remoteAgent.gameObject.SetActive(false);
    }
    
    void UpdateRemoteAgentDestination(Vector3 destPos)
    {
        if(remoteAgent)
        {
            remoteAgent.SetDestination(destPos);
        }
    }
    
    Vector3 GetRemoteAgentPos()
    {
        return remoteAgentTransform.localPosition;
    }
    
    PlayerController target_pc;
    
    
    void SetTarget(PlayerController target)
    {
        target_pc = target;
    }
    
    public float attack_timer = 0;
    bool canDoMeleeDamageToLocalPlayer = true;
    const float kick_duration = 1.1f;//2.5F / 2.5f;
    
    const float kick_damageTimingStart = 0.5F / 2;
    const float kick_damageTimingEnd = 0.85F / 2;
    
    const float dashSpeed = 18F;
    
    public Vector3 localStrikeOffset = new Vector3(0, 1.25f, 1.0f);
    
    const float kick_cooldown = 5F;
    const float kick_distance = 3F;
    const float kick_radius = 2.25F;
    const int kick_dmg = 30;
    const float kick_dashDistance = 0.5F;
    
    
    void UpdateBrain(float dt)
    {
        switch(state)
        {
            case(PadlaLongState.Idle):
            {
                if(canSendCommands)
                {
                    Transform potentialTarget = ChooseTargetClosest(thisTransform.localPosition);
                    if(potentialTarget)
                    {
                        PlayerController pc = potentialTarget.GetComponent<PlayerController>();
                        if(pc)
                        {
                            LockSendingCommands();
                            NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.SetTarget, pc.photonView.ViewID);
                        }
                    }
                }
                
                break;
            }
            case(PadlaLongState.Chasing):
            {
                brainTimer += dt;
                
                if(target_pc)
                {
                    Vector3 targetGroundPos = target_pc.GetGroundPosition();
                    
                    UpdateRemoteAgentDestination(targetGroundPos);
                    
                    Vector3 padlaPosition = thisTransform.localPosition;
                    
                    if(canSendCommands)
                    {
                        if(Math.SqrDistance(targetGroundPos, padlaPosition) < kick_distance  * kick_distance)
                        {
                            NavMeshHit navMeshHit;
                            //Vector3 punch_dir = (Math.GetXZ(targetGroundPos - padlaPosition)).normalized;
                            Vector3 punch_dir = (targetGroundPos - padlaPosition).normalized;
                            
                            Vector3 dash_pos = padlaPosition;
                            if(NavMesh.SamplePosition(padlaPosition + punch_dir * kick_dashDistance, out navMeshHit, 1f, NavMesh.AllAreas))
                            {
                                dash_pos = navMeshHit.position;
                                // InGameConsole.LogFancy("We DO <color=green>DASH ATTACK</color>");
                            }
                            
                            LockSendingCommands();
                            NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.Attack, dash_pos);
                        }
                        else if(brainTimer > PATH_UPDATE_CD)
                        {
                            brainTimer = 0;
                            
                            Vector3 remoteAgentPos = GetRemoteAgentPos();
                            
                            if(Math.SqrDistance(padlaPosition, remoteAgentPos) > 0.175F * 0.175F)
                            {
                                LockSendingCommands();
                                NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.Move, remoteAgentPos);
                            }
                        }
                    }
                
                }
                else
                {
                    if(canSendCommands)
                    {
                        Transform potentialTarget = ChooseTargetClosest(thisTransform.localPosition);
                        if(potentialTarget)
                        {
                            PlayerController pc = potentialTarget.GetComponent<PlayerController>();
                            if(pc)
                            {
                                LockSendingCommands();
                                NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.SetTarget, pc.photonView.ViewID);
                            }
                        }
                        else
                        {
                            LockSendingCommands();
                            NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.SetState, (byte)PadlaLongState.Idle);                        
                        }
                    }
                }
                
                break;
            }
            case(PadlaLongState.Attacking1):
            {
                brainTimer += dt;
                
                if(canSendCommands)
                {
                    if(brainTimer > kick_duration)
                    {
                        brainTimer = 0;
                        Transform potentialTarget = ChooseTargetClosest(thisTransform.localPosition);
                        UpdateRemoteAgentDestination(dashPos);
                        WarpRemoteAgent(dashPos);
                        
                        if(potentialTarget)
                        {
                            PlayerController pc = potentialTarget.GetComponent<PlayerController>();
                            if(pc)
                            {
                                LockSendingCommands();
                                NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.SetTarget, pc.photonView.ViewID);
                            }
                        }
                        else
                        {
                            LockSendingCommands();
                            NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.SetState, (byte)PadlaLongState.Idle);
                        }
                        
                    }
                }
                break;
            }
            case(PadlaLongState.Attacking2):
            {
                break;
            }
            
            case(PadlaLongState.Dead):
            {
                break;
            }
        }
    }
    
    // bool IsGroundedAndOnNavMesh(Vector3 positionCheck, float distanceCheck, out Vector3 navMeshPos)
    // {
    //     bool Result = false;
        
    //     navMeshPos = positionCheck;
        
    //     RaycastHit hit;
    //     if(Physics.Raycast(positionCheck, -vUp, out hit, distanceCheck, groundMask))
    //     {
    //         NavMeshHit navMeshHit;
    //         if(NavMesh.SamplePosition(positionCheck, out navMeshHit, 1, NavMesh.AllAreas))
    //         {
    //             navMeshPos = navMeshHit.position;
    //             Result = true;
    //         }
    //     }
        
    //     return Result;
    // }
   
    
    Vector3 GetCapsulePointBottom()
    {
        Vector3 Result = thisTransform.localPosition;
        
        //Result.x += col.center.x;
        Result.y += col.center.y - col.height/2 + col.radius;
        //Result.z += col.center.z;
        
        return Result;
    }
    
    Vector3 GetCapsulePointTop()
    {
        Vector3 Result = thisTransform.localPosition;
        
        //Result.x += col.center.x;
        Result.y += col.center.y + col.height/2 - col.radius;
        //Result.z += col.center.z;
        
        return Result;
    }
    
    void Kick()
    {
        anim.Play("Base.Kick", 0, 0);
        
        //trail_spine.EmitFor(kick_duration);
        //audio_src.clip = clipKick;
        //audio_src.PlayDelayed(punch1_damageTimingStart);
    }
    
    
    Vector3 gizmoP1;
    Vector3 gizmoP2;
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(currentDestination, 0.3f);
        
        GUIStyle style = new GUIStyle();
        
        style.alignment = TextAnchor.MiddleCenter;
        style.normal.textColor = Colors.Orange;
        style.richText = true;
        style.fontSize = 18;
        style.fontStyle = FontStyle.Bold;
        
        UnityEditor.Handles.Label(transform.localPosition + new Vector3(0, 2.75f, 0), state.ToString(), style);
        
    }
#endif
    
    const float animDampTime = 0.05F;
    
    
    
    
    const float footStepDistance = 3f;
    float distanceTravelledRunningSqr;
    
    void UpdateBrainLocally(float dt)
    {
        switch(state)
        {
            case(PadlaLongState.Idle):
            {
                break;
            }
            case(PadlaLongState.Chasing):
            {
                Vector3 currentPos = thisTransform.localPosition;
                
                float dV = speedMult * dt * moveSpeed;
                Vector3 updatedPos = Vector3.MoveTowards(currentPos, currentDestination, dV);
                thisTransform.localPosition = updatedPos; 
                
                
                RotateToLookAt(currentDestination, rotateTime * speedMult, false);
                
                Vector3 dPos = new Vector3();//= currentDestination - currentPos;
                
                dPos.x = currentDestination.x - currentPos.x;
                dPos.y = currentDestination.y - currentPos.y;
                dPos.z = currentDestination.z - currentPos.z;
                
                anim.SetFloat(MoveSpeedHash, Math.Magnitude(dPos), animDampTime, dt);
                
                distanceTravelledRunningSqr += Math.Magnitude(updatedPos - currentPos);
                
                const float footStepDistanceSqr = footStepDistance * footStepDistance;
                
                if(distanceTravelledRunningSqr > footStepDistanceSqr)
                {
                    distanceTravelledRunningSqr -= footStepDistanceSqr;
                    if(distanceTravelledRunningSqr  > footStepDistanceSqr)
                        distanceTravelledRunningSqr = 0;
                    
                    audio_src.PlayOneShot(clipStep, 0.33F);
                }
                
                break;
            }
            case(PadlaLongState.Attacking1):
            {
                Vector3 currentPos = thisTransform.localPosition;
                
                
                float dV = speedMult * dt * moveSpeed;
                
                
                attack_timer += dt;
                RotateToLookAt(dashPos, rotateTime * speedMult / 4, false);
                
                if(attack_timer > kick_damageTimingStart)
                {
                    
                    dV = speedMult * dt * dashSpeed;
                    thisTransform.localPosition = Vector3.MoveTowards(currentPos, dashPos, dV);
                    SetMovePos(dashPos);
                }
                else
                {
                    thisTransform.localPosition = Vector3.MoveTowards(currentPos, currentDestination, dV);
                }
                
                if(canDoMeleeDamageToLocalPlayer)
                {
                    
                    if(attack_timer > kick_damageTimingStart && attack_timer < kick_damageTimingEnd)
                    {
                        Vector3 dmgPos = thisTransform.localPosition;
                        dmgPos = dmgPos + thisTransform.up * localStrikeOffset.y + thisTransform.forward * localStrikeOffset.z;
                        float dmgRadius = kick_radius;
                        
                        if(TryDoMeleeDamageToLocalPlayer(dmgPos, dmgRadius))
                        {
                            canDoMeleeDamageToLocalPlayer = false;    
                        }
                    }
                    
                }
                
                break;
            }
            case(PadlaLongState.Attacking2):
            {
                break;
            }
            case(PadlaLongState.Dead):
            {
                break;
            }
        }
    }
    
    void Update()
    {
        float dt = UberManager.DeltaTime();
        
        if(PhotonNetwork.IsMasterClient)
        {
            UpdateBrain(dt);
        }
        
        UpdateBrainLocally(dt);
    }
    
    Transform ChooseTargetClosest(Vector3 seekerPos)
    {
        Transform result = null;
        
        int len = NPCManager.Singleton().aiTargets.Count;
        
        float minDistanceSqr = float.MaxValue;
        
        for(int i = 0; i < len; i++)
        {
            Transform potentialTarget = NPCManager.Singleton().aiTargets[i];
            float distSqr = Math.SqrDistance(seekerPos, potentialTarget.position);
            
            if(distSqr <  minDistanceSqr)
            {
                minDistanceSqr = distSqr;
                result = potentialTarget;
            }
        }
        
        return result;
    }
    
    bool TryDoMeleeDamageToLocalPlayer(Vector3 pos, float radius)
    {
        //InGameConsole.LogFancy(string.Format("Padla: <color=yellow>{0}</color>", "TryDoMeleeDamageToLocalPlayer"));
        PlayerController local_pc = PhotonManager.GetLocalPlayer();
        
        // gizmoPos = pos;
        // gizmoRadius = radius;
        
        if(local_pc)
        {
            float sqrDistanceToPlayer = Math.SqrDistance(local_pc.GetGroundPosition() + new Vector3(0, 0.5f, 0), pos);
            
            //InGameConsole.LogFancy(string.Format("Distance to player: <color=yellow>{0}</color>", Mathf.Sqrt(sqrDistanceToPlayer).ToString("f")));
            
            if(sqrDistanceToPlayer < radius * radius)
            {
                Vector3 dmgDir = -thisTransform.localPosition + local_pc.GetGroundPosition();
                dmgDir.y = 0;
                dmgDir.Normalize();
                
                dmgDir.y = 1;
                local_pc.BoostVelocity(dmgDir * 18);
                
                local_pc.TakeDamage(kick_dmg);
                return true;
            }
        }
        
        return false;
    }
    
    [Header("Clips:")]
    public AudioClip clipStep;
    public AudioClip clipHurt1;
    public AudioClip clipDeath;
    public AudioClip clipKick;
}
