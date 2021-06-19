using UnityEngine;

public class MovingNPC : MonoBehaviour
{
    
    //Shared variables:
    static float rayLength = 0.05f;
    public LayerMask groundMask;
    static bool initialized = false;
    const float MaxGravityY = -25f;
    const int pointsOnCircleNum = 8;
    const int minimumContactPointsToStand = 5;
    static Vector3[] pointsOnCircle = new Vector3[8];
    //
    
    Transform thisTransform;
    
    [Header("Ground checking:")]
    public float groundCheckOffsetY = 0f;
    public float gravityMultiplier = 1f;
    public float radius = 1f;
    
    public bool isGrounded = false;
    
    [Header("Movement:")]    
    public Vector3 Velocity;
    
    
    void InitShared()
    {
            //groundMask = GameSettings.Singleton().groundLayer;
            
            float angleStep = 2  * Mathf.PI / pointsOnCircleNum;
            for(int i = 0; i < pointsOnCircleNum; i++)
            {
                pointsOnCircle[i] = new Vector3(Mathf.Cos(i * angleStep) , 0f, Mathf.Sin(i * angleStep));
            }
        
            initialized = true;
    }
    
    void Awake()
    {
        if(!initialized)
        {
            InitShared();
        }
        
        
        groundMask = LayerMask.GetMask("Ground", "Default", "NavMeshSurface");
        thisTransform = transform;
    }
    
    public void SetDirection(Vector3 direction)
    {
        thisTransform.forward = direction;
    }
    
    public void SetVelocityXZ(Vector3 v)
    {
        Velocity.x = v.x;
        Velocity.z = v.z;
    }
    
    public void StopXZ()
    {
        Velocity.x = Velocity.z = 0f;
    }
    
    
    bool CheckIfGrounded()
    {
        bool Result = false;
        
        if(Velocity.y > 0f)
            return false;
        
        Ray ray = new Ray();
        ray.direction = Vector3.down;
        Vector3 pos = thisTransform.position;
        pos.y += groundCheckOffsetY;
        
        
        float RayLength = rayLength + Math.Abs(Velocity.y) * UberManager.DeltaTime();
        
        int contactsNum = 0;
        
        for(int i = 0; i < pointsOnCircleNum; i++)
        {
            ray.origin = pos + pointsOnCircle[i];
            
            if(Physics.Raycast(ray, RayLength, groundMask))
            {
                contactsNum++;
                
                // Debug.DrawRay(ray.origin, ray.direction * (RayLength), Color.red);
            }
            
            if(contactsNum >= minimumContactPointsToStand)
            {
                Result = true;
                
                return Result;
            }
            
            // Debug.DrawRay(ray.origin, ray.direction * (RayLength), Color.blue);
        }
        return Result;
    }
    
    public Vector3 VelocityXZ()
    {
        return new Vector3(Velocity.x, 0f, Velocity.z);
    }
    
    
    void Update()
    {
        isGrounded = CheckIfGrounded();
        
        float dt = UberManager.DeltaTime();
            
        if(!isGrounded)
        {
            Velocity.y += gravityMultiplier * Globals.Gravity * dt;
            if(Velocity.y < MaxGravityY)
                Velocity.y = MaxGravityY;
        }
        else
        {
            Velocity.y = 0f;
        }
        
        thisTransform.Translate(Velocity * dt, Space.World);
    }    
    
    
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if(UnityEditor.EditorApplication.isPlaying || UnityEditor.EditorApplication.isPaused)
        {
            UnityEditor.Handles.color = Color.green;
            UnityEditor.Handles.DrawWireDisc(thisTransform.position - Vector3.up * groundCheckOffsetY, Vector3.up, radius);
        }
    }  
#endif
}
