using UnityEngine;

public class WeaponSway : MonoBehaviour
{
    Transform thisTransform;
    public Transform target;
    
    Quaternion currentRotation;
    
    
    float rotateTime = 0.0233f;
    
    public Vector3 offsetRot;
    
    void Awake()
    {
        thisTransform = transform;
        offsetRot = thisTransform.rotation.eulerAngles;
    }
    
    // public void ReadOriginalRotation()
    // {
    //     originalRotation
    // }
    
    Vector3 velocity = Vector3.zero;
    Quaternion deriv;
    
    void Update()
    {
        //return;
        
        //thisTransform.rotation = Quaternion.identity;
        if(target)
        {
            //return;
            float dt = UberManager.DeltaTime();
            // originalRotation = Quaternion.Euler(Vector3.SmoothDamp(originalRotation.eulerAngles, target.rotation.eulerAngles, ref velocity, rotateTime * dt));
            // originalRotation = Quaternion.RotateTowards(originalRotation, target.rotation, speedDegrees * dt);
            //Quaternion.Slerp()
            
            currentRotation = QuaternionUtil.SmoothDamp(currentRotation, target.rotation, ref deriv, rotateTime);
            thisTransform.rotation = currentRotation;
            thisTransform.Rotate(offsetRot, Space.Self);
        }
    }
    
}
