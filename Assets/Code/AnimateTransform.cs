using UnityEngine;

public class AnimateTransform : MonoBehaviour
{
    public float offset;
    public float freq = 1f;
    public float scaleMult = 1;
    // public Vector3 rotationAxis = new Vector3(0, 0, 0);
    
    void Awake()
    {
        offset = Random.Range(-10.9f, 10.9f);
        //offset = 100;
        TransformAnimatorManager.Singleton().RegisterTransformFloat(this);
    }
}
