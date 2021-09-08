using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;

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
    
    int currentCharIndex;
    string msg_target = string.Empty;
    

    public void ShowMessage(string msg, float time)
    {
        timer = time;
        tmp.SetText(msg);
        //charTimer = 0;
        //tmp.SetText(string.Empty);
        //msg_target = msg;
        currentCharIndex = 0;
        
        OnShow();        
        Invoke(nameof(Flicker), 0.15F);
        Invoke(nameof(Flicker), 0.3F);
        Invoke(nameof(Flicker), 0.45F);
        Invoke(nameof(Flicker), 0.6F);
    }
    
    public static void HideMessage()
    {
        Singleton().OnHide();
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
    
    const float charTypeTime = 0.1f;
    float charTimer;
    
 
    
    void Update()
    {
        float dt = UberManager.DeltaTime();
      
        if(timer > 0)
        {
            timer -= dt;
            if(timer <= 0f)
            {
                OnHide();                
            }
        }
    }
    
}
