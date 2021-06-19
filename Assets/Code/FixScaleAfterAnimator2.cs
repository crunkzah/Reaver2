using UnityEngine;

public class FixScaleAfterAnimator2 : MonoBehaviour
{
    public Transform root;
    public Transform smr_transform;
    
    
    public Vector3 true_scale = new Vector3(1, 1, 1);
    float currentScale;
    bool firstTick = true;
    
    
    void LateUpdate()
    {
        root.localScale = new Vector3(true_scale.x, true_scale.y, true_scale.z);
        smr_transform.localScale = new Vector3(true_scale.x, true_scale.y, true_scale.z);
    }
}
