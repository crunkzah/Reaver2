using UnityEngine;

public class ButtonInGame : MonoBehaviour
{
    //TODO: Maybe rewrite it to call interface like 'Interactable'
    public GameObject[] objectsToCall;
    public ButtonAnimator01 buttonToAnimate;
    
    void Interact()
    {
        if(objectsToCall != null && objectsToCall.Length > 0)
        {
            for(int i = 0; i < objectsToCall.Length; i++)
            {
                objectsToCall[i].SendMessage("Interact", SendMessageOptions.DontRequireReceiver);
            }
            
            
            buttonToAnimate.PressButton();
        }
        else
        {
            InGameConsole.LogError(string.Format("ButtonInGame calls <color=red>unsuccesful</color>. <color=blue>{0}</color>", this.gameObject.name));
        }
    }
    
}
