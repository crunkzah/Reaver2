using UnityEngine;

public class OverrideLightSettings : MonoBehaviour
{
    
    public Color lightColor;
    public float lightIntensity;
    
    void Start()
    {
        Light light = GetComponent<Light>();
        if(light)
        {
            light.color = lightColor;
            light.intensity = lightIntensity;
        }
    }
    
}
