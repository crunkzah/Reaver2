using UnityEngine;

public class HurtZone : MonoBehaviour
{
    public float force = 12;
    public int damage = 20;
    Bounds bounds;
    
    
    void Awake()
    {
        BoxCollider boxColl = GetComponentInChildren<BoxCollider>();
        bounds = boxColl.bounds;
        bounds.Expand(1.15f);
        Destroy(boxColl);
    }
    
    const float check_freq = 0.1f;
    float timer = 0;
    
    void Update()
    {
        if(timer >= check_freq)
        {
            
         
            PlayerController local_pc = PhotonManager.GetLocalPlayer();
            if(local_pc)
            {
                bool mustHurt = CheckBounds(local_pc.GetGroundPosition());
                if(mustHurt)
                {
                    local_pc.TakeDamage(damage);
                    local_pc.BoostVelocity(new Vector3(0, force, 0));
                }
            }
            
            timer = 0;
        }
        
        timer += UberManager.DeltaTime();
    }
    
    
    bool CheckBounds(Vector3 pos)
    {
        return bounds.Contains(pos);
    }
}
