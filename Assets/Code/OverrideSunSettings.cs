using UnityEngine;

public class OverrideSunSettings : MonoBehaviour
{
    Light l;
    
    public bool changeIntensity = true;
    public float intensity = 0.8f;
    
    public bool changeColor = true;
    public Color col;
    
    void Start()
    {
        l = GetComponent<Light>();
        l.shadows = LightShadows.None;
        if(changeIntensity)
            l.intensity = intensity;
        if(changeColor)
            l.color  = col;
    }
    
}
