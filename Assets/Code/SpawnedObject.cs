using UnityEngine;
using Photon.Pun;

public class SpawnedObject : MonoBehaviour
{
    public EnemySpawner parent_enemySpawner;
    
    public void SetEnemySpawnerID(int spawnerID)
    {
        if(PhotonNetwork.IsMasterClient)
            parent_enemySpawner = EnemySpawner.all_spawners[spawnerID];
    }
    
    public void OnObjectDied()
    {
        if(!PhotonNetwork.IsMasterClient)
        {
            return;
        }
        
        
        if(parent_enemySpawner)
        {
            parent_enemySpawner.OnSpawnedChildDied();
        }
    }
}
