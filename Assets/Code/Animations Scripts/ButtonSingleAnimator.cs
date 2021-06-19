using UnityEngine;
using System.Collections;


//FIX This is not used! :
public class ButtonSingleAnimator : MonoBehaviour
{
    public void PressButton()
    {
        if(!isWorking)
        {
            //TODO: Play some sound here
            StartCoroutine(AnimateButtonPress());
        }
    }

    

    public AnimationCurve animCurve;
    const float ANIM_SCALE = 0.025F;
    const float ANIM_TIME = 0.2F;
    Vector3 restPosition;

    bool isWorking = false;

    void Start()
    {
        restPosition = transform.position;
    }


    [Header("Debug vars:")]
    public Material offMat, onMat;
    
    IEnumerator AnimateButtonPress()
    {
        GetComponent<Renderer>().material = onMat;
        isWorking = true;
        
        
        float timePassed = 0f;
        
        
        while(timePassed < 1f)
        {
            timePassed += 1f/ANIM_TIME  *  Time.deltaTime;

            yield return null;
        }

        isWorking = false;
        GetComponent<Renderer>().material = offMat;
    }
}
