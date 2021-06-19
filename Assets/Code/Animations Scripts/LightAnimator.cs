using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightAnimator : MonoBehaviour
{
    new Light light;
    public float freq = 1;
    public float minIntensity = 0;
    public float maxIntensity = 1;
    
    bool direction;
    
    public AnimationCurve curve;
    float t;
    
    void Start()
    {
        light = GetComponent<Light>();
    }
    
    void Update()
    {
        t += UberManager.DeltaTime() * freq;
        
        light.intensity = Math.Lerp(minIntensity, maxIntensity, curve.Evaluate(t));
    }
    
}
