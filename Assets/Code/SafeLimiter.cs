using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SafeLimiter : MonoBehaviour
{
    public Transform safePlace;
    
    Bounds bounds;
    
    PlayerController local_pc;
    
    
    void Awake()
    {
        BoxCollider box = GetComponent<BoxCollider>();
        bounds = box.bounds;
        box.enabled = false;
    }
    
    float timer = 0;
    
    void Update()
    {
        if(timer == 0)
        {
            
        
            if(!local_pc)
            {
                local_pc = PhotonManager.GetLocalPlayer();
            }
            else
            {
                
                if(bounds.Contains(local_pc.GetGroundPosition()))
                {
                    timer = 0.33f;
                    local_pc.TakeDamageNonLethal(25);
                    local_pc.TeleportPlayer(safePlace.position);
                }
            }
        }
        else
        {
            float dt = UberManager.DeltaTime();
            timer -= dt;
            if(timer <= 0)
            {
                timer = 0;
            }
        }
    }
    
}
