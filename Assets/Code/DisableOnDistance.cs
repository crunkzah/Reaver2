using UnityEngine;

public class DisableOnDistance : MonoBehaviour
{
    public float distanceToPlayer = 42;
    
    bool isEnabled = true;
    
    Vector3 thisPosition;
    
    void Start()
    {
        thisPosition = transform.position;
    }
    
    public GameObject[] holders;
    
    void EnableHolder()
    {
        if(holders != null)
        {
            int len = holders.Length;
            for(int i = 0; i < len; i++)
            {
                holders[i].SetActive(true);
            }
        }
        
        isEnabled = true;
    }
    
    void DisableHolder()
    {
        if(holders != null)
        {
            int len = holders.Length;
            for(int i = 0; i < len; i++)
            {
                holders[i].SetActive(false);
            }
        }
        
        isEnabled = false;
    }
    
    
    void Update()
    {
        PlayerController local_pc = PhotonManager.GetLocalPlayer();
        if(local_pc)
        {
            if(Math.SqrDistance(local_pc.GetGroundPosition(), thisPosition) < distanceToPlayer * distanceToPlayer)
            {
                if(!isEnabled)
                {
                    EnableHolder();
                }
            }
            else
            {
                if(isEnabled)
                {
                    DisableHolder();
                }
            }
        }
        else
        {
            if(!isEnabled)
                {
                    EnableHolder();
                }
        }
    }
}
