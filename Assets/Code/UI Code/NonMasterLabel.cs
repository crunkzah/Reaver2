using UnityEngine;
using TMPro;

public class NonMasterLabel : MonoBehaviour
{
    string[] engLabels = new string[] {
        "Waiting for host to choose level",
        "Waiting for host to choose level.",
        "Waiting for host to choose level..",
        "Waiting for host to choose level..."};
    
    float freq = 0.33f;
    float timer  = 0;
    int stringIndex = 0;
    
    TextMeshProUGUI tmp;
    
    void OnEnable()
    {
        stringIndex = 0;
        timer = 0;
        RefreshLabel();
    }
    
    void RefreshLabel()
    {
        if(tmp == null)
        {
            tmp = GetComponent<TextMeshProUGUI>();
        }
        tmp.SetText(engLabels[stringIndex]);
    }
    
    void Update()
    {
        float dt = UberManager.DeltaTime();
        timer += dt;
        if(timer > freq)
        {
            timer %= freq;
            stringIndex++;
            if(stringIndex >= 4)
            {
                stringIndex = 0;
            }
            RefreshLabel();            
        }
    }
    
}
