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
    
    public Transform[] icons;
    MeshRenderer[] icons_renderers;
    
    void Awake()
    {
        // if(!audio_src)
        // {
        //     audio_src = GetComponent<AudioSource>();
        // }
        ps_static_main = ps_static.main;
        icons_renderers = new MeshRenderer[icons.Length];
        for(int i = 0; i < icons.Length; i++)
        {
            icons_renderers[i] = icons[i].GetComponent<MeshRenderer>();
        }
        // mains = new ParticleSystem.MainModule[magazine_status_particleSystems.Length];
        
        // for(int i = 0; i < magazine_status_particleSystems.Length; i++)
        // {
        //     mains[i] = magazine_status_particleSystems[i].main;
        // }
    }
    
    public int GetRocketsInMagazine()
    {
        return rockets_in_magazine;
    }
    
    public void OnShoot()
    {
        speed += speedPerShoot;
        rockets_in_magazine--;
        
        switch(rockets_in_magazine)
        {
            case(2):
            {
                break;
            }
            case(1):
            {
                icons_renderers[2].sharedMaterial = icon_chargingMat;
                icons[2].localScale = new Vector3(0, 0, 0);
                break;
            }
            case(0):
            {
                icons_renderers[2].sharedMaterial = icon_chargingMat;
                icons[2].localScale = new Vector3(0, 0, 0);
                icons_renderers[1].sharedMaterial = icon_chargingMat;
                icons[1].localScale = new Vector3(0, 0, 0);
                break;
            }
        }
        
        charge = 0;
        
        simSpeed_current = simSpeed_onFire;
        
        if(speed > 480f)
        {
            speed = 480f;
        }
    }
    
    float simSpeed_current = simSpeed_normal;
    const float simSpeed_normal = 0.66f;
    const float simSpeed_changeRate = 1f;
    const float simSpeed_onFire = 2.55F;
    
    const int magazine_capacity = 3;
    int rockets_in_magazine = 3;
    
    float charge = 1;
    float recharge_rate = 1f / 3f; // 1 / SECONDS
    
    const float icons_full_scale = 0.009f;
    
    public Material icon_readyMat;
    public Material icon_chargingMat;
    
    public AudioSource audio_src;
    public AudioClip onRocketAddedClip;
    
    public void CooldownsTick(float dt)
    {
        if(rockets_in_magazine < 3)
        {
            if(charge < 1f)
            {
                charge += dt * recharge_rate;
                int i = rockets_in_magazine;
                
                float _scale = Mathf.Lerp(0, icons_full_scale, charge);
                if(icons == null || icons[i] == null)
                    return;
                icons[i].localScale = new Vector3(_scale, _scale, _scale);
                
                if(charge >= 1f)
                {
                    icons_renderers[i].sharedMaterial = icon_readyMat;
                    icons[i].localScale = new Vector3(icons_full_scale, icons_full_scale, icons_full_scale);
                    
                    audio_src.pitch = 0.75f + 0.5f * Mathf.InverseLerp(1, 3, rockets_in_magazine);
                    audio_src.PlayOneShot(onRocketAddedClip, 0.85f);
                    rockets_in_magazine++;
                    charge = 0;
                }
                else
                {
                    icons_renderers[i].sharedMaterial = icon_chargingMat;
                }
            }
        }
    }
    
    
    void Update()
    {
        float dt = UberManager.DeltaTime();
        //CooldownsTick(dt);
        gear.Rotate(new Vector3(0, 0, speed * dt), Space.Self);
        
        simSpeed_current = Mathf.MoveTowards(simSpeed_current, simSpeed_normal, simSpeed_changeRate * dt);
        
        ps_static_main.simulationSpeed = simSpeed_current;
        
        speed = Mathf.MoveTowards(speed, baseSpeed, dt * deccelerationRate);
    }
}
