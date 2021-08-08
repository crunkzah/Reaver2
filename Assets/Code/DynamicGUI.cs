using UnityEngine;

public class DynamicGUI : MonoBehaviour
{
    RectTransform rect_tr;
    
    PlayerController LocalPlayer;
    
    void Awake()
    {
        rect_tr = GetComponent<RectTransform>();
    }
    
    // void Update()
    // {
    //     if(!LocalPlayer)
    //     {
    //         LocalPlayer = PhotonManager.GetLocalPlayer();
    //     }
        
    //     sizeDelta = rect_tr.sizeDelta;
    //     anchoredPosition = rect_tr.anchoredPosition;
    // }
    
    public Vector2 sizeDelta;
    public Vector2 anchoredPosition;
    
    public float multiplier = 1;
    
    public float maxVelMagnitude = 21F;
   // [Range(0.5f, 1f)]
    const float scale_smallest = 0.975f;
   // [Range(1f, 1.2f)]
    const float scale_biggest = 1.025f;
    const float scale_smoothTime = 0.050f;
    float dump;
    
    void Update()
    {
        
        if(LocalPlayer)
        {
            Transform fpsCamTransform = LocalPlayer.GetFPSCameraTransform();
            if(fpsCamTransform)
            {
                Vector3 fpsVel = LocalPlayer.GetFPSVelocity();
                Vector3 fpsVelDir = fpsVel.normalized;
                float fpsMagnitude = Vector3.Magnitude(fpsVel);
                Vector3 fpsDir = fpsCamTransform.forward;
                float dot = Vector3.Dot(Math.GetXZ(fpsVelDir), Math.GetXZ(fpsDir));
                // Vector3 r = -fpsVelDir * fpsVel;
                // rect_tr.anchoredPosition = new Vector2(r.x, r.z);
                float mag_t = Mathf.InverseLerp(0, maxVelMagnitude, fpsMagnitude);
                float scale_target = Mathf.Lerp(scale_smallest, scale_biggest, 0.5f + 0.5f*dot);
                //float scale_target = Math.Lerp(scale_smallest, scale_biggest, mag_t);
                
                
                float scale_smoothed = Mathf.SmoothDamp(rect_tr.localScale.x, scale_target, ref dump, scale_smoothTime);
                //MathUt
                rect_tr.localScale = new Vector3(scale_smoothed, scale_smoothed, scale_smoothed);
            }
        }
        else
        {
            LocalPlayer = PhotonManager.GetLocalPlayer();
            rect_tr.localScale = new Vector3(1f, 1f, 1f);
        }
    }
}
