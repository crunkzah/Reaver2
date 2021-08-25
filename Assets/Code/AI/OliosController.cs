using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public enum OliosState : byte
{
    Idle,
    Fighting,
    Striking_direct,
    Striking_horizontal,
    Striking_vertical,
    Dead,
}

public class OliosController : MonoBehaviour, IDamagableLocal, INetworkObject
{
    public OliosState state;
    
    Animator anim;
    NetworkObject net_comp;
    Rigidbody[] joint_rbs;
    public AudioSource audio_src;
    public Transform parentTransform;
    Transform thisTransform;
    CapsuleCollider col;
    public SpawnedObject spawnedObjectComp;
    
    static int groundMask = -1;
    static int directMask = -1;
    static int normalBulletsMask = -1;
    
    public TrailRendererController[] trail_controllers;
    
    public ParticleSystem death_ps;
    
    public GameObject sun_symbol;
    
    
    void Awake()
    {
        InitJoints();
        instance_id = this.GetInstanceID();
        net_comp = GetComponent<NetworkObject>();
        anim = GetComponent<Animator>();
        col = GetComponent<CapsuleCollider>();
        if(spawnedObjectComp == null)
            spawnedObjectComp = GetComponent<SpawnedObject>();
        if(parentTransform == null)
            parentTransform = transform;
        limbs = GetComponentsInChildren<DamagableLimb>();
        
        thisTransform = transform;
        
        if(directMask == -1)
        {
            directMask = LayerMask.GetMask("Ground", "Ceiling");
            normalBulletsMask  = LayerMask.GetMask("Ground", "Ceiling", "Player");
            groundMask = LayerMask.GetMask("Ground", "Ceiling");
        }
    }
    
    bool canSendCommands = true;
    
    void UnlockSendingCommands()
    {
        canSendCommands = true;
    }
    
    void LockSendingCommands()
    {
        canSendCommands = false;
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
                        SetState(OliosState.Fighting);
                    }
                }
                
                break;
            }
            case(NetworkCommand.Move):
            {
                UnlockSendingCommands();
                Vector3 pos = (Vector3)args[0];
                
                for(int i = 0; i < trail_controllers.Length; i++)
                    trail_controllers[i].EmitFor(0.33F);
                    
                currentFlyDestination = pos;
//                SetMovePos(pos);                
                break;
            }
            case(NetworkCommand.SetState):
            {
                UnlockSendingCommands();
                byte _stateByte = (byte)args[0];
                
                OliosState _state = (OliosState)_stateByte;
                
                switch(_state)
                {
                    case(OliosState.Striking_direct):
                    {
                        if(args.Length > 1)
                        {
                            Vector3 strike_pos = (Vector3)args[1];
                            Vector3 strike_dir = (Vector3)args[2];
                            
                            current_strike_pos = strike_pos;
                            current_strike_dir = strike_dir;
                        }
                        break;
                    }
                    case(OliosState.Striking_horizontal):
                    {
                        if(args.Length > 1)
                        {
                            Vector3 strike_pos = (Vector3)args[1];
                            Vector3 strike_dir = (Vector3)args[2];
                            Vector3 strike_upDir = (Vector3)args[3];
                            
                            current_strike_pos = strike_pos;
                            current_strike_dir = strike_dir;
                            current_strike_upDir = strike_upDir;
                        }
                        break;
                    }
                    case(OliosState.Striking_vertical):
                    {
                        if(args.Length > 1)
                        {
                            Vector3 strike_pos = (Vector3)args[1];
                            Vector3 strike_dir = (Vector3)args[2];
                            
                            current_strike_pos = strike_pos;
                            current_strike_dir = strike_dir;
                        }
                        break;
                    }
                    default:
                    {
                        break;
                    }
                }
                
                SetState((OliosState)_stateByte);
                
                break;
            }
            case(NetworkCommand.Attack):
            {
                UnlockSendingCommands();
                
                
                Strike_direct(current_strike_pos, current_strike_dir);
                SetState(OliosState.Fighting);
                
                
                break;
            }
            case(NetworkCommand.Ability1):
            {
                UnlockSendingCommands();
                
                Strike_Hor(current_strike_pos, current_strike_dir, current_strike_upDir);
                SetState(OliosState.Fighting);
                
                break;
            }
            case(NetworkCommand.Ability2):
            {
                UnlockSendingCommands();
                
                Strike_Vert(current_strike_pos, current_strike_dir);
                SetState(OliosState.Fighting);
                
                break;
            }
            case(NetworkCommand.DieWithForce):
            {
                
                // Vector3 force = (Vector3)args[0];
                
                // byte limb_id = 0;
                // if(args.Length > 1)
                // {
                //     limb_id = (byte)args[1];
                // }
                
                
                Die();
                
                break;
            }
            case(NetworkCommand.TakeDamage):
            {
                int incomingDamage = (int)args[0];
                TakeDamage(incomingDamage);
                
                //Die(Vector3.zero);
                
                break;
            }
            default:
            {
                break;
            }
        }
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
    
    PlayerController target_pc;
    
    void SetTarget(PlayerController target)
    {
        target_pc = target;
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
    
    void Start()
    {
        DisableSkeleton();
        currentFlyDestination = parentTransform.localPosition;
        HitPoints = MaxHealth;
    }
    
    const int MaxHealth = 2500;
    int HitPoints = MaxHealth;
    
    float brainTimer = 0;
    float moveTimer = 0;
    
    const float moveCooldown = 5F;
    const float dashRange = 7F;
    
    int strikeCount = 0;
    const float strikeCooldown_minor = 1.7F;
    const float strikeCooldown_major = 2.5F;
    
    void UpdateBrain(float dt)
    {
        switch(state)
        {
            case(OliosState.Idle):
            {
                if(canSendCommands)
                {
                    Transform potentialTarget = ChooseTargetClosest(parentTransform.localPosition);
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
            case(OliosState.Fighting):
            {
                brainTimer += dt;
                moveTimer += dt;
                
                if(target_pc)
                {
                    if(moveTimer > moveCooldown)
                    {
                        if(canSendCommands)
                        {
                            moveTimer = 0;
                            
                            const int max_tries = 8;
                            
                            Vector3 dashDir;
                            RaycastHit hit;    
                            
                            Vector3 p1 = NPCTool.GetCapsuleBottomPoint(parentTransform, col);
                            gizmo1 = p1;
                            Vector3 p2 = NPCTool.GetCapsuleTopPoint(parentTransform, col);
                            gizmo2 = p2;
                            float capsule_radius = col.radius;
                            gizmo_radius = capsule_radius;
                            
                            
                            for(int tries = 0; tries < max_tries; tries++)
                            {
                                dashDir = Random.onUnitSphere;
                                if(Physics.CapsuleCast(p1, p2, capsule_radius, dashDir, out hit, dashRange, groundMask))
                                {
                                }
                                else
                                {
                                    Vector3 newPos = parentTransform.localPosition + dashDir * dashRange;
                                    LockSendingCommands();
                                    NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.Move, newPos);
                                    tries = int.MaxValue;
                                    break;
                                }
                            }
                        }
                    }
                    
                    if(brainTimer > strikeCooldown_major)
                    {
                        if(canSendCommands)
                        {
                            OliosState _state = OliosState.Striking_direct;
                            //Vector3 _strike_pos = parentTransform.localPosition;
                            Vector3 _strike_pos = currentFlyDestination;
                            _strike_pos.y += 1.5F;
                            Vector3 _strike_dir = (target_pc.GetGroundPosition() + new Vector3(0, 0.25f, 0) - _strike_pos).normalized;
                            Vector3 _strike_upDir = parentTransform.up;
                            
                            switch(strikeCount)
                            {
                                case 0:
                                {
                                    _state = OliosState.Striking_direct;
                                    
                                    strikeCount++;
                                    break;
                                }
                                case 1:
                                {
                                    _state = OliosState.Striking_horizontal;
                                    strikeCount++;
                                    _strike_dir = (target_pc.GetGroundPosition() + new Vector3(0, 0.25f, 0) - _strike_pos).normalized;
                                    break;
                                }
                                case 2:
                                {
                                    _state = OliosState.Striking_direct;
                                    
                                    strikeCount++;
                                    break;
                                }
                                case 3:
                                {
                                    _state = OliosState.Striking_vertical;
                                    strikeCount++;
                                    
                                    _strike_dir = (target_pc.GetGroundPosition() + new Vector3(0, 0.25f, 0) - _strike_pos).normalized;
                                    break;
                                }
                                case 4:
                                {
                                    _state = OliosState.Striking_direct;
                                    
                                    strikeCount++;
                                    break;
                                }
                                case 5:
                                {
                                    _state = OliosState.Striking_horizontal;
                                    strikeCount++;
                                    _strike_dir = (target_pc.GetGroundPosition() + new Vector3(0, 0.25f, 0) - _strike_pos).normalized;
                                    break;
                                }
                                case 6:
                                {
                                    _state = OliosState.Striking_vertical;
                                    strikeCount = 0;
                                    
                                    _strike_dir = (target_pc.GetGroundPosition() + new Vector3(0, 0.25f, 0) - _strike_pos).normalized;
                                    break;
                                }
                            }
                            
                            
                            brainTimer = 0;
                            LockSendingCommands();
                            if(_state == OliosState.Striking_horizontal)
                            {
                                NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.SetState, _state, _strike_pos, _strike_dir, _strike_upDir);
                            }
                            else
                            {
                                NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.SetState, _state, _strike_pos, _strike_dir);
                            }
                        }
                    }
                }
                else
                {
                    if(canSendCommands)
                    {
                        Transform potentialTarget = ChooseTargetClosest(parentTransform.localPosition);
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
            case(OliosState.Striking_direct):
            {
                brainTimer += dt;
                if(brainTimer > strikeDirect_attackTime)
                {
                    if(canSendCommands)
                    {
                        LockSendingCommands();
                        NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.Attack);
                    }
                }
                break;
            }
            case(OliosState.Striking_horizontal):
            {
                brainTimer += dt;
                if(brainTimer > strikeHor_attackTime)
                {
                    if(canSendCommands)
                    {
                        LockSendingCommands();
                        NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.Ability1);
                    }
                }
                break;
            }
            case(OliosState.Striking_vertical):
            {
                brainTimer += dt;
                if(brainTimer > strikeVert_attackTime)
                {
                    if(canSendCommands)
                    {
                        LockSendingCommands();
                        NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.Ability2);
                    }
                }
                break;
            }
            case(OliosState.Dead):
            {
                break;
            }
        }        
    }
    
    float damage_taken_timeStamp;
    
    public void TakeDamageLocally(int dmg, Vector3 hitPosition, Vector3 hitDirection)
    {
        if(state != OliosState.Dead && damage_taken_timeStamp + 0.15F < Time.time)
        {
            float vol = Random.Range(0.8f, 1f);
            //audio_src.PlayOneShot(clipHurt1, vol);
            
            damage_taken_timeStamp = Time.time;
        }
    }
    
    void TakeDamage(int dmg)
    {
        HitPoints -= dmg;
        InGameConsole.LogFancy("Olios hp is " + HitPoints.ToString());
    }
     
    public int GetCurrentHP()
    {
        return HitPoints;
    }
    
    public bool IsDead()
    {
        if(state == OliosState.Dead)
            return true;
            
        return false;
    }
    
    float flyDashSpeed   = 50F;
    float rotateTime     = 0.33F;
    float rotateDegSpeed = 360F;
    Quaternion deriv;
    
    const float strikeDirect_attackTime = 1.3F / 2.5F;
    const float strikeHor_attackTime    = 0.9F / 1.5F;
    const float strikeVert_attackTime   = 1.3F / 2.5F;
    
    public Vector3 currentFlyDestination;
    
    Vector3 gizmo1;
    Vector3 gizmo2;
    float gizmo_radius;
    
#if UNITY_EDITOR

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(gizmo1, gizmo_radius);
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(gizmo2, gizmo_radius);
    }
#endif
    
    void FlyTowardsDestination(float dt)
    {
        Vector3 currentPos = parentTransform.localPosition;
        Vector3 updatedPos = Vector3.MoveTowards(currentPos, currentFlyDestination, dt * flyDashSpeed);
        parentTransform.localPosition = updatedPos;    
    }
    
    int instance_id;
    
    public void QTS()
    {
        if(UberManager.CanMakeQTS(instance_id))
        {
            UberManager.MakeQTS(instance_id, thisTransform, qts_transform.position, QuickTimeType.Default, 1.5f, 900, 0.33f);
        }
    }
    
    void UpdateBrainLocally(float dt)
    {
        switch(state)
        {
            case(OliosState.Idle):
            {
                FlyTowardsDestination(dt);
                break;
            }
            case(OliosState.Fighting):
            {
                FlyTowardsDestination(dt);
                
                if(target_pc)
                {
                    Vector3 dirToTarget = (target_pc.GetHeadPosition() - parentTransform.localPosition).normalized;
                    
                    Quaternion desiredRot = Quaternion.LookRotation(dirToTarget);
                    //parentTransform.localRotation = QuaternionUtil.SmoothDamp(parentTransform.localRotation, desiredRot, ref deriv, rotateTime);
                    parentTransform.localRotation = Quaternion.RotateTowards(parentTransform.localRotation, desiredRot, rotateDegSpeed * dt);
                }
               
                break;
            }
            case(OliosState.Striking_direct):
            {
                FlyTowardsDestination(dt);
                
                Quaternion desiredRot = Quaternion.LookRotation(current_strike_dir);
                parentTransform.localRotation = Quaternion.RotateTowards(parentTransform.localRotation, desiredRot, rotateDegSpeed * 2 * dt);
                
                break;
            }
            case(OliosState.Striking_horizontal):
            {
                FlyTowardsDestination(dt);
                
                Quaternion desiredRot = Quaternion.LookRotation(current_strike_dir);
                parentTransform.localRotation = Quaternion.RotateTowards(parentTransform.localRotation, desiredRot, rotateDegSpeed * 2 * dt);
                
                break;
            }
            case(OliosState.Striking_vertical):
            {
                FlyTowardsDestination(dt);
                
                Quaternion desiredRot = Quaternion.LookRotation(current_strike_dir);
                parentTransform.localRotation = Quaternion.RotateTowards(parentTransform.localRotation, desiredRot, rotateDegSpeed * 2 * dt);
                
                break;
            }
            case(OliosState.Dead):
            {
                break;
            }
        }        
    }
    
    const float projectile_direct_speed = 64F;
    const float projectile_normal_speed = 50F;
    
    Vector3 current_strike_pos;
    Vector3 current_strike_dir;
    Vector3 current_strike_upDir;
    //int strike_count;
    
    void DoLight(Vector3 pos)
    {
        GameObject g = ObjectPool2.s().Get(ObjectPoolKey.LightPooled, false);
        LightPooled light = g.GetComponent<LightPooled>();
        Color color = new Color(1f, 0.71f, 0f, 1f);
        
        float decay_speed = 4 / 0.5f * 2.6f;
        light.DoLight(pos, color, 0.5f, 14, 4, decay_speed);
    }
    
    public Transform qts_transform;
    
    void Strike_direct(Vector3 pos, Vector3 dir)
    {
        GameObject projectile = ObjectPool2.s().Get(ObjectPoolKey.Olios_direct_projectile);
        BulletController bullet = projectile.GetComponent<BulletController>();
        
        DoLight(pos);
        
        ParticlesManager.PlayPooled(ParticleType.shotStar_ps, pos + dir * 0.9f, dir);
        
        bool isMine = PhotonNetwork.IsMasterClient;
        
        bullet.LaunchAsSphere(pos, dir, 0.2f, normalBulletsMask, projectile_direct_speed, 40, isMine);
        bullet.explosionCanDamageLocalPlayer = true;
        bullet.explosionPlayerDamage = 25;
        bullet.explosionDamage = 400;
        bullet.explosionCanDamageNPCs = false;
        bullet.explosionForce = 24;
        bullet.explosionRadius = 6;
    }
    
    void Strike_Hor(Vector3 pos, Vector3 dir, Vector3 upDir)
    {
        //Vector3 ortho = Vector3.Cross(dir, Vector3.right).normalized;
        DoLight(pos);
        float angle = 120F;
        int bullets_count = 24;
        float angleStep = angle / bullets_count;
        angle = -angle / 2;
        bool isMine = PhotonNetwork.IsMasterClient;
        
        ParticlesManager.PlayPooled(ParticleType.shotStar_ps, pos + dir * 0.9f, dir);
        
        for(int i = 0; i < bullets_count; i++)
        {
            Vector3 rotated_dir = Quaternion.AngleAxis(angle, upDir) * dir;
            
            GameObject projectile = ObjectPool.s().Get(ObjectPoolKey.Bullet_npc2);
            BulletController bullet = projectile.GetComponent<BulletController>();
            bullet.LaunchAsSphere(pos, rotated_dir, 0.3F, normalBulletsMask, projectile_normal_speed, 20, isMine);
            
            angle += angleStep;
        }
    }
    
    void Strike_Vert(Vector3 pos, Vector3 dir)
    {
        Vector3 ortho = Vector3.Cross(dir, Vector3.up).normalized;
        DoLight(pos);
        float angle = 120F;
        int bullets_count = 24;
        float angleStep = angle / bullets_count;
        angle = -angle / 2;
        bool isMine = PhotonNetwork.IsMasterClient;
        
        ParticlesManager.PlayPooled(ParticleType.shotStar_ps, pos + dir * 0.9f, dir);
        
        for(int i = 0; i < bullets_count; i++)
        {
            Vector3 rotated_dir = Quaternion.AngleAxis(angle, ortho) * dir;
            
            GameObject projectile = ObjectPool.s().Get(ObjectPoolKey.Bullet_npc2);
            BulletController bullet = projectile.GetComponent<BulletController>();
            bullet.LaunchAsSphere(pos, rotated_dir, 0.3F, normalBulletsMask, projectile_normal_speed, 20, isMine);
            
            angle += angleStep;
        }
    }
    
    public void SetState(OliosState _state)
    {
        if(state == OliosState.Dead)
        {
            return;
        }
        
        switch(_state)
        {
            case(OliosState.Striking_direct):
            {
                anim.Play("Base.StrikeDirect", 0, 0);
                for(int i = 0; i < trail_controllers.Length; i++)
                    trail_controllers[i].EmitFor(0.5F);
                break;
            }
            case(OliosState.Striking_horizontal):
            {
                anim.Play("Base.StrikeHor", 0, 0);
                for(int i = 0; i < trail_controllers.Length; i++)
                    trail_controllers[i].EmitFor(0.5F);
                //InGameConsole.LogFancy("StrikeHor");
                break;
            }
            case(OliosState.Striking_vertical):
            {
                anim.Play("Base.StrikeVert", 0, 0);
                for(int i = 0; i < trail_controllers.Length; i++)
                    trail_controllers[i].EmitFor(0.5F);
                break;
            }
            default:
            {
                break;
            }
        }
        
        state = _state;
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
    
    DamagableLimb[] limbs;
    byte current_limb_to_destroy = 4;
    
    public void D()
    {
        //InGameConsole.LogFancy("D(): " + current_limb_to_destroy.ToString());
        int len = limbs.Length; 
        for(int i = 0; i < len; i++)
        {
            if(limbs[i].limb_id == current_limb_to_destroy)
            {
                InGameConsole.LogFancy("D(): DestroyingLimb");
                limbs[i].TakeDamageLimb(1000 * 1000);
                break;
            }
        }
        anim.enabled = false;
        switch(current_limb_to_destroy)
        {
            case 4:
            {
                current_limb_to_destroy = 6;
                break;
            }                
            case 6:
            {
                current_limb_to_destroy = 9;
                break;
            }                
            case 9:
            {
                current_limb_to_destroy = 11;
                break;
            }
            case 11:
            {
                current_limb_to_destroy = 5;
                break;
            }                    
            case 5:
            {
                current_limb_to_destroy = 8;
                break;
            }        
            case 8:
            {
                current_limb_to_destroy = 10;
                break;
            }                
            case 10:
            {
                current_limb_to_destroy = 3;
                break;
            }    
            case 3:
            {
                current_limb_to_destroy = 2;
                break;                
            }
            case 2:
            {
                current_limb_to_destroy = 1;
                if(death_ps)
                {
                    death_ps.Play(true);
                }
                
                Destroy(parentTransform.gameObject, 4);
                break;                
            }
            
            default:
            {
                break;
            }
        }
        
    }
    
    
    public void Die()
    {
        if(state == OliosState.Dead)
        {
            return;
        }
        
        if(spawnedObjectComp)
            spawnedObjectComp.OnObjectDied();
        
        SetState(OliosState.Dead);
        
        
        anim.Play("Base.Die", 0, 0);
        InGameConsole.LogFancy("Olios Die()");
        
        sun_symbol.transform.SetParent(null);
        sun_symbol.GetComponent<Rigidbody>().isKinematic = false;
        sun_symbol.GetComponent<Rigidbody>().AddForce(Random.onUnitSphere * 6, ForceMode.Impulse);
        sun_symbol.GetComponent<MeshCollider>().enabled = true;
        Destroy(sun_symbol, 4.5F);
        
        
        Invoke("D", 1.2f);
        Invoke("D", 1.6f);
        Invoke("D", 2.2f);
        Invoke("D", 2.5f);
        Invoke("D", 2.7f);
        Invoke("D", 3.1f);
        Invoke("D", 3.3f);
        Invoke("D", 3.4f);
        Invoke("D", 3.5f);
        Invoke("D", 3.6f);
        
        
        //anim.enabled = false;
        col.enabled = false;
        NetworkObjectsManager.UnregisterNetObject(net_comp);
        
        AudioManager.Play3D(SoundType.death_impact_gib_distorted, parentTransform.localPosition);
        HitPoints = 0;
        
        StartCoroutine(DieShaking());
       
        DropHealthCrystals();
    }
    
    const float shakeMult = 0.05f;
    
    IEnumerator DieShaking()
    {
        for(;;)
        {
            float offsetY = shakeMult * Random.Range(-1f, 1f);
            float offsetX = shakeMult * Random.Range(-1f, 1f);
            float offsetZ = shakeMult * Random.Range(-1f, 1f);
            Vector3 currentPos = parentTransform.localPosition;
            currentPos.x += offsetX;
            currentPos.y += offsetY;
            currentPos.z += offsetZ;
            parentTransform.localPosition = currentPos;
            yield return null;
        }
    }
    
    
    
    void DropHealthCrystals()
    {
        //InGameConsole.LogOrange("OliosController: Dropping health crystals");
        HealthCrystal hc = ObjectPool.s().Get(ObjectPoolKey.HealthCrystal).GetComponent<HealthCrystal>();
        hc.Launch(parentTransform.localPosition + new Vector3(0, 1.25f, 0), 20);
        
        hc = ObjectPool.s().Get(ObjectPoolKey.HealthCrystal).GetComponent<HealthCrystal>();
        hc.Launch(parentTransform.localPosition + new Vector3(0, 1.25f, 0), 20);
        
        hc = ObjectPool.s().Get(ObjectPoolKey.HealthCrystal).GetComponent<HealthCrystal>();
        hc.Launch(parentTransform.localPosition + new Vector3(0, 1.25f, 0), 20);
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
    
   
    
}
