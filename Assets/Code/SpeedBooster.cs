using UnityEngine;

public class SpeedBooster : MonoBehaviour
{
    PlayerController local_pc;
    
    public float distance = 0.5f;
    public Transform sensor;
    
    Vector3 sensor_pos;
    
    void Awake()
    {
        sensor_pos = sensor.position;
    }
    
    float timer;
    const float boost_cd = 0.5F;
    public float mult = 2;
    public bool limitVel = false;
    public float magnitudeLimit = 40;
    
    [Header("New settings:")]
    public float force = 23;
    
    void Update()
    {
        float dt = UberManager.DeltaTime();
        
        if(timer > 0)
        {
            timer -= dt;
            if(timer <= 0)
            {
                timer = 0;
            }
        }
        
        if(timer == 0 && PhotonManager.Singleton() != null)
        {
            local_pc = PhotonManager.GetLocalPlayer();
            
            if(local_pc)
            {
                if(Math.SqrDistance(sensor.position, local_pc.GetGroundPosition() + new Vector3(0, 1, 0)) < distance * distance)
                {
                    
                    float dot = Vector3.Dot(transform.up, (local_pc.fpsVelocity).normalized);
                    
                    Vector3 boost_vel = (dot > 0 ? transform.up : -transform.up) * force;
                    
                    local_pc.fpsVelocity = boost_vel;
                    timer = boost_cd;
                    InGameConsole.LogFancy("SpeedBooster boosted velocity!");
                }
            }
        }
    }
}
