using UnityEngine;
using System.Collections;

public class FollowingCamera : MonoBehaviour
{
    const float hoverSpeed = 20f;
    
    static FollowingCamera instance;
    
    public static FollowingCamera Singleton()
    {
        if(instance == null)
            instance = FindObjectOfType<FollowingCamera>();
            
        return instance;
    }
    
    public Camera GUI3d_cam;
    
    
    float lerpTargetRotationYStart;
    float lerpTimeStarted, lerpTimeEnd;
    
    
    public void SetCameraEulerY(float x)
    {
        lerpTimeStarted = UberManager.TimeSinceStart();
        lerpTargetRotationYStart = cameraTargetRotationY;
        lerpTimeEnd = lerpTimeStarted + Math.Abs(cameraTargetRotationY - x) / orbitAngleSpeed;
        cameraTargetRotationY = x;
    }
    
    void Start()
    {
    }
        
    [HideInInspector]
    public Transform cameraTransform;
    [HideInInspector]
    public Camera cam;
    
    
    
    Transform thisTransform;
    Transform target;
    PlayerController pc;
    float hoverMouseScrollSens = 12;
    public GameObject fpsCamChild;
    
    void Awake()
    {
        if(instance != null && instance.GetInstanceID() != this.GetInstanceID())
        {
            this.enabled = false;
            Destroy(this.gameObject);
            InGameConsole.Log("<color=yellow>Destroying excess <color=red>Following camera</color></color>");
        }
        else
        {
            cameraTransform = thisTransform = transform;
            cam = GetComponent<Camera>();
            
            if(fpsCamChild)
            {
                fpsCamChild.SetActive(true);
            }
            
            lookPosition = thisTransform.localPosition + distance * thisTransform.forward;
            
            cameraMovementMaskInt = cameraMovementMask.value;
            
            // DontDestroyOnLoad(this.gameObject);
        }
        
        cameraMovementColliderMask = LayerMask.GetMask("Ground");
    }
    
    // PlayerController fpsPlayer;
    
    // public void AttachFPSCamera(PlayerController _fpsPlayer)
    // {
    //     fpsPlayer = _fpsPlayer;
    // }
    
    // public void ReleaseFPSCamera()
    // {
    //     FollowingCamera.Singleton().transform.SetParent(null);
    //     FollowingCamera.Singleton().transform.localPosition  = new Vector3(0, 8, 0);
    //     FollowingCamera.Singleton().transform.localRotation  = Quaternion.Euler(45f, 0, 0);
    //     if(pc.fpsCam)
        
    // }
    
    void Hover(Vector2 input)
    {
        Vector3 motion = new Vector3(input.x, 0f, input.y);
        
        float mouseScroll = Input.GetAxisRaw("Mouse ScrollWheel");
        if(mouseScroll != 0f && !Inputs.IsCursorOverUI())
        {
            motion.y = -mouseScroll * hoverMouseScrollSens;
        }
        
        thisTransform.Translate(motion * hoverSpeed * UberManager.DeltaTime(), Space.World);
    }
    
    [Header("Shaking:")]
    float shakeVelocityY;
    float shakeVelocityX, shakeVelocityZ;
    const float snapbackStrengthY = 230f;
    public float snapbackStrengthXZ = 225f;
    public float snapbackSpeedXZ = 35f;
    float shakingRandom = 1.5f;
    public Vector3 shakedOffset;
    
        
    public static void ShakeY(float yVel)
    {
        return;
            
        if(Math.Abs(Singleton().shakeVelocityY) < 13f)
        {
            Singleton().shakeVelocityY = -yVel;
        }
        
        
    }
    
    public void ShakeXZ(float scale = 1f)
    {
        return;
        
        float _scale = scale * shakingRandom;
        shakedOffset.x = Random.Range(-_scale, _scale);
        shakedOffset.z = Random.Range(-_scale, _scale);
    }
    
    void ShakingY()
    {
        float dt = UberManager.DeltaTime();
        
        shakeVelocityY += snapbackStrengthY * dt;
        
        float dY = shakeVelocityY * dt;
        
        if(shakedOffset.y + dY > 0f)
        {
            shakedOffset.y = 0f;
            shakeVelocityY = 0f;
        }
        else
            shakedOffset.y += dY;    
            
        if(shakedOffset.x != 0f)            
            shakedOffset.x = Mathf.MoveTowards(shakedOffset.x, 0f, snapbackSpeedXZ * dt);
        if(shakedOffset.z != 0f)
            shakedOffset.z = Mathf.MoveTowards(shakedOffset.z, 0f, snapbackSpeedXZ * dt);
    }
    
    Vector3 shakeVelocityXZ = new Vector3(0, 0, 0);
    
    const float snapbackAcceleration = 20f;
    
    
    void LateUpdate()
    {
        // if(target == null)
        // {
        //     Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            
        //     if(input.x * input.y != 0f)
        //     {
        //         input.x *= Math.NormalizeFactor;
        //         input.y *= Math.NormalizeFactor;
        //     }
        //     Hover(input);
        // }
        // else
        // {
        //     CheckObscurance();
        //     FovAnimation2();
        //     // if(Input.GetKeyDown(KeyCode.R))
        //     // {
        //     //     ShakeY(shakeYAmount);
        //     // }
            
        //     ShakingY();
        //     FollowTarget();   
            
        // }
        
        
        //FovAnimation();
    }
    public LayerMask obscuranceMask;
    [Header("Distance:")]
    public float distanceMin = 4f;
    public float distanceMax = 31f;
    float distance = 15f;
    float zoomSpeed = 5f;
    public float smoothTime = 0.125f;
    public float maxMoveSpeed = 50f; //50 original
    
    public Vector3 offsetPosition;
    public Vector3 offsetDirection = new Vector3(0f, 10f, -6f);
    public float cameraRotationX = 51f;
    public float orbitSmoothTime = 0.05f;
    public float orbitAngleSpeed = 120f;
    
    FadableObject currentObstacle;
    
    Vector3 lookPosition;
    Vector3 vel;
    float angleVel;
    float userCameraY;
    

    Vector3 lookVel;
    public float cameraTargetRotationY = 0f;
    public float smoothedRotationY;
    
    static Vector3 centerViewportPoint = new Vector3(0.5f, 0.5f, 0);
    public bool isObscured;
    [SerializeField] bool isAiming;
    public float dirChangeSpeed = 1;
    
    bool IsCameraObscured(out RaycastHit hit, float rayLength, int layerMask)
    {
        return (Physics.Raycast(cam.ViewportPointToRay(centerViewportPoint), out hit, rayLength, layerMask));
    }
    
    void CheckObscurance()
    {
        RaycastHit hit;
        
        isObscured = IsCameraObscured(out hit, distance, obscuranceMask);
            
        if(isObscured)
        {
            FadableObject fadable = hit.collider.GetComponent<FadableObject>();
            if(fadable != null)
            {
                fadable.shouldFade = true;
                currentObstacle = fadable;
            }
        }
        else
        {
            if(currentObstacle != null)
            {
                currentObstacle.shouldFade = false;
                currentObstacle = null;
            }            
        }
    }
    
    public float offsetScale = 10f;
    public LayerMask cameraMovementMask;
    int cameraMovementMaskInt;
    public float zoomOffsetRatio = 1.35f;
    public float normalOffsetRatio = 0.11f;
    
    public Vector3 debugViewportPoint;
    
    
    readonly static Vector3 vZero = Vector3.zero;
    
    
    [Header("Fancy effects:")]
    bool animateFov = false;
    public float minFov    = 55f;
    public float maxFov    = 57f;
    public float freq      = 0.1f;
    bool positiveDir = true;
    
    const float fovAnimSpeedFast = 90f;
    const float fovAnimSpeedSlow = 40f;
    const float normalFov = 55f;
    const float fastFov = 65f;
    float targetFov = 55f;
    
    float animationTimer = 0f;
    
    // public static void ShakeY(float yVelocity)
    // {
    //     Singleton().shaker.ShakeY(yVelocity);
    // }
    
    
    float fovDumpVel;
    
    void FovAnimation2()
    {
        if(pc)
        {
            // if(Math.Abs(pc.velocity.y) > 1 || Math.Abs(pc.externalVelocity.x) > 1 || Math.Abs(pc.externalVelocity.z) > 1)
            if(!pc.IsGrounded())// || pc.aliveState == PlayerAliveState.Dashing)
            {
                // if(!Mathf.Approximately(targetFov, fastFov))
                {
                    targetFov = fastFov;
                }
            }
            else
            {
                // if(!Mathf.Approximately(targetFov, normalFov))
                {
                    targetFov = normalFov;
                }
            }
        }
        else
        {
            targetFov = normalFov;    
        }
        
        float dt = UberManager.DeltaTime();
        float currentFov = cam.fieldOfView;
        
        if(targetFov > currentFov)
        {
            //currentFov = Mathf.MoveTowards(currentFov, targetFov, fovAnimSpeedSlow * dt);
            currentFov = Mathf.SmoothDamp(currentFov, targetFov, ref fovDumpVel, 0.25f);
        }
        else
        {
            // currentFov = Mathf.MoveTowards(currentFov, targetFov, fovAnimSpeedSlow * dt);
            currentFov = Mathf.SmoothDamp(currentFov, targetFov, ref fovDumpVel, 0.25f);
        }
        
        
        cam.fieldOfView = currentFov;
    }
    
    void FovAnimation()
    {
        
        animationTimer += UberManager.DeltaTime() * freq;
        
        if(animationTimer >= 1f)
        {
            animationTimer -= 1f;
            positiveDir = !positiveDir;
        }
        
        if(animateFov)
        {
            float t = animationTimer;
            
            if(positiveDir)
            {
                cam.fieldOfView = Mathf.SmoothStep(minFov, maxFov, t);
            }
            else
            {
                cam.fieldOfView = Mathf.SmoothStep(maxFov, minFov, t);
            }
        }
    }
    
    void RotateDebugByInput()
    {
        if(Input.GetKeyDown(KeyCode.V))
        {
            userCameraY += 45f;
            if(userCameraY > 360f)
            {
                userCameraY -= 360f;
            }
        }
        
        if(Input.GetKeyDown(KeyCode.B))
        {
            userCameraY -= 45f;
            if(userCameraY < -360f)
            {
                userCameraY -= -360f;
            }
        }
    }
    
    public int cameraMovementColliderMask = -1;
    
    bool lookFurther = false;
    
    void FollowTarget()
    {
        float mouseScroll = Input.GetAxisRaw("Mouse ScrollWheel");
        if(!Inputs.IsCursorOverUI())
        {
            distance += -mouseScroll * zoomSpeed;
            Math.Clamp(distanceMin, distanceMax, ref distance);
        }
        
        float dt = UberManager.DeltaTime();
        
        // RotateDebugByInput();
        
        float smoothTimeMultiplier = 1f;
        float moveSpeedMultiplier = 1f;
        
        if(Math.Abs((smoothedRotationY - userCameraY) - cameraTargetRotationY) > 1.0f)
        {
            smoothTimeMultiplier = 0f;
            moveSpeedMultiplier = 1000f;
        }
        
        smoothedRotationY = Mathf.SmoothDampAngle(smoothedRotationY, cameraTargetRotationY + userCameraY, ref angleVel, orbitSmoothTime, orbitAngleSpeed, dt);
        
        Quaternion quatSmoothedRotationY = Quaternion.Euler(0f, smoothedRotationY, 0f);
        
        Vector3 rotatedOffsetDirection = quatSmoothedRotationY  * offsetDirection.normalized;
        
        offsetPosition.x = offsetPosition.y = offsetPosition.z = 0f;
        
        
        
        if(Inputs.GetKeyDown(KeyCode.LeftShift))
        {
            lookFurther = !lookFurther;
        }
        
        // if(Inputs.LeftShift())
        float _zoomOffsetRatio = normalOffsetRatio;
        
        if(lookFurther)
        {
            _zoomOffsetRatio = zoomOffsetRatio;
            // Vector3 viewPortPoint = cam.ScreenToViewportPoint(Inputs.MousePosition());
            // viewPortPoint.z = viewPortPoint.y;
            // viewPortPoint.y = 0f;
            
            // viewPortPoint.x -= 0.5f;
            // viewPortPoint.z -= 0.5f;         
            // debugViewportPoint = viewPortPoint;
            
            // if(viewPortPoint.x * viewPortPoint.x > 1f || viewPortPoint.z * viewPortPoint.z > 1f)
            // {
            //     viewPortPoint.Normalize();
            // }
            
            // float zoomedOffsetScale = offsetScale * Mathf.InverseLerp(distanceMin, distanceMax, distance) * zoomOffsetRatio;
            // if(zoomedOffsetScale < 3f)
            //     zoomedOffsetScale = 3f;
            // offsetPosition = viewPortPoint * zoomedOffsetScale;
        }
        
        Vector3 viewPortPoint = cam.ScreenToViewportPoint(Inputs.MousePosition());
        viewPortPoint.z = viewPortPoint.y;
        viewPortPoint.y = 0f;
        
        viewPortPoint.x -= 0.5f;
        viewPortPoint.z -= 0.5f;         
        debugViewportPoint = viewPortPoint;
        
        if(viewPortPoint.x * viewPortPoint.x > 1f || viewPortPoint.z * viewPortPoint.z > 1f)
        {
            viewPortPoint.Normalize();
        }
        
        float zoomedOffsetScale = offsetScale * Mathf.InverseLerp(distanceMin, distanceMax, distance) * _zoomOffsetRatio;
        if(zoomedOffsetScale < 3f)
            zoomedOffsetScale = 3f;
        offsetPosition = viewPortPoint * zoomedOffsetScale;
        
        
        Vector3 targetPos = quatSmoothedRotationY * offsetPosition + target.position + distance * rotatedOffsetDirection;
        
        
        
        Vector3 calculatedPosition = Vector3.SmoothDamp(thisTransform.localPosition, targetPos, ref vel, smoothTime * smoothTimeMultiplier, maxMoveSpeed * moveSpeedMultiplier, dt);
        
        Vector3 newPosition = calculatedPosition + shakedOffset;
        Vector3 dV = newPosition - thisTransform.localPosition;
        
        Vector3 dDir = Math.Normalized(dV);
        float dDistance = Math.Magnitude(dV);
        
        thisTransform.localPosition = newPosition;
        // Color col = Color.yellow;
        // RaycastHit hit;
        // Ray ray = new Ray(thisTransform.localPosition, dDir);
        
        // if(Physics.Raycast(ray.origin, ray.direction, out hit, 0.25f, cameraMovementColliderMask))
        // {
        //     //newPosition = hit.point;
        //     col = Color.red;
        // }
        // else
        // {
        //     thisTransform.localPosition = newPosition;
            
        // }
        
        // Debug.DrawRay(ray.origin, ray.direction, col * 0.25f);
        
        
        
        
        // if(GameSettings.Singleton().shakeCamera)
        // {
        //     shaker.Tick(calculatedPosition);
        // }
         
        
        Vector3 localRotation = new Vector3(cameraRotationX, smoothedRotationY, 0f);
        thisTransform.localRotation = Quaternion.Euler(localRotation);
    }
    
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(lookPosition, 1f);
    }
#endif
}



