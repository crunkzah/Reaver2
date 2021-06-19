using UnityEngine;
using UnityEngine.UI;

public class BackButtonGUI : MonoBehaviour
{
    
    Button btn;
    
    void Awake()
    {
        btn = GetComponent<Button>();
    }
    
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            btn.onClick.Invoke();
        }
    }
    
}
