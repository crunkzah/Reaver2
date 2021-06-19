using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectionalLightController : MonoBehaviour
{
    static DirectionalLightController _instance;


    public float xTilt = 65f;

    public static DirectionalLightController singleton()
    {
        if(_instance == null)
        {
            _instance = FindObjectOfType<DirectionalLightController>();
        }

        return _instance;
    }

    static Transform targetToAlignTo;

    public static void BindDirectionalLight(Transform target)
    {
        targetToAlignTo = target;
    }

    public static void UnBindDirectionalLight()
    {
        targetToAlignTo = null;
        if(singleton() != null)
        {
            singleton().transform.rotation = Quaternion.Euler(singleton().xTilt, 0f, 0f);
        }
    }

    void Update()
    {
        if(targetToAlignTo != null)
        {
            singleton().transform.rotation = Quaternion.Euler(xTilt, targetToAlignTo.eulerAngles.y, 0f);
        }
    }



}
