using UnityEngine;
using UnityEngine.UI;


public class DeadGUI : MonoBehaviour
{
    static DeadGUI _instance;
    
    
    public Canvas canvas;
    
    public static DeadGUI Singleton()
    {
        if(_instance == null)
        {
            _instance = FindObjectOfType<DeadGUI>();
        }
        
        return _instance;
    }
    
    void Start()
    {
        Hide();
    }
    
    public static void Show()
    {
        Singleton().canvas.enabled = true;
    }
    
    public static void Hide()
    {
        //return;
        Singleton().canvas.enabled = false;
    }
}
