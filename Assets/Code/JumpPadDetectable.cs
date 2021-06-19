using UnityEngine;

public class JumpPadDetectable : MonoBehaviour
{
    
    public bool isWorking = false;
    
    
    public PlayerController player;
    
    
    public Vector3 currentVelocity;
    
    public static Vector3 gravity = new Vector3(0f, -9.81f, 0f);
    
    
    void Start()
    {
        player = GetComponent<PlayerController>();
        mask = GameSettings.Singleton().jumpedGroundMask;
    }
    
    
    static LayerMask mask;
    
    public void Throw(Vector3 vel)
    {
        if(!isWorking)
        {
            currentVelocity = vel;
            isWorking = true;
        }
    }
    
    void UpdateFlying()
    {
        if(isWorking)
        {
            if(player.isMovementControllable)
            {
                player.isMovementControllable = false;
            }
            
            Ray ray = new Ray(transform.position, Vector3.down);
            
            
            float rayLength = Mathf.Abs(currentVelocity.y);
            
            if(Physics.Raycast(ray, rayLength, mask))
            {
                isWorking = false;
            }
            else
            {
                transform.Translate(currentVelocity * Time.deltaTime);
                
                currentVelocity += gravity * Time.deltaTime;
            }
            
        }
        else
        {
            if(player.isMovementControllable == false)
            {
                currentVelocity = Vector3.zero;
                player.isMovementControllable = true;
            }
        }
    }
    
    
    
    
}
