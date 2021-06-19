using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersistenRotation : MonoBehaviour
{
    Transform thisTransform;
    
    static Quaternion Identity = Quaternion.identity;
    
    void Start()
    {
        thisTransform = transform;
    }
    
    void Update()
    {
        thisTransform.rotation = Identity;
    }
}
