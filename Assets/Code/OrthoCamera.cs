using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrthoCamera : MonoBehaviour
{
    static OrthoCamera _instance;
    public ParticleSystem skulls_ps;
    
    public static OrthoCamera Singleton()
    {
        if(_instance == null)
        {
            _instance = FindObjectOfType<OrthoCamera>();
            if(_instance.cam == null)
            {
                _instance.cam = _instance.GetComponent<Camera>();
            }
        }
        
        return _instance;
    }
    
    Camera cam;
    void Start()
    {
        cam = GetComponent<Camera>();
        Hide();
        //Disable
    }
    
    public static void Show()
    {
        Singleton().cam.enabled = true;
        
        Singleton().skulls_ps.Play();
    }
    
    public static void Hide()
    {
        Singleton().cam.enabled = false;
        Singleton().skulls_ps.Stop();
    }
}
