using UnityEngine;
using Photon.Pun;

public class LevelChanger : MonoBehaviour, Interactable
{
    
    public Level levelToLoad = Level.Level_1;
    
    public void Interact()
    {
        // if(levelToLoad == -1)
        //     return;
            
        UberManager.Singleton().LoadLevel((int)levelToLoad);
    }
}
