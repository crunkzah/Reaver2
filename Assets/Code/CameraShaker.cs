using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShaker : MonoBehaviour
{
    static CameraShaker _instance;
    public static CameraShaker Singleton()
    {
        if(_instance == null)
        {
            _instance = FindObjectOfType<CameraShaker>();
        }        
        
        return _instance;
    }
    
    Transform thisTransform;
    float shakeMult = 0.5f;
    float smoothTime = 0.33f;
    float trauma = 0;
    const float MAX_TRAUMA = 4f;
    
    float horizontalMult = 0.8f;
    float verticalMult = 1.33f;
    
    Vector3 normalPos;
    Vector3 duckingPos;
    
    Vector3 normalCameraPlacePosition = new Vector3(0, 0, 0);
    Vector3 duckCameraPlacePosition = new Vector3(0, -1.1f, 0);
    
    void Awake()
    {
        thisTransform = transform;
    }
    
    static bool canMakeTrauma = true;
    
    public static void MakeTrauma(float amount)
    {
        if(!canMakeTrauma)
        {
            return;
        }
        
        float modified_trauma = Singleton().trauma;
        modified_trauma += amount;
        modified_trauma = Mathf.Clamp(modified_trauma, 0, MAX_TRAUMA);
        Singleton().trauma = modified_trauma;
    }
    
    float v;
    
    public float vAmount = 0.2f;
    
    bool isDucking = false;
    
    public static void Slide()
    {
        Singleton().isDucking = true;
    }
    
    public static void UnSlide()
    {
        Singleton().isDucking = false;
    }
    
    float duckingShake = 0.23f;
    
    float shakeVelocityY = 0;
    Vector3 shakedOffset;
    float snapbackStrengthY = 60;
    
    public static void ShakeY(float amount)
    {
        Singleton().shakeVelocityY -= amount;
    }
    
    void ShakingY()
    {
        float dt = UberManager.DeltaTime();
        
        //shakeVelocityY = Math.Clamp(-10, 10, shakeVelocityY);
        
        shakeVelocityY += snapbackStrengthY * dt;
        
        float dY = shakeVelocityY * dt;
        
        if(shakedOffset.y + dY > 0f)
        {
            shakedOffset.y = 0f;
            shakeVelocityY = 0f;
        }
        else
            shakedOffset.y += dY;    
            
       
    }
    
    float shakeYAmount = 4;
    
    
    
    
    void LateUpdate()
    {
        //return;
        // if(Input.GetKeyDown(KeyCode.V))
        // {
        //     ShakeY(shakeYAmount);
        // }
        
        float dt = UberManager.DeltaTime();
        
        
        float offsetX = trauma * shakeMult * Random.Range(-1f, 1f) * horizontalMult;
        float offsetY = trauma * shakeMult * Random.Range(-1f, 1f) * verticalMult;
        
        trauma = Mathf.SmoothDamp(trauma, 0, ref v, smoothTime, 1000, dt);
        
        Vector3 duckOffset = isDucking ? duckCameraPlacePosition : normalCameraPlacePosition;
        
        if(isDucking)
        {
            MakeTrauma(duckingShake * dt);
        }
        
        ShakingY();
        
        thisTransform.localPosition = new Vector3(offsetX, offsetY, 0) + duckOffset + shakedOffset;// + ;
    }
    
}
