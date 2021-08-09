using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;
using System.Collections.Generic;

public enum EnemySpawnerMode : byte
{
	None,
	Waves,
}

public enum EnemySpawnerState : byte
{
	Disabled,
	Spawning,
	WaitingForNextWave,
	Finished
}

[System.Serializable]
public struct SingleSpawn
{
	public string name;
	public Transform pos_tr;
	public NPCType npc_to_spawn;
}

[System.Serializable]
public struct SingleWave
{
	public string name;
	public SingleSpawn[] singleSpawn;
}

[System.Serializable]
public struct NetworkObjectAndCommand
{
	public NetworkObject net_comp;
	public NetworkCommand command;
}

public class EnemySpawner : MonoBehaviour, Interactable
{
	public bool IsEnabled = true;
	
	public float initial_delay = 1f;
	public float waves_delay = 0.5F;
	public float single_spawn_delay = 0.25F;
	
	
	public static Dictionary<int, EnemySpawner> all_spawners = new Dictionary<int, EnemySpawner>();
	
	int instance_key;
	
	void Awake()
	{
		instance_key = this.GetInstanceID();
		all_spawners.Add(instance_key, this);
	}
	
	float timer = 0;
	
	public EnemySpawnerMode mode;
	public EnemySpawnerState state;
	
	
	
	[Header("Waves:")]
	public SingleWave[] waves_to_spawn;
	
	int waves_spawned;
	int enemies_spawned;
	
	void SpawnOneEnemyFromWave(ref SingleSpawn _singleSpawn)
	{
		NavMeshHit navMeshHit;
		Vector3 pos = _singleSpawn.pos_tr.position;
		Vector3 dir = _singleSpawn.pos_tr.forward;
		dir.y = 0;
		dir.Normalize();
		
		if(NavMesh.SamplePosition(pos, out navMeshHit, 1F, NavMesh.AllAreas))
		{
			pos = navMeshHit.position;
		}
		// else
		// {
		// 	InGameConsole.LogOrange("Couldn't spawn <color=green>NPC</color> because couldn't hit NavMesh - <color=yellow>DebugSpawnOnClick()</color>");
		// }
		NPCType npc_type =  _singleSpawn.npc_to_spawn;
		enemies_spawned++;
		NetworkObjectsManager.Singleton().SpawnNPC_Spawner((byte)(npc_type), pos, dir, this.instance_key);
	}
	
	
	public void Interact()
	{
		if(IsEnabled)
		{
			Invoke(nameof(InitialSpawn), initial_delay);
			if(PhotonNetwork.IsMasterClient)
			{
				for(int i = 0; i < messages_on_init_spawn.Length; i++)
				{
					NetworkObjectsManager.CallNetworkFunction(messages_on_init_spawn[i].net_comp.networkId, messages_on_init_spawn[i].command);
				}
			}
		}
	}
	
	public NetworkObjectAndCommand[] messages_on_init_spawn;
	
	public void InitialSpawn()
	{
		if(state == EnemySpawnerState.Disabled)
		{
			
			state = EnemySpawnerState.Spawning;
		}
		
		
	}
	
	int wavesSpawned = 0;
	
	//public GatesController[] gates_to_call;
	//public GameObject[] interactables_to_call;
	
	public NetworkObjectAndCommand[] things_to_call;
	
	
	void OnOneWaveSpawned()
	{
		//enemies_spawned = 0;
		waves_spawned++;
		
		state = EnemySpawnerState.WaitingForNextWave;
	}
	
	
	
	public void SetStateSpawning()
	{
		state = EnemySpawnerState.Spawning;
	}
	
	public void OnSpawnedChildDied()
	{
		if(!PhotonNetwork.IsMasterClient)
		{
			return;
		}
		
		enemies_spawned--;
		//InGameConsole.LogFancy("OnSpawnedChildDied() count is " + enemies_spawned.ToString());
		if(enemies_spawned <= 0)
		{
			enemies_spawned = 0;
			
			if(waves_spawned >= waves_to_spawn.Length)
			{
				OnAllWavesKilled();
			}
			else
			{
				Invoke(nameof(SetStateSpawning), waves_delay);		
				
			}
		}
	}
	
	void OnAllWavesKilled()
	{
		InGameConsole.LogFancy("<color=yellow>OnAllWavesKilled()</color>");
		state = EnemySpawnerState.Finished;
		CancelInvoke();
		
		int len = things_to_call.Length;
		for(int i = 0; i < things_to_call.Length; i++)
		{
			
			NetworkObjectsManager.CallNetworkFunction(things_to_call[i].net_comp.networkId, things_to_call[i].command);
		}
	}
	
	void Update()
	{
		if(!PhotonNetwork.IsMasterClient)
		{
			return;
		}
		
		switch(state)
		{
			case(EnemySpawnerState.Disabled):
			{
				break;
			}
			case(EnemySpawnerState.Spawning):
			{
				
				float dt = UberManager.DeltaTime();
				timer += dt;
				if(timer >= single_spawn_delay)
				{
					// int len = waves_to_spawn[waves_spawned].singleSpawn.Length;
					// if(enemies_spawned >= waves_to_spawn[waves_spawned].singleSpawn.Length)
					// {
					// 	InGameConsole.LogFancy(string.Format("enemies_spawned >= singleSpawn.Length, {0} >= {1}", enemies_spawned, len));
					// }
					
					SpawnOneEnemyFromWave(ref waves_to_spawn[waves_spawned].singleSpawn[enemies_spawned]);
					timer = 0;
					
					if(enemies_spawned >= waves_to_spawn[waves_spawned].singleSpawn.Length)
					{
						OnOneWaveSpawned();
					}
				}
				
				break;
			}
			case(EnemySpawnerState.WaitingForNextWave):
			{
				
				break;
			}
			case(EnemySpawnerState.Finished):
			{
				
				break;
			}
		}
	}
}
