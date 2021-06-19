using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootSphere : MonoBehaviour
{
    FootStepPlayer footStepPlayer;

    static LayerMask groundMask;
    static bool deleteRendererOnStart = true;
    public bool isTouching = false;

    void Start()
    {
        groundMask = LayerMask.GetMask("Ground");
        footStepPlayer = GetComponentInParent<FootStepPlayer>();

        if(deleteRendererOnStart)
        {
            MeshRenderer rend = GetComponent<MeshRenderer>();
            if(rend != null)
                Destroy(rend);
        }
    }

    void Update()
    {
        Ray ray = new Ray(transform.position, Vector3.down);

        Color rayColor = Color.red;
        
        if(Physics.Raycast(ray, footStepPlayer.footStepRayLength, groundMask))
        {
            rayColor = Color.yellow;
            if(!isTouching)
            {
                footStepPlayer.PlayFootStep();
                isTouching = true;
#if UNITY_EDITOR
                if(footStepPlayer.isPausing)
                {
                    UnityEditor.EditorApplication.isPaused = true;
                }
#endif
            }
        }
        else
        {
            if(isTouching)
            {
#if UNITY_EDITOR
                if(footStepPlayer.isPausing)
                {
                    UnityEditor.EditorApplication.isPaused = true;
                }
#endif
            }
            isTouching = false;
        }

        Debug.DrawRay(ray.origin, ray.direction * footStepPlayer.footStepRayLength, rayColor, 0f);
    }

    // void OnCollisionEnter(Collision collision)
    // {
    //     Debug.Log("<color=orange>Should play footStep</color>");
    //     footStepPlayer.PlayFootStep();
    // }
}
