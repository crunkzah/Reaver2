using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

public enum WitchState : int
{
    Idle,
    Chasing,
    Attacking,
    Casting1,
    Dead
}

public class WitchController : MonoBehaviour, INetworkObject, IDamagableLocal, IPooledObject, IRemoteAgent, ISpawnable
{
    public GameObject remoteAgentPrefab;
    
    public WitchState state;
    
    Transform thisTransform;
    NetworkObject net_comp;
    CapsuleCollider capsuleCol;
    AudioSource audioSource;
    Animator anim;
    
    NavMeshAgent remoteAgent;
    NavMeshPath navMeshPath;
    Transform target;
    public PlayerController target_pc;
    
    OnDamageTakenEffects damage_fx;
    
    static int groundMask = -1;
    static int playerGroundMask = -1;
    
    //Unfuck animation scale:
    public Transform armature_root;
    public Transform smr_transform;
    
    public ParticleSystem handPs1;
    public ParticleSystem handPs2;
    
    
    void Awake()
    {
        capsuleCol = GetComponent<CapsuleCollider>();
        limbForExplosions = GetComponent<LimbForExplosions>();
        //ragdollOwner = GetComponent<RagdollOwner>();
        
        if(groundMask == -1)
        {
            groundMask = LayerMask.GetMask("Ground", "Ceiling");
            playerGroundMask = LayerMask.GetMask("Player", "Ground", "Ceiling");
        }
        
        thisTransform = transform;
        net_comp = GetComponent<NetworkObject>();
        audioSource = GetComponent<AudioSource>();
        damage_fx = GetComponent<OnDamageTakenEffects>();
        SetupRemoteAgent();
        SetupAnimator();
    }
    
    void SetupAnimator()
    {
        anim = GetComponent<Animator>();
        if(anim == null)
        {
            anim = GetComponentInChildren<Animator>();
        }
       
        // GoRunTrigger = Animator.StringToHash("GoRun");
        // LoadTrigger = Animator.StringToHash("Load");
        
        // MoveSpeedAnimHash = Animator.StringToHash("MoveSpeed");
    }
    
    bool canSendCommands = false;
    
    
    float moveSpeed = 4.5f;
    public Vector3 moveDestination;
    
    const float blinkCD = 7;
    float blinkTimer = 0;
    
    [SerializeField] float chasingTimer = 0;
    
    const float castingDuration = 1.5f;
    const float castingCooldown = 1.5f;
    [SerializeField] float castingTimer = 0;
    
    const float castingDistance = 36;
    
    const float updatePathCD = 0.25F;
    public float pathTimer = 0f;
    
    void ResetTimers()
    {
        blinkTimer = 0;
        castingTimer = 0;
        pathTimer = 0;
        chasingTimer = 0;
        castedSpell = false;
    }
    
    void LockSendingCommands()
    {
        canSendCommands = false;
    }
    
    void UnlockSendingCommands()
    {
        canSendCommands = true;
    }
    
    void SetupRemoteAgent()
    {
        GameObject remoteAgentObj = Instantiate(remoteAgentPrefab, Vector3.one * 2000, Quaternion.identity);
        remoteAgentObj.SetActive(false);
        
        DontDestroyOnLoad(remoteAgentObj);
        remoteAgent = remoteAgentObj.GetComponent<NavMeshAgent>();
        remoteAgent.updateRotation = false;
    }
    
    
    public void InitialState()
    {
        HitPoints = MaxHealth;
        capsuleCol.enabled = true;
        
        moveDestination = thisTransform.localPosition;
        DisableRemoteAgent(); 
       
        ResetTimers();
        
        if(gameObject.activeSelf)
            anim.Play("Base.Levitate", 0);
        
        SetState(WitchState.Idle);
        
        UnlockSendingCommands();
        
    }
    
    public void SetSpawnPosition(Vector3 pos)
    {
        //Debug.Log("SetSpawnPosition()");
        moveDestination = pos;
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
    
    public AudioClip blinkClip;
    
    void Blink(Vector3 blinkPos)
    {
        if(state == WitchState.Dead)
        {
            return;
        }
        
        // InGameConsole.LogFancy("Blink!");
        thisTransform.localPosition = blinkPos;
        moveDestination = blinkPos;
        
        if(target_pc != null)
        {
            Vector3 lookDir = (target_pc.GetGroundPosition() - thisTransform.localPosition);
            lookDir.y = 0;
            lookDir = Math.Normalized(lookDir);
            thisTransform.forward = lookDir;
        }
        
        if(PhotonNetwork.IsMasterClient)
        {
            WarpAgentSafe(blinkPos);
        }
        
        audioSource.PlayOneShot(blinkClip, 0.5f);
        ParticlesManager.PlayPooled(ParticleType.blink1_ps, blinkPos, Vector3.forward);
    }
    
    public void ReceiveCommand(NetworkCommand command, params object[] args)
    {
        switch(command)
        {
            case(NetworkCommand.Ability1):
            {
                UnlockSendingCommands();
                
                Vector3 castSpellPosition = (Vector3)args[0];
                Vector3 castSpellAimPosition = (Vector3)args[1];
                
                CastSpell(castSpellPosition, castSpellAimPosition);
                
                break;
            }
            case(NetworkCommand.SetState):
            {
                UnlockSendingCommands();
                WitchState _state = (WitchState)(int)args[0];
                
                SetState(_state);
                
                break;
            }
            case(NetworkCommand.Blink):
            {
                Vector3 blinkPos = (Vector3)args[0];
                Blink(blinkPos);
                
                break;
            }
            case(NetworkCommand.SetTarget):
            {
                UnlockSendingCommands();
                int viewId = (int)args[0];
                
                PhotonView pv = PhotonNetwork.GetPhotonView(viewId);
                if(pv)
                {
                    SetTarget(pv.transform);
                    SetState(WitchState.Chasing);
                }
                
                break;
            }
            case(NetworkCommand.Move):
            {
                Vector3 dest = (Vector3)args[0];
                
                moveDestination = dest;
                
                break;
            }
            case(NetworkCommand.DieWithForce):
            {
                // UnlockSendingCommands();
                
                Vector3 force = (Vector3)args[0];
                
                Die(force);
                
                break;
            }
            case(NetworkCommand.TakeDamage):
            {
                int incomingDamage = (int)args[0];
             
                TakeDamage(incomingDamage);
                
                break;
            }
            case(NetworkCommand.Shoot):
            {
                Vector3 shootPos = (Vector3)args[0];
                Vector3 shootVel = (Vector3)args[1];
                float _projSpeed = Math.Magnitude(shootVel);
                
                
                Shoot1(shootPos, Math.Normalized(shootVel), _projSpeed);
                
                break;
            }
            default:
            {
                break;
            }
        }
    }
    
     const float projectileRadius = 0.3F;
    const float projectileSpeed = 25f;
    bool castedSpell = false;
    
    Vector3 spellCastPos;
    Vector3 spellCastDir;
    
    Quaternion gfxDeriv;
    Quaternion gfxTargetRotation;
    float gfxRotationTime = 0.4f;
    
    void RotateGFX_tr()
    {
        GFX_tr.rotation = QuaternionUtil.SmoothDamp(GFX_tr.rotation, gfxTargetRotation, ref gfxDeriv, gfxRotationTime);
    }
    
    void CastSpell(Vector3 witchPos, Vector3 targetPos)
    {
        SetState(WitchState.Casting1);
        moveDestination = witchPos;
        WarpAgentSafe(moveDestination);
        anim.Play("Base.Spell1");
        
        Vector3 dir = targetPos - shootSpot.position;
        dir = Math.Normalized(dir);
        
        spellCastPos = witchPos;
        spellCastDir = dir;
        
        handPs1.Play();
        handPs2.Play();
    }
    
    public Transform GFX_tr;
    
    public ParticleSystem shot_ps;
    
    void Shoot1(Vector3 pos, Vector3 dir, float _projSpeed)
    {
        shot_ps.Play();
        
        GameObject bullet = ObjectPool.s().Get(ObjectPoolKey.Bullet_npc2);
        bool isBulletMine = PhotonNetwork.IsMasterClient;
        bullet.GetComponent<BulletController>().LaunchAsSphere(pos, dir, projectileRadius, playerGroundMask, _projSpeed, 25, isBulletMine);
        AudioManager.Play3D(SoundType.projectile_launch1, pos, Random.Range(0.9f, 1.1f));
        
        
    }
    
    void WarpAgentSafe(Vector3 pos)
    {
        NavMeshHit navMeshHit;
        if(NavMesh.SamplePosition(pos, out navMeshHit, 4, NavMesh.AllAreas))
        {
            if(remoteAgent.hasPath)
            {
                remoteAgent.ResetPath();
                
            }
            remoteAgent.Warp(navMeshHit.position);    
        }
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
    
     void SetState(WitchState _state)
    {
        switch(_state)
        {
            case(WitchState.Idle):
            {
                WarpAgentSafe(thisTransform.localPosition);
                moveDestination = thisTransform.localPosition;
                ClearTarget();
                
                break;
            }
            case(WitchState.Chasing):
            {
                WarpAgentSafe(thisTransform.localPosition);
                break;
            }
        }
        
       
        state = _state;
    }
    
    public Transform shootSpot;
    public Transform sensor;
    
    bool CanCastSpellAt(PlayerController pc)
    {
        Vector3 aimPosition = pc.GetHeadPosition();
        float distanceToTarget = Vector3.Distance(aimPosition, shootSpot.position);
        
        if(distanceToTarget < castingDistance)
        {
            Vector3 dir = aimPosition - shootSpot.position;
            dir = Math.Normalized(dir);
            
            Ray ray = new Ray(shootSpot.position, dir);
            RaycastHit hit;
            
            //We hit ground thus we dont see player:
            if(Physics.Raycast(ray, out hit, distanceToTarget, groundMask))
            {
                return false;
            }
            //Else we see a player:
            return true;
        }
        
        
        return false;
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
    
    public bool canCast;
    
    void UpdateBrain(float dt)
    {
        switch(state)
        {
            case(WitchState.Idle):
            {
                if(canSendCommands && target_pc == null)
                {
                    Transform potentialTarget = ChooseTargetClosest(thisTransform.localPosition);
                    if(potentialTarget)
                    {
                        LockSendingCommands();
                        int photonId = potentialTarget.GetComponent<PhotonView>().ViewID;
                        
                        NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.SetTarget, photonId);
                        
                    }                    
                }
                
                break;
            }
            case(WitchState.Chasing):
            {
                //If we dont have a target:
                if(canSendCommands && target_pc == null)
                {
                    Transform potentialTarget = ChooseTargetClosest(thisTransform.localPosition);
                    if(potentialTarget)
                    {
                        LockSendingCommands();
                        int photonId = potentialTarget.GetComponent<PhotonView>().ViewID;
                        
                        NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.SetTarget, photonId);
                        
                    }
                    else
                    {
                        LockSendingCommands();
                        NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.SetState, (int)WitchState.Idle);
                    }
                    
                    break;
                }
                
                chasingTimer += dt;
                
                if(chasingTimer > castingCooldown && canSendCommands)
                {
                    canCast = CanCastSpellAt(target_pc);
                    if(CanCastSpellAt(target_pc))
                    {
                        float distanceToDestination = Vector3.Distance(moveDestination, thisTransform.localPosition);
                        Vector3 castSpellPosition = thisTransform.localPosition;
                        
                        if(distanceToDestination > moveSpeed * 0.2f)
                        {
                            castSpellPosition = thisTransform.localPosition + Math.Normalized(moveDestination - thisTransform.localPosition) * 0.2f;
                        }
                        
                        Vector3 castSpellAimPosition = target_pc.GetHeadPosition();
                        
                        LockSendingCommands();
                        NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.Ability1, castSpellPosition, castSpellAimPosition);
                        chasingTimer = 0;
                    }
                }
                
                //If we have a target:   
                if(target_pc)
                {
                    pathTimer += dt;
                    blinkTimer += dt;
                    
                    //If it is time to update path:
                    if(pathTimer > updatePathCD)
                    {
                        Vector3 destPos = remoteAgent.transform.position;
                        
                            
                        //Decide if we may blink:
                        if(blinkTimer > blinkCD && Math.SqrDistance(thisTransform.localPosition, target_pc.GetGroundPosition()) > 9 * 9)
                        {
                            Vector3 blinkPos;
                            if(!NPCManager.RandomPoint(target_pc.GetGroundPosition(), 8, out blinkPos))
                            {
                                blinkPos = thisTransform.localPosition;
                            }
                            
                            NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.Blink, blinkPos);
                                                        
                            blinkTimer = 0;
                            pathTimer = 0;
                        }
                        //if we cannot, just update path:
                        else
                        {
                            if(!CanCastSpellAt(target_pc))
                            {
                                NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.Move, destPos);
                            }
                        }
                        
                        pathTimer = 0;
                    }
                    else
                    {
                        remoteAgent.SetDestination(target_pc.GetGroundPosition());    
                    }
                }
                
                
                break;
            }
            case(WitchState.Casting1):
            {
                if(canSendCommands && !castedSpell && castingTimer > castingDuration * 0.5f)
                {
                    castedSpell = true;
                    
                    //Vector3 _castPos = spellCastPos + new Vector3(0, 2.1f, 0) + spellCastDir * 0.2f;
                    Vector3 _castPos = shootSpot.position;
                    
                    if(target_pc)
                    {
                        spellCastDir = Math.Normalized(target_pc.GetHeadPosition() - shootSpot.position);
                    }
                    Vector3 _projVel = spellCastDir * projectileSpeed * 1f;
                    double timeWhenCast = UberManager.GetPhotonTimeDelayedBy(0.25f);
                    int netId = net_comp.networkId;
                    
                    NetworkObjectsManager.ScheduleCommand(netId, timeWhenCast, NetworkCommand.Shoot, _castPos, _projVel);
                    NetworkObjectsManager.ScheduleCommand(netId, timeWhenCast + 0.150d, NetworkCommand.Shoot, _castPos, _projVel * 0.9f);
                    NetworkObjectsManager.ScheduleCommand(netId, timeWhenCast + 0.300d, NetworkCommand.Shoot, _castPos, _projVel * 0.8f);
                }
                
                if(castingTimer > castingDuration)
                {
                    if(canSendCommands)
                    {
                        LockSendingCommands();
                        
                        castedSpell = false;
                        castingTimer = 0;
                        NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.SetState, (int)WitchState.Chasing);
                    }
                }
                else
                {
                    castingTimer += dt;
                }
                
                break;
            }
        }
    }
    
    readonly static Vector3 vUp = new Vector3(0, 1, 0);
    readonly static Vector3 vOne = new Vector3(1, 1, 1);
    
    Quaternion deriv;
    
    void RotateToDirection(Vector3 targetDir, float timeToRotate)
    {
        targetDir.y = 0;
        targetDir = Math.Normalized(targetDir);
        
        Quaternion targetRotation = Quaternion.LookRotation(targetDir);
        thisTransform.localRotation = QuaternionUtil.SmoothDamp(thisTransform.localRotation, targetRotation, ref deriv, timeToRotate);
    }
    
    void RotateToLookAt(Vector3 lookAtPos, float timeToRotate)
    {
        float dt = UberManager.DeltaTime();
        
        Quaternion targetRotation;
        lookAtPos.y = thisTransform.localPosition.y;
        Vector3 targetDir = (lookAtPos - thisTransform.localPosition);
        
        if(Math.SqrMagnitude(targetDir) > 0.5 * 0.5f)
        {
            targetDir = Math.Normalized(targetDir);
            targetRotation = Quaternion.LookRotation(targetDir);
            
            thisTransform.localRotation = QuaternionUtil.SmoothDamp(thisTransform.localRotation, targetRotation, ref deriv, timeToRotate);
        }
        else
        {
            if(target_pc)
            {
                if(Math.SqrDistance(Math.GetXZ(target_pc.GetGroundPosition()), Math.GetXZ(thisTransform.localPosition)) > 1)
                {
                    targetDir = (target_pc.transform.position - thisTransform.localPosition);
                    targetDir.y = 0;
                    
                    targetRotation = Quaternion.LookRotation(targetDir);
                    thisTransform.localRotation = QuaternionUtil.SmoothDamp(thisTransform.localRotation, targetRotation, ref deriv, timeToRotate);
                }
            }
        }
    }
    
    
    static readonly Quaternion qIdentity = Quaternion.identity;
    
    void UpdateBrainLocally(float dt)
    {
        switch(state)
        {
            case(WitchState.Idle):
            {
                RotateGFX_tr();
                gfxTargetRotation = thisTransform.rotation;
                break;
            }
            case(WitchState.Chasing):
            {
                if(Math.SqrDistance(thisTransform.localPosition, moveDestination) > 0.01f * 0.01f)
                    thisTransform.localPosition = Vector3.MoveTowards(thisTransform.localPosition, moveDestination, moveSpeed * dt);
                RotateToLookAt(moveDestination, 0.2f);
                gfxTargetRotation = thisTransform.rotation;
                RotateGFX_tr();
                
                break;
            }
            case(WitchState.Casting1):
            {
                if(Math.SqrDistance(thisTransform.localPosition, moveDestination) > 0.01f * 0.01f)
                    thisTransform.localPosition = Vector3.MoveTowards(thisTransform.localPosition, moveDestination, moveSpeed * dt);
                RotateToDirection(spellCastDir, 0.2f);
                gfxTargetRotation = Quaternion.LookRotation(spellCastDir, Vector3.up);
                RotateGFX_tr();
                
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
    
    void LateUpdate()
    {
        armature_root.localScale = vOne;
        smr_transform.localScale = vOne;
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
        //if(state == PadlaState.Dead)
            return true;
        //else
        //    return false;
    }
    
    const int MaxHealth = 1000;
    public int HitPoints = 0;
    
    public int GetCurrentHP()
    {
        return HitPoints;
    }
    
    void Die(Vector3 force)
    {
        if(state != WitchState.Dead)
        {
            capsuleCol.enabled = false;
            HitPoints = 0;
            
            
            DisableRemoteAgent();
            SetState(WitchState.Dead);
            
               
            OnDie(force);
            ClearTarget();
        }
    }
    
    public RagdollOwner ragdollOwner;
    LimbForExplosions limbForExplosions;
    
    void OnDie(Vector3 force)
    {
        limbForExplosions.ExplodeEveryLimb();
        HealthCrystal hc = ObjectPool.s().Get(ObjectPoolKey.HealthCrystal).GetComponent<HealthCrystal>();
        hc.Launch(this.transform.localPosition + new Vector3(0, 1.25f, 0), 10);
        
        hc = ObjectPool.s().Get(ObjectPoolKey.HealthCrystal).GetComponent<HealthCrystal>();
        hc.Launch(this.transform.localPosition + new Vector3(0, 1.25f, 0), 10);
        
        hc = ObjectPool.s().Get(ObjectPoolKey.HealthCrystal).GetComponent<HealthCrystal>();
        hc.Launch(this.transform.localPosition + new Vector3(0, 1.25f, 0), 10);
        
        //ragdollOwner.ApplyForceToHead(Vector3.up * 450 + Vector3.zero * 10);
        AudioManager.Play3D(SoundType.witch_death1, thisTransform.localPosition, 1);
        //MakeGibs(thisTransform.localPosition + new Vector3(0, 2.25f, 0), force);
        anim.Play("Base.Dying");
        //this.gameObject.SetActive(false);
    }
    
    
#if UNITY_EDITOR
    GUIStyle style = new GUIStyle();
    void OnDrawGizmos()
    {
        if(Application.isPlaying && thisTransform != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(thisTransform.localPosition, moveDestination);
            Gizmos.DrawWireSphere(moveDestination, 0.75f);
        
            
            // string txt = string.Format("{0}\nHP: {1}", state, HitPoints);
            string txt = string.Format("{0}\nTargetInRange: {1}", state, HitPoints);
            
            style.alignment = TextAnchor.MiddleCenter;
            style.richText = true;
            style.fontStyle = FontStyle.Bold;
            style.normal.textColor = Colors.Orange;
            
            UnityEditor.Handles.Label(thisTransform.localPosition + Vector3.up * 2.5f, txt, style);
        }
    }
    
#endif
}
