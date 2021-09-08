using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;


public class FinishCheckpoint : MonoBehaviour
{
    Vector3 worldPos;
    public float radius = 2;
    
    void Awake()
    {
        worldPos = transform.position;
    }
    
    bool isWorking = true;
    
    void Update()
    {
        if(PhotonNetwork.IsMasterClient && isWorking)
        {
            PlayerController local_player = PhotonManager.GetLocalPlayer();
            
            if(local_player)
            {
                float sqrDistance = Math.SqrDistance(local_player.GetGroundPosition(), worldPos);
                if(sqrDistance < radius * radius)
                {
                    if(Inputs.GetInteractKeyDown())
                    {
                        isWorking = false;
                        ///StartUpScreen.Singleton().FadeIn(2);
                        Invoke(nameof(ProceedToNextLevel), 1);
                    }
                }
            }
        }
    }
    
    public int next_level;
    
    void ProceedToNextLevel()
    {
        Scene scene_to_load = SceneManager.GetSceneByBuildIndex(next_level);
        if(scene_to_load != null)
        {
            UberManager.Load_Level(next_level);            
        }
    }
}
