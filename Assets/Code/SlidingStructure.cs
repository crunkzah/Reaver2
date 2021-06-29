using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SlidingStructureState : byte
{
    Hidden,
    SlidedOut
}

public class SlidingStructure : MonoBehaviour
{
    public SlidingStructureState state = SlidingStructureState.Hidden;
    
    Transform thisTransform;    
    
    public Vector3 slidedOutLocalPos;
    public Vector3 hiddenLocalPos;
    
    public float workingSpeed = 5f;
    
    public ParticleSystem ps;
    public AudioSource audio_src;
    
    public float delay = 0.5F;
    
    public bool hideRenderer;
    
    Renderer[] rends;
    
    void SlideOut_Delayed()
    {
        audio_src.Play();
        if(rends != null && rends.Length > 0)
        {   
            for(int i = 0; i < rends.Length; i++)
            {
                rends[i].enabled = true;
            }
        }   
        ps.Play();
        isWorking = true;
    }
    
    public void ToggleSlide()
    {
        if(state == SlidingStructureState.Hidden)
        {
            state = SlidingStructureState.SlidedOut;
            
            Invoke(nameof(SlideOut_Delayed), delay);
            
            //audio_src.Play();
            //ps.Play();
            
        }
        else
        {
            state = SlidingStructureState.Hidden;
        }
        
        // isWorking = true;
    }
    
    void SlideOut()
    {
        
    }
    
    void SlideIn()
    {
        
    }
    
    [SerializeField] bool isWorking = false;
    
    void OnBecomeHidden()
    {
        
    }
    
    void OnBecomeSlidedOut()
    {
        
    }
    
    void Update()
    {
        if(isWorking)
        {
            float dt = UberManager.DeltaTime();
            switch(state)
            {
                case(SlidingStructureState.Hidden):
                {
                    Vector3 updatedPos = Vector3.MoveTowards(thisTransform.localPosition, hiddenLocalPos, dt * workingSpeed);
                    thisTransform.localPosition = updatedPos;
                    if(Math.SqrDistance(thisTransform.localPosition, hiddenLocalPos) < 0.05F * 0.05F)
                    {
                        thisTransform.localPosition = hiddenLocalPos;
                        OnBecomeHidden();
                        isWorking = false;
                    }
                    
                    break;
                }
                case(SlidingStructureState.SlidedOut):
                {
                    Vector3 updatedPos = Vector3.MoveTowards(thisTransform.localPosition, slidedOutLocalPos, dt * workingSpeed);
                    thisTransform.localPosition = updatedPos;
                    if(Math.SqrDistance(thisTransform.localPosition, slidedOutLocalPos) < 0.05F * 0.05F)
                    {
                        thisTransform.localPosition = slidedOutLocalPos;
                        OnBecomeSlidedOut();
                        isWorking = false;
                    }
                    
                    break;
                }
            }        
        }
    }
    
    void Awake()
    {
        if(hideRenderer)
        {
            rends = GetComponentsInChildren<Renderer>();
        }
        thisTransform = transform;
        slidedOutLocalPos =  thisTransform.localPosition;
        
        GoToHiddenState_Silently();
    }
    
    void Start()
    {
        if(state == SlidingStructureState.Hidden)
        {
            GoToHiddenState_Silently();
            if(hideRenderer)
            {
                for(int i = 0; i < rends.Length; i++)
                {
                    rends[i].enabled = false;
                }
            }
        }
    }
    
    void GoToHiddenState_Silently()
    {
        thisTransform.localPosition = hiddenLocalPos;
    }
    
}
