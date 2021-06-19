using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Elevator01_ButtonsController : MonoBehaviour, Interactable
{

    public ElevatorAnimator elevator;
    public ButtonAnimator01 buttonUp, buttonDown;


    public  void Interact()
    {
        if(elevator.currentFloor == 3)
        {
            buttonDown.PressButton();
            elevator.SetFloor(0);
            
        }
        else
        {
            buttonUp.PressButton();
            elevator.SetFloor(3);
        }

    }

}
