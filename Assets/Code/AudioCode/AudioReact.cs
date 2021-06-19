using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioReact : MonoBehaviour
{
    ParticleSystem ps;
    
    
    public float mult = 1;
    public float burstThreshold = 50;
    
    public int burstNum = 32;
    
    
    
    void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        main_module = ps.main;
    }
    
    ParticleSystem.MainModule main_module;
    
    public float vol;
    
    void Update()
    {
        vol = AudioManager.GetCurrentVolume() * mult;
        main_module.simulationSpeed = vol;
        
        //ps.emission.rateOverTime = vol * mult;
        
        // if(vol > burstThreshold)
        // {
        //     ps.Emit(burstNum);
        // }
        
    }
    
    
}
