using UnityEngine;
using UnityEngine.PostProcessing;
using UnityEngine.SceneManagement;

public class PostProcessingController2 : MonoBehaviour
{
    public PostProcessingProfile outside_camera_profile;
    
    
    
    // Start is called before the first frame update
    void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        if(firstBoot)
        {
            UnFixAo();
        }
    }
    
    
    bool firstBoot = true;
    bool fixed_ao = false;
    float fixAOtimer = 0;
    
    
    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UnFixAo();
    }
    
    void UnFixAo()
    {
        //InGameConsole.LogFancy("UNFIXING FUCKING AMBIENT OCCLUSION");
        fixed_ao = false;
        fixAOtimer = 0;
        
        AmbientOcclusionModel.Settings AO_settings = outside_camera_profile.ambientOcclusion.settings;
        AO_settings.highPrecision = true;
        outside_camera_profile.ambientOcclusion.settings = AO_settings;
    }
    
    void FixAO()
    {
        //InGameConsole.LogOrange("FIXING FUCKING AMBIENT OCCLUSION");
        fixed_ao = true;
        
        AmbientOcclusionModel.Settings AO_settings = outside_camera_profile.ambientOcclusion.settings;
        AO_settings.highPrecision = false;
        outside_camera_profile.ambientOcclusion.settings = AO_settings;
    }

    // Update is called once per frame
    void Update()
    {
        if(!fixed_ao)
        {
            fixAOtimer += UberManager.DeltaTime();
            
            if(fixAOtimer > 1)
            {
                FixAO();
            }
        }
    }
}
