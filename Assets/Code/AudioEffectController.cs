using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioEffectController : MonoBehaviour
{
    
    [Range(1f, 36f)]
    public float radius = 10;
    public AudioMixer mixer;
    
    public float pitch_scale;
    public float freq = 0.2f;
    
    float timer = 0;
    
    PlayerController local_pc;
    
    Transform thisTransform;
    
    void Awake()
    {
        thisTransform = transform;
    }
    
    public bool pitchNeedChange = true;
    
    
    float pitch = 1;
    
    void Update()
    {
        if(!local_pc)
            local_pc = PhotonManager.GetLocalPlayer();
            
        if(local_pc)
        {
            if(Math.SqrDistance(local_pc.GetHeadPosition(), thisTransform.localPosition) < radius * radius)
            {
                float dt = UberManager.DeltaTime();
                timer += dt;
                if(timer > freq)
                {
                    timer -= freq;
                }
                
                float dist = Vector3.Distance(local_pc.GetHeadPosition(), thisTransform.localPosition);
                
                float t = Mathf.InverseLerp(radius, 1, dist);
                
                pitch = 1 + Random.Range(-1f, 1f) * pitch_scale * t;
                pitchNeedChange = true;
            }
            else
            {
                if(pitchNeedChange)
                {
                    mixer.SetFloat("MP", 1);
                    pitchNeedChange = false;
                }
            }
        }
        else
        {
            if(pitchNeedChange)
                pitch = 1;
        }
        
        if(pitchNeedChange)
            mixer.SetFloat("MP", pitch);
        //mixer.Set
    }
    
    void OnDestroy()
    {
        mixer.SetFloat("MP", pitch);   
    }
    
}
