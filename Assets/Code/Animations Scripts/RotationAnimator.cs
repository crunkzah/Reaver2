using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationAnimator : MonoBehaviour
{
    public enum LocalDirection { FORWARD, UP, RIGHT, BACK, DOWN, LEFT}
    public bool isActive = true;

    public LocalDirection localDirection;
    public float rotationSpeed = 60f; // This is in degrees
    public Transform objectToRotate;
    
    public bool randomizeRotation = false;
    public float randomizeRotationStep = 45F;
    
    public bool randomizePositionOnAxis = false;
    public float randomizePositionStep = 0.075F;
    
    Vector3 GetDirection()
    {
        Vector3 dir = Vector3.forward;
        switch(localDirection)
        {
            case LocalDirection.FORWARD:
                dir = Vector3.forward;
                break;
            case LocalDirection.UP:
                dir = Vector3.up;
                break;
         
            case LocalDirection.RIGHT:
                dir = Vector3.right;
                break;
         
            case LocalDirection.BACK:
                dir = Vector3. back;
                break;
         
            case LocalDirection.DOWN:
                dir = Vector3.down;
                break;
         
            case LocalDirection.LEFT:
                dir = Vector3.left;
                break;
        }

        return dir;
    }

    void Start()
    {
        if(objectToRotate == null)
            objectToRotate = transform;
            
        if(randomizeRotation)
        {
            float rand = Random.Range(-6, 6) * randomizeRotationStep;
            objectToRotate.Rotate(GetDirection() * rand, space);
        }
        
        if(randomizePositionOnAxis)
        {
            float rand = Random.Range(-1f, 1f) * randomizePositionStep;
            objectToRotate.Translate(GetDirection() * rand, space);
        }
        
//        RotationManager.Singleton().RegisterRotator(this);
    }
    
    public UnityEngine.Space space = UnityEngine.Space.Self;

    public void Update()
    {
        if(isActive)
        {
            float dt = UberManager.DeltaTime();
            objectToRotate.Rotate(GetDirection() * rotationSpeed * dt, space);
        }
    }
}
