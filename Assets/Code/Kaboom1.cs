using UnityEngine;
using Photon.Pun;

public class Kaboom1 : MonoBehaviour
{
    float radius = 6f;
    
    AudioSource audioSource;
    
    ParticleSystem ps;
    
    
    static int enemyExplosionMask = -1;
    static int limbExplosionMask = -1;
    
    void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        audioSource = GetComponent<AudioSource>();
        if(enemyExplosionMask == -1)
        {
            enemyExplosionMask = LayerMask.GetMask("NPC2");
            limbExplosionMask = LayerMask.GetMask("NPC");
        }
    }
    
    public void Explode(Vector3 pos)
    {
//        DoExplosion(pos, radius, 10f);
    }
    
    
    static Collider[] hits = new Collider[48];
    int targetsHit = 0;
    
    void DoLight(Vector3 pos)
    {
        GameObject g = ObjectPool2.s().Get(ObjectPoolKey.LightPooled, false);
        LightPooled light = g.GetComponent<LightPooled>();
        //Color color = Random.ColorHSV();
        Color color = new Color(1f, 0.33f, 0, 1);
        float decay_speed = 6 / 0.5f * 3;
        pos.y += 0.05F;
        light.DoLight(pos, color, 0.5f, 12, 6, decay_speed);
    }
    
    
    bool isMine = false;
    
    public bool canDamageLocalPlayer = false;
    public bool canDamageNPCs = true;
    public int explosionPlayerDamage = 5;
    
    Collider[] limbs_to_explode = new Collider[128];
    
    public void ExplodeDamageHostile(Vector3 pos, float radius, float force_magnitude, int explosionDamage, bool _isMine, bool _canDamageLocalPlayer = false, int _playerExplosionDamage = 0)
    {
        float explosion_scale = radius * explosion_default_scale;
        ps.transform.localScale = new Vector3(explosion_scale, explosion_scale, explosion_scale);
        
        canDamageLocalPlayer = _canDamageLocalPlayer;
        explosionPlayerDamage = _playerExplosionDamage;
        
        isMine = _isMine;
        ps.Clear();
        ps.Play();
        float _pitch = 1;//Random.Range(0.7f, 1.4f);
        AudioManager.MakeExplosionAt(pos, 1, _pitch);
        
        if(isMine)
        {
            //InGameConsole.LogFancy("ExplodeDamageHostile is <color=green>Mine</color>");
            targetsHit = Physics.OverlapSphereNonAlloc(pos, radius, hits, enemyExplosionMask);
            targetsHit = Mathf.Min(targetsHit, hits.Length);
            
            int slammed_limbs_len = Physics.OverlapSphereNonAlloc(pos, radius * 0.66f, limbs_to_explode, limbExplosionMask);
            for(int  j = 0; j < slammed_limbs_len; j++)
            {
                DamagableLimb limb = limbs_to_explode[j].GetComponent<DamagableLimb>();
                if(limb && limb.CanBeStompedOn())
                {
                    limb.TakeDamageLimb(3500);
              //      InGameConsole.LogFancy("ExplodingLimb");
                }
            }
            
            slammed_limbs_len = Physics.OverlapSphereNonAlloc(pos, radius * 1.2f, limbs_to_explode, limbExplosionMask);
            for(int  j = 0; j < slammed_limbs_len; j++)
            {
                Rigidbody rb;
                rb = limbs_to_explode[j].GetComponent<Rigidbody>();
                if(rb && rb.isKinematic == false)
                {
                    Vector3 dir = (rb.transform.position - pos).normalized;
                    dir += new Vector3(0, 0.1f, 0);
                    //InGameConsole.LogOrange("Trying to add force to rigidbody");
                    rb.AddForce(dir * force_magnitude * 10, ForceMode.Impulse);
                }
            }
            
            
            if(targetsHit > 0)
            {
                //InGameConsole.LogFancy(string.Format("targets hit number {0}", targetsHit));
                
                for(int i = 0; i < targetsHit; i++)
                {
                    InGameConsole.LogFancy(string.Format("Target is {0}", hits[0].gameObject.name));
                    if(isMine && canDamageNPCs)
                    {
                        NetworkObject net_comp = hits[i].GetComponent<NetworkObject>();
                        if(net_comp)
                        {
                            int npc_net_id = net_comp.networkId;
                            IDamagableLocal idl = net_comp.GetComponent<IDamagableLocal>();
                            if(idl != null)
                            {
                                if(explosionDamage > 0)
                                {
                                    Vector3 force = Math.Normalized(hits[i].transform.position + new Vector3(0, 2, 0) - pos) * 14;
                                    Vector3 launchPos = hits[i].transform.position;
                                    
                                    NetworkObjectsManager.CallNetworkFunction(npc_net_id, NetworkCommand.LaunchAirborne, launchPos, force);
                                    NetworkObjectsManager.CallNetworkFunction(npc_net_id, NetworkCommand.TakeDamageExplosive, explosionDamage);
                                }
                            }
                        }
                    }
                    
                    LimbForExplosions limb_explosion = hits[i].GetComponent<LimbForExplosions>();
                    if(limb_explosion)
                    {
                        limb_explosion.OnExplodeAffected();
                    }
                    
                    //int slammed_limbs_len = Physics.OverlapBoxNonAlloc(slamImpactPos, new Vector3(box_size_x, box_size_y, box_size_z), limbs_to_explode, thisTransform.localRotation, slamMask);
            
                    
                  
                }
            }
        }
        else
        {
            //InGameConsole.LogFancy("ExplodeDamageHostile is <color=red>not Mine</color>");
        }
        
        PlayerController local_pc = PhotonManager.GetLocalPlayer();
        if(local_pc)
        {
            Vector3 localPlayerPos = local_pc.GetFPSPosition();
            float distance = Vector3.Distance(localPlayerPos, pos);
            if(distance < radius)
            {
                Vector3 dir = local_pc.GetFPSPosition() - pos;
                
                dir = Math.Normalized(dir);
                
                float t = Mathf.InverseLerp(radius * 1.1f, 1.5f, distance);
                float force_magnitude_ramped = force_magnitude * t;
                
                Vector3 force = dir * force_magnitude_ramped;
                // force.y = force_magnitude * 1.5f;
                FollowingCamera.Singleton().ShakeXZ(1.5f);
                // InGameConsole.LogOrange("Explosion force: " + force);
                local_pc.BoostVelocity(force);
                
                float x = Mathf.InverseLerp(0, radius, distance);
                float trauma = Mathf.Lerp(1f, 3f, x);
                CameraShaker.MakeTrauma(trauma);
                
                if(canDamageLocalPlayer)
                {
                    local_pc.TakeDamage(explosionPlayerDamage);
                }
            }
        }
        
        transform.localPosition = pos;       
        DoLight(pos); 
    }
    
    const float explosion_default_scale = 1F;
    
    // void DoExplosion(Vector3 pos, float radius, float force_magnitude)
    // {
    //     float explosion_scale = radius * explosion_default_scale;
    //     ps.transform.localScale = new Vector3(explosion_scale, explosion_scale, explosion_scale);
        
    //     ps.Clear();
    //     ps.Play();
    //     // audioSource.Stop();
    //     audioSource.PlayOneShot(audioSource.clip);
        
    //     transform.localPosition = pos;
        
    //     PlayerController localPlayer = PhotonManager.Singleton().local_controller;
    //     if(localPlayer)
    //     {
    //         Vector3 localFPSPosition = localPlayer.GetFPSPosition();
    //         float distance = Vector3.Distance(localFPSPosition, pos);
    //         if(distance < radius)
    //         {
    //             localPlayer.TakeDamageOnline(1);
    //             Vector3 dir = localFPSPosition - pos;
    //             dir.y = 0;
    //             dir = Math.Normalized(dir);
                
    //             Vector3 force = dir * force_magnitude;
                
    //             FollowingCamera.Singleton().ShakeXZ(1.5f);
                
    //             localPlayer.BoostVelocity(force);
    //         }
    //     }
    // }
    
    
#if false && UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        
        Gizmos.DrawWireSphere(transform.position, radius);
    }
#endif
    
}
