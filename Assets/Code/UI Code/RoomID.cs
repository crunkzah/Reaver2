using UnityEngine;
using TMPro;
using Photon.Pun;

public class RoomID : MonoBehaviour
{
    public TMP_InputField roomId_inputField_readonly;
    
    void OnEnable()
    {
        if(PhotonNetwork.InRoom)
        {
            string roomId = PhotonNetwork.CurrentRoom.Name;
            roomId_inputField_readonly.SetTextWithoutNotify(roomId);
        }
    }
}
