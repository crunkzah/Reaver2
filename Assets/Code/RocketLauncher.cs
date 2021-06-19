using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketLauncher : MonoBehaviour
{
    public Transform gear;
    
    float baseSpeed = 15f;
    float speed = 45f;
    float speedPerShoot = 140f * 6;
    float deccelerationRate = 225f;
    
    public void OnShoot()
    {
        speed += speedPerShoot;
        if(speed > 480f)
        {
            speed = 480f;
        }
    }
    
    void Update()
    {
        float dt = UberManager.DeltaTime();        
        gear.Rotate(new Vector3(0, 0, speed * dt), Space.Self);
        
        speed = Mathf.MoveTowards(speed, baseSpeed, dt * deccelerationRate);
    }
}
