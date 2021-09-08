using UnityEngine;

public class Shockwave : MonoBehaviour
{
    // public SphereCollider col;
    ParticleSystem burst_ps;
    ParticleSystem.MainModule burst_main;
    AudioSource audio_src;
    
    
    float lifeTimer;
    float currentScale;
    float currentSpeed = 10;
    const float startSpeed = 15;
    const float finalSpeed = 5;
    
    bool didDamageToLocalPlayer = false;
    
    int damage;
    
    const float shockwaveHeight = 1.5f;
    
    Transform thisTransform;
    
    void Awake()
    {
        thisTransform = transform;
        burst_ps = GetComponent<ParticleSystem>();
        burst_main = burst_ps.main;
        audio_src = GetComponent<AudioSource>();
    }
    
    public void DoShockwave(Vector3 pos, float _startBlastRadius, int _damage)
    {
        thisTransform.position = pos;    
        
        if(burst_ps)
            burst_ps.Play();
        didDamageToLocalPlayer = false;
        
        PlayerController _local_pc = PhotonManager.GetLocalPlayer();
        if(_local_pc)
        {
            Vector3 playerPos = _local_pc.GetCenterPosition();
            
            float sqrDistanceToPlayer = Math.SqrDistance(playerPos, pos);
            if(sqrDistanceToPlayer < _startBlastRadius)
            {
                
                    Vector3 boostVel = (playerPos - pos).normalized * 24;
                    _local_pc.BoostVelocity(boostVel);
                    _local_pc.TakeDamage(_damage);
                    
                    didDamageToLocalPlayer = true;                
                
            }    
        }
        
        currentSpeed = startSpeed;
        if(audio_src)
        {
            audio_src.Play();
        }
        currentScale = 1;
        damage = _damage;
    }
    
    PlayerController local_pc;
    
    const float minRadiusMult = 0.7f;
    
    void Update()
    {
        float dt = UberManager.DeltaTime();
        
        currentScale += currentSpeed * dt;
        currentSpeed = Mathf.MoveTowards(currentSpeed, finalSpeed, dt * 4);
        thisTransform.localScale = new Vector3(currentScale, 1, currentScale);
        
        lifeTimer += dt;
           
        if(!didDamageToLocalPlayer)
        {
            if(lifeTimer > 3.35f)
            {
                didDamageToLocalPlayer = true;
                Destroy(this.gameObject, 5);
            }
            if(local_pc)
            {
                Vector3 playerPos = local_pc.GetCenterPosition();
                float distance = Vector3.Distance(playerPos, thisTransform.localPosition);
                
                if(distance < currentScale && distance > currentScale * minRadiusMult)
                {
                    float height_diff = Math.Abs(playerPos.y - thisTransform.localPosition.y);
                    if(height_diff < shockwaveHeight)
                    {
                        didDamageToLocalPlayer = true;
                        local_pc.BoostVelocity(new Vector3(0, 11, 0));
                        local_pc.TakeDamage(damage);
                    }
                }
            }
            else
            {
                local_pc = PhotonManager.GetLocalPlayer();
            }
        }
    }
    
    
}
