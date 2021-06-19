using UnityEngine;

public class WeaponAnimator : MonoBehaviour
{
    
    
    [Header("Glock:")]
    public Transform GlockShutter;
    public float glockShutterSpeed = 1f;
    public float glockAnimationScale = 0.2f;
    public float glockShutterAmount = 0f;
    
    
    
    
    
    Vector3 originalLocalPosition = new Vector3(0.006308556f, 0.07f, 0.229f);
    
    public void PlayAnimation(GunAnimationCommand animationCommand)
    {
        switch(animationCommand)
        {
            case(GunAnimationCommand.GlockFire):
            {
                //Not clean but it works:
                
                if(GlockShutter != null)
                    GlockShutter.Translate(Vector3.left * glockAnimationScale, Space.Self);
                
                break;
            }
        }
    }
    
    void Update()
    {
        GlockShutter.localPosition = Vector3.MoveTowards(GlockShutter.localPosition, originalLocalPosition, glockShutterSpeed * Time.deltaTime);
    }
}
