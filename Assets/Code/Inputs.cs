using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Inputs : MonoBehaviour
{
    // Start is called before the first frame update
    
    static Inputs _instance;
    public static Inputs singleton()
    {
        if(_instance == null)
        {
            _instance = FindObjectOfType<Inputs>();
        }
        
        return _instance;
    }
    
    GraphicRaycaster m_Raycaster;
    PointerEventData m_PointerEventData;
    EventSystem m_EventSystem;
    
    void Awake()
    {
        m_Raycaster = FindObjectOfType<GraphicRaycaster>();
        m_EventSystem = FindObjectOfType<EventSystem>();
    }
    
    public static bool GetInteractKeyDown()
    {
        return Input.GetKeyDown(KeyCode.E);
    }
    
    public static Vector3 MousePosition()
    {
        return Input.mousePosition;
    }
    
    public static bool LeftShift()
    {
        return Input.GetKey(KeyCode.LeftShift);
    }
    
    public static bool GetAbilityR_KeyDown()
    {
        return Input.GetKeyDown(KeyCode.R);
    }
    
    public static bool GetAbilityR_Key()
    {
        return Input.GetKey(KeyCode.R);
    }
    
    public static bool AbilityF()
    {
        return Input.GetKey(KeyCode.F);
    }
    
    public static bool GetSlamAndSlide_Key()
    {
        return Input.GetKey(KeyCode.LeftControl);
    }
    
    public static bool GetSlamAndSlide_KeyDown()
    {
        return Input.GetKeyDown(KeyCode.LeftControl);
    }
    
    public static bool SwitchToPrevSlot_KeyDown()
    {
        return Input.GetKeyDown(KeyCode.Q);
    }

    
    public static bool Arm_FKeyDown()
    {
        return Input.GetKeyDown(KeyCode.F);
    }
    
    public static bool AbilityFKeyUp()
    {
        return Input.GetKeyUp(KeyCode.F);
    }
    
    
    
    public static bool SwitchArmKeyDown()
    {
        return Input.GetKeyDown(KeyCode.G);
    }
    
    public static bool GetKeyDown(KeyCode key, bool ignoreConsole = true)
    {
        if(!ignoreConsole)
        {
            if(InGameConsole.singleton.IsActive())
            {
                return false;
            }
        }
        
        return Input.GetKeyDown(key);
    }
    
    public static bool GetKey(KeyCode key, bool ignoreConsole = false)
    {
        if(!ignoreConsole)
        {
            if(InGameConsole.singleton.IsActive())
            {
                return false;
            }
        }
        
        return Input.GetKeyDown(key);
    }
    
    public static bool MouseButtonDown(int mouse_button)
    {
        return Input.GetMouseButtonDown(mouse_button);
    }
    
    public static bool GetJumpKeyDown()
    {
        return Input.GetKeyDown(KeyCode.Space);
    }
    
    public static bool GetJumpKey()
    {
        return Input.GetKey(KeyCode.Space);
    }
    
    public static bool GetDashKeyDown()
    {
        return Input.GetKeyDown(KeyCode.LeftShift);
    }
    
    List<RaycastResult> results = new List<RaycastResult>();
    
    bool _IsCursorOverUI()
    {
        results.Clear();
        
        m_PointerEventData = new PointerEventData(m_EventSystem);
        m_PointerEventData.position = Input.mousePosition;

        if(m_Raycaster)
            m_Raycaster.Raycast(m_PointerEventData, results);
        
        return results.Count > 0;
    }
    
    
    public static bool IsCursorOverUI()
    {
        if(singleton() == null)
            return false;
            
        return singleton()._IsCursorOverUI();
    }
    
}
