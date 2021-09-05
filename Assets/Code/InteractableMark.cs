using UnityEngine;

public class InteractableMark : MonoBehaviour, INetworkObject
{
    public bool isWorking = false;
    public bool isInteractableOnlyOnce = false;
    bool isInteractable = true;
    
    public GameObject[] interactablesToCall;
    
    
    Renderer rend;
    
    public Material stableMat;
    public Material isWorkingMat;

    
    float rotationSpeed = 1440;
    // public float currentSpeed = 0;
    
    const float timeToRotate = 2F;
    
    float timer = 0;
    
    
    NetworkObject net_comp;
    
    void Awake()
    {
        net_comp = GetComponent<NetworkObject>();
        rend = GetComponent<Renderer>();
    }
    
    public void ReceiveCommand(NetworkCommand command, params object[] args)
    {
        switch(command)
        {
            case(NetworkCommand.TakeDamageLimbWithForce):
            {
                InteractHitScan();
                break;
            }
        }
    }
    
    void Update()
    {
        if(isWorking)
        {
            float dt = UberManager.DeltaTime();
            transform.Rotate(new Vector3(rotationSpeed * dt, 0, 0), Space.Self);
            
            timer -= dt;
            if(timer <= 0)
            {
                isWorking = false;
                Vector3 localRotEuler = transform.localRotation.eulerAngles;
                localRotEuler.x = 0;
                transform.localRotation = Quaternion.Euler(localRotEuler);
            }
            rend.sharedMaterial = isWorkingMat;
        }
        else
        {
            timer = 0;
            rend.sharedMaterial = stableMat;
        }
    }
    
    
    void OnTurnedOn()
    {
        
    }
    
    void OnTurnedOff()
    {
                
    }
    
    void CallInteractables()
    {
        if(interactablesToCall.Length > 0)
        {
            for(int i = 0; i < interactablesToCall.Length; i++)
            {
                interactablesToCall[i].GetComponent<Interactable>().Interact();
            }
        }
    }
    
    
    public void InteractHitScan()
    {
        if(isWorking)
        {
            return;
        }
        
        if(!isInteractable)
        {
            return;
        }
        
        if(isInteractableOnlyOnce)
        {
            isInteractable = false;
        }
        
        CallInteractables();
        
        isWorking = true;
        timer = timeToRotate;
        
    }
}
