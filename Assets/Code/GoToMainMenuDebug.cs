using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToMainMenuDebug : MonoBehaviour
{
    static bool firstStart = true;
    
    void Start()
    {
        if(firstStart)
        {
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            firstStart = false;
            if(currentSceneIndex != 1)
            {
                SceneManager.LoadScene((int)Level.Lobby, LoadSceneMode.Single);
            }
        }
    }
}
