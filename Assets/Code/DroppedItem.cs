using UnityEngine;
using Photon.Pun;

public class DroppedItem : MonoBehaviour, IPooledObject 
{
    public bool isKey = false;
    //public string itemName = "Glock";
    public SceneNetObject enumName = SceneNetObject.UNDEFINED;
    public EntityType entityType = EntityType.UNDEFINED_ENTITY;
    
    public ParticleSystem ps;
    
    NetworkObject net_comp;
    
    void Awake()
    {
        ps = GetComponentInChildren<ParticleSystem>();
    }
    
    void Start()
    {
        net_comp = GetComponent<NetworkObject>();
        thisTransform = transform;
        
        mask = LayerMask.GetMask("Ground", "Default", "Fadable");
        
        if(isKey)
        {
            if(WeaponManager.Singleton() != null)
            WeaponManager.Singleton().RegisterItemOnGround(this.transform);
            
        }        
    }

    void OnDisable()
    {
         
        if(WeaponManager.Singleton() != null)
            WeaponManager.Singleton().UnregisterItemOnGround(this.transform);
    }
    
    readonly static Vector3 vForward = new Vector3(0, 0, 1);

    public void OnPickUp()
    {
        ParticlesManager.PlayPooled(ParticleType.on_item_pickup_ps, thisTransform.position, vForward);
        InGameConsole.LogOrange(string.Format("OnPickUp(): Picked up {0} netId: {1}", this.gameObject.name, net_comp.networkId));
        
        //this.gameObject.SetActive(false);
        WeaponManager.Singleton().UnregisterItemOnGround(this.transform);
        transform.position = new Vector3(2000, 2000, 2000);
        
        if(ps)
        {
            ps.Stop();
        }
    }
    
    public void InitialState()
    {
        dumpVel = 0;
        transform.rotation = Random.rotation;
        transform.position = GetGroundedPosition(transform.position);
    }
    
    public void LaunchItem(Vector3 _velocity)
    {
        velocity = _velocity;
        
        if(thisTransform == null)
        {
            thisTransform = transform;
        }
        
        thisTransform.localScale = new Vector3(0.33f, 0.33f, 0.33f);
        
        isFlying = true;
    }
    
    static LayerMask mask;
    
    public bool isFlying = false;
    
    static Vector3 gravity = new Vector3(0, -17.5f, 0);
    Vector3 velocity = new Vector3(0, 0, 0);
    
    Vector3 vUp = new Vector3(0, 1, 0);
    
    Transform thisTransform;
    
    float dumpVel;
    
    void Update()
    {
        float dt = UberManager.DeltaTime();
        float currentScale = thisTransform.localScale.x;
        
        if(!isKey)
        {
            if(currentScale != 1f)
            {
                currentScale = Mathf.MoveTowards(currentScale, 1f, 1.33f * dt);
                // currentScale = Mathf.SmoothDamp(currentScale, 1f, ref dumpVel, 0.5f);
                    
                thisTransform.localScale = new Vector3(1, 1, 1) * currentScale;
            }
            
            RaycastHit hit;
            
            if(isFlying)
            {
                Vector3 dir = Math.Normalized(velocity);
                velocity += gravity * dt;
                
                float magnitude = Math.Magnitude(velocity);
                
                float magnitude_for_frame = magnitude * dt;
                
                if(magnitude_for_frame < 0.5f)
                    magnitude_for_frame = 0.5f;
                
                
                
                thisTransform.Rotate(vUp, 25f * dt);
                
                
                if(Physics.Raycast(thisTransform.position, dir, out hit, magnitude_for_frame, mask))
                {
                    isFlying = false;
                    
                    thisTransform.position = hit.point + hit.normal * 0.8f;
                    
                    WeaponManager.Singleton().RegisterItemOnGround(this.transform);
                }
                else
                {
                    
                    // thisTransform.localScale = Vector3.one;
                    Vector3 updatedPosition = thisTransform.position + velocity * dt;
                    thisTransform.position = updatedPosition;
                    // thisTransform.Translate(velocity * dt, Space.World);
                }
            }
            else
            {
                if(ps && !ps.isPlaying)
                {
                    ps.Play();
                }
            }
        }
    }

    public Vector3 GetGroundedPosition(Vector3 pos)
    {
        Vector3 resultPos = pos;
        // resultPos.y = 0f;

        Bounds bounds = GetComponent<Renderer>().bounds;

        Vector3 rayOrigin = transform.position;
       
        Ray ray = new Ray(rayOrigin, Vector3.down);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 50f, GameSettings.Singleton().groundLayer))
        {
            resultPos.y = hit.point.y + (bounds.center.y - bounds.min.y)  + GameSettings.Singleton().distanceToGround;
        }
        else
        {
            // InGameConsole.LogError("(DroppedItem) Couldn't hit ray to ground");
            resultPos = new Vector3(2000, 800, 2000);
            WeaponManager.Singleton().UnregisterItemOnGround(this.transform);
        }

        return resultPos;
    }
    
    // public static Vector3 GetGroundedPosition(Transform droppedItem)
    // {
    //      Vector3 resultPos = droppedItem.position;
    //     // resultPos.y = 0f;

    //     Bounds bounds = droppedItem.GetComponent<Renderer>().bounds;

    //     Vector3 rayOrigin = droppedItem.position;
       
    //     Ray ray = new Ray(rayOrigin, Vector3.down);
    //     RaycastHit hit;
    //     if (Physics.Raycast(ray, out hit, 50f, GameSettings.Singleton().groundLayer))
    //     {
    //         resultPos.y = hit.point.y + (bounds.center.y - bounds.min.y)  + GameSettings.Singleton().distanceToGround;
    //     }
    //     else
    //     {
    //         // InGameConsole.LogError("(DroppedItem) Couldn't hit ray to ground");
    //         resultPos = new Vector3(2000, 800, 2000);
    //         WeaponManager.Singleton().UnregisterItemOnGround(droppedItem);
    //     }

    //     return resultPos;
    // }
}
