// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;

public enum TrackingMode : byte
{
    None,
    CopyRotation,
    LookAt,
}

public class TrackTo : MonoBehaviour
{
    Transform thisTransform;
    Transform transformCopyRotation;
    
    public TrackingMode mode;
    
    public float x_qOffset = -90f;
    public float y_qOffset = 180f;
    public float z_qOffset = 0f;
    
    void Awake()
    {
        thisTransform = transform;
        GameObject _obj = new GameObject("ObjectToCopyRotation");
        transformCopyRotation = _obj.transform;
        
        transformCopyRotation.SetParent(thisTransform);
        transformCopyRotation.localPosition = new Vector3(0, 0, 0);
        transformCopyRotation.localRotation = Quaternion.identity;
    }
    
    public Vector3 lookAtPosition;
    
    public void LookAtPos(Vector3 _pos)
    {
        lookAtPosition = _pos;
    }
    public void SetMode(TrackingMode _mode)
    {
        mode = _mode;
    }
    
    Quaternion deriv;
    
    public float smoothTime = 0.5f;
    
    void LateUpdate()
    {
        switch(mode)
        {
            case(TrackingMode.None):
            {
                break;
            }
            case(TrackingMode.LookAt):
            {
                
                Quaternion lookRotation = Quaternion.LookRotation((lookAtPosition - transformCopyRotation.position).normalized);
                
                Quaternion smoothLookRot = QuaternionUtil.SmoothDamp(transformCopyRotation.rotation, lookRotation, ref deriv, smoothTime);
                transformCopyRotation.rotation = smoothLookRot;
                
                //transformCopyRotation.LookAt(lookAtPosition);
                thisTransform.rotation = transformCopyRotation.rotation * Quaternion.Euler(x_qOffset, y_qOffset, z_qOffset);
                
                 break;
            }
            case(TrackingMode.CopyRotation):
            {
                thisTransform.rotation = transformCopyRotation.rotation * Quaternion.Euler(x_qOffset, y_qOffset, z_qOffset);
                
                
                break;
            }
        }    
    }
}
