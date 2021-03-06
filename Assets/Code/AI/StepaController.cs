using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

public enum StepaState : byte
{
    Idle,
    Chasing,
    Shooting,
    Airbourne,
    Fleeing,
    Aiming,
    Dead
}

public class StepaController : MonoBehaviour, INetworkObject, IDamagableLocal, ILaunchableAirbourne, IKillableThing
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
    static int projectileMask = -1;
    
    
    
    DamagableLimb[] limbs;
    
    
    //public TrailRendererController trail_arm;
    //public TrailRendererController trail_spine;
    
    SpawnedObject spawnedObjectComp;
    
    public Cloth coat1;
    public Cloth coat2;
    
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
        if(state != StepaState.Dead)
        {
            return true;
        }
        else
            return false;
    }
    
    
    
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
        
        if(projectileMask == -1)
        {
            projectileMask = LayerMask.GetMask("Ground", "Ceiling", "Player");
        }
        
        path_update_cd = PhotonNetwork.OfflineMode ? PATH_UPDATE_BASE / 2 : PATH_UPDATE_BASE;
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
        
        NPCManager.RegisterKillable(this);
        
        AudioManager.AddEnemiesAlive();
        
        if(!UberManager.RenderCoats)
        {
            coat1.GetComponent<SkinnedMeshRenderer>().enabled = false;
            coat1.enabled = false;
            
            coat2.GetComponent<SkinnedMeshRenderer>().enabled = false;
            coat2.enabled = false;
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
                        SetState(StepaState.Chasing);
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
            case(NetworkCommand.SetState):
            {
                UnlockSendingCommands();
                if(state == StepaState.Dead)
                {
                    return;
                }
                
                byte _state = (byte)args[0];
                
                if(_state == (byte)StepaState.Aiming)
                {
                    audio_src.PlayOneShot(clipAiming);
                    aimingPos = (Vector3)args[1];
                }
                
                SetState((StepaState)_state);
                
                break;
            }
            case(NetworkCommand.Flee):
            {
                UnlockSendingCommands();
                
                Vector3 _fleeingPos = (Vector3)args[0];
                
                fleeingPos = _fleeingPos;
                currentDestination = _fleeingPos;
                
                SetState(StepaState.Fleeing);
                
                break;
            }
            case(NetworkCommand.Shoot):
            {
                UnlockSendingCommands();
                
                Vector3 _shotPos = (Vector3)args[0];
                Vector3 _shotDir = (Vector3)args[1];
                
                SetMovePos(_shotPos);
                WarpRemoteAgent(_shotPos);
                UpdateRemoteAgentDestination(_shotPos);
                
                if(state != StepaState.Dead && state != StepaState.Airbourne)
                {
                    anim.Play("Base.Fire", 0, 0);
                }
                
                
                shootingDirection = _shotDir;
                aim_ps.Stop();
                Shoot(_shotPos + new Vector3(0, offsetShootingY, 0), _shotDir);
                SetState(StepaState.Shooting);
                
                break;
            }
            case(NetworkCommand.TakeDamageExplosive):
            {
                int incomingDamage = (int)args[0];
                
                int _incomingDamage = incomingDamage;
                if(_incomingDamage > HitPoints)
                    _incomingDamage = HitPoints;
                int small_healing_times = _incomingDamage / UberManager.HEALING_DMG_THRESHOLD;
                HealthCrystalSmall.MakeSmallHealing(thisTransform.localPosition + new Vector3(0, 2f, 0), small_healing_times);
                
                TakeDamageExplosive(incomingDamage);
                
                break;
            }
            case(NetworkCommand.TakeDamageLimbNoForce):
            {
                int incomingDamage = (int)args[0];
                
                int _incomingDamage = incomingDamage;
                if(_incomingDamage > HitPoints)
                    _incomingDamage = HitPoints;
                int small_healing_times = _incomingDamage / UberManager.HEALING_DMG_THRESHOLD;
                HealthCrystalSmall.MakeSmallHealing(thisTransform.localPosition + new Vector3(0, 2f, 0), small_healing_times);
                
                
                
                byte limb_id = (byte)args[1];
                TakeDamage(incomingDamage, limb_id);
                //TakeDamageForce(incomingDamage, force, limb_id);
                
                break;
            }
            case(NetworkCommand.TakeDamageLimbWithForce):
            {
                int incomingDamage = (int)args[0];
                
                int _incomingDamage = incomingDamage;
                if(_incomingDamage > HitPoints)
                    _incomingDamage = HitPoints;
                int small_healing_times = _incomingDamage / UberManager.HEALING_DMG_THRESHOLD;
                HealthCrystalSmall.MakeSmallHealing(thisTransform.localPosition + new Vector3(0, 2f, 0), small_healing_times);
                
                Vector3 force = (Vector3)args[1];
                byte limb_id = (byte)args[2];
                int len = limbs.Length;
                for(int i = 0; i < len; i++)
                {
                    if(limbs[i] && limbs[i].limb_id == limb_id)
                    {
                        limbs[i].ReactWithoutPos();
                    }
                }
                TakeDamageForce(incomingDamage, force, limb_id);
                
                break;
            }
            case(NetworkCommand.LaunchAirborne):
            {
                UnlockSendingCommands();
                
                if(state == StepaState.Dead)
                {
                    return;
                }
                
                airbourneTimeStamp = Time.time;
                
                Vector3 launchPos = (Vector3)args[0];
                Vector3 launchVel = (Vector3)args[1];
                
                if(args.Length > 2)
                {
                    //We receive a damage also:
                    int incomingDamage = (int)args[2];
                    byte limb_rnd = (byte)Random.Range(2, 5);
                    TakeDamage(incomingDamage, limb_rnd);
                }
                    
                velocity = launchVel * Globals.NPC_airbourne_force_mult;
                audio_src.PlayOneShot(clipLaunchedAirborne);
                thisTransform.localPosition = launchPos;
                SetState(StepaState.Airbourne);
                    
                break;
            }
            case(NetworkCommand.LaunchAirborneUp):
            {
                UnlockSendingCommands();
                
                if(state != StepaState.Airbourne)
                {
                    Vector3 launchPos = (Vector3)args[0];
                    float upForce = (float)args[1];
                    
                    velocity = new Vector3(0, upForce, 0);
                    audio_src.PlayOneShot(clipLaunchedAirborne);
                    thisTransform.localPosition = launchPos;
                    SetState(StepaState.Airbourne);
                    
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
                
                SetState(StepaState.Idle);
                
                
                break;
            }
            default:
            {
                break;
            }
        }
    }
    
    const float projectileSpeed = 42F;
    const float projectileRadius = 0.4F;
    const int projectileDamage = 30;
    
    const float shootMaxDistance = 55;
    const float shootingChasingCooldown = 2.15F;
    float shootingChasingTimer = 0;
    
    const float shootingDuration = 0.6F;//0.5F;
    
    
    void DoLight(Vector3 pos)
    {
        GameObject g = ObjectPool2.s().Get(ObjectPoolKey.LightPooled, false);
        LightPooled light = g.GetComponent<LightPooled>();
        Color color = new Color(1f, 0.41f, 0f, 1f);
        
        float decay_speed = 4 / 0.5f * 2.6f;
        light.DoLight(pos, color, 0.5f, 14, 4, decay_speed);
    }
    
    void Shoot(Vector3 shotPos, Vector3 shotDir)
    {
        ParticlesManager.PlayPooled(ParticleType.shotStar_ps, shotPos + shotDir * 0.9f, shotDir);
        
        GameObject bullet = ObjectPool.s().Get(ObjectPoolKey.Bullet_npc2);
        bool isBulletMine = PhotonNetwork.IsMasterClient;
        bullet.GetComponent<BulletController>().LaunchAsSphere(shotPos, shotDir, projectileRadius, projectileMask, projectileSpeed, projectileDamage, isBulletMine);
        
        AudioManager.Play3D(SoundType.projectile_launch1, shotPos, 0.7F);
        DoLight(shotPos);
    }
    
    Vector3 aimingPos;
    const float aimingDurationBeforeShooting = 0.5f;
    public ParticleSystem aim_ps;
    
    void SetState(StepaState _state)
    {
        if(state == StepaState.Dead)
        {
            return;
        }
        
        
        switch(_state)
        {
            case(StepaState.Airbourne):
            {
                if(PhotonNetwork.IsMasterClient)
                {
                    DisableRemoteAgent();
                }
                
                anim.Play("Base.Airbourne", 0, 0);
                break;
            }
            case(StepaState.Chasing):
            {
                distanceTravelledRunningSqr = 0;
                brainTimer = 0;
                break;
            }
            case(StepaState.Fleeing):
            {
                distanceTravelledRunningSqr = 0;
                break;
            }
            case(StepaState.Aiming):
            {
                brainTimer = 0;
                anim.Play("Base.Aim", 0, 0);
                aim_ps.Play();
                break;                
            }
            case(StepaState.Shooting):
            {
                brainTimer = 0;
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
    
    float damage_taken_timeStamp2;
    
    void TakeDamage(int dmg, byte limb_id)
    {
        OnTakeDamageSFX();

        //InGameConsole.LogOrange("TakeDamage()");
        HitPoints -= dmg;
        //audio_src.clip clipHurt1
         
        if(HitPoints <= 0)
        {
            Die(Vector3.zero, limb_id);
            HitPoints = 0;
        }
    }
    
    void TakeDamageForce(int dmg, Vector3 force, byte limb_id)
    {
        //if(Time.time - damage_taken_timeStamp > 0.2f)
        //{
            OnTakeDamageSFX();

        //}
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
        OnTakeDamageSFX();
        //if(Time.time - damage_taken_timeStamp > 0.2f)
        //{
        //}
        //InGameConsole.LogOrange("TakeDamageExplosive()");
        HitPoints -= dmg;
        if(HitPoints <= 0)
        {
            DieFromExplosion();
            HitPoints = 0;
        }
    }
    void OnTakeDamageSFX()
    {
        if(Time.time - damage_taken_timeStamp2 > 0.2f)
        {
            audio_src.pitch = Random.Range(0.92f, 1f);
            audio_src.PlayOneShot(clipHurt1, 1);
            damage_taken_timeStamp2 = Time.time;
        }
    }
    
    void DieFromExplosion()
    {
        if(state == StepaState.Dead)
        {
            return;
        }
        
        AudioManager.RemoveEnemiesAlive();
        
///        InGameConsole.LogOrange("DieFromExplosion()");
        
        SetState(StepaState.Dead);
        
        if(spawnedObjectComp)
            spawnedObjectComp.OnObjectDied();
        
        HitPoints = -1;
        
        anim.enabled = false;
        col.enabled = false;
        
        NetworkObjectsManager.UnregisterNetObject(net_comp);
        if(remoteAgent)
            Destroy(remoteAgent.gameObject, 0.1f);
        audio_src.pitch = Random.Range(0.6f, 0.8f);
        audio_src.PlayOneShot(clipDeath);
        
        
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
        
        for(int i = 0; i < len; i++)
        {
            if(!limbs[i].isRootLimb && limbs[i].canBeDestroyed)
            {
                limbs[i].ExplodeLimbWhenMasterAlive();
                break;
            }
        }
        
        
        // if(limb_to_destroy != 0)
        // {
        //     if(limbs != null)
        //     {
        //         for(int i = 0; i < len; i++)
        //         {
        //             if(limbs[i].limb_id == limb_to_destroy)
        //             {
        //                 //Vector3 f = force;
        //                 //InGameConsole.LogOrange(string.Format("Apply force {0} to limb {1}", f, limb_to_destroy));
                        
        //                 //limbs[i].ApplyForceToAdjacentLimbs(f);
        //                 if(!limbs[i].isRootLimb)
        //                 {
        //                     limbs[i].TakeDamageLimb(2500);
                            
        //                 }
        //                 //limbs[i].AddForceToLimb(f);
                        
        //                 //break;
        //             }
        //         }
        //     }
        // }
        DropHealthCrystals();
    }
    
    void Die(Vector3 force, byte limb_to_destroy)
    {
        if(state == StepaState.Dead)
        {
            return;
        }
        
        AudioManager.RemoveEnemiesAlive();
        SetState(StepaState.Dead);
        audio_src.pitch = Random.Range(0.7f, 0.9f);
        audio_src.PlayOneShot(clipDeath);
        
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
        Destroy(coat1.gameObject);
        Destroy(coat2.gameObject);
        DropHealthCrystals();
    }
    
    void DropHealthCrystals()
    {
        HealthCrystal hc = ObjectPool.s().Get(ObjectPoolKey.HealthCrystal).GetComponent<HealthCrystal>();
        hc.Launch(this.transform.localPosition + new Vector3(0, 1.25f, 0), 10);
        hc = ObjectPool.s().Get(ObjectPoolKey.HealthCrystal).GetComponent<HealthCrystal>();
        hc.Launch(this.transform.localPosition + new Vector3(0, 1.25f, 0), 10);
        hc = ObjectPool.s().Get(ObjectPoolKey.HealthCrystal).GetComponent<HealthCrystal>();
        //hc.Launch(this.transform.localPosition + new Vector3(0, 1.25f, 0));
    }
    
    const int MaxHealth = 400;
    public int HitPoints = MaxHealth;
    
    public int GetCurrentHP()
    {
        return HitPoints;
    }
    
    public void TakeDamageLocally(int dmg, Vector3 hitPos, Vector3 hitDir)
    {
        if(state != StepaState.Dead && damage_taken_timeStamp + 0.15F < Time.time)
        {
            float vol = Random.Range(0.8f, 1f);
            //audio_src.PlayOneShot(clipHurt1, vol);
            
            damage_taken_timeStamp = Time.time;
        }
    }
    
    public bool IsDead()
    {
        if(state == StepaState.Dead)
            return true;
        else
            return false;
    }
    
    public StepaState state = StepaState.Idle;
    
    const float PATH_UPDATE_BASE = 0.18F;//0.15F;
    float path_update_cd;
    float brainTimer = 0;
    
    void SetMovePos(Vector3 pos)
    {
        currentDestination = pos;
    }
    
    Vector3 currentDestination;
    static readonly Vector3 vUp = new Vector3(0, 1, 0);
    Quaternion deriv;
    
    const float moveSpeed = 5f;
    const float rotateTime = 0.15F;
    const float rotateTimeAtTarget = 0.2F;
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
        if(state == StepaState.Dead || state == StepaState.Airbourne)
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
        return state == StepaState.Airbourne;
    }
    
    void SetTarget(PlayerController target)
    {
        target_pc = target;
    }
    
    const float offsetShootingY = 2.25F;
    
    public bool CanShootAtPos(Vector3 offsetedCheckPosition, Vector3 posToCheck, float maxDistance)
    {
        Vector3 offsetSensorPosition = offsetedCheckPosition;
        
        
        float sqrDistance = Math.SqrDistance(offsetSensorPosition, posToCheck);
        
        if(sqrDistance > maxDistance * maxDistance)
        {
            return false;
        }
        
        Vector3 shootDir = posToCheck;
        
        //shootDir = posToCheck - offsetSensorPosition
        shootDir.x -= offsetSensorPosition.x;
        shootDir.y -= offsetSensorPosition.y;
        shootDir.z -= offsetSensorPosition.z;
        
        shootDir = Math.Normalized(shootDir);
        
        Ray ray = new Ray(offsetSensorPosition, shootDir);
        
        float distance = Mathf.Sqrt(sqrDistance);
        
        RaycastHit hit;
        if(!Physics.Raycast(ray, out hit, distance, groundMask))
        {
            //Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.red, 2);
            return true;
        }
        //Debug.DrawRay(ray.origin, ray.direction * distance, Color.yellow, 0);
        
        return false;
    }
    
    
    Vector3 fleeingPos;
    float fleeingTimer;
    const float fleeingTimeout = 3F;
    const float fleeingComfortDistance = 12F;
    const float fleeingRange = 6F;
    
    float changeTargetTimer;
    const float changeTargetCooldown = 6;
    
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
        switch(state)
        {
            case(StepaState.Idle):
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
            case(StepaState.Chasing):
            {
                brainTimer += dt;
                shootingChasingTimer += dt;
                
                if(IsTargetValid())
                {
                    changeTargetTimer += dt;
                    if(changeTargetTimer > changeTargetCooldown)
                    {
                        changeTargetTimer = 0;
                        if(canSendCommands && UberManager.readyToSwitchLevel)
                        {
                            Transform potentialTarget = ChooseTargetClosest(thisTransform.localPosition);
                            Vector3 _shootingCheckPosOffsetted = currentDestination;
                            _shootingCheckPosOffsetted.y += offsetShootingY;
                            
                            Vector3 _offsettedTargetHeadPos = target_pc.GetHeadPosition();
                            _offsettedTargetHeadPos.y -= 0.25F;
                            
                            
                            bool _canShootFromDestination = CanShootAtPos(_shootingCheckPosOffsetted, _offsettedTargetHeadPos, shootMaxDistance);
                            if(_canShootFromDestination)
                            {
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
                    }
                    
                    Vector3 targetGroundPos = target_pc.GetGroundPosition();
                    //UpdateRemoteAgentDestination(targetGroundPos);
                    
                    Vector3 shootingCheckPosOffsetted = currentDestination;
                    shootingCheckPosOffsetted.y += offsetShootingY;
                    
                    Vector3 offsettedTargetHeadPos = target_pc.GetHeadPosition();
                    offsettedTargetHeadPos.y -= 0.25F;
                    
                    
                    bool canShootFromDestination = CanShootAtPos(shootingCheckPosOffsetted, offsettedTargetHeadPos, shootMaxDistance);
                    
                    if(!canShootFromDestination)
                    {
                        UpdateRemoteAgentDestination(targetGroundPos);
                        if(shootingChasingTimer > shootingChasingCooldown * 0.5F)
                        {
                            shootingChasingTimer = shootingChasingCooldown * Random.Range(0.4F, 0.6F);
                        }
                    }
                    
                    // else
                    // {
                    //     UpdateRemoteAgentDestination(currentDestination);
                    // }
                    
                    if(canSendCommands && UberManager.readyToSwitchLevel)
                    {
                        if(canShootFromDestination)
                        {
                            if(shootingChasingTimer >= shootingChasingCooldown)
                            {
                                //shootingChasingTimer = 0;
                                shootingChasingTimer = Random.Range(-0.3F, 0.4F);
                                brainTimer = 0;
                                
                                //Vector3 _shotPos =  GetRemoteAgentPos();
                                // Vector3 _shotPos = currentDestination;
                                // Vector3 _shotDir = (offsettedTargetHeadPos - shootingCheckPosOffsetted).normalized;
                                //NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.Shoot, _shotPos, _shotDir);
                                
                                    
                                LockSendingCommands();
                                Vector3 _aimingPos = currentDestination;
                                NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.SetState, StepaState.Aiming, _aimingPos);
                            }
                        }
                        else if(brainTimer > path_update_cd)
                        {
                            brainTimer = 0;
                            Vector3 remoteAgentPos = GetRemoteAgentPos();
                                
                            if(Math.SqrDistance(currentDestination, remoteAgentPos) > 0.1F * 0.1F)
                            {
                                LockSendingCommands();
                                NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.Move, remoteAgentPos);
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
                        else
                        {
                            LockSendingCommands();
                            NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.SetState, (byte)StepaState.Idle);   
                        }
                    }
                    
                }
                
                break;
            }
            case(StepaState.Aiming):
            {
                brainTimer += dt;
                WarpRemoteAgent(aimingPos);
                
                if(brainTimer > aimingDurationBeforeShooting)
                {
                    if(canSendCommands && UberManager.readyToSwitchLevel)
                    {
                        Vector3 targetGroundPos = target_pc.GetGroundPosition();
                    
                        Vector3 shootingCheckPosOffsetted = currentDestination;
                        shootingCheckPosOffsetted.y += offsetShootingY;
                        
                        Vector3 offsettedTargetHeadPos = target_pc.GetHeadPosition();
                        offsettedTargetHeadPos.y -= 0.25F;
                        
                        Vector3 _shotPos = currentDestination;
                        Vector3 _shotDir = (offsettedTargetHeadPos - shootingCheckPosOffsetted).normalized;
                        
                        LockSendingCommands();
                        NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.Shoot, _shotPos, _shotDir);
                    }
                }
                
                break;
            }
            case(StepaState.Shooting):
            {
                brainTimer += dt;
                WarpRemoteAgent(thisTransform.localPosition);
                
                if(brainTimer >= shootingDuration)
                {
                    if(canSendCommands && UberManager.readyToSwitchLevel)
                    {
                        brainTimer = 0;
                        NavMeshHit navMeshHit;
                        
                        int samplingIterationNumber = 0;
                        
                        // if(Math.SqrDistance(t))
                        bool foundNavPos = false;
                        
                        if(IsTargetValid())
                        {
                            if(Math.SqrDistance(target_pc.GetGroundPosition(), thisTransform.localPosition) > 6 * 6)
                            {
                                samplingIterationNumber = 8;
                            }
                        }
                        
                        
                        while(!foundNavPos && samplingIterationNumber < 8)
                        {
                            Vector3 offsetDir = new Vector3(Random.Range(-1F, 1F), 0, Random.Range(-1F, 1F));
                            offsetDir.Normalize();
                            
                            Vector3 positionToTry = thisTransform.localPosition + offsetDir * Random.Range(1f, fleeingRange);
                            
                            samplingIterationNumber++;
                            foundNavPos = NavMesh.SamplePosition(positionToTry, out navMeshHit, 0.05F, NavMesh.AllAreas);
                            
                            
                            if(foundNavPos)
                            {
                                Vector3 capsulePBottom = GetCapsulePointBottom();
                                capsulePBottom.y += 0.05F;
                                Vector3 capsulePTop = GetCapsulePointTop();
                                
                                float capsuleRadius = col.radius;
                                
                                Vector3 dirToNavPos = Vector3.Normalize(navMeshHit.position - thisTransform.localPosition);
                                float distanceToNavPos = Vector3.Distance(navMeshHit.position, thisTransform.localPosition);
                                
                                //RaycastHit hit;
                                //if(Physics.CapsuleCast(capsulePBottom, capsulePTop, capsuleRadius, dirToNavPos, out hit, distanceToNavPos, groundMask))
                                if(Physics.CapsuleCast(capsulePBottom, capsulePTop, capsuleRadius, dirToNavPos, distanceToNavPos, groundMask))
                                {
                                    foundNavPos = false;
                                    //InGameConsole.LogFancy(string.Format("<color=red>FLEE NOT OK. Hit {0} </color>", hit.collider.gameObject.name));
                                    InGameConsole.LogFancy("<color=red>FLEE NOT OK. Hit</color>");
                                }
                                else
                                {
                                    Vector3 _fleeingPos = navMeshHit.position;
                                    LockSendingCommands();
                                    //InGameConsole.LogFancy("<color=green>FLEE OK</color>");
                                    NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.Flee, _fleeingPos);
                                    
                                }
                            }
                        }
                        
                            
                        if(!foundNavPos && canSendCommands)
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
                                NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.SetState, (byte)StepaState.Idle);
                            }
                        }
                    }
                }
                
                break;
            }
            case(StepaState.Fleeing):
            {
                UpdateRemoteAgentDestination(fleeingPos);
                
                if(canSendCommands && UberManager.readyToSwitchLevel)
                {
                    brainTimer += dt;
                    fleeingTimer += dt;
                
                    float sqrDistance = Math.SqrDistance(thisTransform.localPosition, fleeingPos);
                    //InGameConsole.LogFancy("SqrDistance is " + sqrDistance.ToString());
                    if(sqrDistance < 0.2F * 0.2F || fleeingTimer > fleeingTimeout)
                    {
                        LockSendingCommands();
                        brainTimer = 0f;
                        fleeingTimer = 0f;
                        
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
                            NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.SetState, (byte)StepaState.Idle);
                        }
                    }
                    else
                    {
                        if(brainTimer > path_update_cd)
                        {
                            brainTimer = 0;
                            Vector3 remoteAgentPos = GetRemoteAgentPos();
                                
                            if(Math.SqrDistance(currentDestination, remoteAgentPos) > 0.01F * 0.01F)
                            {
                                LockSendingCommands();
                                NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.Move, remoteAgentPos);
                            }
                        }
                    }
                }
                
                break;
            }
            case(StepaState.Airbourne):
            {
                Vector3 currentPos = thisTransform.localPosition;
                
                velocity.y += GRAVITY_Y * dt;
                velocity.y = Math.Clamp(-GRAVITY_MAX, GRAVITY_MAX, velocity.y);
                
                
                if(thisTransform.localPosition.y < Globals.Airbourne_YCoord_Lowest && canSendCommands)
                {
                    LockSendingCommands();
                    //NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.TakeDamageLimbNoForce, HitPoints * 2, 1);
                    NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.TakeDamageLimbNoForce, HitPoints * 2, (byte)7);
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
                                if(NavMesh.SamplePosition(samplePos, out _navMeshHit, 1f, NavMesh.AllAreas))
                                {
                                    if(canSendCommands && UberManager.readyToSwitchLevel)
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
                            //if(NavMesh.SamplePosition(hit.point, out navMeshHit, 0.33f, NavMesh.AllAreas))
                            if(NavMesh.SamplePosition(thisTransform.localPosition, out navMeshHit, 0.33f, NavMesh.AllAreas))
                            {
                                if(canSendCommands && UberManager.readyToSwitchLevel)
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
            case(StepaState.Dead):
            {
                break;
            }
        }
    }
    
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
    float airbourneTimeStamp = 0;
    
  
    
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
        
        if(Application.isPlaying)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(currentDestination, 0.33f);
        }
        if(state == StepaState.Airbourne)
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
    
    
    const float footStepDistance = 1.2F;
    float distanceTravelledRunningSqr;
    
    
    Vector3 shootingDirection;
    
    void UpdateBrainLocally(float dt)
    {
        switch(state)
        {
            case(StepaState.Idle):
            {
                break;
            }
            case(StepaState.Chasing):
            {
                Vector3 currentPos = thisTransform.localPosition;
                
                float dV = speedMult * dt * moveSpeed;
                Vector3 updatedPos = Vector3.MoveTowards(currentPos, currentDestination, dV);
                thisTransform.localPosition = updatedPos; 
                
                if(currentDestination == currentPos)
                {
                    if(IsTargetValid())
                        RotateToLookAt(target_pc.GetGroundPosition(), rotateTimeAtTarget * speedMult, false);
                }
                else
                    RotateToLookAt(currentDestination, rotateTime * speedMult, false);
                
                Vector3 dPos = currentDestination - currentPos;
                
                anim.SetFloat(MoveSpeedHash, Math.Magnitude(dPos), animDampTime, dt);
                
                distanceTravelledRunningSqr += Math.Magnitude(updatedPos - currentPos);
                
                if(distanceTravelledRunningSqr > footStepDistance * footStepDistance)
                {
                    distanceTravelledRunningSqr -= footStepDistance * footStepDistance;
                    if(distanceTravelledRunningSqr  > footStepDistance * footStepDistance)
                        distanceTravelledRunningSqr = 0;
                    
                    //audio_src.PlayOneShot(clipStep, 0.33F);
                }
                
                break;
            }
            case(StepaState.Aiming):
            {
                Vector3 currentPos = thisTransform.localPosition;
                
                float dV = speedMult * dt * moveSpeed;
                Vector3 updatedPos = Vector3.MoveTowards(currentPos, currentDestination, dV);
                thisTransform.localPosition = updatedPos; 
                
                //RotateToLookAt(currentDestination, rotateTime * speedMult, false);
                
                if(currentDestination == currentPos)
                {
                    if(IsTargetValid())
                        RotateToLookAt(target_pc.GetGroundPosition(), 0.1f * speedMult, false);
                }
                else
                    RotateToLookAt(currentDestination, rotateTime * speedMult, false);
                
                Vector3 dPos = currentDestination - currentPos;
                
                anim.SetFloat(MoveSpeedHash, Math.Magnitude(dPos), animDampTime, dt);
                
                distanceTravelledRunningSqr += Math.Magnitude(updatedPos - currentPos);
                
                if(distanceTravelledRunningSqr > footStepDistance * footStepDistance)
                {
                    distanceTravelledRunningSqr -= footStepDistance * footStepDistance;
                    if(distanceTravelledRunningSqr  > footStepDistance * footStepDistance)
                        distanceTravelledRunningSqr = 0;
                    
                    //audio_src.PlayOneShot(clipStep, 0.33F);
                }
                
                break;
            }
            case(StepaState.Shooting):
            {
                Vector3 currentPos = thisTransform.localPosition;
                
                float dV = speedMult * dt * moveSpeed;
                Vector3 updatedPos = Vector3.MoveTowards(currentPos, currentDestination, dV);
                thisTransform.localPosition = updatedPos; 
                
                //RotateToLookAt(currentDestination, rotateTime * speedMult, false);
                
                Vector3 shootingDirectionXZ = Math.GetXZ(shootingDirection);
                Quaternion desiredRotation = Quaternion.LookRotation(shootingDirectionXZ);
                
                thisTransform.localRotation = Quaternion.RotateTowards(thisTransform.localRotation, desiredRotation, dt * 1080);
                
                Vector3 dPos = currentDestination - currentPos;
                
                anim.SetFloat(MoveSpeedHash, Math.Magnitude(dPos), animDampTime, dt);
                
                distanceTravelledRunningSqr += Math.Magnitude(updatedPos - currentPos);
                
                if(distanceTravelledRunningSqr > footStepDistance * footStepDistance)
                {
                    distanceTravelledRunningSqr -= footStepDistance * footStepDistance;
                    if(distanceTravelledRunningSqr  > footStepDistance * footStepDistance)
                        distanceTravelledRunningSqr = 0;
                    
                    //audio_src.PlayOneShot(clipStep, 0.6F);
                }
                
                break;
            }
            case(StepaState.Fleeing):
            {
                Vector3 currentPos = thisTransform.localPosition;
                
                float dV = speedMult * dt * moveSpeed;
                Vector3 updatedPos = Vector3.MoveTowards(currentPos, currentDestination, dV);
                thisTransform.localPosition = updatedPos; 
                
                // if(currentPos == fleeingPos)
                // {
                //     if(IsTargetValid())
                //         RotateToLookAt(target_pc.GetGroundPosition(), rotateTimeAtTarget * speedMult, false);
                // }
                // else
                RotateToLookAt(currentDestination, rotateTime * speedMult, false);
                
                Vector3 dPos = currentDestination - currentPos;
                
                anim.SetFloat(MoveSpeedHash, Math.Magnitude(dPos), animDampTime, dt);
                
                distanceTravelledRunningSqr += Math.Magnitude(updatedPos - currentPos);
                
                if(distanceTravelledRunningSqr > footStepDistance * footStepDistance)
                {
                    distanceTravelledRunningSqr -= footStepDistance * footStepDistance;
                    if(distanceTravelledRunningSqr  > footStepDistance * footStepDistance)
                        distanceTravelledRunningSqr = 0;
                    
                    //audio_src.PlayOneShot(clipStep, 0.4f);
                }
                
                break;
            }
            case(StepaState.Airbourne):
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
            case(StepaState.Dead):
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
    
    
    [Header("Clips:")]
    public AudioClip clipAiming;
    public AudioClip clipStep;
    public AudioClip clipHurt1;
    public AudioClip clipDeath;
    public AudioClip clipLaunchedAirborne;
    public AudioClip clipShoot;
}
