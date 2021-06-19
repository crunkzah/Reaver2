using UnityEngine;
using Photon.Pun;


public class LobbyLevelsPanel : MonoBehaviour
{
    public GameObject nonMasterLabel;
    public GameObject[] levelButtons;
    
    bool isMasterView = true;
    
    void DisableLevelsButtons()
    {
        for(int i = 0; i < levelButtons.Length; i++)
        {
            levelButtons[i].SetActive(false);
        }
    }
    
    void EnableLevelsButtons()
    {
        for(int i = 0; i < levelButtons.Length; i++)
        {
            levelButtons[i].SetActive(true);
        }
    }
    
    void OnEnable()
    {
        if(PhotonNetwork.IsMasterClient)
        {
            GoMasterView();
        }
        else
        {
            GoGuestView();
        }
    }
    
    void GoMasterView()
    {
        EnableLevelsButtons();
        nonMasterLabel.SetActive(false);
        isMasterView = true;
    }
    
    void GoGuestView()
    {
        DisableLevelsButtons();
        nonMasterLabel.SetActive(true);
        isMasterView = false;
    }
    
    void Update()
    {
            if(!PhotonNetwork.IsMasterClient)
            {
                if(isMasterView)
                {
                    GoGuestView();
                }
            }
            if(PhotonNetwork.IsMasterClient)
            {
                if(!isMasterView)
                {
                    GoMasterView();
                }
            }
        
    }
}
