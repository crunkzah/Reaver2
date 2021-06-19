using UnityEngine;
using Photon.Pun;

public class BigGatesRotating : MonoBehaviour, Interactable, INetworkObject
{
    public bool isWorking = false;
    
    public Transform gate_l;
    public Transform gate_r;
    
    AudioSource audio_src;
    
    bool isOpen = false;
    
    OcclusionPortal occPortal;
    
    float angleSpeed;
    
    float timeToWork = 5f;
    
    NetworkObject net_comp;
    
    void Awake()
    {
        audio_src = GetComponent<AudioSource>();
        occPortal = GetComponent<OcclusionPortal>();
        occPortal.open = false;
        net_comp = GetComponent<NetworkObject>();
    }
    
    public void ReceiveCommand(NetworkCommand command, params object[] args)
    {
        switch(command)
        {
            case(NetworkCommand.OpenGates):
            {
                Open();
                break;
            }
            default:
            {
                
                break;
            }
        }
    }
    
    public void Interact()
    {
        InGameConsole.LogFancy("Interacting!");
        
        if(!isOpen)
        {
            Open();
        }
    }
    
    public void Open()
    {
        occPortal.open = true;
        isOpen = true;
        
        timer = 0;
        angleSpeed = 90f / timeToWork;
        
        isWorking = true;
    }
    
    float timer;
    
    void Update()
    {
        if(!isWorking)
            return;
            
        float dt = UberManager.DeltaTime();
        
        timer += dt;
        
        gate_l.Rotate(new Vector3(0, 1, 0) * angleSpeed * dt, Space.Self);
        gate_r.Rotate(new Vector3(0, 1, 0) * -angleSpeed * dt, Space.Self);
        
        if(timer >= timeToWork)
        {
            isWorking = false;
        }
        
    }
}
