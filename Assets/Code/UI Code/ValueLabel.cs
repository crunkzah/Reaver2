using UnityEngine;
using UnityEngine.UI;
using TMPro;


public enum ValueLabelMode : byte
{
    Percent,
    Fov,
}

public class ValueLabel : MonoBehaviour
{
    public Scrollbar scrollbar_target;

    TextMeshProUGUI tmp;
    
    public ValueLabelMode mode;

    void Awake()
    {
        tmp = GetComponent<TextMeshProUGUI>();
    }

    [Header("FoV label Settings:")]
    public float fov_min = 70F;
    public float fov_max = 160F;

    void Update()
    {
        switch(mode)
        {
            case(ValueLabelMode.Percent):
            {
                if(scrollbar_target)
                {
                    float v = scrollbar_target.value;
                    tmp.SetText( ((int)(v * 100)).ToString());
                }
                break;
            }
            case(ValueLabelMode.Fov):
            {
                if(scrollbar_target)
                {
                    float v = scrollbar_target.value;
                    float v_lerped = Mathf.Lerp(fov_min, fov_max, v);
                    tmp.SetText( ((int)(v_lerped)).ToString());
                }
                break;
            }
        }
    }

}
