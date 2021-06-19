using System.Collections.Generic;
using UnityEngine;


public enum ParticleType : int
{
    undefined,
    dust,
    shot,
    impact,
    explosion1,
    bomb1_explosion,
    bullet_decal1,
    bolt_col,
    bomb2_explosion,
    player_particle_trail,
    bomb1_pt,
    bolt_pt,
    player_spawn_ps,
    platform_effect1,
    shot_explosion1,
    npc1_particle_trail,
    on_item_pickup_ps,
    hurt1_ps,
    bullet_timeout_1,
    npc_spawned_1_ps,
    annihilate_1_ps,
    explosion_4,
    dust_impact_1,
    rocket_trail_ps,
    groundSlam1_ps,
    limb_explode_ps,
    limb_headshot_ps,
    shot_sphere1_ps,
    shot_star_ps,
    blood_cloud1,
    revolver_alt,
    revolver_alt_bullet_impact,
    star_ps,
    blink1_ps,
    shot2_big,
    punch_impact1,
    daggerShot_ps,
    gibs1_ps,
    shotStar_ps,
    bullet_impact_dusty,
    bullet_onReflect
}

public enum LineType : int
{
    revolverShot1
}


[System.Serializable]
public struct ParticleToPool
{
    public string name;
    public ParticleType type;
    public GameObject prefab;
    public int quantity;
}


public class ParticleQueue
{
    public int key;
    public int lastIndex;
    public int len;
    
    public GameObject[] gameObj;
    public Transform[] transform;
    public ParticleSystem[] ps;
}

[System.Serializable]
public struct LineRendererToPool
{
    public string name;
    public LineType type;
    public GameObject prefab;
    public int quantity;
}

public class LineRendererQueue
{
    public int key;
    public int lastIndex;
    public int len;
    
    public GameObject[] gameObj;
    public Transform[] transform;
    public LineRenderer[] lr;
}

public class ParticlesManager : MonoBehaviour
{
    
    
    [Header("Dust particle:")]
    public GameObject dustParticlePrefab;
    ParticleSystem[] dustParticleSystems;
    int dustParticleIndex = 0;
    const int dustParticlesNum = 8;
    
    
    [Header("Shot particle:")]
    public GameObject shotParticlePrefab;
    ParticleSystem[] shotParticleSystems;
    int shotParticleIndex = 0;
    const int shotParticlesNum = 64;
    
    [Header("Impact particle:")]
    public GameObject impactParticlePrefab;
    ParticleSystem[] impactParticleSystems;
    int impactParticleIndex = 0;
    const int impactParticlesNum = 5;
    
    [Header("Explosion1 particle:")]
    public GameObject explosion1_ParticlePrefab;
    ParticleSystem[] explosion1_ParticleSystems;
    int explosion1_ParticleIndex = 0;
    const int explosion1_ParticlesNum = 1;
    
    [Header("Bomb explosion 1")]
    public GameObject bomb_explosion1_prefab;
    ParticleSystem[] bomb_explosion1_ParticleSystems;
    int bomb_explosion1_ParticleIndex = 0;
    const int bomb_explosion1_ParticlesNum = 28;
    
    
    
    [Header("Pooled particles:")]
    public ParticleToPool[] particleToPool_originals;
    
    public Dictionary<int, ParticleQueue> particle_pool;
    
    [Header("Pooled Line renderers:")]
    public LineRendererToPool[] LRs_originals;
    Dictionary<int, LineRendererQueue> lrs_pool;
    
    void InitPooledLRs()
    {
        lrs_pool = new Dictionary<int, LineRendererQueue>();
        
        int len = LRs_originals.Length;
        
        for(int i = 0; i < len; i++)
        {
            int quantity = LRs_originals[i].quantity;
            int key = (int) LRs_originals[i].type;
#if UNITY_EDITOR            
            string name = LRs_originals[i].prefab.name;
#endif
            
            if(!lrs_pool.ContainsKey(key))
            {
                LineRendererQueue lrQueue = new LineRendererQueue();
                lrQueue.key       = key;
                lrQueue.lastIndex = 0;
                lrQueue.len       = quantity;
                lrQueue.gameObj = new GameObject[quantity];
                lrQueue.transform = new Transform[quantity];
                lrQueue.lr        = new LineRenderer[quantity];
                
                
                for(int j = 0; j < quantity; j++)
                {
                    GameObject decalCopy = Instantiate(LRs_originals[i].prefab, new Vector3(2000, 3000, 2000), Quaternion.identity, this.transform);
                    
#if UNITY_EDITOR
                    decalCopy.name = name + "_" + j.ToString();
#endif
                    
                    lrQueue.gameObj[j]   = decalCopy;       
                    lrQueue.transform[j] = decalCopy.transform;
                    lrQueue.lr[j]        = decalCopy.GetComponent<LineRenderer>();
                }
                
                lrs_pool.Add(key, lrQueue);
                // GameObject[] copies = new GameObject[quantity];
            }
        }
    }
    
    void InitPooledParticles()
    {
        particle_pool = new Dictionary<int, ParticleQueue>();
        
        int len = particleToPool_originals.Length;
        
        for(int i = 0; i < len; i++)
        {
            int quantity = particleToPool_originals[i].quantity;
            int key = (int) particleToPool_originals[i].type;
#if UNITY_EDITOR            
            string name = particleToPool_originals[i].prefab.name;
#endif
            
            if(!particle_pool.ContainsKey(key))
            {
                ParticleQueue particleQueue = new ParticleQueue();
                particleQueue.key       = key;
                particleQueue.lastIndex = 0;
                particleQueue.len       = quantity;
                particleQueue.gameObj = new GameObject[quantity];
                particleQueue.transform = new Transform[quantity];
                particleQueue.ps        = new ParticleSystem[quantity];
                
                
                for(int j = 0; j < quantity; j++)
                {
                    GameObject decalCopy = Instantiate(particleToPool_originals[i].prefab, new Vector3(2000, 3000, 2000), Quaternion.identity, this.transform);
                    
#if UNITY_EDITOR
                    decalCopy.name = name + "_" + j.ToString();
#endif
                    
                    particleQueue.gameObj[j]   = decalCopy;       
                    particleQueue.transform[j] = decalCopy.transform;
                    particleQueue.ps[j]        = decalCopy.GetComponent<ParticleSystem>();
                }
                
                particle_pool.Add(key, particleQueue);
                // GameObject[] copies = new GameObject[quantity];
            }
        }
    }
    
    public static ParticleTrail SetParticleTrail(ParticleType type, Transform target, float minDistance, int count_per_emit)
    {
        return Instance._SetParticleTrail(type, target, minDistance, count_per_emit);
    }
    
    public ParticleTrail _SetParticleTrail(ParticleType type, Transform target, float minDistance, int count_per_emit)
    {
        int key = (int)type;
        
        if(particle_pool.ContainsKey(key))
        {
            int lastIndex = particle_pool[key].lastIndex;
            
            ParticleTrail pt = particle_pool[key].ps[lastIndex].GetComponent<ParticleTrail>();
            
            pt.SetTrail(target, minDistance, count_per_emit);
            
            // particle_pool[key].transform[lastIndex].localPosition = pos;
            // particle_pool[key].transform[lastIndex].forward = dir;
            
            
            
            // particle_pool[key].ps[lastIndex].Play();
            
            lastIndex++;
            if(lastIndex >= particle_pool[key].len)
            {
                lastIndex = 0;
            }
            
            particle_pool[key].lastIndex = lastIndex;
            
            return pt;
        }
        else
        {
            InGameConsole.LogError(string.Format("_SetParticleTrail(): <color=yellow>Particle pool</color> does not contain {0} key", type));
            return null;
        }
    }
    
    public static GameObject GetNextPooledParticleSystem(ParticleType type)
    {
        return Instance._GetNextPooledParticleSystem(type);
    }
    
    GameObject _GetNextPooledParticleSystem(ParticleType type)
    {
        int key = (int)type;
        
        if(particle_pool.ContainsKey(key))
        {
            int lastIndex = particle_pool[key].lastIndex;
            
            return particle_pool[key].transform[lastIndex].gameObject;
        }
        else
        {
            return null;
        }
    }
    
    public static ParticleSystem PlayPooled(ParticleType type, Vector3 pos, Vector3 forwardDir)
    {
        return Instance._PlayPooled((int)type, pos, forwardDir);
    }
    
    public static ParticleSystem PlayPooledUp(ParticleType type, Vector3 pos, Vector3 upDir)
    {
        return Instance._PlayPooledUp((int)type, pos, upDir);
    }
    
    public ParticleSystem _PlayPooledUp(int type, Vector3 pos, Vector3 upDir)
    {
        ParticleSystem result = null;
        
        int key = (int)type;
        
        if(particle_pool.ContainsKey(key))
        {
            int lastIndex = particle_pool[key].lastIndex;
            
            
            particle_pool[key].transform[lastIndex].localPosition = pos;
            particle_pool[key].transform[lastIndex].up = upDir;
            
            
            
            result = particle_pool[key].ps[lastIndex];
            particle_pool[key].ps[lastIndex].Play();
            
            lastIndex++;
            if(lastIndex >= particle_pool[key].len)
            {
                lastIndex = 0;
            }
            
            particle_pool[key].lastIndex = lastIndex;
            
        }
        else
        {
            InGameConsole.LogError(string.Format("<color=yellow>Particle pool</color> does not contain {0} key", type));
        }
        
        return result;
    }
    
    public ParticleSystem _PlayPooled(int type, Vector3 pos, Vector3 forwardDir)
    {
        ParticleSystem result = null;
        
        int key = (int)type;
        
        if(particle_pool.ContainsKey(key))
        {
            int lastIndex = particle_pool[key].lastIndex;
            
            
            particle_pool[key].transform[lastIndex].localPosition = pos;
            
            particle_pool[key].transform[lastIndex].forward = forwardDir;
            
            
            
            result = particle_pool[key].ps[lastIndex];
            particle_pool[key].ps[lastIndex].Play();
            
            lastIndex++;
            if(lastIndex >= particle_pool[key].len)
            {
                lastIndex = 0;
            }
            
            particle_pool[key].lastIndex = lastIndex;
            
        }
        else
        {
            InGameConsole.LogError(string.Format("<color=yellow>Particle pool</color> does not contain {0} key", type));
        }
        
        return result;
    }
    
    
    public static ParticlesManager Instance;
    
    void Awake()
    {
        if(Instance == null)
        {
          
            
            Instance = this;    
            dustParticleSystems = new ParticleSystem[dustParticlesNum];
            
            for(int i = 0; i < dustParticleSystems.Length; i++)
            {
                GameObject particleSystemCopy = Instantiate(dustParticlePrefab, Vector3.one * 100f, Quaternion.identity, this.transform);
                dustParticleSystems[i] = particleSystemCopy.GetComponent<ParticleSystem>();            
            }
            
            
            shotParticleSystems = new ParticleSystem[shotParticlesNum];
            
            for(int i = 0; i < shotParticleSystems.Length; i++)
            {
                GameObject particleSystemCopy = Instantiate(shotParticlePrefab, Vector3.one * 100f, Quaternion.identity, this.transform);
                shotParticleSystems[i] = particleSystemCopy.GetComponent<ParticleSystem>();            
            }
            
            impactParticleSystems = new ParticleSystem[impactParticlesNum];
            
            for(int i = 0; i < impactParticleSystems.Length; i++)
            {
                GameObject particleSystemCopy = Instantiate(impactParticlePrefab, Vector3.one * 100f, Quaternion.identity, this.transform);
                impactParticleSystems[i] = particleSystemCopy.GetComponent<ParticleSystem>();            
            }
            
            explosion1_ParticleSystems = new ParticleSystem[explosion1_ParticlesNum];
            
            for(int i = 0; i < explosion1_ParticleSystems.Length; i++)
            {
                GameObject particleSystemCopy = Instantiate(explosion1_ParticlePrefab, Vector3.one * 100f, Quaternion.identity, this.transform);
                explosion1_ParticleSystems[i] = particleSystemCopy.GetComponent<ParticleSystem>();            
            }
            
            bomb_explosion1_ParticleSystems = new ParticleSystem[bomb_explosion1_ParticlesNum];
            
            for(int i = 0; i < bomb_explosion1_ParticleSystems.Length; i++)
            {
                GameObject particleSystemCopy = Instantiate(bomb_explosion1_prefab, Vector3.one * 100f, Quaternion.identity, this.transform);
                bomb_explosion1_ParticleSystems[i] = particleSystemCopy.GetComponent<ParticleSystem>();            
            }
            
//            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
    
    void Start()
    {
        InitPooledParticles();
        // InitPooledLRs();
    }
    
    
    public static void Play(ParticleType type, Vector3 position, Vector3 forwardDirection)
    {
        Instance.PlayParticle((int)type, ref position, ref forwardDirection);
    }
    
    public static void Play(int particleSystemIndex, Vector3 position, Vector3 faceDirection)
    {
        Instance.PlayParticle(particleSystemIndex, ref position, ref faceDirection);
    }
       
    
    public void PlayParticle(int particleSystemIndex, ref Vector3 position, ref Vector3 faceDirection)
    {
        switch(particleSystemIndex)
        {
            case 1: // dust
            {
                ParticleSystem ps = dustParticleSystems[dustParticleIndex];
                ps.Clear();
                ps.transform.localPosition = position;
                ps.transform.forward = faceDirection;
                
                ps.Play();
                
                
                dustParticleIndex++;
                dustParticleIndex = (dustParticleIndex >= dustParticlesNum) ? 0 : dustParticleIndex;
                break;
            }
            case 2: // shot
            {
                ParticleSystem ps = shotParticleSystems[shotParticleIndex];
                ps.Clear();
                ps.transform.localPosition = position;
                ps.transform.forward = faceDirection;
                
                ps.Play();
                
                shotParticleIndex++;
                shotParticleIndex = (shotParticleIndex >= shotParticlesNum) ? 0 : shotParticleIndex;

                
                break;
            }
            case 3: //impact
            {
                ParticleSystem ps = impactParticleSystems[impactParticleIndex];
                ps.Clear();
                ps.transform.localPosition = position;
                ps.transform.forward = faceDirection;
                
                ps.Play();
                
                impactParticleIndex++;
                impactParticleIndex = (impactParticleIndex >= impactParticlesNum) ? 0 : impactParticleIndex;

                
                break;
            }
            case 4:
            {
                ParticleSystem ps = explosion1_ParticleSystems[impactParticleIndex];
                ps.Clear();
                ps.transform.localPosition = position;
                ps.transform.forward = faceDirection;
                
                ps.Play();
                
                explosion1_ParticleIndex++;
                explosion1_ParticleIndex = (explosion1_ParticleIndex >= explosion1_ParticlesNum) ? 0 : explosion1_ParticleIndex;
                
                break;
            }
            case 5:
            {
                ParticleSystem ps = bomb_explosion1_ParticleSystems[bomb_explosion1_ParticleIndex];
                ps.Clear();
                ps.transform.localPosition = position;
                ps.transform.forward = faceDirection;
                
                ps.Play();
                
                bomb_explosion1_ParticleIndex++;
                bomb_explosion1_ParticleIndex = (bomb_explosion1_ParticleIndex >= bomb_explosion1_ParticlesNum) ? 0 : bomb_explosion1_ParticleIndex;
                
                break;
            }
            default:
            {
                InGameConsole.LogError(string.Format("You asking to play particles that are not handled ! <color=red>Partile number: {0}</color>" ,particleSystemIndex));
                break;
            }
        }
    }    
    
}
