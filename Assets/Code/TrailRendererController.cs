using UnityEngine;

public class TrailRendererController : MonoBehaviour
{
    TrailRenderer tr;
    
    float timer = 0;
    
    void Awake()
    {
        tr = GetComponent<TrailRenderer>();
        tr.emitting = false;
    }
    
    
    
    public void EmitFor(float time)
    {
        if(timer > 0)
        {
            return;
        }
        timer = time;
    }
    
    void Update()
    {
        float dt = UberManager.DeltaTime();
        
        timer -= dt;
        
        if(timer <= 0)
        {
            timer = 0;
            tr.emitting = false;
        }
        else
        {
            tr.emitting = true;
        }
    }
    
}
