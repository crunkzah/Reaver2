using UnityEngine;

public class ElevatorCaller : MonoBehaviour, Interactable
{
    public ElevatorAnimator elevator;
    public ButtonAnimator01 button;

    public int floorToCall = 0;

    public void Interact()
    {
        
        
        if(elevator != null)
        {
            elevator.SetFloor(floorToCall);
        }
        if(button != null)
        {
            button.PressButton();
        }
    }
}
