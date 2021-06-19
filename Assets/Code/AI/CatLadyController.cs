using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatLadyController : MonoBehaviour
{
    
    Rigidbody[] joint_rbs;
    
    void InitJoints()
    {
      //  joints = GetComponentsInChildren<CharacterJoint>();
        joint_rbs = GetComponentsInChildren<Rigidbody>();
    }
    
    void EnableSkeleton()
    {
        int len = joint_rbs.Length;
        for(int i = 0; i < len; i++)
        {
            if(joint_rbs[i])
                joint_rbs[i].isKinematic = false;
        }
    }
    
    public void DisableSkeleton()
    {
        int len = joint_rbs.Length;
        for(int i = 0; i < len; i++)
        {
            if(joint_rbs[i])
                joint_rbs[i].isKinematic = true;
        }
    }
    
    void Awake()
    {
        InitJoints();
    }
    
    void Start()
    {
        DisableSkeleton();
    }

    void Update()
    {
        
    }
}
