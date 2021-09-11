using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

public enum SinclaireState : byte
{
    Idle,
    Chasing,
    SwordAttacking1,
    Firing1,
    Dead
}

public class SinclaireController : MonoBehaviour, INetworkObject, IDamagableLocal, IKillableThing
{
    public GameObject remoteAgent_prefab;
    NavMeshAgent remoteAgent;
    Transform remoteAgentTransform;
    
    Animator anim;
    public AudioSource audio_src;
    public AudioSource audio_src_feet;
    NetworkObject net_comp;
    Transform thisTransform;
    CapsuleCollider col;
    
    
    Rigidbody[] joint_rbs;
    //CharacterJoint[] joints;
    
    public SinclaireState state;
    
    //public TrailRendererController sword_tr_controller;
    public SinclaireSword sword;
    
    public ParticleSystem[] details_ps;
    
    
    void InitJoints()
    {
      //  joints = GetComponentsInChildren<CharacterJoint>();
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
    
    public ParticleSystem dash_ps;
                
    static int MoveSpeedHash = -1;
    static int groundMask = -1;
    
    DamagableLimb[] limbs;
    
    SpawnedObject spawnedObjectComp;
    
    void Awake()
    {
        
        instance_id = GetInstanceID();
        spawnedObjectComp = GetComponent<SpawnedObject>();
        InitJoints();
        DisableSkeleton();
        
        col = GetComponent<CapsuleCollider>();
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
        
        GameObject shooterHelperGO = new GameObject("ShooterHelper");
        shooterHelper = shooterHelperGO.transform;
        shooterHelper.SetParent(thisTransform);
        
        shooterHelper.localRotation = Quaternion.identity;
        shooterHelper.localPosition = new Vector3(0, 0, 0);
        
        
        path_update_cd = PhotonNetwork.OfflineMode ? PATH_UPDATE_BASE / 2.5f : PATH_UPDATE_BASE;
    }
    
    void InitAsMaster()
    {
        remoteAgent = Instantiate(remoteAgent_prefab, thisTransform.localPosition, thisTransform.localRotation).GetComponent<NavMeshAgent>();
        remoteAgentTransform = remoteAgent.transform;
    }
    
    public Transform head;
    
    public Vector3 GetHitSpot()
    {
        return head.position;
    }
    
    public NetworkObject GetNetComp()
    {
        return net_comp;
    }
    
    public byte GetHitSpotLimbId()
    {
        return 7;
    }
    
    public bool CanBeBounceHit()
    {
        if(state != SinclaireState.Dead)
        {
            return true;
        }
        else
            return false;
    }
    
    void Start()
    {
        if(PhotonNetwork.IsMasterClient)
        {
            InitAsMaster();    
        }
        
        NPCManager.RegisterKillable(this);
        
        AudioManager.AddEnemiesAlive();
        
        SetMovePos(thisTransform.localPosition);
        WarpRemoteAgent(thisTransform.localPosition);
    }
    
    public bool canSendCommands = true;
    
    float lockSendingCommandsTimeStamp;
    
    void LockSendingCommands()
    {
        lockSendingCommandsTimeStamp = Time.time;
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
                        SetState(SinclaireState.Chasing);
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
                
                Vector3 attackPosEnd = (Vector3)args[0];
                
                brainTimer = 0;
                sword_attack_timer = 0;
                
                RotateToLookAt(currentDestination, 0.0F);
                SetMovePos(attackPosEnd);
                WarpRemoteAgent(attackPosEnd);
                UpdateRemoteAgentDestination(attackPosEnd);
                
                DoSwordAttack1();
                SetState(SinclaireState.SwordAttacking1);
                
                break;
            }
            case(NetworkCommand.Shoot):
            {
                UnlockSendingCommands();
                
                Vector3 shootPos = (Vector3)args[0];
                pos_to_fire_at = (Vector3)args[1];
                
                
                
                brainTimer = 0;
                firing_timer = 0;
                canFire = true;
                // anim.SetTrigger("FireTrigger");
                anim.Play("Base.Fire", 0, 0);
                
                shotsNumber++;
                
                SetMovePos(shootPos);
                UpdateRemoteAgentDestination(shootPos);
                WarpRemoteAgent(shootPos);
                
                SetState(SinclaireState.Firing1);
                
                break;
            }
            case(NetworkCommand.SetState):
            {
                UnlockSendingCommands();
                byte _state = (byte)args[0];
                
                SetState((SinclaireState)_state);
                
                break;
            }
            case(NetworkCommand.TakeDamageExplosive):
            {
                int incomingDamage = (int)args[0];
                
                int small_healing_times = incomingDamage / UberManager.HEALING_DMG_THRESHOLD;
                HealthCrystalSmall.MakeSmallHealing(thisTransform.localPosition + new Vector3(0, 2.5f, 0), small_healing_times);
                
                TakeDamageExplosive(incomingDamage);
                
                break;
            }
            case(NetworkCommand.TakeDamageLimbNoForce):
            {
                int incomingDamage = (int)args[0];
                
                int small_healing_times = incomingDamage / UberManager.HEALING_DMG_THRESHOLD;
                HealthCrystalSmall.MakeSmallHealing(thisTransform.localPosition + new Vector3(0, 2.5f, 0), small_healing_times);
                
                byte limb_id = (byte)args[1];
                TakeDamage(incomingDamage, limb_id);
                //TakeDamageForce(incomingDamage, force, limb_id);
                
                break;
            }
            case(NetworkCommand.TakeDamageLimbWithForce):
            {
                int incomingDamage = (int)args[0];
                
                int small_healing_times = incomingDamage / UberManager.HEALING_DMG_THRESHOLD;
                HealthCrystalSmall.MakeSmallHealing(thisTransform.localPosition + new Vector3(0, 2.5f, 0), small_healing_times);
                
                Vector3 force = (Vector3)args[1];
                byte limb_id = (byte)args[2];
                TakeDamageForce(incomingDamage, force, limb_id);
                
                break;
            }
            
            default:
            {
                break;
            }
        }
    }
    
    //const float footStepDistance = 1.25f;
    float distanceTravelledRunning;
    
    void SetTarget(PlayerController target)
    {
        target_pc = target;
    }
    
    void SetState(SinclaireState _state)
    {
        if(state == SinclaireState.Dead)
        {
            return;
        }
        
        if(current_qts)
        {
            Destroy(current_qts);
        }
        
        switch(_state)
        {
            case(SinclaireState.Chasing):
            {
                distanceTravelledRunning = 0;
                brainTimer = path_update_cd;
                //anim.SetLayerWeight(1, 1);
                break;
            }
            case(SinclaireState.Idle):
            {
                distanceTravelledRunning = 0;
                //anim.SetLayerWeight(1, 1);
                break;
            }
            case(SinclaireState.SwordAttacking1):
            {
                distanceTravelledRunning = 0;
                canDoMeleeDamageToLocalPlayer = true;
                break;
            }
            default:
            {
                break;
            }
        }
        
        state = _state;
    }
    
    
    
    void DropHealthCrystals()
    {
        HealthCrystal hc = ObjectPool.s().Get(ObjectPoolKey.HealthCrystal).GetComponent<HealthCrystal>();
        hc.Launch(this.transform.localPosition + new Vector3(0, 1.25f, 0), 10);
        
        hc = ObjectPool.s().Get(ObjectPoolKey.HealthCrystal).GetComponent<HealthCrystal>();
        hc.Launch(this.transform.localPosition + new Vector3(0, 1.25f, 0), 10);
        
        hc = ObjectPool.s().Get(ObjectPoolKey.HealthCrystal).GetComponent<HealthCrystal>();
        hc.Launch(this.transform.localPosition + new Vector3(0, 1.25f, 0), 10);
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
    
    const float PATH_UPDATE_BASE = 0.15F;
    float path_update_cd;
    float brainTimer = 0;
    
    void WarpRemoteAgent(Vector3 pos)
    {
        if(remoteAgent)
        {
            NavMeshHit navMeshHit;
            NavMesh.SamplePosition(pos, out navMeshHit, 1, NavMesh.AllAreas);
            remoteAgent.ResetPath();
            remoteAgent.Warp(navMeshHit.position);
        }
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
    
    
    public float sword_attack_timer = 0;
    bool canDoMeleeDamageToLocalPlayer = true;
    const float sword_attack1_duration = 4.0F / 3.5f;
    
    const float sword_attack1_damageTimingStart = 0.28F / 2;
    //const float sword_attack1_damageTimingEnd = 1.1F / 4;
    
    //const float sword_attack1_distance = 2F;
    const float sword_attack1_radius = 1.55F;
    const int sword_attack1_dmg = 25;
    
    
    const float firing_duration = 2 / 2;
    const int firing_damage_per_proj = 25;
    const float firing_timing = 1F / 2;
    const float firing_cooldown = 4.5F;
    const float firing_distance = 144F;
    
    public float firing_masterTimer;
    
    float firing_timer = 0;
    bool canFire = false;
    
    Vector3 pos_to_fire_at;
    
    
    
    Vector3 gizmoPos;
    float gizmoRadius;
    
    Vector3 destinationGizmo;
    
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(gizmoPos, gizmoRadius);
        
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(destinationGizmo, 0.4f);
    }
    
    bool TryDoMeleeDamageToLocalPlayer(Vector3 pos, float radius)
    {
        //InGameConsole.LogFancy(string.Format("Sinclaire: <color=yellow>{0}</color>", "TryDoMeleeDamageToLocalPlayer"));
        PlayerController local_pc = PhotonManager.GetLocalPlayer();
        
        gizmoPos = pos;
        gizmoRadius = radius;
        
        if(local_pc && local_pc.CanTakeDamageFromProjectile())
        {
            float sqrDistanceToPlayer = Math.SqrDistance(local_pc.GetGroundPosition() + new Vector3(0, 0.5f, 0), pos);
            
            //InGameConsole.LogFancy(string.Format("Distance to player: <color=yellow>{0}</color>", Mathf.Sqrt(sqrDistanceToPlayer).ToString("f")));
            
            if(sqrDistanceToPlayer < radius * radius)
            {
                //Don't boost velocity if it is regular attack - it is annoying
                Vector3 dmgDir = -thisTransform.localPosition + local_pc.GetGroundPosition();
                dmgDir.y = 0;
                dmgDir.Normalize(); 
                
                dmgDir.y = 1;
                if(local_pc.CanTakeDamageFromProjectile())
                {
                    if(Math.SqrMagnitude(local_pc.fpsVelocity) < 6f * 6f)
                        local_pc.BoostVelocity(dmgDir * 14);    
                    local_pc.TakeDamage(sword_attack1_dmg);
                }
                return true;
            }
        }
        return false;
    }
    
    Transform shooterHelper;
    
    const int daggerDamage = 30;
    
    const float daggerSpeed = 36;
    
    const float daggerRadius = 0.55f;
    
    public void ShootTriple(Vector3 shootPos, Vector3 shootDir)
    {
        //shooterHelper.localRotation = Quaternion.LookRotation(shootDir, Vector3.up);
        
        audio_src.PlayOneShot(clipFireShot);
        
        shooterHelper.forward = shootDir;
        
        Vector3 forwardDir = shootDir;
        Vector3 rightDir = forwardDir + shooterHelper.right * 0.15f;
        rightDir.Normalize();
        Vector3 leftDir = forwardDir - shooterHelper.right * 0.15f;
        leftDir.Normalize();
        
        //rightDir = shooterHelper.InverseTransformDirection(rightDir);
        //leftDir = shooterHelper.InverseTransformDirection(leftDir);
        
        GameObject dagger_go;
        FlyingDagger dagger;
        
        bool isMine = PhotonNetwork.IsMasterClient;
        
        dagger_go = ObjectPool2.s().Get(ObjectPoolKey.FlyingDagger1, false);
        
        int owner_id = col.GetInstanceID();
        
        dagger = dagger_go.GetComponent<FlyingDagger>();
        dagger.LaunchSinclaireDagger(shootPos, forwardDir, daggerSpeed, daggerRadius, daggerDamage, isMine, owner_id);
        dagger.nailedExplosionDelay = 2.25f;
        
        dagger_go = ObjectPool2.s().Get(ObjectPoolKey.FlyingDagger1, false);
        
        dagger = dagger_go.GetComponent<FlyingDagger>();
        dagger.LaunchSinclaireDagger(shootPos, rightDir, daggerSpeed, daggerRadius, daggerDamage, isMine, owner_id);
        dagger.nailedExplosionDelay = 2.0f;
        
        dagger_go = ObjectPool2.s().Get(ObjectPoolKey.FlyingDagger1, false);
        
        dagger = dagger_go.GetComponent<FlyingDagger>();
        dagger.LaunchSinclaireDagger(shootPos, leftDir, daggerSpeed, daggerRadius, daggerDamage, isMine, owner_id);
        dagger.nailedExplosionDelay = 2.5f;
        
        ParticlesManager.PlayPooled(ParticleType.daggerShot_ps, shootPos + shootDir*1.0f, forwardDir);
    }
    
    int shotsNumber = 0;
    
    public void ShootSingle(Vector3 shootPos, Vector3 shootDir)
    {
        //shooterHelper.localRotation = Quaternion.LookRotation(shootDir, Vector3.up);
        
        audio_src.PlayOneShot(clipFireShot);
        
        shooterHelper.forward = shootDir;
        
        Vector3 forwardDir = shootDir;
        Vector3 rightDir = forwardDir + shooterHelper.right * 0.15f;
        rightDir.Normalize();
        Vector3 leftDir = forwardDir - shooterHelper.right * 0.15f;
        leftDir.Normalize();
        
        //rightDir = shooterHelper.InverseTransformDirection(rightDir);
        //leftDir = shooterHelper.InverseTransformDirection(leftDir);
        
        GameObject dagger_go;
        FlyingDagger dagger;
        
        bool isMine = PhotonNetwork.IsMasterClient;
        
        dagger_go = ObjectPool2.s().Get(ObjectPoolKey.FlyingDagger1, false);
        
        int owner_id = col.GetInstanceID();
        
        dagger = dagger_go.GetComponent<FlyingDagger>();
        dagger.LaunchSinclaireDagger(shootPos, forwardDir, daggerSpeed, daggerRadius, daggerDamage, isMine, owner_id);
        dagger.nailedExplosionDelay = 1.25f;
        
        
        ParticlesManager.PlayPooled(ParticleType.daggerShot_ps, shootPos + shootDir*1.0f, forwardDir);
    }
    
    
    Vector3 attack1_dashEndPos;
    void DoSwordAttack1_withDash(Vector3 dashEndPos)
    {
        anim.Play("Base.Sword_Swing1", 0, 0);
        
        
        sword.OnSwing();
    }
    
    void DoSwordAttack1()
    {
        anim.Play("Base.Sword_Swing1", 0, 0);
        audio_src.PlayOneShot(clipSwordSwing1, 1f);
        sword.OnSwing();
        dash_ps.Play();
    }
    
    
    public bool inRange1;
    public bool capsuleHit;
    public bool inRange2;
    public float inRange2_range;
    
    float changeTargetTimer;
    const float changeTargetCooldown = 4.5f;
    
    bool IsTargetValid()
    {
        if(target_pc && target_pc.isAlive)
        {
            return true;
        }
        else
            return false;
    }
    
    void UpdateBrain(float dt)
    {
        if(!canSendCommands)
        {
            if(Time.time - lockSendingCommandsTimeStamp > 1)
            {
                UnlockSendingCommands();
            }
        }
        
        switch(state)
        {
            case(SinclaireState.Idle):
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
            case(SinclaireState.Chasing):
            {
                if(IsTargetValid())
                {
                    brainTimer += dt;
                    firing_masterTimer += dt;
                    Vector3 targetGroundPos = target_pc.GetGroundPosition();
                    
                    UpdateRemoteAgentDestination(targetGroundPos);
                   
                    if(canSendCommands)
                    {
                        changeTargetTimer += dt;
                        if(changeTargetTimer > changeTargetCooldown)
                        {
                            changeTargetTimer = 0;
                            if(canSendCommands)
                            {
                                Transform potentialTarget = ChooseTargetClosest(thisTransform.localPosition);
                                if(potentialTarget && (potentialTarget.GetInstanceID() != target_pc.thisTransform.GetInstanceID()))
                                {
                                    PlayerController pc = potentialTarget.GetComponent<PlayerController>();
                                    if(pc)
                                    {
                                        LockSendingCommands();
                                        NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.SetTarget, pc.pv.ViewID);
                                        break;
                                    }
                                }
                            }
                        }
                        
                        Vector3 sinclaireGroundPosition = thisTransform.localPosition;
                        
                        inRange1 = Math.SqrDistance(targetGroundPos, sinclaireGroundPosition) < dashDistanceCheck * dashDistanceCheck;
                        if(inRange1)
                        {
                            NavMeshHit navMeshHit;
                            //Vector3 punch_dir = (Math.GetXZ(targetGroundPos - padlaPosition)).normalized;
                            Vector3 punch_dir = (targetGroundPos - sinclaireGroundPosition).normalized;
                            
                            Vector3 dash_pos = sinclaireGroundPosition;
                            RaycastHit hit;
                            Vector3 capsuleBottom = NPCTool.GetCapsuleBottomPoint(thisTransform, col);
                            Vector3 capsuleTop = NPCTool.GetCapsuleTopPoint(thisTransform, col);
                            
                            if(Physics.SphereCast(capsuleBottom, col.radius, punch_dir, out hit, dashDistance, groundMask))
                            {
                             //   InGameConsole.LogFancy("Hit something");
                                if(NavMesh.SamplePosition(sinclaireGroundPosition + punch_dir * dashDistance, out navMeshHit, 1f, NavMesh.AllAreas))
                                {
                                    dash_pos = navMeshHit.position;
                                    // InGameConsole.LogFancy("We DO <color=green>DASH ATTACK</color>");
                                }
                            }
                            else
                            {
                                if(NavMesh.SamplePosition(sinclaireGroundPosition + punch_dir * dashDistance, out navMeshHit, 1f, NavMesh.AllAreas))
                                {
                                    dash_pos = navMeshHit.position;
                                    // InGameConsole.LogFancy("We DO <color=green>DASH ATTACK</color>");
                                }
                            }
                            
                            
                            
                            LockSendingCommands();
                            NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.Attack, dash_pos);
                            
                            // Vector3 _attackPosStart = thisTransform.localPosition;
                            // Vector3 _attackDir = (targetGroundPos - thisTransform.localPosition).normalized;
                            // Vector3 _attackPosEnd = GetDashPosition(_attackPosStart + _attackDir * dashDistance, sword_attack1_distance,  col);
                            // inRange2_range = Math.SqrDistance(targetGroundPos, targetGroundPos);
                            // inRange2 = Math.SqrDistance(targetGroundPos, targetGroundPos) < sword_attack1_distance * sword_attack1_distance;
                            
                            // if(inRange2)
                            // {
                            //     LockSendingCommands();
                            //     NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.Attack, _attackPosStart, _attackPosEnd);
                            // }
                        }
                        else if(firing_masterTimer >= firing_cooldown)
                        {
                            Vector3 sinclaireOffsettedPos = thisTransform.localPosition + new Vector3(0, 2, 0);
                            Vector3 targetPos = target_pc.GetCenterPosition() + new Vector3(0, 0.3f, 0);
                            
                            if(Math.SqrDistance(targetPos, sinclaireOffsettedPos) < firing_distance * firing_distance)
                            {
                                float distance = Math.Magnitude(targetPos - sinclaireOffsettedPos);
                                Vector3 shootDir = (targetPos - sinclaireOffsettedPos).normalized;
                                Ray ray = new Ray(sinclaireOffsettedPos, shootDir);
                                if(!Physics.SphereCast(ray, 0.3f, distance, groundMask))
                                {
                                    LockSendingCommands();
                                    Vector3 shootPos = currentDestination;
                                    
                                    NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.Shoot, shootPos, targetPos);
                                    firing_masterTimer = 0;
                                }
                                else
                                {
                                    firing_masterTimer = firing_cooldown * 0.75f;
                                }
                            }
                            else
                            {
                                firing_masterTimer = firing_cooldown * 0.75f;
                            }
                        }
                        else if(brainTimer > path_update_cd)
                        {
                            brainTimer = 0;
                            
                            Vector3 movePos = GetRemoteAgentPos();
                            if(Math.SqrDistance(thisTransform.localPosition, movePos) > 0.175F * 0.175F)
                            {
                                LockSendingCommands();
                                // NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.Move, movePos);
                                NetworkObjectsManager.CallNetworkFunctionUnreliable(net_comp.networkId, NetworkCommand.Move, movePos);
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
                    }
                }
                
                
                break;
            }
            case(SinclaireState.SwordAttacking1):
            {
                brainTimer += dt;
                
                if(canSendCommands)
                {
                    if(brainTimer > sword_attack1_duration)
                    {
                        LockSendingCommands();
                        brainTimer = 0;
                        Transform potentialTarget = ChooseTargetClosest(thisTransform.localPosition);
                        if(potentialTarget)
                        {
                            PlayerController pc = potentialTarget.GetComponent<PlayerController>();
                            if(pc)
                            {
                                NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.SetTarget, pc.pv.ViewID);
                            }
                        }
                        else
                        {
                            NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.SetState, (byte)SinclaireState.Idle);
                        }
                    }
                }
                
                break;
            }
            case(SinclaireState.Firing1):
            {
                brainTimer += dt;
                
                if(canSendCommands)
                {
                    if(brainTimer > firing_duration)
                    {
                        LockSendingCommands();
                        brainTimer = 0;
                        Transform potentialTarget = ChooseTargetClosest(thisTransform.localPosition);
                        if(potentialTarget)
                        {
                            PlayerController pc = potentialTarget.GetComponent<PlayerController>();
                            if(pc)
                            {
                                NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.SetTarget, pc.pv.ViewID);
                            }
                        }
                        else
                        {
                            NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.SetState, (byte)SinclaireState.Idle);
                        }
                    }
                }
                
                break;
            }
            case(SinclaireState.Dead):
            {
                break;
            }
        }
    }
    
    static readonly Vector3 vUp = new Vector3(0, 1, 0);
    Quaternion deriv;
    
    Vector3 currentDestination;
    
    const float moveSpeed = 6.5F;//;0.738248f * 2;//7F;
    const float attackingMoveSpeed = 36F;
    
    const float rotateTime = 0.2F;
    float speedMult = 1;
    
    void SetMovePos(Vector3 pos)
    {
        currentDestination = pos;
    }
    
    const float rotationDistanceEpsilon = 0.01F;
    
    void RotateToLookAt(Vector3 lookAtPointXZ, float timeToRotate)
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
            if(state != SinclaireState.SwordAttacking1 && target_pc)
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
    
    const float animDampTime = 0.1F;
    
    public void animEvent()
    {
        
        //InGameConsole.LogFancy(string.Format("AnimEvent(): sword_attack_timer is <color=yellow>{0}</color>", sword_attack_timer.ToString("f")));
    }
    
    public void animEnd()
    {
        //InGameConsole.LogFancy(string.Format("AnimEnd(): sword_attack_timer is <color=yellow>{0}</color>", sword_attack_timer.ToString("f")));
    }
    
    
    Vector3 localStrikeOffset = new Vector3(0, 1.25f, 1.0f);
    
    int instance_id;
    public Transform qts_transform;
    public QuickTimeSphere current_qts;
    
    void UpdateBrainLocally(float dt)
    {
        switch(state)
        {
            case(SinclaireState.Chasing):
            {
                Vector3 currentPos = thisTransform.localPosition;
                
                float dV = speedMult * dt * moveSpeed;
                thisTransform.localPosition = Vector3.MoveTowards(currentPos, currentDestination, dV);
                
                RotateToLookAt(currentDestination, rotateTime * speedMult);
                
                
                Vector3 dPos = new Vector3();//currentDestination - currentPos;
                dPos.x = currentDestination.x - currentPos.x;
                dPos.y = currentDestination.y - currentPos.y;
                dPos.z = currentDestination.z - currentPos.z;
                
                distanceTravelledRunning += Math.Magnitude(dPos);
                
                const float footStepDistance = 1.25f * 3;
                
                if(distanceTravelledRunning > footStepDistance)
                {
                    distanceTravelledRunning = 0;
                    
                    audio_src_feet.pitch = Random.Range(0.9f, 1.1f);
                    audio_src_feet.PlayOneShot(clipStep, 0.5F);
                }
                
                anim.SetFloat(MoveSpeedHash, Math.Magnitude(dPos), animDampTime, dt);
                
                break;
            }
            case(SinclaireState.SwordAttacking1):
            {
                Vector3 currentPos = thisTransform.localPosition;
                float dV = speedMult * dt * attackingMoveSpeed;
                
                if(sword_attack_timer < sword_attack1_damageTimingStart)
                    dV = 0;
                
                destinationGizmo = currentDestination;
                thisTransform.localPosition = Vector3.MoveTowards(currentPos, currentDestination, dV);
                
                if(UberManager.CanMakeQTS(instance_id))
                {
                    current_qts = UberManager.MakeQTS(instance_id, thisTransform, qts_transform.position, QuickTimeType.Default, 0.75f, 900, 0.25f);
                }   
                
                RotateToLookAt(currentDestination, 0.08F);
                
                sword_attack_timer += dt;
                if(canDoMeleeDamageToLocalPlayer)
                {
                    //if(sword_attack_timer > sword_attack1_damageTimingStart && sword_attack_timer < sword_attack1_damageTimingEnd)
                    float sqrDistanceToDestination = Math.SqrDistance(currentDestination, currentPos);
                    //if(sword_attack_timer > sword_attack1_damageTimingStart && dV > 0 && sqrDistanceToDestination > 0f)
                    if(sword_attack_timer > sword_attack1_damageTimingStart)
                    {
                        Vector3 dmgPos = thisTransform.localPosition;
                        dmgPos = dmgPos + thisTransform.up * localStrikeOffset.y + thisTransform.forward * localStrikeOffset.z;// + thisTransform.forward * localStrikeOffset.z;
                        float dmgRadius = sword_attack1_radius;
                        if(TryDoMeleeDamageToLocalPlayer(dmgPos, dmgRadius))
                        {
                            //InGameConsole.LogFancy(string.Format("Did melee damage at {0}", sword_attack1_damageTimingStart));
                            sword_attack_timer = 0;
                            canDoMeleeDamageToLocalPlayer = false;    
                        }
                    }
                }
                
                
                break;
            }
            case(SinclaireState.Firing1):
            {
                Vector3 currentPos = thisTransform.localPosition;
                
                float dV = speedMult * dt * moveSpeed;
                thisTransform.localPosition = Vector3.MoveTowards(currentPos, currentDestination, dV);
                RotateToLookAt(pos_to_fire_at, rotateTime * speedMult);
                
                firing_timer += dt;
                
                if(canFire)
                {
                    if(firing_timer > firing_timing)
                    {
                        canFire = false;
                        Vector3 shootPosOffsetted = currentDestination + new Vector3(0, 2f, 0);
                        Vector3 _shootDir = (pos_to_fire_at - shootPosOffsetted).normalized; 
                        if(shotsNumber % 2 == 0)
                            ShootTriple(shootPosOffsetted, _shootDir);
                        else
                            ShootSingle(shootPosOffsetted, _shootDir);
                    }
                    else
                    {
                        if(firing_timer > 0.25f)
                        {
                            if(UberManager.CanMakeQTS(instance_id))
                            {
                                current_qts = UberManager.MakeQTS(instance_id, thisTransform, qts_transform.position, QuickTimeType.Default, 0.75f, 900, 0.25f);
                            }       
                        }
                    }
                }
                
                break;                
            }
            default:
            {
                break;
            }
        }
    }
    
    
    const int MaxHealth = 2500;
    int HitPoints = MaxHealth;
    
    void TakeDamage(int dmg, byte limb_id)
    {
        InGameConsole.LogOrange("TakeDamage()");
        HitPoints -= dmg;
         
        if(HitPoints <= 0)
        {
            Die(Vector3.zero, limb_id);
            HitPoints = 0;
        }
    }
    
    void TakeDamageForce(int dmg, Vector3 force, byte limb_id)
    {
        //InGameConsole.LogOrange("TakeDamageForce()");
        HitPoints -= dmg;
         
        if(HitPoints <= 0)
        {
            Die(force, limb_id);
            HitPoints = 0;
        }
    }
    
    void TakeDamageExplosive(int dmg)
    {
        //InGameConsole.LogOrange("TakeDamageExplosive()");
        HitPoints -= dmg;
        if(HitPoints <= 0)
        {
            DieFromExplosion();
            HitPoints = 0;
        }
    }
    
    void DieFromExplosion()
    {
        if(state == SinclaireState.Dead)
        {
            return;
        }
        
        AudioManager.RemoveEnemiesAlive();
        
        //InGameConsole.LogOrange("DieFromExplosion()");
        
        SetState(SinclaireState.Dead);
        
        if(spawnedObjectComp)
            spawnedObjectComp.OnObjectDied();
        
        HitPoints = -1;
        
        anim.enabled = false;
        col.enabled = false;
        
        NetworkObjectsManager.UnregisterNetObject(net_comp);
        if(remoteAgent)
            Destroy(remoteAgent.gameObject, 0.1f);
            
        
        
        audio_src.PlayOneShot(clipDeath);
        HitPoints = 0;
        
        EnableSkeleton();  
        AudioManager.Play3D(SoundType.death_impact_gib_distorted, thisTransform.localPosition);
        
        int len = limbs.Length;
        for(int i = 0; i < len; i++)
        {
            limbs[i].MakeLimbDead();
        }
        int limb_to_destroy = 3;
        
        
        if(limb_to_destroy != 0)
        {
            if(limbs != null)
            {
                for(int i = 0; i < len; i++)
                {
                    if(limbs[i].limb_id == limb_to_destroy)
                    {
                        //Vector3 f = force;
                        //InGameConsole.LogOrange(string.Format("Apply force {0} to limb {1}", f, limb_to_destroy));
                        
                        //limbs[i].ApplyForceToAdjacentLimbs(f);
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
    
    void Die(Vector3 force, byte limb_to_destroy)
    {
        if(state == SinclaireState.Dead)
        {
            return;
        }
        
        AudioManager.RemoveEnemiesAlive();
        
        SetState(SinclaireState.Dead);
        
        if(spawnedObjectComp)
            spawnedObjectComp.OnObjectDied();
        
        sword.Drop();
        
        anim.enabled = false;
        col.enabled = false;
        
        NetworkObjectsManager.UnregisterNetObject(net_comp);
        if(remoteAgent)
            Destroy(remoteAgent.gameObject, 0.1f);
            
        audio_src.PlayOneShot(clipDeath, 1f);
        HitPoints = 0;
        
        
        EnableSkeleton();  
        AudioManager.Play3D(SoundType.death_impact_gib_distorted, thisTransform.localPosition);
        
        int len = limbs.Length;
        for(int i = 0; i < len; i++)
        {
            limbs[i].MakeLimbDead();
        }
        len = details_ps.Length;     
        for(int i = 0; i < len; i++)
        {
            details_ps[i].emissionRate = 0;
            Destroy(details_ps[i].gameObject, 2f);
        }
        
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
                        
                        //break;
                    }
                }
            }
        }
        DropHealthCrystals();
    }
    
    
    float damage_taken_timeStamp = 0;
    
    
    public void TakeDamageLocally(int dmg, Vector3 hitPos, Vector3 hitDir)
    {
        if(state != SinclaireState.Dead && damage_taken_timeStamp + 0.15F < Time.time)
        {
            float vol = Random.Range(0.8f, 1f);
            audio_src.PlayOneShot(clipHurt1, vol);
            
            damage_taken_timeStamp = Time.time;
        }
    }
    
    public int GetCurrentHP()
    {
        return HitPoints;
    }
    
    public bool IsDead()
    {
        if(state == SinclaireState.Dead)
            return true;
        else
            return false;
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
    
    const float dashDistanceCheck = 3.25F;
    const float dashDistance = 7.0F;
    
    
    
    public Vector3 GetDashPosition(Vector3 pos_to_check, float zeroDistance, CapsuleCollider capsule)
    {
        Vector3 Result = thisTransform.localPosition;
        
        Vector3 startDashPos = thisTransform.localPosition;
        Vector3 dashDirection = (pos_to_check - startDashPos).normalized;
        
        RaycastHit hit;
        
        NavMeshHit _navMeshHit;
        
        Vector3 capsuleBottom = NPCTool.GetCapsuleBottomPoint(thisTransform, col);
        Vector3 capsuleTop = NPCTool.GetCapsuleTopPoint(thisTransform, col);
        capsuleTop = Vector3.Lerp(capsuleBottom, capsuleTop, 0.33f);
        capsuleHit = Physics.CapsuleCast(capsuleBottom, capsuleTop, col.radius * 0.9f, dashDirection, out hit, dashDistance, groundMask);
        if(capsuleHit)
        {
            if(NavMesh.SamplePosition(hit.point, out _navMeshHit, 0.33f, NavMesh.AllAreas))
            {
                Result = _navMeshHit.position;
            }
            else
            {
                Result = thisTransform.localPosition;
            }
        }
        else
        {
            if(NavMesh.SamplePosition(pos_to_check, out _navMeshHit, 0.33f, NavMesh.AllAreas))
            {
                Result = _navMeshHit.position;
            }
            else
            {
                Result = thisTransform.localPosition;
            }
        }
        
        return Result;
    }
    
    [Header("Audio clips:")]
    public AudioClip clipHurt1;
    public AudioClip clipDeath;
    public AudioClip clipSwordSwing1;
    public AudioClip clipFireShot;
    public AudioClip clipStep;
    
    
}
