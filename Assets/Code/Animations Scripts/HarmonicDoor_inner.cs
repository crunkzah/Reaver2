using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HarmonicDoor_inner : MonoBehaviour
{
    Animation anim;
    
    void Awake()
    {
        anim = GetComponent<Animation>();
    }
    
    public HarmonicDoor harmonicDoor;
    
    public void Play()
    {
        anim.Play();
    }
    
    public void ClipEnded()
    {
        //Debug.Log("ClipEnded() " + this.gameObject.name);
        harmonicDoor.OnSegmentOpened();
    }
    
}
