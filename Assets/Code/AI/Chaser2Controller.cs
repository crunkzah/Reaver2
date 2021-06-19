using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

public enum ChaserState : int
{
    Idle,
    Chasing,
    Attacking,
    Airborne,
    Dead
}

public class Chaser2Controller : MonoBehaviour, INetworkObject, IDamagableLocal, IPooledObject, IRemoteAgent
{
    public GameObject remoteAgentPrefab;
    public ChaserState state;
    
    Transform thisTransform;
    NetworkObject net_comp;
    NavMeshAgent remoteAgent;
    Animator anim;
    AudioSource audioSource;
    Material        normalMaterial;
    SkinnedMeshRenderer smr;
    CapsuleCollider capsuleCol;
    
    
    NavMeshPath navMeshPath;
    Transform target;
    PlayerController target_pc;
    
    OnDamageTakenEffects damage_fx;
    
    static int groundMask = -1;
    
    
    const float updatePathCD = 0.175F;
    float pathTimer = 0f;
    
    bool canSendCommands = false;
    
    const float attackDuration = 0.75f;
    float attackingTimer = 0f;
    
    
    void SetupRemoteAgent()
    {
        GameObject remoteAgentObj = Instantiate(remoteAgentPrefab, Vector3.one * 2000, Quaternion.identity);
        
        DontDestroyOnLoad(remoteAgentObj);
        remoteAgent = remoteAgentObj.GetComponent<NavMeshAgent>();
        remoteAgent.updateRotation = false;
    }
    
    void Awake()
    {
        if(groundMask == -1)
        {
            groundMask = LayerMask.GetMask("Ground");
        }
        SetupAnimator();
        capsuleCol = GetComponent<CapsuleCollider>();
        ragdollOwner = GetComponent<RagdollOwner>();
        
        thisTransform = transform;
        net_comp = GetComponent<NetworkObject>();
        
        audioSource = GetComponent<AudioSource>();
        damage_fx = GetComponent<OnDamageTakenEffects>();
        SetupRemoteAgent();
        
        
        navMeshPath = new NavMeshPath();
        
        
        
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
        
        InGameConsole.LogOrange("RemoteAgentOnSpawn()");
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
        SetState(ChaserState.Idle);
           
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
        pathTimer = 0f;
        attackingTimer = 0f;
    }
    
    float airborneWeightSpeed = 2f;
    
    void SetState(ChaserState _state)
    {
        switch(_state)
        {
            case(ChaserState.Idle):
            {
                anim.SetLayerWeight(1, 0);
                target = null;
                target_pc = null;
                break;
            }
            case(ChaserState.Airborne):
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
                SetState(ChaserState.Airborne);
                
                DisableRemoteAgent();
                
                break;
            }
            case(NetworkCommand.DieWithForce):
            {
                // UnlockSendingCommands();
                
                Vector3 force = (Vector3)args[0];
                
                Die(force);
                
                break;
            }
            case(NetworkCommand.SetState):
            {
                UnlockSendingCommands();
                
                ChaserState _state = (ChaserState)(int)args[0];
                
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
                    SetState(ChaserState.Chasing);
                }
                
                
                break;
            }
            case(NetworkCommand.TakeDamage):
            {
                int incomingDamage = (int)args[0];
             
                TakeDamage(incomingDamage);
                
                break;
            }
            case(NetworkCommand.Attack):
            {
                Vector3 _attackPos = (Vector3)args[0];
                Vector3 _attackDir = (Vector3)args[1];
                
                UnlockSendingCommands();
                DoAttack(_attackPos, _attackDir);
                SetState(ChaserState.Attacking);
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
            // if(PhotonNetwork.IsMasterClient && state != ChaserState.Dead)
            // {
            //     CallDie(Vector3.zero);
            // }
            HitPoints = 0;
        }
    }
    
    // void CallDie(Vector3 force)
    // {
    //     NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.Die, force);
    // }
    
    public void TakeDamageLocally(int dmg, Vector3 damagePoint, Vector3 bulletDir)
    {
        // HitPoints -= dmg;
        if(damage_fx)
        {
            damage_fx.OnTakeDamage(damagePoint, bulletDir);
        }
    }
    
    public bool IsDead()
    {
        return true;
    }
    
    void Start()
    {
        prevPos = transform.position;
    }
    
    bool wasLocalPlayerHurt = false;
    
    void WarpAgentSafe(Vector3 pos)
    {
        NavMeshHit navMeshHit;
        if(NavMesh.SamplePosition(pos, out navMeshHit, 4, NavMesh.AllAreas))
        {
            remoteAgent.Warp(navMeshHit.position);    
        }
    }
    
    void DoAttack(Vector3 pos, Vector3 dir)
    {
        SetDestinationWithoutDirection(pos);
        SetDirection(dir);
        
        if(PhotonNetwork.IsMasterClient)
        {
            //InGameConsole.LogFancy("WarpAgentSafe from <color=yellow>DoAttack()</color>");
            WarpAgentSafe(pos);
            remoteAgent.SetDestination(pos);
            //if(remoteAgent.hasPath)
            //     remoteAgent.ResetPath();
        }
        
        attackingTimer = 0;
        //We hurt only local player:
        wasLocalPlayerHurt = false;
        
        
        
        DoAttack_anim();
    }
    
    public Transform HurtPoint;
    
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
        float sqrDist = Math.SqrDistance(localPlayerPos, HurtPoint.position);
        if(sqrDist < HurtPoint.localScale.x)
        {
            wasLocalPlayerHurt = true;
            // PhotonManager.Singleton().local_controller.TakeDamage(attackDamage);
//            PhotonManager.Singleton().local_controller.TakeDamageMelee(attackDamage);
            Vector3 boostVel = thisTransform.forward * 18f;
            
            PhotonManager.Singleton().local_controller.BoostVelocityAdditive(boostVel);
        }       
    }
    
    float attackDistance = 2.55f;
    public Transform attackDistanceSensor;
    
    void UpdateBrain()
    {
        float dt = UberManager.DeltaTime();
        
        switch(state)
        {
            case(ChaserState.Idle):
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
            case(ChaserState.Chasing):
            {
                if(canSendCommands && target)
                {
                    //Debug.DrawLine(attackDistanceSensor.position, target_pc.GetHeadPosition(), Color.yellow);
                    if((Math.SqrDistance(attackDistanceSensor.position, target_pc.GetHeadPosition()) < attackDistance * attackDistance))
                    {
                        LockSendingCommands();
                       
                        Vector3 attackPos = thisTransform.position;
                        Vector3 dirXZ = (target.position - thisTransform.position);
                        dirXZ.y = 0;
                        dirXZ = Math.Normalized(dirXZ);
                        NetworkObjectsManager.PackNetworkCommand(net_comp.networkId, NetworkCommand.Attack, attackPos, dirXZ);
                    }
                }
                
                
                
                if(target)
                {
                    if(canSendMove)
                    {
                        if(pathTimer > updatePathCD)
                        {
                            if(Math.SqrDistance(target.position, thisTransform.position) > 2)
                            {
                                // Vector3 dest = GetDestination(target_pc.GetGroundPosition());
                                // if(Math.SqrDistance(remoteAgent.destination, target.position) > 1)
                                // {
                                    Vector3 dest = GetDestination(target_pc.GetGroundPositionPredicted());
                                    LockMoveCommand();
                                    
                                    //NetworkObjectsManager.ScheduleCommand(net_comp.networkId, UberManager.GetPhotonTimeDelayedBy(0.05f), NetworkCommand.Move, dest);
                                    NetworkObjectsManager.PackNetworkCommand(net_comp.networkId, NetworkCommand.Move, dest);
                                // }
                            }
                            else
                            {
                                pathTimer = 0f;
                            }
                        }
                        else
                        {
                            UpdateRemoteAgent(target_pc.GetGroundPositionPredicted());
                            pathTimer += dt;
                        }
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
            case(ChaserState.Attacking):
            {
                if(attackingTimer > attackDuration)
                {
                    if(canSendCommands)
                    {
                        if(target)
                        {
                            if((Math.SqrDistance(attackDistanceSensor.position, target_pc.GetHeadPosition()) < attackDistance * attackDistance))
                            {
                                LockSendingCommands();
                                Vector3 attackPos = thisTransform.position;
                                Vector3 dirXZ = (target.position - thisTransform.position);
                                dirXZ.y = 0;
                                dirXZ = Math.Normalized(dirXZ);
                                NetworkObjectsManager.PackNetworkCommand(net_comp.networkId, NetworkCommand.Attack, attackPos, dirXZ);
                                
                            }
                            else
                            {
                            
                                LockSendingCommands();
                                NetworkObjectsManager.PackNetworkCommand(net_comp.networkId, NetworkCommand.SetState, (int)ChaserState.Chasing);
                            }
                        }
                        else
                        {
                            
                            LockSendingCommands();
                            NetworkObjectsManager.PackNetworkCommand(net_comp.networkId, NetworkCommand.SetState, (int)ChaserState.Chasing);
                        }
                    }
                }
                else
                {
                    attackingTimer += dt;
                }
                break;
            }
            case(ChaserState.Airborne):
            {
                    
                break;
            }
        }
    }
    
    public Vector3 airborneVelocity;
    
    void Die(Vector3 force)
    {
        if(state != ChaserState.Dead)
        {
            capsuleCol.enabled = false;
            HitPoints = 0;
            
            
            DisableRemoteAgent();
            SetState(ChaserState.Dead);
            
               
            OnDie(force);
            ClearTarget();
        }
    }
    
    public RagdollOwner ragdollOwner;
    LimbForExplosions limbForExplosions;
    
    void OnDie(Vector3 force)
    {
        //ragdollOwner.ApplyForceToHead(Vector3.up * 450 + Vector3.zero * 10);
        if(limbForExplosions == null)
        {
            limbForExplosions = GetComponent<LimbForExplosions>();
        }
        limbForExplosions.OnExplodeAffected();    
        MakeGibs(thisTransform.position + new Vector3(0, 2.25f, 0), force);
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
    float moveSpeed = 6f;
    
    Vector3 prevPos;
    
    void UpdateBrainLocally()
    {
        float dt = UberManager.DeltaTime();
        switch(state)
        {
            case(ChaserState.Idle):
            {
                MoveTransform();
                RotateToDesiredDirection();
                
                break;
            }
            case(ChaserState.Chasing):
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
            case(ChaserState.Attacking):
            {
                MoveTransformNoAnimation();
                TryDoDamageToLocalPlayer();
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
            case(ChaserState.Airborne):
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
    
    int DoAttackTrigger;
    int MoveSpeedAnimHash;
    
    void SetupAnimator()
    {
        anim = GetComponent<Animator>();
        if(anim == null)
        {
            anim = GetComponentInChildren<Animator>();
        }
       
        DoAttackTrigger = Animator.StringToHash("DoAttack");
        
        MoveSpeedAnimHash = Animator.StringToHash("MoveSpeed");
    }
    
  
   
    
    void DoAttack_anim()
    {
        anim.SetTrigger(DoAttackTrigger);
        
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
            
            // Debug.DrawRay(p1, Vector3.right, Color.red);
            // Debug.DrawRay(p2, Vector3.right, Color.green);
            
            
            
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
                            NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.DieWithForce, hit.normal * 10);
                        }
                        else
                        {
                            //remoteAgent.Warp(thisTransform.position);
                            WarpAgentSafe(thisTransform.position);
                            EnableRemoteAgent();
                            NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.SetState, (int)ChaserState.Idle);
                        }
                    }
                    airborneVelocity = new Vector3(0, 0, 0);
                    hasDestination = false;
                    // InGameConsole.LogOrange("Collision!");
                    SetState(ChaserState.Idle);
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
                             NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.DieWithForce, hit.normal * 10);
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
    
    bool CapsuleCast(Vector3 vel, out Vector3 calculatedVel)
    {
        float dt = UberManager.DeltaTime();
        bool Result = true;
        
        RaycastHit hit;
            
        Vector3 p1 = thisTransform.position;
        p1.y += capsuleCol.center.y;
        p1.y += capsuleCol.height / 2;
        p1.y -= capsuleCol.radius;
        
        
        
        Vector3 p2 = thisTransform.position;
        p2.y += capsuleCol.center.y;
        p2.y -= capsuleCol.height / 2;
        p2.y += capsuleCol.radius;
        
        
        calculatedVel = vel;
        
        int maxIterations = 4;
        
        for(int i = 0; i < maxIterations && !Result; i++)
        {
            Vector3 airborneDir = Math.Normalized(calculatedVel);
            float airborneMagnitude = Math.Magnitude(calculatedVel);
            Result = Physics.CapsuleCast(p1, p2, capsuleCol.radius, airborneDir, out hit, airborneMagnitude * dt, groundMask);
            
            if(i > 0)
            {
                InGameConsole.LogFancy("Additional casting: " + calculatedVel);
            }
            
            if(Result)
            {
                float dot = Vector3.Dot(hit.normal, airborneDir);
                    
                calculatedVel = airborneVelocity + hit.normal * airborneMagnitude * Math.Abs(dot);
            }
        }
        
        
        return Result;
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
                Gizmos.DrawSphere(destination, 0.4f);
            }
            else
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(thisTransform.position, destination);
                Gizmos.DrawSphere(destination, 0.4f);
            }
            
            
            
            switch(state)
            {
                case(ChaserState.Chasing):
                {
                    //Gizmos.color = Color.red;
                    //Gizmos.DrawWireSphere(attackDistanceSensor.position, attackDistance);
                    break;
                }
            }
            
            string txt = string.Format("{0}\nHP: {1}", state, HitPoints);
            
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
    
    
}