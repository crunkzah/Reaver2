using UnityEngine;

public class DeferredStrike : MonoBehaviour
{
    bool isWorking;
    float timer;
    
    float delay;
    float duration;
    int damage;
    float force;
    
    public AudioSource audio_src;
    public ParticleSystem ps_cooking;
    public ParticleSystem ps_burst;
    
    bool didStrike = false;
    
    
    
    public void DoStartStrike(Vector3 strike_pos, float _delay, float _duration, int _dmg, float _force)
    {
        transform.localPosition = strike_pos;
        
        delay       = _delay;
        duration    = _duration;
        force       = _force;
        damage      = _dmg;
        force       = _force;
        
        ps_cooking.Play();
        
        isWorking   = true;
        timer = 0;
    }
    
    public CapsuleCollider col;
    
    void DoStrike()
    {
        InGameConsole.LogFancy("DoStrike() from <color=yellow>" + this.gameObject.GetInstanceID() + "</color>");
        ps_burst.Play();
        
        PlayerController local_pc = PhotonManager.GetLocalPlayer();
        if(local_pc)
        {
            Vector3 player_pos = local_pc.GetCenterPosition();
            //audio_src.transform.position = col.ClosestPoint(player_pos);
            if(col.bounds.Contains(player_pos))
            {
                local_pc.TakeDamage(damage);
                local_pc.BoostVelocity(Vector3.up * force);
            }
        }
        audio_src.Play();
    }
    
    void Update()
    {
        if(!isWorking)
        {
            return;
        }
        
        float dt = UberManager.DeltaTime();
        timer += dt;
        
        if(!didStrike && timer > delay)
        {
            DoStrike();
            didStrike = true;    
        }
        
        if(didStrike && timer > duration)
        {
            isWorking = false;
            Destroy(this.gameObject, 1f);
        }
    }
    
}
