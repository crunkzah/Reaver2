using UnityEngine;
using UnityEngine.SceneManagement;

public class KickStarter : MonoBehaviour
{
    public GameObject uberManagerPrefab;
    
    public bool wasKickstarted = false;
    
    void Start()
    {
        if(!wasKickstarted && UberManager.instance == null)
        {
            //Instantiate(uberManagerPrefab, Vector3.zero, Quaternion.identity);
            SceneManager.LoadScene((int)Level.UberManager, LoadSceneMode.Additive);
            wasKickstarted = true;
        }
    }
}
