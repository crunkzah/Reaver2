using UnityEngine;

public class GameSettings : MonoBehaviour {

    static GameSettings _instance;
    public static GameSettings Singleton()
    {
        
        if (_instance == null)
            _instance = FindObjectOfType<GameSettings>();
        
        return _instance;
    }
    

    [Header("Pickupable items:")]
    public LayerMask groundLayer;
    public float distanceToGround = 0.1f;

    [Header("Bullets")]
    public LayerMask bulletCollisionLayerMask;
    public const float bulletLifeTime = 16f;

    [Header("AI Settings:")]
    public bool ai_Disabled = false;
    public float aiRefreshRate = 0.25f;
    public LayerMask enemyTargetMask;
    public LayerMask enemyVisionMask;
    public LayerMask enemyVisionMaskWithoutPlayer;


    [Header("Other:")]
    public LayerMask notifySphereMask;
    public LayerMask itemsOnGroundMask;
    
    public LayerMask jumpPadMask;
    public LayerMask jumpedGroundMask;
    
    [Header("Shaking camera:")]
    public bool shakeCamera = true;
}
