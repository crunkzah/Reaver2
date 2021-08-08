using UnityEngine;
using UnityEngine.UI;

public class CrosshairController : MonoBehaviour
{
    static CrosshairController _instance;
    
    public static CrosshairController Singleton()
    {
        if(_instance == null)
        {
            _instance = FindObjectOfType<CrosshairController>();
        }
        
        return _instance;
    }
        
    public RectTransform v_u;
    public RectTransform v_d;
    public RectTransform h_l;
    public RectTransform h_r;
    
    
    Vector2 v_u_zero = new Vector2(0, 8);
    Vector2 v_d_zero = new Vector2(0, -8);
    Vector2 h_l_zero = new Vector2(-8, 0);
    Vector2 h_r_zero = new Vector2(8, 0);
    
    // public Vector3 v_u_p;
    // public Vector3 v_d_p;
    // public Vector3 h_l_p;
    // public Vector3 h_r_p;
    
    public static void HideCrosshair()
    {
        Singleton().v_u.GetComponent<Image>().enabled = false;
        Singleton().v_d.GetComponent<Image>().enabled = false;
        Singleton().h_l.GetComponent<Image>().enabled = false;
        Singleton().h_r.GetComponent<Image>().enabled = false;
    }
    
    public static void ShowCrosshair()
    {
        Singleton().v_u.GetComponent<Image>().enabled = true;
        Singleton().v_d.GetComponent<Image>().enabled = true;
        Singleton().h_l.GetComponent<Image>().enabled = true;
        Singleton().h_r.GetComponent<Image>().enabled = true;
    }
    
    Vector2 deriv1;
    Vector2 deriv2;
    Vector2 deriv3;
    Vector2 deriv4;
    
    const float smoothTime = 0.175F;
    
    float trauma = 0;
    const float max_trauma = 1;
    
    public float trauma_mult = 3;
    
    public static void MakeTrauma(float x)
    {
        Singleton()._MakeTrauma(x);
    }
    
    public void _MakeTrauma(float x)
    {
        trauma += x;
        if(trauma > max_trauma)
        {
            trauma = max_trauma;
        }
    }
    
    const float trauma_scale = 2;
    
    float trauma_deriv;
    
    void TraumaTick()
    {
        trauma = Mathf.SmoothDamp(trauma, 0, ref trauma_deriv, smoothTime);
    }
    
    void Update()
    {
        TraumaTick();
        
        float trauma_scaled = trauma * trauma_scale;
        
        
        v_u.anchoredPosition = v_u_zero + trauma_scaled * v_u_zero;
        v_d.anchoredPosition = v_d_zero + trauma_scaled * v_d_zero;
        h_l.anchoredPosition = h_l_zero + trauma_scaled * h_l_zero;
        h_r.anchoredPosition = h_r_zero + trauma_scaled * h_r_zero;
        
        float scale = 1f + trauma_scaled * 0.5f;
        v_u.localScale = new Vector3(scale, scale, scale);
        v_d.localScale = new Vector3(scale, scale, scale);
        h_l.localScale = new Vector3(scale, scale, scale);
        h_r.localScale = new Vector3(scale, scale, scale);
        // v_u_p = v_u.anchoredPosition;
        // v_d_p = v_d.anchoredPosition;
        // h_l_p = h_l.anchoredPosition;
        // h_r_p = h_r.anchoredPosition;
    }
}
