using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;
using System.Collections.Generic;

public enum SinclaireCoolState : byte
{
    Idle,
    Chasing,
    SwordAttacking1,
    Firing1,
    SpawningCool,
    Vanished,
    Slamming,
    Dead
}

public class SinclaireCoolController : MonoBehaviour, INetworkObject, IDamagableLocal, IBoss, IKillableThing
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
    //CharacterJoint[] joints;
    
    //public ParticleSystem hair;
    
    public SinclaireCoolState state;
    
    public GameObject shockwave_prefab;
    
    //public TrailRendererController sword_tr_controller;
    public SinclaireSword sword;
    
    public ParticleSystem[] details_ps;
    
    const float vanishDuration = 3f; 
    
    const int shockWaveDamage = 33;
    
    void MakeShockWave(Vector3 pos)
    {
        GameObject shockwave_obj = Instantiate(shockwave_prefab, new Vector3(1000, 1000, 1000), Quaternion.identity);
        shockwave_obj.GetComponent<Shockwave>().DoShockwave(pos, 4f, shockWaveDamage);
    }
    
    
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
    LimbForExplosions limb_for_explosions;
    
    public SkinnedMeshRenderer smr;
    public MeshRenderer dagger_rend;
    public MeshRenderer dagger_rend2;
    
    
    void GoVanish()
    {
        dagger_rend.enabled     = false;
        dagger_rend2.enabled    = false;
        smr.enabled             = false; 
        
        sword.Appear();
        sword.Hide();
        
        limb_for_explosions.canBeAffected = false;
        int len = limbs.Length;
        for(int i = 0; i < len; i++)
        {
            limbs[i].react = DamageReactType.no_react;
        }
        len = details_ps.Length;
        for(int i = 0; i < len; i++)
            details_ps[i].Stop();
    }
    
    void ReturnFromVanish()
    {
        dagger_rend.enabled  = true;
        dagger_rend2.enabled = true;
        smr.enabled          = true;
        
        limb_for_explosions.canBeAffected = true;
        int len = limbs.Length;
        for(int i = 0; i < len; i++)
        {
            
            limbs[i].react = DamageReactType.headShot_dmg;
        }
        sword.Show();
        len = details_ps.Length;
        for(int i = 0; i < len; i++)
            details_ps[i].Play();
    }
    
    void Awake()
    {
        instance_id = GetInstanceID();
        spawnedObjectComp = GetComponent<SpawnedObject>();
        InitJoints();
        DisableSkeleton();
        
        limb_for_explosions = GetComponent<LimbForExplosions>();
        
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
    
    public bool isBoss = true;
    
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
        if(state != SinclaireCoolState.Dead)
        {
            return true;
        }
        else
            return false;
    }
    
    void Start()
    {
        CalculatedMaxHealth = MaxHealth * PhotonNetwork.CurrentRoom.PlayerCount;
        HitPoints = CalculatedMaxHealth;
        
        if(PhotonNetwork.IsMasterClient)
        {
            InitAsMaster();    
        }
        
        NPCManager.RegisterKillable(this);
        
        if(UberManager.GetCurrentLevelIndex() == 2 && UberManager.Singleton().infernoCircle == 0) //If we are on level_Start
        {
            SetState(SinclaireCoolState.SpawningCool);
            anim.Play("Base.SpawningCool", 0, 0);
            anim.SetLayerWeight(1, 0);
            sword.Hide();
        }
        
        SetMovePos(thisTransform.localPosition);
        WarpRemoteAgent(thisTransform.localPosition);
    }
    
    
    
    public void Sw()
    {
        sword.Appear();
        audio_src.PlayOneShot(clipSwordAppear);
       
    }
    
    public float GetBossHitPointsPercents()
    {
        float Result = HitPoints / (CalculatedMaxHealth * 0.01f);
        return Result;
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
    
    public ParticleSystem slam_ps;
    
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
                        SetState(SinclaireCoolState.Chasing);
                    }
                }
                
                brainTimer = path_update_cd;
                
                break;
            }
            case(NetworkCommand.Ability1):
            {
                UnlockSendingCommands();
                //int viewID = (int)args[0];
                if(isBoss)
                {
                    BossHPBar.SetBoss(this.gameObject, "Sinclaire Cool");
                }
                
                anim.Play("Base.Idle", 0, 0);
                anim.SetLayerWeight(1, 1);  
                
                SetState(SinclaireCoolState.Idle);
                
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
                SetState(SinclaireCoolState.SwordAttacking1);
                
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
                
                SetMovePos(shootPos);
                UpdateRemoteAgentDestination(shootPos);
                WarpRemoteAgent(shootPos);
                
                SetState(SinclaireCoolState.Firing1);
                
                break;
            }
            case(NetworkCommand.SetState):
            {
                UnlockSendingCommands();
                byte _state = (byte)args[0];
                
                SetState((SinclaireCoolState)_state);
                
                break;
            }
            case(NetworkCommand.TakeDamageExplosive):
            {
                if(state == SinclaireCoolState.SpawningCool)
                {
                    return;
                }
                
                int incomingDamage = (int)args[0];
                
                int _incomingDamage = incomingDamage;
                if(_incomingDamage > HitPoints)
                    _incomingDamage = HitPoints;
                int small_healing_times = _incomingDamage / UberManager.HEALING_DMG_THRESHOLD;
                HealthCrystalSmall.MakeSmallHealing(thisTransform.localPosition + new Vector3(0, 2.5f, 0), small_healing_times);
                
                TakeDamageExplosive(incomingDamage);
                
                break;
            }
            case(NetworkCommand.TakeDamageLimbNoForce):
            {
                if(state == SinclaireCoolState.SpawningCool)
                {
                    return;
                }
                int incomingDamage = (int)args[0];
                
                int _incomingDamage = incomingDamage;
                if(_incomingDamage > HitPoints)
                    _incomingDamage = HitPoints;
                int small_healing_times = _incomingDamage / UberManager.HEALING_DMG_THRESHOLD;
                HealthCrystalSmall.MakeSmallHealing(thisTransform.localPosition + new Vector3(0, 2.5f, 0), small_healing_times);
                
                byte limb_id = (byte)args[1];
                TakeDamage(incomingDamage, limb_id);
                //TakeDamageForce(incomingDamage, force, limb_id);
                
                break;
            }
            case(NetworkCommand.TakeDamageLimbWithForce):
            {
                if(state == SinclaireCoolState.SpawningCool)
                {
                    return;
                }
                int incomingDamage = (int)args[0];
                
                int _incomingDamage = incomingDamage;
                if(_incomingDamage > HitPoints)
                    _incomingDamage = HitPoints;
                int small_healing_times = _incomingDamage / UberManager.HEALING_DMG_THRESHOLD;
                HealthCrystalSmall.MakeSmallHealing(thisTransform.localPosition + new Vector3(0, 2.5f, 0), small_healing_times);
                
                Vector3 force = (Vector3)args[1];
                byte limb_id = (byte)args[2];
                TakeDamageForce(incomingDamage, force, limb_id);
                
                break;
            }
            case(NetworkCommand.Ability2):
            {
                UnlockSendingCommands();
                
                GoVanish();
                SetState(SinclaireCoolState.Vanished);
                
                
                break;
            }
            case(NetworkCommand.Ability3):
            {
                UnlockSendingCommands();
                
                currentSlamPos = (Vector3)args[0];
                float slamHeight = (float)args[1];
                
                ReturnFromVanish();                
                thisTransform.localPosition = currentSlamPos + new Vector3(0, slamHeight, 0);
                if(remoteAgent)
                {
                    remoteAgent.enabled = false;
                }
                anim.Play("Base.Slam", 0, 0);
                slam_ps.Play();
                didSlam = false;
                
                SetState(SinclaireCoolState.Slamming);
                
                
                break;
            }
            default:
            {
                break;
            }
        }
    }
    
    const float slamSpeed = 72;
    Vector3 currentSlamPos;
    bool didSlam = false;
    
    
    
    public void S()
    {
        if(PhotonNetwork.IsMasterClient)
        {
            if(remoteAgent)
            {
                remoteAgent.enabled = true;
                WarpRemoteAgent(thisTransform.localPosition);
                UpdateRemoteAgentDestination(thisTransform.localPosition);
                
            }
            
            firing_masterTimer = 0.5f;
            slam_cooldown_timer = 0;
            if(canSendCommands && UberManager.readyToSwitchLevel)
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
                    NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.SetState, (byte)SinclaireCoolState.Idle);
                }
            }
        }
        //MakeShockWave(thisTransform.localPosition);
    }
    
    void SetTarget(PlayerController target)
    {
        target_pc = target;
    }
    
    void SetState(SinclaireCoolState _state)
    {
        if(state == SinclaireCoolState.Dead)
        {
            return;
        }
        
        if(current_qts)
        {
            Destroy(current_qts);
        }
        
        
        switch(_state)
        {
            case(SinclaireCoolState.Chasing):
            {
                brainTimer = path_update_cd / 2;
                break;
            }
            case(SinclaireCoolState.Idle):
            {
                break;
            }
            case(SinclaireCoolState.SwordAttacking1):
            {
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
        
        ref List<PlayerController> pcs = ref UberManager.Singleton().playerControllers;
        int len = pcs.Count;
        
        float minDistanceSqr = float.MaxValue;
        
        for(int i = 0; i < len; i++)
        {
            PlayerController potentialTarget = pcs[i];
            if(pcs[i].isAlive)
            {
                float distSqr = Math.SqrDistance(seekerPos, potentialTarget.GetGroundPosition());
                
                if(distSqr <  minDistanceSqr)
                {
                    minDistanceSqr = distSqr;
                    result = potentialTarget.thisTransform;
                }
            }
        }
        
        return result;
    }
    
    const float PATH_UPDATE_BASE = 0.1F;
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
    const float sword_attack1_duration = 4.0F / 3.8f;
    
    const float sword_attack1_damageTimingStart = 0.25F / 2;
    //const float sword_attack1_damageTimingEnd = 1.1F / 4;
    
    //const float sword_attack1_distance = 2F;
    const float sword_attack1_radius = 2F;
    const int sword_attack1_dmg = 30;
    
    
    const float firing_duration = 2 / 2;
    const int firing_damage_per_proj = 25;
    const float firing_timing = 1F / 2.5f;
    const float firing_cooldown = 4.0F;
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
            float sqrDistanceToPlayer = Math.SqrDistance(local_pc.GetCenterPosition(), pos);
            
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
    
    const float daggerSpeed = 42;
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
    
    Vector3 attack1_dashEndPos;
    void DoSwordAttack1_withDash(Vector3 dashEndPos)
    {
        anim.Play("Base.Sword_Swing1", 0, 0);
        
        
        sword.OnSwing();
    }
    
    void DoSwordAttack1()
    {
        anim.Play("Base.Sword_Swing1", 0, 0);
        audio_src.PlayOneShot(clipSwordSwing1, 0.6f);
        sword.OnSwing();
        dash_ps.Play();
    }
    
    
    public bool inRange1;
    public bool capsuleHit;
    public bool inRange2;
    public float inRange2_range;
    
    
    const float spawning_duration = 4.4f;
    const float slam_cooldown = 6;
    float slam_cooldown_timer;
    
    float changeTargetTimer;
    const float changeTargetCooldown = 6.0f;
    
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
            case(SinclaireCoolState.Idle):
            {
                if(canSendCommands && UberManager.readyToSwitchLevel)
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
            case(SinclaireCoolState.Chasing):
            {
                if(IsTargetValid())
                {
                    changeTargetTimer += dt;
                    if(changeTargetTimer > changeTargetCooldown)
                    {
                        changeTargetTimer = 0;
                        if(canSendCommands && UberManager.readyToSwitchLevel)
                        {
                            Transform potentialTarget = ChooseTargetClosest(thisTransform.localPosition);
                            if(potentialTarget.GetInstanceID() != target_pc.thisTransform.GetInstanceID())
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
                    
                    brainTimer += dt;
                    firing_masterTimer += dt;
                    slam_cooldown_timer += dt;
                    
                    
                    Vector3 targetGroundPos = target_pc.GetGroundPosition();
                    
                    UpdateRemoteAgentDestination(targetGroundPos);
                   
                    if(canSendCommands && UberManager.readyToSwitchLevel)
                    {
                        if(slam_cooldown_timer > slam_cooldown)
                        {
                            LockSendingCommands();
                            NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.Ability2);
                            break;
                            
                            // int playerCount = NPCManager.AITargets().Count;
                            
                            // int rand = Random.Range(0, playerCount);
                            
                            
                            // PlayerController slam_target = NPCManager.AITargets()[rand].GetComponent<PlayerController>(); 
                            // if(slam_target && slam_target.isAlive)
                            // {
                            //     LockSendingCommands();
                            //     NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.Ability1)
                            //     break;                                                                
                            // }
                            // else
                            // {
                            //     slam_cooldown_timer = slam_cooldown * 0.8f;
                            // }
                        }
                        
                        Vector3 sinclaireGroundPosition = thisTransform.localPosition;
                        
                        inRange1 = Math.SqrDistance(targetGroundPos, sinclaireGroundPosition) < dashDistanceCheck * dashDistanceCheck;
                        if(inRange1)
                        {
                            NavMeshHit navMeshHit;
                            //Vector3 punch_dir = (Math.GetXZ(targetGroundPos - padlaPosition)).normalized;
                            Vector3 punch_dir = (targetGroundPos - sinclaireGroundPosition).normalized;
                            
                            Vector3 dash_pos = sinclaireGroundPosition;
                            if(NavMesh.SamplePosition(sinclaireGroundPosition + punch_dir * dashDistance, out navMeshHit, 1f, NavMesh.AllAreas))
                            {
                                dash_pos = navMeshHit.position;
                                // InGameConsole.LogFancy("We DO <color=green>DASH ATTACK</color>");
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
                                NetworkObjectsManager.CallNetworkFunctionUnreliable(net_comp.networkId, NetworkCommand.Move, movePos);
                            }
                        }
                    }
                
                }
                else
                {
                    if(canSendCommands && UberManager.readyToSwitchLevel)
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
            case(SinclaireCoolState.SpawningCool):
            {
                brainTimer += dt;
                if(brainTimer > spawning_duration && canSendCommands)
                {
                    LockSendingCommands();
                    brainTimer = 0;
                    NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.Ability1);
                }
                break;
            }
            case(SinclaireCoolState.SwordAttacking1):
            {
                brainTimer += dt;
                
                if(canSendCommands && UberManager.readyToSwitchLevel)
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
                            NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.SetState, (byte)SinclaireCoolState.Idle);
                        }
                    }
                }
                
                break;
            }
            case(SinclaireCoolState.Firing1):
            {
                brainTimer += dt;
                
                if(canSendCommands && UberManager.readyToSwitchLevel)
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
                            NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.SetState, (byte)SinclaireCoolState.Idle);
                        }
                    }
                }
                
                break;
            }
            case(SinclaireCoolState.Vanished):
            {
                brainTimer += dt;
                if(brainTimer > vanishDuration)
                {
                    ref List<PlayerController> pcs = ref UberManager.Singleton().playerControllers;
                    int playerCount = pcs.Count;
                    int playersAlive = 0;
                    for(int i = 0; i < playerCount; i++)
                    {
                        if(pcs[i].isAlive)
                            playersAlive++;
                    }
                    
                    float slamHeight = 18;
                    Vector3 slamPos = thisTransform.localPosition;
                    
                    if(playersAlive > 0)
                    {
                        int rand = Random.Range(0, playersAlive);
                        PlayerController slam_target = pcs[rand].GetComponent<PlayerController>(); 
                        
                        if(slam_target && slam_target.isAlive)
                        {
                            RaycastHit hit;
                            
                            Vector3 slamTargetPredictedPos = slam_target.GetCenterPositionPredictedXZ();
                            Ray ray = new Ray(slamTargetPredictedPos, Vector3.down);
                            
                            if(Physics.SphereCast(ray, 1f,  out hit, 200, groundMask))
                            {
                                NavMeshHit navMeshHit;
                                if(NavMesh.SamplePosition(hit.point, out navMeshHit, 1, NavMesh.AllAreas))
                                {
                                    //slamHeight = Mathf.Max(slam_target.GetGroundPosition().y + 3 ;
                                    slamPos = navMeshHit.position;
                                    InGameConsole.LogFancy("FOUND OK POS <color=yellow>" + slamPos + "</color>");
                                }
                                else
                                {
                                    InGameConsole.LogFancy("FOUND NOT POS FROM NAVMESH SAMPLE POSITION");
                                }
                            }
                            else
                            {
                                InGameConsole.LogFancy("FOUND NOT POS FROM RAYCAST");
                            }
                        }
                    }
                        
                    LockSendingCommands();
                    NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.Ability3, slamPos, slamHeight);
                }
                break;
            }
            case(SinclaireCoolState.Slamming):
            {
                break;
            }
            case(SinclaireCoolState.Dead):
            {
                break;
            }
        }
    }
    
    static readonly Vector3 vUp = new Vector3(0, 1, 0);
    Quaternion deriv;
    
    Vector3 currentDestination;
    
    const float moveSpeed = 9F;//;0.738248f * 2;//7F;
    const float attackingMoveSpeed = 45F;
    
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
            if(state != SinclaireCoolState.SwordAttacking1 && target_pc)
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
    
    
    Vector3 localStrikeOffset = new Vector3(0, 2f, 1f);
    
    int instance_id;
    public Transform qts_transform;
    public QuickTimeSphere current_qts;
    
    void UpdateBrainLocally(float dt)
    {
        switch(state)
        {
            case(SinclaireCoolState.Chasing):
            {
                Vector3 currentPos = thisTransform.localPosition;
                
                float dV = speedMult * dt * moveSpeed;
                thisTransform.localPosition = Vector3.MoveTowards(currentPos, currentDestination, dV);
                
                RotateToLookAt(currentDestination, rotateTime * speedMult);
                
                
                Vector3 dPos = new Vector3();//currentDestination - currentPos;
                dPos.x = currentDestination.x - currentPos.x;
                dPos.y = currentDestination.y - currentPos.y;
                dPos.z = currentDestination.z - currentPos.z;
                
                anim.SetFloat(MoveSpeedHash, Math.Magnitude(dPos), animDampTime, dt);
                
                break;
            }
            case(SinclaireCoolState.SwordAttacking1):
            {
                Vector3 currentPos = thisTransform.localPosition;
                float dV = speedMult * dt * attackingMoveSpeed;
                
                if(sword_attack_timer < sword_attack1_damageTimingStart)
                    dV = 0;
                
                destinationGizmo = currentDestination;
                thisTransform.localPosition = Vector3.MoveTowards(currentPos, currentDestination, dV);
                
                if(UberManager.CanMakeQTS(instance_id))
                {
                    current_qts = UberManager.MakeQTS(instance_id, thisTransform, qts_transform.position, QuickTimeType.Default, 0.75f, 900, 0.2f);
                }   
                
                RotateToLookAt(currentDestination, 0.08F);
                
                sword_attack_timer += dt;
                if(canDoMeleeDamageToLocalPlayer)
                {
                    //if(sword_attack_timer > sword_attack1_damageTimingStart && sword_attack_timer < sword_attack1_damageTimingEnd)
                    float sqrDistanceToDestination = Math.SqrDistance(currentDestination, currentPos);
                    if(sword_attack_timer > sword_attack1_damageTimingStart)
                    {
                        Vector3 dmgPos = thisTransform.localPosition + thisTransform.up * localStrikeOffset.y + thisTransform.forward * localStrikeOffset.z;
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
            case(SinclaireCoolState.Firing1):
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
                        ShootTriple(shootPosOffsetted, _shootDir);
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
            case(SinclaireCoolState.SpawningCool):
            {
                break;
            }
            case(SinclaireCoolState.Vanished):
            {
                
                break;
            }
            case(SinclaireCoolState.Slamming):
            {
                thisTransform.localPosition = Vector3.MoveTowards(thisTransform.localPosition, currentSlamPos, slamSpeed * dt);
                
                if(!didSlam)
                {
                    float sqrDistance = Math.SqrDistance(thisTransform.localPosition, currentSlamPos);
                    if(sqrDistance < 0.1f * 0.1f)
                    {
                        didSlam = true;
                        anim.Play("Base.OnSlammed", 0, 0);
                        MakeShockWave(currentSlamPos + new Vector3(0, 0.33f, 0));                        
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
    
    
    const int MaxHealth = 11750;
    int CalculatedMaxHealth;
    int HitPoints;
    
    void TakeDamage(int dmg, byte limb_id)
    {
        if(state == SinclaireCoolState.SpawningCool || state == SinclaireCoolState.Vanished)
        {
            return;
        }
        //InGameConsole.LogOrange("TakeDamage()");
        HitPoints -= dmg;
         
        if(HitPoints <= 0)
        {
            Die(Vector3.zero, limb_id);
            HitPoints = 0;
        }
    }
    
    void TakeDamageForce(int dmg, Vector3 force, byte limb_id)
    {
        if(state == SinclaireCoolState.SpawningCool || state == SinclaireCoolState.Vanished)
        {
            return;
        }
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
        if(state == SinclaireCoolState.SpawningCool || state == SinclaireCoolState.Vanished)
        {
            return;
        }
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
        if(state == SinclaireCoolState.Dead)
        {
            return;
        }
        
        //InGameConsole.LogOrange("DieFromExplosion()");
        
        SetState(SinclaireCoolState.Dead);
        
        if(spawnedObjectComp)
            spawnedObjectComp.OnObjectDied();
        
        HitPoints = -1;
        
        anim.enabled = false;
        col.enabled = false;
        
        NetworkObjectsManager.UnregisterNetObject(net_comp);
        if(remoteAgent)
            Destroy(remoteAgent.gameObject, 0.1f);
            
        
        
        audio_src.PlayOneShot(clipDeath, 0.5f);
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
        if(state == SinclaireCoolState.Dead)
        {
            return;
        }
        
        SetState(SinclaireCoolState.Dead);
        
        if(spawnedObjectComp)
            spawnedObjectComp.OnObjectDied();
        
        sword.Drop();
        
        
        
        anim.enabled = false;
        col.enabled = false;
        
        NetworkObjectsManager.UnregisterNetObject(net_comp);
        if(remoteAgent)
            Destroy(remoteAgent.gameObject, 0.1f);
            
        audio_src.PlayOneShot(clipDeath, 0.5f);
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
        if(state != SinclaireCoolState.Dead && damage_taken_timeStamp + 0.15F < Time.time)
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
        if(state == SinclaireCoolState.Dead)
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
    
    const float dashDistanceCheck = 3.55F;
    const float dashDistance = 8.0F;
    
    
    
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
    public AudioClip clipSwordAppear;
}
