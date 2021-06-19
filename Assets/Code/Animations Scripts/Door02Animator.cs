using UnityEngine;

public class Door02Animator : MonoBehaviour, Interactable
{
    NetworkObject netComponent;    
    Transform thisTransform;
    
    public float heightAnimation = 3.85f;
    const float maxSpeed = 4.25f;
    
    float currentSpeed = 0f;
    
    const float acceleration = 12f;
    
    public bool interactableOnce = true;
    public bool interactable = true;
    public bool isOpen = false;
    
    float openPosY;
    float closedPosY;
    
    void Awake()
    {
        thisTransform = transform;
        netComponent = GetComponent<NetworkObject>();
        
        if(!isOpen)
        {
            closedPosY = thisTransform.localPosition.y;
            openPosY = closedPosY - heightAnimation;    
        }
        else
        {
            openPosY = thisTransform.localPosition.y;
            closedPosY = openPosY + heightAnimation;
        }
    }
    
    public void Interact()
    {
        if(!interactable)
        {
            InGameConsole.Log(string.Format("Trying to interact with {0}, but it's not interactable", this.gameObject.name));
            return;
        }
        
        if(interactableOnce)
        {
            interactable = false;
        }
        
        if(isOpen)
        {
            Close();
            OnDoorCloseStart();
        }
        else
        {
            Open();
            OnDoorOpenStart();
        }
    }
    
    public void Open()
    {
        isOpen = true;
    }
    
    public void Close()
    {
        isOpen = false;
    }
    
    void OnDoorOpenStart()
    {
        if(audioSource)
        {
            audioSource.Play();
        }
        
        if(ps_fire_line)
        {
            ps_fire_line.Stop();
        }
        
        if(ps_fire_burst1)
        {
            ps_fire_burst1.Play();
            ps_fire_burst2.Play();
        }
        currentSpeed = -maxSpeed;
        // InGameConsole.LogFancy("OnDoorOpenStart()");
    }
    
    void OnDoorCloseStart()
    {
        if(audioSource)
        {
            audioSource.Play();
        }
        
        if(ps_fire_line)
        {
            ps_fire_line.Play();
        }
        
        currentSpeed = -maxSpeed;
        // InGameConsole.LogFancy("OnDoorCloseStart()");
    }
    
    void OnDoorOpened()
    {
        currentSpeed = 0f;
        // InGameConsole.LogFancy("OnDoorOpened()");
    }
    
    void OnDoorClosed()
    {
        currentSpeed = 0f;
        // InGameConsole.LogFancy("OnDoorClosed()");
    }
    
    public ParticleSystem ps_fire_line;
    public ParticleSystem ps_fire_burst1;
    public ParticleSystem ps_fire_burst2;
    
    
    public AudioSource audioSource;
    
    public Transform gear;
    public Transform gears_small_holder;
    public Transform gear1_small;
    public Transform gear2_small;
    
    readonly static Vector3 vRight = new Vector3(1, 0, 0);
    
    void Update()
    {
         if(isOpen)
         {
            if(thisTransform.localPosition.y != openPosY)
            {
                float dt = UberManager.DeltaTime();
                currentSpeed += acceleration * dt;
                
                float newPosY = thisTransform.localPosition.y - currentSpeed * dt;
                
                if(newPosY <= openPosY)
                {
                    newPosY = openPosY;
                    OnDoorOpened();
                }
                
                
                
                thisTransform.localPosition = new Vector3(thisTransform.localPosition.x, newPosY, thisTransform.localPosition.z);
                
                if(gear)
                {
                    gear.Rotate(vRight * currentSpeed * 40 *  dt, Space.Self);
                    gears_small_holder.Rotate(vRight * currentSpeed * -75 *  dt, Space.Self);
                    gear1_small.Rotate(vRight * currentSpeed * 60 *  dt, Space.Self);
                    gear2_small.Rotate(vRight * currentSpeed * -60 *  dt, Space.Self);
                }
            }
         }
         else
         {
             if(thisTransform.localPosition.y != closedPosY)
             {
                float dt = UberManager.DeltaTime();
                currentSpeed += acceleration * dt;
                
                float newPosY = thisTransform.localPosition.y + currentSpeed * dt;
                
                if(newPosY >= closedPosY)
                {
                    newPosY = closedPosY;
                    OnDoorClosed();
                }
                
                thisTransform.localPosition = new Vector3(thisTransform.localPosition.x, newPosY, thisTransform.localPosition.z);
                 
                if(gear)
                {
                    gear.Rotate(vRight * currentSpeed * -40 *  dt, Space.Self);
                    gears_small_holder.Rotate(vRight * currentSpeed * 75 *  dt, Space.Self);
                    gear1_small.Rotate(vRight * currentSpeed * -60 *  dt, Space.Self);
                    gear2_small.Rotate(vRight * currentSpeed * 60 *  dt, Space.Self);
                }
             }
         }
    }
    
}
