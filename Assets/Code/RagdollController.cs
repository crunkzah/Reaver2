using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollController : MonoBehaviour
{
    public bool disableAtStart = true;
    [SerializeField] List<Rigidbody> rbs;
    [SerializeField] Rigidbody rootBone;
    //[SerializeField] List<Collider> colliders;

    void Start()
    {
        FindRbsAndColliders();
        if(rootBone == null)
        {
            //Debug.LogError("RootBone not set on " + this.gameObject.name);
            InGameConsole.LogError(string.Format("RootBone not set on {0}", this.gameObject.name));
        }
        if(disableAtStart)
        {
            ToggleRagdoll(true);
        }
    }

    public void FindRbsAndColliders()
    {
        GetComponentsInChildren<Rigidbody>(true, rbs);
        //GetComponentsInChildren<Collider>(true, colliders);
    }


    public void ApplyForceToRootBone(Vector3 force)
    {
        // transform.Translate(Vector3.up * 0.075f, Space.Self);
        rootBone.AddForce(force, ForceMode.Impulse);
    }


    //@Incomplete Animator is still taking control over ragdoll if Animator is not disabled 
    public void ToggleRagdoll(bool value)
    {
        if(rbs != null /*&& colliders != null*/)
        {
            for(int i = 0; i < rbs.Count; i++)
            {
                rbs[i].isKinematic = value;
            }
        }
    }

}
