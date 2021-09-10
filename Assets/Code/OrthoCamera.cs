using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class OrthoCamera : MonoBehaviour
{
    static OrthoCamera _instance;
    public ParticleSystem skulls_ps;
    
    public static OrthoCamera Singleton()
    {
        if(_instance == null)
        {
            _instance = FindObjectOfType<OrthoCamera>();
            
            if(_instance.cam == null)
            {
                _instance.cam = _instance.GetComponent<Camera>();
            }
        }
        
        return _instance;
    }
    
    Camera cam;
    void Start()
    {
        cam = GetComponent<Camera>();
        HideDeathScreen();
        
        //HideBossHPBarFancyThings();
        //Hide();
        //Disable
    }
    
    
    
    // bool isDeathScreenOn = false;
    // bool isBossHPBarFancyThingsOn = false;
    
    static bool show3DGUI = true;
    
    public Transform HOLDER_DYNAMIC_3DGUI;
    public Transform HOLDER_3DGUI;
    public Transform Heart;
    Material heart_mat;
    static Color heart_start_color     = new Color(1f, 0, 3f/255f);
    //static Color heart_start_color_dark     = new Color(0.8f, 0, 0f);
    static Color heart_healed_color    = new Color(3f/255f, 1, 0);
    static Color heart_hurt_color      = new Color(0.25f, 0, 3f/255f);
    Color heartColor;
    int colorID;
    
    const float heartScaleSpeed = 4;
    const float heartColorSpeed = 2f;
    
    
    public RotationAnimator heart_outer_gear;
    public RotationAnimator heart_inner_gear;
    public TextMeshPro hitpoints_label;
    
    public ParticleSystem currentHitPoints_ps;
    ParticleSystem.MainModule currentHitPoints_ps_main;
    ParticleSystem.ShapeModule currentHitPoints_ps_shape;
    ParticleSystem.EmissionModule currentHitPoints_ps_emission;
    const float hitpoints_full_emission = 330;
    
    Vector3 hitPoints_full_pos = new Vector3(1f, 0.06f, 0);
    const float hitPointsFullRadius = 0.69f;
    
    
    public ParticleSystem maxHitPoints_ps;
    ParticleSystem.ShapeModule maxHitPoints_ps_shape;
    ParticleSystem.EmissionModule maxHitPoints_ps_emission;
    Vector3 maxHitPoints_full_pos = new Vector3(1f, 0.06f, 0);
    const float maxHitPointsFullRadius = 0.69f;
    
    
    public ParticleSystem stamina_ps;
    ParticleSystem.ShapeModule stamina_ps_shape;
    ParticleSystem.EmissionModule stamina_ps_emission;
    Vector3 stamina_full_pos = new Vector3(1f, 0.06f, 0);
    const float staminaFullRadius = 0.69f;
    const float stamina_full_emission = 250;
    
    public ParticleSystem onHeal_ps;
    public ParticleSystem onTakeDamage_ps;
    
    
    public static void OnHeal()
    {
        if(show3DGUI)
        {
            Singleton().heartColor = heart_healed_color;
            Singleton().Heart.localScale = Vector3.one * 1.2f;
            Singleton().onHeal_ps.Play();
        }
    }
    
    public static void OnTakeDamage()
    {
        //InGameConsole.LogFancy("OnHeal()");
        if(show3DGUI)
        {
            Singleton().heartColor = heart_hurt_color;
            Singleton().Heart.localScale = Vector3.one * 0.8f;
            Singleton().onTakeDamage_ps.Play();
        }
    }
    
    
    void Awake()
    {
        if(currentHitPoints_ps)
        {
            currentHitPoints_ps_main = currentHitPoints_ps.main;
            currentHitPoints_ps_shape = currentHitPoints_ps.shape;
            currentHitPoints_ps_emission = currentHitPoints_ps.emission;
        }
        if(maxHitPoints_ps)
        {
            maxHitPoints_ps_shape = maxHitPoints_ps.shape;
            maxHitPoints_ps_emission = maxHitPoints_ps.emission;
        }
        if(stamina_ps)
        {
            stamina_ps_shape = stamina_ps.shape;
            stamina_ps_emission = stamina_ps.emission;
        }
        
        if(Heart)
        {
            heart_mat = Heart.GetComponent<MeshRenderer>().sharedMaterials[1];
            
            
            //heart_outer_gear.GetComponent<MeshRenderer>().materials[1] = heart_mat;
            //heart_inner_gear.GetComponent<MeshRenderer>().materials[1] = heart_mat;
            
            heartColor = heart_start_color;
            colorID = Shader.PropertyToID("_RampColorTint");
            heart_mat.SetColor(colorID, heartColor);
        }
    }
    
    void Update()
    {
        if(show3DGUI)
        {
            PlayerController local_pc = PhotonManager.GetLocalPlayer();
            if(local_pc)    
            {
                HOLDER_DYNAMIC_3DGUI.gameObject.SetActive(true);
            }
            else
            {
                HOLDER_DYNAMIC_3DGUI.gameObject.SetActive(false);    
            }
        }
        else
        {
            HOLDER_DYNAMIC_3DGUI.gameObject.SetActive(false);
        }
    }
    
    int playerHitPointsOld = 50;
    float playerStaminaOld = 50;
    
    
    public static void Show3DGUI()
    {
        show3DGUI = true;
    } 
    
    public static void Hide3DGUI()
    {
        show3DGUI = false;
        
    }
    
    public static void Update3DGUI(float dt, int currentHP, float maxHPPenalty, int maxHP, float stamina)
    {
        if(Singleton())
        {
            Singleton()._Update3DGUI(dt, currentHP, maxHPPenalty, maxHP, stamina);
        }
    }
    
    static bool dynamic3DGUI = true;
    
    float maxVelMagnitude = 21F;
   // [Range(0.5f, 1f)]
    const float scale_smallest = 0.975f;
   // [Range(1f, 1.2f)]
    const float scale_biggest = 1.025f;
    const float scale_smoothTime = 0.050f;
    float dump;
    
    public float lerpSpeed = 8;
    
    void _Update3DGUI(float dt, int currentHP, float maxHPPenalty, int maxHP, float stamina)
    {
        //if(playerHitPointsOld != currentHP)
        //{
            playerHitPointsOld = currentHP;
            hitpoints_label.SetText(currentHP.ToString());
            float hpPercentage = (float)currentHP / (float)maxHP;
            
            float oldRadius = currentHitPoints_ps_shape.radius;
            float _radius = Mathf.Lerp(0, hitPointsFullRadius, hpPercentage);
            
            if(!Mathf.Approximately(oldRadius, _radius))
            {
                currentHitPoints_ps_shape.radius = Mathf.MoveTowards(currentHitPoints_ps_shape.radius, _radius, dt * lerpSpeed);
                currentHitPoints_ps.Stop();
                currentHitPoints_ps.Play();
            }
            
            
            float _X = Mathf.Lerp(0,  hitPoints_full_pos.x, hpPercentage);
            float X = currentHitPoints_ps.transform.localPosition.x;
            X = Mathf.MoveTowards(X, _X, dt * lerpSpeed);
            
            
            currentHitPoints_ps_emission.rateOverTime = Mathf.Lerp(0, hitpoints_full_emission, hpPercentage);
            //InGameConsole.LogFancy(string.Format("Radius: {0}, X: {1}", radius, X));
            //X = hitPoints_full_pos.x;
            currentHitPoints_ps.transform.localPosition = new Vector3(X, currentHitPoints_ps.transform.localPosition.y, currentHitPoints_ps.transform.localPosition.z);
        //}
        
        
       // if(playerStaminaOld != stamina)
        //{
            playerStaminaOld = stamina;
            
            //hitpoints_label.SetText(currentHP.ToString());
            float staminaPercentage = (float)stamina / 100f;
            oldRadius = stamina_ps_shape.radius;
            _radius = Mathf.Lerp(0, staminaFullRadius, staminaPercentage);
            
            if(!Mathf.Approximately(oldRadius, _radius))
            {
                stamina_ps_shape.radius = Mathf.MoveTowards(stamina_ps_shape.radius, _radius, dt * lerpSpeed);
                stamina_ps.Stop();
                stamina_ps.Play();
            }
            
            //stamina_ps_shape.radius = radius;
            stamina_ps_shape.radius = Mathf.MoveTowards(stamina_ps_shape.radius, _radius, dt * lerpSpeed);;
             
            _X = Mathf.Lerp(0,  stamina_full_pos.x, staminaPercentage);
            X = stamina_ps.transform.localPosition.x;
            X = Mathf.MoveTowards(X, _X, dt * lerpSpeed);
            
            
            // float X = Mathf.Lerp(0,  stamina_full_pos.x, staminaPercentage);
            stamina_ps_emission.rateOverTime = Mathf.Lerp(0, stamina_full_emission, staminaPercentage);
            //InGameConsole.LogFancy(string.Format("Radius: {0}, X: {1}", radius, X));
            stamina_ps.transform.localPosition = new Vector3(X, stamina_ps.transform.localPosition.y, stamina_ps.transform.localPosition.z);
       // }
        //float t = maxHP - 
        
        if(dynamic3DGUI)
        {
            PlayerController local_pc = PhotonManager.GetLocalPlayer();
            if(local_pc)
            {
                Transform fpsCamTransform = local_pc.GetFPSCameraTransform();
                if(fpsCamTransform)
                {
                    Vector3 fpsVel = local_pc.GetFPSVelocity();
                    Vector3 fpsVelDir = fpsVel.normalized;
                    float fpsMagnitude = Vector3.Magnitude(fpsVel);
                    Vector3 fpsDir = fpsCamTransform.forward;
                    float dot = Vector3.Dot(Math.GetXZ(fpsVelDir), Math.GetXZ(fpsDir));
                    // Vector3 r = -fpsVelDir * fpsVel;
                    // rect_tr.anchoredPosition = new Vector2(r.x, r.z);
                    float mag_t = Mathf.InverseLerp(0, maxVelMagnitude, fpsMagnitude);
                    float scale_target = Mathf.Lerp(scale_smallest, scale_biggest, 0.5f + 0.5f*dot);
                    
                    float scale_smoothed = Mathf.SmoothDamp(HOLDER_DYNAMIC_3DGUI.localScale.x, scale_target, ref dump, scale_smoothTime);
                    HOLDER_DYNAMIC_3DGUI.localScale = new Vector3(scale_smoothed, scale_smoothed, 1);
                }
            }
        }
        
        if(Heart)
        {
            Heart.localScale = Vector3.MoveTowards(Heart.localScale, new Vector3(1, 1, 1), dt * heartScaleSpeed);
            if(heartColor != heart_start_color)
            {
                currentHitPoints_ps_main.startColor = heartColor;
                heartColor = Vector4.MoveTowards(heartColor, heart_start_color, heartColorSpeed * dt);
                // float blackness = Random.Range(0.95f, 1f);
                // heartColor.r *= blackness;
                // heartColor.g *= blackness;
                // heartColor.b *= blackness;
                
                //InGameConsole.LogFancy("SetColor() " + heartColor);
                heart_mat.SetColor(colorID, heartColor);
            }
        }
    }
    
    
    void LateUpdate()
    {
        return;
        float dt = UberManager.DeltaTime();
        
        
        if(!skulls_ps.isPlaying)
        {
            cam.enabled = false;
        }
        else
        {
            cam.enabled = true;
        }
        
        // if(!isDeathScreenOn && !isBossHPBarFancyThingsOn)
        // {
        //     if(isCameraEnabled)
        //         Hide();
        // }
        
        // if(isDeathScreenOn)
        // {
        //     if(!isCameraEnabled)
        //     {
        //         Show
        //     }
        // }
        
        // if(isBossHPBarFancyThingsOn)
        // {
            
        // }
    }
    
    public static void HideDeathScreen()
    {
        Singleton().skulls_ps.Stop();
    }
    
    public static void ShowDeathScreen()
    {
        Singleton().skulls_ps.Play();
    }
    
    public static void Show()
    {
        Singleton().cam.enabled = true;
        
        //Singleton().skulls_ps.Play();
    }
    
    public static void Hide()
    {
        return;
        Singleton().cam.enabled = false;
        //Singleton().skulls_ps.Stop();
    }
}
