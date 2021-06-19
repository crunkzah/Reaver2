using UnityEngine;


//This component only serves one purpose - to pop up the LABEL
//And then InteractablePopUpManager tells attached NetworkObject to interact.  - 21.08.2019
[RequireComponent(typeof(NetworkObject))]
public class InteractableByPlayer : MonoBehaviour
{

    public Transform pivotForLabel;
    
    public bool usesPivotForLabelAsPoint = false;
    
    public void GetPointOfInteractable(out Vector3 position)
    {
        if(usesPivotForLabelAsPoint)
        {
            position = pivotForLabel.position;
        }
        else
        {
            position = transform.position;
        }
    }
    
    
    #if UNITY_EDITOR
    void Start()
    {
        if(pivotForLabel == null)
        {
            Debug.LogError("pivotForLabel is not set for " + this.gameObject.name);
        }
    }
    #endif

    void OnEnable()
    {
        InteractablePopUpManager.RegisterInteractable(this);
    }

    void OnDisable()
    {
        InteractablePopUpManager.UnregisterInteractable(this);
    }


    // Not sure if this is needed, 
    //   because it throws error sometimes
    //   when closing the game(editor) - 22.08.2019
    // void OnDisable()
    // {
    //     InteractablePopUpManager.UnregisterInteractable(this);
    // }


    #if UNITY_EDITOR
    void OnDrawGizmos()
    {
        GUIStyle style = new GUIStyle();
        style.alignment = TextAnchor.MiddleCenter;
        style.normal.textColor = Color.yellow;
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.75f, this.gameObject.name +  "\nInteractable\nby player\n", style);
    }
    #endif
}
