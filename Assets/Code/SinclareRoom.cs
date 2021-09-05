using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SinclareRoom : MonoBehaviour, INetworkObject
{
    NetworkObject net_comp;
    
    void Awake()
    {
        net_comp = GetComponent<NetworkObject>();
    }
    
    bool wasActivated = false;
    
    public void ReceiveCommand(NetworkCommand command, params object[] args)
    {
        switch(command)
        {    
            case(NetworkCommand.Ability1):
            {
                if(!wasActivated)
                {
                    wasActivated = true;
                    ShowRoom();
                }
                break;
            }
            case(NetworkCommand.Ability2):
            {
                if(wasActivated)
                {
                    HideRoom();
                    wasActivated = false;
                }
                break;
            }
            default:
            {
                break;
            }
        }
    }
    
    public Collider[] cols;
    public OcclusionPortal[] portals;
    public NavMeshObstacle[] obstacles;
    public ParticleSystem[] fires_ps;
    
    void Start()
    {
        Init();
    }
    
    void HideRoom()
    {
        for(int i = 0; i < cols.Length; i++)
        {
            cols[i].enabled = false;
        }
        
        for(int i = 0; i < portals.Length; i++)
        {
            portals[i].open = true;
        }
        
        for(int i = 0; i < obstacles.Length; i++)
        {
            obstacles[i].carving = false;
            obstacles[i].enabled = false;
        }
        
        for(int i = 0; i < fires_ps.Length; i++)
        {
            ParticleSystem.MainModule main = fires_ps[i].main;
            main.loop = false;
        }
        
        InGameConsole.LogFancy("Hide room");
    }
    
    
    void ShowRoom()
    {
        for(int i = 0; i < cols.Length; i++)
        {
            cols[i].enabled = true;
        }
        
        for(int i = 0; i < portals.Length; i++)
        {
            portals[i].open = false;
        }
        
        for(int i = 0; i < obstacles.Length; i++)
        {
            obstacles[i].carving = true;
            obstacles[i].enabled = true;
        }
        
        for(int i = 0; i < fires_ps.Length; i++)
        {
            fires_ps[i].Play();
        }
        
        InGameConsole.LogFancy("Show room");
    }
    
    void Init()
    {
        MeshRenderer[] rends = GetComponentsInChildren<MeshRenderer>();
        if(rends != null)
        {
            for(int i = 0; i < rends.Length; i++)
            {
                rends[i].enabled = false;
            }
        }
        
        for(int i = 0; i < cols.Length; i++)
        {
            cols[i].enabled = false;
        }
        
        for(int i = 0; i < portals.Length; i++)
        {
            portals[i].open = true;
        }
        
        for(int i = 0; i < obstacles.Length; i++)
        {
            obstacles[i].carving = true;
            //obstacles[i].carveOnlyStationary = true;
            obstacles[i].enabled = false;
        }
        
        for(int i = 0; i < fires_ps.Length; i++)
        {
            fires_ps[i].Stop();
        }
    }
    
    
}
