using UnityEngine;

public class FPSWeaponPlaceMover : MonoBehaviour
{
    Vector3 localOrigin = new Vector3(0.169f, -0.4729f, 0.256f);
    Vector3 fpsVel;
    Transform thisTransform;
    
    void Awake()
    {
        thisTransform = transform;
        // localOrigin = thisTransform.localPosition;
    }
    
    Vector3 dump;
    public float smoothTime = 0.5f;
    public float scaleVertical = 0.05f;
    public float scaleHorizontal = 0.05f;
    
    public void Tick(Vector3 vel)
    {
        fpsVel = -vel;
        
        fpsVel.x = Mathf.Clamp(fpsVel.x, -10, 10);
        fpsVel.z = Mathf.Clamp(fpsVel.z, -10, 10);
        fpsVel.y = Mathf.Clamp(fpsVel.y, -12, 12);
        
        if(Math.Abs(fpsVel.x) > 10)
             fpsVel.x = Mathf.Sign(fpsVel.x) * 10;
        
        fpsVel.x *= scaleHorizontal;
        fpsVel.z *= scaleHorizontal;
        fpsVel.y *= scaleVertical;
        
        
        Vector3 targetLocalPos = localOrigin - fpsVel;
        
        Vector3 updatedLocalPos = Vector3.SmoothDamp(thisTransform.localPosition, targetLocalPos, ref dump, smoothTime);
        thisTransform.localPosition = updatedLocalPos;
    }
    
}
