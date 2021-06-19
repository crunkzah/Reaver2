using UnityEngine;

public class ReactiveRotator : MonoBehaviour
{
    public Vector3 axis = Vector3.right;
    
    public float frictionK = 720;
    public float force;
    
    Transform thisTransform;
    Quaternion localOriginalRotation;
    
    void Awake()
    {
        thisTransform = transform;
        localOriginalRotation = thisTransform.localRotation;
    }
    
    void OnEnable()
    {
        force = 0;
        thisTransform.localRotation = localOriginalRotation;
    }
    
    public void RotateByForce(float f)
    {
        force += f;
    }
    
    void Update()
    {
        float dt = UberManager.DeltaTime();
        
        force = Mathf.MoveTowards(force, 0, dt * frictionK);
    
        thisTransform.Rotate(axis * dt * force);
    }
}
