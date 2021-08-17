using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectsSwitcher : MonoBehaviour
{
    public BoxCollider box;
    Bounds bounds;
    
    void Start()
    {
        bounds = box.bounds;
    }
    
    bool shouldSwitch = false;
    bool containsPlayer = false;
    
    void Switch()
    {
        
    }
    
    void Update()
    {
        PlayerController local_pc = PhotonManager.GetLocalPlayer();
        if(local_pc)
        {
            containsPlayer = bounds.Contains(local_pc.GetGroundPosition());
            if(!shouldSwitch && containsPlayer)
            {
                shouldSwitch = true;
                
            }
        }
        else
        {
            containsPlayer = false;
        }
    }
    
    
}
