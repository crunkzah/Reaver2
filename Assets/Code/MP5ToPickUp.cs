using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MP5ToPickUp : MonoBehaviour, INetworkObject
{
    NetworkObject net_comp;
    
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
            case(NetworkCommand.Ability1):
            {
                Destroy(this.gameObject, 0.05f);
                break;
            }
            default:
            {
                break;
            }
        }
    }
    
    void Awake()
    {
        net_comp = GetComponent<NetworkObject>();
    }
    
    public GunType gunTypeToGive = GunType.AR;
    
    void Update()
    {
        if(canSendCommands && PhotonNetwork.IsMasterClient)
        {
            ref List<PlayerController> pcs = ref UberManager.Singleton().playerControllers;
            int len = pcs.Count;
            for(int i = 0; i < len; i++)
            {
                PlayerController playerTarget = pcs[i];
                if(playerTarget)
                {
                    float distance_to_mp5 = Math.SqrDistance(transform.localPosition, playerTarget.GetHeadPosition());
                    if(distance_to_mp5 < 2.5f * 2.5f)
                    {
                        LockSendingCommands();
                        NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.Ability1);
                        // if(messages_on_pickup != null)
                        // {
                        //     for(int j = 0; j < messages_on_pickup.Length; j++)
                        //     {
                        //         NetworkObjectsManager.CallNetworkFunction(messages_on_pickup[j].net_comp.networkId, messages_on_pickup[j].command);
                        //     }
                        // }
                        PlayerInventory.Singleton().RaiseEventGiveWeaponToAllPlayers(gunTypeToGive);
                    }
                }
            }
        }
    }
}
