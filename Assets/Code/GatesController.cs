using UnityEngine;

public enum GateState : int
{
    Closed,
    Open,
    Locked,
}

public enum GateDetectionMode : int
{
    Old,
    New
}

public class GatesController : MonoBehaviour, INetworkObject
{
    public GateDetectionMode detectionMode = GateDetectionMode.Old;
    public GateState state = GateState.Closed;
    public AudioSource audioSource;
    public AudioClip openClip;
    public AudioClip closedClip;
    
    public float animationSpeed = 9f;
    
    public Transform detectionTransform;
    Vector3 detectionPosGlobal;
    
    
    public Transform leftGate;
    public Transform rightGate;
    
    
    public Vector3 closedPosLeftLocal;
    public Vector3 openPosLeftLocal;
    
    public Vector3 closedPosRightLocal;
    public Vector3 openPosRightLocal;
    
    Vector3 targetPosLeftLocal;
    Vector3 targetPosRightLocal;
    
    public MeshRenderer[] status_emissives;
    
    public Material closed_mat;
    public Material locked_mat;
    
    //public GameObject[] locks;
    
    NetworkObject net_comp;
    
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
    
    
    
    
    public void Lock()
    {
        audioSource.PlayOneShot(lock_clip);
        state = GateState.Locked;
        ProcessStatusLight(state);
    }
    
    public AudioClip unlock_clip;
    public AudioClip lock_clip;
    
    public void Unlock()
    {
        audioSource.PlayOneShot(unlock_clip);
        state = GateState.Closed;
        ProcessStatusLight(state);
    }
    
    
    void ProcessStatusLight(GateState _state)
    {
        switch(_state)
        {
            case(GateState.Closed):
            {
                int len2 = status_emissives.Length;
                for(int i = 0; i < len2; i++)
                {
                    status_emissives[i].sharedMaterial = closed_mat;
                }
                break;
            }
            case(GateState.Open):
            {
                int len2 = status_emissives.Length;
                for(int i = 0; i < len2; i++)
                {
                    status_emissives[i].sharedMaterial = closed_mat;
                }
                break;
            }
            case(GateState.Locked):
            {
                int len2 = status_emissives.Length;
                for(int i = 0; i < len2; i++)
                {
                    status_emissives[i].sharedMaterial = locked_mat;
                }
                break;
            }
        }
    }
    
    
    OcclusionPortal occlusion_portal;
    
    
    public float radius = 9;
    
    static int detectorMask = -1;
    
    Transform thisTransform;
    
    void Awake()
    {
        if(detectorMask == -1)
        {
            //detectorMask = LayerMask.GetMask("Player", "NPC2");
            detectorMask = LayerMask.GetMask("Player");
        }
        
        audioSource = GetComponent<AudioSource>();
        occlusion_portal = GetComponent<OcclusionPortal>();
        net_comp = GetComponent<NetworkObject>();
        
        thisTransform = transform;
        if(detectionMode == GateDetectionMode.New)
            detectionPosGlobal = detectionTransform.position;
    }
    
    void Start()
    {
        ProcessStatusLight(state);
    }
    
    bool Detect()
    {
        bool Result = false;
        switch(detectionMode)
        {
            case(GateDetectionMode.Old):
            {
                Result = Physics.CheckSphere(thisTransform.position, radius, detectorMask);
                break;
            }
            case(GateDetectionMode.New):
            {
                Result = Physics.CheckSphere(detectionPosGlobal, radius, detectorMask);
                break;
            }
        }
        
        return Result;
    }
    
    bool isClosed = true;
    
    void OnClosed()
    {
        isClosed = true;
        occlusion_portal.open = false;
    }
    
    void Update()
    {
        if(state != GateState.Locked)
        {
            if(Detect())
            {
                if(state == GateState.Closed)
                {
                    //Open:
                    // targetPosLeftLocal = openPosLeftLocal;
                    // targetPosRightLocal = openPosRightLocal;
                    
                    Open();
                    ProcessStatusLight(state);
                }
            }
            else
            {
                if(state == GateState.Open)
                {
                    //Close:
                    // targetPosLeftLocal = openPosLeftLocal;
                    // targetPosRightLocal = openPosRightLocal;
                    
                    Close();
                    ProcessStatusLight(state);
                }
            }
        }
        else
        {
            targetPosLeftLocal = closedPosLeftLocal;
            targetPosRightLocal = closedPosRightLocal;
            occlusion_portal.open = false;
            
            ProcessStatusLight(state);
        }
        
        if(Math.SqrDistance(leftGate.localPosition, targetPosLeftLocal) > 0.01F * 0.01F)
        {
            float dt = UberManager.DeltaTime();
            float dPos = dt * animationSpeed;
            
            leftGate.localPosition = Vector3.MoveTowards(leftGate.localPosition, targetPosLeftLocal, dPos);
            rightGate.localPosition = Vector3.MoveTowards(rightGate.localPosition, targetPosRightLocal, dPos);
        }
        else
        {
            if(state == GateState.Closed && !isClosed)
            {
                OnClosed();
            }
        }
    }   
    
    void Open()
    {
        state = GateState.Open;
        
        targetPosLeftLocal = openPosLeftLocal;
        targetPosRightLocal = openPosRightLocal;
        isClosed = false;
        occlusion_portal.open = true;
        
        audioSource.pitch = 1;
        audioSource.PlayOneShot(openClip);
        
        DoLight_open(thisTransform.position);
    }
    
    void DoLight_open(Vector3 pos)
    {
        GameObject g = ObjectPool2.s().Get(ObjectPoolKey.LightPooled, false);
        LightPooled light = g.GetComponent<LightPooled>();
        //Color color = Random.ColorHSV();
        Color color = new Color(0f, 1f, 0, 1);
        float decay_speed = 6 / 0.5f * 4;
        pos.y += 1F;
        float radius = 9;
        light.DoLight(pos, color, 1f, 5, radius, decay_speed);
    }
    
    void Close()
    {
        state = GateState.Closed;
        
        targetPosLeftLocal = closedPosLeftLocal;
        targetPosRightLocal = closedPosRightLocal;
        
        
        
        audioSource.pitch = 0.5f;
        audioSource.PlayOneShot(openClip);
    }
    
    
#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if(Application.isPlaying && detectionMode == GateDetectionMode.New)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(detectionPosGlobal, radius);
        }
    }
#endif
}
