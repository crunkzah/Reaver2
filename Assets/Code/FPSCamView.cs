using UnityEngine;
using UnityEngine.PostProcessing;

public class FPSCamView : MonoBehaviour
{
    Camera cam;
    
    //public float normalFov = 110;
    
    void Start()
    {
        cam = GetComponent<Camera>();
        if(TitlesManager.Singleton() != null)
            TitlesManager.SetupCamera(cam);
        PostProcessingBehaviour postProcessingBehaviour = GetComponent<PostProcessingBehaviour>();
        if(postProcessingBehaviour)
        {
            if(PostProcessingController2.Singleton())
            {
                if(postProcessingBehaviour.profile != PostProcessingController2.Singleton().profile_active)
                {
                    InGameConsole.LogOrange("Swapping profiles!");
                    postProcessingBehaviour.profile = PostProcessingController2.Singleton().profile_active;
                }
            }
        }
    }
}
