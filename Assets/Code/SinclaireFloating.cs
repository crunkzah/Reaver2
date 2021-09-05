using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SinclaireFloating : MonoBehaviour, INetworkObject
{
    NetworkObject net_comp;
    public AudioSource audio_src;
    public AudioClip die_clip;
    
    public ParticleSystem die_ps;
    public ParticleSystem floating_ps;
    public Renderer[] rends;
    public Transform[] godRays;
    public Light floating_light;
    
    
    void Awake()
    {
        net_comp = GetComponent<NetworkObject>();
        light_baseIntensity = floating_light.intensity;
        floating_light.renderMode = LightRenderMode.ForcePixel;
        light_baseRange = floating_light.range;
    }
    
    void DestroyLight()
    {
        Destroy(floating_light.gameObject, 0.1f);
    }
    
    const float flick_freq = 0.125F;
    float timer = 0f;
    float light_baseIntensity;
    float light_baseRange;
    
    bool isWorking = true;
    
    void Update()
    {
        if(!isWorking)
        {
            return;
        }
        
        float dt = UberManager.DeltaTime();
        
        timer += dt; 
        if(timer == 0f)
        {
            timer = Random.Range(0.025f, flick_freq);
            floating_light.intensity = Random.Range(0.7f, 1.2f) * light_baseIntensity;
            floating_light.range = Random.Range(0.85f, 1.15f) * light_baseRange;
            
        }
    }
    
    void DoLight(Vector3 pos)
    {
        GameObject g = ObjectPool2.s().Get(ObjectPoolKey.LightPooled, false);
        LightPooled light = g.GetComponent<LightPooled>();
        Color color = new Color(1f, 1f, 1f, 1f);
        
        float lightRadius = 16;
        float decay_speed = lightRadius / 3;
        light.DoLight(pos, color, 5, 1.2f, lightRadius, decay_speed);
    }
    
    IEnumerator ShrinkGodRays()
    {
        for(;;)
        {
            for(int i = 0; i < godRays.Length; i++)
            {
                godRays[i].localScale = Vector3.MoveTowards(godRays[i].localScale, Vector3.zero, 10 * Time.deltaTime);        
            }
            yield return null;
        }
    }
    
    public void ReceiveCommand(NetworkCommand command, params object[] args)
    {
        switch(command)
        {
            case(NetworkCommand.Die):
            {
                
                isWorking = false;
                for(int i = 0; i < rends.Length; i++)
                {
                    rends[i].enabled = false;
                }
                  
                PlayerController local_pc = PhotonManager.GetLocalPlayer();
                if(local_pc)
                {
                    if(Math.SqrDistance(local_pc.GetGroundPosition(), transform.position) < 6*6 * 2.5F)
                        local_pc.BoostVelocity(new Vector3(0, 16f, 0));
                }
                
                if(die_ps)
                    die_ps.Play();
                    
                if(floating_ps)
                    floating_ps.Stop();
                
                    
                if(die_clip)
                    audio_src.PlayOneShot(die_clip, 1);
                    
                floating_light.range *= 1.33f;
                floating_light.intensity *= 1.5f;
                
                Invoke(nameof(DestroyLight), 0.1f);
                
                
                StartCoroutine(ShrinkGodRays());
                DoLight(transform.position + Vector3.down * 14);
                
                Destroy(this.gameObject, 10);
                
                
                break;
            }
            default:
            {
                break;
            }
        }
    }
    void OnDestroy()
    {
        StopAllCoroutines();
    }
}
