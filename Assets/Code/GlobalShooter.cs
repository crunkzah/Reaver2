using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;


public enum GlobalShooter_ability : int
{
    Single_shot,
    Default,
    Spawn_effect,
    Spawn_npc,
    shoot_1_quad,
    Shoot_bullet_1,
    Kaboom1,
}


public class GlobalShooter : MonoBehaviour, INetworkObject
{
    static GlobalShooter _instance;
    
    public static GlobalShooter Singleton()
    {
        if(_instance == null)
        {
            _instance = FindObjectOfType<GlobalShooter>();
            
        }
        
        return _instance;
    }
    
    public NetworkObject net_comp;
    
    void Awake()
    {
        net_comp = GetComponent<NetworkObject>();
        bulletCollisionMaskEnemy = LayerMask.GetMask("Default", "Ground", "Player", "Fadable");
        hostileNPCMask = LayerMask.GetMask("NPC2");
    }
    
    readonly static Vector3 vForward = new Vector3(0, 0, 1f);
    
    public void ReceiveCommand(NetworkCommand command, params object[] args)
    {
        switch(command)
        {
            case(NetworkCommand.GroundSlam):
            {
                Vector3 slamPos = (Vector3)args[0];
                float timeWhenSlam = (float)args[1];
                
                MakeGroundSlam(slamPos, timeWhenSlam);
                break;
            }
            case(NetworkCommand.Ability1):
            {
                int shot_type = (int)args[0];
                Vector3 pos = (Vector3)args[1];
                Vector3 dir = (Vector3)args[2];
                
                MakeFancyShooting(shot_type, pos, dir);
                
                break;
            }
            case(NetworkCommand.GlobalCommand): // This is used to time for spawning npcs:
            {
                int _int_cmd_type = (int)args[0];
                GlobalShooter_ability _ability = (GlobalShooter_ability)_int_cmd_type;
                
                switch(_ability)
                {
                    case(GlobalShooter_ability.Spawn_effect):
                    {
                        Vector3 effect_pos = (Vector3)args[1];
                        ParticlesManager.PlayPooled(ParticleType.bomb2_explosion, effect_pos, vForward);
                        
                        break;
                    }
                    case(GlobalShooter_ability.Spawn_npc): // poolkey, pos
                    {
                        ObjectPoolKey obj_key = (ObjectPoolKey)(int)(args[1]);
                        Vector3 spawn_pos = (Vector3)args[2];
                        Vector3 spawn_dir = (Vector3)args[3];
                        
                        NavMeshHit navMeshHit;
                                        
                        if(NavMesh.SamplePosition(spawn_pos, out navMeshHit, 25f, NavMesh.AllAreas))
                        {
                            Vector3 finalPos = navMeshHit.position;
                            NetworkObjectsManager.SpawnNewObject(obj_key, finalPos, spawn_dir);
                        }
                        
                        break;
                    }
                    case(GlobalShooter_ability.shoot_1_quad): // poolkey, pos
                    {
                        Vector3 pos = (Vector3)args[1];
                        Shoot_1_quad(pos);
                        
                        
                        break;
                    }
                    case(GlobalShooter_ability.Shoot_bullet_1):
                    {
                        
                        break;
                    }
                    case(GlobalShooter_ability.Kaboom1):
                    {
                        Vector3 pos = (Vector3)args[1];
                        
                        
                        Kaboom1(pos);
                        
                        break;
                    }
                    default:
                    {
                        break;
                    }
                }            
                
                break;
            }
        }
    }
    
    void Kaboom1(Vector3 pos)
    {
        // AudioManager.PlayClip(SoundType.Explosion_1, 0.8f, 1);
        
        GameObject obj = ObjectPool.s().Get(ObjectPoolKey.Kaboom1, false);
        obj.GetComponent<Kaboom1>().Explode(pos);
        // FollowingCamera.ShakeY(Random.Range(6.5f, 8f));
        FollowingCamera.ShakeY(14f);
    }
    
    
    void MakeFancyShooting(int type, Vector3 pos, Vector3 dir)
    {
        GlobalShooter_ability ability = (GlobalShooter_ability)type;
        switch(ability)
        {
            case GlobalShooter_ability.Single_shot:
            {
                SingleShooting(pos, dir);
                break;
            }
        }
    }
    
    void Shoot_bullet_1(Vector3 pos, Vector3 dir)
    {
        ObjectPoolKey obj_key = ObjectPoolKey.Bullet_npc2;
        BulletController bc;
        GameObject obj;
        obj = ObjectPool.s().Get(obj_key);
        
        bc = obj.GetComponent<BulletController>();
        bool isBulletMine = PhotonNetwork.IsMasterClient;
        
//        bc.LaunchAsSphere(pos, dir, 0.3f, bulletCollisionMaskEnemy, 10, isBulletMine);
        AudioManager.PlayClip(SoundType.shoot_npc_1, 0.7f, 0.7f);
    }
    
    
    void Shoot_1_quad(Vector3 pos)
    {
        ObjectPoolKey obj_key = ObjectPoolKey.Bullet_npc2;
        GameObject obj;
        Vector3 dir, bullet_pos;
        float r = 1f;
        
        int quantity = 18;
        
        float angleStep = 360f / quantity;
        dir = new Vector3(1, 0, 0);
        
        double time = UberManager.GetPhotonTimeDelayedBy(0.15f);
        
        for(int i = 0; i < quantity; i++)
        {
            obj = ObjectPool.s().Get(obj_key);
            bullet_pos = pos + dir * r;
            
            dir = Math.GetVectorRotatedXZ(dir, angleStep);
            
            
            
          
                     
            time += 0.15d;
        }
        
    }
    
    
    
    static int bulletCollisionMaskEnemy = -1;
    static int hostileNPCMask = -1;
    
    public static void MakeGibs(Vector3 pos, int quantity)
    {
        Singleton()._MakeGibs(pos, quantity);
    }
    
    Vector3 vZero = new Vector3(0, 0, 0);
    
    void _MakeGibs(Vector3 pos, int quantity)
    {
        
        Vector3 offset;
        Vector3 vel = new Vector3(0, 0, 0);
        Vector3 dir;
        
        GameObject gib;
        
        for(int i = 0; i < quantity; i++)
        {
            gib = ObjectPool.s().Get(ObjectPoolKey.FlyingGib1, false);
            offset = Random.insideUnitSphere * 1f;
            vel.y = Random.Range(3.75f, 5.5f);
            
            dir = Math.Normalized(offset);
            vel.x = dir.x * Random.Range(4.5f, 6.25f);
            vel.z = dir.z * Random.Range(4.5f, 6.25f);
            
            gib.GetComponent<FlyingGib>().Launch(pos + offset, vel);
            
        }
        
        gib = ObjectPool.s().Get(ObjectPoolKey.FlyingGib1, false);
        gib.GetComponent<FlyingGib>().Launch(pos, vZero);
    }
    
    
    //SingleShooting:
    
    
    void SingleShooting(Vector3 pos, Vector3 dir)
    {
        GameObject bullet = ObjectPool.s().Get(ObjectPoolKey.Bullet_npc2);
        BulletController bulletController = bullet.GetComponent<BulletController>();
        
        bool isBulletMine = PhotonNetwork.IsMasterClient;
        
//        bulletController.LaunchAsSphere(pos, dir, 0.3f, bulletCollisionMaskEnemy, 16f, isBulletMine);        
    }
    
    static Collider[] slammedTargets = new Collider[128];
    
    float groundSlamRadius = 8f;
    
    public void MakeGroundSlam(Vector3 pos, float timeWhenSlam)
    {
        ParticlesManager.PlayPooled(ParticleType.groundSlam1_ps, pos, Vector3.forward);
        int slammedTargetNum = Physics.OverlapSphereNonAlloc(pos, groundSlamRadius, slammedTargets, hostileNPCMask);
        
        for(int i = 0; i < slammedTargetNum; i++)
        {
            int net_id = slammedTargets[i].GetComponent<NetworkObject>().networkId;
            Vector3 force = new Vector3(0, 12, 0);
            Vector3 npcPos = slammedTargets[i].transform.position;
            NetworkObjectsManager.ScheduleCommand(net_id, timeWhenSlam, NetworkCommand.LaunchAirborne, npcPos, force);
        }
    }
    
}
