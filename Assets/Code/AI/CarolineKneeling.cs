using UnityEngine;
using Photon.Pun;

public class CarolineKneeling : MonoBehaviour, INetworkObject
{
    public Transform shotgun_transform;
    
    public void ReceiveCommand(NetworkCommand command, params object[] args)
    {
        switch(command)
        {
            case(NetworkCommand.Ability1):
            {
                //UnlockSendingCommands();
                
                DoLight(shotgun_transform.position);
                shotgun_transform.gameObject.SetActive(false);
                //MessagePanel.Singleton().ShowMessage("Fire")
                
                break;
            }
            default:
            {
                break;
            }
        }
    }
    
    
    NetworkObject net_comp;
    void Awake()
    {
        net_comp = GetComponent<NetworkObject>();
    }
    
    void DoLight(Vector3 pos)
    {
        GameObject g = ObjectPool2.s().Get(ObjectPoolKey.LightPooled, false);
        LightPooled light = g.GetComponent<LightPooled>();
        Color color = new Color(1f, 0.71f, 0.1f, 1f);
        
        float decay_speed = 5;
        light.DoLight(pos, color, 3f, 14, 3, decay_speed);
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
    
    public NetworkObjectAndCommand[] messages_on_pickup;
    
    void Update()
    {
        if(canSendCommands && PhotonNetwork.IsMasterClient)
        {
            for(int i = 0; i < NPCManager.AITargets().Count; i++)
            {
                PlayerController playerTarget = NPCManager.AITargets()[i].GetComponent<PlayerController>();
                if(playerTarget)
                {
                    float distance_to_revolver = Math.SqrDistance(shotgun_transform.position, playerTarget.GetHeadPosition());
                    //InGameConsole.LogFancy("distance_to_revolver is " + distance_to_revolver.ToString("f"));
                    if(distance_to_revolver < 1.6f * 1.6f)
                    {
                        LockSendingCommands();
                        NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.Ability1);
                        if(messages_on_pickup != null)
                        {
                            for(int j = 0; j < messages_on_pickup.Length; j++)
                            {
                                NetworkObjectsManager.CallNetworkFunction(messages_on_pickup[j].net_comp.networkId, messages_on_pickup[j].command);
                            }
                        }
                        PlayerInventory.Singleton().RaiseEventGiveWeaponToAllPlayers(GunType.Revolver);
                    }
                }
            }
            
            // PlayerController masterPlayer = PhotonManager.GetLocalPlayer();
            // if(masterPlayer)
            // {
            //     float distance_to_revolver = Vector3.Distance(shotgun_transform.position, masterPlayer.GetHeadPosition());
            //     //InGameConsole.LogFancy("distance_to_revolver is " + distance_to_revolver.ToString("f"));
            //     if(distance_to_revolver < 1.5f)
            //     {
            //         LockSendingCommands();
            //         NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.Ability1);
            //         PlayerInventory.Singleton().RaiseEventGiveWeaponToAllPlayers(GunType.Shotgun);
            //     }
            // }
        }
    }
}
