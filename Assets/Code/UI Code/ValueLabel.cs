using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ValueLabel : MonoBehaviour
{
    public Scrollbar scrollbar_target;

    TextMeshProUGUI tmp;

    void Awake()
    {
        tmp = GetComponent<TextMeshProUGUI>();
    }

    

    void Update()
    {
        if(scrollbar_target)
        {
            float v = scrollbar_target.value;
            tmp.SetText( ((int)(v * 100)).ToString());
        }
    }

}
