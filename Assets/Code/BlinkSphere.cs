using UnityEngine;

public class BlinkSphere : MonoBehaviour
{
    Transform thisTransform;
    
    static int ricochetMask = -1;
    
    float force = 24;
    Vector3 velocity = new Vector3(0, 0, 0);
    const float gravity = -15.0F;
    
    const float time_to_be_alive = 3f;
    float timer = 0;
    
    public bool isWorking = false;
    
    public AudioSource audioSrc;
    
    public ParticleSystem ps;
    
    float blipTimer = 0;
    const float blipFreq  = 0.15F;
    
    void Awake()
    {
        
        thisTransform = transform;
        if(ricochetMask == -1)
        {
            ricochetMask = LayerMask.GetMask("Ground", "Ceiling");
        }
    }
    
    public void Launch(Vector3 pos, Vector3 dir)
    {
        velocity = dir * force;
        timer = 0;
        blipTimer = 0;
        isWorking = true;
        audioSrc.pitch = 1f;
    }
    
    public Vector3 GetBlinkPosition()
    {
        return thisTransform.localPosition + new Vector3(0, 0.15f, 0);
    }
    
    void CheckCollision(float dt)
    {
        RaycastHit hit;
        Ray ray = new Ray(thisTransform.localPosition, Math.Normalized(velocity));
        
        if(Physics.Raycast(ray, out hit, Math.Magnitude(velocity) * dt, ricochetMask))
        {
            velocity = Math.Magnitude(velocity) * 0.8f * Vector3.Reflect(Math.Normalized(velocity), hit.normal);
            OnReflect();
        }
    }
    
    void OnReflect()
    {
        ps.Play();
        //InGameConsole.LogFancy("OnReflect()");
        AudioManager.Play3D(SoundType.ricochet1, thisTransform.localPosition, Random.Range(0.9f, 1.1f), 0.6f, 5);
    }
    
    void Blip()
    {
        audioSrc.PlayOneShot(audioSrc.clip, 0.33f);
    }
    
    void Update()
    {
        if(!isWorking)
        {
            return;
        }
        
        if(blipTimer > blipFreq)
        {
            blipTimer -= blipFreq;
            Blip();
        }
        
        if(timer > time_to_be_alive)
        {
            EndLifeFromTime();
        }
        else
        {
            float dt = UberManager.DeltaTime();
            CheckCollision(dt);
            
            blipTimer += dt;
            timer += dt;
            velocity.y += gravity * dt;
            velocity.y = Mathf.Clamp(velocity.y, -50, 50);
            Vector3 updatedPos = thisTransform.localPosition + velocity * dt;
            thisTransform.localPosition = updatedPos;
        }
    }
    
    
    public void EndLifeFromTime()
    {
        audioSrc.PlayOneShot(audioSrc.clip, 0.5f);
        audioSrc.pitch = 0.5f;
        
        isWorking = false;
        
        //InGameConsole.LogFancy("EndLife()");
        GetComponent<Renderer>().enabled = false;
        Destroy(this.gameObject, 1.5f);   
        ps.Play();    
    }
    
    public void EndLife()
    {
        isWorking = false;
        
        //InGameConsole.LogFancy("EndLife()");
        GetComponent<Renderer>().enabled = false;
        Destroy(this.gameObject, 1.5f);   
        ps.Play();         
    }
}
