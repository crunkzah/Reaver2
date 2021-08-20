using UnityEngine;
using System.Collections.Generic;


public enum DamageReactType
{
    small_dmg,
    headShot_dmg
}

public class DamagableLimb : MonoBehaviour
{
    public NetworkObject net_comp_from_parent;
    Rigidbody rb;
    [HideInInspector] public Rigidbody[] rbs;
    [HideInInspector] public CharacterJoint[] joints;
    
    public DamageReactType react;
    Transform thisTransform;
    
    Collider col;
    
    public bool isRootLimb = false;
    public bool isHeadshot = false;
    public bool canBeDestroyed = false;
    [SerializeField]bool isMasterAlive = true;
    float timeStampWhenDead = -1;
    
    
    public void MakeLimbDead()
    {
        //InGameConsole.LogFancy(string.Format("MakeLimbDead <color=yellow>({0})</color>", this.transform.name));
        isMasterAlive = false;
        timeStampWhenDead = Time.time;
    }
    
    public bool CanBeStompedOn()
    {
        if(!canBeDestroyed)
        {
            return false;
        }
        
        if(isMasterAlive)
        {
            //InGameConsole.LogFancy(string.Format("<color=red>Can't be stomped on</color> case 1, limb: <color=yellow>{0}</color>", this.transform.name));
            return false;
        }
        
        if(Time.time > timeStampWhenDead + 0.1f)
        {
            //InGameConsole.LogFancy("<color=green>Can be stomped on</color>");   
            return true;
        }
        
        //InGameConsole.LogFancy("<color=red>Can't be stomped on</color> case 2");
        return false;
    }
    
    
    
    
    public ParticleSystem deadLimb_ps;
    
    public byte limb_id;
    public List<DamagableLimb> adjacentLimbs = new List<DamagableLimb>(4);
    
    void Awake()
    {
        thisTransform = transform;
        if(net_comp_from_parent == null)
            net_comp_from_parent = GetComponentInParent<NetworkObject>();
            
            
        col = GetComponent<Collider>();
        rb = GetComponent<Rigidbody>();
        
        
        if(thisTransform.parent != null)
        {
            DamagableLimb parentLimb = thisTransform.parent.GetComponent<DamagableLimb>();
            if(parentLimb)
            {
                adjacentLimbs.Add(parentLimb);
            }
        }
        
        int childCount = thisTransform.childCount;
        for(int i = 0; i < childCount; i++)
        {
            DamagableLimb _limb = thisTransform.GetChild(i).GetComponent<DamagableLimb>();
            if(_limb)
            {
                adjacentLimbs.Add(_limb);
            }
        }
        
        
        if(canBeDestroyed)
        {
            rbs = GetComponentsInChildren<Rigidbody>();
            joints = GetComponentsInChildren<CharacterJoint>();
        }
    }
    
    readonly static Vector3 vForward = new Vector3(0, 0, 1);
    
    public void AddForceToLimb(Vector3 force)
    {
        if(rb)
        {
            rb.AddForce(force, ForceMode.Impulse);
        }
    }
    
    public void ApplyForceToAdjacentLimbs(Vector3 force)
    {
        int len = adjacentLimbs.Count;
        for(int i = 0; i < len; i++)
        {
            adjacentLimbs[i].AddForceToLimb(force);
        }
    }
    
    public void ReactWithoutPos()
    {
        switch(react)    
        {
            case(DamageReactType.small_dmg):
            {
                ParticlesManager.PlayPooled(ParticleType.hurt1_ps, thisTransform.position, vForward);
                ObjectPool.s().Get(ObjectPoolKey.BloodSprayer, false).GetComponent<BloodStainSprayer>().MakeStains(thisTransform.position);
                
                break;
            }
            case(DamageReactType.headShot_dmg):
            {
                ParticlesManager.PlayPooled(ParticleType.blood_cloud1, thisTransform.position, vForward);
                ParticlesManager.PlayPooled(ParticleType.limb_headshot_ps, thisTransform.position, vForward);
                
                ObjectPool.s().Get(ObjectPoolKey.BloodSprayer, false).GetComponent<BloodStainSprayer>().MakeStains(thisTransform.position);
                
                break;
            }
        }
    }
    
    public int limb_HitPoints = 100;
    
    public void TakeDamageLimb(int damage)
    {
        if(!canBeDestroyed)
        {
            return;
        }
        
        limb_HitPoints -= damage;
        if(limb_HitPoints <= 0)
        {
            DestroyLimb();
        }
    }
    
    void DestroyLimb()
    {
        if(deadLimb_ps)
        {
            deadLimb_ps.Play();
        }
        
        
        canBeDestroyed = false;
        
        Vector3 limbPos = thisTransform.position;
        
        int len = rbs.Length;
        int joints_len = joints.Length;
        
        for(int i = 0; i < len; i++)
        {
            if(i < joints_len)
            {
                Destroy(joints[i]);
            }
            Destroy(rbs[i]);
        }
        
        thisTransform.localScale = new Vector3(0.001f, 0.001f, 0.001f);
        col.enabled = false;
        Destroy(col);
        
        if(isRootLimb)
        {
            Destroy(net_comp_from_parent.gameObject, 5);
        }
        
        
        //Effects:
        {
            AudioManager.Play3D(SoundType.limb_gib1, limbPos, Random.Range(0.75f, 1.05f));
            ParticlesManager.PlayPooled(ParticleType.gibs1_ps, limbPos, Vector3.forward);
            ParticlesManager.PlayPooled(ParticleType.hurt1_ps, limbPos, Vector3.forward);
            ObjectPool.s().Get(ObjectPoolKey.BloodSprayer, false).GetComponent<BloodStainSprayer>().MakeStains(limbPos);
        }
    }
    
    
    public void React(Vector3 pos, Vector3 damage_dir)
    {
        switch(react)    
        {
            case(DamageReactType.small_dmg):
            {
                ParticlesManager.PlayPooled(ParticleType.hurt1_ps, pos, -damage_dir);
                ObjectPool.s().Get(ObjectPoolKey.BloodSprayer, false).GetComponent<BloodStainSprayer>().MakeStains(pos);
                
                break;
            }
            case(DamageReactType.headShot_dmg):
            {
                ParticlesManager.PlayPooled(ParticleType.blood_cloud1, pos, -damage_dir);
                ParticlesManager.PlayPooled(ParticleType.limb_headshot_ps, pos, -damage_dir);
                
                ObjectPool.s().Get(ObjectPoolKey.BloodSprayer, false).GetComponent<BloodStainSprayer>().MakeStains(pos);
                
                break;
            }
        }
    }
}
