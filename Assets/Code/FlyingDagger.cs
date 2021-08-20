using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

public enum DaggerState : int
{
    None,
    Flying,
    Nailed
}

public enum DaggerMode : int
{
    Sinclaire,
}

public class FlyingDagger : MonoBehaviour
{
    public bool isMine = false;
    public DaggerMode mode;    
    
    [Header("Clips:")]
    public AudioClip clipflyingLoop;
    public AudioClip clipOnNailed;
    public AudioClip clipNailedTimerLoop;
    public AudioClip clipExplosion;
    
    
    bool indicator_flag = true;
    public Material indicator_on_mat;
    public Material indicator_off_mat;
    
    Transform thisTransform;
    AudioSource audio_src;
    
    public ParticleSystem stuckOnWall_ps;
    public ParticleSystem stuckOnWall_blip_ps;
    
    
    static bool staticFieldsInit = false;
    static int damageCollisionMask;
    static int groundMask;
    
    static int playerLayer;
    static int npc2Layer;
    
    TrailRenderer tr;
    public ParticleSystem trail_ps;
    
    public MeshRenderer indicator_rend;
    
    void Awake()
    {
        rend = GetComponent<MeshRenderer>();
        thisTransform = transform;
        audio_src = GetComponent<AudioSource>();
        tr = GetComponentInChildren<TrailRenderer>();
        trail_ps = GetComponentInChildren<ParticleSystem>();
        
        if(!staticFieldsInit)
        {
            damageCollisionMask = LayerMask.GetMask("Player");
            groundMask = LayerMask.GetMask("Ground", "Ceiling", "InvisibleCollider");
            
            playerLayer = LayerMask.NameToLayer("Player");
            npc2Layer = LayerMask.NameToLayer("NPC2");
            
            staticFieldsInit = true;   
        }
    }
    
    
    public void OnNailedToWall()
    {
        // InGameConsole.LogFancy("OnNailedToWall()");
        if(stuckOnWall_ps)
        {
            stuckOnWall_ps.Play();
        }
        audio_src.Stop();
        
        audio_src.clip = clipNailedTimerLoop;
        audio_src.PlayDelayed(0.75f);
        audio_src.loop = true;
        
        audio_src.PlayOneShot(clipOnNailed);
    }
    
    HashSet<int> pierced_cols = new HashSet<int>();
    Collider[] exploded_cols = new Collider[64];
    
    float lifeTimer = 0;
    
    const float LifeTime = 16;
    
    Vector3 flyingDir;
    float flyingSpeed;
    float radius;
    
    int damage;
    
    DaggerState state;
    
    public float nailedExplosionDelay = 0;
    float nailedTimer = 0;
    float blip_cd = 0;
    
    const float explosion_radius = 5;
    const int explosion_damage_to_player = 20;
    
    MeshRenderer rend;
    
    void ExplodeDagger()
    {
        // rend.enabled = false;
        
        Vector3 explode_pos = thisTransform.localPosition;
        
        // //Apply damage to NPCs:
        // int len = Physics.OverlapSphereNonAlloc(explode_pos, explosion_radius, exploded_cols, npc2Layer);
        // if(len > exploded_cols.Length)
        //     len = exploded_cols.Length;
        
        // for(int i = 0; i < len; i++)
        // {
        //     Vector3 col_offset_pos = exploded_cols[i].bounds.center + exploded_cols[i].transform.localPosition;
        //     // RaycastHit hit;
            
        //     float distance = Math.Magnitude(col_offset_pos - explode_pos);
        //     // if(!Physics.Raycast(explode_pos, col_offset_pos, distance, groundMask))
        //     // {
        //         IDamagableLocal idl = exploded_cols[i].GetComponent<IDamagableLocal>();
        //         if(idl != null)
        //         {
        //             idl.TakeDamageLocally(explosion_damage, col_offset_pos, Math.Normalized(col_offset_pos - explode_pos));
        //         }
                
        //         if(isMine)
        //         {
        //             //Collider not obscured, do damage:
        //             NetworkObject net_comp = exploded_cols[i].GetComponent<NetworkObject>();
        //             if(net_comp)
        //             {
        //                 NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.TakeDamage, explosion_damage);
        //             }
        //         }
        //     // }
        // }
        
        
        //Apply damage to local player:
        // PlayerController local_pc;
        // local_pc = PhotonManager.GetLocalPlayer();
        // if(local_pc)
        // {
        //     Vector3 playerPos = local_pc.controller.ClosestPoint(explode_pos);
        //     if(Math.SqrDistance(playerPos, explode_pos) < explosion_radius * explosion_radius)
        //     {
        //         float distance = Vector3.Distance(playerPos, explode_pos);
        //         Vector3 dir = (explode_pos - playerPos).normalized;
                
        //         // if(!Physics.Raycast(explode_pos, dir, distance))
        //         // {
        //             local_pc.TakeDamage(explosion_damage);
        //             FollowingCamera.Singleton().ShakeXZ(1.5f);
        //         // }
        //     }
        // }
        
        GameObject obj = ObjectPool.s().Get(ObjectPoolKey.Kaboom1, false);
        
        bool isExplosionMine = PhotonNetwork.IsMasterClient;
        
        
        obj.GetComponent<Kaboom1>().ExplodeDamageHostile(thisTransform.localPosition, 3, 10F, 0, isExplosionMine, true, explosion_damage_to_player);
        
        audio_src.loop = false;
        //InGameConsole.LogFancy("Explode dagger!");
        
    } 
    
    public int owner_id;
    
    public void LaunchSinclaireDagger(Vector3 pos, Vector3 dir, float speed, float _radius, int _damage, bool _isMine, int _ownerID)
    {
        rend.enabled = true;
        mode = DaggerMode.Sinclaire;
        
        thisTransform.localPosition = pos;
        thisTransform.localRotation = Quaternion.LookRotation(dir, Vector3.up);
        
        flyingSpeed = speed;
        flyingDir = dir;
        radius = _radius;
        
        lifeTimer = 0;
        isMine = _isMine;
        damage = _damage;
        
        nailedTimer = 0;
        
        state = DaggerState.Flying;
        
        pierced_cols.Clear();
        pierced_cols.Add(owner_id);
        
        if(tr)
        {
            tr.Clear();
        }
        if(trail_ps)
        {
            trail_ps.Clear();
            trail_ps.Play();
        }
        if(stuckOnWall_ps)
        {
            stuckOnWall_ps.Stop();
        }
        
        audio_src.Stop();
        audio_src.pitch = 1;
        audio_src.clip = clipflyingLoop;
        audio_src.loop = true;
        audio_src.Play();
        
        indicator_flag = true;
        
    }
    
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosion_radius);
        //transform.position
    }
    
    
    // DamagableLimb limb = col.GetComponent<DamagableLimb>();
    //         if(limb)
    //         {
    //             targetNetworkObject    = limb.net_comp_from_parent;
    //             if(limb.isHeadshot)
    //             {
    //                 damage = (int)((float)damage * headshotDmgMult);
    //                 //InGameConsole.LogFancy("Headshot damage is " + dmg.ToString());
    //             }
    //             limb.React(point, damageDirection);
    //             //Debug:
    //             limb.AddForceToLimb(damage * damageDirection);
    //         }
    
    void SinclaireDaggerUpdate(float dt)
    {
        switch(state)
        {
            case(DaggerState.None):
            {
                break;
            }
            case(DaggerState.Flying):
            {
                Vector3 pos = thisTransform.localPosition;
                Vector3 updatedPos = pos + flyingSpeed * flyingDir * dt;
                
                RaycastHit groundHit;
                Ray ray = new Ray(pos, flyingDir);
                
                bool shouldBeNailed = Physics.Raycast(ray, out groundHit, flyingSpeed * dt, groundMask);
                
                if(!shouldBeNailed)
                {
                    RaycastHit hit;
                    if(Physics.SphereCast(pos, radius, flyingDir, out hit, flyingSpeed * dt, damageCollisionMask))
                    {
                        int col_id = hit.collider.GetInstanceID();
                        
                        if(pierced_cols.Contains(col_id))
                        {
                            
                        }
                        else
                        {
                            int col_layer = hit.collider.gameObject.layer;
                            
                            // if(col_layer == npc2Layer)
                            // {
                            //     pierced_cols.Add(col_id);
                            //     IDamagableLocal  idl = hit.collider.GetComponent<IDamagableLocal>();
                            //     if(idl != null)
                            //     {
                            //         idl.TakeDamageLocally(damage, hit.point, flyingDir);
                                    
                            //         if(isMine)
                            //         {
                            //             int target_hp = idl.GetCurrentHP();
                            //             int remainingHitPoints = target_hp - damage;
                            //             NetworkObject targetNetworkObject = hit.collider.GetComponent<NetworkObject>();
                            //             if(targetNetworkObject)
                            //             {
                            //                 if(remainingHitPoints <= 0)
                            //                 {
                            //                     Vector3 force = flyingDir * damage;
                                                
                            //                     //InGameConsole.LogFancy(string.Format("Trying to add force to limb {0} of {1}", limb.limb_id, targetNetworkObject.gameObject.name));
                            //                     NetworkObjectsManager.CallNetworkFunction(targetNetworkObject.networkId, NetworkCommand.DieWithForce, force);                                
                            //                 }
                            //                 else
                            //                 {
                            //                     NetworkObjectsManager.CallNetworkFunction(targetNetworkObject.networkId, NetworkCommand.TakeDamage, damage);
                            //                 }
                            //             }
                            //         }
                            //     }
                            // }
                            //else if (col_layer == playerLayer)
                            if (col_layer == playerLayer)
                            {
                                pierced_cols.Add(col_id);
                                
                                PlayerController pController = hit.collider.GetComponent<PlayerController>();
                                if(pController.pv.IsMine)
                                {
                                    pController.TakeDamage(damage);
                                }
                                
                            }
                        }
                    }
                }
                else
                {
                    updatedPos = groundHit.point - flyingDir * 0.5f;
                    state = DaggerState.Nailed;
                    nailedTimer = 0;
                    blip_cd = 0.5f;
                    
                    OnNailedToWall();
                }
                
                thisTransform.localPosition = updatedPos;
                thisTransform.Rotate(new Vector3(1, 0, 0) * 360 * 12 * dt, Space.Self);
                
                if(shouldBeNailed)
                {
                    thisTransform.forward = flyingDir;
                }
                
                lifeTimer += dt;
                
                if(lifeTimer > LifeTime)
                {
                    state = DaggerState.None;
                }
                break;
            }
            case(DaggerState.Nailed):
            {
                nailedTimer += dt;
                
                float ping_pitch = 0.25f + Mathf.Lerp(0, nailedExplosionDelay, nailedTimer) * 0.65f;
                audio_src.pitch = ping_pitch;
                if(nailedTimer > blip_cd)
                {
                    indicator_rend.sharedMaterial = indicator_flag ? indicator_on_mat : indicator_off_mat;
                    indicator_flag = !indicator_flag;
                    
                    blip_cd += 0.175f;
                    stuckOnWall_blip_ps.Play();
                }
                
                
                
                if(nailedTimer >= nailedExplosionDelay)
                {
                    ExplodeDagger();
                    state = DaggerState.None;
                }
                
                break;
            }
        }
    }
    
    
    void Update()
    {
        float dt = UberManager.DeltaTime();
        
        switch(mode)
        {
            case(DaggerMode.Sinclaire):
            {
                SinclaireDaggerUpdate(dt);
                break;
            }
            default:
            {
                break;
            }
        }
        
    }
}
