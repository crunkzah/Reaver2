using UnityEngine;

public class DrawBridge01Animator : MonoBehaviour, ITriggerable
{
    
    public float maxAngleSpeed = -40f;
    public float closedAngle = -80f;
    public float acceleration = -12f;
    
    
    public float gearsSpeed = 20f;
    
    public bool isWorking = false;
    
    float angleTravelled = 0f;
    
    public float currentSpeed = 0f;
    
    
    public float accelerationDir = 1f;
    public float speedThreshold = 25f;
    
    public Gear[] gears;
    
    public void OnTrigger()
    {
        isWorking = true;
        angleTravelled = 0f;
        OnWorkStarted();
    }
    
    public void OnWorkStarted()
    {
        
    }
    
    public void OnWorkFinished()
    {
        // this.enabled = false;
    }
    
    Quaternion targetRotation;
    
    void Start()
    {
        targetRotation = Quaternion.Euler(closedAngle, 0f, 0f);
    }
    
    
    
    void Update()
    {
        if(isWorking)
        {
            if(Mathf.Abs(angleTravelled) > Mathf.Abs(closedAngle))
            {
                angleTravelled = closedAngle;
                isWorking = false;
                
                
                OnWorkFinished();
            }
            
            
            if(Mathf.Abs(currentSpeed) > speedThreshold)
            {
                accelerationDir = -1f;
            }
            
            currentSpeed += accelerationDir * acceleration * Time.deltaTime;
            
            angleTravelled += currentSpeed * Time.deltaTime;
            
            
            transform.Rotate(Vector3.right * currentSpeed * Time.deltaTime, Space.Self);
            
            for(int i = 0; i < gears.Length; i++)
            {
                gears[i].transform.Rotate(currentSpeed * gearsSpeed * Vector3.right * gears[i].ratio * Time.deltaTime, Space.Self);
            }
            // transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, angleSpeed * Time.deltaTime);
            
            
        }
    }
}
