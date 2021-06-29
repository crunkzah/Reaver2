using UnityEngine;

public class Sonar : MonoBehaviour
{
    public AudioClip clip;
    public float freq = 2;
    
    float timer;
    
    void Update()
    {
        float dt = UberManager.DeltaTime();
        timer += dt;
        if(timer >= freq)
        {
            timer -= freq;
            Ping();
        }
    }
    
    public AudioSource audio_src;
    
    void Ping()
    {
        audio_src.PlayOneShot(clip);
        DoLight(audio_src.transform.position);
    }
    
    void DoLight(Vector3 pos)
    {
        GameObject g = ObjectPool2.s().Get(ObjectPoolKey.LightPooled, false);
        LightPooled light = g.GetComponent<LightPooled>();
        //Color color = Random.ColorHSV();
        Color color = new Color(1, 1f, 0.0f, 1);
        float decay_speed = 16;
        float radius = 24;
        
        light.DoLight(pos, color, 5f, 4, radius, decay_speed);
    }
}
