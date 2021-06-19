using UnityEngine;

[System.Serializable]
public struct TransformAndDestination
{
    public Transform transform;
    
    public Vector3 origin;
    public Vector3 destination;
    public bool isForwardReversed;
}



public class BigGates01Animator : MonoBehaviour, Interactable
{
    public TransformAndDestination[] gates;
    public float animationMagnitude = 2f;
    public float maxSpeed = 1f;
    public float acceleration = 2f;
    
    public bool isWorking = false;
    public bool isReversedDirection = false;
    public float currentSpeed = 0f;
    float distanceTravelled;
    
    [Header("Audio:")]
    public AudioClip clip;
    
    AudioSource audioSource;
    
    
    public bool isLocked = true;
    
    
    
    
    [Header("Gears settings:")]
    public Gear[] gears;
    public float globalGearSpeedMultiplier = 1.2f;
    
    
    
    
    
    
    
    public enum GateState : int
    {
        Closed,
        Opening,
        Open,
        Closing
    }
    
    GateState state;
    
    void Start()
    {
#if UNITY_EDITOR
        if(gates == null || gates.Length == 0)
        {
            InGameConsole.LogError("Gates are not set in editor.");
        }
#endif
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = clip;
        
        for(int i = 0; i < gates.Length; i++)
        {
            gates[i].origin = gates[i].transform.position;
            
            if(gates[i].isForwardReversed)
            {
                gates[i].destination = gates[i].transform.position + gates[i].transform.forward * animationMagnitude;                      
            }
            else
            {
                gates[i].destination = gates[i].transform.position - gates[i].transform.forward * animationMagnitude;      
            }
        }
    }
    
    
    public float timer = 0f;
    
    
    public GameObject leverOnInteractDenied;
    
    
    
    
    void OnInteractDenied()
    {
        InGameConsole.Log("<color=blue>OnInteractDenied()</color>");
        
        if(leverOnInteractDenied)
            leverOnInteractDenied.GetComponent<Interactable>().Interact();
        // leverOnInteractDenied.SendMessage("Interact");
    }
    
    public void UnlockGates()
    {
        isLocked = false;
        InGameConsole.Log("Gates <color=green>unlocked</color> !");
    }
    
    
    void OnOpenBegan()
    {
        distanceTravelled = 0f;
        
        state = GateState.Opening;
        isReversedDirection = false;
        isWorking = true;
        
        timer = 0f;
        
        
        audioSource.Play();
    }
    
    public GameObject[] OnOpenInteractables;
    
    void OnOpenEnded()
    {
        state = GateState.Open;
        
        isWorking = false;
        
        if(isInteractable)
        {
            if(OnOpenInteractables != null)
            {
                int len = OnOpenInteractables.Length;
                for(int i = 0; i < len; i++)
                {
                    OnOpenInteractables[i].GetComponent<Interactable>().Interact();
                }
            }
        }
        
        isInteractable = false;
        
    }
    
    void OnCloseBegan()
    {
        state = GateState.Closing;
        
        distanceTravelled = 0f;
        isReversedDirection = true;
        isWorking = true;
        
        timer = 0f;
        
        audioSource.Play();
    }
    
    void OnCloseEnded()
    {
        state = GateState.Closed;
        
        isWorking = false;        
    }
    
    public bool isInteractable = true;
    
    public void Interact()
    {
        if(isInteractable == false)
        {
            return;
        }
        
        switch(state)
        {
            case GateState.Closed:
            {
                if(isLocked)
                {
                    OnInteractDenied();
                }
                else
                {
                    OnOpenBegan();
                    
                }
                break;
            }
            case GateState.Open:
            {
                OnCloseBegan();
                break;
            }
            default:
            {
                break;
            }
            
            // default:
            // {
            //     string msg = string.Format("<color=red>{0} has unhandled state.</color>", this.gameObject.name);
            //     InGameConsole.LogError(msg);
            //     break;
            // }
        }
    }
    
    
    // public float gearSpeeDebug = 25f;
    
    void Update()
    {
        float dt = UberManager.DeltaTime();
        
        if(isWorking)
        {
            timer += dt;
            
            Vector3 targetPos;
            
            currentSpeed += acceleration * dt;
            if(currentSpeed > maxSpeed)
            {
                currentSpeed = maxSpeed;
            }
            
            
            
            for(int i = 0; i < gates.Length; i++)            
            {
                targetPos = isReversedDirection ? gates[i].origin : gates[i].destination;
                gates[i].transform.position = Vector3.MoveTowards(gates[i].transform.position, targetPos,
                                                                    currentSpeed * dt);
                                                                    
                
                if(distanceTravelled > animationMagnitude)
                {
                    switch(state)
                    {
                        case(GateState.Opening):
                        {
                            OnOpenEnded();
                            break;
                        }
                        case(GateState.Closing):
                        {
                            OnCloseEnded();
                            break;
                        }
                        default:
                        {
                            break;
                        }
                    }                        
                }
            }
            
            
           if(gears != null && gears.Length > 0)
            {
                Vector3 multipliedAngleVelocity = globalGearSpeedMultiplier * Vector3.right * currentSpeed * dt;
                
                InGameConsole.LogOrange("Gears is ok");
                
                for(int i = 0; i < gears.Length; i++)
                {
                    gears[i].transform.Rotate(gears[i].ratio * multipliedAngleVelocity, Space.Self);
                    InGameConsole.LogOrange(string.Format("Rotating gear {0}", gears[i].transform.name));
                }
                
                
            }
            else
            {
                InGameConsole.LogOrange("Gears is null");
            }
             
            distanceTravelled += currentSpeed * dt;
            
            // if((targetPos - gates[i].transform.position) > 0)
            
        }
        else
        {
            currentSpeed = 0f;
        }
    }
    
    
#if UNITY_EDITOR
    
    void OnDrawGizmos()
    {
        GUIStyle style = new GUIStyle();
        style.alignment = TextAnchor.MiddleCenter;
        style.normal.textColor = new Color(1f, 64f/256f, 1f);
        string text = string.Format("State: <b>{0}</b>\nDist: {1}\nTimer: {2}", state, distanceTravelled, timer);
        
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.25f, text, style);
    }

#endif    

}
