using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    Transform target;
    Transform thisTransform;
    
    void Start()
    {
        thisTransform = transform;
    }
    
    void Update()
    {
        if(target == null)
        {
            PlayerController local_player = PhotonManager.GetLocalPlayer();
            if(local_player != null)
            {
                target = local_player.transform;
            }
        }
        else
        {
            thisTransform.localPosition = target.localPosition;
        }
        
    }
    
}
