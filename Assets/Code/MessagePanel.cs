using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MessagePanel : MonoBehaviour
{

    static MessagePanel _instance;
    public static MessagePanel Singleton()
    {
        if(_instance == null)
        { 
            _instance = FindObjectOfType<MessagePanel>();
        }
        
        return _instance;
    }
    
    void Start()
    {
        OnHide();
    }
    

    public TextMeshProUGUI tmp;
    public Image panel_img;

    float timer = 0;

    public void ShowMessage(string msg, float time)
    {
        timer = time;
        tmp.SetText(msg);
        
        OnShow();        
        Invoke(nameof(Flicker), 0.15F);
        Invoke(nameof(Flicker), 0.3F);
        Invoke(nameof(Flicker), 0.45F);
        Invoke(nameof(Flicker), 0.6F);
    }
    
    public void HideMessage()
    {
        OnHide();
    }
    
    void Flicker()
    {
        panel_img.enabled = !panel_img.enabled;
        tmp.enabled = !tmp.enabled;
    }
    
    void OnShow()
    {
        panel_img.enabled = true;
        tmp.enabled = true;
        AudioManager.PlayClip(SoundType.ui_blip1, 1, 1.4f);
    }
    
    void OnHide()
    {
        panel_img.enabled = false;
        tmp.enabled = false;
    }
    
    void Update()
    {
        if(timer > 0)
        {
            float dt = UberManager.DeltaTime();
            
            timer -= dt;
            if(timer <= 0f)
            {
                OnHide();                
            }
        }
    }
    
}
