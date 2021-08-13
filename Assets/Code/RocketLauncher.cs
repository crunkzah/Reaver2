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
    
    public ParticleSystem ps_static;
    ParticleSystem.MainModule ps_static_main;
    
    void Awake()
    {
        ps_static_main = ps_static.main;
    }
    
    public void OnShoot()
    {
        speed += speedPerShoot;
        
        simSpeed_current = simSpeed_onFire;
        
        if(speed > 480f)
        {
            speed = 480f;
        }
    }
    
    float simSpeed_current = simSpeed_normal;
    const float simSpeed_normal = 0.66f;
    const float simSpeed_changeRate = 1f;
    const float simSpeed_onFire = 2.25F;
    
    
    void Update()
    {
        float dt = UberManager.DeltaTime();        
        gear.Rotate(new Vector3(0, 0, speed * dt), Space.Self);
        
        simSpeed_current = Mathf.MoveTowards(simSpeed_current, simSpeed_normal, simSpeed_changeRate * dt);
        
        ps_static_main.simulationSpeed = simSpeed_current;
        
        speed = Mathf.MoveTowards(speed, baseSpeed, dt * deccelerationRate);
    }
}
