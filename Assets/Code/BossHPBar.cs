using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossHPBar : MonoBehaviour
{
    
    static BossHPBar _instance;
    public static BossHPBar Singleton()
    {
        if(_instance == null)
        {
            _instance = FindObjectOfType<BossHPBar>();
        }
        
        return _instance;
    }
    
    public GameObject target_object;
    IBoss boss;
    
    Canvas canvas;
    RectTransform canvas_rect;
    
    void Awake()
    {
        canvas = GetComponent<Canvas>();
        canvas_rect = canvas.GetComponent<RectTransform>();
    }
    
    public TextMeshProUGUI bossLabel;
    public Image bossHPBar;
    public Image bossHPBar_diff;
    
    
    public static void SetBoss(GameObject boss_object, string bossName)
    {
        Singleton()._SetBoss(boss_object, bossName);
    }
    
    void _SetBoss(GameObject boss_object, string bossName)
    {
        bossLabel.SetText(bossName);
        SetTargetObject(boss_object);
        bossHPBar.transform.localScale = new Vector3(0, 1, 1);
        isBossKilled = false;
        
        if(canvas)
        {
            canvas.enabled = true;
        }
    }
    
    void HideHPBar()
    {
        //InGameConsole.LogFancy("HideHPBar()");
        canvas.enabled = false;
        boss = null;
        target_object = null;
        // if(OrthoCamera.Singleton())
        //     OrthoCamera.HideBossHPBarFancyThings();
    }
    
    public void SetTargetObject(GameObject _target)
    {
        target_object = _target;
        if(target_object)
            boss = target_object.GetComponent<IBoss>();
    }
    
    const float speed = 0.055f;
    const float smoothTime = 0.33f;
    
    bool isBossKilled = false;
    
    void OnBossKilled()
    {
        HideHPBar();
    }
    
    //public float hpPercentageOld;
    public float hpPercentage;
    
    float vel;
    float trauma = 0;
    const float shakeMult = 1.5f;
    
    const float horizontalMult = 1.1f;
    const float verticalMult = 1.3f;
    
    const float traumaSmoothTime = 0.15f;
    
    float v;
    
    const float maxTrauma = 5;
    
    void Shaking(float dt)
    {
        if(trauma > maxTrauma)
        {
            trauma = maxTrauma;
        }
        
        float offsetX = trauma * shakeMult * Random.Range(-1f, 1f) * horizontalMult;
        float offsetY = trauma * shakeMult * Random.Range(-1f, 1f) * verticalMult;
        
        //trauma = Mathf.SmoothDamp(trauma, 0, ref v, traumaSmoothTime);
        trauma = Mathf.MoveTowards(trauma, 0, 3 * dt);
        //InGameConsole.LogOrange(trauma.ToString("f"));
        
        canvas_rect.anchoredPosition = new Vector3(offsetX, offsetY, 0);// + ;
    }
    
    void Update()
    {
        if(target_object == null)
        {
            boss = null;
        }
        if(boss != null)
        {
            float dt = UberManager.DeltaTime();
            float _hpPercentageNow = boss.GetBossHitPointsPercents();
            if(_hpPercentageNow < hpPercentage)
            {
                trauma += 1.25f;
                if(trauma > 3f)
                    trauma = 3;
            }
            
            hpPercentage = _hpPercentageNow;
            
            if(hpPercentage <= 0f)
            {
                if(!isBossKilled)
                {
                    isBossKilled = true;
                    Invoke(nameof(OnBossKilled), 1);
                }
            }
            
            Shaking(dt);
            
            float scale_x = hpPercentage / 100f;
            float current_scale_x = bossHPBar_diff.transform.localScale.x;
            current_scale_x = Mathf.SmoothDamp(current_scale_x, scale_x, ref vel, smoothTime);
            
            bossHPBar_diff.transform.localScale = new Vector3(current_scale_x, 1, 1);      
            bossHPBar.transform.localScale = new Vector3(scale_x, 1, 1);      
        }
        else
        {
            if(canvas.enabled)
            {
                HideHPBar();    
            }
        }
    }
}
