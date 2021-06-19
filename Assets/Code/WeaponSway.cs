using UnityEngine;

public class WeaponSway : MonoBehaviour
{
    Transform thisTransform;
    public Transform target;
    
    Quaternion originalRotation;
    
    
    float rotateTime = 0.0233f;
    
    public Vector3 offsetRot;
    
    void Awake()
    {
        thisTransform = transform;
        offsetRot = thisTransform.rotation.eulerAngles;
    }
    
    Vector3 velocity = Vector3.zero;
    Quaternion deriv;
    
    void Update()
    {
        //return;
        
        //thisTransform.rotation = Quaternion.identity;
        if(target)
        {
            
            float dt = UberManager.DeltaTime();
            // originalRotation = Quaternion.Euler(Vector3.SmoothDamp(originalRotation.eulerAngles, target.rotation.eulerAngles, ref velocity, rotateTime * dt));
            // originalRotation = Quaternion.RotateTowards(originalRotation, target.rotation, speedDegrees * dt);
            
            originalRotation = QuaternionUtil.SmoothDamp(originalRotation, target.rotation, ref deriv, rotateTime);
            thisTransform.rotation = originalRotation;
            thisTransform.Rotate(offsetRot, Space.Self);
        }
    }
    
}
