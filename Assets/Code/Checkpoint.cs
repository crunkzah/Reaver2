using UnityEngine;
using Photon.Pun;
using TMPro;

public enum CheckpointState : byte
{
    Waiting,
    Activated
}

public enum CheckpointMode : byte
{
    Visible,
    Invisible
}

public class Checkpoint : MonoBehaviour, INetworkObject
{
    public CheckpointMode mode;
    public CheckpointState state;
    
    void Init()
    {
        switch(mode)
        {
            case(CheckpointMode.Visible):
            {
                break;
            }
            case(CheckpointMode.Invisible):
            {
                Renderer[] rends = GetComponentsInChildren<Renderer>();
                for(int i = 0; i < rends.Length; i++)
                {
                    rends[i].enabled = false;
                }
                audio_src.enabled = false;
                
                break;
            }
        }
    }
    
    void Start()
    {
        Init();
    }
    
    NetworkObject net_comp;
    Transform thisTransform;
    
    
    AudioSource audio_src;
    Light checkPoint_light;
    
    public MeshRenderer[] gears_renders;
    
    public ParticleSystem activated_ps;
    public ParticleSystem static_ps;
    
    public TextMeshPro label_tmp;
    
    public SlidingStructure[] sliding_structures_to_call_on_activate;
    
    int playersMask = -1;
    
    bool canSendCommands = true;
    
    void LockSendingCommands()
    {
        canSendCommands = false;
    }
    
    void UnlockSendingCommands()
    {
        canSendCommands = true;
    }
    
    public void ReceiveCommand(NetworkCommand command, params object[] args)
    {
        switch(command)
        {
            case(NetworkCommand.Ability1):
            {
                UnlockSendingCommands();
                Activate();
                
                break;
            }
            default:
            {
                break;
            }
        }
    }
    
    static bool DEBUG_disable_checkpoints = false;
    
    [Header("Objects to send message on activation, Master only:")]
    public GameObject[] objects_to_activate;
    
    void Awake()
    {
        net_comp = GetComponent<NetworkObject>();
        audio_src = GetComponent<AudioSource>();
        playersMask = LayerMask.GetMask("Player");
        thisTransform = transform;
        
        if(DEBUG_disable_checkpoints)
        {
            this.enabled = false;
        }
        
        checkPoint_light = GetComponentInChildren<Light>();
    }
    
    bool CheckIfPlayersInArea()
    {
        bool Result = true;
        
        int playersCount = NPCManager.AITargets().Count;
        
        if(playersCount == 0)
        {
            return false;
        }
        
        for(int i = 0; i < playersCount; i++)
        {
            float sqrDistanceToPlayer = Math.SqrDistance(NPCManager.AITargets()[i].localPosition, thisTransform.position);
            if(sqrDistanceToPlayer < radius * radius)
            {
                Result = true;
            }
            else
                return false;
        }
        
        return Result;
    }
    
    
    public Material waitingMat;
    public Material activatedMat;
    
    void Activate_FancyThings()
    {
        audio_src.pitch = 1;
        audio_src.volume = 0.33F;
        audio_src.maxDistance = 3f;
        
        for(int i = 0; i < gears_renders.Length; i++)
        {
            gears_renders[i].sharedMaterial = activatedMat;
        }
        
        checkPoint_light.enabled = false;
        DoLight(transform.position);
        
        activated_ps.Play();
        static_ps.Stop();
        
        thisTransform.localScale = new Vector3(0.2f, 0.5f, 0.5f) * 0.7f;
        
        label_tmp.CrossFadeColor(new Color(0, 0, 0, 0.33f), 0.5f, false, true);
    }
    
    public void DoLight(Vector3 pos)
    {
        pos.y += 0.1f;
        GameObject light_obj = ObjectPool2.s().Get(ObjectPoolKey.LightPooled, false);
        LightPooled light =  light_obj.GetComponent<LightPooled>();
        Color color = new Color(0.9f, 0.4f, 0.6f);
        
        light.DoLight(pos, color, 1, 5, 7, 10);
    }
    
    void Activate()
    {
        if(state == CheckpointState.Waiting)
        {
            //InGameConsole.LogFancy(string.Format("Activated Checkpoint on {0}({1})", this.gameObject.name, this.net_comp.networkId));
            state = CheckpointState.Activated;
            switch(mode)
            {
                case(CheckpointMode.Invisible):
                {
                    break;
                }
                case(CheckpointMode.Visible):
                {
                    Activate_FancyThings();
                    break;
                }
            }
            
            
            
            
            for(int i = 0; i < sliding_structures_to_call_on_activate.Length; i++)
            {
                sliding_structures_to_call_on_activate[i].ToggleSlide();
            }
            
            
            if(PhotonNetwork.IsMasterClient)
            {
                for(int i = 0; i < objects_to_activate.Length; i++)
                {
                    Interactable _int = objects_to_activate[i].GetComponent<Interactable>();
                    if(_int == null)
                    {
                        InGameConsole.LogOrange("Interactable is null...");
                    }
                    else
                    {
                        _int.Interact();
                    }
                }
            }
        }
    }
    
    void Deactivate()
    {
        if(state == CheckpointState.Activated)
        {
            InGameConsole.LogFancy(string.Format("Activated Checkpoint on {0}({1})", this.gameObject.name, this.net_comp.networkId));
            state = CheckpointState.Activated;
            
            audio_src.pitch = -0.75F;
            audio_src.volume = 0.65F;
            audio_src.maxDistance = 16;
            
            checkPoint_light.enabled = true;
            
            static_ps.Play();
            
            thisTransform.localScale = new Vector3(0.2f, 0.5f, 0.5f);
            
            for(int i = 0; i < gears_renders.Length; i++)
            {
                gears_renders[i].sharedMaterial = waitingMat;
            }
        }
    }
    
    
    public float radius = 2.0F;
    
    
    void Update()
    {
        if(!PhotonNetwork.IsMasterClient)
        {
            return;
        }
        
        switch(state)
        {
            case(CheckpointState.Waiting):
            {
                if(canSendCommands)
                {
                    if(CheckIfPlayersInArea())
                    {
                        LockSendingCommands();
                        NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.Ability1);
                    }
                }
                
                
                break;
            }
            case(CheckpointState.Activated):
            {
                break;
            }
        }
    }
}
