using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameStats : MonoBehaviour
{
    static GameStats _instance;
    
    public RectTransform statsPanel;
    public TextMeshProUGUI gameTimer_label;
    public TextMeshProUGUI restarts_label;
    public TextMeshProUGUI difficulty_label;
    public TextMeshProUGUI inferno_circle_label;
    
    const float speed = 2000 / 1.2f;
    
    
    public float time_spent_target;
    float time_spent_current;
    
    void Start()
    {
        Hide();
    }
    
    public static GameStats Singleton()
    {
        if(_instance == null)
        {
            _instance = FindObjectOfType<GameStats>();
        }
        
        return _instance;
    }
    
    public static void SetStats(int restarts, float time_spent, int _difficulty)
    {
        Singleton()._SetStats(restarts, time_spent, _difficulty);
    }
    
    
    string GetDifficultyString(int diff)
    {
        switch(diff)
        {
            case(0):
            {
                return "Easy";
                break;
            }
            case(1):
            {
                return "Normal";
                break;
            }
            case(2):
            {
                
                return "Hard";
                break;
            }
            case(3):
            {
                return "Nightmare";
                break;
            }
            default:
            {
                return "Invalid difficulty";
            }
        }
    }
    
    void _SetStats(int restarts, float time_spent, int difficulty)
    {
        restarts_label.SetText(restarts.ToString());
        difficulty_label.SetText(GetDifficultyString(difficulty));
        time_spent_target = time_spent;
        time_spent_current = 0;
    }
    
    public static void Hide()
    {
        //return;
        Singleton()._Hide();
    }
    
    void _Hide()
    {
        statsPanel.gameObject.SetActive(false);
        isShowing = false;
    }
    
    public static bool IsShowing()
    {
        return Singleton().isShowing;
    }
    
    bool isShowing = false;
    
    public static void Show()
    {
        Singleton()._Show();
    }
    
    void _Show()
    {
        if(isShowing)
        {
            return;
        }
        
        statsPanel.anchoredPosition = new Vector2(0, 2000);
        statsPanel.gameObject.SetActive(true);
        isShowing = true;
    }
    
    static readonly Vector2 V2_zero = new Vector2(0, 0);
    
    void Update()
    {
        if(isShowing)
        {
            float dt = UberManager.DeltaTime();
            
            statsPanel.anchoredPosition = Vector2.MoveTowards(statsPanel.anchoredPosition, V2_zero, speed * dt);
            if(time_spent_current != time_spent_target)
                gameTimer_label.SetText(time_spent_current.ToString("f"));
            
            float v = time_spent_target / 4f;
            time_spent_current = Mathf.MoveTowards(time_spent_current, time_spent_target, dt * v);
            
            if(Input.GetKeyDown(KeyCode.Escape))
            {
                Hide();
            }
        }
    }
}
