using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using ExitGames.Client.Photon;

public class PlayerInventory : MonoBehaviour
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
    
    // private void OnEnable()
    // {
    //     PhotonNetwork.AddCallbackTarget(this);
    // }

    // private void OnDisable()
    // {
    //     PhotonNetwork.RemoveCallbackTarget(this);
    // }
    
    
    public void RaiseEventGiveWeaponToAllPlayers(GunType gunToGive)
    {
        if(PhotonNetwork.IsMasterClient)
        {
            object[] content = new object[] { (byte)gunToGive }; // Array contains the target position and the IDs of the selected units
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All }; // You would have to set the Receivers to All in order to receive this event on the local client as well
            PhotonNetwork.RaiseEvent(EventCodes.GiveItem, content, raiseEventOptions, SendOptions.SendReliable);
        }
    }
    // public const byte GiveItemEventCode = 1;
    
    // public void OnEvent(EventData photonEvent)
    // {
    //     //InGameConsole.LogFancy(string.Format("OnEvent() {0}", photonEvent.Code));
        
    //     byte eventCode = photonEvent.Code;
        
    //     if (eventCode == GiveItemEventCode)
    //     {
    //         object[] data = (object[])photonEvent.CustomData;
    //         // for(int i = 0; i < data.Length; i++)
    //         // {
    //         //     InGameConsole.LogFancy("Data: <color=yellow>" + data[i].ToString() + "</color>");
    //         // }

    //         GunType gunToGive = (GunType)data[0];
    //         GiveWeaponLocally(gunToGive);
    //     }
        
    // }
    //[SerializeField]
    public GunType[][] playerGunSlots;
    
    
    bool wasInit = false;
    
    public void Init()
    {
        if(wasInit)
        {
            return;
        }
        
        wasInit = true;
        
        playerGunSlots = new GunType[4][];
        for(int i = 0; i < playerGunSlots.Length; i++)
        {
            playerGunSlots[i] = new GunType[2];
            for(int j = 0; j < playerGunSlots[i].Length; j++)
            {
                playerGunSlots[i][j] = GunType.None;
            }
        }
    }
    
    void Start()
    {
        Init();
        //We already do this in UberManager
        //DontDestroyOnLoad(this);
        playerGunSlots[0][0] = GunType.Revolver;    
        playerGunSlots[0][1] = GunType.Revolver_blue;
        playerGunSlots[1][0] = GunType.Shotgun;
        playerGunSlots[1][1] = GunType.Shotgun_blue;
        playerGunSlots[2][0] = GunType.AR;
        playerGunSlots[2][1] = GunType.MP5_alt;
        playerGunSlots[3][0] = GunType.RocketLauncher;
    }
    
    
    
    
    
    public void GiveWeaponLocally(GunType gunToGive)
    {
        PlayerController localPlayer = PhotonManager.GetLocalPlayer();
        switch(gunToGive)
        {
            case(GunType.Revolver):
            {
                playerGunSlots[0][0] = gunToGive;
                
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
                playerGunSlots[1][0] = gunToGive;
                
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
                playerGunSlots[3][0] = gunToGive;
                
                if(localPlayer)
                {
                    FPSGunController localFPSGunController = localPlayer.GetComponent<FPSGunController>();
                    localFPSGunController.ReadPlayerInventory();
                    localFPSGunController.WieldRocketLauncher();
                }
                break;
            }
            case(GunType.AR):
            {
                playerGunSlots[2][0] = gunToGive;
                if(localPlayer)
                {
                    FPSGunController localFPSGunController = localPlayer.GetComponent<FPSGunController>();
                    localFPSGunController.ReadPlayerInventory();
                    localFPSGunController.WieldAR();
                }
                
                break;
            }
            case(GunType.MP5_alt):
            {
                playerGunSlots[2][1] = gunToGive;
                if(localPlayer)
                {
                    FPSGunController localFPSGunController = localPlayer.GetComponent<FPSGunController>();
                    localFPSGunController.ReadPlayerInventory();
                    localFPSGunController.WieldMP5_alt();
                }
                break;
            }
            case(GunType.Revolver_blue):
            {
                playerGunSlots[0][1] = gunToGive;
                if(localPlayer)
                {
                    FPSGunController localFPSGunController = localPlayer.GetComponent<FPSGunController>();
                    localFPSGunController.ReadPlayerInventory();
                 //   localFPSGunController.WieldMP5_alt();
                }
                break;
            }
            default:
            {
                break;
            }
        }
        
       
    }
}
