using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

public enum ScourgeCoolState : byte
{
    Idle,
    Chasing,
    Shooting,
    Fleeing,
    Dead
}

public class ScourgeCoolController : MonoBehaviour, INetworkObject, IDamagableLocal, IKillableThing
{
    public GameObject remoteAgent_prefab;
    public ParticleSystem eye_ps;
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
    
    void Awake()
    {
        spawnedObjectComp = GetComponent<SpawnedObject>();
        
        InitJoints();
        DisableSkeleton();
        
        slide_ps_main = slide_ps.emission;
        
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
        if(state != ScourgeCoolState.Dead)
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
                        SetState(ScourgeCoolState.Chasing);
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
                if(state == ScourgeCoolState.Dead)
                {
                    return;
                }
                
                byte _state = (byte)args[0];
                
                InGameConsole.LogFancy(string.Format("SetState <color=yellow>{0}</color>", (ScourgeCoolState)_state));
                
                if((ScourgeCoolState)_state == ScourgeCoolState.Shooting)
                {
                    Vector3 _shootingPos = (Vector3)args[1];
                    SetMovePos(_shootingPos);
                    WarpRemoteAgent(_shootingPos);
                    UpdateRemoteAgentDestination(_shootingPos);
                }
                SetState((ScourgeCoolState)_state);
                
                
                break;
            }
            case(NetworkCommand.Flee):
            {
                UnlockSendingCommands();
                
                Vector3 _fleeingPos = (Vector3)args[0];
                
                fleeingPos = _fleeingPos;
                currentDestination = _fleeingPos;
                
                SetState(ScourgeCoolState.Fleeing);
                
                break;
            }
            case(NetworkCommand.Shoot):
            {
                UnlockSendingCommands();
                
                Vector3 _shotPos = (Vector3)args[0];
                Vector3 _shotDir = (Vector3)args[1];
                
                //SetMovePos(_shotPos);
                //WarpRemoteAgent(_shotPos);
                //UpdateRemoteAgentDestination(_shotPos);
                
                if(state != ScourgeCoolState.Dead)
                {
                    if(shotsPerformed % 4 == 0)
                        anim.Play("Base.Fire_R", 0, 0);
                    else if (shotsPerformed % 3 == 0)
                        anim.Play("Base.Fire_L", 0, 0);
                    else if(shotsPerformed % 2 == 0)
                        anim.Play("Base.Fire_R_s", 0, 0);
                    else
                        anim.Play("Base.Fire_L_s", 0, 0);
                }
                
                
                shootingDirection = _shotDir;
                
                Shoot(_shotPos + new Vector3(0, offsetShootingY, 0), _shotDir);
                //SetState(ScourgeCoolState.Shooting);
                
                break;
            }
            case(NetworkCommand.TakeDamageExplosive):
            {
                int incomingDamage = (int)args[0];
                
                int _incomingDamage = incomingDamage;
                if(_incomingDamage > HitPoints)
                    _incomingDamage = HitPoints;
                int small_healing_times = _incomingDamage / UberManager.HEALING_DMG_THRESHOLD;
                HealthCrystalSmall.MakeSmallHealing(thisTransform.localPosition + new Vector3(0, 1.5f, 0), small_healing_times);
                
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
                HealthCrystalSmall.MakeSmallHealing(thisTransform.localPosition + new Vector3(0, 1.5f, 0), small_healing_times);
                
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
                HealthCrystalSmall.MakeSmallHealing(thisTransform.localPosition + new Vector3(0, 1.5f, 0), small_healing_times);
                
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
    
    const float projectileSpeed = 55F;
    const float projectileRadius = 0.33F;
    const int projectileDamage = 12;
    //const int shotsPerRound = 3;
    int shotsPerformed = 0; 
    
    const float shootMaxDistance = 55;
    const float shootingChasingCooldown = 2.25F;
    float shootingChasingTimer = 0;
    
    const float shootingDuration = 1.9F;//0.5F;
    
    
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
        
        GameObject bullet = ObjectPool.s().Get(ObjectPoolKey.Bullet_yellow);
        bool isBulletMine = PhotonNetwork.IsMasterClient;
        bullet.GetComponent<BulletController>().LaunchAsSphere(shotPos, shotDir, projectileRadius, projectileMask, projectileSpeed, projectileDamage, isBulletMine);
        
        AudioManager.Play3D(SoundType.shoot2, shotPos, 1F, 0.6f);
        DoLight(shotPos);
    }
    
    void SetState(ScourgeCoolState _state)
    {
        if(state == ScourgeCoolState.Dead)
        {
            return;
        }
        
        
        switch(_state)
        {
            case(ScourgeCoolState.Chasing):
            {
                distanceTravelledRunningSqr = 0;
                brainTimer = 0;
                break;
            }
            case(ScourgeCoolState.Fleeing):
            {
                distanceTravelledRunningSqr = 0;
                break;
            }
            case(ScourgeCoolState.Shooting):
            {
                if(eye_ps)
                    eye_ps.Play();
                brainTimer = 0.33F;
                shotsPerformed = 0;
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
        if(state == ScourgeCoolState.Dead)
        {
            return;
        }
        
        //InGameConsole.LogOrange("DieFromExplosion()");
        
        SetState(ScourgeCoolState.Dead);
        
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
        if(state == ScourgeCoolState.Dead)
        {
            return;
        }
        
        SetState(ScourgeCoolState.Dead);
        
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
        DropHealthCrystals();
    }
    
    void DropHealthCrystals()
    {
        HealthCrystal hc = ObjectPool.s().Get(ObjectPoolKey.HealthCrystal).GetComponent<HealthCrystal>();
        hc.Launch(this.transform.localPosition + new Vector3(0, 1.25f, 0), 10);
        hc = ObjectPool.s().Get(ObjectPoolKey.HealthCrystal).GetComponent<HealthCrystal>();
        hc.Launch(this.transform.localPosition + new Vector3(0, 1.25f, 0), 10);
        hc = ObjectPool.s().Get(ObjectPoolKey.HealthCrystal).GetComponent<HealthCrystal>();
        hc.Launch(this.transform.localPosition + new Vector3(0, 1.25f, 0), 10);
        hc = ObjectPool.s().Get(ObjectPoolKey.HealthCrystal).GetComponent<HealthCrystal>();
        hc.Launch(this.transform.localPosition + new Vector3(0, 1.25f, 0), 10);
    }
    
    const int MaxHealth = 2500;
    public int HitPoints = MaxHealth;
    
    public int GetCurrentHP()
    {
        return HitPoints;
    }
    
    public void TakeDamageLocally(int dmg, Vector3 hitPos, Vector3 hitDir)
    {
        if(state != ScourgeCoolState.Dead && damage_taken_timeStamp + 0.15F < Time.time)
        {
            float vol = Random.Range(0.8f, 1f);
            //audio_src.PlayOneShot(clipHurt1, vol);
            
            damage_taken_timeStamp = Time.time;
        }
    }
    
    public bool IsDead()
    {
        if(state == ScourgeCoolState.Dead)
            return true;
        else
            return false;
    }
    
    public ScourgeCoolState state = ScourgeCoolState.Idle;
    
    const float PATH_UPDATE_BASE = 0.2F;//0.15F;
    float path_update_cd;
    float brainTimer = 0;
    
    void SetMovePos(Vector3 pos)
    {
        currentDestination = pos;
    }
    
    Vector3 currentDestination;
    static readonly Vector3 vUp = new Vector3(0, 1, 0);
    Quaternion deriv;
    
    const float moveSpeed = 10F;
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
    const float fleeingRange = 9F;
    
    void UpdateBrain(float dt)
    {
        switch(state)
        {
            case(ScourgeCoolState.Idle):
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
            case(ScourgeCoolState.Chasing):
            {
                brainTimer += dt;
                shootingChasingTimer += dt;
                
                if(target_pc)
                {
                    Vector3 targetGroundPos = target_pc.GetGroundPosition();
                    //UpdateRemoteAgentDestination(targetGroundPos);
                    
                    Vector3 shootingCheckPosOffsetted = currentDestination;
                    shootingCheckPosOffsetted.y += offsetShootingY;
                    
                    Vector3 offsettedTargetHeadPos = target_pc.GetHeadPosition();
                    offsettedTargetHeadPos.y -= 0.25F;
                    
                    bool canShootFromDestination = CanShootAtPos(shootingCheckPosOffsetted, offsettedTargetHeadPos, shootMaxDistance);
                    
                    if(!canShootFromDestination)
                    {
                        //InGameConsole.LogFancy("ScourgeCool: Can't shoot from this position");
                        UpdateRemoteAgentDestination(targetGroundPos);
                        if(shootingChasingTimer > shootingChasingCooldown * 0.5F)
                        {
                            shootingChasingTimer = shootingChasingCooldown * Random.Range(0.4F, 0.6F);
                        }
                    }
                    
                    if(canSendCommands)
                    {
                        if(canShootFromDestination)
                        {
                            if(shootingChasingTimer >= shootingChasingCooldown)
                            {
                                shootingChasingTimer = Random.Range(-0.3F, 0.4F);
                                brainTimer = 0;
                                
                                Vector3 _shootingPos = currentDestination;
                                    
                                LockSendingCommands();
                                NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.SetState, (byte)ScourgeCoolState.Shooting, _shootingPos);
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
                            NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.SetState, (byte)ScourgeCoolState.Idle);   
                        }
                    }
                    
                }
                
                break;
            }
            case(ScourgeCoolState.Shooting):
            {
                brainTimer += dt;
                if(canSendCommands)
                {
                    if(target_pc)
                    {
                        bool shouldShoot = false;
                        switch(shotsPerformed)
                        {
                            case 0:
                            {
                                if(brainTimer > 0.4F)
                                    shouldShoot = true;
                                break;
                            }
                            case 1:
                            {
                                if(brainTimer > 0.65F)
                                    shouldShoot = true;
                                break;
                            }
                            case 2:
                            {
                                if(brainTimer > 0.9F)
                                    shouldShoot = true;
                                break;
                            }
                            case 3:
                            {
                                if(brainTimer > 1.15F)
                                    shouldShoot = true;
                                break;
                            }
                            case 4:
                            {
                                if(brainTimer > 1.4F)
                                    shouldShoot = true;
                                break;
                            }
                            case 5:
                            {
                                if(brainTimer > 1.65F)
                                    shouldShoot = true;
                                break;
                            }
                            default:
                            {
                                
                                break;
                            }
                        }
                        if(shouldShoot)
                        {
                            Vector3 targetGroundPos = target_pc.GetGroundPosition();
                            Vector3 shootingCheckPosOffsetted = currentDestination;
                            shootingCheckPosOffsetted.y += offsetShootingY;
                            
                            Vector3 offsettedTargetHeadPos = target_pc.GetHeadPosition();
                            offsettedTargetHeadPos.y -= 0.25F;
                            
                            bool canShootFromDestination = CanShootAtPos(shootingCheckPosOffsetted, offsettedTargetHeadPos, shootMaxDistance);
                            
                            
                            if(canShootFromDestination)
                            {
                                shotsPerformed++;
                                Vector3 _shotPos = currentDestination;
                                Vector3 _shotDir = (offsettedTargetHeadPos - shootingCheckPosOffsetted).normalized;
                                NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.Shoot, _shotPos, _shotDir);
                            }
                            else
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
                                    NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.SetState, (byte)ScourgeCoolState.Idle);
                                }
                            }
                        }
                        else if (brainTimer > shootingDuration)
                        {
                            NavMeshHit navMeshHit;
                        
                            int samplingIterationNumber = 0;
                            bool foundNavPos = false;
                            
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
                                        LockSendingCommands();
                                        NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.SetState, (byte)ScourgeCoolState.Idle);
                                    }
                                    else
                                    {
                                        Vector3 _fleeingPos = navMeshHit.position;
                                        LockSendingCommands();
                                        //InGameConsole.LogFancy("<color=green>FLEE OK</color>");
                                        NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.Flee, _fleeingPos);
                                        
                                    }
                                }
                                else
                                {
                                    LockSendingCommands();
                                    NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.SetState, (byte)ScourgeCoolState.Chasing);
                                }
                            }
                        }
                    }
                    else
                    {
                        InGameConsole.LogOrange("<color=yellow>No target</color>");
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
                            NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.SetState, (byte)ScourgeCoolState.Idle);
                        }
                    }
                }
                
                break;
            }
            case(ScourgeCoolState.Fleeing):
            {
                UpdateRemoteAgentDestination(fleeingPos);
                
                if(canSendCommands)
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
                            NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.SetState, (byte)ScourgeCoolState.Idle);
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
            case(ScourgeCoolState.Dead):
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
    }
#endif
    
    const float animDampTime = 0.05F;
    
    
    const float footStepDistance = 1.8F;
    float distanceTravelledRunningSqr;
    
    
    Vector3 shootingDirection;
    
    public ParticleSystem slide_ps;
    ParticleSystem.EmissionModule slide_ps_main;
    
    void UpdateBrainLocally(float dt)
    {
        switch(state)
        {
            case(ScourgeCoolState.Idle):
            {
                slide_ps_main.rateOverTime = 0;
                break;
            }
            case(ScourgeCoolState.Chasing):
            {
                Vector3 currentPos = thisTransform.localPosition;
                
                float dV = speedMult * dt * moveSpeed;
                Vector3 updatedPos = Vector3.MoveTowards(currentPos, currentDestination, dV);
                thisTransform.localPosition = updatedPos; 
                
                if(currentDestination == currentPos)
                {
                    if(target_pc)
                        RotateToLookAt(target_pc.GetGroundPosition(), rotateTimeAtTarget * speedMult, false);
                }
                else
                    RotateToLookAt(currentDestination, rotateTime * speedMult, false);
                
                Vector3 dPos = currentDestination - currentPos;
                
               
                slide_ps_main.rateOverTime = 0;
                
                
                anim.SetFloat(MoveSpeedHash, Math.Magnitude(dPos), animDampTime, dt);
                
                distanceTravelledRunningSqr += Math.Magnitude(updatedPos - currentPos);
                
                if(distanceTravelledRunningSqr > footStepDistance * footStepDistance)
                {
                    distanceTravelledRunningSqr -= footStepDistance * footStepDistance;
                    if(distanceTravelledRunningSqr  > footStepDistance * footStepDistance)
                        distanceTravelledRunningSqr = 0;
                    
                    audio_src.PlayOneShot(clipStep, 0.33F);
                }
                
                break;
            }
            case(ScourgeCoolState.Shooting):
            {
                Vector3 currentPos = thisTransform.localPosition;
                
                float dV = speedMult * dt * moveSpeed;
                Vector3 updatedPos = Vector3.MoveTowards(currentPos, currentDestination, dV);
                thisTransform.localPosition = updatedPos; 
                slide_ps_main.rateOverTime = 0;
                //RotateToLookAt(currentDestination, rotateTime * speedMult, false);
                Vector3 shootingDirectionXZ = Math.GetXZ(shootingDirection);
                if(target_pc)
                {
                    shootingDirectionXZ = Math.GetXZ(target_pc.GetGroundPosition() - thisTransform.localPosition);
                }
                
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
                    
                    audio_src.PlayOneShot(clipStep, 0.6F);
                }
                
                break;
            }
            case(ScourgeCoolState.Fleeing):
            {
                Vector3 currentPos = thisTransform.localPosition;
                
                float dV = speedMult * dt * moveSpeed;
                Vector3 updatedPos = Vector3.MoveTowards(currentPos, currentDestination, dV);
                thisTransform.localPosition = updatedPos; 
                
                RotateToLookAt(currentDestination, rotateTime * speedMult, false);
                
                Vector3 dPos = currentDestination - currentPos;
                
                slide_ps_main.rateOverTime = 25;
                
                anim.SetFloat(MoveSpeedHash, Math.Magnitude(dPos), animDampTime, dt);
                
                distanceTravelledRunningSqr += Math.Magnitude(updatedPos - currentPos);
                
                if(distanceTravelledRunningSqr > footStepDistance * footStepDistance)
                {
                    distanceTravelledRunningSqr -= footStepDistance * footStepDistance;
                    if(distanceTravelledRunningSqr  > footStepDistance * footStepDistance)
                        distanceTravelledRunningSqr = 0;
                    
                    audio_src.PlayOneShot(clipStep, 0.4f);
                }
                
                break;
            }
            case(ScourgeCoolState.Dead):
            {
                slide_ps_main.rateOverTime = 0;
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
    
    
    [Header("Clips:")]
    public AudioClip clipStep;
    public AudioClip clipHurt1;
    public AudioClip clipDeath;
    public AudioClip clipLaunchedAirborne;
    public AudioClip clipShoot;
}
