using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HarmonicDoorState
{
    Closed,
    Opening,
    Open,
    Closing
}

public class HarmonicDoor : MonoBehaviour, INetworkObject
{
    HarmonicDoor_inner[] inners;
    void Awake()
    {
        net_comp = GetComponent<NetworkObject>();
        inners = GetComponentsInChildren<HarmonicDoor_inner>();
        
        for(int i = 0; i < inners.Length; i++)
        {
            inners[i].harmonicDoor = this;
            inners[i].transform.localPosition = new Vector3(0, 0, 0);
        }
        
        if(audioSource)
        {
            audioSource.playOnAwake = false;
            audioSource.Stop();
        }
    }
    
    public void ReceiveCommand(NetworkCommand command, params object[] args)
    {
        switch(command)
        {
            case NetworkCommand.OpenGates:
            {
                Open();
                break;
            }
            default:
            {
                break;
            }
        }
    }
    
    NetworkObject net_comp;
    
    int currentSegment = 0; 
    
    
    
    void Update()
    {
        // if(Input.GetKeyDown(KeyCode.Y))
        // {
        //     Animate();
        // }
    }
    
    public AudioSource audioSource;
    
    public void OnSegmentOpened()
    {
        //InGameConsole.LogFancy("OnSegmentOpened()!");
        currentInnerIndex++;
        if(currentInnerIndex < inners.Length)
        {
            inners[currentInnerIndex].Play();
        }
        
        if(audioSource)
        {
            audioSource.pitch += 0.1f;
            audioSource.Play();            
        }
    }
    
    
    int currentInnerIndex = 0;
    
    bool isOpen = false;
    
    public void Open()
    {
        if(isOpen)
            return;
        isOpen = true;
        Animate();
    }
    
    void Animate()
    {
        audioSource.pitch = 0.8f;
        currentInnerIndex = 0;
        // InGameConsole.LogFancy("Animate()!");
        inners[0].Play();
        
        
        // for(int i = 0; i < inners.Length; i++)
        // {
        //     inners[i].Play();
        //     InGameConsole.LogFancy("Animate " + i);
        // }
    }
    
    
}
