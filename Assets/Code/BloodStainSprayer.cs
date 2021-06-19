using UnityEngine;

public class BloodStainSprayer : MonoBehaviour
{
    Transform thisTransform;
    
    void Awake()
    {
        thisTransform = transform;
        if(groundMask == -1)
        {
            groundMask = LayerMask.GetMask("Ground", "Ceiling");
        }
    }
    
    bool isWorking = false;
    float timer = 0;
    
    int rays_checked;
    
    public void MakeStains(Vector3 pos)
    {
        thisTransform.localPosition = pos;
        
        isWorking = true;
        rays_checked = 0;
        timer = 0;
    }
    
    const float ray_radius = 7F;
    const float ray_freq = 0.12F;
    const int stains_per_make = 16;
    static int groundMask = -1;
    
    const int stainChance = 3;
    
    ObjectPoolKey GetRandomStainPoolKey()
    {
        int stain_index = Random.Range(0, 4);
                    
        ObjectPoolKey stain_key = ObjectPoolKey.Blood_stain1;
        switch(stain_index)
        {
            case(0):
            {
                stain_key = ObjectPoolKey.Blood_stain1;
                break;
            }
            case(1):
            {
                stain_key = ObjectPoolKey.Blood_stain2;
                break;
            }
            case(2):
            {
                stain_key = ObjectPoolKey.Blood_stain3;
                break;
            }
            case(3):
            {
                stain_key = ObjectPoolKey.Blood_stain4;
                break;
            }
        } 
        
        return stain_key;
    }
    
    void Update()
    {
        if(isWorking)
        {
            float dt = UberManager.DeltaTime();
            
            timer += dt;
            
            if(timer > ray_freq)
            {
                timer = 0;
                rays_checked++;
                
                int roll = Random.Range(0, 10);
                if(roll <= stainChance)
                {
                    RaycastHit hit;
                    Ray ray = new Ray(thisTransform.localPosition, Random.onUnitSphere);
                    
                    if(Physics.Raycast(ray, out hit, ray_radius, groundMask))
                    {
                        ObjectPoolKey stain_key = GetRandomStainPoolKey();
                        
                        GameObject stain = ObjectPool.s().Get(stain_key, false);
                        
                        stain.transform.localPosition = hit.point + hit.normal * Random.Range(-0.05f, 0.075f);
                        stain.transform.up = hit.normal;
                        stain.transform.localScale = new Vector3(Random.Range(0.7f, 1.7f), 1, Random.Range(0.7f, 1.7f));
                    }
                }
            }
            
            if(rays_checked >= stains_per_make)
            {
                isWorking = false;
            }
        }
    }
    
}
