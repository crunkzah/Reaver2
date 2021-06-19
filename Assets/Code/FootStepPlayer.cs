using UnityEngine;
using Photon.Pun;

public class FootStepPlayer : MonoBehaviour
{
    public float stepDistance = 0.4f;

    
    AudioSource source;

    public  float distanceTravelled = 0f;
    Vector2 oldPosition;

    public float[] clipTimes;
    [Header("Collider based settings:")]
    public AudioClip oneshotClip;
    public bool isPausing = false;
    public float footStepRayLength = 0.2f;
    public float footStepCooldown = 0.1f;
    float timeForNextFootStep;
    
    PhotonView pv;

    void Start()
    {
#if UNITY_EDITOR
        if(source == null)
            Debug.LogWarning("AudioSource for " + this.gameObject.name + " not set");
#endif

        source = GetComponent<AudioSource>();
        oldPosition = new Vector2(transform.position.x, transform.position.z);
        
        pv = GetComponent<PhotonView>();
    }

    int index = 0;

    int incrementIndex()
    {
        if(index + 1 > clipTimes.Length)
            index = 0;
        return index++;
    }

    float lastTimeStamp = 0f;
    
    Vector3 footStepParticleOffset = Vector3.up * 0.25f;
    
    
    bool useParticles = false;

    public void PlayFootStep()
    {
        if(Time.time > timeForNextFootStep)
        {
            if(pv.IsMine)            
            {
                FollowingCamera.ShakeY(Random.Range(3.3f, 4f));
            }
            
            if(useParticles)
            {
                ParticlesManager.Play(1, transform.position + footStepParticleOffset, -this.transform.forward);
            }
            timeForNextFootStep = Time.time + footStepCooldown;
        }

        lastTimeStamp = Time.time;
    }
}
