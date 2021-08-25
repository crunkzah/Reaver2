using UnityEngine;
using Photon.Pun;

public class DisableAccordingByPlayers : MonoBehaviour
{
    //Если количество игроков в пати не равно playersNeeded, отключаем объект
    [Range(1, 4)]
    public int playersNeeded = 1;
    
    void Start()
    {
        if(PhotonNetwork.CurrentRoom != null)
        {
            int players_in_lobby = PhotonNetwork.CurrentRoom.PlayerCount;
            if(players_in_lobby != playersNeeded)
            {
                InGameConsole.LogFancy("Disabling " + this.gameObject.name);
                this.gameObject.SetActive(false);
            }
        }
    }
}
