using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

[System.Serializable]
public struct ArenaUnit
{
    public string name;
    public ObjectPoolKey poolKey;
    public Unit type;
    public float spawnAfterTime;
    public float spawnRate;
    public int numberPerSpawn;
    public float lastTimeSpawned;
}

public enum Unit
{
    Small,
    Big
}

public class ArenaSpawnManager : MonoBehaviour
{
    
    bool isWorking = false;
    
    [Header("Units:")]
    public ArenaUnit[] units;
    
    [Header("Unit spawns:")]
    public Transform[] smallSpawns;
    public Transform[] bigSpawns;
    
    public float spawnRadius = 10f;
    
    public float arenaTimer;
    
    static readonly Quaternion qZero = Quaternion.identity;
    
    public int maxTimesToSpawn = 3;
    int timesSpawned = 0;
    
    
    
    [Header("Events:")]
    bool spawnInitialGunsHappened = false;
    
    float uberTime;
    
    void Update()
    {
        if(PhotonNetwork.IsMasterClient)
        {
            if(isWorking)
            {
                arenaTimer += UberManager.DeltaTime();
                
                uberTime = UberManager.TimeSinceStart();
                
                SmallUnitSpawning();
                
                SpawnGuns();
            }
            else
            {
                if(PhotonManager.Singleton().local_player_gameObject)
                {
                    StartArena();
                }
            }
        }
    }
    
    
    void StartArena()
    {
        isWorking = true;
        InGameConsole.LogOrange("!!! ARENA STARTING !!!");
    }
    
    void SmallUnitSpawning()
    {
        if(timesSpawned > maxTimesToSpawn)
            return;
        
        int len = units.Length;
        
        for(int i = 0; i < len; i++)
        {
            if(arenaTimer > units[i].spawnAfterTime)
            {
                if(uberTime > (units[i].lastTimeSpawned + units[i].spawnRate))
                {
                    units[i].lastTimeSpawned = uberTime;
                    
                    NavMeshHit navMeshHit;
                    Vector3 spawnPos;
                    
                    bool f = false;
                    
                    for(int j = 0; j < units[i].numberPerSpawn; j++)
                    {   
                        spawnPos = f ? smallSpawns[0].position : smallSpawns[1].position;
                        
                        spawnPos += new Vector3(1, 0, 1) * spawnRadius;
                        
                        if(NavMesh.SamplePosition(spawnPos, out navMeshHit, 150, NavMesh.AllAreas))
                        {
                            timesSpawned++;
                            int spawnNetId = NetworkObjectsManager.SpawnNewObject(units[i].poolKey, navMeshHit.position, Vector3.forward);
                            
                            Vector3 arenaCenterDest = new Vector3(0, 0, 0) + new Vector3(1, 0, 1) * Random.Range(3, 8);
                            
                            NetworkObjectsManager.ScheduleCommand(spawnNetId, PhotonNetwork.Time + 0.25d, NetworkCommand.Move, arenaCenterDest);
                            
                            //NetworkObjectsManager.PackNetworkCommand(spawnNetId, NetworkCommand.Move, arenaCenterDest);
                        }
                        else
                        {
                            InGameConsole.LogWarning(string.Format("Couldn't sample position for {0}", this.gameObject.name));
                        }
                        
                        f = !f;
                        
                        
                    }
                }
            }
        }
    }
    
    
    void SpawnGuns()
    {
        if(!spawnInitialGunsHappened)
        {
            spawnInitialGunsHappened = true;
            
            // NetworkObjectsManager.CreateNetworkObject(SceneNetObject.GLOCK_DROPPED, new Vector3(0, 3, 0) + new Vector3(Random.Range(0, 3), 0, Random.Range(0, 3)), qZero);
            // NetworkObjectsManager.CreateNetworkObject(SceneNetObject.PISTOL_AMMO_DROPPED, new Vector3(0, 3, 0) + new Vector3(Random.Range(0, 3), 0, Random.Range(0, 3)), qZero);
            
            // NetworkObjectsManager.CreateNetworkObject(SceneNetObject.MASTIFF_DROPPED, new Vector3(0, 3, 0) + new Vector3(Random.Range(0, 6), 0, Random.Range(0, 6)), qZero);
            //NetworkObjectsManager.CreateNetworkObject(SceneNetObject.SHOTGUN_AMMO_DROPPED, new Vector3(0, 3, 0) + new Vector3(Random.Range(0, 6), 0, Random.Range(0, 6)), qZero);
            
            // NetworkObjectsManager.CreateNetworkObject(SceneNetObject.CROSSBOW_AMMO_DROPPED, new Vector3(0, 3, 0) + new Vector3(Random.Range(0, 3), 0, Random.Range(0, 3)), qZero);
            // NetworkObjectsManager.CreateNetworkObject(SceneNetObject.CROSSBOW_DROPPED, new Vector3(0, 3, 0) + new Vector3(Random.Range(0, 3), 0, Random.Range(0, 3)), qZero);
            
            //NetworkObjectsManager.CreateNetworkObject(SceneNetObject.HEAVYBOLTER_AMMO_DROPPED, new Vector3(0, 3, 0) + new Vector3(Random.Range(0, 6), 0, Random.Range(0, 6)), qZero);
            //NetworkObjectsManager.CreateNetworkObject(SceneNetObject.HEAVYBOLTER_DROPPED, new Vector3(0, 3, 0) + new Vector3(Random.Range(0, 6), 0, Random.Range(0, 6)), qZero);
            
            int netId = NetworkObjectsManager.SpawnNewItem(ObjectPoolKey.HeavyBolterDropped, new Vector3(0, 2, 0) + new Vector3(Random.Range(0, 6), 0, Random.Range(0, 6)), new Vector3(0, 4, 0));
            // NetworkObjectsManager.Singleton().runtimePool[netId].GetComponent<IPooledObject>().InitialState();
            netId = NetworkObjectsManager.SpawnNewItem(ObjectPoolKey.HeavyBolterAmmoDropped, new Vector3(0, 4, 0) + new Vector3(Random.Range(0, 6), 0, Random.Range(0, 6)), new Vector3(0, 4, 0));
            // NetworkObjectsManager.Singleton().runtimePool[netId].GetComponent<IPooledObject>().InitialState();
            
            InGameConsole.LogOrange("SpawnGuns()");
        }
    }
    
    
}
