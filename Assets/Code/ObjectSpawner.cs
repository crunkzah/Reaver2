using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

public enum SpawnerMode 
{
    Item,
    Npc    
}

public class ObjectSpawner : MonoBehaviour, Interactable
{
    public ObjectPoolKey obj_key;
    
    public bool spawnImmediate = false;
    
    public SpawnerMode mode = SpawnerMode.Npc;
    
    public Vector3 droppedItemVelocity = new Vector3(0, 5f, 0);
  
        

    
    public void Interact()
    {
        if(mode == SpawnerMode.Item)
        {
            SpawnItem();                
        }
        
        
        if(mode == SpawnerMode.Npc)
        {
            SpawnNPC();
        }
    }
    

    
    void Start()
    {
              
//        return;
        
        
        
        if(spawnImmediate && PhotonNetwork.IsMasterClient)
        {
            Interact();
            
        }
    }
    
    bool spawnedNPC = false;

    
    void SpawnItem()
    {
        int net_id  = NetworkObjectsManager.SpawnNewItem(obj_key, transform.position, droppedItemVelocity);
    }
    
    readonly static Vector3 vForward = new Vector3(0, 0, 1);
    public float npc_spawn_delay = 5f;
    
    void SpawnNPC()
    {
        NavMeshHit navMeshHit;
        
        
        Vector3 spawn_pos = transform.position;
     
                
        if(NavMesh.SamplePosition(spawn_pos, out navMeshHit, 5f, NavMesh.AllAreas))
        {
            Vector3 finalPos = navMeshHit.position;
            
            Vector3 finalDir = transform.forward;
            
            
            //int net_id = GlobalShooter.Singleton().net_comp.networkId;
            //NetworkObjectsManager.ScheduleCommand(net_id, UberManager.GetPhotonTimeDelayedBy(0.1f), NetworkCommand.GlobalCommand, (int)GlobalShooter_ability.Spawn_effect, finalPos);
            var key = obj_key;
            NetworkObjectsManager.SpawnNewObject(key, finalPos, finalDir);
            //NetworkObjectsManager.ScheduleCommand(net_id, UberManager.GetPhotonTimeDelayedBy(npc_spawn_delay), NetworkCommand.GlobalCommand, (int)GlobalShooter_ability.Spawn_npc, obj_key, finalPos, finalDir);
        }
        else
        {
            InGameConsole.LogError(string.Format("{0} can't hit <color=red>navMesh</color>!", this.gameObject.name));
        }
    }
    
    

}
