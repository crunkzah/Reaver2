using UnityEngine;
using UnityEngine.UI;

public class BloodOnCamera : MonoBehaviour
{
    
    static BloodOnCamera instance;
    public static BloodOnCamera Singleton()
    {
        if(instance == null)
        {
            instance = FindObjectOfType<BloodOnCamera>();
        }
        
        return instance;
    }
    
    float bloodTimer = 0;
    const float bloodTimeWindow = 0.25f;
    
    public static void MakeSplashOnScreen(Vector3 pos)
    {
        //return;
        Singleton()._MakeSplashOnScreen(pos);
    }   
    
    public Image[] blood_images; 
    int splashes_num;
    int prevImgIndex;
    const float blood_alpha_onSplash = 0.45f;
    const float alphaDecreaseRate = 0.72f;
    
    void Start()
    {
        splashes_num = blood_images.Length;
        for(int i = 0; i < splashes_num; i++)
        {
            blood_images[i].enabled = false;
        }
    }
    
    void Update()
    {
        float dt = UberManager.DeltaTime();
        
        bloodTimer -= dt;
        if(bloodTimer < 0)
        {
            bloodTimer = 0;
        }
        
        for(int i = 0; i < splashes_num; i++)
        {
            Color col = blood_images[i].color;
            
            col.a = Mathf.MoveTowards(col.a, 0, alphaDecreaseRate * dt);
            blood_images[i].color = col;
            
            // if(col.a <= 0)
            // {
            //     blood_images[i].enabled = false;
            //     InGameConsole.Log("<color=yellow>Disabling</color> " + blood_images[i].gameObject.name);
            // }
            //if(blood_images[i].enabled)
        }
    }
    
    PlayerController local_pc;
    const float MaxDistance = 1.6f;
    
    void _MakeSplashOnScreen(Vector3 pos)
    {
        if(!local_pc)
        {
            local_pc = PhotonManager.GetLocalPlayer();
        }
        
        if(local_pc)
        {
            Vector3 playerPos = local_pc.GetHeadPosition();
            
            float distSqr = Math.SqrDistance(playerPos, pos);
             
            //InGameConsole.LogFancy("_MakeSplashOnScreen(), distance to player: " + Mathf.Sqrt(distSqr).ToString("f"));
            
            if((distSqr < MaxDistance * MaxDistance) && (bloodTimer == 0))
            {
                bloodTimer = bloodTimeWindow;
                int rnd = Random.Range(0, splashes_num);
                if(rnd == prevImgIndex)
                {
                    rnd = Random.Range(0, splashes_num);
                }
                
                Color col = blood_images[rnd].color;
                col.a = blood_alpha_onSplash;
                blood_images[rnd].color = col;
                
                
                blood_images[rnd].enabled = true;
                InGameConsole.LogFancy("<color=red>Enabling</color> " + blood_images[rnd].gameObject.name);
                
                prevImgIndex = rnd;
            }
        }
    }
    
    
    
  
    
}
