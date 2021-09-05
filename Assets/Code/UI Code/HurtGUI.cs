using UnityEngine;
using UnityEngine.UI;

public class HurtGUI : MonoBehaviour
{
    Canvas canvas;
    
    [Header("Hurt:")]
    public Image hurt_img;
    const float hurtAlphaStart = 1.8f;
    public float hurtAlphaAcceleration = 8f;
    float hurtAlphaCurrentSpeed;
    [Header("HealP:")]
    public Image heal_img;
    const float healAlphaStart = 2.0f;
    public float healAlphaAcceleration = 8f;
    float healAlphaCurrentSpeed;
    
    void Awake()
    {
        canvas = GetComponent<Canvas>();
    }
    
    void Start()
    {
        canvas.enabled = false;
        
        hurt_img.enabled = false;
        heal_img.enabled = false;
        
        Singleton();
    }
    
    static HurtGUI instance;
    public static HurtGUI Singleton()
    {
        if(instance == null)
        {
            instance = FindObjectOfType<HurtGUI>();
        }
        
        return instance;
    }
    
    public static void ShowHeal()
    {
        Singleton()._ShowHeal();
    }
    
    void _ShowHeal()
    {
        canvas.enabled = true;
        heal_img.enabled = true;
        
        Color col = heal_img.color;
        col.a = 0;
        heal_img.color = col;
        
        healAlphaCurrentSpeed = healAlphaStart;
    }
    
    public static void ShowHurt()
    {
        Singleton()._ShowHurt();
    }
    
    
    void _ShowHurt()
    {
        canvas.enabled = true;
        hurt_img.enabled = true;
        
        Color col = hurt_img.color;
        col.a = 0.7f;
        hurt_img.color = col;
        
        hurtAlphaCurrentSpeed = hurtAlphaStart;
    }
    
    
    void Update()
    {
        if(canvas.enabled)
        {
            float dt = UberManager.DeltaTime();
            if(hurt_img.enabled)
            {
                Color col = hurt_img.color;
                hurtAlphaCurrentSpeed -= dt * hurtAlphaAcceleration * 2;
                
                col.a += dt * hurtAlphaCurrentSpeed;
                if(col.a > 0)
                {
                    hurt_img.color = col;
                }
                else
                {
                    hurt_img.enabled = false;
                }
            }
            if(heal_img.enabled)
            {
                Color col = heal_img.color;
                healAlphaCurrentSpeed -= dt * healAlphaAcceleration * 2;
                col.a += dt * healAlphaCurrentSpeed;
                
                if(col.a > 0)
                {
                    heal_img.color = col;
                }
                else
                {
                    heal_img.enabled = false;
                }
            }
            
            if(!heal_img.enabled && !hurt_img.enabled)
            {
                canvas.enabled = false;
            }
        }
    }
    
    
    
}
