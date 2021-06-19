using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightPooled : MonoBehaviour
{
    Transform thisTransform;
    new Light light;
    
    void Awake()
    {
        thisTransform = transform;
        light = GetComponent<Light>();
        light.shadows = LightShadows.None;
        EndLight();
    }
    
    float lifeTimer = 0;
    float time_to_be_alive = 0;
    float intensity = 1;
    float radius = 1;
    float decaySpeed = 1;
    float decaySpeedInitial = 1;
    Color color;
    
    public void DoLight(Vector3 pos, Color _color, float _lifeTime, float _intensity, float _radius, float _decaySpeed)
    {
        lifeTimer = _lifeTime;
        intensity = _intensity;
        radius = _radius;
        decaySpeedInitial = _decaySpeed;
        decaySpeed = _decaySpeed;
        color = _color;
        
        thisTransform.localPosition = pos;
        
        light.enabled = true;
        light.color = color;
        light.intensity = intensity;
    }
    
    
    void EndLight()
    {
        lifeTimer = 0;
        light.enabled = false;
    }
    
    void Update()
    {
        float dt = UberManager.DeltaTime();
        
        if(lifeTimer > 0)
        {
            lifeTimer -= dt;
            if(lifeTimer <= 0)
            {
                EndLight();
            }
            else
            {
                
                light.range = radius;
                if(radius == 0)
                {
                    EndLight();
                }
                radius -= decaySpeed * dt;
                decaySpeed += decaySpeedInitial * dt / 2 ;
                if(radius < 0)
                {
                    radius = 0;
                }
            }
        }
    }
    
}
