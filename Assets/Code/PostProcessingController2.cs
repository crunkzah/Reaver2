using UnityEngine;
using UnityEngine.PostProcessing;
using UnityEngine.SceneManagement;

[System.Serializable]
public struct PostProcessingControls
{
    public float postExposure;
    public float temperature; 
    public float saturation;
    public float contrast;
    public float chromaticAberration;
    
}

public enum PostProcessingState
{
    Normal,
    Berserk,
    PlayerDead,
}

public class PostProcessingController2 : MonoBehaviour
{
    public PostProcessingProfile outside_camera_profile;
    
    
    public PostProcessingProfile profile_active;
    ColorGradingModel.Settings colorGradingSettings_active;
    //ColorGradingModel.TonemappingSettings colorGradingTonemappingSettings_active;
    ChromaticAberrationModel.Settings chromaticAberrationSettings_active;
    
    static PostProcessingController2 _instance;
    public static PostProcessingController2 Singleton()
    {
        if(_instance == null)
        {
            _instance = FindObjectOfType<PostProcessingController2>();
        }
        
        return _instance;
    }
    
    
    public bool changeColorGrading = true;
    
    public float blendTimeNormal = 0.33F;
    public float blendTimeDead = 0.33F;
    public float blendTimeBerserk = 0.2F;
    
    public PostProcessingControls ppc_normal;
    public PostProcessingControls ppc_berserk;
    public PostProcessingControls ppc_playerDead;
    public PostProcessingControls ppc_active;
    
    public PostProcessingState state;
    
    
    //Dumps:
    float vel1, vel2, vel3, vel4;
    
    void ApplyColorGradingSettings()
    {
        colorGradingSettings_active.basic.temperature = ppc_active.temperature;
        colorGradingSettings_active.basic.contrast = ppc_active.contrast;
        colorGradingSettings_active.basic.saturation = ppc_active.saturation;
        colorGradingSettings_active.basic.postExposure = ppc_active.postExposure;
        
        profile_active.colorGrading.settings = colorGradingSettings_active;
        
        //InGameConsole.LogFancy("ApplyColorGradingSettings()");
        //colorGradingSettings_active.tonemapping = colorGradingTonemappingSettings_active;
    }
    
    void ApplyChromaticAberration()
    {
        chromaticAberrationSettings_active.intensity = ppc_active.chromaticAberration;
        if(Mathf.Approximately(chromaticAberrationSettings_active.intensity, 0f))
        {
            profile_active.chromaticAberration.enabled = false;
        }
        else
        {
            if(!profile_active.chromaticAberration.enabled)
            {
                profile_active.chromaticAberration.enabled = true;
            }
        }
    }
    
    PostProcessingControls ppc_target;
    
    public static void SetState(PostProcessingState _state)
    {
        if(Singleton() == null)
            return;
        
        Singleton()._SetState(_state);
    }
    
    void _SetState(PostProcessingState _state)
    {
        state = _state;
        InGameConsole.LogFancy("PostProcessingController2: state is <color=yellow>" + _state + "</color>");
    }
    
    void _SetState_Immediately(PostProcessingState _state)
    {
        state = _state;
        switch(state)
        {
            case(PostProcessingState.Normal):
            {
                ppc_active = ppc_normal;
                
                break;
            }
            case(PostProcessingState.Berserk):
            {
                ppc_active = ppc_normal;
                
                break;
            }
            case(PostProcessingState.PlayerDead):
            {
                ppc_active = ppc_normal;
                
                break;
            }
        }
        
        ApplyColorGradingSettings();
        ApplyChromaticAberration();
    }
    
    void DebugInput()
    {
        if(Input.GetKeyDown(KeyCode.Z))
        {
            switch(state)
            {
                case(PostProcessingState.Normal):
                {
                    SetState(PostProcessingState.Berserk);
                    
                    break;
                }
                case(PostProcessingState.Berserk):
                {
                    SetState(PostProcessingState.PlayerDead);
                    
                    break;
                }
                case(PostProcessingState.PlayerDead):
                {
                    SetState(PostProcessingState.Normal);
                    
                    break;
                }
            }
        }
    }
    
    void UpdatePostProcessing(float dt)
    {
        bool needChange = false;
        float _blendTime;
        
        switch(state)
        {
            case(PostProcessingState.Normal):
            {
                _blendTime = blendTimeNormal;
                ppc_target = ppc_normal;
                
                break;
            }
            case(PostProcessingState.Berserk):
            {
                _blendTime = blendTimeBerserk;
               ppc_target = ppc_berserk;
                
                break;
            }
            case(PostProcessingState.PlayerDead):
            {
                _blendTime = blendTimeDead;
                ppc_target = ppc_playerDead;
                
                break;
            }
        }
        
        if(!Mathf.Approximately(ppc_active.temperature, ppc_target.temperature))
        {
            ppc_active.temperature = Mathf.SmoothDamp(ppc_active.temperature, ppc_target.temperature, ref vel1, blendTimeNormal);
            needChange = true;
        }
        
        if(!Mathf.Approximately(ppc_active.contrast, ppc_target.contrast))
        {
            ppc_active.contrast = Mathf.SmoothDamp(ppc_active.contrast, ppc_target.contrast, ref vel2, blendTimeNormal);
            needChange = true;
        }
        
        if(!Mathf.Approximately(ppc_active.postExposure, ppc_target.postExposure))
        {
            ppc_active.postExposure = Mathf.SmoothDamp(ppc_active.postExposure, ppc_target.postExposure, ref vel3, blendTimeNormal);
            needChange = true;
        }
        
        if(!Mathf.Approximately(ppc_active.saturation, ppc_target.saturation))
        {
            ppc_active.saturation = Mathf.SmoothDamp(ppc_active.saturation, ppc_target.saturation, ref vel4, blendTimeNormal);
            needChange = true;
        }
        
        if(true)
        {
            ApplyColorGradingSettings();
        }
    }
    
    
    
    // Start is called before the first frame update
    void Start()
    {
        colorGradingSettings_active         = profile_active.colorGrading.settings;
        
        _SetState_Immediately(PostProcessingState.Normal);
        
        //chromaticAberrationSettings_active  = profile_active.chromaticAberration.settings;
        
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
        AO_settings.highPrecision = false;
        outside_camera_profile.ambientOcclusion.settings = AO_settings;
    }
    
    void FixAO()
    {
        //InGameConsole.LogOrange("FIXING FUCKING AMBIENT OCCLUSION");
        fixed_ao = true;
        
        AmbientOcclusionModel.Settings AO_settings = outside_camera_profile.ambientOcclusion.settings;
        AO_settings.highPrecision = true;
        outside_camera_profile.ambientOcclusion.settings = AO_settings;
    }
    
    
    
    
    // Update is called once per frame
    void Update()
    {
        float dt = UberManager.DeltaTime();
        
        if(!fixed_ao)
        {
            fixAOtimer += dt;
            
            if(fixAOtimer > 1)
            {
                FixAO();
            }
        }
        
        UpdatePostProcessing(dt);
        
        DebugInput();
    }
}
