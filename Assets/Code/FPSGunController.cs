using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;
using System.Collections;


public enum GunType : byte
{
    None,
    Revolver,
    Shotgun,
    RocketLauncher,
    AR,
    MP5_alt,
    Revolver_blue,
    Shotgun_blue,
}

public struct MeleeTimedDamage
{
    public int damage;
    public Vector3 pos;
    public float sweepRange;
    public Vector3 sweepDir;
    
}

public struct ShotgunHit
{
    public Collider col;
    public int damage;
}

public enum ArmType : byte 
{
    None,
    Arm1,
    Sunstrike
}

[System.Serializable]
public struct InventoryPosition
{
    public int mainIndex;
    public int altIndex;
    public InventoryPosition(int main, int alt)
    {
        mainIndex = main;
        altIndex = alt;
    }
    public bool Equals(InventoryPosition x)
    {
        if((this.mainIndex == x.mainIndex) && (this.altIndex == x.altIndex))
            return true;
        else 
            return false;
    }
    public override string ToString()
    {
        return string.Format("[<color=yellow>{0}</color>,<color=red>{1}</color>]", this.mainIndex, this.altIndex    );
    }
}

public class FPSGunController : MonoBehaviour
{
    public AudioSource gunAudio;
    public AudioSource gunAudio_pitched;
    
    const int revolverDmg = 300;
    const int revolverDmg_reflect = 150;
    const int revolverDmg_ult = 550;//530;
    const float revolverFireRate = 0.325F * 2;
    const float revolverParryRate = 0.9f;
    
    const int shotgunDmgPellet = 46;
    const int shotgunDmgPellet_Alt = 44;
    const float shotgunFireRate = 1.15F;
    const float shotgunAngle = 10F;
    const int shotgunPelletCount = 22;
    const int shotgunBluePelletCount = 9;
    const int shotgunPelletCount_Alt = 7;
    
    const int rocketLauncherDmg = 200;
    const float rocketLauncherFireRate = 0.3F;
    
    const int swordAttackDmg = 125;
    const float swordAttackRate = 0.2F;
    
    const int arDmg = 60;
    public float currentARFireRate = ARFireRateFastest;
    const float ARFireRateAcceleration = -0.08F;
    const float ARFireRateFastest = 0.075F;//0.1F;
    const float ARFireRateSlowest = 0.135F;
    
    const int mp5_dmg = 62;
    const float mp5FireRate = 0.095F;
    const int mp5_alt_dmg = 700;
    const float mp5_alt_force = 14f;
    const float mp5FireRate_alt = 0.7f;
    public float mp5_grenade_upMult = 0.5f;
    
    public ArmType currentArm = ArmType.Arm1;
    
    
     public Transform gunPlaceFPS;
    
    [Header("Armed fps weapons:")]
    public Revolver revolver_fps;
    public Animator arm_right_animator;
    public Transform gunPoint_revolver_fps;
    //public Animator revolver_animatorFPS;
    public ParticleSystem revolverFX_ps;
    public ParticleSystem revolverFX_stronger_ps;
    public ParticleSystem revolverFX_alt_ps;
    
    
    public Revolver revolver_blue_fps;
    public Transform gunPoint_revolver_blue_fps;
    //public Animator revolver_animatorFPS;
    public ParticleSystem revolverFX_blue_ps;
    public ParticleSystem revolverFX_blue_stronger_ps;
    public ParticleSystem revolverFX_blue_alt_ps;
    
    public TrailRendererController sword1TR;
    
    public Transform shotgun_fps;
    public ParticleSystem shotgunFX_ps;
    public ParticleSystem shotgunFX_alt__ps;
    public Animator shotgun_animatorFPS;
    
    public Transform shotgun_blue_fps;
    public ParticleSystem shotgunFX_blue_ps;
    //public ParticleSystem shotgunFX_alt_blue_ps;
    public Animator shotgun_blue_animatorFPS;
    public MeshRenderer shotgun_hook_rend_fps;
    
    public Transform rocketLauncher_fps;
    public RocketLauncher rocketLauncher;
    public ParticleSystem rocketLauncher_FX;
    public Transform gunPoint_rocketLauncher_fps;
    public Animator rocketLauncher_animatorFPS;
    public Animator rocketLauncher_alt_animatorFPS;
    
    public Transform AR_fps;
    public Transform AR_gunpointFPS;
    public Animator AR_animatorFPS;
    public ParticleSystem AR_ps;
    public ParticleSystem AR_stronger_ps;
    
    public Transform mp5_alt_fps;
    public Transform mp5_alt_gunpointFPS;
    public Animator mp5_alt_animatorFPS;
    public ParticleSystem mp5_alt_ps;
    public Transform gunPoint_mp5_grenade_fps;
    
    
    public Transform AR_ghost_fps;
    public Transform AR_gunpoint_ghostFPS;
    public Animator AR_ghost_animator_fps;
    public ParticleSystem AR_ghost_ps;
    
    
    [Header("Armed tps weapons:")]
    public Transform revolver_tps;
    public Transform shotgun_tps;
    public Transform rocketLauncher_tps;
    public Transform AR_tps;
    public Transform AR_ghost_tps;
    public Transform mp5_alt_tps;
    public Transform gunPoint_revolver_tps;
    public Transform revolver_blue_tps;
    public Transform shotgun_blue_tps;
    
    PlayerController pController;
    PhotonView pv;
    
    public GunType[][] inventory;
    
    
    public InventoryPosition currentSlotPositionInInventory;
    public InventoryPosition prevSlotPositionInInventory;
    
    // public int currentSlot = 0;
    // public int currentAltSlot = 0;
    // public int prevSlot = 1;
    // public int prevAltSlot = 0;
    
    TrailRendererController meleeTr;
  
    
    static int bulletMask = -1;
    static int npcMask2 = -1;
    static int npcMask = -1;
    static int staticObjectsMask = -1;
    static int interactablesMask = -1;
    static int QTSLayer = -1;
    static int groundCeilingMask = -1;
    
    
    bool touchedGroundAfterSpawn = false;
    
    void Awake()
    {
        if(bulletMask == -1)
        {
            bulletMask          = LayerMask.GetMask("Ground", "NPC", "Ceiling");
            npcMask2            = LayerMask.GetMask("NPC2");
            npcMask             = LayerMask.NameToLayer("NPC");
            staticObjectsMask   = LayerMask.GetMask("Ground", "Ceiling");
            interactablesMask   = LayerMask.GetMask("Interactable");
            QTSLayer            = LayerMask.GetMask("QTS");
            groundCeilingMask   = LayerMask.GetMask("Ground", "Ceiling");
        }
        
        inventory = new GunType[4][];
        for(int i = 0; i < inventory.Length; i++)
        {
            inventory[i] = new GunType[2];
        }
        
        BaseFire1_hash = Animator.StringToHash("Base.Fire1");
        pController = GetComponent<PlayerController>();
        pv = GetComponent<PhotonView>();
        
        PlayerInventory.Singleton().Init();
        
        ReadPlayerInventory();
    }
    
    public void ReadPlayerInventory()
    {
        for(int i = 0; i < inventory.Length; i++)
        {
            for(int j = 0; j < inventory[i].Length; j++)
            {
                inventory[i][j] = PlayerInventory.Singleton().playerGunSlots[i][j];
            }
        }
    }
    
    public void WieldRevolver()
    {
        if(pv.IsMine)
        {
            currentSlotPositionInInventory = new InventoryPosition(0, 0);
            WieldGunFPS();
        }
    }
    
    public void WieldShotgun()
    {
        if(pv.IsMine)
        {
            currentSlotPositionInInventory = new InventoryPosition(1, 0);
            WieldGunFPS();
        }
    }
    
    public void WieldAR()
    {
        if(pv.IsMine)
        {
            currentSlotPositionInInventory = new InventoryPosition(2, 0);
            WieldGunFPS();
        }
    }
    
    public void WieldMP5_alt()
    {
        if(pv.IsMine)
        {
            currentSlotPositionInInventory = new InventoryPosition(2, 1);
            WieldGunFPS();
        }
    }
    
    public void WieldRocketLauncher()
    {
        if(pv.IsMine)
        {
            currentSlotPositionInInventory = new InventoryPosition(3, 0);
            WieldGunFPS();
        }
    }
    
    public void WieldRevolver_Blue()
    {
        if(pv.IsMine)
        {
            currentSlotPositionInInventory = new InventoryPosition(0, 1);
            WieldGunFPS();
        }
    }
    
    public void WieldShotgun_Blue()
    {
        if(pv.IsMine)
        {
            currentSlotPositionInInventory = new InventoryPosition(1, 1);
            WieldGunFPS();
        }
    }
    
    
    
    void Start()
    {
        if(pv.IsMine)
        {
            // FPSEffectsSetup();
            WieldGunFPS();
            OnBeforeTouchedGround();
        }
        else
        {
            gunAudio.spatialize = true;
            gunAudio.spatialBlend = 1;
            DestroyFPSGuns();
            WieldGunTPS(0);
        }
    }
    
    public void DestroyFPSGuns()
    {
        Destroy(revolver_fps.gameObject);
        Destroy(shotgun_fps.gameObject);
        Destroy(rocketLauncher_fps.gameObject);
        Destroy(AR_fps.gameObject);
        Destroy(AR_ghost_fps.gameObject);
        Destroy(arm_animator.gameObject);
        Destroy(arm_right_animator.gameObject);
        Destroy(mp5_alt_fps.gameObject);
        Destroy(revolver_blue_fps.gameObject);
        Destroy(shotgun_blue_fps.gameObject);
    }
    
    // void IncrementCurrentSlot(int x)
    // {
    //     int t = currentSlot;
    //     currentSlot += x;
    //     if(currentSlot >= slots.Length)
    //     {
    //         currentSlot = 0;
    //     }
    //     else
    //     {
    //         if(currentSlot <= 0)
    //         {
    //             currentSlot = slots.Length - 1;
    //         }
    //     }
        
    //     prevSlot = t;
        
    // }
    
    void SetCurrentSlot(InventoryPosition newPositionFromInventory)
    {
        InGameConsole.LogFancy(string.Format("SetCurrentSlot() {0}-><color=green>{1}</color>", currentSlotPositionInInventory.ToString(), newPositionFromInventory.ToString()));
        if(currentSlotPositionInInventory.Equals(newPositionFromInventory))
        {
            InGameConsole.LogFancy("Kek");
            return;
        }
        
        InventoryPosition t = currentSlotPositionInInventory;
        
        currentSlotPositionInInventory = newPositionFromInventory;
        
        prevSlotPositionInInventory = t;
    }
    
    bool SwapAltWeaponKey()
    {
        return Inputs.GetInteractKeyDown();
        //return Inputs.GetInteractKey();
    }
    
    int KeyboardSwitchWeapon()
    {
        int Result = -1;
        
        bool key1 = Input.GetKeyDown(KeyCode.Alpha1);
        bool key2 = Input.GetKeyDown(KeyCode.Alpha2);
        bool key3 = Input.GetKeyDown(KeyCode.Alpha3);
        bool key4 = Input.GetKeyDown(KeyCode.Alpha4);
        
        if(key1)
        {
            Result = 0;
        }
        else if(key2)
        {
            Result = 1;
        }
        else if(key3)
        {
            Result = 2;
        }
        else if(key4)
        {
            Result = 3;
        }
        
        return Result;
    }
    
    public float gunTimer = 0;
    float fireRateMultiplier = 1f;
    float gunAltTimer = 0;
    float mp5AltFireTimer = 0;
    const float mp5AltFireTimerCooldown = 2;
    //public float meleeCharge = 0;
    //float meleeChargeRate = 2;
    
    bool hasARGhost = false;
    float ARGhostCooldownTimer = 0;
    const float ARGhostCooldown = 10;//= ARGhostDuration + 12f;
    
    bool ARGhostFireQueued = false;
    int ARGhostFires_queued_num = 0;
    float ARGhostFire_timer = 0;
    const float ARGhostFire_delay = 0.04F;// / 2; // arFireRate / 2
    
    float ARGhostTimer = 0;
    const float ARGhostDuration = 7f;
    
    void EquipARGhost()
    {
        AR_ghost_fps.gameObject.SetActive(true);
    }
    
    
    void EquipARGhostRPC(bool val)
    {
        //AR_ghost_tps.gameObject.SetActive(val);
        
    }
    
    void UnequipARGhost()
    {
        AR_ghost_fps.gameObject.SetActive(false);
    }
    
    void ARGhostFireTick(float dt)
    {
        //if(ARGhostFireQueued)
        if(ARGhostFires_queued_num > 0)
        {
            if(ARGhostFire_timer <= 0)
            {
                //ARGhostFireQueued = false;
                ARGhostFires_queued_num--;
                if(ARGhostFires_queued_num < 0) 
                    ARGhostFires_queued_num = 0;
                
                ARGhostFire_timer = 0;
                Ray AR_Ray = pController.GetFPSRay();
                
                GaussianDistribution gd = new GaussianDistribution();
                Quaternion rotation = Quaternion.LookRotation(AR_Ray.direction);
                Vector3 randomPoint = new Vector3(gd.Next(0f, 1f, -1f, 1f), gd.Next(0f, 1f, -1f, 1f), 0);
                Vector3 bulletDir = AR_Ray.direction + rotation * randomPoint * ARCurrentSpread;
                
                AR_Ray.direction = Math.Normalized(bulletDir);
                
                ARCurrentSpread += ARSpreadPerShot * (hasARGhost ? 2.5f : 1);
                
                //float _ARSpreadMax_ = (hasARGhost ? ARSpreadMax * 2.75F : ARSpreadMax);
                float _ARSpreadMax_ = ARSpreadMax;// * 2.75F;
                
                if(ARCurrentSpread > _ARSpreadMax_)
                {
                    ARCurrentSpread = _ARSpreadMax_;
                }
                    
                byte fpsCommand = (byte)FPS_Func.Shoot_AR_Ghost;
                FPSCommand(fpsCommand, AR_Ray.origin, AR_Ray.direction);    
                pv.RPC("FPSCommand", RpcTarget.Others, fpsCommand, AR_Ray.origin, AR_Ray.direction);
                                    
                    
                //ShootARGhost(AR_Ray.origin, AR_Ray.direction);
                //pv.RPC("ShootARGhost", RpcTarget.Others, AR_Ray.origin, AR_Ray.direction);
            }
            
            ARGhostFire_timer -= dt;
        }
    }
    
    
    float Arm_timer = 0;
    
    void CycleArm()
    {
        switch(currentArm)
        {
            case(ArmType.None):
            {
                currentArm = ArmType.Arm1;
                arm1_overcharged = false;
                arm1_punchCount = 0;
                //InGameConsole.LogFancy("Current ability: " + currentAbilityF.ToString());
                break;
            }
            case(ArmType.Arm1):            
            {
                currentArm = ArmType.None;
                
                //InGameConsole.LogFancy("Current ability: " + currentAbilityF.ToString());
                break;
            }
            case(ArmType.Sunstrike):
            {
                break;
            }
        }
    }
    
    
    //public AudioClip lavaBurstClip;
    public AudioSource gunAudio2;
   
    
    public AudioClip abilityCDRefreshedClip;
    
    public void OnAbilityCDRefreshed()
    {
        gunAudio.PlayOneShot(abilityCDRefreshedClip, 0.5f);
    }
    
    float currentGunSpread = 0;
    const float ARSpreadDecreaseRate = 1f;
    const float ARSpreadMult = 0.1f;
    
    const float punchDistance = 2.85F;
    
    const int punchDamage = 300;
    const int punchDamage_Ult = 600;
    
    
    
    public void Punch()
    {
        if(pv.IsMine)
        {
            Ray ray = pController.GetFPSRay();
            RaycastHit hit;
            
            Arm_timer = arm1_punchCooldown * fireRateMultiplier;
            
            int rand = Random.Range(0, 10);
        
            if(Physics.Raycast(ray, out hit, punchDistance, interactablesMask))
            {
                if(rand % 2 == 0)
                    arm_animator.Play("Base.Punch_impact", 0, 0);
                else
                    arm_animator.Play("Base.Hook_impact", 0, 0);
                    
                Interactable interactable = hit.collider.GetComponent<Interactable>();
                
                if(interactable != null)
                {
                    NetworkObject interactableNetComp = hit.collider.GetComponent<NetworkObject>();
                    if(interactableNetComp)
                    {
                        NetworkObjectsManager.InteractWithNetObject(interactableNetComp.gameObject);
                    }
                }
            }
            
            if(Physics.Raycast(ray, out hit, punchDistance, bulletMask))
            {
                if(rand % 2 == 0)
                    CurrentArmAnimator().Play("Base.Punch_impact", 0, 0);
                else
                    CurrentArmAnimator().Play("Base.Hook_impact", 0, 0);
                //OnHitScan(hit.point, ray.direction, hit.normal, punchDamage, hit.collider);
                OnPunch(hit.point, ray.direction, hit.normal, punchDamage, hit.collider);
                        
            }
            else
            {
                if(rand % 2 == 0)
                    CurrentArmAnimator().Play("Base.Punch", 0, 0);
                else
                    CurrentArmAnimator().Play("Base.Hook", 0, 0);
                    
                Arm_timer = arm1_punchCooldown / 2;
                AudioManager.Play3D(SoundType.punch_whoosh1, ray.origin, 1, 0.7f);
            }
            
        }
    }
    
    void PetWithLeftArm()
    {
        if(arm_animator)
            arm_animator.Play("Base.Pet", 0, 0);
    }
    
   
    
    void OnPunch(Vector3 point, Vector3 damageDirection, Vector3 normal, int damage, Collider col = null, NetworkObject targetNetworkObject = null)
    {
        if(targetNetworkObject == null)
        {
            targetNetworkObject = col.GetComponent<NetworkObject>();
        }
        
        DamagableLimb limb = col.GetComponent<DamagableLimb>();
        if(limb)
        {
            targetNetworkObject    = limb.net_comp_from_parent;
            if(limb.isHeadshot)
            {
                damage *= 2;
                if(pv.IsMine)
                {
                    CrosshairController.MakeHeadShot();
                }
            }
            limb.React(point, damageDirection);
        }
        
        if(targetNetworkObject != null)
        {
            IDamagableLocal idl = targetNetworkObject.GetComponent<IDamagableLocal>();
            if(idl != null)
            {
                idl.TakeDamageLocally(damage, point, damageDirection);
                
                if(pv.IsMine)
                {
                    if(!limb.isMasterAlive)
                    {
                        // InGameConsole.LogFancy("Destroy limb from punch!");
                        CameraShaker.MakeTrauma(0.2f);
                        limb.TakeDamageLimb(damage);
                        float __pitch = Random.Range(0.95f, 1.05f);
                        AudioManager.Play3D(SoundType.punch_impact1, point, __pitch, 0.7f);
                        
                        return;
                    }
                    
                    //DropHealthCrystalsFromPunch(limb.transform, punchDamage, limb.isHeadshot);
                    CameraShaker.MakeTrauma(0.2f);
                    ILaunchableAirbourne ila = targetNetworkObject.GetComponent<ILaunchableAirbourne>();
                    
                    if(damageDirection.y < 0.135F)
                    {
                        damageDirection.y = 0.135F;
                        damageDirection.Normalize();
                    }
                    
                    if(pv.IsMine)
                    {
                        //int small_healing_times = _incomingDamage / UberManager.HEALING_DMG_THRESHOLD;
                        HealthCrystalSmall.MakeSmallHealing(point, 15);
                    }
                    
                    Vector3 launchPos = targetNetworkObject.transform.localPosition;
                    NetworkObjectsManager.CallNetworkFunction(targetNetworkObject.networkId, NetworkCommand.LaunchAirborne, launchPos, damageDirection * 14);
                    if(damage != 0)
                    {
                        //NetworkObjectsManager.CallNetworkFunction(targetNetworkObject.networkId, NetworkCommand.TakeDamageExplosive, damage);
                        NetworkObjectsManager.CallNetworkFunction(targetNetworkObject.networkId, NetworkCommand.TakeDamageLimbWithForce, damage, damageDirection * 300, limb.limb_id);
                    }
                }
                                    
            }
            
        }
        else
        {
            PhotonView pv_that_was_hit = col.GetComponent<PhotonView>();
            if(pv_that_was_hit)
            {
                // col.GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.AllViaServer, dmg);
                //col.GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.All, dmg);
                
            }
            else // We hit something static
            {
                if(pv.IsMine)
                {
                    
                    CameraShaker.MakeTrauma(0.06f);
                }
                ParticlesManager.PlayPooled(ParticleType.punch_impact1, point, -damageDirection);
            }
        }
        
        
        
        float _pitch = Random.Range(0.5F, 0.85F);
        AudioManager.Play3D(SoundType.punch_impact1, point, _pitch, 0.6F);
        // InGameConsole.LogFancy("Punch sound");
    }
    
    void DropHealthCrystalsFromPunch(Transform target_transform, int damage, bool isHeadshot)
    {
        HealthCrystal hc;
        
        int crystals_num = isHeadshot ? 5 : 3;
        
        for(int i = 0; i < crystals_num; i++)
        {
            hc = ObjectPool.s().Get(ObjectPoolKey.HealthCrystal_smaller).GetComponent<HealthCrystal>(); 
            float y = Random.Range(0.75F, 1.5F);
            hc.Launch(target_transform.position + new Vector3(0, y, 0), 20);
        }
    }
    
    public Animator arm_animator;
    public float arm1_punchCooldown = 0.5f;
    
    int arm1_punchCount = 0;
    const int arm1_punchesToCharge = 3;
    
    
    
    void Arm_Tick(float dt)
    {
        Arm_timer -= dt;
       
        if(Arm_timer <= 0f)
        {
            Arm_timer = 0;
        }
        
        if(arm1_overcharged)
        {
            arm1_overcharge_timer -= dt;
            
            if(arm1_overcharge_timer <= 0f)
            {
                arm1_overcharged = false;
            }
        }
       
        if(Inputs.SwitchArmKeyDown() && pController.CanControlPlayer())
        {
            CycleArm();
        }
            
        switch(currentArm)
        {
            case(ArmType.None):
            {
                break;
            }
            case(ArmType.Arm1):
            {
                if(Inputs.Arm_FKeyDown() && pController.CanControlPlayer())
                {
                    if(GetCurrentWeapon() == GunType.AR && hasARGhost)
                        return;
                        
                    if(Arm_timer == 0f)
                    {
                        //ArmLeft_Pet   
                        Ray ray = pController.GetFPSRay();
                        
                        RaycastHit hit;
                        if(Physics.Raycast(ray, out hit, punchDistance, groundCeilingMask))
                        {
                            SecretLady lady = hit.collider.GetComponent<SecretLady>();
                            if(lady != null)
                            {
                                lady.OnTouch();
                                Arm_timer = 0.25f;
                                byte fpsCommand = (byte)FPS_Func.ArmLeft_Pet;
                                FPSCommand(fpsCommand);
                                pv.RPC("FPSCommand", RpcTarget.Others, fpsCommand);
                            }
                            else
                            {
                                byte fpsCommand = (byte)FPS_Func.Punch1;
                                FPSCommand(fpsCommand);
                                pv.RPC("FPSCommand", RpcTarget.Others, fpsCommand);
                            }
                        }
                        else
                        {
                            byte fpsCommand = (byte)FPS_Func.Punch1;
                            FPSCommand(fpsCommand);
                            pv.RPC("FPSCommand", RpcTarget.Others, fpsCommand);
                        }
                        
                        Ray punchQtsRay = pController.GetFPSRay();
                        RaycastHit qts_hit;
                        
                        if(Physics.Raycast(punchQtsRay, out qts_hit, punchDistance, QTSLayer))
                        {
                            QuickTimeSphere qts = qts_hit.collider.GetComponent<QuickTimeSphere>();
                            if(qts)
                            {
                                qts.OnHit();
                                pController.MakeImmuneForDamageForXTime(0.150F);
                            }
                        }
                    }
                }
                break;
            }
            case(ArmType.Sunstrike):
            {
                break;
            }
            
        }
    }
    
    float switchWeaponTimer = 0f;
    
    // bool SwitchToPrevSlot_KeyDown()
    // {
    //     return Input.GetKeyDown(KeyCode.Q);
    // }
    
    bool isF_arm_combo = false;
    
    RevolverState revolverState;
    RevolverState revolver_blue_State;
    
    
    public Animator CurrentArmAnimator()
    {
        switch(currentArm)
        {
            case(ArmType.Arm1):
            {
                return arm_animator;
            }
            default:
            {
                return null;
            }
        }
    }
    
    void OnRevolverUltStarted()
    {
        pController.moveSpeedMultiplier_RevolverUlt = 0.6f;
        pController.canSlide = false;
        pController.SetBerserkFov();
        PostProcessingController2.SetState(PostProcessingState.Berserk);  
        if(CurrentArmAnimator())
        {
            CurrentArmAnimator().Play("Base.Revolver_Ult", 0, 0);
            arm_right_animator.Play("Base.Ult_Roll", 0, 0);
        }
        revolver_fps.anim.Play("Base.Ult", 0, 0);
    }
    
    void OnRevolverBlueUltStarted()
    {
        pController.moveSpeedMultiplier_RevolverUlt = 0.6f;
        pController.canSlide = false;
        pController.SetBerserkFov();
        PostProcessingController2.SetState(PostProcessingState.Berserk);  
        if(CurrentArmAnimator())
        {
            CurrentArmAnimator().Play("Base.Revolver_Ult", 0, 0);
            arm_right_animator.Play("Base.Ult_Roll", 0, 0);
        }
        revolver_blue_fps.anim.Play("Base.Ult", 0, 0);
    }
    
    AnimatorStateInfo leftArmAnimatorStateInfo;
    
    void OnRevolverUltEnded()
    {
        if(CurrentArmAnimator() != null)
        {
            if(revolverState == RevolverState.Ult)
            {
                CurrentArmAnimator().Play("Base.UltToEnd", 0, 0);
            }
            // leftArmAnimatorStateInfo = CurrentArmAnimator().GetCurrentAnimatorStateInfo(0);
            // if(leftArmAnimatorStateInfo.shortNameHash == Animator.StringToHash("Base.UltToEnd"))
            // {
            //     InGameConsole.LogFancy(string.Format("NameHash: {0} is <color=green>OK</color>", leftArmAnimatorStateInfo.shortNameHash));
            // }
            // else
            // {
            //     InGameConsole.LogFancy(string.Format("NameHash: {0}", leftArmAnimatorStateInfo.shortNameHash));
            // }
        }
        if(!isInBerserk)
        {
            pController.SetTargetFovNormal();
        }
    }
    
    void SetPlayerControllerNormalState()
    {
        PostProcessingController2.SetState(PostProcessingState.Normal);  
        pController.moveSpeedMultiplier_RevolverUlt = 1;
        pController.canSlide = true;
        pController.SetTargetFovNormal();
    }
    
    public GunType GetWeaponFromInventory(int mainIndex, int altIndex)
    {
        return inventory[mainIndex][altIndex];
    }
    
    public GunType GetCurrentWeapon()
    {
        return inventory[currentSlotPositionInInventory.mainIndex][currentSlotPositionInInventory.altIndex];
    }
    
    public void R_Alt()
    {
        if(GetCurrentWeapon() != GunType.Revolver)
        {
            return;
        }
        
        revolver_alt_fired_count++;
        
        gunTimer = revolverFireRate * 1.0F;
        
        //Ray revolverRay = pController.GetFPSRay();
        Ray revolverRay = pController.GetMiddleFPSRay();
        byte fpsCommand = (byte)FPS_Func.Shoot_revolver_stronger;
        FPSCommand(fpsCommand, revolverRay.origin, revolverRay.direction);
        pv.RPC("FPSCommand", RpcTarget.Others, fpsCommand, revolverRay.origin, revolverRay.direction);
    }
    
    public void S_Alt()
    {
        if(GetCurrentWeapon() != GunType.Shotgun)
        {
            return;
        }
        gunTimer = fireRateMultiplier * shotgunFireRate * 1.25f;
        
        // Ray shotgunRay = pController.GetFPSRay();
        Ray shotgunRay = pController.GetLowerFPSRay();
        byte fpsCommand = (byte)FPS_Func.Shoot_shotgun_alt;
        
        FPSCommand(fpsCommand, shotgunRay.origin, shotgunRay.direction);
        pv.RPC("FPSCommand", RpcTarget.Others, fpsCommand, shotgunRay.origin, shotgunRay.direction);
        
        // byte seed = (byte)Random.Range(0, 64);
        // FPSCommand2(fpsCommand, shotgunRay.origin, shotgunRay.direction, seed);
        // pv.RPC("FPSCommand2", RpcTarget.Others, fpsCommand, shotgunRay.origin, shotgunRay.direction, seed);
    }
    
    public void S_Blue()
    {
        if(GetCurrentWeapon() != GunType.Shotgun_blue)
        {
            return;
        }
        gunTimer = fireRateMultiplier * shotgunFireRate * 1.33f;
        
        // Ray shotgunRay = pController.GetFPSRay();
        Ray shotgunRay = pController.GetLowerFPSRay();
        byte fpsCommand = (byte)FPS_Func.Shoot_shotgun_blue;
        
        byte seed = (byte)Random.Range(0, 64);
        InGameConsole.LogFancy("Blue Shotgun seed: " + seed);
        FPSCommand2(fpsCommand, shotgunRay.origin, shotgunRay.direction, seed);
        pv.RPC("FPSCommand2", RpcTarget.Others, fpsCommand, shotgunRay.origin, shotgunRay.direction, seed);
        
        // byte seed = (byte)Random.Range(0, 64);
        // FPSCommand2(fpsCommand, shotgunRay.origin, shotgunRay.direction, seed);
        // pv.RPC("FPSCommand2", RpcTarget.Others, fpsCommand, shotgunRay.origin, shotgunRay.direction, seed);
    }
    
    public GrapplingHook hook;
       
    // public void HookCallFPSCommand()
    // {
    //     if(GetCurrentWeapon() != GunType.Shotgun_blue)
    //     {
    //         InGameConsole.LogFancy("TRYING TO HOOK WITHOUT <color=blue>BLUE</color> SHOTGUN");
    //         return;
    //     }
        
    //     gunTimer = fireRateMultiplier * shotgunFireRate * 1.25f;
        
    //     // Ray shotgunRay = pController.GetFPSRay();
    //     Ray shotgunRay = pController.GetLowerFPSRay();
    //     byte fpsCommand = (byte)FPS_Func.Hook_shotgun_blue;
        
    //     FPSCommand(fpsCommand, shotgunRay.origin, shotgunRay.direction);
    //     pv.RPC("FPSCommand", RpcTarget.Others, fpsCommand, shotgunRay.origin, shotgunRay.direction);
        
    //     // byte seed = (byte)Random.Range(0, 64);
    //     // FPSCommand2(fpsCommand, shotgunRay.origin, shotgunRay.direction, seed);
    //     // pv.RPC("FPSCommand2", RpcTarget.Others, fpsCommand, shotgunRay.origin, shotgunRay.direction, seed);
    // }
    
    float hookTimer; 
    
    void Hook(Vector3 hookStartPos, Vector3 dir)
    {
        if(pv.IsMine)
        {
            //CameraShaker.MakeTrauma(0.4F);
            //gunTimer = fireRateMultiplier * 0.3f;
            hookTimer = 0.8f;
            shotgun_blue_animatorFPS.Play("Base.Fire_alt", 0, 0);
            //shotgunFX_ps.Play();
            //shotgunFX_alt__ps.Play();
            
            // if(pController.IsGrounded())
            // {
            //     pController.BoostVelocityAdditive(-shotDir * 17.5f);
            // }
            // else
            // {
            //     pController.BoostVelocityAdditive(-shotDir * 13.5f);
            // }
        }
        byte fpsCommand = (byte)FPS_Func.Hook_shotgun_blue;
        FPSCommand(fpsCommand, hookStartPos, dir);
        pv.RPC("FPSCommand", RpcTarget.Others, fpsCommand, hookStartPos, dir);
        // hook.ReleaseHook();
        // hook.LaunchHook(pController, dir, pv.IsMine);
    }
    
    int revolver_alt_fired_count            = 0;
    float revolver_charge;
    const float revolver_chargeRate         = 1.85F;
    
    float Ability_R_timer                   = 0;
    float Ability_R_cooldown_timer          = 0;
    const float Ability_R_Cooldown          = Ability_R_Berserk_duration;
    const float Ability_R_Berserk_duration  = 4.5F;
    bool isInBerserk = false;
    
    float berserkMultiplier = 0.4F;
    float berserkMultiplierMoveSpeed = 0.2F;
    
    public void OnInject()
    {
        if(pv.IsMine)
        {
            BerserkPowerUp();
            AudioManager.PlayClip(SoundType.gun_pick_up, 0.8f, 1f);
        }
        //InGameConsole.LogFancy("OnInject()");
    }
    
    // public void rofl1()
    // {
    //     AudioManager.PlayClip(SoundType.gun_pick_up, 0.6f, 1.1f);
    // }
    
    // public void rofl2()
    // {
    //     AudioManager.PlayClip(SoundType.gun_pick_up, 0.3f, 1.2f);
    // }
    
    // public void rofl3()
    // {
    //     AudioManager.PlayClip(SoundType.gun_pick_up, 0.45f, 1.3f);
    // }
    
    public void StartArmInject()
    {
        Animator _currentArmAnim = CurrentArmAnimator();
        if(_currentArmAnim)
        {
            _currentArmAnim.Play("Base.Inject", 0, 0);
        }
        Invoke(nameof(OnInject), 1.05f / 2.5f);
        Ability_R_timer = Ability_R_Berserk_duration;
        Ability_R_cooldown_timer = Ability_R_Cooldown;
        
    }
    
    public void BerserkPowerUp()
    {
        if(pv.IsMine)
        {
            pController.SetBerserkFov();
            PostProcessingController2.SetState(PostProcessingState.Berserk);  
            pController.TakeDamageNonLethal(50);
            // Ability_R_timer = Ability_R_Berserk_duration;
            // Ability_R_cooldown_timer = Ability_R_Cooldown;
            
            pController.moveSpeedMultiplier += pController.moveSpeedMultiplier * berserkMultiplierMoveSpeed;
            
            isInBerserk = true;
            fireRateMultiplier -= fireRateMultiplier * berserkMultiplier;
            InGameConsole.LogFancy(string.Format("BerserkPowerUp(), fireRateMultiplier is <color=red>{0}</color>", ((int)(fireRateMultiplier*100)).ToString()));
        }
    }
    
    public void BerserkPowerDown()
    {
        if(pv.IsMine)
        {
            pController.SetTargetFovNormal();
            PostProcessingController2.SetState(PostProcessingState.Normal);  
            Ability_R_timer = Ability_R_Berserk_duration;
            Ability_R_cooldown_timer = Ability_R_Cooldown;
            
            isInBerserk = false;
            fireRateMultiplier = 1f;
            pController.moveSpeedMultiplier = 1F;
        }
    }
    
    
    void Ability_R_Tick(float dt)
    {
        Ability_R_timer -= dt;
        if(isInBerserk)
        {
            //InGameConsole.LogFancy(string.Format("Ability_R_timer: <color=yellow>{0}</color>", Ability_R_timer.ToString("f")));
            if(Ability_R_timer <= 0f)
            {
                BerserkPowerDown();
            }
        }
        
        
        Ability_R_cooldown_timer -= dt;
        if(Ability_R_cooldown_timer < 0f)
        {
            Ability_R_cooldown_timer = 0;
        }
        
        if(Ability_R_cooldown_timer == 0f && Inputs.GetAbilityR_KeyDown())
        {
            StartArmInject();
            //BerserkPowerUp();
        }
    }
    
    public GameObject[] onFirstTouchedGroundObjectsToActivate;
    
    void OnBeforeTouchedGround()
    {
        if(onFirstTouchedGroundObjectsToActivate != null)
        {
            int len = onFirstTouchedGroundObjectsToActivate.Length;
            for(int i = 0; i < len; i++)
            {
                onFirstTouchedGroundObjectsToActivate[i].SetActive(false);
            }
        }
    }
    
    void OnFirstTouchedGround()
    {
        gunAudio.PlayOneShot(switchWeaponClip);
        if(onFirstTouchedGroundObjectsToActivate != null)
        {
            int len = onFirstTouchedGroundObjectsToActivate.Length;
            for(int i = 0; i < len; i++)
            {
                onFirstTouchedGroundObjectsToActivate[i].SetActive(true);
            }
        }
    }
    
    public GunType GetCurrentWeaponInHands()
    {
        return inventory[currentSlotPositionInInventory.mainIndex][currentSlotPositionInInventory.altIndex];
    }
    
    public GunType GetPrevWeapon()
    {
        return inventory[prevSlotPositionInInventory.mainIndex][prevSlotPositionInInventory.altIndex];
    }
    
    public bool isInDialogueOrInRange = false;
    
    void Update()
    {
        if(pv.IsMine == false)
        {
            return;
        }
        
        if(!pController.isAlive)
        {
            return;
        }
        
        if(!touchedGroundAfterSpawn)
        {
            if(pController.IsGrounded())
            {
                touchedGroundAfterSpawn = true;
                OnFirstTouchedGround();
            }
            return;
        }
        
        float dt = UberManager.DeltaTime();
        hookTimer -= dt;
        if(hookTimer <= 0)
        {
            hookTimer = 0;
        }
        
        Arm_Tick(dt);
        Ability_R_Tick(dt);
        rocketLauncher.CooldownsTick(dt);
        
        if(Input.GetKeyDown(KeyCode.Alpha6))
        {
            for(int i = 0; i < inventory.Length; i++)
            {
                string s = "";
                for(int j = 0; j < inventory[i].Length; j++)
                {
                    s += inventory[i][j].ToString() + " ";
                }
                InGameConsole.LogFancy(s);
            }
        }
        
        TickWeaponCooldowns(dt);
               
        int keyBoard = KeyboardSwitchWeapon();
        
        if(!pController.CanControlPlayer())
        {
            keyBoard = -1;
        }
        
        switchWeaponTimer -= dt;
        if(switchWeaponTimer <= 0)
        {
            switchWeaponTimer = 0f;
        }
        
        if(keyBoard == -1)
        {
            // float mouseScroll = Input.GetAxis("Mouse ScrollWheel");
            // if(!pController.CanControlPlayer())
            // {
            //     mouseScroll = 0;
            // }
            
            // if(mouseScroll > 0)
            // {
            //     IncrementCurrentSlot(1);
            //     WieldGunFPS();
            // }
            // else
            // {
            //     if(mouseScroll < 0)
            //     {
            //         IncrementCurrentSlot(-1);
            //         WieldGunFPS();
            //     }
            //     else
            //     {
            if(Inputs.SwitchToPrevSlot_KeyDown() && pController.CanControlPlayer())
            {
                if(switchWeaponTimer == 0f && inventory[currentSlotPositionInInventory.mainIndex][currentSlotPositionInInventory.altIndex] != GunType.None)
                {
                    InventoryPosition t = currentSlotPositionInInventory;
                    currentSlotPositionInInventory = prevSlotPositionInInventory;;
                    prevSlotPositionInInventory = t;
                    switchWeaponTimer = GetSwitchWeaponDuration();
                    WieldGunFPS();
                }
            }
            else if(SwapAltWeaponKey())
            {
                if(DialogueManager.GetState() != DialogueState.Hidden)
                {
                    return;
                }
                //GunType currentGun = inventory[currentSlotPositionInInventory.mainIndex][currentSlotPositionInInventory.altIndex];
                GunType currentGun = GetCurrentWeaponInHands();
                
                switch(currentGun)
                {
                    case(GunType.None):
                    {
                        break;
                    }
                    case(GunType.Revolver):
                    {
                        if(GetWeaponFromInventory(0, 1) == GunType.None)
                        {
                            return;
                        }
                        currentSlotPositionInInventory = new InventoryPosition(0, 1);
                        //inventory[currentSlotPositionInInventory.mainIndex][currentSlotPositionInInventory.altIndex] = GunType.MP5_alt;
                        
                        WieldGunFPS();
                        switchWeaponTimer = GetSwitchWeaponDuration();
                        break;
                    }
                    case(GunType.Revolver_blue):
                    {
                        if(GetWeaponFromInventory(0, 0) == GunType.None)
                        {
                            return;
                        }
                        currentSlotPositionInInventory = new InventoryPosition(0, 0);
                        WieldGunFPS();
                        switchWeaponTimer = GetSwitchWeaponDuration();
                        
                        break;
                    }
                    case(GunType.Shotgun):
                    {
                        if(GetWeaponFromInventory(1, 1) == GunType.None)
                        {
                            return;
                        }
                        currentSlotPositionInInventory = new InventoryPosition(1, 1);
                        WieldGunFPS();
                        switchWeaponTimer = GetSwitchWeaponDuration();
                        break;
                    }
                    case(GunType.Shotgun_blue):
                    {
                        if(GetWeaponFromInventory(1, 0) == GunType.None)
                        {
                            return;
                        }
                        currentSlotPositionInInventory = new InventoryPosition(1, 0);
                        WieldGunFPS();
                        switchWeaponTimer = GetSwitchWeaponDuration();
                        break;
                    }
                    case(GunType.RocketLauncher):
                    {
                        break;
                    }
                    case(GunType.AR):
                    {
                        if(GetWeaponFromInventory(2, 1) == GunType.None)
                        {
                            return;
                        }
                        currentSlotPositionInInventory = new InventoryPosition(2, 1);
                        //inventory[currentSlotPositionInInventory.mainIndex][currentSlotPositionInInventory.altIndex] = GunType.MP5_alt;
                        
                        WieldGunFPS();
                        switchWeaponTimer = GetSwitchWeaponDuration();
                        break;
                    }
                    case(GunType.MP5_alt):
                    {
                        if(GetWeaponFromInventory(2, 0) == GunType.None)
                        {
                            return;
                        }
                        currentSlotPositionInInventory = new InventoryPosition(2, 0);
                        
                        //inventory[currentSlotPositionInInventory] = GunType.AR;
                        WieldGunFPS();
                        switchWeaponTimer = GetSwitchWeaponDuration();
                        break;
                    }
                }
            }
            //     }
            // }
        }
        else
        {
            //if(keyBoard != GetWeaponFromInventory && inventory[keyBoard] != GunType.None)
            //if(keyBoard != currentSlotPositionInInventory.mainIndex && GetWeaponFromInventory(keyBoard, 0) != GunType.None)
            if(keyBoard != currentSlotPositionInInventory.mainIndex)
            {
                if(switchWeaponTimer == 0)
                {
                    if(GetCurrentWeaponInHands() == GunType.Revolver)
                    {
                        OnRevolverUltEnded();
                    }
                    
                    SetCurrentSlot(new InventoryPosition(keyBoard, 0));
                    switchWeaponTimer = GetSwitchWeaponDuration();
                    WieldGunFPS();
                }
            }
        }
        
        bool primaryFire =  Input.GetMouseButton(0);
        bool primaryFireKeyDown = Input.GetMouseButtonDown(0);
        bool altFire = Input.GetMouseButton(1);
        bool altFireKeyDown = Input.GetMouseButtonDown(1);
        
        
        
        if(!pController.CanControlPlayer())
        {
            altFire = false;
            primaryFire = false;
        }
        
        gunTimer -= dt;
        if(gunTimer < 0)
        {
            gunTimer = 0;
        }
        
        gunAltTimer -= dt;
        if(gunAltTimer < 0)
        {
            gunAltTimer = 0;
        }
        
        switch(GetCurrentWeaponInHands())
        {
            case(GunType.Revolver):
            {
                switch(revolverState)
                {
                    case(RevolverState.Normal):
                    {
                        if(gunTimer == 0)
                        {
                            isF_arm_combo = false;
                            //if(primaryFireKeyDown)
                            if(primaryFire)
                            {
                                Ray revolverRay = pController.GetFPSRay();
                                
                                revolver_fps.OnShotFPS();
                                
                                gunTimer += fireRateMultiplier * revolverFireRate;
                                byte fpsCommand = (byte)FPS_Func.Shoot_revolver;
                                FPSCommand(fpsCommand, revolverRay.origin, revolverRay.direction);
                                pv.RPC("FPSCommand", RpcTarget.Others, fpsCommand, revolverRay.origin, revolverRay.direction);
                                
                                RaycastHit qts_hit;
                                //InGameConsole.LogFancy("Trying to call qts!!!!!");
                                if(Physics.Raycast(revolverRay, out qts_hit, 200f, QTSLayer))
                                {
                                    //InGameConsole.LogFancy("Calling qts!!!!!");
                                    CameraShaker.MakeTrauma(0.15f);
                                    revolver_fps.anim.Play("Base.Parry", 0, 0);
                                    arm_right_animator.Play("Base.Parry", 0, 0);
                                    gunTimer  = revolverParryRate;
                                    QuickTimeSphere qts = qts_hit.collider.GetComponent<QuickTimeSphere>();
                                    if(qts)
                                        qts.OnHit();
                                }
                                
                            }
                            else
                            {
                                if(altFire)
                                {
                                    revolverState = RevolverState.Charging;
                                }
                            }
                        }
                        break;
                    }
                    case(RevolverState.Charging):
                    {
                        revolver_fps.StartShaking();
                        revolver_fps.shaking_mult = revolver_charge;
                        
                        if(primaryFireKeyDown || !altFire)
                        {
                            if(revolver_charge >= 0.9F)
                            {
                                const float base_revolver_alt_delay = 0.06F;
                                
                                R_Alt();
                                Invoke("R_Alt", base_revolver_alt_delay * 1);
                                Invoke("R_Alt", base_revolver_alt_delay * 2);
                                if(isInBerserk)
                                {
                                    Invoke("R_Alt", base_revolver_alt_delay * 3);
                                    Invoke("R_Alt", base_revolver_alt_delay * 4);
                                }
                            }
                            revolver_charge = 0;
                            revolverState = RevolverState.Normal;
                            
                            revolver_fps.StopShaking();
                            revolver_fps.shaking_mult = revolver_charge;
                        }
                        else
                        {
                            if(altFire && Inputs.Arm_FKeyDown() && !pController.isSliding && currentArm != ArmType.None)
                            {
                                revolver_charge = 0;
                                revolver_fps.StopShaking();
                                revolver_fps.shaking_mult = revolver_charge;
                                revolver_fps.shaking_mult_smoothed = 0;
                                OnRevolverUltStarted();
                                revolverState = RevolverState.Ult;
                            }
                            else
                            {
                                if(!altFire)
                                {
                                    revolver_charge -= dt * (revolver_chargeRate / 2) * fireRateMultiplier;    
                                }
                                else
                                    revolver_charge += dt * revolver_chargeRate * fireRateMultiplier;
                                //InGameConsole.LogFancy(string.Format("Charging <color=yellow>{0}</color>", revolver_charge.ToString("f")));
                                if(revolver_charge >= 1)
                                {
                                    revolver_charge = 1f;
                                }
                                if(revolver_charge <= 0)
                                {
                                    revolverState = RevolverState.Normal;
                                }
                            }
                        }
                        
                        break;
                    }
                    case(RevolverState.Ult):
                    {
                        if(primaryFireKeyDown)
                        {
                            if(gunTimer == 0)
                            {
                                gunTimer += fireRateMultiplier * revolverFireRate * 2.25f;
                                
                                Ray revolverRay = pController.GetFPSRay();
                                    
                                revolver_fps.OnShotFPS();
                                
                                byte fpsCommand = (byte)FPS_Func.Shoot_revolver_ult;
                                FPSCommand(fpsCommand, revolverRay.origin, revolverRay.direction);
                                pv.RPC("FPSCommand", RpcTarget.Others, fpsCommand, revolverRay.origin, revolverRay.direction);
                                
                                
                                
                                SetPlayerControllerNormalState();
                                revolver_fps.shaking_mult = 0;
                                
                                revolverState = RevolverState.Normal;
                            }
                        }
                        break;
                    }
                }
                break;
            }
            case(GunType.Revolver_blue):
            {
                switch(revolver_blue_State)
                {
                    case(RevolverState.Normal):
                    {
                        if(gunTimer == 0)
                        {
                            isF_arm_combo = false;
                            //if(primaryFireKeyDown)
                            if(primaryFire)
                            {
                                Ray revolverRay = pController.GetFPSRay();
                                
                                revolver_blue_fps.OnShotFPS();
                                
                                gunTimer += fireRateMultiplier * revolverFireRate;
                                byte fpsCommand = (byte)FPS_Func.Shoot_revolver_blue;
                                FPSCommand(fpsCommand, revolverRay.origin, revolverRay.direction);
                                pv.RPC("FPSCommand", RpcTarget.Others, fpsCommand, revolverRay.origin, revolverRay.direction);
                                
                                //RaycastHit qts_hit;
                                //InGameConsole.LogFancy("Trying to call qts!!!!!");
                                // if(Physics.Raycast(revolverRay, out qts_hit, 200f, QTSLayer))
                                // {
                                //     //InGameConsole.LogFancy("Calling qts!!!!!");
                                //     CameraShaker.MakeTrauma(0.15f);
                                //     revolver_blue_fps.anim.Play("Base.Parry", 0, 0);
                                //     arm_right_animator.Play("Base.Parry", 0, 0);
                                //     gunTimer  = revolverParryRate;
                                //     QuickTimeSphere qts = qts_hit.collider.GetComponent<QuickTimeSphere>();
                                //     if(qts)
                                //         qts.OnHit();
                                // }
                                
                            }
                            else
                            {
                                if(altFire)
                                {
                                    revolver_blue_State = RevolverState.Charging;
                                    //revolver_alt_fired_count = 0;
                                    
                                    //R_Alt();
                                    //Invoke("R_Alt", 0.125F);
                                    //Invoke("R_Alt", 0.25F);
                                   // Invoke("R_Alt", 0.375F);
                                }
                            }
                        }
                        // else
                        // {
                        //     //Ult:
                        //     // if(!pController.isSliding && currentArm != ArmType.None)
                        //     // {
                        //     //     if(Arm_FKeyDown())
                        //     //     {
                        //     //         OnRevolverUltStarted();
                        //     //         revolverState = RevolverState.Ult;
                        //     //     }
                        //     // }
                        // }
                        break;
                    }
                    case(RevolverState.Charging):
                    {
                        revolver_blue_fps.StartShaking();
                        revolver_blue_fps.shaking_mult = revolver_charge;
                        
                        if(primaryFireKeyDown || !altFire)
                        {
                            if(revolver_charge >= 0.9F)
                            {
                                 Ray revolverRay = pController.GetFPSRay();
                                revolver_blue_fps.OnShotFPS();
                                
                                gunTimer += fireRateMultiplier * revolverFireRate;
                                byte fpsCommand = (byte)FPS_Func.Shoot_revolver_blue_stronger;
                                FPSCommand(fpsCommand, revolverRay.origin, revolverRay.direction);
                                pv.RPC("FPSCommand", RpcTarget.Others, fpsCommand, revolverRay.origin, revolverRay.direction);
                            }
                            revolver_charge = 0;
                            revolver_blue_State = RevolverState.Normal;
                            
                            revolver_blue_fps.StopShaking();
                            revolver_blue_fps.shaking_mult = revolver_charge;
                        }
                        else
                        {
                            if(altFire && Inputs.Arm_FKeyDown() && !pController.isSliding && currentArm != ArmType.None)
                            {
                                revolver_charge = 0;
                                revolver_blue_fps.StopShaking();
                                revolver_blue_fps.shaking_mult = revolver_charge;
                                revolver_blue_fps.shaking_mult_smoothed = 0;
                                OnRevolverBlueUltStarted();
                                revolver_blue_State = RevolverState.Ult;
                            }
                            else
                            {
                                //InGameConsole.LogFancy(string.Format("Charging <color=yellow>{0}</color>", revolver_charge.ToString("f")));
                                revolver_charge += dt * revolver_chargeRate * fireRateMultiplier;
                                if(revolver_charge >= 1)
                                {
                                    revolver_charge = 1f;
                                }
                            }
                        }
                        
                        break;
                    }
                    case(RevolverState.Ult):
                    {
                        if(primaryFireKeyDown)
                        {
                            if(gunTimer == 0)
                            {
                                gunTimer += fireRateMultiplier * revolverFireRate;
                                
                                Ray revolverRay = pController.GetFPSRay();
                                    
                                revolver_blue_fps.OnShotFPS();
                                
                                byte fpsCommand = (byte)FPS_Func.Shoot_revolver_blue_ult;
                                Vector3 explosionPosition = revolverRay.origin + revolverRay.direction * 0.25f;
                                FPSCommand(fpsCommand, explosionPosition, revolverRay.direction);
                                pv.RPC("FPSCommand", RpcTarget.Others, fpsCommand, explosionPosition, revolverRay.direction);
                                
                                SetPlayerControllerNormalState();
                                revolver_blue_fps.shaking_mult = 0;
                                
                                revolver_blue_State = RevolverState.Normal;
                            }
                        }
                        break;
                    }
                }
                break;
            }
            case(GunType.Shotgun):
            {
                    if(gunTimer == 0)
                    {
                        if(primaryFire)
                        {
                            gunTimer += fireRateMultiplier * shotgunFireRate;
                            
                            Ray shotgunRay = pController.GetFPSRay();
                            byte seed = (byte)Random.Range(0, 64);
                            
                            byte fpsCommand = (byte)FPS_Func.Shoot_shotgun;
                                    
                            FPSCommand2(fpsCommand, shotgunRay.origin, shotgunRay.direction, seed);
                            pv.RPC("FPSCommand2", RpcTarget.Others, fpsCommand, shotgunRay.origin, shotgunRay.direction, seed);
                        }
                        else
                        {
                            if(altFire && gunAltTimer == 0)
                            {
                                S_Alt();
                                //Invoke("S_Alt", 0f);
                                //Invoke("S_Alt", 0.175F);
                            }
                        }
                    }
                    
                   
                
                break;
            }
            case(GunType.Shotgun_blue):
            {
                    if(hook && hook.state == HookState.None && hookTimer == 0)
                    {
                        if(!shotgun_hook_rend_fps.enabled)
                        {
                            OnBlueHookReturned();
                            shotgun_hook_rend_fps.enabled = true;
                        }
                    }
                    else
                        shotgun_hook_rend_fps.enabled = false;
                
                    if(gunTimer == 0)
                    {
                        if(primaryFire)
                        {
                            gunTimer += fireRateMultiplier * shotgunFireRate;
                            
                            S_Blue();
                            Invoke(nameof(S_Blue), 0.18f);
                        }
                        else
                        {
                            //if(altFire && gunAltTimer == 0)
                            if(altFire)
                            {
                                Ray shotgunRay = pController.GetFPSRay();
                                
                                if(hook.state == HookState.None && hookTimer == 0)
                                {
                                    Hook(pController.GetCenterPosition(), shotgunRay.direction);
                                }
                            }
                        }
                    }
                
                break;
            }
            case(GunType.RocketLauncher):
            {
                if(gunTimer == 0 && rocketLauncher.GetRocketsInMagazine() > 0)
                {
                    if(primaryFire)
                    {
                        Vector3 _shotPos = gunPoint_rocketLauncher_fps.position;
                        Vector3 _shotDir = gunPoint_rocketLauncher_fps.forward;
                        
                        if(pController.CheckIfFPSRayObscure())
                        {
                            Ray rocketLauncherRay = pController.GetFPSRay();
                            _shotPos = rocketLauncherRay.origin;
                            _shotDir = rocketLauncherRay.direction;
                            _shotPos -= _shotDir * 1.0F;
                        }
                        
                        gunTimer += fireRateMultiplier * rocketLauncherFireRate;
                        
                        byte fpsCommand = (byte)FPS_Func.Shoot_rocketLauncher;
                                    
                        FPSCommand(fpsCommand, _shotPos, _shotDir);
                        pv.RPC("FPSCommand", RpcTarget.Others, fpsCommand, _shotPos, _shotDir);
                    }
                    else if (altFire)
                    {
                        Vector3 _shotPos = pController.GetLowerFPSRay().origin;
                        Vector3 _shotDir = pController.GetLowerFPSRay().direction;
                        
                        if(pController.CheckIfFPSRayObscure())
                        {
                            Ray rocketLauncherRay = pController.GetFPSRay();
                            _shotPos = rocketLauncherRay.origin;
                            _shotDir = rocketLauncherRay.direction;
                            _shotPos -= _shotDir * 1.0F;
                        }
                        
                        gunTimer += fireRateMultiplier * rocketLauncherFireRate;
                        
                        Vector3 upDir = pController.GetFPSCameraTransform().up;
                        
                        byte fpsCommand = (byte)FPS_Func.Shoot_rocketLauncher_alt;
                                    
                        FPSC3(fpsCommand, _shotPos, _shotDir, upDir);
                        pv.RPC("FPSC3", RpcTarget.Others, fpsCommand, _shotPos, _shotDir, upDir);
                    }
                }
                break;
            }
            case(GunType.AR):
            {
                ARGhostFireTick(dt);
                
                if(gunTimer == 0)
                {
                    if(primaryFire)
                    {
                        Ray AR_Ray = pController.GetFPSRay();
                        
                        GaussianDistribution gd = new GaussianDistribution();
                        Quaternion rotation = Quaternion.LookRotation(AR_Ray.direction);
                        Vector3 randomPoint = new Vector3(gd.Next(0f, 1f, -1f, 1f), gd.Next(0f, 1f, -1f, 1f), 0);
                        Vector3 bulletDir = AR_Ray.direction + rotation * randomPoint * ARCurrentSpread;
                        
                        // InGameConsole.LogFancy("BulletDir: " + bulletDir.ToString());
                        // InGameConsole.LogFancy("Spread: " + _ARSpreadMax_.ToString());
                        AR_Ray.direction = Math.Normalized(bulletDir);
                        
                
                        float _ARSpreadMax_ = (hasARGhost ? ARSpreadMax * 1.55f : ARSpreadMax);
                        ARCurrentSpread += ARSpreadPerShot;
                        if(ARCurrentSpread > _ARSpreadMax_)
                        {
                            ARCurrentSpread = _ARSpreadMax_;
                        }
                        
                        
                        byte fpsCommand = (byte)FPS_Func.Shoot_AR;
                        FPSCommand(fpsCommand, AR_Ray.origin, AR_Ray.direction);    
                        pv.RPC("FPSCommand", RpcTarget.Others, fpsCommand, AR_Ray.origin, AR_Ray.direction);
                        
                        currentARFireRate -= ARFireRateAcceleration * dt;
                        currentARFireRate = Mathf.Clamp(currentARFireRate, ARFireRateFastest, ARFireRateSlowest);
                        
                        
                        //gunTimer += fireRateMultiplier * ARFireRateMax;// * 0.5f;    
                        
                        if(hasARGhost && ARGhostTimer < ARGhostDuration - 0.25f)
                        {
                            //float _arFireRate =  arFireRate
                            currentARFireRate = ARFireRateFastest;
                            //ARGhostFireQueued = true;
                            ARGhostFires_queued_num++;
                            ARGhostFire_timer = ARGhostFire_delay;
                            
                            if(pController.IsGrounded())
                            {
                                if(Math.SqrMagnitude(pController.fpsVelocity) < 23 * 23 )
                                {
                                    pController.BoostVelocityAdditive(-AR_Ray.direction * 6.0f);
                                }
                            }
                            else
                            {
                                if(Math.SqrMagnitude(pController.fpsVelocity) < 10 * 10 || pController.fpsVelocity.y < 0)
                                {
                                    pController.BoostVelocityAdditive(-AR_Ray.direction * 4.2f);
                                }
                            }
                        }
                        float pitch_ar_t = Mathf.InverseLerp(ARFireRateSlowest, ARFireRateFastest, currentARFireRate);
                        gunAudio_pitched.pitch = Mathf.Lerp(0.4f, 1, pitch_ar_t);
                        gunTimer += currentARFireRate;
                        
                    }
                    else
                    {
                        currentARFireRate += ARFireRateAcceleration * 0.25F * dt;
                        currentARFireRate = Mathf.Clamp(currentARFireRate, ARFireRateFastest, ARFireRateSlowest);
                    }
                    if(altFire)
                    {
                        if(!hasARGhost && ARGhostCooldownTimer == 0)
                        {
                            
                            ARGhostTimer = ARGhostDuration;
                            ARGhostCooldownTimer = ARGhostCooldown;    
                            hasARGhost = true;
                            EquipARGhost();
                        }
                    }
                }
                break;
            }
            case(GunType.MP5_alt):
            {
                if(gunTimer == 0)
                {
                    if(primaryFire)
                    {
                        Ray mp5Ray = pController.GetFPSRay();
                        gunTimer += fireRateMultiplier * mp5FireRate;
                        byte fpsCommand = (byte)FPS_Func.Shoot_mp5;
                        FPSCommand(fpsCommand, mp5Ray.origin, mp5Ray.direction);
                        pv.RPC("FPSCommand", RpcTarget.Others, fpsCommand, mp5Ray.origin, mp5Ray.direction);
                        
                        // if(hasARGhost && ARGhostTimer < ARGhostDuration - 0.25f)
                        // {
                        //     //float _arFireRate =  arFireRate
                        //     currentARFireRate = ARFireRateFastest;
                        //     ARGhostFireQueued = true;
                        //     ARGhostFire_timer = ARGhostFire_delay;
                            
                        //     if(pController.IsGrounded())
                        //     {
                        //         if(Math.SqrMagnitude(pController.fpsVelocity) < 23 * 23 )
                        //         {
                        //             pController.BoostVelocityAdditive(-mp5Ray.direction * 4.0f);
                        //         }
                        //     }
                        // }
                    }
                    else
                    {
                        if(altFire && (mp5AltFireTimer == 0))
                        {
                            Vector3 _shotPos = gunPoint_mp5_grenade_fps.position;
                            Vector3 _shotDir = gunPoint_mp5_grenade_fps.forward;
                            
                            if(pController.CheckIfFPSRayObscure())
                            {
                                Ray mp5_grenade_Ray = pController.GetFPSRay();
                                _shotPos = mp5_grenade_Ray.origin;
                                _shotDir = mp5_grenade_Ray.direction;
                                _shotPos -= _shotDir * 1.0F;
                            }
                            else
                            {
                                // float dot = Vector3.Dot(Vector3.up, _shotDir);
                                // if(dot < 0.5f)
                                // {
                                //     _shotDir += Vector3.up * mp5_grenade_upMult;
                                //     _shotDir.Normalize();
                                //     InGameConsole.LogFancy(string.Format("Shooting grenade, dot: {0}, _ShotDir: {1}", dot, _shotDir));
                                // }
                            }
                            mp5AltFireTimer = mp5AltFireTimerCooldown;
                            gunTimer += fireRateMultiplier * mp5FireRate_alt;
                            
                            byte fpsCommand = (byte)FPS_Func.Shoot_mp5_grenade;
                                        
                            FPSCommand(fpsCommand, _shotPos, _shotDir);
                            pv.RPC("FPSCommand", RpcTarget.Others, fpsCommand, _shotPos, _shotDir);
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
    
    float ARCurrentSpread = 0f;
    const float ARSpreadMax = 0.07f;
    const float ARSpreadPerShot = 0.075f;
    const float ARSpreadStabilizationRate = 0.15f;
    
    
    
    void TickWeaponCooldowns(float dt)
    {
        ARGhostCooldownTimer -= dt;
        if(ARGhostCooldownTimer <= 0)
        {
            ARGhostCooldownTimer = 0;
        }
        
        ARGhostTimer -= dt;
        
        if(hasARGhost && ARGhostTimer < 0.45f)
        {
            hasARGhost = false;
            AR_ghost_animator_fps.Play("Base.Unwield");
        }
        
        if(ARGhostTimer <= 0)
        {
            if(!hasARGhost && (AR_ghost_fps != null) && AR_ghost_fps.gameObject.activeSelf)
            {
                UnequipARGhost();
            }
            ARGhostTimer = 0;
        }
        
        ARCurrentSpread -= dt * ARSpreadStabilizationRate;
        
        if(ARCurrentSpread <= 0)
        {
            ARCurrentSpread = 0;
        }
        
        mp5AltFireTimer -= dt;
        if(mp5AltFireTimer <= 0)
        {
            mp5AltFireTimer = 0;
        }
    }
    
    const float switchWeaponDuration = 0.41f;
    float GetSwitchWeaponDuration()
    {
        if(fastWeaponSwitch)
        {
            fastWeaponSwitch = false;
            //InGameConsole.LogFancy("GetSwitchWeaponDuration()GetSwitchWeaponDuration()GetSwitchWeaponDuration()");
            return 0.15f;
        }
        else
            return switchWeaponDuration;
    }
    public bool fastWeaponSwitch = false;
    
    public AudioClip switchWeaponClip;
    
    void WieldGunFPS()
    {
        //GunType gun = inventory[currentSlotPositionInInventory];
        GunType gun = GetCurrentWeaponInHands();
        //InGameConsole.LogOrange(string.Format("WieldGunFPS(): gun to wield is <color=yellow>{0}</color>", gun.ToString()));
        
        if(pv.IsMine)
        {
            CancelInvoke();
            pv.RPC("WieldGunTPS", RpcTarget.Others, (byte)gun);
        }
        
        gunAudio.PlayOneShot(switchWeaponClip, 0.3f);
        
        revolverState = RevolverState.Normal;
        //SetPlayerControllerNormalState();
        
        //ARGhostFireQueued = false;
        ARGhostFires_queued_num = 0;
        
        switch(gun)
        {
            case(GunType.None):
            {
                if(!pController.isAlive)
                {
                    return;
                }
                revolver_fps.gameObject.SetActive(false);
                shotgun_fps.gameObject.SetActive(false);
                rocketLauncher_fps.gameObject.SetActive(false);
                AR_fps.gameObject.SetActive(false);
                AR_ghost_fps.gameObject.SetActive(false);
                mp5_alt_fps.gameObject.SetActive(false);
                revolver_blue_fps.gameObject.SetActive(false);
                shotgun_blue_fps.gameObject.SetActive(false);
                
                arm_right_animator.Play("Base.Hidden", 0, 0);
                
                gunTimer = GetSwitchWeaponDuration();
                
                break;
            }
            case(GunType.Revolver):
            {
                if(!pController.isAlive)
                {
                    return;
                }
                revolver_fps.gameObject.SetActive(true);
                arm_right_animator.Play("Base.Wield", 0, 0);
                revolver_fps.anim.Play("Base.Wield", 0, 0);
                
                shotgun_fps.gameObject.SetActive(false);
                rocketLauncher_fps.gameObject.SetActive(false);
                AR_fps.gameObject.SetActive(false);
                AR_ghost_fps.gameObject.SetActive(false);
                mp5_alt_fps.gameObject.SetActive(false);
                revolver_blue_fps.gameObject.SetActive(false);
                shotgun_blue_fps.gameObject.SetActive(false);
                
                gunTimer = GetSwitchWeaponDuration();
                revolver_fps.shaking_mult = 0;
                
                break;
            }
            case(GunType.Shotgun):
            {
                if(!pController.isAlive)
                {
                    return;
                }
                revolver_fps.gameObject.SetActive(false);
                shotgun_animatorFPS.Play("Base.Wield", 0, 0);
                
                shotgun_fps.gameObject.SetActive(true);
                rocketLauncher_fps.gameObject.SetActive(false);
                AR_fps.gameObject.SetActive(false);
                AR_ghost_fps.gameObject.SetActive(false);
                mp5_alt_fps.gameObject.SetActive(false);
                revolver_blue_fps.gameObject.SetActive(false);
                shotgun_blue_fps.gameObject.SetActive(false);
                
                arm_right_animator.Play("Base.Hidden", 0, 0);
                gunTimer = GetSwitchWeaponDuration();
                
                break;
            }
            case(GunType.RocketLauncher):
            {
                if(!pController.isAlive)
                {
                    return;
                }
                revolver_fps.gameObject.SetActive(false);
                shotgun_fps.gameObject.SetActive(false);
                rocketLauncher_fps.gameObject.SetActive(true);
                AR_fps.gameObject.SetActive(false);
                AR_ghost_fps.gameObject.SetActive(false);
                mp5_alt_fps.gameObject.SetActive(false);
                revolver_blue_fps.gameObject.SetActive(false);
                shotgun_blue_fps.gameObject.SetActive(false);
                
                arm_right_animator.Play("Base.Hidden", 0, 0);
                
                gunTimer = GetSwitchWeaponDuration() / 3;
                
                
                break;
            }
            case(GunType.AR):
            {
                if(!pController.isAlive)
                {
                    return;
                }
                revolver_fps.gameObject.SetActive(false);
                shotgun_fps.gameObject.SetActive(false);
                rocketLauncher_fps.gameObject.SetActive(false);
                AR_fps.gameObject.SetActive(true);
                mp5_alt_fps.gameObject.SetActive(false);
                revolver_blue_fps.gameObject.SetActive(false);
                shotgun_blue_fps.gameObject.SetActive(false);
                
                currentARFireRate = ARFireRateFastest;
                
                arm_right_animator.Play("Base.Hidden", 0, 0);
                
                if(hasARGhost)
                {
                    AR_ghost_fps.gameObject.SetActive(true);
                    currentARFireRate = ARFireRateFastest;
                }
                
                
                gunTimer = GetSwitchWeaponDuration();
                
                break;
            }
            case(GunType.MP5_alt):
            {
                if(!pController.isAlive)
                {
                    return;
                }
                revolver_fps.gameObject.SetActive(false);
                shotgun_fps.gameObject.SetActive(false);
                rocketLauncher_fps.gameObject.SetActive(false);
                AR_fps.gameObject.SetActive(false);
                mp5_alt_fps.gameObject.SetActive(true);
                revolver_blue_fps.gameObject.SetActive(false);
                shotgun_blue_fps.gameObject.SetActive(false);
                
                arm_right_animator.Play("Base.Hidden", 0, 0);
                
                
                gunTimer = GetSwitchWeaponDuration();
                break;
            }
            case(GunType.Shotgun_blue):
            {
                if(!pController.isAlive)
                {
                    return;
                }
                revolver_fps.gameObject.SetActive(false);
                
                shotgun_animatorFPS.Play("Base.Wield", 0, 0);
                
                shotgun_fps.gameObject.SetActive(false);
                rocketLauncher_fps.gameObject.SetActive(false);
                AR_fps.gameObject.SetActive(false);
                AR_ghost_fps.gameObject.SetActive(false);
                mp5_alt_fps.gameObject.SetActive(false);
                revolver_blue_fps.gameObject.SetActive(false);
                shotgun_blue_fps.gameObject.SetActive(true);
                
                arm_right_animator.Play("Base.Hidden", 0, 0);
                gunTimer = GetSwitchWeaponDuration();
                
                break;
            }
            case(GunType.Revolver_blue):
            {
                if(!pController.isAlive)
                {
                    return;
                }
                revolver_fps.gameObject.SetActive(false);
                arm_right_animator.Play("Base.Wield", 0, 0);
                revolver_blue_fps.anim.Play("Base.Wield", 0, 0);
                
                shotgun_fps.gameObject.SetActive(false);
                rocketLauncher_fps.gameObject.SetActive(false);
                AR_fps.gameObject.SetActive(false);
                AR_ghost_fps.gameObject.SetActive(false);
                mp5_alt_fps.gameObject.SetActive(false);
                revolver_blue_fps.gameObject.SetActive(true);
                shotgun_blue_fps.gameObject.SetActive(false);
                
                gunTimer = GetSwitchWeaponDuration();
                revolver_blue_fps.shaking_mult = 0;
                
                break;
            }
        }
    }
    
    [PunRPC]
    void WieldGunTPS(byte _gun)
    {
        GunType gun = (GunType)_gun;
        switch(gun)
        {
            case(GunType.None):
            {
                revolver_tps.gameObject.SetActive(false);
                shotgun_tps.gameObject.SetActive(false);
                rocketLauncher_tps.gameObject.SetActive(false);
                AR_tps.gameObject.SetActive(false);
                mp5_alt_tps.gameObject.SetActive(false);
                revolver_blue_tps.gameObject.SetActive(false);
                shotgun_blue_tps.gameObject.SetActive(false);
                                
                break;
            }
            case(GunType.Revolver):
            {
                revolver_tps.gameObject.SetActive(true);
                shotgun_tps.gameObject.SetActive(false);
                rocketLauncher_tps.gameObject.SetActive(false);
                AR_tps.gameObject.SetActive(false);
                mp5_alt_tps.gameObject.SetActive(false);
                revolver_blue_tps.gameObject.SetActive(false);
                shotgun_blue_tps.gameObject.SetActive(false);
                
                break;
            }
            case(GunType.Shotgun):
            {
                revolver_tps.gameObject.SetActive(false);
                shotgun_tps.gameObject.SetActive(true);
                rocketLauncher_tps.gameObject.SetActive(false);
                AR_tps.gameObject.SetActive(false);
                mp5_alt_tps.gameObject.SetActive(false);
                revolver_blue_tps.gameObject.SetActive(false);
                shotgun_blue_tps.gameObject.SetActive(false);
                
                break;
            }
            case(GunType.RocketLauncher):
            {
                revolver_tps.gameObject.SetActive(false);
                shotgun_tps.gameObject.SetActive(false);
                rocketLauncher_tps.gameObject.SetActive(true);
                AR_tps.gameObject.SetActive(false);
                mp5_alt_tps.gameObject.SetActive(false);   
                revolver_blue_tps.gameObject.SetActive(false);
                shotgun_blue_tps.gameObject.SetActive(false);
                
                break;
            }
            case(GunType.AR):
            {
                revolver_tps.gameObject.SetActive(false);
                shotgun_tps.gameObject.SetActive(false);
                rocketLauncher_tps.gameObject.SetActive(false);
                AR_tps.gameObject.SetActive(true);
                mp5_alt_tps.gameObject.SetActive(false);
                revolver_blue_tps.gameObject.SetActive(false);
                shotgun_blue_tps.gameObject.SetActive(false);
                   
                break;
            }
            case(GunType.MP5_alt):
            {
                revolver_tps.gameObject.SetActive(false);
                shotgun_tps.gameObject.SetActive(false);
                rocketLauncher_tps.gameObject.SetActive(false);
                AR_tps.gameObject.SetActive(false);
                mp5_alt_tps.gameObject.SetActive(true);
                revolver_blue_tps.gameObject.SetActive(false);
                shotgun_blue_tps.gameObject.SetActive(false);
                
                break;
            }
            case(GunType.Revolver_blue):
            {
                revolver_tps.gameObject.SetActive(false);
                shotgun_tps.gameObject.SetActive(false);
                rocketLauncher_tps.gameObject.SetActive(false);
                AR_tps.gameObject.SetActive(false);
                mp5_alt_tps.gameObject.SetActive(false);
                revolver_blue_tps.gameObject.SetActive(true);
                shotgun_blue_tps.gameObject.SetActive(false);
                
                break;
            }
            case(GunType.Shotgun_blue):
            {
                revolver_tps.gameObject.SetActive(false);
                shotgun_tps.gameObject.SetActive(false);
                rocketLauncher_tps.gameObject.SetActive(false);
                AR_tps.gameObject.SetActive(false);
                mp5_alt_tps.gameObject.SetActive(false);
                revolver_blue_tps.gameObject.SetActive(false);
                shotgun_blue_tps.gameObject.SetActive(true);
                
                break;
            }
            
        }
    }
    
  
    bool FireButton()
    {
        return Input.GetMouseButtonDown(0);
    }
    
    bool AltFireButton()
    {
        return Input.GetMouseButtonDown(1);
    }
    
    //Revolver:
    public AudioClip revolverShotClip;
    public AudioClip revolverShot_strongerAdditionalClip;
    //Shotgun:
    public AudioClip shotgunShotClip;
    public AudioClip shotgunAltShotClip;
    public AudioClip shotgunBlueShotClip;
    //RocketLauncher:
    public AudioClip rocketLauncherShotClip;
    //AR:
    public AudioClip arShotClip;
    
    Vector3[] revolverShotPositions = new Vector3[2];
    
    void OnGUI()
    {
        return;
        if(pv.IsMine)
        {
            GUIStyle style = new GUIStyle();
            style.alignment = TextAnchor.MiddleCenter;
            
            float w = 350;
            float h = 50;
            
            style.normal.textColor = Color.green;
            GUI.Label(new Rect(Screen.width/2 - w/2, Screen.height - h*2.9f, w, h), "currentSlotPos: [<color=yellow>" + currentSlotPositionInInventory.mainIndex.ToString() + "</color>,<color=red>" + currentSlotPositionInInventory.altIndex.ToString() + "</color>]", style);
            GUI.Label(new Rect(Screen.width/2 - w/2, Screen.height - h*2.6f, w, h), "prevSlotPos: [<color=yellow>" + prevSlotPositionInInventory.mainIndex.ToString() + "</color>,<color=red>" + prevSlotPositionInInventory.altIndex.ToString() + "</color>]", style);
        }
       
    }
    
    
    bool arm1_overcharged = false;
    const float arm1_overcharge_duration = 2;
    float arm1_overcharge_timer = 0;
    
    [PunRPC]
    void FPSCommand(byte func_byte)
    {
        FPS_Func func = (FPS_Func)func_byte;
        
        switch(func)
        {
            case(FPS_Func.Punch1):
            {
                if(!pv.IsMine)
                {
                    pController.debugAvatarAnim.SetTrigger("DoAttack");
                }
                Punch();
                break;
            }
            case(FPS_Func.ArmLeft_Pet):
            {
                if(!pv.IsMine)
                {
                    pController.debugAvatarAnim.SetTrigger("DoAttack");
                }
                PetWithLeftArm();
                break;
            }
            default:
            {
                InGameConsole.LogWarning(string.Format("FPSCommand(): {0} is <color=red>not</color> handled", func));
                break;
            }
        }
    }
    
    float crosshairTrauma = 1;
    
    [PunRPC]
    void FPSCommand(byte func_byte, Vector3 pos, Vector3 dir)
    {
        FPS_Func func = (FPS_Func)func_byte;
        
        switch(func)
        {
            case(FPS_Func.Shoot_revolver):
            {
                if(pv.IsMine)
                {
                    if(GetCurrentWeapon() != GunType.Revolver && pController.isAlive)
                        return;
                    
                    CrosshairController.MakeTrauma(crosshairTrauma);
                }
                
                ShootRevolver(pos, dir);
                break;
            }
            case(FPS_Func.Shoot_revolver_stronger):
            {
                if(pv.IsMine)
                {
                    if(GetCurrentWeapon() != GunType.Revolver && pController.isAlive)
                        return;
                    if(pController.IsGrounded())
                    {
                        pController.velocity.y = 0;
                    }
                    pController.BoostVelocityAdditive(-dir * 6f);
                    CrosshairController.MakeTrauma(crosshairTrauma);
                }
                
                ShootRevolver_Stronger(pos, dir);
                break;
            }
            case(FPS_Func.Shoot_revolver_ult):
            {
                if(pv.IsMine)
                {
                    if(GetCurrentWeapon() != GunType.Revolver && pController.isAlive)
                        return;
                        
                    CrosshairController.MakeTrauma(crosshairTrauma);
                }
                
                ShootRevolver_Ult(pos, dir);
                break;
            }
            case(FPS_Func.Shoot_shotgun_alt):
            {
                if(pv.IsMine)
                {
                    if(GetCurrentWeapon() != GunType.Shotgun && pController.isAlive)
                        return;
                        
                    CrosshairController.MakeTrauma(crosshairTrauma);
                }
                
                ShootShotgun_Alt2(pos, dir);
                break;
            }
            case(FPS_Func.Hook_shotgun_blue):
            {
                if(pv.IsMine)
                {
                    if(GetCurrentWeapon() != GunType.Shotgun_blue && pController.isAlive)
                        return;
                }
                
                hook.LaunchHook(pController, dir, pv.IsMine);
                
                break;
            }
            case(FPS_Func.Shoot_AR):
            {
                if(pv.IsMine)
                {
                    if(GetCurrentWeapon() != GunType.AR && pController.isAlive)
                        return;
                        
                    CrosshairController.MakeTrauma(crosshairTrauma);
                }
                //ShootAR(pos, dir);
                ShootAR_HitScan(pos, dir);
                break;
            }
            case(FPS_Func.Shoot_AR_Ghost):
            {
                if(pv.IsMine)
                {
                    if(GetCurrentWeapon() != GunType.AR && GetCurrentWeapon() != GunType.MP5_alt && pController.isAlive)
                        return;
                        
                    CrosshairController.MakeTrauma(crosshairTrauma);
                }
                //ShootARGhost(pos, dir);
                ShootARGhost_HitScan(pos, dir);
                break;
            }
            case(FPS_Func.Shoot_mp5):
            {
                if(pv.IsMine)
                {
                    if(GetCurrentWeapon() != GunType.MP5_alt && pController.isAlive)
                        return;
                        
                    CrosshairController.MakeTrauma(crosshairTrauma);
                }
                ShootMP5_HitScan(pos, dir);
                break;
            }
            case(FPS_Func.Shoot_mp5_grenade):
            {
                if(pv.IsMine)
                {
                    if(GetCurrentWeapon() != GunType.MP5_alt && pController.isAlive)
                        return;
                        
                    CrosshairController.MakeTrauma(crosshairTrauma);
                }
                ShootMP5_alt(pos, dir);
                break;
            }
            case(FPS_Func.Shoot_rocketLauncher):
            {
                if(pv.IsMine)
                {
                    if(GetCurrentWeapon() != GunType.RocketLauncher && pController.isAlive)
                        return;
                        
                    CrosshairController.MakeTrauma(crosshairTrauma);
                }
                ShootRocketLauncher(pos, dir);
                break;
            }
            case(FPS_Func.Shoot_revolver_blue):
            {
                if(pv.IsMine)
                {
                    if(GetCurrentWeapon() != GunType.Revolver_blue && pController.isAlive)
                        return;
                    
                    CrosshairController.MakeTrauma(crosshairTrauma);
                }
                
                ShootRevolver_Blue(pos, dir);
                break;
            }
            case(FPS_Func.Shoot_revolver_blue_stronger):
            {
                if(pv.IsMine)
                {
                    if(GetCurrentWeapon() != GunType.Revolver_blue && pController.isAlive)
                        return;
                    if(pController.IsGrounded())
                    {
                        pController.velocity.y = 0;
                    }
                    pController.BoostVelocityAdditive(-dir * 6f);
                    CrosshairController.MakeTrauma(crosshairTrauma);
                }
                
                ShootRevolver_Stronger_Blue(pos, dir);
                break;
            }
            case(FPS_Func.Shoot_revolver_blue_ult):
            {
                if(pv.IsMine)
                {
                    if(GetCurrentWeapon() != GunType.Revolver_blue && pController.isAlive)
                        return;
                        
                    CrosshairController.MakeTrauma(crosshairTrauma);
                }
                
                ShootRevolver_Ult_blue(pos, dir);
                break;
            }
            
            default:
            {
                InGameConsole.LogWarning(string.Format("FPSCommand(): {0} is <color=red>not</color> handled", func));
                break;
            }
        }
    }
    
    [PunRPC]
    void FPSC3(byte func_byte, Vector3 pos, Vector3 dir, Vector3 upDir)
    {
        FPS_Func func = (FPS_Func)func_byte;
        
        switch(func)
        {
            case(FPS_Func.Shoot_rocketLauncher_alt):
            {
                if(pv.IsMine)
                {
                    if(GetCurrentWeapon() != GunType.RocketLauncher && pController.isAlive)
                        return;
                    CrosshairController.MakeTrauma(crosshairTrauma);
                }
                
                ShootRocketLauncher_Alt(pos, dir, upDir);
                break;
            }
            default:
            {
                break;
            }
        }
    }
    
    [PunRPC]
    void FPSCommand2(byte func_byte, Vector3 pos, Vector3 dir, byte seed)
    {
        FPS_Func func = (FPS_Func)func_byte;
        
        //InGameConsole.LogFancy(string.Format("{0}, {1}, {2}, {3}", func, pos, dir, seed));
        
        switch(func)
        {
            case(FPS_Func.Shoot_shotgun):
            {
                //ShootShotgun(pos, dir, seed);
                if(pv.IsMine)
                {
                    if(GetCurrentWeapon() != GunType.Shotgun && pController.isAlive)
                        return;
                    CrosshairController.MakeTrauma(crosshairTrauma);
                }
                
                ShootShotgun_HitScan(pos, dir, seed);
         //       InGameConsole.LogFancy(string.Format("{0}", "ShootShotgun()"));
                break;
            }
            case(FPS_Func.Shoot_shotgun_alt):
            {
                if(pv.IsMine)
                {
                    if(GetCurrentWeapon() != GunType.Shotgun && pController.isAlive)
                        return;
                    CrosshairController.MakeTrauma(crosshairTrauma);
                }
                
                ShootShotgun_Alt(pos, dir, seed);
                break;
            }
            case(FPS_Func.Shoot_shotgun_blue):
            {
                //ShootShotgun(pos, dir, seed);
                if(pv.IsMine)
                {
                    if(GetCurrentWeapon() != GunType.Shotgun_blue && pController.isAlive)
                        return;
                    CrosshairController.MakeTrauma(crosshairTrauma);
                }
                
                //ShootShotgun_HitScan(pos, dir, seed);
                ShootShotgun_blue_HitScan(pos, dir, seed);
         //       InGameConsole.LogFancy(string.Format("{0}", "ShootShotgun()"));
                break;
            }
            default:
            {
                InGameConsole.LogWarning(string.Format("FPSCommand(): {0} is <color=red>not</color> handled", func));
                break;
            }
        }
    }
    
    RaycastHit[] hits = new RaycastHit[64];
    HashSet<int> hits_scanned = new HashSet<int>();
    
    void DoLightRevolverUlt(Vector3 pos)
    {
        GameObject g = ObjectPool2.s().Get(ObjectPoolKey.LightPooled, false);
        LightPooled light = g.GetComponent<LightPooled>();
        Color color = new Color(1f, 0.1f, 0f, 1f);
        
        float decay_speed = 6 / 0.5F * 1.33F;
        light.DoLight(pos, color, 0.5f, 16, 6, decay_speed);
    }
    
    void ShootRevolver_Ult(Vector3 shotPos, Vector3 hitScanDirection)
    {
        gunAudio2.PlayOneShot(revolverShotClip, 1);
        
        Ray ray = new Ray(shotPos, hitScanDirection);
        
        int ult_dmg = revolverDmg_ult;
        
        if(pv.IsMine)
        {
            CameraShaker.MakeTrauma(0.6f);
            revolver_fps.anim.Play("Base.Fire_ult", 0, 0);
            arm_right_animator.Play("Base.Fire_ult", 0, 0);
            if(CurrentArmAnimator())
            {
                CurrentArmAnimator().Play("Base.UltToEnd", 0, 0);
            }
            revolverFX_stronger_ps.Play();
            
            pController.BoostVelocity(-ray.direction * 24.0F);
            revolverFX_ps.Play();
            
            if(Math.Abs(pController.velocity.y) > 2f)
            {
                ult_dmg = revolverDmg_ult + revolverDmg_ult / 4;
            }
        }
        RaycastHit hit;
        
        float revolverShotMaxDistance = 200F;
        
        Vector3 lineStart = pv.IsMine ? gunPoint_revolver_fps.position : gunPoint_revolver_tps.position;
        Vector3 lineEnd = lineStart + ray.direction * revolverShotMaxDistance;
        
        if(Physics.Raycast(ray, out hit, revolverShotMaxDistance, staticObjectsMask))
        {
            lineEnd = hit.point;
            
            DoLightRevolverUlt(lineEnd - ray.direction * 0.1f);
            
            revolverShotMaxDistance = hit.distance;
            OnHitScan(hit.point, hitScanDirection, hit.normal, revolverDmg_reflect, hit.collider);
        }
        else
        {
            OnHitScan(shotPos + hitScanDirection * revolverShotMaxDistance, hitScanDirection, -hitScanDirection, revolverDmg_reflect, null);
        }
        
        hits_scanned.Clear();
        
        int hitsNum = Physics.RaycastNonAlloc(ray, hits, revolverShotMaxDistance, bulletMask);
        
        hitsNum = Mathf.Min(hitsNum, hits.Length);
        
        
        for(int i = 0; i < hitsNum; i++)
        {
            DamagableLimb hit_limb = hits[i].collider.GetComponent<DamagableLimb>();
            if(hit_limb)
            {
                int limb_net_id = hit_limb.net_comp_from_parent.networkId;
                if(!hits_scanned.Contains(limb_net_id))
                {
                    hits_scanned.Add(limb_net_id);
                    
                    OnHitScan(hits[i].point, hitScanDirection, hit.normal, ult_dmg, hits[i].collider, null, 2.6f, 1, 900);
                }
            }
        }
        
        ParticlesManager.PlayPooled(ParticleType.shot_star_ps, lineStart, ray.direction);
        GameObject bulletFX = ObjectPool.s().Get(ObjectPoolKey.Revolver_bullet_ult);
        bulletFX.GetComponent<BulletControllerHurtless>().Launch2(lineStart, lineEnd);
    }
    
    void ShootRevolver_Ult_blue(Vector3 shotPos, Vector3 hitScanDirection)
    {
        if(pv.IsMine)
        {
            CameraShaker.MakeTrauma(0.9f);
            revolver_blue_fps.anim.Play("Base.Fire_ult", 0, 0);
            arm_right_animator.Play("Base.Fire_blue_ult", 0, 0);
            if(CurrentArmAnimator())
            {
                CurrentArmAnimator().Play("Base.UltToEnd", 0, 0);
            }
            revolverFX_blue_stronger_ps.Play();
            
            //pController.BoostVelocity(-ray.direction * 24.0F);
            //revolverFX_ps.Play();
            
            
        }
        
        GameObject obj = ObjectPool.s().Get(ObjectPoolKey.Kaboom1, false);
            
        bool isMine = pv.IsMine;
        obj.GetComponent<Kaboom1>().ExplodeDamageHostile(shotPos, 6, 38, 800, isMine, true, 15);
    }
    
    void ShootRevolver_Stronger(Vector3 shotPos, Vector3 hitScanDirection)
    {
        if(playerGunLight)
        {
            playerGunLight.ShootRevolver();
        }
        
        gunAudio.PlayOneShot(revolverShotClip, 0.5f);
        gunAudio.PlayOneShot(revolverShot_strongerAdditionalClip, 1f);
        
        if(pv.IsMine)
        {
            CameraShaker.MakeTrauma(0.1f);
            if(revolver_fps && revolver_fps.anim)
                revolver_fps.anim.Play("Base.Fire1", 0, 0);
            revolverFX_stronger_ps.Play();
            if(arm_right_animator)
                arm_right_animator.Play("Base.Fire1", 0, 0);
            revolverFX_ps.Play();
        }
                
        //RaycastHit hit;
        Ray ray = new Ray(shotPos, hitScanDirection);
        
        //float revolverShotMaxDistance = 200f;
        
        //Vector3 lineStart = pv.IsMine ? gunPoint_revolver_fps.position : gunPoint_revolver_tps.position;
        //Vector3 lineEnd = lineStart + ray.direction * revolverShotMaxDistance;
        bool isBulletMine = pv.IsMine;
        
        GameObject bullet = ObjectPool.s().Get(ObjectPoolKey.Revolver_bullet_reflective);    
        BulletController bulletController = bullet.GetComponent<BulletController>();
        bulletController.max_reflects = 6;
        
        
        bulletController.LaunchAsSphere(ray.origin, ray.direction, 0.005F, bulletMask, 90F, revolverDmg_reflect, isBulletMine);
        bulletController.time_to_be_alive = 6F;
       
    }
    
    void ShootRevolver(Vector3 shotPos, Vector3 hitScanDirection)
    {
        if(playerGunLight)
        {
            playerGunLight.ShootRevolver();
        }
        if(gunAudio)
        {
            gunAudio.PlayOneShot(revolverShotClip, 1);
        }
        
        if(pv.IsMine)
        {
            CameraShaker.MakeTrauma(0.15f);
            revolver_fps.anim.Play("Base.Fire1", 0, 0);
            arm_right_animator.Play("Base.Fire1", 0, 0);
            revolverFX_ps.Play();
        }
                
        RaycastHit hit;
        Ray ray = new Ray(shotPos, hitScanDirection);
        
        float revolverShotMaxDistance = 200f;
        
        Vector3 lineStart = pv.IsMine ? gunPoint_revolver_fps.position : gunPoint_revolver_tps.position;
        Vector3 lineEnd = lineStart + ray.direction * revolverShotMaxDistance;
        
        if(Physics.Raycast(ray, out hit, revolverShotMaxDistance, bulletMask))
        {
            lineEnd = hit.point;
            OnHitScan(hit.point, hitScanDirection, hit.normal, revolverDmg, hit.collider, null, 2.5f, 1, 650);
        }
        else
        {
            OnHitScan(shotPos + hitScanDirection * revolverShotMaxDistance, hitScanDirection, -hitScanDirection, revolverDmg, null);
        }
        
        GameObject bulletFX = ObjectPool.s().Get(ObjectPoolKey.RevolverBullet);
        bulletFX.GetComponent<BulletControllerHurtless>().Launch2(lineStart, lineEnd);
        //ParticlesManager.PlayPooled(ParticleType.shot_star_ps, lineStart, ray.direction);
    }
    
    void ShootRevolver_Blue(Vector3 shotPos, Vector3 hitScanDirection)
    {
        if(playerGunLight)
        {
            playerGunLight.ShootRevolver();
        }
        if(gunAudio)
        {
            gunAudio.PlayOneShot(revolverShotClip, 1);
        }
        
        if(pv.IsMine)
        {
            CameraShaker.MakeTrauma(0.15f);
            revolver_blue_fps.anim.Play("Base.Fire1", 0, 0);
            arm_right_animator.Play("Base.Fire1", 0, 0);
            revolverFX_blue_ps.Play();
        }
                
        RaycastHit hit;
        Ray ray = new Ray(shotPos, hitScanDirection);
        
        float revolverShotMaxDistance = 200f;
        
        Vector3 lineStart = pv.IsMine ? gunPoint_revolver_blue_fps.position : gunPoint_revolver_tps.position;
        Vector3 lineEnd = lineStart + ray.direction * revolverShotMaxDistance;
        
        if(Physics.Raycast(ray, out hit, revolverShotMaxDistance, bulletMask))
        {
            lineEnd = hit.point;
            OnHitScan(hit.point, hitScanDirection, hit.normal, revolverDmg, hit.collider, null, 2.5f, 1, 650);
        }
        else
        {
            OnHitScan(shotPos + hitScanDirection * revolverShotMaxDistance, hitScanDirection, -hitScanDirection, revolverDmg, null);
        }
        
        GameObject bulletFX = ObjectPool.s().Get(ObjectPoolKey.RevolverBullet);
        bulletFX.GetComponent<BulletControllerHurtless>().Launch2(lineStart, lineEnd);
        //ParticlesManager.PlayPooled(ParticleType.shot_star_ps, lineStart, ray.direction);
    }
    
    void ShootRevolver_Stronger_Blue(Vector3 shotPos, Vector3 hitScanDirection)
    {
        if(playerGunLight)
        {
            playerGunLight.ShootRevolver();
        }
        if(gunAudio)
        {
            gunAudio.PlayOneShot(revolverShotClip, 1);
        }
        
        if(pv.IsMine && pController.isAlive)
        {
            CameraShaker.MakeTrauma(0.15f);
            revolver_blue_fps.anim.Play("Base.Fire2", 0, 0);
            arm_right_animator.Play("Base.Fire2", 0, 0);
            revolverFX_blue_ps.Play();
        }
                
        RaycastHit hit;
        Ray ray = new Ray(shotPos, hitScanDirection);
        
        float revolverShotMaxDistance = 200f;
        
        Vector3 lineStart = pv.IsMine ? gunPoint_revolver_blue_fps.position : gunPoint_revolver_tps.position;
        Vector3 lineEnd = lineStart + ray.direction * revolverShotMaxDistance;
        
        if(Physics.Raycast(ray, out hit, revolverShotMaxDistance, bulletMask))
        {
            lineEnd = hit.point;
            OnHitScanBounce(hit.point, hitScanDirection, hit.normal, revolverDmg * 2, hit.collider, null, 2.5f, 2, 1650);
        }
        else
        {
            OnHitScan(shotPos + hitScanDirection * revolverShotMaxDistance, hitScanDirection, -hitScanDirection, revolverDmg, null);
        }
        
        ParticlesManager.PlayPooled(ParticleType.shot_star_ps, lineStart, ray.direction);
        GameObject bulletFX = ObjectPool.s().Get(ObjectPoolKey.Revolver_bullet_ult);
        bulletFX.GetComponent<BulletControllerHurtless>().Launch2(lineStart, lineEnd);
        //ParticlesManager.PlayPooled(ParticleType.shot_star_ps, lineStart, ray.direction);
    }
    
    
    
    const float shotgunSpread = 0.25F;
    // const float shotgunSpread_Alt = 0.275F * 2F;
    const float shotgunSpread_Alt = 0.275F * 0.175F * 0.175F;
    
    const int shotgunHits_len = 16;
    //Collider[] shotgunHits = new Collider[shotgunHits_len];
    //int shotgunHitsNum = 0;
    Dictionary<int, ShotgunHit> shotgunHits_netIDs_and_dmg = new Dictionary<int, ShotgunHit>(64);
    Vector3 shotgunLastHitPoint;
    Vector3 shotgunLastHitDir;
    Vector3 shotgunLastHitNormal;
    
    static readonly Vector3 vForward = new Vector3(0, 0, 1);
    
    void ShootShotgun_HitScan(Vector3 shotPos, Vector3 hitScanDirection, byte seed)
    {
        if(playerGunLight)
        {
            playerGunLight.ShootShotgun();
        }
        if(gunAudio)
        {
            gunAudio.PlayOneShot(shotgunShotClip, 1);
        }
        
        if(pv.IsMine)
        {
            CameraShaker.MakeTrauma(0.5f);
        
            shotgun_animatorFPS.Play("Base.Fire1", 0, 0);
            shotgunFX_ps.Play();
            
            if(pController.IsGrounded())
            {
                
                pController.BoostVelocityAdditive(-hitScanDirection * 17.5f);
            }
            else
            {
                if(pController.fpsVelocity.y < 0 && (Vector3.Dot(Vector3.down, hitScanDirection) > 0.9f))
                    pController.fpsVelocity.y = 0;
                pController.BoostVelocityAdditive(-hitScanDirection * 9.5f);
            }
        }
        
        shotgunHits_netIDs_and_dmg.Clear();
        
        ParticlesManager.PlayPooled(ParticleType.shot_explosion1, shotPos + hitScanDirection, hitScanDirection);
        
        Random.InitState(seed);
        GaussianDistribution gd = new GaussianDistribution();
        
        Quaternion rotation = Quaternion.LookRotation(hitScanDirection);
        
        RaycastHit hit;
        Ray ray = new Ray(shotPos, hitScanDirection);
        
        
        for(int i = 0; i < shotgunPelletCount; i++)
        {
            Vector3 randomPoint = new Vector3(gd.Next(0f, 1f, -1f, 1f), gd.Next(0f, 1f, -1f, 1f), 0);
            
            Vector3 pelletDir = hitScanDirection + rotation * randomPoint * shotgunSpread;
            ray.direction = Math.Normalized(pelletDir);
            
            
            float pellet_speed = 110f;
            float ray_max_distance = 60f;
            
            
            GameObject bullet = ObjectPool.s().Get(ObjectPoolKey.ShotgunPellet);    
            BulletController bulletController = bullet.GetComponent<BulletController>();
            bulletController.LaunchAsSphere(ray.origin, ray.direction, 0.1f, bulletMask, pellet_speed, shotgunDmgPellet, false);
            bulletController.time_to_be_alive = ray_max_distance / pellet_speed;
            bulletController.on_die_behave = BulletOnDieBehaviour.Hurtless;
            
            
            if(Physics.Raycast(ray, out hit, ray_max_distance, bulletMask))
            {
                bulletController.time_to_be_alive = pellet_speed * hit.distance;// / 0.5f;
                //lineEnd = hit.point;
                DamagableLimb hit_limb = hit.collider.GetComponent<DamagableLimb>();
                if(hit_limb)
                {
                    
                    int hit_netId = hit_limb.net_comp_from_parent.networkId;
                    int rnd = Random.Range(0, 10);
                    
                    if(rnd > 7)
                        hit_limb.React(hit_limb.transform.localPosition, vForward);
                    
                    if(shotgunHits_netIDs_and_dmg.ContainsKey(hit_netId))
                    {
                        ShotgunHit x = new ShotgunHit();
                        
                        x.col = shotgunHits_netIDs_and_dmg[hit_netId].col;
                        x.damage = shotgunHits_netIDs_and_dmg[hit_netId].damage;
                        x.damage += shotgunDmgPellet;
                        
                        shotgunHits_netIDs_and_dmg[hit_netId] = x;
                    }
                    else
                    {
                        ShotgunHit x = new ShotgunHit();
                        x.col = hit.collider;
                        x.damage = shotgunDmgPellet;
                        
                        shotgunHits_netIDs_and_dmg.Add(hit_netId, x);
                        
                        shotgunLastHitPoint = hit.point;
                        shotgunLastHitDir = hitScanDirection;
                        shotgunLastHitNormal = hit.normal;
                    }
                }
                else
                {
                    OnHitScan(hit.point, hitScanDirection, hit.normal, shotgunDmgPellet, hit.collider, null, 1, 0.1f);
                }
            }
            else
            {
                OnHitScan(shotPos + hitScanDirection * 60, hitScanDirection, -hitScanDirection, shotgunDmgPellet, null);
            }
        }
        
        foreach(KeyValuePair<int, ShotgunHit> hit_pair in shotgunHits_netIDs_and_dmg)
        {
            InGameConsole.LogFancy(string.Format("Doing {0} damage to {1}", hit_pair.Value.damage, hit_pair.Value.col.name));
            OnHitExplosive(shotgunLastHitPoint, shotgunLastHitDir, shotgunLastHitNormal, hit_pair.Value.damage, hit_pair.Value.col, null, 1, 1f);
        }
    }
    
    public void OnBlueHookReturned()
    {
        if(pv.IsMine)
        {
            shotgun_blue_animatorFPS.Play("Base.Fire_alt", 0, 0);
        }
    }
    
    void ShootShotgun_blue_HitScan(Vector3 shotPos, Vector3 hitScanDirection, byte seed)
    {
        if(playerGunLight)
        {
            playerGunLight.ShootShotgun();
        }
        if(gunAudio)
        {
            gunAudio.PlayOneShot(shotgunBlueShotClip, 1);
        }
        
        if(pv.IsMine)
        {
            CameraShaker.MakeTrauma(0.25f);
        
            shotgun_blue_animatorFPS.Play("Base.Fire1", 0, 0);
            shotgunFX_blue_ps.Play();
            
            if(pController.IsGrounded())
            {
                pController.BoostVelocityAdditive(-hitScanDirection * 12.5f);
            }
            else
            {
                pController.BoostVelocityAdditive(-hitScanDirection * 4.5f);
                if(pController.fpsVelocity.y < 0  && (Vector3.Dot(Vector3.down, hitScanDirection) > 0.9f))
                    pController.fpsVelocity.y = 0;
            }
        }
        
        shotgunHits_netIDs_and_dmg.Clear();
        
        ParticlesManager.PlayPooled(ParticleType.shot_explosion1, shotPos + hitScanDirection, hitScanDirection);
        
        Random.InitState(seed);
        GaussianDistribution gd = new GaussianDistribution();
        
        Quaternion rotation = Quaternion.LookRotation(hitScanDirection);
        
        RaycastHit hit;
        Ray ray = new Ray(shotPos, hitScanDirection);
        
        
        for(int i = 0; i < shotgunBluePelletCount; i++)
        {
            Vector3 randomPoint = new Vector3(gd.Next(0f, 1f, -1f, 1f), gd.Next(0f, 1f, -1f, 1f), 0);
            
            Vector3 pelletDir = hitScanDirection + rotation * randomPoint * shotgunSpread;
            ray.direction = Math.Normalized(pelletDir);
            
            
            float pellet_speed = 60f;
            float ray_max_distance = 60f;
            
            
            GameObject bullet = ObjectPool.s().Get(ObjectPoolKey.ShotgunPellet);    
            BulletController bulletController = bullet.GetComponent<BulletController>();
            bulletController.LaunchAsSphere(ray.origin, ray.direction, 0.1f, bulletMask, pellet_speed, shotgunDmgPellet, false);
            bulletController.time_to_be_alive = ray_max_distance / pellet_speed;
            bulletController.on_die_behave = BulletOnDieBehaviour.Hurtless;
            
            
            if(Physics.Raycast(ray, out hit, ray_max_distance, bulletMask))
            {
                bulletController.time_to_be_alive = pellet_speed * hit.distance;// / 0.5f;
                //lineEnd = hit.point;
                DamagableLimb hit_limb = hit.collider.GetComponent<DamagableLimb>();
                if(hit_limb)
                {
                    
                    int hit_netId = hit_limb.net_comp_from_parent.networkId;
                    int rnd = Random.Range(0, 10);
                    
                    if(rnd > 7)
                        hit_limb.React(hit_limb.transform.localPosition, vForward);
                    
                    if(shotgunHits_netIDs_and_dmg.ContainsKey(hit_netId))
                    {
                        ShotgunHit x = new ShotgunHit();
                        
                        x.col = shotgunHits_netIDs_and_dmg[hit_netId].col;
                        x.damage = shotgunHits_netIDs_and_dmg[hit_netId].damage;
                        x.damage += shotgunDmgPellet;
                        
                        shotgunHits_netIDs_and_dmg[hit_netId] = x;
                    }
                    else
                    {
                        ShotgunHit x = new ShotgunHit();
                        x.col = hit.collider;
                        x.damage = shotgunDmgPellet;
                        
                        shotgunHits_netIDs_and_dmg.Add(hit_netId, x);
                        
                        shotgunLastHitPoint = hit.point;
                        shotgunLastHitDir = hitScanDirection;
                        shotgunLastHitNormal = hit.normal;
                    }
                }
                else
                {
                    OnHitScan(hit.point, hitScanDirection, hit.normal, shotgunDmgPellet, hit.collider, null, 1, 0.1f);
                }
            }
            else
            {
                OnHitScan(shotPos + hitScanDirection * 60, hitScanDirection, -hitScanDirection, shotgunDmgPellet, null);
            }
        }
        
        foreach(KeyValuePair<int, ShotgunHit> hit_pair in shotgunHits_netIDs_and_dmg)
        {
            //InGameConsole.LogFancy(string.Format("Doing {0} damage to {1}", hit_pair.Value.damage, hit_pair.Value.col.name));
            OnHitExplosive(shotgunLastHitPoint, shotgunLastHitDir, shotgunLastHitNormal, hit_pair.Value.damage, hit_pair.Value.col, null, 1, 1f);
        }
    }
    
    void ShootShotgun(Vector3 shotPos, Vector3 dir, byte seed)
    {
        if(playerGunLight)
        {
            playerGunLight.ShootShotgun();
        }
        if(gunAudio)
        {
            gunAudio.PlayOneShot(shotgunShotClip, 1);
        }
        
        if(pv.IsMine)
        {
            CameraShaker.MakeTrauma(0.55f);
        
            shotgun_animatorFPS.Play("Base.Fire1", 0, 0);
            shotgunFX_ps.Play();
            
            
        }
        ParticlesManager.PlayPooled(ParticleType.shot_explosion1, shotPos + dir, dir);
                
        Ray ray = new Ray(shotPos, dir);
        
        Random.InitState(seed);
        GaussianDistribution gd = new GaussianDistribution();
        
        Quaternion rotation = Quaternion.LookRotation(dir);
        
        bool isBulletMine = pv.IsMine;
        
        for(int i = 0; i < shotgunPelletCount; i++)
        {
            Vector3 randomPoint = new Vector3(gd.Next(0f, 1f, -1f, 1f), gd.Next(0f, 1f, -1f, 1f), 0);
            
            Vector3 pelletDir = dir + rotation * randomPoint * shotgunSpread;
            ray.direction = Math.Normalized(pelletDir);
           
            
            GameObject bullet = ObjectPool.s().Get(ObjectPoolKey.ShotgunPellet);    
            BulletController bulletController = bullet.GetComponent<BulletController>();
            
            bulletController.LaunchAsSphere(ray.origin, ray.direction, 0.1f, bulletMask, 90f, shotgunDmgPellet, isBulletMine);
            bulletController.time_to_be_alive = 1;
        }
        
    }
    
    void ShootShotgun_Alt2(Vector3 shotPos, Vector3 shotDir)
    {
        if(playerGunLight)
        {
            playerGunLight.ShootShotgun();
        }
        if(gunAudio)
        {
            gunAudio.PlayOneShot(shotgunShotClip, 1);
        }
        
        if(pv.IsMine)
        {
            CameraShaker.MakeTrauma(0.4F);
        
            shotgun_animatorFPS.Play("Base.Fire_alt", 0, 0);
            //shotgunFX_ps.Play();
            shotgunFX_alt__ps.Play();
            
            // if(pController.IsGrounded())
            // {
            //     pController.BoostVelocityAdditive(-shotDir * 17.5f);
            // }
            // else
            // {
            //     pController.BoostVelocityAdditive(-shotDir * 13.5f);
            // }
        }
        
        ParticlesManager.PlayPooled(ParticleType.shot_explosion1, shotPos + shotDir, shotDir);
        
        Ray ray = new Ray(shotPos, shotDir);
        Vector3 ortho = Vector3.Cross(shotDir, Vector3.up).normalized;
        
        float pellet_speed = 70F;
        
        bool isMine = pv.IsMine;
        
        GameObject bullet = ObjectPool.s().Get(ObjectPoolKey.Shotgun_bullet_alt);
        BulletController bulletController = bullet.GetComponent<BulletController>();
        
        const int shotgunDmgPellet_alt = 140;
        
        bulletController.LaunchAsSphere(ray.origin, ray.direction, 0.1f, bulletMask, pellet_speed, shotgunDmgPellet_alt, isMine);
        bulletController.time_to_be_alive = 2F;
        bulletController.on_die_behave = BulletOnDieBehaviour.Default;
        
        
        for(int i = 0; i < shotgunPelletCount_Alt - 1; i++)
        {
            Vector3 rotatedOrtho = Quaternion.AngleAxis(60 * (i+1), shotDir) * ortho;
            Vector3 offsettedOrigin = shotPos + rotatedOrtho * shotgunSpread_Alt;
            
            bullet = ObjectPool.s().Get(ObjectPoolKey.Shotgun_bullet_alt);    
            bulletController = bullet.GetComponent<BulletController>();
            
            Vector3 offset_direction = (offsettedOrigin - shotPos).normalized;
            Vector3 altered_direction = ray.direction + offset_direction * 0.033F;
            altered_direction.Normalize();
            
            bulletController.LaunchAsSphere(offsettedOrigin, altered_direction, 0.1f, bulletMask, pellet_speed, shotgunDmgPellet_Alt, isMine);
            bulletController.time_to_be_alive = 2F;
            bulletController.on_die_behave = BulletOnDieBehaviour.Default;
        }
    }
    
    void ShootShotgun_Alt(Vector3 shotPos, Vector3 hitScanDirection, byte seed)
    {
        if(playerGunLight)
        {
            playerGunLight.ShootShotgun();
        }
        if(gunAudio)
        {
            gunAudio.PlayOneShot(shotgunShotClip, 1);
        }
        
        if(pv.IsMine)
        {
            CameraShaker.MakeTrauma(0.33f);
        
            shotgun_animatorFPS.Play("Base.Fire1", 0, 0);
            shotgunFX_ps.Play();
            
            if(pController.IsGrounded())
            {
                pController.BoostVelocityAdditive(-hitScanDirection * 17.5f);
            }
            else
            {
                pController.BoostVelocityAdditive(-hitScanDirection * 13.5f);
            }
        }
        
        ParticlesManager.PlayPooled(ParticleType.shot_explosion1, shotPos + hitScanDirection, hitScanDirection);
        
        Random.InitState(seed);
        GaussianDistribution gd = new GaussianDistribution();
        
        Quaternion rotation = Quaternion.LookRotation(hitScanDirection);
        
        RaycastHit hit;
        Ray ray = new Ray(shotPos, hitScanDirection);
        
        
        for(int i = 0; i < shotgunPelletCount_Alt; i++)
        {
            Vector3 randomPoint = new Vector3(gd.Next(0f, 1f, -1f, 1f), gd.Next(0f, 1f, -1f, 1f), 0);
            
            Vector3 pelletDir = hitScanDirection + rotation * randomPoint * shotgunSpread_Alt;
            ray.direction = Math.Normalized(pelletDir);
            
            float pellet_speed = 110f;
            float ray_max_distance = 60f;
            
            GameObject bullet = ObjectPool.s().Get(ObjectPoolKey.ShotgunPellet);    
            BulletController bulletController = bullet.GetComponent<BulletController>();
            bulletController.LaunchAsSphere(ray.origin, ray.direction, 0.1f, bulletMask, pellet_speed, shotgunDmgPellet, false);
            bulletController.time_to_be_alive = ray_max_distance / pellet_speed;
            bulletController.on_die_behave = BulletOnDieBehaviour.Hurtless;
            
            if(Physics.Raycast(ray, out hit, ray_max_distance, bulletMask))
            {
                
                bulletController.time_to_be_alive = pellet_speed * hit.distance;// / 0.5f;
                //lineEnd = hit.point;
                OnHitScan(hit.point, hitScanDirection, hit.normal, shotgunDmgPellet, hit.collider, null, 3, 0.125f);
            }
            else
            {
                OnHitScan(shotPos + hitScanDirection * 60, hitScanDirection, -hitScanDirection, shotgunDmgPellet, null);
            }
        }
    }
    
    void ShootRocketLauncher(Vector3 shotPos, Vector3 direction)
    {
        GameObject bullet = ObjectPool.s().Get(ObjectPoolKey.RocketLauncher_rocket);
        BulletController bulletController = bullet.GetComponent<BulletController>();
        
        if(pv.IsMine)
        {
            CameraShaker.MakeTrauma(0.1f);
            rocketLauncher_animatorFPS.Play("Base.Fire1", 0, 0);
            
            rocketLauncher.OnShoot();
        }
        
        if(gunAudio)
        {
            gunAudio.PlayOneShot(rocketLauncherShotClip, 1);
        }
        
        rocketLauncher_FX.Play();
        bool isMine = pv.IsMine;
        bulletController.LaunchAsSphere(shotPos, direction, 0.15F, bulletMask, 125, rocketLauncherDmg, isMine);
        bulletController.on_die_behave = BulletOnDieBehaviour.Explode_1;
        bulletController.explosionRadius = 4.5f;
        bulletController.explosionForce = 36;
        bulletController.explosionDamage = 400;
        bulletController.explosionPlayerDamage = 10;
        bulletController.explosionCanDamageLocalPlayer = true;
        bulletController.explosionCanDamageNPCs = true;
    }
    
    void ShootRocketLauncher_Alt(Vector3 shotPos, Vector3 shotDir, Vector3 upDir)
    {
       if(playerGunLight)
        {
            playerGunLight.ShootShotgun();
        }
        if(gunAudio)
        {
            gunAudio.PlayOneShot(rocketLauncherShotClip, 1);
        }
        
        if(pv.IsMine)
        {
            CameraShaker.MakeTrauma(1.0F);
            rocketLauncher_alt_animatorFPS.Play("Base.Fire1", 0, 0);
            rocketLauncher_animatorFPS.Play("Base.Fire1", 0, 0);
            rocketLauncher.OnShoot();
            //shotgunFX_alt__ps.Play();
        }
        
        ParticlesManager.PlayPooled(ParticleType.shot_explosion1, shotPos + shotDir, shotDir);
        
        Ray ray = new Ray(shotPos, shotDir);
        Vector3 ortho = Vector3.Cross(shotDir, Vector3.up).normalized;
        
        float projectile_speed = 80F;
        float angle = 70F;
        int bullets_count = 8;
        float angleStep = angle / bullets_count;
        angle = -angle / 2;
        bool isMine = pv.IsMine;
        
        GameObject bullet = ObjectPool.s().Get(ObjectPoolKey.RocketLauncher_alt_projectile);
        BulletController bulletController = bullet.GetComponent<BulletController>();
        
        const int rocketLauncher_dmg_alt = 300;
        
        bulletController.LaunchAsSphere(ray.origin, ray.direction, 0.25F, bulletMask, projectile_speed, rocketLauncher_dmg_alt, isMine);
        bulletController.time_to_be_alive = 4F;
        //bulletController.on_die_behave = BulletOnDieBehaviour.Default;
        
        for(int i = 0; i <= bullets_count; i++)
        {
            Vector3 rotated_dir = Quaternion.AngleAxis(angle, upDir) * shotDir;
            
            bullet = ObjectPool.s().Get(ObjectPoolKey.RocketLauncher_alt_projectile);
            bulletController = bullet.GetComponent<BulletController>();
            bulletController.LaunchAsSphere(ray.origin, rotated_dir, 0.25F, bulletMask, projectile_speed, rocketLauncher_dmg_alt, isMine);
            bulletController.time_to_be_alive = 4F;
            
            angle += angleStep;
        }
    }
    
    int BaseFire1_hash;
    
    public ParticleSystem AR_bulletShells_ps;
    public ParticleSystem ARGhost_bulletShells_ps;
    
    public ParticleSystem mp5_alt_bulletShells_ps;
    
    float arShoot_pitched_timeStamp;
    
    void ShootAR_HitScan(Vector3 shotPos, Vector3 hitScanDirection)
    {
        if(playerGunLight)
        {
            playerGunLight.ShootRevolver();
        }
        if(gunAudio)
        {
            //gunAudio.PlayOneShot(arShotClip, 0.9f);
            gunAudio_pitched.PlayOneShot(arShotClip, 1);
        }
        
        if(pv.IsMine)
        {
            CameraShaker.MakeTrauma(0.025f);
            //if(Time.time - arShoot_pitched_timeStamp > 0.09f)
            //{
            //    arShoot_pitched_timeStamp = Time.time;
                AR_animatorFPS.Play(BaseFire1_hash, 0, 0);
            //}
            if(AR_bulletShells_ps)
            {
                AR_bulletShells_ps.Emit(1);
            }
            if(hasARGhost)
            {
                AR_stronger_ps.Play();
            }
            else
            {
                AR_ps.Play();
            }
        //revolverFX_ps.Play();
        }
        
        RaycastHit hit;
        Ray ray = new Ray(shotPos, hitScanDirection);
        
        float ray_max_distance = 200f;
        float bullet_speed = 170f;
        
        GameObject bullet = ObjectPool.s().Get(ObjectPoolKey.AR_Bullet);    
        BulletController bulletController = bullet.GetComponent<BulletController>();
        
        bulletController.LaunchAsSphere(AR_gunpointFPS.position, ray.direction, 0.05f, bulletMask, bullet_speed, 0, false);
        bulletController.time_to_be_alive = ray_max_distance / bullet_speed;
        bulletController.on_die_behave = BulletOnDieBehaviour.Hurtless;
        
        
        if(Physics.Raycast(ray, out hit, ray_max_distance, bulletMask))
        {
            bulletController.time_to_be_alive = hit.distance / bullet_speed;
            OnHitScan(hit.point, hitScanDirection, hit.normal, arDmg, hit.collider, null, 1.5f, 0.075f, 0);
        }
        else
        {
            OnHitScan(shotPos + hitScanDirection * ray_max_distance, hitScanDirection, -hitScanDirection, revolverDmg, null);
        }
    }
    
    void ShootARGhost_HitScan(Vector3 shotPos, Vector3 hitScanDirection)
    {
        if(gunAudio)
        {
            gunAudio.PlayOneShot(arShotClip, 0.6f);
        }
        
        if(pv.IsMine)
        {
            CameraShaker.MakeTrauma(0.025f);
            if(AR_ghost_animator_fps)
                AR_ghost_animator_fps.Play(BaseFire1_hash , 0, 0);
            if(ARGhost_bulletShells_ps)
            {
                ARGhost_bulletShells_ps.Emit(1);
            }
            AR_ghost_ps.Play();
            revolverFX_ps.Play();
        }
        
        RaycastHit hit;
        Ray ray = new Ray(shotPos, hitScanDirection);
        
        //float revolverShotMaxDistance = 200f;
        
        float ray_max_distance = 200f;
        float bullet_speed = 170f;
        
        GameObject bullet = ObjectPool.s().Get(ObjectPoolKey.AR_Bullet);    
        BulletController bulletController = bullet.GetComponent<BulletController>();
        bulletController.on_die_behave = BulletOnDieBehaviour.Hurtless;
        
        
        bulletController.LaunchAsSphere(AR_gunpoint_ghostFPS.position, ray.direction, 0.05f, bulletMask, bullet_speed, 0, false);
        bulletController.time_to_be_alive = ray_max_distance / bullet_speed;
        
        
        if(Physics.Raycast(ray, out hit, ray_max_distance, bulletMask))
        {
            bulletController.time_to_be_alive = hit.distance / bullet_speed;
            //lineEnd = hit.point;
            OnHitScan(hit.point, hitScanDirection, hit.normal, arDmg, hit.collider, null, 1.5f, 0.005f, 0);
        }
        else
        {
            OnHitScan(shotPos + hitScanDirection * ray_max_distance, hitScanDirection, -hitScanDirection, revolverDmg, null);
        }
    }
    
    void ShootMP5_HitScan(Vector3 shotPos, Vector3 hitScanDirection)
    {
        if(playerGunLight)
        {
            playerGunLight.ShootRevolver();
        }
        if(gunAudio)
        {
            gunAudio.PlayOneShot(arShotClip, 0.9f);
        }
        
        if(pv.IsMine)
        {
            CameraShaker.MakeTrauma(0.025f);
            mp5_alt_animatorFPS.Play(BaseFire1_hash, 0, 0);
            if(mp5_alt_bulletShells_ps)
            {
                mp5_alt_bulletShells_ps.Emit(1);
            }
        }
        if(mp5_alt_ps)        
            mp5_alt_ps.Play();
        
        RaycastHit hit;
        Ray ray = new Ray(shotPos, hitScanDirection);
        
        float ray_max_distance = 200f;
        float bullet_speed = 220f;
        
        GameObject bullet = ObjectPool2.s().Get(ObjectPoolKey.mp5_bullet);    
        BulletController bulletController = bullet.GetComponent<BulletController>();
        
        //bulletController.LaunchAsSphere(mp5_alt_gunpointFPS.position, ray.direction, 0.05f, bulletMask, bullet_speed, 0, false);
        
        bulletController.LaunchAsSphere(AR_gunpointFPS.position, ray.direction, 0.05f, bulletMask, bullet_speed, 0, false);
        bulletController.time_to_be_alive = ray_max_distance / bullet_speed;
        bulletController.on_die_behave = BulletOnDieBehaviour.Hurtless;
        
        if(Physics.Raycast(ray, out hit, ray_max_distance, bulletMask))
        {
            bulletController.time_to_be_alive = hit.distance / bullet_speed;
            OnHitScan(hit.point, hitScanDirection, hit.normal, mp5_dmg, hit.collider, null, 1.5f, 0.075f);
        }
        else
        {
            OnHitScan(shotPos + hitScanDirection * ray_max_distance, hitScanDirection, -hitScanDirection, revolverDmg, null);
        }
    }
    
    void ShootMP5_alt(Vector3 shotPos, Vector3 direction)
    {
        GameObject bullet = ObjectPool2.s().Get(ObjectPoolKey.mp5_grenade);
                
        
        if(pv.IsMine)
        {
            CameraShaker.MakeTrauma(0.1f);
            mp5_alt_animatorFPS.Play("Base.Fire2", 0, 0);
        }
        
        if(gunAudio)
        {
            gunAudio.PlayOneShot(rocketLauncherShotClip, 1);
        }
        
        //rocketLauncher_FX.Play();
        BulletController bulletController = bullet.GetComponent<BulletController>();
        bool isMine = pv.IsMine;
        float grenade_speed = 30;
        bulletController.LaunchAsSphere(shotPos, direction, 0.15F, bulletMask, grenade_speed, mp5_alt_dmg, isMine);
        bulletController.gravProjV = direction * grenade_speed;
        bulletController.useGravity = true;
        bulletController.gravityMultiplier = 2;
        bulletController.on_die_behave = BulletOnDieBehaviour.Explode_1;
        
        bulletController.explosionRadius = 4.5f;
        bulletController.explosionForce = 36;
        bulletController.explosionDamage = 400;
        bulletController.uniqueID = Random.Range(0, 256);
        
        bulletController.explosionCanDamageLocalPlayer = false;
        bulletController.explosionCanDamageNPCs = true;
    }
    
    
    void OnHitScan(Vector3 point, Vector3 damageDirection, Vector3 normal, int damage, Collider col = null, NetworkObject targetNetworkObject = null, float headshotDmgMult = 2, float shakeMultiplier = 1, float forceMultiplier = 1)
    {
        if (col != null)
        {
            if(targetNetworkObject == null)
            {
                targetNetworkObject = col.GetComponent<NetworkObject>();
            }
            
            DamagableLimb limb = col.GetComponent<DamagableLimb>();
            if(limb)
            {
                targetNetworkObject    = limb.net_comp_from_parent;
                if(limb.isHeadshot)
                {
                    damage = (int)((float)damage * headshotDmgMult);
                    //PlayHeadshotSmall
                    //InGameConsole.LogFancy("Headshot damage is " + dmg.ToString());
                }
                if(!limb.isMasterAlive)
                {
                    limb.TakeDamageLimb(damage);
                    limb.AddForceToLimb(damage * damageDirection);
                }
                limb.React(point, damageDirection);
                //Debug:
            }
            
            if(targetNetworkObject != null)
            {
                IDamagableLocal idl = targetNetworkObject.GetComponent<IDamagableLocal>();
                if(idl != null)
                {
                    idl.TakeDamageLocally(damage, point, damageDirection);
                    
                    if(pv.IsMine)
                    {
                        if(limb.isHeadshot && headshotDmgMult > 1.1f)
                        {
                            CrosshairController.MakeHeadShot();
                        }
                        
                        //int remainingHitPoints = target_hp - damage;
                        //InGameConsole.LogFancy(string.Format("Remaining hitpoints: {0}", (target_hp - damage)));
                        CameraShaker.MakeTrauma(0.15f * shakeMultiplier);
                        if(limb.isMasterAlive)
                        {
                            if(forceMultiplier > 0.01f)
                            {
                                NetworkObjectsManager.CallNetworkFunction(targetNetworkObject.networkId, NetworkCommand.TakeDamageLimbWithForce, damage, damageDirection * forceMultiplier, limb.limb_id);
                            }
                            else
                            {
                                NetworkObjectsManager.CallNetworkFunction(targetNetworkObject.networkId, NetworkCommand.TakeDamageLimbNoForce, damage, limb.limb_id);
                                //TakeDamageLimbNoForce
                            }
                        }
                    }
                }
            }
            else
            {
                PhotonView pv = col.GetComponent<PhotonView>();
                if(pv)
                {
                    // col.GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.AllViaServer, dmg);
                    //col.GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.All, dmg);
                    
                }
                else // We hit something static
                {
                    ParticlesManager.Play(ParticleType.shot, point, -damageDirection);
                }
            }
        }
    }
    
    HashSet<IKillableThing> killables_cache = new HashSet<IKillableThing>();
    
    void OnHitScanBounce(Vector3 point, Vector3 damageDirection, Vector3 normal, int damage, Collider col = null, NetworkObject targetNetworkObject = null, float headshotDmgMult = 2, float shakeMultiplier = 1, float forceMultiplier = 1)
    {
        if (col != null)
        {
            if(targetNetworkObject == null)
            {
                targetNetworkObject = col.GetComponent<NetworkObject>();
            }
            
            DamagableLimb limb = col.GetComponent<DamagableLimb>();
            if(limb)
            {
                targetNetworkObject    = limb.net_comp_from_parent;
                if(limb.isHeadshot)
                {
                    damage = (int)((float)damage * headshotDmgMult);
                    //PlayHeadshotSmall
                    //InGameConsole.LogFancy("Headshot damage is " + dmg.ToString());
                }
                if(!limb.isMasterAlive)
                {
                    limb.TakeDamageLimb(damage);
                    limb.AddForceToLimb(damage * damageDirection);
                }
                limb.React(point, damageDirection);
                //Debug:
            }
            
            if(targetNetworkObject != null)
            {
                IDamagableLocal idl = targetNetworkObject.GetComponent<IDamagableLocal>();
                if(idl != null)
                {
                    idl.TakeDamageLocally(damage, point, damageDirection);
                    
                    if(pv.IsMine)
                    {
                        if(limb.isHeadshot && headshotDmgMult > 1.1f)
                        {
                            CrosshairController.MakeHeadShot();
                        }
                        
                        //int remainingHitPoints = target_hp - damage;
                        //InGameConsole.LogFancy(string.Format("Remaining hitpoints: {0}", (target_hp - damage)));
                        CameraShaker.MakeTrauma(0.15f * shakeMultiplier);
                        if(limb.isMasterAlive)
                        {
                            if(forceMultiplier > 0.01f)
                            {
                                NetworkObjectsManager.CallNetworkFunction(targetNetworkObject.networkId, NetworkCommand.TakeDamageLimbWithForce, damage, damageDirection * forceMultiplier, limb.limb_id);
                            }
                            else
                            {
                                NetworkObjectsManager.CallNetworkFunction(targetNetworkObject.networkId, NetworkCommand.TakeDamageLimbNoForce, damage, limb.limb_id);
                                //TakeDamageLimbNoForce
                            }
                            
                            if(pv.IsMine)
                            {
                                if(!limb.isHeadshot)
                                {
                                    return;
                                }
                                killables_cache.Clear();
                                int len = NPCManager.killablesMaxNum;
                                const float maxDistanceSqr = 16 * 16;
                                // const float maxDistanceSqr = 32 * 32;
                                int maxBounces = 4;
                                // if(maxBounces > NPCManager.killables.Count)
                                // {
                                //     maxBounces = NPCManager.killables.Count;
                                // }
                                Vector3 startPoint = point;
                                
                                for(int j = 0; j < maxBounces; j++)
                                {
                                    //InGameConsole.LogFancy(string.Format("maxBounces: {0}, killables.Count: {1}", maxBounces, NPCManager.killables.Count));
                                    IKillableThing killable_target = null;
                                    float closestDistance = float.MaxValue;
                                    
                                    for(int i = 0; i < len; i++)
                                    {
                                        if(NPCManager.killables[i] != null)
                                        {
                                            if(NPCManager.killables[i].CanBeBounceHit() && !killables_cache.Contains(NPCManager.killables[i]))
                                            {
                                                float sqrDistanceToKillable = Math.SqrDistance(NPCManager.killables[i].GetHitSpot(), startPoint);
                                                if((sqrDistanceToKillable < maxDistanceSqr) && (sqrDistanceToKillable < closestDistance))
                                                {
                                                    closestDistance = sqrDistanceToKillable;
                                                    killable_target = NPCManager.killables[i];
                                                }
                                            }
                                        }
                                    }
                                    
                                    if(killable_target != null)
                                    {
                                        Vector3 hitSpot = killable_target.GetHitSpot();
                                        RaycastHit bounceHit; 
                                        Vector3 bounceDir = (hitSpot - startPoint).normalized;
                                        Ray bounceRay = new Ray(startPoint, bounceDir);
                                        float d = Math.Magnitude(hitSpot - startPoint);
                                        killables_cache.Add(killable_target);
                                        
                                        float bounceDelay = 0.1f;
                                        if(!PhotonNetwork.OfflineMode)
                                        {
                                            bounceDelay = bounceDelay / 2;
                                        }
                                        
                                        if(Physics.Raycast(bounceRay, out bounceHit, d, groundCeilingMask))
                                        {
                                            //Target is obstructed:
                                            float _bounceDelay = (j > 2) ? bounceDelay / 1.5f : bounceDelay;
                                            StartCoroutine(RBounceWall(startPoint, bounceHit.point, bounceDelay * (j + 1)));
                                            //NetworkObjectsManager.CallGlobalCommand(GlobalCommand.BounceHit_RevolverBlue, RpcTarget.All, startPoint, bounceHit.point);
                                            
                                        }
                                        else
                                        {
                                            //Target was not obstructed:
                                            //maxBounces--;
                                            NetworkObject killable_net_comp = killable_target.GetNetComp();
                                            if(killable_net_comp)
                                            {
                                                //NetworkObjectsManager.CallGlobalCommand(GlobalCommand.BounceHit_RevolverBlue, RpcTarget.All, startPoint, hitSpot);
                                                //NetworkObjectsManager.CallNetworkFunction(killable_net_comp.networkId, NetworkCommand.TakeDamageLimbWithForce, 400, bounceDir * 800,  killable_target.GetHitSpotLimbId());
                                                float _bounceDelay = (j > 2) ? bounceDelay / 1.5f : bounceDelay;
                                                StartCoroutine(RBounce(startPoint, hitSpot, killable_net_comp.networkId, bounceDir, killable_target.GetHitSpotLimbId(), bounceDelay * (j + 1)));
                                               // ShootRevolver_Blue(startPoint, bounceRay.direction);
                                                startPoint = hitSpot;
                                            }
                                        }
                                        
                                    }
                                    else
                                    {
                                        j = maxBounces + 1;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                PhotonView pv = col.GetComponent<PhotonView>();
                if(pv)
                {
                    // col.GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.AllViaServer, dmg);
                    //col.GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.All, dmg);
                    
                }
                else // We hit something static
                {
                    ParticlesManager.Play(ParticleType.shot, point, -damageDirection);
                }
            }
        }
    }
    
    IEnumerator RBounceWall(Vector3 startPoint, Vector3 hitPoint, float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        // Now do your thing here
        NetworkObjectsManager.CallGlobalCommand(GlobalCommand.BounceHit_RevolverBlue, RpcTarget.All, startPoint, hitPoint);
    }
    
    
    IEnumerator RBounce(Vector3 startPoint, Vector3 hitSpot, int networkId, Vector3 bounceDir, byte limb_id, float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        
        NetworkObjectsManager.CallGlobalCommand(GlobalCommand.BounceHit_RevolverBlue, RpcTarget.All, startPoint, hitSpot);
        NetworkObjectsManager.CallNetworkFunction(networkId, NetworkCommand.TakeDamageLimbWithForce, 400, bounceDir * 800,  limb_id);
    }
    
    void OnHitExplosive(Vector3 point, Vector3 damageDirection, Vector3 normal, int damage, Collider col = null, NetworkObject targetNetworkObject = null, float headshotDmgMult = 2, float shakeMultiplier = 1, float forceMultiplier = 1)
    {
        if (col != null)
        {
            if(targetNetworkObject == null)
            {
                targetNetworkObject = col.GetComponent<NetworkObject>();
            }
            
            DamagableLimb limb = col.GetComponent<DamagableLimb>();
            if(limb)
            {
                targetNetworkObject    = limb.net_comp_from_parent;
                if(limb.isHeadshot)
                {
                    damage = (int)((float)damage * headshotDmgMult);
                    //PlayHeadshotSmall
                    //InGameConsole.LogFancy("Headshot damage is " + dmg.ToString());
                }
                if(!limb.isMasterAlive)
                {
                    limb.AddForceToLimb(damage * damageDirection);
                }
                limb.React(point, damageDirection);
                //Debug:
            }
            
            if(targetNetworkObject != null)
            {
                IDamagableLocal idl = targetNetworkObject.GetComponent<IDamagableLocal>();
                if(idl != null)
                {
                    idl.TakeDamageLocally(damage, point, damageDirection);
                    
                    if(pv.IsMine)
                    {
                        if(limb.isHeadshot && headshotDmgMult > 1.1f)
                        {
                            CrosshairController.MakeHeadShot();
                        }
                        
                        //int remainingHitPoints = target_hp - damage;
                        //InGameConsole.LogFancy(string.Format("Remaining hitpoints: {0}", (target_hp - damage)));
                        if(limb.isMasterAlive)
                        {
                            CameraShaker.MakeTrauma(0.15f * shakeMultiplier);
                            NetworkObjectsManager.CallNetworkFunction(targetNetworkObject.networkId, NetworkCommand.TakeDamageExplosive, damage);
                        }
                    }
                }
            }
            else
            {
                PhotonView pv = col.GetComponent<PhotonView>();
                if(pv)
                {
                    // col.GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.AllViaServer, dmg);
                    //col.GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.All, dmg);
                    
                }
                else // We hit something static
                {
                    ParticlesManager.Play(ParticleType.shot, point, -damageDirection);
                }
            }
        }
    }
    
    
    public PlayerLightController playerGunLight;
    
}
