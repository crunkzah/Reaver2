using UnityEngine;

public class ChainController : MonoBehaviour
{
    public float speed = 2;
    public float height = 6;
    public float audio_mult = 2;
    public float min_speed = 0.2f;
    
    Transform thisTransform;
    
    void Awake()
    {
        Init();
    }
    
    Transform[] chainLinks;
    
    void Init()
    {
        thisTransform = transform;
        
        int childCount = thisTransform.childCount;
        
        chainLinks = new Transform[childCount];
        
        for(int i = 0; i < childCount; i++)    
        {
            chainLinks[i] = thisTransform.GetChild(i);
        }
    }
    
    
    
    void Update()
    {
        float dt = UberManager.DeltaTime();    
        
        float currentVolume = AudioManager.GetCurrentVolume();
        
        int len = chainLinks.Length;
        for(int i = 0; i < len; i++)
        {
            Vector3 v = chainLinks[i].localPosition;
            float _speed = min_speed + currentVolume * audio_mult;
            v.y += _speed * dt;
            //v.y += speed * dt;
            
            if(v.y > height)
            {
                v.y = v.y - height;
//                chainLinks[i].localRotation = Quaternion.Euler(0, Random.Range(-15F, 15F), 0);
            }
            
            chainLinks[i].localPosition = v;
        }
    }
}
