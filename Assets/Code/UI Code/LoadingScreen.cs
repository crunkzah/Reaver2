using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingScreen : MonoBehaviour
{
    static LoadingScreen instance;
    
    public GameObject background;
    public GameObject tmp_loading;
    
    int levelIndex = -1;
    
    public static void SetOn(int _levelIndex)
    {
        instance.levelIndex = _levelIndex;
        instance.gameObject.SetActive(true);
    }
    
    public static void SetOff()
    {
        instance.gameObject.SetActive(false);
    }
    
    
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SetOff();
    }
    
    void Awake()
    {
        if(instance == null)
        {
            DontDestroyOnLoad(gameObject);
            instance = this;
            SceneManager.sceneLoaded += OnSceneLoaded;
            
            tmp_loading.SetActive(true);
            background.SetActive(true);
            
            SetOff();
        }
        else
        {
            // InGameConsole.LogFancy("<color=red>Destroying!!!!!</color>");
            Destroy(this.gameObject);
        }
    }
}
