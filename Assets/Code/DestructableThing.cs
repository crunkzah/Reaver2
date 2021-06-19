using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DestructableThing : MonoBehaviour, IDamagableLocal, INetworkObject
{
    NetworkObject net_comp;
    
    public Collider col_npc2;
    public Collider col;
    
    NavMeshObstacle navMeshObstacle;
    AudioSource audioSource;
    
    public GameObject obj_to_destroy;
    public ParticleSystem ps;
    
    bool isDestroyed = false;
    
    void Awake()
    {
        net_comp = GetComponent<NetworkObject>();
        navMeshObstacle = GetComponent<NavMeshObstacle>();
        audioSource = GetComponent<AudioSource>();
    }
    
    public void ReceiveCommand(NetworkCommand command, params object[] args)
    {
        switch(command)
        {
            case(NetworkCommand.LaunchAirborne):
            {
                if(!isDestroyed)
                {
                    DestroyMe();
                }
                break;
            }
            default:
            {
                break;    
            }
        }
    }
    
    void DestroyMe()
    {
        InGameConsole.LogFancy("DestroyMe() on " + this.gameObject.name);
        col.enabled = false;
        col_npc2.enabled = false;
        navMeshObstacle.enabled = false;
        
        if(obj_to_destroy != null)
        {
            Destroy(obj_to_destroy);
        }
        
        audioSource.Play();
        ps.Play();
        
        isDestroyed = true;
    }
    
    public bool IsDead()
    {
     //   if(state == PadlaState.Dead)
     //       return true;
     //   else
            return false;
    }
    
    public int GetCurrentHP()
    {
        return 999999;
    }
    
    public void TakeDamageLocally(int dmg, Vector3 hitPoint, Vector3 bullet_dir)
    {
        
    }
}
