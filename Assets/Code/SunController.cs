using System.Collections;
using UnityEngine;

public class SunController : MonoBehaviour
{
    static SunController instance;
    public static SunController Singleton()
    {
        if(instance == null)
        {
            instance = FindObjectOfType<SunController>();    
        }
        
        return instance;
    }
    
    Light sun;

    float animationTimer = 0f;
    
    [Header("Light animation:")]
    public bool animateIntensity = true;
    public float minIntensity    = 0.12f;
    public float maxIntensity    = 0.6f;
    public float freq = 0.2f;
    bool positiveDir = true;
    
    
    
    
    void Awake()
    {
        sun = GetComponent<Light>();
    }
    
    void Update()
    {
        
        animationTimer += UberManager.DeltaTime() * freq;
        
        if(animationTimer >= 1f)
        {
            animationTimer -= 1f;
            positiveDir = !positiveDir;
        }
        
        if(animateIntensity)
        {
            float t = animationTimer;
            
            
            
            if(positiveDir)
            {
                sun.intensity = Mathf.SmoothStep(minIntensity, maxIntensity, t);
            }
            else
            {
                sun.intensity = Mathf.SmoothStep(maxIntensity, minIntensity, t);
            }
        }
    }
}
