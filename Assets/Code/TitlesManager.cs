using UnityEngine;
using UnityEngine.UI;
using TMPro;


public enum Title
{
    Undefined,
    Undercity,
    The_Machinery,
    Arena_I,
    Arena_II
}

public class TitlesManager : MonoBehaviour
{
    static TitlesManager instance;
    public static TitlesManager Singleton()
    {
        if(instance == null)
        {
            instance = FindObjectOfType<TitlesManager>();
        }
        return instance;
    }
    
    
    
    
    
    public static void SetupCamera(Camera cam)
    {
        Singleton()._SetupCamera(cam);
    }
    
    void _SetupCamera(Camera cam)
    {
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = cam;
        canvas.planeDistance = 2;
    }
    
    Canvas canvas;
    
    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        if(sb == null)
        {
            sb = new System.Text.StringBuilder();            
        }
        
        canvas = GetComponent<Canvas>();
    }
    
    static string[] titles = {"Undefined", "Undercity", "The Machinery", "Arena I", "Arena II"};
    
    //Top title:
    public TextMeshProUGUI topTitle;
    const float topTitle_timeToBeVisible = 6F;
    const float fadeSpeed = 0.75F;
    const float topTitle_alphaDelay = -0.5F;
    const float charTypeSpeed = 0.1F * 0.45F;
    public float topTitle_timer;
    public float topTitle_timerType;
    public float topTitle_alpha;
    
    
    // void Awake()
    // {
    //     if(instance == null)
    //     {
    //         DontDestroyOnLoad(this.gameObject);
    //         instance = this;
    //         TitlesInitialState();
    //     }
    //     else
    //     {
    //         if(instance.GetInstanceID() != this.GetInstanceID())
    //         {
    //             Destroy(this);
    //         }
    //     }
    // }
    
    void OnCharTyped()
    {
        AudioManager.PlayClip(SoundType.onCharTyped1, 0.6f, 1f);
    }
    
    void OnFirstCharTyped()
    {
        AudioManager.PlayClip(SoundType.onCharTyped1, 1f, 1f);
    }
    
    
    public static void ShowTitle(Title title)
    {
        InGameConsole.LogOrange(string.Format("Showing title {0}", title.ToString()));
        instance._ShowTitle2(title);
    }
    
    void _ShowTitle(Title title)
    {
        
        topTitle_alpha = topTitle_alphaDelay;
        topTitle_timer = 0f;
        topTitle_timerType = 0;
        
        int titleIndex = (int)title;
        
        if(titleIndex < 0 || titleIndex > titles.Length)
        {
            titleIndex = 0;
        }
        
        instance.topTitle.SetText(titles[titleIndex]);
        
        Color col = topTitle.color;
        col.a = 0;
        topTitle.color = col;
        
        topTitle.gameObject.SetActive(true);
    }
    
    void ProcessTopTitle(float dt)
    {
        if(topTitle_timer < topTitle_timeToBeVisible)
        {
            topTitle_timer += dt;
            topTitle_alpha += fadeSpeed * dt;
            
            Color col = topTitle.color;
            col.a = topTitle_alpha;
            topTitle.color = col;
        }
        else
        {
            if(topTitle_alpha > 0)
            {
                //topTitle_timer += -dt;
                topTitle_alpha += -fadeSpeed * dt;
                
                Color col = topTitle.color;
                
                
                col.a = topTitle_alpha;
                topTitle.color = col;
            }
        }
    }
    
    bool isWorking = false;
    static System.Text.StringBuilder sb;
    
    
    
    string GetSubstringOfTitle(int titleIndex, int length)
    {
        sb.Clear();
        for(int i = 0; i <= length; i++)
        {
            sb.Append(titles[titleIndex][i]);
            
        }
        
        return sb.ToString();
    }
    
    void ProcessTopTitle2(float dt)
    {
        if(topTitle_timer < topTitle_timeToBeVisible)
        {
            topTitle_timer += dt;
            
            topTitle_timerType += dt;
            if(topTitle_timerType > charTypeSpeed)
            {
//                topTitle_timerType  -= charTypeSpeed;
                topTitle_timerType = 0;
                charIndex++;
                if(charIndex < titles[titleIndex].Length)
                {
                    //string newChar = titles[titleIndex][charIndex].ToString();
                    //string newTitle = topTitle.text + titles[titleIndex][charIndex].ToString();
                    string newTitle = GetSubstringOfTitle(titleIndex, charIndex);
                    topTitle.SetText(newTitle);
                    OnCharTyped();
                }
                
            }
        }
        else
        {
            if(topTitle_alpha > 0)
            {
                topTitle_alpha += -fadeSpeed * dt;
                
                Color col = topTitle.color;
                
                col.a = topTitle_alpha;
                topTitle.color = col;
            }
            else
            {
                Color col = topTitle.color;
                col.a = 0;
                topTitle.color = col;
                isWorking = false;
            }
        }
    }
    
    public int charIndex = 0;
    int titleIndex = 0;
    
    void _ShowTitle2(Title title)
    {
        isWorking = true;
        topTitle_timer = 0f;
        topTitle_alpha = 1;
        
        
        
        Color col = topTitle.color;
        col.a = topTitle_alpha;
        topTitle.color = col;
        
        titleIndex = (int)title;
        
        if(titleIndex < 0 || titleIndex > titles.Length)
        {
            titleIndex = 0;
        }
        
        charIndex = 0;
        topTitle_timerType = -0.5f;
        
        instance.topTitle.SetText(titles[titleIndex][charIndex].ToString());
        OnFirstCharTyped();
        
        topTitle.gameObject.SetActive(true);
    }
    
    
    
    
    void TitlesInitialState()
    {
        topTitle_timer = topTitle_timeToBeVisible;
        titleIndex = 0;
        topTitle_alpha = topTitle_alphaDelay;
        topTitle_timer = 0f;
        Color col = topTitle.color;
        col.a = 0;
        topTitle.color = col;
        
        topTitle.SetText(string.Empty);
        
        
        topTitle.gameObject.SetActive(false);
    }
    
    void Update()
    {
        if(isWorking)
        {
            float dt = UberManager.DeltaTime();
            ProcessTopTitle2(dt);
        }
    }
    
    
    
}
