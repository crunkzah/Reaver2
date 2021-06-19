using UnityEngine;

public class VerticalGateController : MonoBehaviour, Interactable
{
    public GateState state = GateState.Closed;
    AudioSource audioSource;
    
    public AudioClip openClip;
    public AudioClip closeClip;
    
    public float openOffsetY = 4;
    
    public float speed = 8f;
    Transform thisTransform;
    Vector3 closedPos;
    Vector3 openPos;
    
    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        thisTransform = transform;
        closedPos = thisTransform.localPosition;
        openPos = closedPos + new Vector3(0, openOffsetY, 0);
    }
    
    public void Interact()
    {
        if(state == GateState.Closed)
        {
            Open();
        }
        else
        {
            Close();
        }
    }
    
    void Update()
    {
        Vector3 targetPos  = closedPos;
        
        switch(state)
        {
            case(GateState.Open):
            {
                targetPos = openPos;
                break;
            }
            case(GateState.Closed):
            {
                targetPos = closedPos;
                break;
            }
        }
        
        float dt = UberManager.DeltaTime();
        
        thisTransform.localPosition = Vector3.MoveTowards(thisTransform.localPosition, targetPos, speed * dt);
    }
    
    public void Open()
    {
        state = GateState.Open;
        
        audioSource.PlayOneShot(openClip, 1);
    }
    
    public void Close()
    {
        state = GateState.Closed;
        
        audioSource.PlayOneShot(closeClip, 1);
    }
    
}
