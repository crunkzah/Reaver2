using UnityEngine;

public class FPSCamView : MonoBehaviour
{
    Camera cam;
    
    void Start()
    {
        cam = GetComponent<Camera>();
        if(TitlesManager.Singleton() != null)
            TitlesManager.SetupCamera(cam);
    }
}
