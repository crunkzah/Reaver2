using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;
using System.Collections.Generic;

public class Shooter3_shotgun : MonoBehaviour, INetworkObject, IDamagableLocal, IPooledObject, IRemoteAgent
{
    [Header("Audio:")]
    public AudioClip shootClip;
    
    
    public GameObject remoteAgentPrefab;
    public ShooterState state;
    
    Transform thisTransform;
    NetworkObject net_comp;
    NavMeshAgent remoteAgent;
    Animator anim;
    AudioSource audioSource;
    Material        normalMaterial;
    SkinnedMeshRenderer smr;
    CapsuleCollider capsuleCol;
    
    Transform target;
    PlayerController target_pc;
    
    OnDamageTakenEffects damage_fx;
    
    static int groundMask = -1;
    static int playerGroundMask = -1;
    
    
    const float updatePathCD = 0.33F;
    float pathTimer = 0f;
    
    bool canSendCommands = false;
    
    int shotsPerRound = 8;
    float timeBetweenShots = 0.3f;
    float loadingDuration = 1.5f;
    float loadingTimer = 0f;
    
    const float attackCD = 1.75f;
    float attackingTimer = 0f;
    
    
    void SetupRemoteAgent()
    {
        GameObject remoteAgentObj = Instantiate(remoteAgentPrefab, Vector3.one * 2000, Quaternion.identity);
        remoteAgentObj.SetActive(false);
        
        DontDestroyOnLoad(remoteAgentObj);
        remoteAgent = remoteAgentObj.GetComponent<NavMeshAgent>();
        remoteAgent.updateRotation = false;
    }
    
    void Awake()
    {
        if(groundMask == -1)
        {
            groundMask = LayerMask.GetMask("Ground");
            playerGroundMask = LayerMask.GetMask("Player", "Ground");
        }
        
        limbForExplosions = GetComponent<LimbForExplosions>();
        SetupAnimator();
        capsuleCol = GetComponent<CapsuleCollider>();
        ragdollOwner = GetComponent<RagdollOwner>();
        
        thisTransform = transform;
        net_comp = GetComponent<NetworkObject>();
        
        audioSource = GetComponent<AudioSource>();
        damage_fx = GetComponent<OnDamageTakenEffects>();
        SetupRemoteAgent();
        
        
        
        
        
        
        smr = GetComponentInChildren<SkinnedMeshRenderer>();
        normalMaterial = smr.sharedMaterial;
        damage_fx.SetupObject(smr, normalMaterial);
        
        if(playerMask == -1)
        {
            playerMask = LayerMask.GetMask("Player");
        }
        
        
        
    }
    
    
    //Animation:
    public Transform armature; 
    
    
    bool canSendMove = false;
    
    void UnlockMoveCommand()
    {
        canSendMove = true;
    }
    
    void LockMoveCommand()
    {
        canSendMove = false;
    }
    
    void LockSendingCommands()
    {
        canSendCommands = false;
    }
    
    void UnlockSendingCommands()
    {
        canSendCommands = true;
    }
    
    const int MaxHealth = 1000;
    public int HitPoints = 0;
    
    public int GetCurrentHP()
    {
        return HitPoints;
    }
    
    void SetTarget(Transform t)
    {
        target = t;
        target_pc = t.GetComponent<PlayerController>();
    }
    
    void ClearTarget()
    {
        target = null;
        target_pc = null;
    }
    
    void EnableRemoteAgent()
    {
        remoteAgent.gameObject.SetActive(true);
    }
    
    void DisableRemoteAgent()
    {
        remoteAgent.gameObject.SetActive(false);
        if(remoteAgent.hasPath)
            remoteAgent.ResetPath();
    }
    
    public void RemoteAgentOnSpawn(Vector3 pos)
    {
        NavMeshHit navMeshHit;
        if(NavMesh.SamplePosition(pos, out navMeshHit, 1000f, NavMesh.AllAreas))
        {
            Vector3 _spawnPos = navMeshHit.position;
            EnableRemoteAgent();
                
            remoteAgent.Warp(_spawnPos);        
            
                      
        }
        
        //InGameConsole.LogOrange("RemoteAgentOnSpawn()");
    }
    
    public void InitialState()
    {
        ClearTarget();
        capsuleCol.enabled = true;
        wasLocalPlayerHurt = false;
        airborneVelocity = new Vector3(0, 0, 0);
        
        HitPoints = MaxHealth;
        DisableRemoteAgent(); 
       
        ResetTimers();
        hasDestination = false;
        SetState(ShooterState.Idle);
           
        UnlockMoveCommand();
        UnlockSendingCommands();
    }
    
    
    
  
    
    Vector3 destination;
    bool hasDestination = false;
    void SetDestinationAndDirection(Vector3 pos)
    {
        if(Math.SqrDistance(thisTransform.position, pos) > 0.01f)
        {
            Vector3 dir = (pos - thisTransform.position);
            dir.y = 0;
            dir = Math.Normalized(dir);
            SetDirection(dir);
            
        }
        hasDestination = true;
        destination = pos;
        pathTimer = 0f;
    }
    
    void ClearDestination()
    {
        destination = thisTransform.position;
    }
    
    
    
    void ResetTimers()
    {
        loadingTimer = 0f;
        pathTimer = 0f;
        attackingTimer = 0f;
    }
    
    public ParticleSystem loading_ps;
    public ParticleSystem shoot_ps;
    
    float airborneWeightSpeed = 1.6f;
    
    void SetState(ShooterState _state)
    {
        switch(_state)
        {
            case(ShooterState.Idle):
            {
                target = null;
                target_pc = null;
                
                if(state == ShooterState.Loading)
                    GoRun_anim();
                    
                anim.SetLayerWeight(1, 0);
                
                break;
            }
            case(ShooterState.Loading):
            {
                Load_anim();
                loadingTimer = 0;
                if(loading_ps)
                {
                    loading_ps.Clear();
                    loading_ps.Play();
                }
                
                break;
            }
            case(ShooterState.Airborne):
            {
                
                
                break;
            }
        }
        
       
        state = _state;
    }
    
    public void ReceiveCommand(NetworkCommand command, params object[] args)
    {
        switch(command)
        {
            case(NetworkCommand.Shoot):
            {
                UnlockSendingCommands();
                Vector3 shootPos = (Vector3)args[0];
                Vector3 shootDir = (Vector3)args[1];
                
                ShootShotgun(shootPos, shootDir);
                                
                break;
            }
            case(NetworkCommand.LaunchAirborne):
            {
                Vector3 launchPos = (Vector3)args[0];
                Vector3 force = (Vector3)args[1];
                if(force.y <= 3f)
                {
                    force.y = 3f;
                }
                
                thisTransform.position = launchPos;
                
                if(force.x != 0 && force.z != 0)
                {
                    thisTransform.forward = Math.Normalized(Math.GetXZ(-force));
                }
                
                airborneVelocity = force;
                SetState(ShooterState.Airborne);
                DisableRemoteAgent();
                
                break;
            }
            case(NetworkCommand.SetState):
            {
                UnlockSendingCommands();
                
                ShooterState _state = (ShooterState)(int)args[0];
                
                SetState(_state);
            
                
                break;
            }
            case(NetworkCommand.SetTarget):
            {
                UnlockSendingCommands();
                
                int viewID = (int)args[0];
                
                PhotonView pv = PhotonNetwork.GetPhotonView(viewID);
                
                if(pv)
                {
                    SetTarget(pv.transform);
                    SetState(ShooterState.Chasing);
                }
                
                
                break;
            }
            case(NetworkCommand.TakeDamageLimbWithForce):
            {
                int incomingDamage = (int)args[0];
             
                TakeDamage(incomingDamage);
                
                break;
            }
            
            case(NetworkCommand.Move):
            {
                UnlockMoveCommand();
                Vector3 dest = (Vector3)args[0];
             
                SetDestinationAndDirection(dest);
                
                break;
            }
        }
    }
    
    void TakeDamage(int dmg)
    {
        HitPoints -= dmg;
        if(HitPoints <= 0)
        {
           
            HitPoints = 0;
        }
    }
    
    public void TakeDamageLocally(int dmg, Vector3 damagePoint, Vector3 bulletDir)
    {
        if(damage_fx)
        {
            damage_fx.OnTakeDamage(damagePoint, bulletDir);
        }
    }
    
    public bool IsDead()
    {
     //   if(state == PadlaState.Dead)
            return true;
     //   else
     //       return false;
    }
    
    void WarpAgentSafe(Vector3 pos)
    {
        NavMeshHit navMeshHit;
        if(NavMesh.SamplePosition(pos, out navMeshHit, 4, NavMesh.AllAreas))
        {
            remoteAgent.Warp(navMeshHit.position);    
        }
    }
    
    void Start()
    {
        prevPos = transform.position;
    }
    
    bool wasLocalPlayerHurt = false;
    
   
    
    public Transform gunPoint;
    
    const int attackDamage = 50;
    
    static int  playerMask = -1;
    
    void TryDoDamageToLocalPlayer()
    {
        if(wasLocalPlayerHurt || PhotonManager.Singleton().local_controller == null)
        {
            return;
        }
        
        // if(Physics.CheckSphere(HurtPoint.position, HurtPoint.localScale.x, playerMask))
        Vector3 localPlayerPos = PhotonManager.Singleton().local_controller.GetFPSPosition();
        float sqrDist = Math.SqrDistance(localPlayerPos, gunPoint.position);
        if(sqrDist < gunPoint.localScale.x)
        {
            wasLocalPlayerHurt = true;
            PhotonManager.Singleton().local_controller.TakeDamage(attackDamage);
            Vector3 boostVel = thisTransform.forward * 18f;
            
            PhotonManager.Singleton().local_controller.BoostVelocityAdditive(boostVel);
        }       
    }
    
    float attackDistance = 2.55f;
    public Transform shootingSphereCastPosition;
    float shootingDistance = 18f;
    
    public bool targetIsInRange = false;
    
    bool TargetIsInRange(Vector3 targetPos)
    {
        bool Result = false;
        float distanceToTarget = Vector3.Distance(targetPos, thisTransform.position);
        
        if(distanceToTarget < shootingDistance)
        {
            Vector3 dir = Math.Normalized(targetPos - shootingSphereCastPosition.position);
            
            
            //Debug.DrawRay(shootingSphereCastPosition.position, dir * distanceToTarget, Color.green, 1f);
            RaycastHit hit;
            if(Physics.SphereCast(shootingSphereCastPosition.position, projectileRadius, dir, out hit, distanceToTarget, groundMask))
            {
                Result = false;
            }
            else
                Result = true;
        }
        
        targetIsInRange = Result;
        
        return Result;
    }
    
    const float projectileRadius = 0.3f;
    const float projectileSpeed = 30f;
    float spread = 0.1f;
    
    
    void ShootShotgun(Vector3 pos, Vector3 dir)
    {
        GameObject bullet = ObjectPool.s().Get(ObjectPoolKey.Bullet_npc2);
        bool isBulletMine = PhotonNetwork.IsMasterClient;
        bullet.GetComponent<BulletController>().LaunchAsSphere(pos, dir, projectileRadius, playerGroundMask, projectileSpeed, 25, isBulletMine);
        
        Quaternion q = Quaternion.LookRotation(dir, Vector3.up);
        
        for(int i = 0; i < shotsPerRound; i++)
        {
            bullet = ObjectPool.s().Get(ObjectPoolKey.Bullet_npc2);
            Vector3 v = new Vector3(0, 0, 1);
            v += spread * Math.shotDirRadial8[i];
            v = Math.Normalized(v);
            
            Vector3 finalDir = q * v;
            
            bullet.GetComponent<BulletController>().LaunchAsSphere(pos, finalDir, projectileRadius, playerGroundMask, projectileSpeed, 25, isBulletMine);
        }
        
        
        audioSource.PlayOneShot(shootClip);
        anim.SetTrigger("Shoot");
        shoot_ps.Play();
    }
    
    
   
    
    void UpdateBrain()
    {
        float dt = UberManager.DeltaTime();
        
        switch(state)
        {
            case(ShooterState.Idle):
            {
                if(target == null && canSendCommands)
                // if(canSendCommands)
                {
                    Transform _target = ChooseTargetClosest(thisTransform.position);
                    if(_target != null)
                    {
                        int viewID = _target.GetComponent<PhotonView>().ViewID;
                        LockSendingCommands();
                        NetworkObjectsManager.PackNetworkCommand(net_comp.networkId, NetworkCommand.SetTarget, viewID);
                    }
                }
                
                break;
            }
            case(ShooterState.Chasing):
            {
                if(target)
                {
                    if(canSendMove)
                    {
                        if(pathTimer > updatePathCD)
                        {
                            if(!TargetIsInRange(target_pc.GetHeadPosition()))
                            {
                                LockMoveCommand();
                                // Vector3 dest = GetDestination(target_pc.GetGroundPosition());
                                Vector3 dest = GetDestination(target_pc.GetGroundPositionPredicted());
                                
                                
                                NetworkObjectsManager.PackNetworkCommand(net_comp.networkId, NetworkCommand.Move, dest);
                            }
                            else
                            {
                                if(destination != thisTransform.position)
                                {
                                    LockMoveCommand();
                                    Vector3 dest = GetDestination(thisTransform.position);
                                    WarpAgentSafe(thisTransform.position);
                                    
                                    if(remoteAgent.hasPath)
                                        remoteAgent.ResetPath();
                                        
                                    //NetworkObjectsManager.ScheduleCommand(net_comp.networkId, UberManager.GetPhotonTimeDelayedBy(0.05f), NetworkCommand.Move, dest);
                                    NetworkObjectsManager.PackNetworkCommand(net_comp.networkId, NetworkCommand.Move, dest);
                                }
                                
                                
                                pathTimer = 0f;
                            }
                        }
                        else
                        {
                            // if(!TargetIsInRange(target.position))
                            // {
                            //     UpdateRemoteAgent(target_pc.GetGroundPositionPredicted(50f));
                            // }
                            pathTimer += dt;
                        }
                    }
                    
                    if(attackingTimer > attackCD && canSendCommands)
                    {
                        LockSendingCommands();
                        Vector3 dest = GetDestination(thisTransform.position);
                        WarpAgentSafe(thisTransform.position);
                        
                        if(remoteAgent.hasPath)
                            remoteAgent.ResetPath();
                            
                        attackingTimer = 0;
                        NetworkObjectsManager.PackNetworkCommand(net_comp.networkId, NetworkCommand.SetState, (int)ShooterState.Loading);
                    }
                    else
                    {
                        attackingTimer += dt;
                    }
                }
                else
                {
                    if(canSendCommands)
                    {
                        Transform _target = ChooseTargetClosest(thisTransform.position);
                        if(_target != null)
                        {
                            int viewID = _target.GetComponent<PhotonView>().ViewID;
                            LockSendingCommands();
                            NetworkObjectsManager.PackNetworkCommand(net_comp.networkId, NetworkCommand.SetTarget, viewID);
                        }
                    }
                }
                
                break;
            }
            case(ShooterState.Loading):
            {
                if(target)
                {
                    if(loadingTimer > loadingDuration)
                    {
                        if(canSendCommands)
                        {
                            LockSendingCommands();
                            Vector3 shootPos = gunPoint.position;
                            Vector3 shootDir = (target_pc.GetHeadPosition() - shootPos);
                            shootDir = Math.Normalized(shootDir);
                            
                            
                            NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.Shoot, shootPos, shootDir);
                            
                            NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.SetState, (int)ShooterState.Idle);
                            
                        }
                    }
                    else
                    {
                        loadingTimer += dt;
                    }
                }
                else
                {
                    LockSendingCommands();
                    NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.SetState, (int)ShooterState.Idle);
                }
                break;
            }
            case(ShooterState.Airborne):
            {
                    
                break;
            }
        }
    }
    
    public Vector3 airborneVelocity;
    
    void Die(Vector3 force)
    {
        if(state != ShooterState.Dead)
        {
            capsuleCol.enabled = false;
            HitPoints = 0;
            
            
            DisableRemoteAgent();
            SetState(ShooterState.Dead);
            
               
            OnDie(force);
            ClearTarget();
        }
    }
    
    public RagdollOwner ragdollOwner;
    LimbForExplosions limbForExplosions;
    
    void OnDie(Vector3 force)
    {
        limbForExplosions.ExplodeEveryLimb();
        //ragdollOwner.ApplyForceToHead(Vector3.up * 450 + Vector3.zero * 10);
        AudioManager.Play3D(SoundType.death1_npc, thisTransform.position, Random.Range(0.6f, 0.67f));
        //MakeGibs(thisTransform.position + new Vector3(0, 2.25f, 0), force);
        this.gameObject.SetActive(false);
    }
    
    static Vector3 vZero = new Vector3(0, 0, 0);
    
    void MakeGibs(Vector3 pos, Vector3 _force)
    {
        int quantity = Random.Range(12, 16);
        
        Vector3 offset;
        Vector3 vel = new Vector3(0, 0, 0);
        Vector3 dir;
        
        GameObject gib;
        
        for(int i = 0; i < quantity; i++)
        {
            gib = ObjectPool.s().Get(ObjectPoolKey.FlyingGib1, false);
            offset = Random.insideUnitSphere * 1f;
            vel.y = Random.Range(3.75f, 5.5f) * 2f;
            
            dir = Math.Normalized(offset);
            vel.x = dir.x * Random.Range(4.5f, 6.25f) * 2.25f;
            vel.z = dir.z * Random.Range(4.5f, 6.25f) * 2.25f;
            
            gib.GetComponent<FlyingGib>().Launch(pos + offset, vel);
            
        }
        
        gib = ObjectPool.s().Get(ObjectPoolKey.FlyingGib1, false);
        gib.GetComponent<FlyingGib>().Launch(pos, vZero);
    }

    // Update is called once per frame
    void Update()
    {
        if(PhotonNetwork.IsMasterClient)
        {
            UpdateBrain();
        }
        
        UpdateBrainLocally();
    }
    
    
    readonly static Vector3 vOne = new Vector3(1, 1, 1);
    
    void LateUpdate()
    {
        if(armature.localScale.x != 1)
        {
            armature.localScale = vOne;
            smr.transform.localScale = vOne;
        }
        float dt = UberManager.DeltaTime();
        
        currentVel = (thisTransform.position - prevPos) / dt;
        prevPos = thisTransform.position;
        
        anim.SetFloat(MoveSpeedAnimHash, Math.Magnitude(currentVel));
    }
    Vector3 currentVel;
    float moveSpeed = 5f;
    
    Vector3 prevPos;
    
    void UpdateBrainLocally()
    {
        float dt = UberManager.DeltaTime();
        switch(state)
        {
            case(ShooterState.Idle):
            {
                MoveTransform();
                RotateToDesiredDirection();
                
                break;
            }
            case(ShooterState.Chasing):
            {
                MoveTransform();
                
                if(target)
                {
                    // InGameConsole.LogFancy("CurrentVelSqr: " + Math.Magnitude(currentVel));
                    if(Math.SqrMagnitude(currentVel) < 1.5f * 1.5f)
                    {
                        Vector3 dirToTarget = (target.position - thisTransform.position);
                        dirToTarget.y = 0;
                        dirToTarget = Math.Normalized(dirToTarget);
                        SetDirection(dirToTarget);
                    }
                }
                
                RotateToDesiredDirection();
                
                break;
            }
            case(ShooterState.Loading):
            {
                MoveTransformNoAnimation();
                if(target)
                {
                    Vector3 dirToTarget = (target.position - thisTransform.position);
                    dirToTarget.y = 0;
                    dirToTarget = Math.Normalized(dirToTarget);
                    SetDirection(dirToTarget);
                }
                RotateToDesiredDirection();
                
                break;
            }
            case(ShooterState.Airborne):
            {
                MoveTransformAirborne();
                float w = anim.GetLayerWeight(1);
                if(w != 1f)
                {
                    w = Mathf.MoveTowards(w, 1, airborneWeightSpeed * dt);  
                    anim.SetLayerWeight(1, w);
                }       
                
                
                break;
            }
        }
    }
    
    AnimatorStateInfo animState;
    
    
    int LoadTrigger;
    int GoRunTrigger;
    int MoveSpeedAnimHash;
    
    void SetupAnimator()
    {
        anim = GetComponent<Animator>();
        if(anim == null)
        {
            anim = GetComponentInChildren<Animator>();
        }
       
        GoRunTrigger = Animator.StringToHash("GoRun");
        LoadTrigger = Animator.StringToHash("Load");
        
        MoveSpeedAnimHash = Animator.StringToHash("MoveSpeed");
    }
    
  
   void GoRun_anim()
   {
       InGameConsole.LogFancy("GoRunAnim()");
       anim.SetTrigger(GoRunTrigger);
   }
    
    void Load_anim()
    {
        InGameConsole.LogOrange("LoadAnim()");
        anim.SetTrigger(LoadTrigger);
        anim.ResetTrigger(GoRunTrigger);
        anim.ResetTrigger("Shoot");
    }
    
    
    void MoveTransform()
    {
        float dt = UberManager.DeltaTime();
        Vector3 currentPos = thisTransform.localPosition;
        Vector3 updatedPos = currentPos;
        
        if(hasDestination)
        {
            currentPos = thisTransform.localPosition;
            
            updatedPos = Vector3.MoveTowards(currentPos, destination, moveSpeed * dt);
            
            thisTransform.localPosition = updatedPos;
        }   
        
        
        
    }
    
    void RotateToDesiredDirection()
    {
        float dt = UberManager.DeltaTime();
        thisTransform.forward = Vector3.RotateTowards(thisTransform.forward, desiredDirection, Mathf.PI * 2 * dt, 2 * dt);
    }
    
    void MoveTransformNoAnimation()
    {
        float dt = UberManager.DeltaTime();
        Vector3 currentPos = thisTransform.localPosition;
        Vector3 updatedPos = currentPos;
        
        if(hasDestination)
        {
            currentPos = thisTransform.localPosition;
            
            updatedPos = Vector3.MoveTowards(currentPos, destination, moveSpeed * dt);
            
            thisTransform.localPosition = updatedPos;
        }   
      
    }
    
    static Vector3 vUp = new Vector3(0, 1, 0);
    
    void MoveTransformAirborne()
    {
        float dt = UberManager.DeltaTime();
        
        Vector3 currentPos = thisTransform.localPosition;
        Vector3 updatedPos = currentPos;
        
        airborneVelocity.y += -9.81F * 1.25f * dt;
        airborneVelocity.x = Mathf.MoveTowards(airborneVelocity.x, 0f, 2 * dt);
        airborneVelocity.z = Mathf.MoveTowards(airborneVelocity.z, 0f, 2 * dt);
        
        //thisTransform.forward = Math.Normalized(Math.GetXZ(-airborneVelocity));
        
        {
            RaycastHit hit;
            
            Vector3 p1 = thisTransform.position;
            p1.y += capsuleCol.center.y;
            p1.y += capsuleCol.height / 2;
            p1.y -= capsuleCol.radius;
            
            
            
            
            Vector3 p2 = thisTransform.position;
            p2.y += capsuleCol.center.y;
            p2.y -= capsuleCol.height / 2;
            p2.y += capsuleCol.radius;
            
            Vector3 airborneDir = Math.Normalized(airborneVelocity);
            float airborneMagnitude = Math.Magnitude(airborneVelocity);
            
            if(Physics.CapsuleCast(p1, p2, capsuleCol.radius, airborneDir, out hit, airborneMagnitude * dt, groundMask))
            {
                float dot = Vector3.Dot(hit.normal, airborneDir);
                    
                Vector3 calculatedV = airborneVelocity + hit.normal * airborneMagnitude * Math.Abs(dot);
                if(Vector3.Dot(hit.normal, vUp) > 0.707f)
                {
                    if(PhotonNetwork.IsMasterClient && canSendCommands)
                    {
                        LockSendingCommands();
                        if(Math.SqrMagnitude(airborneVelocity) > 36 * 36)
                        {
                        }
                        else
                        {
                            //remoteAgent.Warp(thisTransform.position);
                            WarpAgentSafe(thisTransform.position);
                            EnableRemoteAgent();
                            NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.SetState, (int)ShooterState.Idle);
                        }
                    }
                    airborneVelocity = new Vector3(0, 0, 0);
                    hasDestination = false;
                    // InGameConsole.LogOrange("Collision!");
                    SetState(ShooterState.Idle);
                }
                
                if(Physics.CapsuleCast(p1, p2, capsuleCol.radius, Math.Normalized(calculatedV), out hit, Math.Magnitude(calculatedV) * dt, groundMask))
                {
                    calculatedV = Vector3.zero;
                }
                
                airborneVelocity = calculatedV;
            }
            else
            {
                if(PhotonNetwork.IsMasterClient)
                {
                    if(airborneVelocity.y < -50)
                    {
                        if(canSendCommands)
                        {
                             LockSendingCommands();
                        }
                    }
                }
                
            }
            
            
            // Debug.DrawRay(p1, airborneDir, Color.blue);
            // Debug.DrawRay(p2, airborneDir, Color.red);
        }
        
       
        updatedPos += airborneVelocity * dt;
        
        
        thisTransform.localPosition = updatedPos;
    }
    
    public Vector3[] debugCorners;
    
    void UpdateRemoteAgent(Vector3 pos)
    {
        //remoteAgent.Warp(pos);
        remoteAgent.SetDestination(pos);
    }
    
    Vector3 GetDestination(Vector3 targetPos)
    {
        Vector3 Result = thisTransform.position;
        
        remoteAgent.SetDestination(targetPos);
        
        if(Math.SqrDistance(remoteAgent.transform.position, thisTransform.position) > 2 * 2)
        {
            Vector3 dirToAgent = (remoteAgent.transform.position - thisTransform.position);
            dirToAgent = Math.Normalized(dirToAgent);
            
            return thisTransform.position + dirToAgent * 2;
        }
        
        return remoteAgent.transform.position;
    }
    
    Vector3 desiredDirection = Vector3.right;
    
    void SetDirection(Vector3 dir)
    {
        desiredDirection = dir;
    }
    
    void SetDestinationWithoutDirection(Vector3 pos)
    {
        // SetDestinationAndDirection(pos);
        hasDestination = true;
        destination = pos;
        pathTimer = 0f;
        
        if(PhotonNetwork.IsMasterClient)
            UpdateRemoteAgent(pos);
    }
    
#if UNITY_EDITOR
    GUIStyle style = new GUIStyle();
    void OnDrawGizmos()
    {
        if(Application.isPlaying && thisTransform != null)
        {
            
            
            Gizmos.color = Color.red;
            
            if(hasDestination)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(thisTransform.position, destination);
                Gizmos.DrawWireSphere(destination, 0.75f);
            }
        
            
            // string txt = string.Format("{0}\nHP: {1}", state, HitPoints);
            string txt = string.Format("{0}\nTargetInRange: {1}", state, targetIsInRange);
            
            style.alignment = TextAnchor.MiddleCenter;
            style.richText = true;
            style.fontStyle = FontStyle.Bold;
            style.normal.textColor = Colors.Orange;
            
            UnityEditor.Handles.Label(thisTransform.position + Vector3.up * 2.5f, txt, style);
        }
    }
    
#endif
    
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
    
    
}