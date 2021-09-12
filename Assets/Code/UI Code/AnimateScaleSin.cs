using UnityEngine;

public class AnimateScaleSin : MonoBehaviour
{
    Transform thisTransform;
    
    void Awake()
    {
        thisTransform = transform;
    }
    
    float sinFreq = 4;
    float sinScale = 0.025f;
    
    void Update()
    {
        float scale = 1f + sinScale * Mathf.Sin(sinFreq * Time.time);
        thisTransform.localScale = new Vector3(scale, scale, 1);
    }
}
