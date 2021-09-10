using UnityEngine;

public class SecretLady : MonoBehaviour
{
    bool wasTouched = false;
    
    public bool has_msg = true;
    public string eng_msg = "These games are getting better than realistic...";
    public string rus_msg = "These games are getting better than realistic...";
    
    string msg = string.Empty;
    
    public void OnTouch()
    {
        if(wasTouched)
        {
            return;
        }
        
        if(has_msg)
        {
            wasTouched = true;
            switch(UberManager.lang)
            {
                case(Language.English):
                {
                    if(string.IsNullOrEmpty(eng_msg))
                    {
                        return;
                    }
                    msg = eng_msg;
                    break;
                }
                case(Language.Russian):
                {
                    if(string.IsNullOrEmpty(rus_msg))
                    {
                        return;
                    }
                    msg = rus_msg;
                    break;
                }
            }
            Invoke(nameof(SM), 2.25f);
        }
    }
    
    void SM()
    {
        MessagePanel.Singleton().ShowMessage(msg, 8);
    }
            
    // // Start is called before the first frame update
    // void Start()
    // {
        
    // }

    // // Update is called once per frame
    // void Update()
    // {
        
    // }
}
