using UnityEngine;

public class Interactable_ps_fancy1 : MonoBehaviour, IPooledObject
{
    
    ParticleSystem ps;
    
    
    Vector3 destination;
    
    float speed = 1;
    
    void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        
    }
    
    public void InitialState()
    {
        ps.Stop();
        ps.Clear();
        speed = 1;
    }
    
    public void SetDestination(Vector3 startPos, Vector3 dest, float _speed)
    {
        ps.Stop();
        ps.Clear();
        
        transform.position = startPos;
        destination = dest;
        speed = _speed;
        ps.Play();
        
    }
    
    void Update()
    {
        float dt = UberManager.DeltaTime();
        transform.localPosition = Vector3.MoveTowards(transform.localPosition, destination, speed * dt);
    }
}
