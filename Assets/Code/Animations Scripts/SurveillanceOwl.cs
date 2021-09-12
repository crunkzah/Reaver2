using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurveillanceOwl : MonoBehaviour
{
    public Transform eyeLeft;
    public Transform eyeRight;
    
    public Transform target;
    Transform thisTransform;
    
    const float sqrViewRadius = 27f * 27f;
    
    void Start()
    {
        thisTransform = GetComponent<Transform>();
        originalEyesDirection = eyeLeft.forward;
        originalHeadDirection = thisTransform.forward;
    }
    
    const float scanRate = 0.1f;
    float timeForNextScan = 0f;
    public float dotProduct;
    
    
    Vector3 originalEyesDirection;
    Vector3 originalHeadDirection;
    
    void Update()
    {
        
        if(UberManager.TimeSinceStart() > timeForNextScan)
        {
            float minSqrDistance = float.MaxValue;
            
            target = null;
            
            // for(int i = 0; i < NPCManager.Singleton().aiTargets.Count; i++)
            // {
            //     Transform possibleTarget = NPCManager.Singleton().aiTargets[i];
            //     float sqrDistance = Math.SqrDistance(thisTransform.position, NPCManager.Singleton().aiTargets[i].position);
                
            //     if(sqrDistance < sqrViewRadius && sqrDistance < minSqrDistance)                        
            //     {
            //         target = possibleTarget;
            //         minSqrDistance = sqrDistance;
            //     }
            // }
            
            
            
            timeForNextScan = UberManager.TimeSinceStart() + scanRate;         
        }
        
        if(target != null)
        {
            Vector3 dirL = (target.position + Globals.playerEyesOffset - eyeLeft.position);
            dirL.Normalize();
            
            Vector3 dirR = (target.position + Globals.playerEyesOffset - eyeRight.position);
            dirR.Normalize();
            
            dotProduct = Vector3.Dot(dirL, originalEyesDirection);
            if(dotProduct > 0.65f)
            {
                eyeLeft.forward  = dirL;
                eyeRight.forward = dirR;
            }
            else
            {
                eyeLeft.forward = originalEyesDirection;
                eyeRight.forward = originalEyesDirection;
            }
            
            // Vector3 dirHead = (target.position - thisTransform.position);
            // dirHead.Normalize();
            
            // dotProduct = Vector3.Dot(dirHead, originalHeadDirection);
            
            // if(dotProduct > 0.925f)
            // {
            //     thisTransform.forward = dirHead;
                
            // }
        }
    }
}
