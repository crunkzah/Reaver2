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
    
    
    void Update()
    {
        float dt = UberManager.DeltaTime();
        
        
        float offsetX = Random.Range(-1f, 1f) * 0.025F;
        float offsetY = Random.Range(-1f, 1f) * 0.025F;
        
        offsetX *= shaking_mult;
        offsetY *= shaking_mult;
        
        thisTransform.localPosition = new Vector3(offsetX, offsetY, 0);
    }
    
    public void StartShaking()
    {
        isShaking = true;
    }
    
    public void StopShaking()
    {
        isShaking = false;    
    }
    
    
    void Awake()
    {
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
    }
}
