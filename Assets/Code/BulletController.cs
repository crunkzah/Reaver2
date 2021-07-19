using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

public enum BulletCollisionType
{
    Raycast,
    Sphere
}

public enum BulletOnDieEffects
{
    Default,
    ShootyThing1
}

public enum BulletOnDieBehaviour
{
    Default,
    Reflect,
    Explode_1,
    Hurtless
}

public class BulletController : MonoBehaviour, IPooledObject
{
    public Bullet bullet;
    
    
    public BulletOnDieEffects bulletOnDieEffects = BulletOnDieEffects.Default;
    public bool overrideParticleOnHit = false;
    public ParticleType particles_on_hit_override = ParticleType.shot;
    
    bool play_sound_on_hit = true;
    // public int ownerPhotonViewId = -1;
    NetObjectOwner netObjectOwner;
    
    public BulletOnDieBehaviour on_die_behave = BulletOnDieBehaviour.Default;
    
    int damage = 0;    

    //public Vector3 prevPosition;
    public float time_to_be_alive;
    float lifeTimer = 0f;
    
    Transform thisTransform;
    
    public ParticleSystem ps;    
    public ParticleTrail attached_particle_trail;
    
    public TrailRenderer tr;
    MeshRenderer rend;
    
    bool isWorking = false;
    bool isMine = false;
    
    int collisionMask;
    
    Vector3 fly_direction = Vector3.forward;
    float reflect_speed_before_pause;
    float currentSpeed = 10;
    
    [Header("This is very important:")]
    public float sphere_radius = 0.3f;
    [Header("----------------------:")]
    
    BulletCollisionType collision_type = BulletCollisionType.Sphere;
    
    public SoundType soundOnHit = SoundType.None;
    
    public int max_reflects = 4;
    int reflects_count = 0;
    int pierced_num = 0;
    
    
    
    

    void Awake()
    {
        thisTransform = transform;
        netObjectOwner = GetComponent<NetObjectOwner>();
        if(ps == null)
            ps = GetComponent<ParticleSystem>();
        rend = GetComponent<MeshRenderer>();
        
        if(tr == null)
            tr = GetComponent<TrailRenderer>();
    }
    
    void Start()
    {
        BulletsManager.RegisterBullet(this);
    }
    
    public void InitialState()
    {
        damage = 0;
        time_to_be_alive = GameSettings.bulletLifeTime;
        lifeTimer = 0f;
        currentSpeed = 6;
        //on_die_behave = BulletOnDieBehaviour.Default;
    }
    
    void ClearEffects()
    {
        if(ps)
        {
            ps.Clear();
            ps.Play();
        }
        
        if(tr)
        {
            tr.Clear();
            tr.emitting = true;
        }
    }
    
    void StopEffects()
    {
        if(tr)
        {
            Invoke("SET", tr.time);
            // tr.emitting = false;
            // tr.Clear();
        }
    }
    void SET() //Stop Emitting Trails
    {
        if(tr)
        {
            tr.emitting = false;
            tr.Clear();
        }
    }
    
    
    
    HashSet<int> pierced_cols = new HashSet<int>();
    HashSet<int> pierced_networkObjects = new HashSet<int>();
    
    public void LaunchAsSphere(Vector3 pos, Vector3 _direction, float _radius, int _collisionMask, float bulletSpeed, int _damage, bool _isMine)
    {
        pierced_cols.Clear();
        pierced_networkObjects.Clear();
        
        this.pierced_num = 0;
        this.damage = _damage;
        this.sphere_radius = _radius;
        this.isMine = _isMine;
        this.fly_direction = _direction;
        // this.netObjectOwner.ownerId  = _ownerId;
        this.collisionMask = _collisionMask;
        
        reflects_count = 0;
        
        thisTransform.localPosition = pos;
        thisTransform.forward = _direction;
        
        currentSpeed = bulletSpeed;
        
        collision_type = BulletCollisionType.Sphere;
        
        
        
        
        ClearEffects();
        if(rend)
        {
            //InGameConsole.LogFancy("Rend.enabled = <color=green>true</color>");   
            rend.enabled = true;
        }
        isWorking = true;
        
        
    }
    
    void OnDrawGizmosSelected()
    {
        
        // if(collision_type == BulletCollisionType.Sphere)
        // {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, sphere_radius);
        // }
    }
    
   
    
    void SphereMode(float dt)
    {
        RaycastHit hit;
        float distanceThisFrame = currentSpeed * dt;
        
        
        if(Physics.SphereCast(thisTransform.localPosition, sphere_radius, fly_direction, out hit, distanceThisFrame, collisionMask))
        {
            OnHit(hit.point, fly_direction, hit.normal, hit.collider);
        }

// #if UNITY_EDITOR
//         gizmoSphere = thisTransform.localPosition;
//         gizmoRadius = sphere_radius;
// #endif
        Vector3 newPosition = new Vector3();
        Vector3 currentPos = thisTransform.localPosition;
        
        newPosition.x = currentPos.x + fly_direction.x * distanceThisFrame;
        newPosition.y = currentPos.y + fly_direction.y * distanceThisFrame;
        newPosition.z = currentPos.z + fly_direction.z * distanceThisFrame;
        
        transform.localPosition = newPosition;
    }
    
    public void UpdateMe()
    {
        if(isWorking)
        {
            float dt = UberManager.DeltaTime();
            
            if(collision_type == BulletCollisionType.Sphere)
            {
                SphereMode(dt);
            }
            
            

            CheckLifeTimer(dt);
        }
    }
      
    void CheckLifeTimer(float dt)
    {
        if(lifeTimer >= time_to_be_alive)
        {
            RunOutOfTime();
        }

        lifeTimer += dt;
    }
    
    void RunOutOfTime()
    {
        //ParticlesManager.PlayPooled(ParticleType.bullet_timeout_1, thisTransform.localPosition, fly_direction);        
        CancelInvoke();
        isWorking = false;
        if(rend)
            rend.enabled = false;
        //this.gameObject.SetActive(false);
    }
    
    Vector3 GetBulletForce()
    {
        return this.damage * this.fly_direction * this.currentSpeed;
    }
    
    public bool isShotByPlayer = false;
    const float renderer_delay = 0.25F;
    
    
    

    void OnHit(Vector3 point, Vector3 direction, Vector3 normal, Collider col = null)
    {
        bool shouldDie = (on_die_behave == BulletOnDieBehaviour.Reflect ? false : true);
        bool shouldPlayFXonHit = true;
        
        // InGameConsole.LogFancy("OnHit()");
        
        
        if(on_die_behave == BulletOnDieBehaviour.Hurtless)
        {
            RunOutOfTime();
            //InGameConsole.LogFancy("Hurtless OnHit()");
            return;
        }
        
        
        if(on_die_behave == BulletOnDieBehaviour.Explode_1)
        {
            // if(PhotonNetwork.IsMasterClient)
            // {
                GameObject obj = ObjectPool.s().Get(ObjectPoolKey.Kaboom1, false);
                
                // float _radius = Random.Range(2, 7);
                float _radius = 6;
                
                obj.GetComponent<Kaboom1>().ExplodeDamageHostile(thisTransform.localPosition, _radius, 33F, 300, isMine);
            // }
            
            //FollowingCamera.ShakeY(13f);
        }
        else
        {
            if (col != null)
            {
                int col_instanceID = col.GetInstanceID();
                if(pierced_cols.Contains(col_instanceID))
                {
                    return;
                }
                
                
                pierced_cols.Add(col_instanceID);
                
                DamagableLimb limb = col.GetComponent<DamagableLimb>();
                NetworkObject targetNetworkObject;
                
                if(limb)
                {
                    targetNetworkObject    = limb.net_comp_from_parent;
                    
                    
                    limb.AddForceToLimb(damage * direction);
                    limb.React(point, direction);
                }
                else
                {
                    targetNetworkObject = col.GetComponent<NetworkObject>();
                }
                
                if(targetNetworkObject != null)
                {
                    if(pierced_networkObjects.Contains(targetNetworkObject.networkId))
                    {
                        return;
                    }
                    else
                    {
                        //InGameConsole.LogOrange("ADDED NETWORK OBJECT");
                        pierced_networkObjects.Add(targetNetworkObject.networkId);
                    }
                    //shouldPlayFXonHit = false;
                    IDamagableLocal idl = targetNetworkObject.GetComponent<IDamagableLocal>();
                    if(idl != null)
                    {
                        shouldPlayFXonHit = false;
                        idl.TakeDamageLocally(damage, point, fly_direction);
                        if(isMine)
                        {
                            
                            int target_hp = idl.GetCurrentHP();
                            int remainingHitPoints = target_hp - this.damage;
                            Vector3 force = fly_direction * damage;
                            if(target_hp > 0)
                            {
                                if(remainingHitPoints <= 0)
                                {
                                    // InGameConsole.LogFancy("ONE");
                                    NetworkObjectsManager.CallNetworkFunction(targetNetworkObject.networkId, NetworkCommand.DieWithForce, force, limb.limb_id);                                
                                }
                                else
                                {
                                    // InGameConsole.LogFancy("TWO");
                                   
                                    NetworkObjectsManager.CallNetworkFunction(targetNetworkObject.networkId, NetworkCommand.TakeDamage, damage);
                                }
                            }
                            else
                            {
                                if(idl.IsDead())
                                {
                                    if(limb)
                                    {
                                        // InGameConsole.LogFancy("THREE");
                                        shouldPlayFXonHit = false;
                                        limb.TakeDamageLimb(damage);
                                    }
                                }
                                else
                                {
                                    // InGameConsole.LogFancy("FOUR");
                                    NetworkObjectsManager.CallNetworkFunction(targetNetworkObject.networkId, NetworkCommand.DieWithForce, force, limb.limb_id);
                                }
                            }
                        }
                    }
                }
                else
                {
                    PlayerController player_hit = col.GetComponent<PlayerController>();
                    if(player_hit)
                    {
                        if(player_hit.CanTakeDamageFromProjectile())
                        {
                            
                            PhotonView pv = col.GetComponent<PhotonView>();
                            
                            if(pv && pv.IsMine)
                            {
                                //shouldPlayFXonHit = false;
                                col.GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.All, damage);
                            }
                        }
                        else
                        {
                            InGameConsole.LogFancy("Player immune to damage");
                            shouldDie = false;
                        }
                    }
                    else // We hit something static
                    {
                        //InGameConsole.LogFancy("OnHit() <color=yellow>static</color>");
                        if(on_die_behave == BulletOnDieBehaviour.Reflect)
                        {
                            reflects_count++;
                            
                            AudioManager.Play3D(SoundType.bullet_reflect_sound, point, Random.Range(0.9f, 1f), 0.65f, 1);
                            ParticlesManager.PlayPooled(ParticleType.bullet_onReflect, point, upDir);
                            DoLight_reflect_1(thisTransform.localPosition);
                            
                            if(reflects_count >= max_reflects)
                            {
                                RunOutOfTime();
                            }
                            else
                            {
                                //InGameConsole.LogFancy("Reflect");
                                
                                reflect_speed_before_pause = currentSpeed;
                                currentSpeed = 0;
                                
                                Invoke("R", 0.1F);
                                
                                Vector3 before_dir = fly_direction;
                                ParticlesManager.PlayPooled(ParticleType.bullet_timeout_1, point, fly_direction);
                                fly_direction = Vector3.Reflect(fly_direction, normal);
                                
                                
                                thisTransform.forward = fly_direction;
                                
                                shouldDie = false;
                            }
                        }
                    }
                }
            }
        }
        
        void DoLight_reflect_1(Vector3 pos)
        {
            GameObject g = ObjectPool2.s().Get(ObjectPoolKey.LightPooled, false);
            LightPooled light = g.GetComponent<LightPooled>();
            //Color color = Random.ColorHSV();
            Color color = new Color(1f, 0.33f, 0, 1);
            float decay_speed = 6 / 0.5f * 4;
            pos.y += 0.05F;
            float radius = 2.5f;
            light.DoLight(pos, color, 0.5f, 12, radius, decay_speed);
        }
       
        
        
        
        Vector3 impact_dir_normal = -direction;
        //impact_dir_normal = Quaternion.Euler(Random.onUnitSphere * 0.05f) * impact_dir_normal;
        
        if(shouldDie)
        {
            StopEffects();
            if(shouldPlayFXonHit)
            {
                switch(bulletOnDieEffects)
                {
                    case(BulletOnDieEffects.Default):
                    {
                        if(overrideParticleOnHit)
                        {
                            ParticlesManager.PlayPooled(particles_on_hit_override, point, impact_dir_normal);
                        }
                        else
                        {
                            ParticlesManager.Play(ParticleType.shot, point, impact_dir_normal);
                        }
                        break;
                    }
                    case(BulletOnDieEffects.ShootyThing1):
                    {
                        
                        ParticlesManager.PlayPooled(ParticleType.shot2_big, point, impact_dir_normal);
                        break;
                    }
                }
                
            }
            
            if(soundOnHit != SoundType.None)
            {
                AudioManager.Play3D(SoundType.bullet_impact_sound1, point);
            }
            if(rend)
            {
                //InGameConsole.LogFancy("Rend.enabled = <color=red>false</color>");
                rend.enabled = false;
            }
            isWorking = false;
            
            if(ps)
            {
                ps.Stop();
            }
            
            if(attached_particle_trail)
            {
                attached_particle_trail.ClearTarget();
                attached_particle_trail = null;
            }
            
            // this.gameObject.SetActive(false);
        }
    }
    
    void R()
    {
        pierced_cols.Clear();
        pierced_networkObjects.Clear();
                    
        currentSpeed = reflect_speed_before_pause;
        damage = damage + damage * 15 / 100;
    }
    static Vector3 upDir = new Vector3(0, 1, 0);
}
