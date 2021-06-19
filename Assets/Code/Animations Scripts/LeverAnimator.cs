using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class LeverAnimator : MonoBehaviour, Interactable
{
    public bool interactable_only_once = false;
    public bool interactable_by_player = true;

    public AudioClip clip;
    AudioSource source;

    bool isWorking = false;

    public float angleLimit = 40f;
    public float timeToLever = 0.5f;

    float angleSpeed;
    public bool isOn = false;
    Quaternion rotation_on, rotation_off, targetRotation;

    public MonoBehaviour[] interactables_to_call;
    
    void Start()
    {
        source = GetComponent<AudioSource>();
        angleSpeed = angleLimit / timeToLever;



        rotation_off = transform.rotation;
        transform.Rotate(Vector3.up * angleLimit, Space.Self);
        rotation_on = transform.rotation;

        // if lever is on initially, we swap target rotations, clever hack - 06.08.2019
        if(isOn)
        {
            Quaternion q = rotation_on;
            rotation_on = rotation_off;
            rotation_off = q;
        }
        transform.rotation = rotation_off;
    }

    //[PunRPC]
    public void Interact()
    {
        if(interactable_by_player)
        {
            if(interactable_only_once)
                interactable_by_player = false;
            SwitchLever();
        }
    }  

    void SwitchLever()
    {
        isWorking = true;
        targetRotation = isOn ? rotation_off : rotation_on;
        OnWorkBegin();

    }

    void OnWorkBegin()
    {
        source.PlayOneShot(clip);
    }


    void Update()
    {
        // if(Input.GetKeyDown(KeyCode.E))
        //     Interact();
        if(isWorking == false)
            return;
        
        if(transform.rotation == targetRotation)
        {
            isWorking = false;
            isOn = !isOn;
            OnWorkFinished();
            return;
        }

        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, angleSpeed * Time.deltaTime);
        
    }


    //[PunRPC]
    void OnWorkFinished()
    {
//        print("OnWorkFinished()");
         if(interactables_to_call != null)
        {
            for(int i = 0; i < interactables_to_call.Length; i++)
            {
                interactables_to_call[i].SendMessage("Interact", SendMessageOptions.RequireReceiver);
            }
        }
    }
    
}
