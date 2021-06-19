using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    
    static PlayerManager Instance;
    
    public static PlayerManager Singleton()
    {
        if(Instance == null)
        {
            Instance = FindObjectOfType<PlayerManager>();
        }
        
        return Instance;
    }
    
    GameObject localPlayer;
    PlayerController localController;
    
    public void SetLocalPlayer(GameObject player)
    {
        localPlayer = player;
        localController = player.GetComponent<PlayerController>();
    }
    
    void Update()
    {
        Audio();
        //PostProcessing();
    }
    
    
    
    void Audio()
    {
        if(localPlayer)
        {
            if(!localController.isAlive)
            {
                AudioManager.SetState(AudioState.PlayerDead);
            }
            else
            {
                AudioManager.SetState(AudioState.Normal);
            }
            
        }
        else
        {
            AudioManager.SetState(AudioState.Normal);
            //FollowingCamera.Singleton().minFov = 54;
            //FollowingCamera.Singleton().maxFov = 58;
        }
    }
    
   
    
    void PostProcessing()
    {
        PostProcessingState currentPPstate = PostProcessingController.Singleton().state;
        
        if(localPlayer)
        {
            if(!localController.isAlive)
            {
                if(currentPPstate != PostProcessingState.PlayerDead)
                {
                    PostProcessingController.Singleton().SetState(PostProcessingState.PlayerDead);
                }
            }
            else
            {
                if(currentPPstate != PostProcessingState.Normal)
                {
                    PostProcessingController.Singleton().SetState(PostProcessingState.Normal);
                }
            }
            
        }
        else
        {
            if(currentPPstate != PostProcessingState.Normal)
            {
                PostProcessingController.Singleton().SetState(PostProcessingState.Normal);
            }
        }
    }
}
