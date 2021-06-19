using UnityEngine;

public class ScaleAnimator : MonoBehaviour
{
    public float minScale = 0.95f;
    public float maxScale = 1.15f;
    public float freq = 1.5f;
    public float amplitude = 0.15f;
    
    float currentScale = 1;

    void Update()
    {
        currentScale = minScale + amplitude * Mathf.Sin(Time.time * freq);
        
        transform.localScale = new Vector3(currentScale, currentScale, currentScale);
    }
    
    
    
}
