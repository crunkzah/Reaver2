using UnityEngine;

public class Revolver : MonoBehaviour
{
    public Animator anim;
    public Transform thisTransform;
    
    public Transform pump_hud;
    GameObject[] pump_bullet_hud = new GameObject[PUMPED_BULLETS_MAX];
    
    public int pumped_bullets_charged = 0;
    public int pumped_bullets_left = 0;
    public const int PUMPED_BULLETS_MAX = 5;
    
    
    public ParticleSystem fps_shot_ps;
    
    public MeshRenderer bullets_emissive;
    public Material bullets_normal_mat;
    public Material bullets_cooked_mat;
    
    int drumSpeedHash;
    
    void Awake()
    {
        audio_src = GetComponent<AudioSource>();
        drumSpeedHash = Animator.StringToHash("drumRollSpeed");
        thisTransform = transform;
        if(anim == null)
        {
            anim  = GetComponent<Animator>();
        }
        
        int len = pump_hud.childCount;
        
        for(int i = 0; i < len; i++)
        {
            pump_bullet_hud[i] = pump_hud.GetChild(i).gameObject;
        }
        
        Revolver_stronger_cook_ps_main      = Revolver_stronger_cook_ps.main;
        Revolver_stronger_cook_ps_emission  = Revolver_stronger_cook_ps.emission;
    }
    
    ParticleSystem.MainModule Revolver_stronger_cook_ps_main;
    ParticleSystem.EmissionModule Revolver_stronger_cook_ps_emission;
    
    
    public void PumpBullet()
    {
        if(pumped_bullets_charged < PUMPED_BULLETS_MAX || pumped_bullets_left < pumped_bullets_charged)
        {
            pumped_bullets_left++;
            if(pumped_bullets_charged < PUMPED_BULLETS_MAX)
                pumped_bullets_charged++;
        }
        RedrawPumpHud();
    }
    
    public void OnShotFPS()
    {
        fps_shot_ps.Play();
        
        if(pumped_bullets_left > 0)
        {
            pumped_bullets_left--;
            if(pumped_bullets_left == 0)
            {
                pumped_bullets_charged = 0;
            }
        }
        RedrawPumpHud();
    }
    
    void RedrawPumpHud()
    {
        for(int i = 0; i < PUMPED_BULLETS_MAX; i++)
        {
            pump_bullet_hud[i].SetActive(false);
        }
        
        for(int i = 0; i < pumped_bullets_left; i++)
        {
            pump_bullet_hud[i].SetActive(true);
        }
    }
    
    void OnEnable()
    {
        RedrawPumpHud();
    }
    
    bool isShaking = false;
    public float shaking_mult = 0;
    public float shaking_mult_smoothed = 0;
    float dump;
    
    public Transform rightArm_fps;
    public ParticleSystem Revolver_stronger_cook_ps;
    public Transform revolver_holder_fps;
    Vector3 revolver_holder_chargingPos = new Vector3(-0.19f, -0.12f, 0.4f);
    Vector3 revolver_holder_normalPos = new Vector3(0.04f, 0.06f, 0.505f);
    public WeaponSway revolver_sway;
    
    
    static readonly Vector3 vOne = new Vector3(1f, 1f, 1f);
    
    Color ult_ready_color   = Color.red;
    Color ult_cooking_color = Color.white;
    float timer = 0;
    const float shake_rate = 0.01f;
    
    void Update()
    {
       
        float dt = UberManager.DeltaTime();
        
        
        float offsetX = Random.Range(-1f, 1f) * 0.01F;
        float offsetY = Random.Range(-1f, 1f) * 0.01F;
        
        offsetX *= shaking_mult;
        offsetY *= shaking_mult;
        
        float cook_ps_scale = shaking_mult * 0.0075F;
        Revolver_stronger_cook_ps.transform.localScale = new Vector3(cook_ps_scale, cook_ps_scale, cook_ps_scale);
        
        if(shaking_mult < shaking_mult_smoothed)
        {
            shaking_mult_smoothed = Mathf.MoveTowards(shaking_mult_smoothed, shaking_mult, 1.5f * dt);
        }
        else
        {
            shaking_mult_smoothed = shaking_mult;
        }
        
        float t_lerpVector = shaking_mult_smoothed * 2.5f;
        
        revolver_holder_fps.localPosition = Math.SmoothStepVector(revolver_holder_normalPos, revolver_holder_chargingPos, t_lerpVector);
        revolver_sway.offsetRot.x = Mathf.SmoothStep(0f, 18f, t_lerpVector);
        //revolver_holder_fps.localPosition = Math.Lerp(revolver_holder_normalPos, revolver_holder_chargingPos, shaking_mult_smoothed);
        
        if(shaking_mult > 0f)
        {
            Revolver_stronger_cook_ps_emission.rateOverTime = 10;
        }
        else
        {
            Revolver_stronger_cook_ps_emission.rateOverTime = 0;
        }
        
        if(shaking_mult > 0.9f)
        {
            Revolver_stronger_cook_ps_main.startColor = ult_ready_color;
            if(bullets_emissive.sharedMaterial == bullets_normal_mat)
            {
                OnCharged();
            }
            bullets_emissive.sharedMaterial = bullets_cooked_mat;
        }
        else
        {
            Revolver_stronger_cook_ps_main.startColor = ult_cooking_color;
            bullets_emissive.sharedMaterial = bullets_normal_mat;
        }
        anim.SetFloat(drumSpeedHash, shaking_mult_smoothed * 3f);
        anim.SetLayerWeight(1, shaking_mult_smoothed);
        
        Vector3 offsetWithShaking = new Vector3(offsetX + 0.01f, offsetY + 0.122f, -0.215f);
        timer += dt;
        if(timer > shake_rate)
        {
            thisTransform.localPosition = offsetWithShaking;
            rightArm_fps.localPosition = new Vector3(offsetX, offsetY + 0.136f, -0.223f);
        }
        //InGameConsole.LogFancy("OffSetWithShaking: <color=yellow> " + offsetWithShaking + "</color>");
    }
    
    AudioSource audio_src;
    
    public void OnCharged()
    {
        audio_src.Play();
    }
    
    public void StartShaking()
    {
        isShaking = true;
        //Revolver_stronger_cook_ps.Play();
    }
    
    public void StopShaking()
    {
        isShaking = false;
        //Revolver_stronger_cook_ps.Stop();
    }
    
    
    
}
