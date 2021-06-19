using UnityEngine;

public class PlayerLight : MonoBehaviour
{
    Transform thisTransform;
    new Light light;
    
    public Transform target;
    public PlayerController player; 
    
    public float height = 16f;
    float moveSpeed = 18f;
    
    Vector3 offsetXZ = new Vector3(0, 0, 0.3f);
    
    public void AttachPlayer(PlayerController _player)
    {
        player = _player;
        target = player.transform;
        
        thisTransform.localPosition = GetDesiredPos(target.position);
        thisTransform.localRotation = Quaternion.Euler(93, 0, 0);
    }
    
    void Awake()
    {
        thisTransform = transform;
        light = GetComponent<Light>();
    }
    
    Vector3 GetDesiredPos(Vector3 pos)
    {
        Vector3 Result = pos;
        Result = pos;
        Result.x += offsetXZ.x;
        Result.z += offsetXZ.z;
        
        Result.y += height;
        
        return Result;
    }
    
    void Update()
    {
        float dt  = UberManager.DeltaTime();
        if(target)
        {
            float speed = 24;
            Vector3 desiredPos = GetDesiredPos(target.position);
            
            thisTransform.localPosition = Vector3.MoveTowards(thisTransform.localPosition, desiredPos, dt * speed);
            
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
    
    
}
