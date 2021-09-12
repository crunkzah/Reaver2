using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

public enum PadlaLongState : byte
{
    Idle,
    Chasing,
    Stomping,
    Dead
}

public class PadlaLongController : MonoBehaviour, INetworkObject, IDamagableLocal, IKillableThing
{
    public GameObject remoteAgent_prefab;
    public GameObject stomp_prefab;
    public GameObject deferredStrike_prefab;
    
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
        
        path_update_cd = PhotonNetwork.OfflineMode ? PATH_UPDATE_BASE / 2 : PATH_UPDATE_BASE;
    }
    
    void InitAsMaster()
    {
        remoteAgent = Instantiate(remoteAgent_prefab, thisTransform.localPosition, thisTransform.localRotation).GetComponent<NavMeshAgent>();
        remoteAgentTransform = remoteAgent.transform;
    }
    
    Transform head;
    
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
        if(head == null)
        {
            return false;
        }
        
        if(state != PadlaLongState.Dead)
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
                
                Vector3 stompPos = (Vector3)args[0];
                Vector3 stompDir = (Vector3)args[1];
                
                brainTimer = 0;
                //attack_timer = 0;
                stomp_timer = 0;
                
                SetStompPos(stompPos, stompDir);
                
                SetMovePos(stompPos);
                WarpRemoteAgent(stompPos);
                UpdateRemoteAgentDestination(stompPos);
                Stomp(stompPos);
                SetState(PadlaLongState.Stomping);
                
                
                break;
            }
            case(NetworkCommand.Ability1):
            {
                UnlockSendingCommands();
                Clap();
                break;
            }
            case(NetworkCommand.Ability2):
            {
                //UnlockSendingCommands();
                Vector3 _clap_pos = (Vector3)args[0];
                NetworkClap(_clap_pos);
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
                TakeDamageForce(incomingDamage, force, limb_id);
                
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
            case(PadlaLongState.Stomping):
            {
                anim.SetFloat(MoveSpeedHash, 0);
                stomp_happened = false;
                stomp_local_timer = 0;
                
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
    
    void TakeDamage(int dmg, byte limb_id)
    {
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
        if(state == PadlaLongState.Dead)
        {
            return;
        }
        
        //InGameConsole.LogOrange("DieFromExplosion()");
        
        SetState(PadlaLongState.Dead);
        
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
        hc.Launch(this.transform.localPosition + new Vector3(0, 2.25f, 0), 15);
        hc.Launch(this.transform.localPosition + new Vector3(0, 2.25f, 0), 15);
        hc.Launch(this.transform.localPosition + new Vector3(0, 2.25f, 0), 15);
    }
    
    
    const int MaxHealth = 4800;
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
    
    const float PATH_UPDATE_BASE = 0.125F * 2;
    float path_update_cd;
    float brainTimer = 0;
    
    Vector3 stompLocalPos;
    Vector3 stompLocalDir;
    
    void SetStompPos(Vector3 _dashPos, Vector3 _stompLocalDir)
    {
        stompLocalPos = _dashPos;
        stompLocalDir = _stompLocalDir;
    }
    
    void SetMovePos(Vector3 pos)
    {
        currentDestination = pos;
    }
    
    Vector3 currentDestination;
    static readonly Vector3 vUp = new Vector3(0, 1, 0);
    Quaternion deriv;
    
    const float moveSpeed = 3.5F;
    const float rotateTime = 0.2F;
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
    
    const float stomp_distance  = 12F;
    const float stomp_cooldown  = 2.25F;
    const float stomp_duration  = 2F;
    const float stomp_radius    = 5F;
    const int   stomp_dmg       = 30;
          float stomp_timer     = 0F;
    float stomp_local_timer     = 0f;
    
    float clap_timer            = 0f;
    const float clap_cooldown   = 4f;
    
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
            case(PadlaLongState.Idle):
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
            case(PadlaLongState.Chasing):
            {
                brainTimer += dt;
                stomp_timer += dt;
                clap_timer += dt;
                
                if(IsTargetValid())
                {
                    Vector3 targetGroundPos = target_pc.GetGroundPosition();
                    
                    UpdateRemoteAgentDestination(targetGroundPos);
                    
                    Vector3 padlaLongPosition = thisTransform.localPosition;
                    
                    if(canSendCommands && UberManager.readyToSwitchLevel)
                    {
                        if(stomp_timer > stomp_cooldown &&  Math.SqrDistance(targetGroundPos, padlaLongPosition) < stomp_distance  * stomp_distance)
                        {
                            Vector3 stompPos = padlaLongPosition;
                            Vector3 stompDir = Math.GetXZ((targetGroundPos - padlaLongPosition).normalized).normalized;
                            
                            LockSendingCommands();
                            NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.Attack, stompPos, stompDir);
                        }
                        else if(clap_timer > clap_cooldown)
                        {
                            clap_timer = 0;
                            LockSendingCommands();
                            NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.Ability1);
                        }
                        else if(brainTimer > path_update_cd)
                        {
                            brainTimer = 0;
                            
                            Vector3 remoteAgentPos = GetRemoteAgentPos();
                            
                            if(Math.SqrDistance(padlaLongPosition, remoteAgentPos) > 0.175F * 0.175F)
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
                            NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.SetState, (byte)PadlaLongState.Idle);                        
                        }
                    }
                }
                
                break;
            }
            case(PadlaLongState.Stomping):
            {
                brainTimer += dt;
                
                if(canSendCommands && UberManager.readyToSwitchLevel)
                {
                    if(brainTimer > stomp_duration)
                    {
                        brainTimer = 0;
                        Transform potentialTarget = ChooseTargetClosest(thisTransform.localPosition);
                        UpdateRemoteAgentDestination(stompLocalPos);
                        WarpRemoteAgent(stompLocalPos);
                        
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
                            NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.SetState, (byte)PadlaLongState.Idle);
                        }
                        
                    }
                }
                break;
            }
            case(PadlaLongState.Dead):
            {
                break;
            }
        }
    }
    
    bool stomp_happened = false;
    
    void Stomp(Vector3 pos)
    {
        anim.Play("Base.Stomp", 0, 0);
        
        
        
        //audio_src.clip = clipKick;
        //audio_src.PlayDelayed(punch1_damageTimingStart);
    }
    
    public ParticleSystem clap_cooking_ps_l;
    public ParticleSystem clap_cooking_ps_r;
    public ParticleSystem clap_ps;
    
    public void NetworkClap(Vector3 clap_pos)
    {
        GameObject clap_deferred_strike = Instantiate(deferredStrike_prefab, clap_pos, Quaternion.identity);
        float delay = PhotonNetwork.OfflineMode ? 0.65f : 0.5f;
        clap_deferred_strike.GetComponent<DeferredStrike>().DoStartStrike(clap_pos, delay, 5f, 10, 11f);
    }
    
    
    
    public void OnClap()
    {
        if(PhotonNetwork.IsMasterClient)
        {
            //Vector3 clap_pos = thisTransform.localPosition + new Vector3(Random.Range(-1f, 1f), 0.0f, Random.Range(-1f, 1f)) * 5;
            
            InGameConsole.LogOrange("Players count is <color=yellow>" + UberManager.Singleton().playerControllers.Count.ToString() + "</color>");
            
            if(IsTargetValid())
            {
                Vector3 ground_pos = target_pc.GetGroundPosition();
                NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.Ability2, ground_pos);
                
            }
            
            // for(int i = 0; i < UberManager.Singleton().players_controller.Count; i++)
            // {
            //     PlayerController t = UberManager.Singleton().players_controller[i];
            //     if(t)
            //     {
            //         InGameConsole.LogFancy("GroundPos: " + ground_pos);
            //         // RaycastHit hit;
            //         // if(Physics.Raycast(ground_pos + new Vector3(0, 1, 0), new Vector3(0, -1, 0), out hit, 3f, groundMask))
            //         // {
            //         //     ground_pos = hit.point;
            //         // }
                    
            //     }
            // }
        }
        
        //audio_src.PlayOneShot(clipClap, 0.7f);
        clap_ps.Play();
        
        InGameConsole.LogOrange("<color=green>OnClap</color>");
    }
    
    public void OnClapEnd()
    {
        //InGameConsole.LogOrange("<color=red>OnClapEnd</color>");
        anim.SetLayerWeight(1, 0f);
    }
    
    void Clap()
    {
        anim.SetLayerWeight(1, 1f);
        anim.Play("Clap", 1, 0);
        
        clap_cooking_ps_l.Play();
        clap_cooking_ps_r.Play();
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
        
        UnityEditor.Handles.Label(transform.localPosition + new Vector3(0, 4f, 0), state.ToString(), style);
        
    }
#endif
    
    const float animDampTime = 0.05F;
    
    
    
    
    const float footStepDistance = 2.1f;
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
            case(PadlaLongState.Stomping):
            {
                Vector3 currentPos = thisTransform.localPosition;
                float dV = speedMult * dt * moveSpeed;
                stomp_local_timer += dt;
                
                //RotateToLookAt(stompLocalPos, rotateTime * speedMult / 4, false);
                Vector3 stompingDirXZ = Math.GetXZ(stompLocalDir);
                Quaternion desiredRotation = Quaternion.LookRotation(stompingDirXZ);
                
                thisTransform.localRotation = Quaternion.RotateTowards(thisTransform.localRotation, desiredRotation, dt * 720);
                
                thisTransform.localPosition = Vector3.MoveTowards(currentPos, currentDestination, dV);
                
                if(!stomp_happened && stomp_local_timer > 0.775F)
                {
                    stomp_happened = true;
                    GameObject stomp = Instantiate(stomp_prefab, stompLocalPos, Quaternion.identity);
                    stomp.GetComponent<GroundStomp>().MakeStomp(20, stomp_dmg, stomp_radius);
                }
                
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
    public AudioClip clipStep;
    public AudioClip clipHurt1;
    public AudioClip clipDeath;
    public AudioClip clipStomp;
    public AudioClip clipClap;
}
