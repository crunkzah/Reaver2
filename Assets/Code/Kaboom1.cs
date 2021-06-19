using UnityEngine;
using Photon.Pun;

public class Kaboom1 : MonoBehaviour
{
    float radius = 6f;
    
    AudioSource audioSource;
    
    ParticleSystem ps;
    
    
    static int enemyExplosionMask = -1;
    
    void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        audioSource = GetComponent<AudioSource>();
        if(enemyExplosionMask == -1)
        {
            enemyExplosionMask = LayerMask.GetMask("NPC2");
        }
    }
    
    public void Explode(Vector3 pos)
    {
        DoExplosion(pos, radius, 10f);
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
    
    
    public void ExplodeDamageHostile(Vector3 pos, float radius, float force_magnitude, int dmg)
    {
        ps.Clear();
        ps.Play();
        //audioSource.Play();
        audioSource.PlayOneShot(audioSource.clip);
        
        
        
        
        
        // InGameConsole.LogFancy("ExplodeDamageHostile");
        // {
        //     targetsHit = Physics.OverlapSphereNonAlloc(pos, radius, hits, enemyExplosionMask);
        //     targetsHit = Mathf.Min(targetsHit, hits.Length);
            
            
        //     if(targetsHit > 0)
        //     {
        //         for(int i = 0; i < targetsHit; i++)
        //         {
        //             if(PhotonNetwork.IsMasterClient)
        //             {
        //                 NetworkObject net_comp = hits[i].GetComponent<NetworkObject>();
        //                 if(net_comp)
        //                 {
        //                     int npc_net_id = net_comp.networkId;
        //                     IDamagableLocal idl = net_comp.GetComponent<IDamagableLocal>();
        //                     if(idl != null)
        //                     {
        //                         if(idl.GetCurrentHP() - dmg <= 0)
        //                         {
        //                             Vector3 dieForce_dir = Math.Normalized(hits[i].ClosestPoint(pos) - pos);
        //                             NetworkObjectsManager.CallNetworkFunction(npc_net_id, NetworkCommand.DieWithForce, dmg * dieForce_dir);
        //                         }
        //                         else
        //                         {
        //                             Vector3 force = Math.Normalized(hits[i].transform.position + new Vector3(0, 2, 0) - pos) * 14;
        //                             // force.x = force.z = 0;
        //                             //force.y += 5;
        //                             Vector3 launchPos = hits[i].transform.position;
        //                             NetworkObjectsManager.CallNetworkFunction(npc_net_id, NetworkCommand.LaunchAirborne, launchPos, force);
                                    
        //                              //NetworkObjectsManager.PackNetworkCommand(npc_net_id, NetworkCommand.LaunchAirborne, force);
        //                             //  NetworkObjectsManager.PackNetworkCommand(npc_net_id, NetworkCommand.TakeDamage, dmg);
        //                         }
        //                     }
        //                 }
        //             }
                    
        //             LimbForExplosions limb_explosion = hits[i].GetComponent<LimbForExplosions>();
        //             if(limb_explosion)
        //             {
        //                 limb_explosion.OnExplodeAffected();
        //             }
                    
                  
        //         }
        //     }
        // }
        
        PlayerController local_pc = PhotonManager.GetLocalPlayer();
        if(local_pc)
        {
            float dist = Math.SqrDistance(local_pc.transform.position, pos);
            if(dist < radius)
            {
                Vector3 dir = local_pc.GetFPSPosition() - pos;
                
                dir = Math.Normalized(dir);
                
                float t = Mathf.InverseLerp(radius * 1.1f, 1.5f, dist);
                float force_magnitude_ramped = force_magnitude * t;
                
                Vector3 force = dir * force_magnitude_ramped;
                // force.y = force_magnitude * 1.5f;
                FollowingCamera.Singleton().ShakeXZ(1.5f);
                // InGameConsole.LogOrange("Explosion force: " + force);
                local_pc.BoostVelocity(force);
            }
        }
        
        transform.localPosition = pos;       
        DoLight(pos); 
    }
    
    void DoExplosion(Vector3 pos, float radius, float force_magnitude)
    {
        ps.Clear();
        ps.Play();
        // audioSource.Stop();
        audioSource.PlayOneShot(audioSource.clip);
        
        transform.localPosition = pos;
        
        PlayerController localPlayer = PhotonManager.Singleton().local_controller;
        if(localPlayer)
        {
            Vector3 localFPSPosition = localPlayer.GetFPSPosition();
            float sqrDist = Math.SqrDistance(localFPSPosition, pos);
            if(sqrDist < radius * radius)
            {
                localPlayer.TakeDamageOnline(1);
                Vector3 dir = localFPSPosition - pos;
                dir.y = 0;
                dir = Math.Normalized(dir);
                
                Vector3 force = dir * force_magnitude;
                
                FollowingCamera.Singleton().ShakeXZ(1.5f);
                
                localPlayer.BoostVelocity(force);
            }
        }
    }
    
    
#if false && UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        
        Gizmos.DrawWireSphere(transform.position, radius);
    }
#endif
    
}
