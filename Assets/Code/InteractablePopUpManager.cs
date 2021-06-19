using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InteractablePopUpManager : MonoBehaviour
{
    #region  Singleton
    static InteractablePopUpManager _instance;
    public static InteractablePopUpManager singleton{
        get{
            if(_instance == null)
                _instance = FindObjectOfType<InteractablePopUpManager>();
            return _instance;
        }
    }
    #endregion

    public GameObject interactable_label;
    TextMeshPro tmp;

    public float interactionDistance = 2f;

    List<InteractableByPlayer> interactable_entities = new List<InteractableByPlayer>();

    //The local player is target to check distance to with interactables - 20.08.2019
    public Transform localPlayerTransform;

    public static void SetLocalPlayer(Transform localPlayer)
    {
        singleton.localPlayerTransform = localPlayer;
    }

    public GameObject inspected_item;

    //HACK: this is to prevent RPC spam - 20.08.2019:
    float timeLastInteraction = 0f;
    const  float interactionRpcMinDelay = 0.1f;

    void Awake()
    {
        tmp = interactable_label.GetComponent<TextMeshPro>();
        
    }

    void Update()
    {
        if(localPlayerTransform != null)
        {
            int i = 0;
            Vector3 interactablePoint;
            
            for( ; i < interactable_entities.Count; i++)
            {
                interactable_entities[i].GetPointOfInteractable(out interactablePoint);
                
                if(Vector3.SqrMagnitude(interactablePoint - localPlayerTransform.position) <= interactionDistance * interactionDistance)
                {
                    if(interactable_label.activeSelf == false || interactable_label.transform.position != interactable_entities[i].pivotForLabel.position)
                    {
                        interactable_label.SetActive(true);
                        interactable_label.transform.position = interactable_entities[i].pivotForLabel.position;
                        
                        Vector3 dirToCamera = interactable_entities[i].pivotForLabel.position - FollowingCamera.Singleton().transform.position; 
                        dirToCamera.y = 0f;
                        dirToCamera.Normalize();
                        
                        interactable_label.transform.rotation = Quaternion.LookRotation(dirToCamera, Vector3.up);
                    }
                    inspected_item = interactable_entities[i].gameObject;

                    if(Input.GetKeyDown(KeyCode.E))
                    {
                    
                        //We are preventing RPC spam:
                        if(Time.time - timeLastInteraction > interactionRpcMinDelay)
                        {
                            timeLastInteraction = Time.time;
                            NetworkObjectsManager.InteractWithNetObject(inspected_item);
                        }
                    }

                    break;
                }
            }
            if(i == interactable_entities.Count)
            {
                inspected_item = null;
                interactable_label.SetActive(false);
            }
        }
    }

    public static void RegisterInteractable(InteractableByPlayer entity)
    {
        if(singleton.interactable_entities.Contains(entity))
            Debug.LogError("interactable_entities already contains " + entity.gameObject.name);
        singleton.interactable_entities.Add(entity);
    }

    public static void UnregisterInteractable(InteractableByPlayer entity)
    {
        if(entity != null)
        {
            //TODO: This is maybe an overkill as checking
            if(singleton != null && singleton.interactable_entities != null)
            {
                if(singleton.interactable_entities.Contains(entity))
                {
                    singleton.interactable_entities.Remove(entity);
                }
            }
        }
    }

}
