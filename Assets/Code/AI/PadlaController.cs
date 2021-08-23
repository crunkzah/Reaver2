using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

public enum PadlaState : byte
{
    Idle,
    Chasing,
    Attacking1,
    Attacking2,
    Airbourne,
    Dead,
    Hanging
}

public class PadlaController : MonoBehaviour, INetworkObject, IDamagableLocal, ILaunchableAirbourne
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
    
    
    public DamagableLimb[] limbs;
    
    
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
        
        path_update_cd = PhotonNetwork.OfflineMode ? PATH_UPDATE_BASE / 3 : PATH_UPDATE_BASE;
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
    
    Vector3 attackDashDir;
    
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
                        SetState(PadlaState.Chasing);
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
                
                if(state == PadlaState.Dead || state == PadlaState.Airbourne)
                    return;
                
                Vector3 attackDashPos = (Vector3)args[0];
                attackDashDir = (Vector3)args[1];
                
                brainTimer = 0;
                attack_timer = 0;
                
                SetDashPos(attackDashPos);
                
                SetMovePos(currentDestination);
                WarpRemoteAgent(currentDestination);
                UpdateRemoteAgentDestination(currentDestination);
                Punch1_L();
                SetState(PadlaState.Attacking1);
                
                
                break;
            }
            case(NetworkCommand.Ability1):
            {
                UnlockSendingCommands();
                
                if(state == PadlaState.Dead || state == PadlaState.Airbourne)
                    return;
                
                Vector3 attackDashPos = (Vector3)args[0];
                attackDashDir = (Vector3)args[1];
                
                brainTimer = 0;
                attack_timer = 0;
                
                SetDashPos(attackDashPos);
                
                SetMovePos(currentDestination);
                WarpRemoteAgent(currentDestination);
                UpdateRemoteAgentDestination(currentDestination);
                Punch1_R();
                SetState(PadlaState.Attacking1);
                
                
                break;
            }
            
            case(NetworkCommand.SetState):
            {
                UnlockSendingCommands();
                
                if(state == PadlaState.Dead)
                {
                    return;
                }
                
                byte _state = (byte)args[0];
                
                SetState((PadlaState)_state);
                
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
            case(NetworkCommand.LaunchAirborne):
            {
                UnlockSendingCommands();
                
                if(state == PadlaState.Dead)
                {
                    return;
                }
                
                //airbourneTimeStamp = Time.time;
                
                Vector3 launchPos = (Vector3)args[0];
                Vector3 launchVel = (Vector3)args[1];
                
                if(args.Length > 2)
                {
                    //We receive a damage also:
                    int incomingDamage = (int)args[2];
                    TakeDamage(incomingDamage);
                }
                    
                velocity = launchVel * Globals.NPC_airbourne_force_mult;
                audio_src.PlayOneShot(clipLaunchedAirborne);
                thisTransform.localPosition = launchPos;
                SetState(PadlaState.Airbourne);
                    
                break;
            }
            case(NetworkCommand.LaunchAirborneUp):
            {
                UnlockSendingCommands();
                
                if(state != PadlaState.Airbourne)
                {
                    Vector3 launchPos = (Vector3)args[0];
                    float upForce = (float)args[1];
                    
                    velocity = new Vector3(0, upForce, 0);
                    audio_src.PlayOneShot(clipLaunchedAirborne);
                    thisTransform.localPosition = launchPos;
                    SetState(PadlaState.Airbourne);
                    
                }
                
                break;
            }
            case(NetworkCommand.LandOnGround):
            {
                UnlockSendingCommands();
                
                Vector3 landPos = (Vector3)args[0];
                
                WarpRemoteAgent(landPos);
                SetMovePos(landPos);
                
                
                anim.Play("Base.Idle", 0, 0);
                
                SetState(PadlaState.Idle);
                
                
                break;
            }
            default:
            {
                break;
            }
        }
    }
    
    void SetState(PadlaState _state)
    {
        if(state == PadlaState.Dead)
        {
            return;
        }
        
        switch(_state)
        {
            case(PadlaState.Attacking1):
            {
                canDoMeleeDamageToLocalPlayer = true;
                anim.SetFloat(MoveSpeedHash, 0);
                break;
            }
            case(PadlaState.Airbourne):
            {
                if(PhotonNetwork.IsMasterClient)
                {
                    DisableRemoteAgent();
                }
                
                anim.Play("Base.Airbourne", 0, 0);
                break;
            }
            case(PadlaState.Chasing):
            {
                distanceTravelledRunningSqr = 0;
                brainTimer = path_update_cd;
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
        if(state == PadlaState.Dead)
        {
            return;
        }
        
        SetState(PadlaState.Dead);
        
        if(spawnedObjectComp)
            spawnedObjectComp.OnObjectDied();
        
        HitPoints = -1;
        
        anim.enabled = false;
        col.enabled = false;
        
        NetworkObjectsManager.UnregisterNetObject(net_comp);
        if(remoteAgent)
            Destroy(remoteAgent.gameObject, 0.1f);
            
        int len = joint_rbs.Length;
        
        
        audio_src.PlayOneShot(clipDeath, 0.5f);
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
                    limbs[i].MakeLimbDead();
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
                        
                        //break;
                    }
                }
            }
        }
        DropHealthCrystals();
    }
    
    void DropHealthCrystals()
    {
        HealthCrystal hc = ObjectPool.s().Get(ObjectPoolKey.HealthCrystal).GetComponent<HealthCrystal>();
        hc.Launch(this.transform.localPosition + new Vector3(0, 1.25f, 0), 10);
    }
    
    
    const int MaxHealth = 300;
    public int HitPoints = MaxHealth;
    
    public int GetCurrentHP()
    {
        return HitPoints;
    }
    
    public bool IsDead()
    {
        if(state == PadlaState.Dead)
            return true;
        else
            return false;
    }
    
    public void TakeDamageLocally(int dmg, Vector3 hitPos, Vector3 hitDir)
    {
        if(state != PadlaState.Dead && damage_taken_timeStamp + 0.15F < Time.time)
        {
            float vol = Random.Range(0.8f, 1f);
            audio_src.PlayOneShot(clipHurt1, vol);
            
            damage_taken_timeStamp = Time.time;
        }
    }
    
    public PadlaState state = PadlaState.Idle;
    
    const float PATH_UPDATE_BASE = 0.125F;
    float path_update_cd;
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
    
    const float moveSpeed = 12F;
    const float rotateTime = 0.1F/2;
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
    
    public bool CanBeLaunchedUp()
    {
        if(state == PadlaState.Dead || state == PadlaState.Airbourne)
        {
            return false;
        }
        
        return true;
    }
    
    public bool CanBeLaunched()
    {
        return true;
    }
    
    public bool IsCurrentlyAirborne()
    {
        return state == PadlaState.Airbourne;
    }
    
    void SetTarget(PlayerController target)
    {
        target_pc = target;
    }
    
    public float attack_timer = 0;
    bool canDoMeleeDamageToLocalPlayer = true;
    const float punch1_duration = 0.9f;//2.5F / 2.5f;
    
    const float punch1_damageTimingStart = 0.5F / 2;
    const float punch1_damageTimingEnd = 0.85F / 2;
    
    const float dashSpeed = 27F;
    
    public Vector3 localStrikeOffset = new Vector3(0, 0.75f, 1.0f);
    
    const float punch1_cooldown = 5F;
    const float punch1_distance = 2F;
    const float punch1_radius = 1.85F;
    const int punch1_dmg = 25;
    const float punch1_dashDistance = 4f;
    
    int numberOfPunchesPerformed = 0;
    
    
    void UpdateBrain(float dt)
    {
        switch(state)
        {
            case(PadlaState.Idle):
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
                            NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.SetTarget, pc.pv.ViewID);
                        }
                    }
                }
                
                break;
            }
            case(PadlaState.Chasing):
            {
                brainTimer += dt;
                
                if(target_pc)
                {
                    Vector3 targetGroundPos = target_pc.GetGroundPosition();
                    
                    UpdateRemoteAgentDestination(targetGroundPos);
                    
                    Vector3 padlaPosition = thisTransform.localPosition;
                    
                    if(canSendCommands)
                    {
                        if(Math.SqrDistance(targetGroundPos, padlaPosition) < punch1_distance  * punch1_distance)
                        {
                            NavMeshHit navMeshHit;
                            //Vector3 punch_dir = (Math.GetXZ(targetGroundPos - padlaPosition)).normalized;
                            Vector3 punch_dir = (targetGroundPos - padlaPosition).normalized;
                            
                            Vector3 dash_pos = padlaPosition;
                            if(NavMesh.SamplePosition(padlaPosition + punch_dir * punch1_dashDistance, out navMeshHit, 0.125f, NavMesh.AllAreas))
                            {
                                dash_pos = navMeshHit.position;
                                InGameConsole.LogFancy("We DO <color=green>DASH ATTACK</color>");
                            }
                            else
                                InGameConsole.LogFancy("We DO <color=orange>NOT DASH ATTACK</color>");
                            
                            LockSendingCommands();
                            if(numberOfPunchesPerformed % 2 == 0)
                            {
                                NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.Attack, dash_pos, punch_dir);
                            }
                            else
                            {
                                NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.Ability1, dash_pos, punch_dir);
                            }
                        }
                        else if(brainTimer > path_update_cd)
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
                                NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.SetTarget, pc.pv.ViewID);
                            }
                        }
                        else
                        {
                            LockSendingCommands();
                            NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.SetState, (byte)PadlaState.Idle);                        
                        }
                    }
                }
                
                break;
            }
            case(PadlaState.Attacking1):
            {
                brainTimer += dt;
                
                if(canSendCommands)
                {
                    if(brainTimer > punch1_duration)
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
                                NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.SetTarget, pc.pv.ViewID);
                            }
                        }
                        else
                        {
                            LockSendingCommands();
                            NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.SetState, (byte)PadlaState.Idle);
                        }
                        
                    }
                }
                break;
            }
            case(PadlaState.Attacking2):
            {
                break;
            }
            case(PadlaState.Airbourne):
            {
                Vector3 currentPos = thisTransform.localPosition;
                
                velocity.y += GRAVITY_Y * dt;
                velocity.y = Math.Clamp(-GRAVITY_MAX, GRAVITY_MAX, velocity.y);
                
                if(thisTransform.localPosition.y < -500 && canSendCommands)
                {
                    LockSendingCommands();
                    InGameConsole.LogOrange("Padla airbourne timeout!!!!!!!!");
                    NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.DieWithForce, new Vector3(0, 0, 0));
                }
                
                RaycastHit hit;
                
                float velMag = Math.Magnitude(velocity);
                Vector3 velDir = velocity.normalized;
                
                Vector3 capsulePBottom = GetCapsulePointBottom();
                Vector3 capsulePTop = GetCapsulePointTop();
                
                gizmoP1 = capsulePBottom;
                gizmoP2 = capsulePTop;
                
                float capsuleRadius = col.radius;
                
                if(velMag > 0)
                {
                    if(Physics.CapsuleCast(capsulePBottom, capsulePTop, capsuleRadius, velDir, out hit, velMag * dt, groundMask))
                    {
                        float dot = Vector3.Dot(hit.normal, velDir);
                        velocity = velocity + hit.normal * velMag * Math.Abs(dot);
                        
                        velMag = Math.Magnitude(velocity);
                        
                        if(velMag > 0.1f)
                        {
                            if(Physics.CapsuleCast(capsulePBottom, capsulePTop, capsuleRadius, velDir, velMag * dt, groundMask))
                            {
                                velocity.x = velocity.y = velocity.z = 0;
                                
                                NavMeshHit _navMeshHit;
                                Vector3 samplePos = thisTransform.localPosition;
                                if(NavMesh.SamplePosition(samplePos, out _navMeshHit, 0.66f, NavMesh.AllAreas))
                                {
                                    if(canSendCommands)
                                    {
                                        LockSendingCommands();
                                      //  InGameConsole.LogOrange("<color=green>Sending LandOnGround() </color>");
                                        NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.LandOnGround, _navMeshHit.position);
                                    }    
                                }
                                else
                                {
                                    InGameConsole.LogFancy(string.Format("{0}: Double capsule cast hit!", this.gameObject.name));
                                }
                            }
                        }
                        else
                        {
                            //InGameConsole.LogOrange("Magnitude of vel is " + velMag.ToString());
                            NavMeshHit navMeshHit;
                            if(NavMesh.SamplePosition(thisTransform.localPosition, out navMeshHit, 0.33f, NavMesh.AllAreas))
                            {
                                if(canSendCommands)
                                {
                                    LockSendingCommands();
                                    InGameConsole.LogOrange("<color=green>Sending LandOnGround() </color>");
                                    NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.LandOnGround, navMeshHit.position);
                                }
                            }
                        }
                    }
                    
                }
                
                Vector3 updatedPos = currentPos + velocity * dt;                
                thisTransform.localPosition = updatedPos;
                
                
                break;
            }
            case(PadlaState.Dead):
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
    
    //Airbourne variables:
    public Vector3 velocity;
    const float GRAVITY_Y = Globals.NPC_gravity;//-9.8F;
    const float GRAVITY_MAX = 50F;
    //float airbourneTimeStamp = 0;
    
    void Punch1_L()
    {
        numberOfPunchesPerformed++;
        anim.Play("Base.Punch1_L", 0, 0);
        //trail_arm.EmitFor(punch1_duration);
        trail_spine.EmitFor(punch1_duration);
        
        audio_src.clip = clipPunch1;
        audio_src.PlayDelayed(punch1_damageTimingStart);
    }
    
    void Punch1_R()
    {
        numberOfPunchesPerformed++;
        anim.Play("Base.Punch1_R", 0, 0);
        //trail_arm.EmitFor(punch1_duration);
        trail_spine.EmitFor(punch1_duration);
        
        audio_src.clip = clipPunch1;
        audio_src.PlayDelayed(punch1_damageTimingStart);
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
        if(state == PadlaState.Airbourne)
        {
            string s = string.Format("Velocity: <color=green>{0}</color>", velocity);
            UnityEditor.Handles.Label(transform.localPosition + new Vector3(0, 2.55f, 0), s, style);
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(gizmoP1, col.radius);
            Gizmos.DrawWireSphere(gizmoP2, col.radius);
            Gizmos.DrawLine(gizmoP1, gizmoP2);
        }
    }
#endif
    
    const float animDampTime = 0.05F;
    
    
    
    
    const float footStepDistance = 1.4f;
    float distanceTravelledRunningSqr;
    
    void UpdateBrainLocally(float dt)
    {
        switch(state)
        {
            case(PadlaState.Idle):
            {
                break;
            }
            case(PadlaState.Chasing):
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
            case(PadlaState.Attacking1):
            {
                Vector3 currentPos = thisTransform.localPosition;
                float dV = speedMult * dt * moveSpeed;
                
                attack_timer += dt;
                
                Vector3 attackDashDirXZ = Math.GetXZ(attackDashDir);
                // if(target_pc)
                // {
                //     shootingDirectionXZ = Math.GetXZ(target_pc.GetGroundPosition() - thisTransform.localPosition);
                // }
                
                Quaternion desiredRotation = Quaternion.LookRotation(attackDashDirXZ);
                
                thisTransform.localRotation = Quaternion.RotateTowards(thisTransform.localRotation, desiredRotation, dt * 2080);
                
                
                RotateToLookAt(dashPos, rotateTime * speedMult / 4, false);
                
                if(attack_timer > punch1_damageTimingStart)
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
                    
                    if(attack_timer > punch1_damageTimingStart && attack_timer < punch1_damageTimingEnd)
                    {
                        Vector3 dmgPos = thisTransform.localPosition;
                        dmgPos = dmgPos + thisTransform.up * localStrikeOffset.y + thisTransform.forward * localStrikeOffset.z;
                        float dmgRadius = punch1_radius;
                        
                        if(TryDoMeleeDamageToLocalPlayer(dmgPos, dmgRadius))
                        {
                            //InGameConsole.LogFancy(string.Format("Did melee damage at {0}", sword_attack1_damageTimingStart));
                            //attack_timer = 0;
                            canDoMeleeDamageToLocalPlayer = false;    
                        }
                    }
                    
                }
                
                break;
            }
            case(PadlaState.Attacking2):
            {
                break;
            }
            case(PadlaState.Airbourne):
            {
                if(PhotonNetwork.IsMasterClient)
                {
                    return;
                }
                
                Vector3 currentPos = thisTransform.localPosition;
                
                velocity.y += GRAVITY_Y * dt;
                velocity.y = Math.Clamp(-GRAVITY_MAX, GRAVITY_MAX, velocity.y);
                
                
                
                RaycastHit hit;
                
                float velMag = Math.Magnitude(velocity);
                Vector3 velDir = velocity.normalized;
                
                Vector3 capsulePBottom = GetCapsulePointBottom();
                Vector3 capsulePTop = GetCapsulePointTop();
                
                float capsuleRadius = col.radius;
                
                if(velMag > 0)
                {
                    if(Physics.CapsuleCast(capsulePBottom, capsulePTop, capsuleRadius, velDir, out hit, velMag * dt, groundMask))
                    {
                        float dot = Vector3.Dot(hit.normal, velDir);
                        velocity = velocity + hit.normal * velMag * Math.Abs(dot);
                        
                        velMag = Math.Magnitude(velocity);
                        
                        if(velMag > 0)
                        {
                            //if(Physics.CapsuleCast(capsulePBottom, capsulePTop, capsuleRadius, velDir, out hit, velMag * dt, groundMask))
                            if(Physics.CapsuleCast(capsulePBottom, capsulePTop, capsuleRadius, velDir, velMag * dt, groundMask))
                            {
                                velocity.x = velocity.y = velocity.z = 0;
                                InGameConsole.LogFancy(string.Format("{0}: Double capsule cast hit!", this.gameObject.name));
                            }
                        }
                    }
                }
                Vector3 updatedPos = currentPos + velocity * dt;
                thisTransform.localPosition = updatedPos;
                                
                break;
            }
            case(PadlaState.Dead):
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
                local_pc.BoostVelocity(dmgDir * 5);
                
                local_pc.TakeDamage(punch1_dmg);
                return true;
            }
        }
        
        return false;
    }
    
    [Header("Clips:")]
    public AudioClip clipStep;
    public AudioClip clipHurt1;
    public AudioClip clipDeath;
    public AudioClip clipLaunchedAirborne;
    public AudioClip clipPunch1;
}
