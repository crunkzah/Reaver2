using UnityEngine;
using System.Collections;

public class ButtonAnimator01 : MonoBehaviour
{
    public void PressButton()
    {
        if(!isWorking)
        {
            //TODO: Play some sound here
            StartCoroutine(AnimateButtonPress());
            OnButtonPressed();
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
        source = GetComponent<AudioSource>();
    }


    [Header("Debug vars:")]
    public Material offMat, onMat;

    public AudioClip clip;
    AudioSource source;
    
    void OnButtonPressed()
    {
        source.PlayOneShot(clip);
        print("OnButtonPressed");
    }

    IEnumerator AnimateButtonPress()
    {
        GetComponent<Renderer>().material = onMat;
        isWorking = true;
        
        float z;
        float t = 0f;
        
        
        while(t < 1f)
        {
            t += 1f/ANIM_TIME  *  Time.deltaTime;
            if(t > 1f) t = 1f;
            z = restPosition.z +  animCurve.Evaluate(t) * ANIM_SCALE;
            transform.position = new Vector3(transform.position.x, transform.position.y, z);
            yield return null;
        }

        isWorking = false;
        GetComponent<Renderer>().material = offMat;
    }
}
