using UnityEngine;

public class FPSFoostepPlayer : MonoBehaviour
{
    public AudioClip step1;
    public PlayerController pController;
    
    Transform thisTransform;
    Vector3 prevPos;
    AudioSource audioSrc;
    
    void Awake()
    {
        thisTransform = transform;
        audioSrc = GetComponent<AudioSource>();
        if(pController == null)
            pController = GetComponent<PlayerController>();
    }
    
    public float distanceTravelled;
    float stepDistance = 2.65f * 2;
    public float timerOut = 0.5f;
    public float timer = 0;
    
    float basePitch = 0.4f;
    float pitchRange = 0.1f;
    public float stepShakeY = 3f;
    
    void Update()
    {
        if(pController.IsGrounded() && !pController.isSliding)
        {
            distanceTravelled += Vector3.Distance(prevPos, Math.GetXZ(thisTransform.position));
            
            if(distanceTravelled > stepDistance)
            {
                MakeStep();
                distanceTravelled = 0;
                // sqrDistanceTravelled -= stepDistance * stepDistance;
            }
            
            prevPos = Math.GetXZ(thisTransform.position);
        }
        else
        {
            distanceTravelled = 0;
        }
    }
    
    void MakeStep()
    {
        // InGameConsole.LogFancy("MakeStep()");
        CameraShaker.ShakeY(stepShakeY);
        audioSrc.pitch = Random.Range(basePitch - pitchRange, basePitch + pitchRange);
        audioSrc.PlayOneShot(step1, 1f);
    }
}
