using UnityEngine;

public class Detectable : MonoBehaviour
{

    void OnEnable()
    {
//        Debug.Log("Registering " + this.gameObject + " as target.");
        DetectorBounds.RegisterTarget(this);
    }

    public Vector3 WorldPos
    {
        get{
            return transform.position; 
        }
    }

    
    void OnDisable()
    {
        DetectorBounds.UnregisterTarget(this);
    }
}
