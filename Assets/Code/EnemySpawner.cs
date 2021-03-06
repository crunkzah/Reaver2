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
	//int enemies_spawned;
	int currentSingleSpawnIndex;
	int enemies_spawned_fromSingleWave;
	int enemies_killed;
	int enemies_to_be_killed_from_single_wave;
	
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
		
		currentSingleSpawnIndex++;
		if(currentSingleSpawnIndex >= waves_to_spawn[waves_spawned].singleSpawn.Length)
		{
			currentSingleSpawnIndex = 0;
			OnOneWaveSpawned();
		}
		NPCType npc_type =  _singleSpawn.npc_to_spawn;
		//enemies_spawned++;
		//enemies_spawned_fromSingleWave++;
		
		NetworkObjectsManager.Singleton().SpawnNPC_Spawner((byte)(npc_type), pos, dir, this.instance_key);
	}
	
	
	public void Interact()
	{
		if(IsEnabled)
		{
			Invoke(nameof(InitialSpawn), initial_delay);
			if(PhotonNetwork.IsMasterClient)
			{
				if(messages_on_init_spawn != null)
				{
					for(int i = 0; i < messages_on_init_spawn.Length; i++)
					{
						if(messages_on_init_spawn[i].net_comp)
							NetworkObjectsManager.CallNetworkFunction(messages_on_init_spawn[i].net_comp.networkId, messages_on_init_spawn[i].command);
					}
				}
			}
		}
	}
	
	public NetworkObjectAndCommand[] messages_on_init_spawn;
	
	public void InitialSpawn()
	{
		if(state == EnemySpawnerState.Disabled)
		{
			enemies_to_be_killed_from_single_wave = waves_to_spawn[currentSingleSpawnIndex].singleSpawn.Length;
			state = EnemySpawnerState.Spawning;
		}
		
		if(PhotonNetwork.IsMasterClient)
		{
			NetworkObjectsManager.CallGlobalCommand(GlobalCommand.AddEnemiesAlive, RpcTarget.All, 1);
		}
		//AudioManager.AddEnemiesAlive();
		
	}
	
	int wavesSpawned = 0;
	
	//public GatesController[] gates_to_call;
	//public GameObject[] interactables_to_call;
	
	public NetworkObjectAndCommand[] things_to_call;
	
	
	void OnOneWaveSpawned()
	{
		//enemies_spawned = 0;
		//waves_spawned++;
		
		
		state = EnemySpawnerState.WaitingForNextWave;
	}
	
	
	
	public void SetStateSpawning()
	{
		state = EnemySpawnerState.Spawning;
		enemies_killed = 0;
		enemies_to_be_killed_from_single_wave = waves_to_spawn[waves_spawned].singleSpawn.Length;
		//if(waves_spawned < waves_to_spawn[waves_spawned].singleSpawn.Length)
	}
	
	public void OnSpawnedChildDied()
	{
		if(!PhotonNetwork.IsMasterClient)
		{
			return;
		}
		
		enemies_killed++;
		//InGameConsole.LogFancy("OnSpawnedChildDied() count is " + enemies_spawned.ToString());
		//if(enemies_spawned <= 0)
		if(enemies_killed >= enemies_to_be_killed_from_single_wave)
		{
			//enemies_spawned = 0;
			waves_spawned++;
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
		InGameConsole.LogFancy(string.Format("<color=yellow>OnAllWavesKilled() <color=green>{0}</color></color>", this.gameObject.name));
		state = EnemySpawnerState.Finished;
		CancelInvoke();
		
		int len = things_to_call.Length;
		for(int i = 0; i < things_to_call.Length; i++)
		{
			if(things_to_call[i].net_comp)
				NetworkObjectsManager.CallNetworkFunction(things_to_call[i].net_comp.networkId, things_to_call[i].command);
		}
		
		if(PhotonNetwork.IsMasterClient)
		{
			NetworkObjectsManager.CallGlobalCommand(GlobalCommand.AddEnemiesAlive, RpcTarget.All, -1);
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
					SpawnOneEnemyFromWave(ref waves_to_spawn[waves_spawned].singleSpawn[currentSingleSpawnIndex]);
					timer = 0;
					
					// if(enemies_spawned >= waves_to_spawn[waves_spawned].singleSpawn.Length)
					// {
					// 	OnOneWaveSpawned();
					// }
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
