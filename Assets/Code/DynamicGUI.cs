using UnityEngine;

public class DynamicGUI : MonoBehaviour
{
    RectTransform rect_tr;
    
    PlayerController LocalPlayer;
    
    void Awake()
    {
        rect_tr = GetComponent<RectTransform>();
    }
    
    void Update()
    {
        if(!LocalPlayer)
        {
            LocalPlayer = PhotonManager.GetLocalPlayer();
        }
        
        sizeDelta = rect_tr.sizeDelta;
        anchoredPosition = rect_tr.anchoredPosition;
    }
    
    public Vector2 sizeDelta;
    public Vector2 anchoredPosition;
    
    public float multiplier = 1;
    
    void LateUpdate()
    {
        
        if(LocalPlayer)
        {
            LocalPlayer.GetFPSVelocity();
        }
    }
}
