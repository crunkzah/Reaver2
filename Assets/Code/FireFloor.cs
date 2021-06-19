using UnityEngine;
using Photon.Pun;

public class FireFloor : MonoBehaviour, INetworkObject
{
    NetworkObject net_comp;
    Bounds bounds;
    
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
            AudioManager.PlayClip(SoundType.Explosion_1, 1, 0.5f);
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
            bool isActiveNow = CheckIfPlayerIsInZone();
            if(!isActive && isActiveNow)
            {
                if(canSendCommands)
                {
                    LockSendingCommands();
                    
                    NetworkObjectsManager.PackNetworkCommand(net_comp.networkId, NetworkCommand.Attack);
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
    
    bool CheckIfPlayerIsInZone()
    {
        bool Result = false;
        
        //@Debug
        if(PhotonManager.Singleton())
        {
            int len = UberManager.Singleton().players.Count;
            
            for(int i = 0; i < len; i++)
            {
                GameObject player = UberManager.Singleton().players[i];
                if(player)
                {
                    if(bounds.Contains(player.transform.position))
                    {
                        // if(!isActive)
                        // {
                        //     player.GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.AllViaServer, 1000);
                        // }
                        Result = true;
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
