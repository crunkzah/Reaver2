using UnityEngine;
using Photon.Pun;


public class PressureButton : MonoBehaviour, IActivatable
{
   
    NetworkObject net_comp;
    
    Transform thisTransform;
    
    public Renderer emissiveRenderer;
    
    public Material activeMat;
    public Material inactiveMat;
    
    void Awake()
    {
        thisTransform = transform;
    }
    
    void Start()
    {
        net_comp = GetComponent<NetworkObject>();
    }
    
    public LayerMask mask;
    public bool standingOn = false;
    
    
    void Update()
    {
        if(PhotonNetwork.IsMasterClient)
        {
            bool isStandingOnNow = CheckPressure();
            
            if(isStandingOnNow)
            {
                if(!standingOn)
                {
                    standingOn = true;
                    //Ask all of clients to activate button:
                    NetworkObjectsManager.Activate(net_comp.networkId);
                }
            }
            else
            {
                if(standingOn)
                {
                    standingOn = false;
                    
                    NetworkObjectsManager.Deactivate(net_comp.networkId);
                }                
            }
        }
    }
    
    public GameObject[] linkeds;
    
    
    public void Activate()
    {
        emissiveRenderer.sharedMaterial = activeMat;
        
        InteractWithLinkeds();
    }
    
    void InteractWithLinkeds()
    {
        if(linkeds != null)
        {
            int len = linkeds.Length;
            
            for(int i  = 0; i < len; i++)
            {
                linkeds[i].GetComponent<Interactable>().Interact();    
            }
        }
    }
    
    public void Deactivate()
    {
        emissiveRenderer.sharedMaterial = inactiveMat;
        
        InteractWithLinkeds();
    }
    
    static float radius = 1.2f;
    static Vector3 spherePos = new Vector3(0, 1.3f, 0);
    
    Collider[] standings = new Collider[1];
    
    
    bool CheckPressure()
    {
        bool Result = false;
        
        Vector3 pos = thisTransform.position + spherePos;
        
        int numStandings = Physics.OverlapSphereNonAlloc(pos, radius, standings, mask);
        
        if(numStandings > 0)
        {
            Result = true;
        }
        
        return Result;
    }
    
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        
        Gizmos.DrawWireSphere(transform.position + spherePos, radius);
    }
#endif
}
