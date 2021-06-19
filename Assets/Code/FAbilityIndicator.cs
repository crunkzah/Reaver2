using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FAbilityIndicator : MonoBehaviour
{
    void Awake()
    {
    }
    
    public void Hide()
    {
    }
    
    public void Show()
    {
    }
    
    FPSGunController fpsGunController;
    
    public void SetTarget(FPSGunController _t)
    {
        fpsGunController = _t;
    }
    
    
    void Update()
    {
        return;
        
        if(fpsGunController == null)
        {
            Hide();
        }
        else
        {
            switch(fpsGunController.currentArm)
            {
                case(ArmType.None):
                {
                    Hide();
                    
                    break;
                }
                case(ArmType.Arm1):
                {
                    
                    break;
                }
                case(ArmType.Sunstrike):
                {
                    
                    break;
                }
            }
        }
    }
    
    
}