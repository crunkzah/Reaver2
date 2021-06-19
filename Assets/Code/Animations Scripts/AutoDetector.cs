using System.Collections.Generic;
using UnityEngine;

public class AutoDetector : MonoBehaviour
{
    public BoxCollider boxColliderBounds;
    public Bounds bounds;

    public bool isDetecting = false;
    Interactable interactable_component;

    public void Detect()
    {
        
        isDetecting = true;
        interactable_component.Interact();
    }

    public void Undetect()
    {
        isDetecting = false;
        interactable_component.Interact();
    }

    void Awake()
    {
        if(boxColliderBounds == null)
            InGameConsole.LogWarning("boxColliderBounds is null on " + this.gameObject.name);
        else
            bounds = boxColliderBounds.bounds;

        interactable_component = GetComponent<Interactable>();

        #if UNITY_EDITOR
        if(interactable_component == null)
            InGameConsole.LogWarning("Interactable_component not found on " + this.gameObject.name);
        #endif
    }

    void OnEnable()
    {
        DetectorBounds.RegisterDetector(this);
    }

    void OnDisable()
    {
        DetectorBounds.UnregisterDetector(this);
    }


}
