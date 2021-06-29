using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using ExitGames.Client.Photon;

public class PlayerInventory : MonoBehaviour, IOnEventCallback
{
    static PlayerInventory _instance;
    
    public static PlayerInventory Singleton()
    {
        if(_instance == null)
        {
            _instance = FindObjectOfType<PlayerInventory>();
        }
        
        return _instance;
    }
    
    private void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    private void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }
    
    public const byte GiveItemEventCode = 1;
    
    public void RaiseEventGiveWeaponToAllPlayers(GunType gunToGive)
    {
        if(PhotonNetwork.IsMasterClient)
        {
            object[] content = new object[] { (byte)gunToGive }; // Array contains the target position and the IDs of the selected units
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All }; // You would have to set the Receivers to All in order to receive this event on the local client as well
            PhotonNetwork.RaiseEvent(GiveItemEventCode, content, raiseEventOptions, SendOptions.SendReliable);
        }
    }
    
    public void OnEvent(EventData photonEvent)
    {
        InGameConsole.LogFancy(string.Format("OnEvent() {0}", photonEvent.Code));
        
        byte eventCode = photonEvent.Code;
        
        if (eventCode == GiveItemEventCode)
        {
            object[] data = (object[])photonEvent.CustomData;

            GunType gunToGive = (GunType)data[0];
            GiveWeaponLocally(gunToGive);
        }
        
    }
    
    void Start()
    {
        DontDestroyOnLoad(this);
        playerGunSlots[0] = GunType.Revolver;
        playerGunSlots[1] = GunType.Shotgun;
        playerGunSlots[2] = GunType.RocketLauncher;
        playerGunSlots[3] = GunType.AR;
    }
    
    
    public GunType[] playerGunSlots = new GunType[4];
    
    
    
    public void GiveWeaponLocally(GunType gunToGive)
    {
        switch(gunToGive)
        {
            case(GunType.Revolver):
            {
                playerGunSlots[0] = gunToGive;
                PlayerController localPlayer = PhotonManager.GetLocalPlayer();
                
                if(localPlayer)
                {
                    FPSGunController localFPSGunController = localPlayer.GetComponent<FPSGunController>();
                    localFPSGunController.ReadPlayerInventory();
                    localFPSGunController.WieldRevolver();
                }
                break;
            }
            case(GunType.Shotgun):
            {
                playerGunSlots[1] = gunToGive;
                
                PlayerController localPlayer = PhotonManager.GetLocalPlayer();
                
                if(localPlayer)
                {
                    FPSGunController localFPSGunController = localPlayer.GetComponent<FPSGunController>();
                    localFPSGunController.ReadPlayerInventory();
                    localFPSGunController.WieldShotgun();
                }
                
                break;
            }
            case(GunType.RocketLauncher):
            {
                playerGunSlots[2] = gunToGive;
                break;
            }
            case(GunType.AR):
            {
                playerGunSlots[3] = gunToGive;
                break;
            }
            default:
            {
                break;
            }
        }
        
       
    }
}
