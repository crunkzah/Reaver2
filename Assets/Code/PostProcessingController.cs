using UnityEngine.PostProcessing;
using UnityEngine;


public enum PostProcessingState : int
{
    Normal,
    PlayerDead
}


[System.Serializable]
public struct ProfileCustom
{
    public int index;
    
    public Vector2 contrastMinMax;
    public float contrastFlickeringFreq;
    public float contrastBlendTime;
    
    public float saturation;
    public float saturationBlendTime;
}

public class PostProcessingController : MonoBehaviour
{
    public float timer_contrast_normalized = 0;
    public bool timer_contrast_dir = true;
    public PostProcessingState state;
    
    [Header("Custom profiles:")]
    public ProfileCustom normalProfile;
    public ProfileCustom deadProfile;
    ProfileCustom activeProfile = new ProfileCustom();
    
    PostProcessingProfile current_profile;
    
    ColorGradingModel.Settings colorGradingSettings;
    
    PostProcessingBehaviour ppBehaviour;
    
    
    
    static PostProcessingController Instance;
    
    public static PostProcessingController Singleton()
    {
        if(Instance == null)
        {
            Instance = FindObjectOfType<PostProcessingController>();
        }
        
        return Instance;
    }
    
    
    public void SetState(PostProcessingState _state)
    {
        
        InGameConsole.LogFancy("PostProcessing: new state: <color=yellow>" + _state.ToString() + "</color>");
        contrastVel = saturationVel = 0;
        state = _state;
        // switch(_state)
        // {
        //     case(PostProcessingState.Normal):
        //     {
        //         SetProfile(normalProfile);
        //         break;
        //     }
        //     case(PostProcessingState.PlayerDead):
        //     {
        //         SetProfile(deadProfile);
        //         break;
        //     }
        // }
    }
    
    void SetProfile(ProfileCustom _profile)
    {
        activeProfile = _profile;
        //ppBehaviour.profile = _profile;
        //current_profile = _profile;
    }
    
    void Awake()
    {
        activeProfile.index = -1;
        
        
        ppBehaviour = GetComponent<PostProcessingBehaviour>();
        current_profile = ppBehaviour.profile;
        
        colorGradingSettings = current_profile.colorGrading.settings;
        
        
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            if(Instance.GetInstanceID() != this.GetInstanceID())
            {
                InGameConsole.LogFancy("<color=yellow>PostProcessingController: </color> is destroyed because another static instance is present.");
                Destroy(this.gameObject);
            }
        }
    }
    
    void Start()
    {
        SetState(PostProcessingState.Normal);
        current_profile.ambientOcclusion.enabled = true;
        fixed_ao = false;
        timeWhenTurnOffAO = Time.time + 0.33f;
    }
    
    void DebugInput()
    {
        if(Inputs.GetKeyDown(KeyCode.P))
        {
            if(state == PostProcessingState.Normal)
            {
                SetState(PostProcessingState.PlayerDead);
            }
            else
            {
                if(state == PostProcessingState.PlayerDead)
                {
                    SetState(PostProcessingState.Normal);
                }
            }
        }
    }
    
    
    void OnDestroy()
    {
        // fixed_ao = false;
        // current_profile.ambientOcclusion.enabled = true;
    }
    
    public float timeWhenTurnOffAO = float.MaxValue;
    public bool fixed_ao = false;
    
    void FixAmbientOcclusion()
    {
        if(!fixed_ao && Time.time > timeWhenTurnOffAO)
        {
            current_profile.ambientOcclusion.enabled = false;
            fixed_ao = true;        
            InGameConsole.LogFancy("Fixed AO");
        }
    }
    
    void Update()
    {
        //FixAmbientOcclusion();
        //DebugInput();
        
        // float dt = UberManager.DeltaTime();
        
        // switch(state)
        // {
        //     case(PostProcessingState.Normal):
        //     {
        //         if(activeProfile.index != normalProfile.index)
        //         {
        //             activeProfile = normalProfile;
        //         }
        //         // if(animate_contrast)
        //         // {
        //         //     timer_contrast_normalized += dt * contrast_freq;
        
        //         //     if(timer_contrast_normalized > 1f)
        //         //     {
        //         //         timer_contrast_normalized -= 1f;
        //         //         timer_contrast_dir = !timer_contrast_dir;
        //         //     }
                    
                    
        //         //     if(timer_contrast_dir)
        //         //     {
        //         //         current_contrast = Mathf.SmoothStep(contrast_min_max.x, contrast_min_max.y, timer_contrast_normalized);
        //         //     }
        //         //     else
        //         //     {
        //         //         current_contrast = Mathf.SmoothStep(contrast_min_max.y, contrast_min_max.x, timer_contrast_normalized);
        //         //     }
                    
        //         //     SetContrast(current_contrast);
                    
        //         //     SetColorGradingSettings();
        //         // }
                
                
        //         break;
        //     }
        //     case(PostProcessingState.PlayerDead):
        //     {
        //         if(activeProfile.index != deadProfile.index)
        //         {
        //             activeProfile = deadProfile;
        //         }
        //         break;
        //     }
        // }
        
        // AnimateSaturation(dt);
        // //AnimateContrast(dt);
        
        
        // SetColorGradingSettings();
    }
    
    float saturationVel;
    float contrastVel;
    
    public static void AddSaturation(float dSat)
    {
        //Singleton().colorGradingSettings.basic.saturation += dSat;
    }
    
    
    void AnimateSaturation(float dt)
    {
        float currentSaturation = colorGradingSettings.basic.saturation;
        //float updatedSaturation = Mathf.MoveTowards(currentSaturation, activeProfile.saturation, dt * saturationChangeSpeed);
        float updatedSaturation = Mathf.SmoothDamp(currentSaturation, activeProfile.saturation, ref saturationVel, activeProfile.saturationBlendTime);
        
        SetSaturation(updatedSaturation);
    }
    
    void SetSaturation(float saturation)
    {
        colorGradingSettings.basic.saturation = saturation;
    }
    
    void AnimateContrast(float dt)
    {
        timer_contrast_normalized += dt * activeProfile.contrastFlickeringFreq;

        if(timer_contrast_normalized > 1f)
        {
            timer_contrast_normalized -= 1f;
            timer_contrast_dir = !timer_contrast_dir;
        }
        
        float updatedContrast = 1;
        
        float targetContrast = timer_contrast_dir ? activeProfile.contrastMinMax.y : activeProfile.contrastMinMax.x;
        float startContrast = timer_contrast_dir ? activeProfile.contrastMinMax.x : activeProfile.contrastMinMax.y;
        
        float currentContrast = colorGradingSettings.basic.contrast;
        //updatedContrast = Mathf.SmoothDamp(currentContrast, targetContrast, ref contrastVel, activeProfile.contrastBlendTime);
        
        //updatedContrast = Mathf.MoveTowards(currentContrast, targetContrast, dt * activeProfile.contrastFlickeringSpeed);
        updatedContrast = Mathf.Lerp(startContrast, targetContrast, timer_contrast_normalized);
        
        
        SetContrast(updatedContrast);
        
        SetColorGradingSettings();
    }
    void SetContrast(float contrast)
    {
        colorGradingSettings.basic.contrast = contrast;
    }
    
    void SetColorGradingSettings()
    {
        current_profile.colorGrading.settings = colorGradingSettings;
    }
    
    
    
        
}
