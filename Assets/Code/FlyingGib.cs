using UnityEngine;

public class FlyingGib : MonoBehaviour
{
    TrailRenderer tr;
    Transform thisTransform;
    Vector3 velocity;
    
    const float gravity = -12F;//-9.8F;
    const float simulationMultiplier = 1.0F;
    const float collisionRadius = 0.275F;
    readonly static Vector3 vForward = new Vector3(0, 0, 1);
    static int groundMask = -1;
    bool isWorking = false;
    
    const float lifeTime = 5F;
    float lifeTimer = 0f;
    
    ParticleSystem ps_trail_blood;
    
    void Awake()
    {
        ps_trail_blood = GetComponent<ParticleSystem>();
        thisTransform = transform;
        if(groundMask == -1)
        {
            groundMask = LayerMask.GetMask("Ground");
        }
        
        tr = GetComponent<TrailRenderer>();
    }
    
    public void Launch(Vector3 pos, Vector3 vel)
    {
        thisTransform.localPosition = pos;
        ps_trail_blood.Clear();
        thisTransform.localRotation = Quaternion.Euler(Random.Range(0, 360f), Random.Range(0, 360f), Random.Range(0, 360f));
        velocity = vel;
        isWorking = true;
        
        lifeTimer = 0f;
        
        tr.Clear();
    }
    
    void Update()
    {
        if(isWorking)
        {
            float dt = UberManager.DeltaTime() * simulationMultiplier;
            
            Vector3 localPos = thisTransform.localPosition;
            Vector3 updatedPos = localPos + velocity * dt;
            
            thisTransform.localPosition = updatedPos;
            
            velocity.y += gravity * dt;
            
            RaycastHit hit;
            Vector3 dir = Math.Normalized(velocity);
            float speed = Math.Magnitude(velocity);
            
            //if(Physics.CheckSphere(updatedPos, collisionRadius, groundMask))
            if(Physics.SphereCast(localPos, collisionRadius, dir, out hit, speed * dt, groundMask))
            {
                GameObject bloodPuddle = ObjectPool.s().Get(ObjectPoolKey.BloodPuddle_1);
                bloodPuddle.transform.localPosition = hit.point;
                bloodPuddle.transform.up = hit.normal;
                //ParticlesManager.PlayPooledUp(ParticleType.blood_puddle_1, updatedPos, hit.normal);
                // Debug.Log("Normal: " + hit.normal);
                isWorking = false;
                thisTransform.localPosition = hit.point;
                // gameObject.SetActive(false);
            }
            
            if(lifeTimer > lifeTime)
            {
                isWorking = false;   
            }
            else
            {
                lifeTimer += UberManager.DeltaTime();
            }
        }
    }
    
#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, collisionRadius);
    }
#endif
}
