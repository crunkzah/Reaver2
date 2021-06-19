using UnityEngine;

public class PlatformEffect1 : MonoBehaviour
{
    public bool is_player_standing_on = false;
    // public Transform thisTransform;
    public Vector3 world_static_pos;
    
    public ParticleSystem attached_ps;
    
    void Awake()
    {
        world_static_pos = transform.position;
        // thisTransform = transform;
    }
    
}
