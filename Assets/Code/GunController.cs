using UnityEngine;
using System;
using System.Collections.Generic;
using Photon.Pun;

public class GunController : MonoBehaviour 
{

    BodyShakeIKAnimator bonesToShake;

    public Transform gunPlace;
    
    [Header("Muzzle Flash Positions:")]
    public Transform muzzlePistol;
    public Transform muzzleShotgun;
    
    [Header("Muzzle Flash Particles:")]
    public ParticleSystem glockFireVFX;
    public ParticleSystem shotgunFire_PS;
    
    [Header("Shells emitters:")]
    public ParticleSystem bolter_shells_ps;

    PhotonView photonView;    
    
    public Weapon slot1;
    public Weapon slot2;
    public Weapon slot3;


    public int currentSlot = 0;

    public AudioSource gunAudio;
    GameObject weaponGameObject;
    public Weapon currentWeapon;
    PlayerAnimationController playerAnimController;
    //Transform muzzle;
    Transform muzzleTransform;
    [SerializeField] float weaponTimer = 0f;
    public int bulletsInClip = 0;

    const float changingWeaponDuration = 0.5f;
    
    
    Dictionary<AmmoType, int> ammo = new Dictionary<AmmoType, int>();
    
    public Action RedrawUIAction;
    
    int bulletMask;
    
    Transform thisTransform;
    
    [Header("Holsted weapons:")]
    public Transform holsted_slot1;
    public GameObject holsted_weapon1;
    public Transform holsted_slot2;
    public GameObject holsted_weapon2;
    
    private void Awake()
    {
        sphere_mask = LayerMask.GetMask("Ground");
        bulletMask = LayerMask.GetMask("Default", "Ground", "NPC", "Fadable");
        playerController = GetComponent<PlayerController>();
        thisTransform = transform;
        
        if (gunAudio == null)
            Debug.LogWarning("Gun audio source player not set!");
        //muzzlePistol = transform.Find("muzzlePistol");
        
#if UNITY_EDITOR
        if(muzzlePistol == null)
        {
            Debug.LogError("MuzzlePistol not found");
        }
#endif

        bonesToShake = GetComponent<BodyShakeIKAnimator>();
        photonView = GetComponent<PhotonView>();
        
        playerAnimController = GetComponent<PlayerAnimationController>();
     
        Array ammoTypes = Enum.GetValues(typeof(AmmoType));
        foreach(AmmoType type in ammoTypes)
            ammo.Add(type, 0);
    }

    void UnArmSlot(int slotNumber)
    {
        switch(slotNumber)
        {
            case 0:
                slot1 = null;
                break;

            case 1:
                slot2 = null;
                break;
            case 2:
                slot3 = null;
                break;
        }
    }

    public Weapon GetWeaponInSlot()
    {
        Weapon Result_slot = slot1;
        switch(currentSlot)
        {
            case 0:
                Result_slot = slot1;
                break;

            case 1:
                Result_slot = slot2;
                break;
            case 2:
                Result_slot = slot3;
                break;
        }
        
        return Result_slot;
        // return currentSlot == 0 ? slot1 : slot2;
    }

    public void SwitchSlot()
    {
        //if(ammo.ContainsKey(GetWeaponInSlot().ammoType))
        //    ammo[GetWeaponInSlot().ammoType] += bulletsInClip;
        bolterShotsCount = 0;
        
        currentSlot++;
        if(currentSlot > 2)
        {
            currentSlot = 0;
        }
        
        // currentSlot = currentSlot == 0 ? 1 : 0;
    }

    void AssignToCurrentSlot(Weapon weapon)
    {
        switch(currentSlot)
        {
            case 0:
                slot1 = weapon;
                break;
            case 1:
                slot2 = weapon;
                break;
            case 2:
                slot3 = weapon;
            break;
        }
    }

    //[RPC]
    void UnArmWeapon()
    {
        if(photonView.IsMine)
        {
            revolver_fps.gameObject.SetActive(false);
            shotgun_fps.gameObject.SetActive(false);
            rocketLauncher_fps.gameObject.SetActive(false);            
        }
        else
        {
            revolver_tps.gameObject.SetActive(false);
            shotgun_tps.gameObject.SetActive(false);
            rocketLauncher_tps.gameObject.SetActive(false);
        }
    }
    
    
    public Transform gunPlaceFPS;
    
    [Header("Armed fps weapons:")]
    public Transform shotgun_fps;
    public Transform revolver_fps;
    public Transform rocketLauncher_fps;
    
    
    [Header("Armed tps weapons:")]
    public Transform shotgun_tps;
    public Transform revolver_tps;
    public Transform rocketLauncher_tps;
    
    public LineRenderer revolver_lr;
    
    void WieldWeaponGoodFPS(EntityType weapon_entity_type)
    {
        switch(weapon_entity_type)
        {
            case(EntityType.REVOLVER_ENTITY):
            {
                revolver_fps.gameObject.SetActive(true);
                shotgun_fps.gameObject.SetActive(false);
                rocketLauncher_fps.gameObject.SetActive(false);
                
                break;
            }
            case(EntityType.SHOTGUN_ENTITY):
            {
                revolver_fps.gameObject.SetActive(false);
                shotgun_fps.gameObject.SetActive(true);
                rocketLauncher_fps.gameObject.SetActive(false);
                
                break;
            }
            case(EntityType.ROCKETLAUNCHER_ENTITY):
            {
                revolver_fps.gameObject.SetActive(false);
                shotgun_fps.gameObject.SetActive(false);
                rocketLauncher_fps.gameObject.SetActive(true);
                
                break;
            }
        }
    }
    
    void WieldWeaponGoodTPS(EntityType weapon_entity_type)
    {
        switch(weapon_entity_type)
        {
            case(EntityType.REVOLVER_ENTITY):
            {
                revolver_tps.gameObject.SetActive(true);
                shotgun_tps.gameObject.SetActive(false);
                rocketLauncher_tps.gameObject.SetActive(false);
                
                break;
            }
            case(EntityType.SHOTGUN_ENTITY):
            {
                revolver_tps.gameObject.SetActive(false);
                shotgun_tps.gameObject.SetActive(true);
                rocketLauncher_tps.gameObject.SetActive(false);
                
                break;
            }
            case(EntityType.ROCKETLAUNCHER_ENTITY):
            {
                revolver_tps.gameObject.SetActive(false);
                shotgun_tps.gameObject.SetActive(false);
                rocketLauncher_tps.gameObject.SetActive(true);
                
                break;
            }
        }
    }
    
    // This must work locally!
    public void WieldWeapon(Weapon weaponToWield = null)
    {
        
        UnArmWeapon();
        if(weaponToWield == null)
        {
            timeWhenCanFire = 0f;
            DrawUI();
            
            return;
        }
        
        if(photonView.IsMine)
        {
            // weaponGameObject = Instantiate(weaponToWield.prefab, gunPlaceFPS, false);
            WieldWeaponGoodFPS(weaponToWield.ent_type);
        }
        else
        {
            // weaponGameObject = Instantiate(weaponToWield.prefab, gunPlace, false);
            WieldWeaponGoodTPS(weaponToWield.ent_type);
        }
        
       
        
        // timeWhenCanFire = Time.time + 1.5f;
        timeWhenCanFire = UberManager.TimeSinceStart() + changingWeaponDuration;
        DrawUI();
    }

    public int GetCurrentAmmo()
    {
        return 1000;
        
        
        int result = -1;
        if(GetWeaponInSlot() != null)
            result = ammo[GetWeaponInSlot().ammoType];

        return result;
    }
    

    int itemOnGroundIndex = -1;
    int itemsCount = 0;
    public List<Transform> itemsOnGround = new List<Transform>();
    List<Transform> items = new List<Transform>();
    
    void ScanForPickupables()
    {
        //List<Transform> items = WeaponManager.singleton.GetNearestPickUpableWeapons(transform.position, 3f);
        WeaponManager.Singleton().GetNearestPickUpableWeapons(transform.position, 3f, ref items);

        // int count = itemsOnGround.Count;
        
        for(int i = 0; i < itemsOnGround.Count; i++)
        {
            if(items.Contains(itemsOnGround[i]) == false)
            {
                itemsOnGround.RemoveAt(i);
                if(itemOnGroundIndex == i)
                    itemOnGroundIndex--;
            }
        }

        for(int i = 0; i < items.Count; i++)
        {
            if(itemsOnGround.Contains(items[i]) == false)
                itemsOnGround.Add(items[i]);
        }
        


        itemOnGroundIndex = Mathf.Clamp(itemOnGroundIndex, 0, itemsOnGround.Count - 1);
        itemsCount = itemsOnGround.Count;

        if(itemsCount > 0)
        {
            if(Input.GetKeyDown(KeyCode.V))
            {
                itemOnGroundIndex++;
                if(itemOnGroundIndex >= itemsCount) //loop index
                    itemOnGroundIndex = 0;
            }

            Transform item = itemsOnGround[itemOnGroundIndex];
            DroppedItem droppedItem = item.GetComponent<DroppedItem>();
            Weapon weaponToPickup = WeaponManager.Singleton().GetWeapon(droppedItem.entityType);
            AmmoBox ammoBox = item.GetComponent<AmmoBox>();
            if(weaponToPickup != null)
                ItemInfo3dLabel.singleton.PopUp(item.position, weaponToPickup);
            else
            {
                if(ammoBox != null)
                {
                    ItemInfo3dLabel.singleton.PopUp(item.position, ammoBox.ammoType);
                }
                else
                {
                    //KEY:
                    if(droppedItem.isKey)
                        ItemInfo3dLabel.singleton.PopUpKey(item.position, droppedItem.entityType);
                }
            }
            

            if(Input.GetKeyDown(KeyCode.G))
            {
                AudioManager.PlayClip(SoundType.gun_pick_up, 0.6f, 1);
                droppedItem.OnPickUp();
                
                NetworkObject droppedItemNetComp = droppedItem.GetComponent<NetworkObject>();
                
                //InGameConsole.LogOrange(string.Format(" Disabling <color=green>{0}</color> with netId: <color=yellow>{1}</color>", droppedItemNetComp.gameObject.name, droppedItemNetComp.networkId));
                
                NetworkObjectsManager.DisableObject(droppedItemNetComp.networkId);
                
                if(weaponToPickup != null)
                {
                    if(GetWeaponInSlot() != null)
                    {
                        switch(currentSlot)
                        {
                            case 0:
                            {
                                if(slot2 == null)
                                    SwitchSlot();
                                else
                                {
                                    //Drop item in our hand:
                                    DropItem(GetWeaponInSlot().poolKey);
                                }
                                break;
                            }
                            case 1:
                            {
                                if(slot3 == null)
                                    SwitchSlot();
                                else
                                {
                                    //Drop item in our hand:
                                    DropItem(GetWeaponInSlot().poolKey);
                                }
                                break;
                            }
                            case 2:
                            {
                                if(slot1 == null)
                                    SwitchSlot();
                                else
                                {
                                    //Drop item in our hand:
                                    DropItem(GetWeaponInSlot().poolKey);
                                }
                                break;
                            }
                        }
                    }
                    WearToSlot(droppedItem.entityType);
                }
                else
                {
                    
                    if(ammoBox != null)
                    {
                        ammo[ammoBox.ammoType] = ammo[ammoBox.ammoType] + ammoBox.amount;
                        DrawUI();
                    }
                }
            }
        }
        else
        {
            itemOnGroundIndex = -1;
            ItemInfo3dLabel.singleton.Hide();
        }
    }
    
    
    void DropItem(ObjectPoolKey poolKey)
    {
        Vector3 startPos = thisTransform.position + new Vector3(0, 1.25f, 0) + thisTransform.forward * 0.85f; 
        
        Vector3 velocity = new Vector3(0, 2f, 6f);
        
        velocity = thisTransform.TransformDirection(velocity);
        
        velocity += playerController.velocity;
        
        
        if(PhotonNetwork.IsMasterClient)
        {
            NetworkObjectsManager.SpawnNewItem(poolKey, startPos, velocity);
        }
        else
        {
            NetworkObjectsManager.AskMasterToSpawnNewItem(poolKey, startPos, velocity);
        }
    }
    


    //Network-friendly version of WearToSlot(Weapon weapon)
    void WearToSlot(EntityType weaponType)
    {
        Weapon weapon = WeaponManager.Singleton().GetWeapon(weaponType);
        
        WearToSlot(weapon);
    }
    
    void WearToSlot(Weapon weapon)
    {
        Weapon weaponInHands = GetWeaponInSlot();
        

        AssignToCurrentSlot(weapon);
        WieldWeapon(weapon); 
    }
    
    
    int bolterShotsCount = 0;
    const float bolterSpread = 0.125F;
    
    static readonly Quaternion qIdentity = Quaternion.identity;
    
    Vector3 gizmoPos;
    
    public Transform checkIfCanShootSpherePlace;
    float radiusShootSphere = 0.75f;
    int sphere_mask;
    
    public PlayerLightController playerGunLight;
    
    void ShootActualBullets(EntityType weaponEntity, Vector3 shotPosition, Vector3 shotDirection, out Vector3 dir_modified, bool modify)
    {
        Weapon weaponToFireWith = WeaponManager.Singleton().GetWeapon(weaponEntity);
        
        dir_modified = shotDirection;
        
        switch(weaponEntity)
        {
            case(EntityType.GLOCK_ENTITY):
            {
                GameObject bullet = ObjectPool.s().Get(ObjectPoolKey.GlockBullet);
                
                BulletController bulletController = bullet.GetComponent<BulletController>();
                
                // bulletController.LaunchAsRay(shotPosition, shotDirection, bulletMask);
                
                break;                
            }
            case(EntityType.SHOTGUN_ENTITY):
            {
               
                int pelletsCount = 16;
                float angleRange = 20f;
                float angleStep = angleRange / pelletsCount;
                float angle = -angleRange / 2;
               
                for(int i = 0; i < pelletsCount; i++)
                {
                    GameObject bullet = ObjectPool.s().Get(ObjectPoolKey.ShotgunPellet);    
                    
                    BulletController bulletController = bullet.GetComponent<BulletController>();
                    
                    
                    // Vector3 rotatedShotDirection = Math.GetVectorRotatedXZ(shotDirection, angle);
                    
                     Vector3 rotatedShotDirection = Quaternion.AngleAxis(angle, Vector3.up) * shotDirection;
                    
                    // bulletController.LaunchAsRay(shotPosition, rotatedShotDirection, bulletMask);
                    angle += angleStep;
                    bulletController.time_to_be_alive = 0.725f;
                }
                
                if(playerGunLight)
                {
                    playerGunLight.ShootShotgun();
                }
               
                
                break;                
            }
            case(EntityType.CROSSBOW_ENTITY):
            {
                GameObject bolt = ObjectPool.s().Get(ObjectPoolKey.Bolt);
                var boltController = bolt.GetComponent<CrossbowBoltController>();
                boltController.Launch(shotPosition, shotDirection, bulletMask);
            
                //bolt.GetComponent<NetObjectOwner>().ownerId = this.photonView.ViewID;
                
                break;                
            }
            case(EntityType.HEAVYBOLTER_ENTITY):
            {
                GameObject bullet = ObjectPool.s().Get(ObjectPoolKey.HeavyBolterBullet);
                
                bolterShotsCount++;
                
                bool isCountEven = (bolterShotsCount % 2 == 0);
                
                Vector3 offset = isCountEven ? thisTransform.right : -thisTransform.right; 
                
                offset *= bolterSpread;
                
                shotPosition += offset;
                
                gizmoPos = shotPosition;
                
                BulletController bulletController = bullet.GetComponent<BulletController>();
                bulletController.on_die_behave = BulletOnDieBehaviour.Reflect;
                if(modify && false)
                {
                    float randomAngle = UnityEngine.Random.Range(-5f, 5f);
                    dir_modified = Math.GetVectorRotatedXZ(dir_modified, randomAngle);
                }
                
                
                float angleOffset = 3;
                if(!isCountEven)
                {
                    angleOffset = -angleOffset;
                }
                
               
                
                //bolter_shells_ps.Emit(1);
                
                if(playerGunLight)
                {
                    playerGunLight.ShootBolter();
                }
                //bulletController.Launch(shotPosition, shotDirection,  this.photonView.ViewID, bulletMask);
                // bulletController.LaunchAsRay(shotPosition, dir_modified, bulletMask);
                
                break;
            }
            case(EntityType.REVOLVER_ENTITY):
            {
                // GameObject bullet = ObjectPool.s().Get(ObjectPoolKey.RevolverBullet);
                
                // BulletController bulletController = bullet.GetComponent<BulletController>();
               
                
                // //bolter_shells_ps.Emit(1);
                
                if(playerGunLight)
                {
                    playerGunLight.ShootRevolver();
                }
                
                
                ShootRevolver(shotPosition, dir_modified);
                
                // bulletController.LaunchAsRay(shotPosition, dir_modified, bulletMask);
                // bulletController.time_to_be_alive = 0.01f;
                
                break;
            }
            case(EntityType.ROCKETLAUNCHER_ENTITY):
            {
                GameObject bullet = ObjectPool.s().Get(ObjectPoolKey.RocketLauncher_rocket);
                
                BulletController bulletController = bullet.GetComponent<BulletController>();
               
                
                //bolter_shells_ps.Emit(15);
                
                if(playerGunLight)
                {
                    playerGunLight.ShootRocketLauncher();
                }
                
                //bulletController.LaunchAsRay(shotPosition, dir_modified,  this.photonView.ViewID, bulletMask);
                // bulletController.LaunchAsSphere(shotPosition, dir_modified, bulletMask, 46);
                bulletController.on_die_behave = BulletOnDieBehaviour.Explode_1;
                
                
                break;
            }
            default:
            {
                
                break;
            }
        }
    }
    
    const int revolverDamage = 300;
    
    void ShootRevolver(Vector3 shotPos, Vector3 dir)
    {
        RaycastHit hit;
        Ray ray = new Ray(shotPos, dir);
        if(Physics.Raycast(ray, out hit, 200, bulletMask))
        {
             OnHitScan(hit.point, -hit.normal, hit.normal, revolverDamage, hit.collider);
        }
        else
        {
            
            OnHitScan(shotPos + dir * 200, dir, -dir, revolverDamage, null);
        }
    }
    
    void OnHitScan(Vector3 point, Vector3 direction, Vector3 normal, int dmg, Collider col = null)
    {
        if (col != null)
        {
            DamagableLimb limb = col.GetComponent<DamagableLimb>();
            NetworkObject targetNetworkObject;
            
            if(limb)
            {
                targetNetworkObject    = limb.net_comp_from_parent;
                limb.React(point, direction);
            }
            else
            {
                targetNetworkObject = col.GetComponent<NetworkObject>();
            }
            
            
            if(targetNetworkObject != null)
            {
                IDamagableLocal idl = targetNetworkObject.GetComponent<IDamagableLocal>();
                if(idl != null)
                {
                    //idl.TakeDamageLocally(dmg, direction, 0f, 0f);
                    
                    if(photonView.IsMine)
                    {
                        NetworkObjectsManager.CallNetworkFunction(targetNetworkObject.networkId, NetworkCommand.TakeDamageLimbWithForce, dmg);
                    }
                                                            
                                        
                }
            }
            else
            {
                PhotonView pv = col.GetComponent<PhotonView>();
                if(pv)
                {
                    // col.GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.AllViaServer, dmg);
                    col.GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.All, dmg);
                    
                }
                else // We hit something static
                {
                    ParticlesManager.Play(ParticleType.shot, point, -direction);
                }
            }
        }
    }

    [PunRPC]
    public void FireRPC(int weaponEntityInt, Vector3 bullet_pos, Vector3 bullet_dir)
    {
        EntityType weaponEntity = (EntityType)weaponEntityInt;
        OnShotEffects(weaponEntity);
       
        Weapon weaponToFireWith = WeaponManager.Singleton().GetWeapon(weaponEntity);
        
        if(weaponToFireWith != null)
        {
        
            // if (weaponToFireWith.fireAudioClip != null)
            // {
            //     gunAudio.pitch = UnityEngine.Random.Range(0.8f, 1.1f);
            //     gunAudio.PlayOneShot(weaponToFireWith.fireAudioClip, 1);
            // }
            
           
            Vector3 dump_modifed_dir;
            ShootActualBullets(weaponEntity, bullet_pos, bullet_dir, out dump_modifed_dir, false);

        }
        else
        {
            InGameConsole.LogWarning(string.Format("!!!!! WeaponToFireWith is null.  Object: <color=red>{}</color>", this.gameObject.name));
        }
    }
    
    
    void OnShotEffects(EntityType weaponEntity)
    {
         Weapon weaponToFireWith = WeaponManager.Singleton().GetWeapon(weaponEntity);
        
         switch(weaponEntity)
            {
                case EntityType.GLOCK_ENTITY:
                {
                    playerAnimController.DoPistolRecoilOnce();
                    bonesToShake.MakeTrauma(0.33f);
                    
                    // glockFireVFX.Play();
                    break;
                }
                case EntityType.SHOTGUN_ENTITY:
                {
                    playerAnimController.DoShotgunRecoilOnce();
                    bonesToShake.MakeTrauma(0.33f);
                    
                    
                    if (weaponToFireWith.fireAudioClip != null)
                    {
                        // gunAudio.pitch = 1f;
                        gunAudio.PlayOneShot(weaponToFireWith.fireAudioClip, 1);
                    }
                    
                    // shotgunFire_PS.Play();
                    
                    if(photonView.IsMine)
                    {
                        FollowingCamera.ShakeY(UnityEngine.Random.Range(8.5f, 9.25f));
                    }
                    
                    break;
                }
                case EntityType.CROSSBOW_ENTITY:
                {
                    playerAnimController.DoShotgunRecoilOnce();
                    bonesToShake.MakeTrauma(0.33f);
                    
                    break;
                }
                case EntityType.HEAVYBOLTER_ENTITY:
                {
                    playerAnimController.DoBolterRecoilOnce();
                    bonesToShake.MakeTrauma(0.25f);
                    
                    if (weaponToFireWith.fireAudioClip != null)
                    {
                        //gunAudio.pitch = 0.5f;
                        gunAudio.PlayOneShot(weaponToFireWith.fireAudioClip, 0.75f);
                    }
                    
                    // shotgunFire_PS.Play();
                    
                    if(photonView.IsMine)
                    {
                        FollowingCamera.ShakeY(UnityEngine.Random.Range(6f, 8.5f));
                    }
                    
                    break;
                }
                case EntityType.REVOLVER_ENTITY:
                {
                    playerAnimController.DoBolterRecoilOnce();
                    bonesToShake.MakeTrauma(0.25f);
                    
                    if (weaponToFireWith.fireAudioClip != null)
                    {
                        // gunAudio.pitch = 0.5f;
                        gunAudio.PlayOneShot(weaponToFireWith.fireAudioClip, 0.4f);
                    }
                    
                    // shotgunFire_PS.Play();
                    
                    if(photonView.IsMine)
                    {
                        FollowingCamera.ShakeY(UnityEngine.Random.Range(8f, 9f));
                    }
                    
                    break;
                }
                case EntityType.ROCKETLAUNCHER_ENTITY:
                {
                    playerAnimController.DoBolterRecoilOnce();
                    bonesToShake.MakeTrauma(0.33f);
                    
                    if (weaponToFireWith.fireAudioClip != null)
                    {
                        // gunAudio.pitch = 0.5f;
                        gunAudio.PlayOneShot(weaponToFireWith.fireAudioClip, 0.3f);
                    }
                    
                    // shotgunFire_PS.Play();
                    
                    if(photonView.IsMine)
                    {
                        FollowingCamera.ShakeY(UnityEngine.Random.Range(9f, 10f));
                    }
                    
                    break;
                }
                default:
                {
                    break;
                }
            }
    }

    
    public bool FireLocally(EntityType weaponEntity, Vector3 bullet_pos, Vector3 bullet_dir, out Vector3 dir_modified)
    {
        dir_modified = bullet_dir;
        Weapon currentWeapon = WeaponManager.Singleton().GetWeapon(weaponEntity);
        if (currentWeapon == null || weaponTimer != 0f)
            return false;
         
        
        if(currentWeapon.fireAudioClip != null)
        {
            
            gunAudio.PlayOneShot(currentWeapon.fireAudioClip, 1f);
        }
        
        ShootActualBullets(weaponEntity, bullet_pos, bullet_dir, out dir_modified, true);
        
        weaponTimer = 0.01f;
        DrawUI();
        
        OnShotEffects(weaponEntity);
        
        return true;
    }
    
    PlayerController playerController;

    public void Tick()
    {
        if(photonView.IsMine)
        {
            WeaponClock();
            if(playerController.isAlive && playerController.aliveState != PlayerAliveState.Dashing)
            {
                ScanForPickupables();
                
                if(Input.GetMouseButton(0))
                {
                    // Vector3 bullet_pos = muzzlePistol.position;
                    Vector3 bullet_pos = playerController.GetFPSBulletStartPos();
                    Weapon weaponInHands = GetWeaponInSlot();
                    
                    EntityType weaponInHandsEntity = EntityType.UNDEFINED_ENTITY;
                    if(weaponInHands)
                    {
                        weaponInHandsEntity = weaponInHands.ent_type;
                    }
                    
                    switch(weaponInHandsEntity)
                    {
                        case(EntityType.GLOCK_ENTITY):
                        {
                            // bullet_pos = muzzlePistol.position;
                            bullet_pos = playerController.GetFPSBulletStartPos();
                            
                            break;
                        }
                        case(EntityType.SHOTGUN_ENTITY):
                        {
                            // bullet_pos = muzzleShotgun.position;
                            bullet_pos = playerController.GetFPSBulletStartPos();
                            
                            break;
                        }
                        case(EntityType.CROSSBOW_ENTITY):
                        {
                            // bullet_pos = muzzleShotgun.position;
                            bullet_pos = playerController.GetFPSBulletStartPos();
                            
                            break;
                        }
                        case(EntityType.HEAVYBOLTER_ENTITY):
                        {
                            // bullet_pos = muzzleShotgun.position;
                            bullet_pos = playerController.GetFPSBulletStartPos();
                            
                            break;
                        }
                        default:
                        {
                            break;
                        }
                    }
                    
                    
                    // Vector3 bullet_dir = muzzlePistol.forward;
                    // Vector3 bullet_dir = playerController.lookDirection;
                    Vector3 bullet_dir = playerController.GetFPSDirection();
                    
                    
                    if(GetWeaponInSlot() != null)
                    {
                        EntityType weapon_ent = GetWeaponInSlot().ent_type; 
                        Vector3 dump_dir_modified;
                        
                       
                        if(FireLocally(weapon_ent, bullet_pos, bullet_dir, out dump_dir_modified))
                        {
                            //photonView.RPC("FireRPC", RpcTarget.Others, (int)weapon_ent, bullet_pos, bullet_dir);
                            photonView.RPC("FireRPC", RpcTarget.Others, (int)weapon_ent, bullet_pos, dump_dir_modified);
                        }
                    }
                }

                // if(Input.GetKeyDown(KeyCode.R))
                // {
                //     Reload();
                // }

                if(Input.GetKeyDown(KeyCode.Q))
                {
                    SwitchSlot();
                    WieldWeapon(GetWeaponInSlot());
                }
            }
        }
    }
    

    [SerializeField] float timeWhenCanFire = 0f;

    void WeaponClock()
    {
        if (weaponTimer == 0f)
            return;
        
        if (GetWeaponInSlot() != null && weaponTimer >= GetWeaponInSlot().FireRate && UberManager.TimeSinceStart() > timeWhenCanFire)
        {
            weaponTimer = 0f;
            return;
        }

        weaponTimer += UberManager.DeltaTime();
    }

    void DrawUI()
    {
        if(RedrawUIAction != null)
            RedrawUIAction();
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(gizmoPos, 0.7f);
    }
#endif

}
