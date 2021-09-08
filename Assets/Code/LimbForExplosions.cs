using UnityEngine;

public class LimbForExplosions : MonoBehaviour
{
    public DamagableLimb[] limbs;
    
    static Vector3 vForward = new Vector3(0, 0, 1);
    public bool canBeAffected = true;
    
    void Awake()
    {
        limbs = GetComponentsInChildren<DamagableLimb>();
    }
    
    public void ExplodeEveryLimb()
    {
        if(!canBeAffected)
        {
            return;
        }
        
        
        int len = limbs.Length;
        
        for(int i = 0; i < len; i++)
        {
            Vector3 pos = limbs[i].transform.position;
            // InGameConsole.LogFancy("Exploding limb");
            ParticlesManager.PlayPooled(ParticleType.limb_explode_ps, pos, vForward);
        }
    }
    
    public void OnExplodeAffected()
    {
        if(!canBeAffected)
        {
            return;
        }
        
        if(limbs != null)
        {
            int len = limbs.Length;
            for(int i = 0; i < len; i++)
            {
                limbs[i].ReactWithoutPos();
            }
        }
    }
}
