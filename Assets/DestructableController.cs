using UnityEngine;
using UnityEngine.AI;

public class DestructableController : MonoBehaviour, INetworkObject
{
    FloorDestructable999[] floors;
    
    public NavMeshObstacle navObstacle;
    
    public ParticleSystem ps_destructable_blowUp;
    public ParticleSystem ps_destructable_floor_static;
    public AudioSource audio_src;
    public BoxCollider floor_col;
    
    NetworkObject net_comp;
    
    void Awake()
    {
        net_comp = GetComponent<NetworkObject>();
    }
    
    public void ReceiveCommand(NetworkCommand command, params object[] args)
    {
        switch(command)
        {
            case(NetworkCommand.Attack):
            {
                if(isAlive)
                {
                    Invoke(nameof(BlowUpAndDestroy), 3);
                }
                break;
            }
            default:
            {
                break;
            }
        }
    }
        
    void Start()
    {
        
        floors = FindObjectsOfType<FloorDestructable999>();
        int len = floors.Length;
        for(int i = 0; i < len; i++)
        {
            floors[i].transform.localPosition += new Vector3(240, 0, 0);
        }
    }
    
    bool isAlive = true;
    
    void BlowUpAndDestroy()
    {
        navObstacle.enabled = true;
        navObstacle.carving = true;
        isAlive = false;
        
        floor_col.enabled = false;
        
        audio_src.Play();
        ps_destructable_floor_static.Play();
        ps_destructable_blowUp.Play();
        
        PlayerController local_pc = PhotonManager.GetLocalPlayer();
        if(local_pc)
        {
            local_pc.BoostVelocity(new Vector3(0, 45f, 0));
        }
        
        for(int i = 0; i < floors.Length; i++)
        {
            floors[i].gameObject.SetActive(false);
        }
        
        InGameConsole.LogOrange("<color=red>Destroy()</color>");
    }
    
    void BecomeAlive()
    {
        navObstacle.enabled = false;
        isAlive = true;
        
        for(int i = 0; i < floors.Length; i++)
        {
            floors[i].gameObject.SetActive(true);
        }
        
        InGameConsole.LogOrange("<color=green>BecomeAlive()</color>");
    }
    
    void Update()
    {
        // if(Inputs.GetKeyDown(KeyCode.G))
        // {
        //     if(isAlive)
        //     {
        //         Destroy();
        //     }
        //     else
        //     {
        //         BecomeAlive();
        //     }
        // }
    }
}
