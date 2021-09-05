using UnityEngine;

public class HealthCrystal : MonoBehaviour, IPooledObject
{
    Transform thisTransform;
    
    
    void Awake()
    {
        if(groundLayer == -1)
        {
            groundLayer = LayerMask.GetMask("Ground");
        }
        thisTransform = transform;
    }
    
    
    const float maxSpeed = 720000f;
    const float acceleration = 20f;
    
    float currentSpeed = 0;
    
    float lifeTimer = 0;
    
    const float LifeTime = 7.3f;
    
    public int hp = 20;
    
    Vector3 vel;
    
    public ParticleSystem ps;
    public ParticleSystem ps_on_die;
    
    public void InitialState()
    {
        // vel = Vector3.zero;        
    }
    
    bool isNotFlyingToPlayer = true;
    
    static int groundLayer = -1;
    
    const float GRAVITY_Y = -9.8f * 1.5f;
    Vector3 velocity;
    const float startVelY = 9f;
    
    public void Launch(Vector3 pos, int _hp)
    {
        this.gameObject.SetActive(true);
        
        isNotFlyingToPlayer = true;
        
        isWorking = true;
        
        ps.Clear();
        ps.Play();
        hp = _hp;
        
        lifeTimer = Random.Range(LifeTime - 0.2f, LifeTime + 0.2f);
        
        velocity.x = Random.Range(-1f, 1f);
        velocity.z = Random.Range(-1f, 1f);
        velocity.y = startVelY;
        
        touchedGround = false;
        
        vel = Vector3.zero;
        currentSpeed = 16;
        Vector3 randV = Random.onUnitSphere;
        randV.y = 0;
        thisTransform.localPosition = pos + randV * 0.15f;
    }
    
    bool isWorking = false;
    bool touchedGround = false;

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
            
            if(Math.SqrDistance(thisTransform.localPosition, playerPos) < 0.225F * 0.225F)
            {
                PhotonManager.Singleton().local_controller.Heal(hp);
                EndLife();
            }
            
            if(Math.SqrDistance(thisTransform.localPosition, playerPos) < 4f * 4f)
            {
                isNotFlyingToPlayer = false;
            }
                                    
            if(lifeTimer <= 0)
            {
                EndLife();
            }
            else
            {
                lifeTimer -= dt;
            }
            
            
            if(!isNotFlyingToPlayer)
            {
                Vector3 updatedPos = Vector3.MoveTowards(thisTransform.localPosition, playerPos, dt * currentSpeed);
                
                thisTransform.localPosition = updatedPos;
            
                currentSpeed += acceleration * dt;
                if(currentSpeed > maxSpeed)
                {
                    currentSpeed = maxSpeed;
                }
            }
            else
            {
                if(!touchedGround)
                {
                    velocity.y += GRAVITY_Y * dt;
                    Vector3 updatedPos = thisTransform.localPosition;
                    updatedPos.x += velocity.x * dt;
                    updatedPos.z += velocity.z * dt;
                    updatedPos.y += velocity.y * dt;
                    
                    float rayDistance = Math.Magnitude(velocity) * dt;
                    
                    Ray ray = new Ray(thisTransform.localPosition, velocity.normalized);
                    RaycastHit hit;
                    if(Physics.Raycast(ray, out hit, rayDistance, groundLayer))
                    {
                        thisTransform.localPosition = hit.point;
                        velocity.x = velocity.z  = velocity.y = 0;
                        touchedGround = true;
                    }
                    else
                    {
                        thisTransform.localPosition = updatedPos;
                    }
                }
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
