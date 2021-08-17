using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

public class DestructableController : MonoBehaviour, INetworkObject
{
    FloorDestructable999[] floors;
    
    public NavMeshObstacle navObstacle;
    
    public ParticleSystem ps_destructable_blowUp;
    public ParticleSystem ps_destructable_floor_static;
    public AudioSource audio_src;
    public BoxCollider floor_col;
    
    public GameObject[] objects_to_set_active;
    
    public float delay = 3F;
    
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
                    Invoke(nameof(BlowUpAndDestroy), delay);
                }
                break;
            }
            default:
            {
                break;
            }
        }
    }
        
    public Vector3 offsetToPlaceBeforeBlowUp = new Vector3(240, 0, 0);
    
    public FloorDestructable999[] premadeDestructables;
        
    void Start()
    {
        
        if(premadeDestructables == null || premadeDestructables.Length == 0)
        {
            floors = FindObjectsOfType<FloorDestructable999>();
            int len = floors.Length;
            for(int i = 0; i < len; i++)
            {
                floors[i].transform.localPosition += offsetToPlaceBeforeBlowUp;
            }
        }
        else
        {
            int len = premadeDestructables.Length;
            for(int i = 0; i < len; i++)
            {
                premadeDestructables[i].transform.localPosition += offsetToPlaceBeforeBlowUp;
            }
            
        }
    }
    
    bool isAlive = true;
    
    public Vector3 boostVelocity = new Vector3(0, 45f, 0);
    
    void BlowUpAndDestroy()
    {
        if(navObstacle)
        {
            navObstacle.enabled = true;
            navObstacle.carving = true;
        }
        isAlive = false;
        
        floor_col.enabled = false;
        
        audio_src.Play();
        
        if(ps_destructable_floor_static)
            ps_destructable_floor_static.Play();
        ps_destructable_blowUp.Play();
        
        PlayerController local_pc = PhotonManager.GetLocalPlayer();
        if(local_pc)
        {
            if(Math.SqrMagnitude(boostVelocity) > 0)
            {
                local_pc.BoostVelocity(boostVelocity);
            }
        }
        
        int len = 0;
        
        if(floors != null)
        {
            len = floors.Length;
            for(int i = 0; i < len; i++)
            {
                floors[i].gameObject.SetActive(false);
            }
        }
        
        if(premadeDestructables != null)
        {
            // len = premadeDestructables.Length;
            for(int i = 0; i < premadeDestructables.Length; i++)
            {
                premadeDestructables[i].gameObject.SetActive(false);
            }
        }
        
        if(objects_to_set_active != null)
        {
            for(int i = 0; i < objects_to_set_active.Length; i++)
            {
                objects_to_set_active[i].SetActive(true);
            }
        }
        
        //InGameConsole.LogOrange("<color=red>Destroying destructables()</color>");
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
