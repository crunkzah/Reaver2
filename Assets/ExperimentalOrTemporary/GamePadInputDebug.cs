using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePadInputDebug : MonoBehaviour
{
    public string[] gamepads;
    public float mouseX;
    public float mouseY;
    
    
    void Update()
    {
        gamepads = Input.GetJoystickNames();
        
        mouseX = Input.GetAxisRaw("Mouse X");
        mouseY = Input.GetAxisRaw("Mouse Y");
    }
}
