using UnityEngine;

public class HealthCrystal : MonoBehaviour, IPooledObject
{
    Transform thisTransform;
    
    
    void Awake()
    {
        thisTransform = transform;
    }
    
    
    const float maxSpeed = 8F;
    const float acceleration = 4F;
    
    float currentSpeed = 0;
    
    float lifeTimer = 0;
    
    const float LifeTime = 5;
    
    public int hp = 20;
    
    Vector3 vel;
    
    public ParticleSystem ps;
    public ParticleSystem ps_on_die;
    
    public void InitialState()
    {
        // vel = Vector3.zero;        
    }
    
    Vector3 offsetVelocity;
    
    const float offsetZeroingSpeed = 0.7F;
    
    const float offsetVelocityMult = 0.2f;
    
    public void Launch(Vector3 pos, int _hp)
    {
        this.gameObject.SetActive(true);
        
        
        isWorking = true;
        
        ps.Clear();
        ps.Play();
        hp = _hp;
        
        lifeTimer = Random.Range(LifeTime - 0.3f, LifeTime + 0.3f);
        // lifeTimer = LifeTime;
        
        offsetVelocity = Random.onUnitSphere * offsetVelocityMult;
        
        vel = Vector3.zero;
        currentSpeed = 0;
        
        thisTransform.localPosition = pos + Random.onUnitSphere * 0.15f;
    }
    
    bool isWorking = false;

    // Update is called once per frame
    void Update()
    {
        if(!isWorking)
        {
            return;
        }
        
        float dt = UberManager.DeltaTime();
        
        PlayerController local_player = PhotonManager.Singleton().local_controller;
        
        if(local_player && local_player.isAlive)
        {
            Vector3 playerPos = PhotonManager.Singleton().local_controller.GetGroundPosition();
            playerPos.y += 1.15F;
            
            if(lifeTimer < LifeTime - 0.25f && Math.SqrDistance(thisTransform.localPosition, playerPos) < 0.8F * 0.8F)
            {
                PhotonManager.Singleton().local_controller.Heal(hp);
                EndLife();
            }
            
                                    
            if(lifeTimer <= 0)
            {
                EndLife();
            }
            else
            {
                lifeTimer -= dt;
            }
            
            Vector3 updatedPos = Vector3.MoveTowards(thisTransform.localPosition, playerPos, dt * currentSpeed);
            updatedPos += offsetVelocity * dt;
            
            offsetVelocity = Vector3.MoveTowards(offsetVelocity, Vector3.zero, offsetZeroingSpeed * dt);
            
            thisTransform.localPosition = updatedPos;
            
            
            currentSpeed += acceleration * dt;
            if(currentSpeed > maxSpeed)
            {
                currentSpeed = maxSpeed;
            }
        }
        else
        {
            EndLife();
        }
        
        //Vector3 updatedPos = thisTransform.localPosition + vel * dt;
    }
    
    void EndLife()
    {
        ps.Clear(false);
        ps.Stop();
        
        ps_on_die.Play();
        isWorking = false;
        //this.gameObject.SetActive(false);
        
    }
}
