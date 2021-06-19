using UnityEngine;

public class FixScaleAfterAnimator : MonoBehaviour
{
    public Transform root;
    public Transform smr_transform;
    
    
    public float true_scale = 1;
    float currentScale;
    bool firstTick = true;
    
    
    void LateUpdate()
    {
        root.localScale = new Vector3(true_scale, true_scale, true_scale);
        smr_transform.localScale = new Vector3(true_scale, true_scale, true_scale);
    }
}
