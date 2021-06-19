using UnityEngine;
using Photon.Pun;

public class DetectBothPlayers : MonoBehaviour, INetworkObject
{
    NetworkObject net_comp;
    Bounds bounds;
    
    
    public GameObject[] interactables_to_call;
    
    void Awake()
    {
        net_comp = GetComponent<NetworkObject>();
        
        
        BoxCollider boxCol = GetComponent<BoxCollider>();
        if(boxCol)
        {
            bounds = boxCol.bounds;
        }
        
        mask = LayerMask.GetMask("Player");
    }
    
    bool canSendCommands = true;
    
    void LockSendingCommands()
    {
        canSendCommands = false;
    }
    
    void UnlockSendingCommands()
    {
        canSendCommands = true;
    }
    
    public void ReceiveCommand(NetworkCommand command, params object[] args)
    {
        switch(command)
        {
            case NetworkCommand.Attack:
            {
                UnlockSendingCommands();
                DoAttack();
                break;
            }
            case NetworkCommand.Ability1:
            {
                UnlockSendingCommands();
                StopAttacking();
                break;
            }
        }
    }
    
    
    
    void DoAttack()
    {
        isActive = true;
        if(!ps.isPlaying)
        {
            ps.Play();
            // AudioManager.PlayClip(SoundType.Explosion_1, 1, 0.9f);
        }
        
        int len = interactables_to_call.Length;
        
        for(int i = 0; i < len; i++)
        {
            interactables_to_call[i].GetComponent<Interactable>().Interact();
        }
    }
    
    void StopAttacking()
    {
        isActive = false;
        if(ps.isPlaying)
        {
            ps.Stop();
        }
    }
    
    public ParticleSystem ps;
    
    public bool isActive = false;
    
    void Update()
    {
        if(PhotonNetwork.IsMasterClient)
        {
            bool isActiveNow = CheckIfBothPlayersAreInZone();
            if(!isActive && isActiveNow)
            {
                if(canSendCommands)
                {
                    LockSendingCommands();
                    
                    NetworkObjectsManager.ScheduleCommand(net_comp.networkId, UberManager.GetPhotonTimeDelayedBy(0.1f), NetworkCommand.Attack);
                }
            }
            else
            {
                if(isActive && !isActiveNow)
                {
                    if(canSendCommands)
                    {
                        LockSendingCommands();
                        
                        NetworkObjectsManager.PackNetworkCommand(net_comp.networkId, NetworkCommand.Ability1);
                    }
                }
            }
        }
    }
    
    readonly static Quaternion qZero = Quaternion.identity;
    int mask;
    
    bool CheckIfBothPlayersAreInZone()
    {
        bool Result = true;
        
        //@Debug
        if(UberManager.Singleton())
        {
            if(UberManager.Singleton().players.Count == 0)
            {
                {
                    Result = false;
                }
            }
            
            
            int len = UberManager.Singleton().players.Count;
            
            for(int i = 0; i < len; i++)
            {
                GameObject player = UberManager.Singleton().players[i];
                if(player)
                {
                    if(!bounds.Contains(player.transform.position))
                    {
                        Result = false;
                        break;
                    }
                }
            }
        }
        
        return Result;
    }
    
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if(Application.isPlaying)
        {
            Gizmos.color = isActive ? Color.red : Color.blue;
            
            Gizmos.DrawWireCube(bounds.center, bounds.size);
                
            // Gizmos.DrawWireCube(boundsCenter, boundsExtents * 2);
        }
    }
#endif
}
