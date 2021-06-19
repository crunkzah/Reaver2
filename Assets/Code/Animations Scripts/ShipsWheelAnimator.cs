using UnityEngine;

public class ShipsWheelAnimator : MonoBehaviour, Interactable
{
    public GameObject[] interactablesToCall;
    
    Interactable[] interactables;
    
    public LeverState state;
    
    public float maxRotationSpeed = 60f;
    
    
    public float angleAmount = 360f;
    
    public float angleTravelled;
    
    public Transform wheel;
    
    public bool callOnTurningOn = false;
    public bool callOnTurningOff = false;
    
    
    static Vector3 axis = Vector3.right;
    
    public bool interactableByPlayer = true;
    
    
    float interpolation = 0f;
    float interpolationSpeed = 0f;
    float interpolationTime = 0f;
    
    
    public Quaternion localOnRotation;
    public Quaternion localOffRotation;
    
    bool isWorking = false;
    float currentSpeed;
    
    void Start()
    {
        interactables = new Interactable[interactablesToCall.Length];
        for(int i = 0; i < interactablesToCall.Length; i++)
        {
            Interactable _interactable = interactablesToCall[i].GetComponent<Interactable>();
            if(_interactable != null)
            {
                interactables[i] = _interactable;
            }
#if UNITY_EDITOR
            else
            {
                InGameConsole.LogError(string.Format("<color=red>Interactable not set on {0} !! </color>", interactablesToCall[i].name));
            }
#endif
        }
        
        localOffRotation = wheel.localRotation;
        localOnRotation = wheel.localRotation * Quaternion.Euler(axis * angleAmount);
        
        
    }
    
    public void Interact()
    {
        if(interactableByPlayer)
        {
            if(!isWorking)
            {
                switch(state)
                {
                    case LeverState.Off:
                    {
                        OnPositiveWorkStarted();
                        isWorking = true;
                        break;
                    }
                    case LeverState.On:
                    {
                        OnNegativeWorkStarted();
                        isWorking = true;
                        break;
                    }
                }
            }            
        }        
    }
    
    void OnPositiveWorkStarted()
    {
        interpolation = 0f;
    }
    
    void OnPositiveWorkEnded()
    {
        state = LeverState.On;
        
        if(callOnTurningOn)
        {
            CallInteractables();
        }
    }
    
    
    void OnNegativeWorkStarted()
    {
        interpolation = 0f;
    }
    
    
    
    void CallInteractables()
    {
        for(int i = 0; i < interactables.Length; i++)
        {
            interactables[i].Interact();
        }   
    }
    
    void OnNegativeWorkEnded()
    {
        
        state = LeverState.Off;
        
        if(callOnTurningOff)
        {
            CallInteractables();
        }
    }
    
    void Update()
    {
        if(isWorking)
        {
            Quaternion start = (state == LeverState.Off ? localOffRotation : localOnRotation);
            Quaternion target = (state == LeverState.Off ? localOnRotation : localOffRotation);
            
            float dir = (state == LeverState.Off ? 1f : -1f);
            interpolation += 1f * (1f/maxRotationSpeed) * UberManager.DeltaTime();
            
            Math.Clamp(0f, 1f, ref interpolation);
            
            transform.localRotation = Quaternion.Slerp(start, target, interpolation);
            
            if(interpolation == 1f)
            {
                isWorking = false;
                switch(state)
                {
                    case(LeverState.Off):
                    {
                        OnPositiveWorkEnded();
                        break;
                    }
                    case(LeverState.On):
                    {
                        OnNegativeWorkEnded();
                        break;
                    }
                }    
            }
                
            
            
        }
    }   
}