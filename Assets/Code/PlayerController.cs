using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;


public enum PlayerAliveState : int
{
    Normal,
    Dashing
}

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour, IPunObservable 
{
    public GameObject playerLightFlyingPrefab;
    public PlayerLight playerLight;
    
    public GameObject playerBlobPrefab;
    
    
    [Header("Movement:")]
    const float fpsJumpForceSecond = 16F;
    
    
    
    public Vector3 velocity;
    public bool isAlive = false;

    public LayerMask mouseRayMask;

    public CharacterController controller;
    const float MIN_GRAVITY = -10F;//-0.57F;
    Camera playerCamera;
    Transform camera_transform;
    
    static readonly Vector3 PLAYER_GRAVITY = new Vector3(0, -13F, 0);
    
    Transform thisTransform;
    public SkinnedMeshRenderer player_renderer;
        
    AudioSource playerAudioSource;
    
    public PlayerAliveState aliveState = PlayerAliveState.Normal;

    public PhotonView pv;
    
    public Material hurtMaterial;
    Material mainMaterial;
    
    int obscuranceFPSMask;

    void Awake()
    {
        obscuranceFPSMask = LayerMask.GetMask("Ground", "Default", "NPC2");
        gunController = GetComponent<FPSGunController>();
        playerAudioSource = GetComponent<AudioSource>();
        controller       = GetComponent<CharacterController>();
        playerCamera     = FindObjectOfType<Camera>();
        camera_transform = playerCamera.transform;
        pv       = GetComponent<PhotonView>();
        thisTransform    = GetComponent<Transform>();
        platformMask = LayerMask.GetMask("Fadable", "Ground", "Default");
        externalVelocityMask = LayerMask.GetMask("Ground", "Fadable", "InvisibleCollider");
        groundMask = LayerMask.GetMask("Ground", "Ceiling");
        groundAndNPCMask = LayerMask.GetMask("Ground", "NPC2", "Ceiling");
        npc2Mask = LayerMask.GetMask("NPC2");
        slamMask = LayerMask.GetMask("NPC");
    }
    
    
    void MakePlayerLight()
    {
        playerLight = (Instantiate(playerLightFlyingPrefab, new Vector3(2000, 2000, 2000), Quaternion.identity, null)).GetComponent<PlayerLight>();
        playerLight.AttachPlayer(this);
    }
    
    
    public Vector3 GetNetworkPosition()
    {
        return thisTransform.position;
    }
    
    
    void MakePlayerBlob()
    {
         Instantiate(playerBlobPrefab, new Vector3(2000, 2000, 2000), Quaternion.identity).GetComponent<PlayerBlob>().SetTarget(transform);
    }
    
    public GameObject Dash_prefab;
    

    void Start()
    {
        //MakePlayerLight();
        
        
        
        if(pv.IsMine)
        {
            MakePlayerBlob();
            LockCursor();
            wind_audioSource.enabled = true;
            
            windPsUp_main = windPsUp.main;
            windPsDown_main = windPsDown.main;
            //Physics.M
            
            player_renderer.enabled = false;
            
            
            //Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("NPC2"), false);
            //
            mainMaterial = player_renderer.sharedMaterial;
            
            AudioManager.PlayClip(SoundType.PlayerSpawn, 0.2f, 1);
            PlayerGUI_In_Game.HidePlayerGUI();
        }
        else
        {
            Destroy(wind_audioSource.gameObject);
            Destroy(fpsWeaponPlaceMover);
        }
        
        UberManager.Singleton().AddPlayerToList(this.gameObject);
        
        Revive();
    }
    
    float bunnyHopDecreaseRate = 0.065F;
    float bunnyHopIncreasePerJump = 0.125F;
    
    
    
    void ProcessMaxSpeed(float dt)
    {
        float decreaseSpeed = controller.isGrounded ? bunnyHopDecreaseRate * 3.4F : bunnyHopDecreaseRate;
        
        bunnyHopSpeedMult = Mathf.MoveTowards(bunnyHopSpeedMult, bunnyHopMinMult, dt * decreaseSpeed);
        bunnyHopSpeedMult = Mathf.Clamp(bunnyHopSpeedMult, bunnyHopMinMult, bunnyHopMaxMult);
        
        fpsMaxMoveSpeedCurrent = baseMoveSpeed * bunnyHopSpeedMult * moveSpeedMultiplier;
        
        fpsAcceleration = baseFpsAcceleration * bunnyHopSpeedMult;
        if(fpsAcceleration < baseFpsAcceleration)
        {
            fpsAcceleration = baseFpsAcceleration;
        }
    }
    
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        
        
        
        // if(!photonView.IsMine)
        // {
        //     return;
        // }
        
        // if(tryingToSlam)
        // {
        //     return;
        // }
        
        // if(hit.collider.gameObject.layer == LayerMask.NameToLayer("NPC2"))
        // {
        //     if(thisTransform.localPosition.y - hit.collider.transform.localPosition.y > 2)
        //     {
        //         Vector3 randDir = Random.onUnitSphere;
        //         randDir.y = 0;
        //         randDir = Math.Normalized(randDir);
                
        //         BoostVelocity(hit.normal * 7f + randDir * 3);
        //         InGameConsole.LogOrange(string.Format("<color=green>OnControllerColliderHit():</color> col: {0}, normal: {1}", hit.collider.gameObject.name, hit.normal));
        //     }
        //     //if(hit.normal)
        // }
    }
    
    public void Heal(int _hp)
    {
        if(HitPoints <= 0)
        {
            //Means we are dead
            return;
        }
        
        HitPoints += _hp;
        
        OnHealed();
        
        if(HitPoints > MaxHealth)
        {
            HitPoints = MaxHealth;
        }
    }
    
    public AudioClip onHealedClip;
    
    void OnHealed()
    {
        playerAudioSource.PlayOneShot(onHealedClip, 0.75f);
        //InGameConsole.LogFancy("Healed!");
        HurtGUI.ShowHeal();
    }
    
    float staminaTimer = 0f;
    
    void StaminaTick(float dt)
    {
        if(staminaTimer == 0)
        {
            float _staminaRegenRate = isSliding ? 0 : StaminaRegenRate;
            
            Stamina += dt * _staminaRegenRate;
            if(Stamina > 100)
            {
                Stamina = 100;
            }
        }
        else
        {
            staminaTimer -= dt;
            if(staminaTimer < 0)
            {
                staminaTimer = 0;
            }
        }
    }
    
    void Revive()
    {
        PhotonManager.wasFirstPlayerSpawned = true;
        
        if(pv.IsMine)
        {
            SetFPSCameraToPlayer();
            PostProcessingController2.SetState(PostProcessingState.Normal);  
            OrthoCamera.Hide();
            DeadGUI.Hide();
               
            //playerCamera.GetComponent<FollowingCamera>().SetTarget(this.transform);
        }
        
        HitPoints = MaxHealth;
        isAlive = true;
        
        Stamina = MaxStamina;
        
        NPCManager.Singleton().RegisterAiTarget(this.transform);
        
        
        Ray ray = new Ray(transform.position, Vector3.down);
        
        Vector3 particlesPos = transform.position;
        RaycastHit hit;
        
        int groundMask = LayerMask.GetMask("Ground");
        
        if(Physics.Raycast(ray, out hit, 10f, groundMask))
        {
            particlesPos = hit.point;
        }
        
        ParticlesManager.PlayPooled(ParticleType.player_spawn_ps, particlesPos, Vector3.forward);
    }
    
    

    bool isGrounded = false;
    
    bool wasGroundedBefore = false;
    Vector3 velocityInMomentOfGrounded;

#region PhotonView Serialization
    Vector3 syncEndDirection = Vector3.forward;
    Vector3 syncStartDirection = Vector3.forward;
    Vector3 syncPosition, syncStartPosition, syncEndPosition;
    float lastSyncTime, syncDelay, syncTime;
    
    Vector3 syncVelocity;

    

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.IsWriting)
        {
            stream.SendNext(transform.forward);
            
            
            stream.SendNext(transform.position);
            
            // stream.SendNext(velocity);
            //stream.SendNext(finalVelocity);
            stream.SendNext(fpsVelocity);
          //  stream.SendNext((int)aliveState);
        }
        else
        {
            syncTime     = 0f;
            syncDelay    = Time.time - lastSyncTime;
            lastSyncTime = Time.time;

            syncEndDirection    = (Vector3)stream.ReceiveNext();
            syncPosition        = (Vector3)stream.ReceiveNext();

            
            syncStartDirection  = transform.forward;
            syncStartPosition   = transform.position;
            syncEndPosition     = syncPosition;
            
            //This is used for animation only:
            syncVelocity        = (Vector3)stream.ReceiveNext();
            
            
         //   aliveState = (PlayerAliveState)stream.ReceiveNext();
            
        }
    }

#endregion


    void SyncedMovement()
    {
        // TODO: Handle the case when packet arrives very late (say 350ms). - 13.01.2020
        
        if(aliveState == PlayerAliveState.Normal)
        {
            syncTime += UberManager.DeltaTime();
            
            float t = syncTime / syncDelay;

            Vector3 oldPosition = thisTransform.position;
            // Currently we are slamming the Y coord   -12.08.2019
            // thisTransform.position = new Vector3(thisTransform.position.x, syncEndPosition.y, thisTransform.position.z);
            
            float yCoordInterpolated = Mathf.Lerp(syncStartPosition.y, syncEndPosition.y, syncTime / syncDelay); 
            
            thisTransform.position = new Vector3(thisTransform.position.x, yCoordInterpolated, thisTransform.position.z);
            
            float fpsSyncMagnitudeXZ = Mathf.Max(Math.Magnitude(syncVelocity), fpsMaxMoveSpeedCurrent);
            
            if(Math.SqrDistance(thisTransform.position, syncEndPosition) > 8f * 8f)
            {
                InGameConsole.Log(string.Format("<color=yellow>Snapping foreign player!</color>"));
                thisTransform.position = syncEndPosition;
            }
            else
            {
                thisTransform.position = Vector3.MoveTowards(thisTransform.position, syncEndPosition, fpsSyncMagnitudeXZ * UberManager.DeltaTime());
            }
            thisTransform.forward =  Vector3.Slerp(syncStartDirection, syncEndDirection, t);

            // This 'velocity' is used for animation (if not controlled locally):
            //velocity = syncVelocity;
            fpsVelocity = syncVelocity;
        }
        else
        {
            if(aliveState == PlayerAliveState.Dashing)
            {
                thisTransform.position = Vector3.MoveTowards(thisTransform.position, syncEndPosition, dash_speed * UberManager.DeltaTime());
            }
        }
    }
    
    public bool isMovementControllable = true;
    
    public Vector3 lookDirection = Vector3.forward;
    public Vector3 pointerDirection = Vector3.forward;
    
    // [Header("Gamepads:")]
    // // public string[] gamepads;
    // public float mouseYRaw;
    // public float mouseXRaw;
    
    // float stickThreshold = 0.05f;
    
    // public Vector2 gamepadDirection = Vector2.right;
    // public Vector2 actualGamepadDirection = Vector2.left;
    
    // public bool usingMouse = true;
    
    public bool isControllableByPlayer = true;
    
    //public Vector3 externalVelocity = Vector3.zero;
    //public Vector3 recoilVelocity = Vector3.zero;
    
    
    static Vector3 vZero = Vector3.zero;
    const float tractionStrength = 16f;
    public float F_airbourne_min = 8f;
    public float F_airbourne_max = 400f;
    const float F_grounded = 26f;
    
    void CheckOutOfBounds()
    {
        if(isAlive)
        {
            // if(thisTransform.position.y < -105)
            // {
            //     InGameConsole.LogOrange("OUT OF BOUNDS OUT OF BOUNDS");
            //     Die();
            // }
        }
    }
    
    int externalVelocityMask = -1;
    int groundMask = -1;
    int groundAndNPCMask = -1;
    int npc2Mask;
    
    
    public bool IsGrounded()
    {
        return controller.isGrounded;
    }
    
    static readonly Vector3 vUp = new Vector3(0, 1, 0);
    Vector3 inputVelocity;
    public Vector3 finalVelocity;
    
    
    public Transform fpsCam_transform;
    Camera fpsCam_camera;
    Camera fpsCam_weaponView_cam;
    
    
    public void SetFPSCameraToPlayer()
    {
        if(pv.IsMine)
        {
            Transform _camTr = FollowingCamera.Singleton().transform;
            if(_camTr)
            {
                Rigidbody _camRb = _camTr.gameObject.GetComponent<Rigidbody>();
                SphereCollider _camCol = _camTr.gameObject.GetComponent<SphereCollider>();
                if(_camRb)
                    Destroy(_camRb);
                if(_camCol)
                    Destroy(_camCol);
            }
            
            //_camTr.position = Vector3.zero;
            //_camTr.rotation = Quaternion.identity;
            
            _camTr.SetParent(fpsCameraPlace);
            _camTr.localPosition = Vector3.zero;
            _camTr.transform.localRotation = Quaternion.identity;
            
            
            fpsCamXRotation = 0;
            
            Camera guiCamera = FollowingCamera.Singleton().GUI3d_cam;
            PlayerGUI_In_Game.Singleton().AssignPlayerGUI(this, guiCamera);
            // FollowingCamera.Singleton().enabled = false;
            
            // fpsCam_transform = FollowingCamera.Singleton().transform;
            fpsCam_camera = _camTr.GetComponent<Camera>();
            fpsCam_weaponView_cam = _camTr.GetChild(0).GetComponent<Camera>();
        }
    }
    
    //
    [Header("FPS:")]
    public Transform fpsCameraPlace;
    public Transform fpsCameraTilting;
    //FPS:
    const float MAX_NEGATIVE_GRAVITY = -75F;
    public Vector3 fpsVelocity;
    bool invertVertical = true;
    
    public Transform fpsWallJumpSensor;
    bool makeMouseSensitivityUniversal = false;
    public float fpsMouseSens = 1f;
    const float baseMoveSpeed = 12.5F;
    float fpsMaxMoveSpeedCurrent = 12.5f;
    
    float bunnyHopSpeedMult = bunnyHopMinMult;
    const float bunnyHopMaxMult = 1.5F;
    const float bunnyHopMinMult = 0.833F;
    
    const float fpsDuckSpeed = 4.0f;
    const float fpsSlideSpeed = 17.5F;
    const float fpsJumpForce = 10;
    const float fpsGravity = -9.8f * 3F;
    public float player_gravity_multiplier = 1;
    float fpsAcceleration = baseFpsAcceleration;
    const float baseFpsAcceleration = 80F;
    const float fpsOppositeDecceleration = baseFpsAcceleration * 3;
    const float fpsWallJumpForceXZ = 19F;
    const float fpsWallJumpForceY = 16F;
    const float fpsDashStrengthGrounded = 30F;
    const float fpsDashStrengthAirForward = 22F;
    const float fpsDashStrengthAir = 22F;
    const float slideGravity = MIN_GRAVITY * 5;
    float fpsFriction  = 90f;
    const float fpsAirbourneFrictionMult = 0.66F;//0.33f;
    Vector3 slideVel;
    Vector3 desiredVelWorld;
    bool fpsMouseLookBlocked = false;
    
    FPSGunController gunController;
    
    public float vertClamped;
    
    public Transform HeadTarget;
    
    public Vector3 GetHeadPosition()
    {
        return HeadTarget.position;
    }
    
    public float moveSpeedMultiplier = 1;
    public float moveSpeedMultiplier_RevolverUlt = 1;
    
    public void SetMoveSpeedMult(float mult)
    {
        moveSpeedMultiplier = mult;
    }
    
    public bool isSliding = false;
    public ParticleSystem slidingPs;
    
    Vector3 normalCameraPlacePosition = new Vector3(0, 0, 0);
    Vector3 duckCameraPlacePosition = new Vector3(0, 0f, 0);//new Vector3(0, -1.626f, 0);
    float targetFov = 110;
    float fovSpeed = 60f;
    
    const float normalFov = 110;
    const float duckingFov = 120;
    
    
    public void TeleportPlayer(Vector3 tp_pos)
    {
//        InGameConsole.LogOrange("TeleportPlayer");
        InGameConsole.LogOrange(string.Format("{0} -> <color=green>{1}</color>", thisTransform.localPosition, tp_pos));
        thisTransform.localPosition = tp_pos;
        fpsVelocity.x = fpsVelocity.y = fpsVelocity.z = 0;
        skipUpdatedsTimer = 0.1f;
    }
    
    float skipUpdatedsTimer = 0;
    
    // public void ResetFov()
    // {
    //     fovSpeed = 60f;
    //     targetFov = normalFov;
    // }
    
    public void SetBerserkFov()
    {
        targetFov = normalFov * 1.2f;
        fovSpeed = 120f;
    }
    
    void SetTargetFov(float _targetFov, float _fovSpeed)
    {
        targetFov = _targetFov;
        fovSpeed = _fovSpeed;
    }
    
    public void SetTargetFovNormal()
    {
        targetFov = normalFov;
        fovSpeed = 60f;
    }
    
    void InterpolateFov(float dt)
    {
        float currentFov = fpsCam_camera.fieldOfView;
        fpsCam_camera.fieldOfView = Mathf.MoveTowards(currentFov, targetFov, dt * fovSpeed);
        
        
    }
    
    float slideControllerHeight = 0.9f;//1f;
    float normalControllerHeight = 2.4f;
    
    //public AudioSource slidingAudioClip;
    public AudioSource playerAudioSource_sliding;
    
    void OnSlideStarted()
    {
        isSliding = true;
        slidingPs.Play();
        
        controller.height = slideControllerHeight;
        controller.center = new Vector3(0, slideControllerHeight / 2f, 0);
        
        if(pv.IsMine)
        {
            CameraShaker.Slide();
            fpsCameraPlace.localPosition = duckCameraPlacePosition;
            // fpsCam_camera.fieldOfView = duckingFov;
            slideVel.y = slideGravity;
            //Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("NPC2"), true);
        }
        
        
        playerAudioSource_sliding.Play();
    }
    
    public bool canSlide = true;
    
    void OnSlideEnded()
    {
        isSliding = false;
        float castDistance = normalControllerHeight - slideControllerHeight;
        RaycastHit hit;
        slidingPs.Stop();
        playerAudioSource_sliding.Stop();
        
        if(Physics.CapsuleCast(GetTopCapsuleP(), GetBottomCapsuleP(), controller.radius, vUp, out hit,  castDistance, groundAndNPCMask))
        {
            InGameConsole.LogOrange("Can't stand from sliding!");
            Debug.Log("Collider: <color=yellow>" + hit.collider + "</color>");
        }
        else
        {
            controller.height = normalControllerHeight;
            controller.center = new Vector3(0, normalControllerHeight/2f, 0);
            if(pv.IsMine)
            {
                slideVel.x = 0;
                slideVel.z = 0;
                slideVel.y = slideGravity;
                
                CameraShaker.UnSlide();
                
                fpsCameraPlace.localPosition = normalCameraPlacePosition;
                //Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("NPC2"), false);
            }
        }
    }
    
    [PunRPC]
    public void Blink(Vector3 pos)
    {
        thisTransform.localPosition = pos;
        fpsVelocity = Vector3.zero;
        
        AudioManager.Play3D(SoundType.blink_player, pos, 1, 0.4f);
        ParticlesManager.PlayPooled(ParticleType.star_ps, pos, Vector3.forward);
        ParticlesManager.PlayPooled(ParticleType.blink1_ps, pos, Vector3.forward);
        
        //blinkSphere.EndLife();
    }
    
    float LookSensitivity()
    {
        return (fpsMouseLookBlocked ? 0 : fpsMouseSens);
    }
    
    float LookSensitivityHor()
    {
        if(makeMouseSensitivityUniversal)
        {
            float _w = (float)Screen.width;
            float _h = (float)Screen.height;
            
            float ratioHW = _h / _w;
            
            
            return (fpsMouseLookBlocked ? 0 : fpsMouseSens * ratioHW);
            // horizontalRotation *= ratioWH;
            // verticalRotation *= ratioHW;
        }
        
        return (fpsMouseLookBlocked ? 0 : fpsMouseSens);
    }
    
    float LookSensitivityVert()
    {
        if(makeMouseSensitivityUniversal)
        {
            float _w = (float)Screen.width;
            float _h = (float)Screen.height;
            
            float ratioWH = _w / _h;
            
            
            return (fpsMouseLookBlocked ? 0 : fpsMouseSens * ratioWH);
            // horizontalRotation *= ratioWH;
            // verticalRotation *= ratioHW;
        }
        
        return (fpsMouseLookBlocked ? 0 : fpsMouseSens);
    }
    
    public Vector3 GetFPSVelocity()
    {
        return fpsVelocity;
    }
    
    public void LockCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        fpsMouseLookBlocked = false;
    }
    
    public void ReleaseCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        fpsMouseLookBlocked = true;
    }
    
    public void ReleasePlayerMovement()
    {
        canControlPlayer = true;
    }
    
    public void LockPlayerMovement()
    {
        canControlPlayer = false;
    }
    
    float fpsCamXRotation = 0f;
    
    public float tiltingTargetAngle = 0;
    //public float tiltingSpeed = 30f;
    public float tiltAngle = 1.5f;
    
    public Animator weaponSwingingAnimator;
    
    int moveSpeedH = -1;
    Quaternion deriv;
    public float tiltTime = 0.1f;
    public float swingingDampTime = 0f;
    
    
    
    float rttSendCD = 1f;
    float rttSendTimer = 0f;
    public int rtt = 0;
    public float rttInSeconds;
    
    [PunRPC]
    void ReceiveRTT(int _rtt)
    {
        rtt = _rtt;
        rttInSeconds = (float)rtt * 0.001F;
    }
    
    
    // Vector3 prevPos;
    // public Vector3 factualV;
    
    float tiltDeriv;
    
    void LateUpdate()
    {
        if(!pv.IsMine)
        {
            return;
        }
        float dt = UberManager.DeltaTime();
        
        InterpolateFov(dt);
            
        //Tilting:        
        Vector3 cameraLocalRotEuler = fpsCameraTilting.localRotation.eulerAngles;
        Quaternion targetQ = Quaternion.Euler(cameraLocalRotEuler.x, cameraLocalRotEuler.y, tiltingTargetAngle);
        fpsCameraTilting.localRotation = QuaternionUtil.SmoothDamp(fpsCameraTilting.localRotation, targetQ, ref deriv, tiltTime);
        
        if(moveSpeedH == -1)
        {
            moveSpeedH = Animator.StringToHash("MoveSpeed");
        }
        
        float currentSpeedXZ = Math.Magnitude(Math.GetXZ(fpsVelocity));
        
        if(currentSpeedXZ < 0.5f)
        {
            currentSpeedXZ = 0.5F;
        }
        
        float blendParamTarget = currentSpeedXZ;
        if(isSliding || !controller.isGrounded)
        {
            blendParamTarget = currentSpeedXZ / 5f;
        }
        
        weaponSwingingAnimator.SetFloat(moveSpeedH, blendParamTarget, swingingDampTime, dt);
        
        // factualV = (transform.position -  prevPos) / dt;
        // prevPos = transform.position;
    }
    
    
    int SlamDirectDamage = 320;
    const float slamVelocityY = MAX_NEGATIVE_GRAVITY;//-39;
    
    public AudioClip noStaminaClip;
    public AudioClip slamDirectDamageClip;
    
    public void NotEnoughStamina()
    {
        playerAudioSource.PlayOneShot(noStaminaClip, 0.75f);
    }
    
    [PunRPC]
    void GroundSlam(Vector3 _pos)
    {
        if(pv.IsMine)
        {
            if(!tryingToSlam)
            {
                if(Stamina < groundSlamStaminaCost)
                {
                    NotEnoughStamina();
                    return;
                }
                //InGameConsole.LogFancy("GroundSlam()");
                Stamina -= groundSlamStaminaCost;
                staminaTimer += staminaRegenDelay;
                
                groundSlamStartPos = _pos; 
                
                fpsVelocity.y = slamVelocityY;
                fpsVelocity.x = 0;
                fpsVelocity.z = 0;
            
                tryingToSlam = true;
            }
        }
    }
    
    
    float StaminaRegenRate = 38f;
    const float dashStaminaCost = 33f;
    const float groundSlamStaminaCost = 40;
    
    void ConsumeStamina(float cost)
    {
        Stamina -= cost;
        staminaTimer += staminaRegenDelay;
    }
    
    const float dashDamageImmune = 0.2F;
    float damageImmuneTimer = 0;
    
    public ParticleSystem wind_player_ps;
    
    
    
    void FPSDash(Vector3 dashDirWorldSpace)
    {
        if(Stamina < dashStaminaCost)
        {
            NotEnoughStamina();
            return;
        }
        
        ConsumeStamina(dashStaminaCost);
        
        if(wind_player_ps)
        {
            wind_player_ps.Play();
        }
        
        //InGameConsole.LogFancy("FPSDash()");
        if(dashDirWorldSpace.x == 0 && dashDirWorldSpace.z == 0)
        {
            dashDirWorldSpace = thisTransform.forward;
            
            fpsVelocity.y = 0;
            
            if(controller.isGrounded)
            {
                fpsVelocity.x = dashDirWorldSpace.x * fpsDashStrengthGrounded;
                fpsVelocity.z = dashDirWorldSpace.z * fpsDashStrengthGrounded;
            }
            else
            {
                fpsVelocity.x = dashDirWorldSpace.x * fpsDashStrengthAirForward;
                fpsVelocity.y = 1;
                fpsVelocity.z = dashDirWorldSpace.z * fpsDashStrengthAirForward;
            }
        }
        else
        {
            fpsVelocity.y = 0;
            
            if(controller.isGrounded)
            {
                fpsVelocity.x = dashDirWorldSpace.x * fpsDashStrengthGrounded;
                fpsVelocity.z = dashDirWorldSpace.z * fpsDashStrengthGrounded;
            }
            else
            {
                fpsVelocity.x = dashDirWorldSpace.x * fpsDashStrengthAir;
                fpsVelocity.y = 1;
                fpsVelocity.z = dashDirWorldSpace.z * fpsDashStrengthAir;
            }
        }
        
        pv.RPC("FPSDashFX", RpcTarget.All);
    }
    
    public AudioClip dashAudioClip;
    
    public void MakeImmuneForDamageForXTime(float x)
    {
        damageImmuneTimer = x;
    }
    
    [PunRPC]
    void FPSDashFX()
    {
        playerAudioSource.PlayOneShot(dashAudioClip, 0.5f);
        MakeImmuneForDamageForXTime(dashDamageImmune);
    }
    
    Vector3 GetTopCapsuleP()
    {
        Vector3 p1 = thisTransform.localPosition;
        p1.y += controller.center.y;
        p1.y += controller.height / 2;
        p1.y -= controller.radius;
        
        g1 = p1;
        
        return p1;
    }
    
    Vector3 g1, g2;
    
    Vector3 GetBottomCapsuleP()
    {
        Vector3 p2 = thisTransform.position;
        p2.y += controller.center.y;
        p2.y -= controller.height / 2;
        p2.y += controller.radius;
        
        g2 = p2;
        
        return p2;
    }
    
    static readonly Vector3 vForward = new Vector3(0, 0, 1);
    
    Vector3 groundSlamStartPos;
    const float slamPhysicsForce = 80;
    
    Collider[] slammed_cols_rb = new Collider[64];
    Collider[] slammed_cols_npcs = new Collider[32];
    
    int slamMask = -1;
    
    
    
    void DoDamageSlamAirbourne(float dt)
    {
        if(fpsVelocity.y != slamVelocityY)
        {
            return;
        }
        
        Vector3 p1 = GetBottomCapsuleP();
        p1.y -= 0.1f;
        Vector3 p2 = GetBottomCapsuleP();
        p2.y += 0.1f;
        
        RaycastHit hit;
        if(Physics.CapsuleCast(p1, p2, controller.radius * 1.1f, Vector3.down, out hit, dt * Math.Abs(slamVelocityY), npc2Mask))
        {
            NetworkObject npc_net_comp = hit.collider.GetComponent<NetworkObject>();
            if(npc_net_comp != null)
            {
                IDamagableLocal idl = npc_net_comp.GetComponent<IDamagableLocal>();
                if(idl != null)
                {
                    int npc_hp = idl.GetCurrentHP();
                    LimbForExplosions lfe = npc_net_comp.GetComponent<LimbForExplosions>();
                    lfe.OnExplodeAffected();
                    
                    if(npc_hp - SlamDirectDamage <= 0)
                    {
                        Physics.IgnoreCollision(controller, hit.collider, true);
                        idl.TakeDamageLocally(SlamDirectDamage, thisTransform.localPosition, Vector3.down);
                        byte limb_to_destroy = (byte)Random.Range(1, 4);
                        NetworkObjectsManager.CallNetworkFunction(npc_net_comp.networkId, NetworkCommand.DieWithForce, Vector3.down * 250, limb_to_destroy);                       
                        InGameConsole.Log("<color=green>DOING SLAM DIRECT DAMAGE 1</color>");
                    }
                    else
                    {
                        idl.TakeDamageLocally(SlamDirectDamage, thisTransform.localPosition, Vector3.down);
                        NetworkObjectsManager.CallNetworkFunction(npc_net_comp.networkId, NetworkCommand.TakeDamage, SlamDirectDamage);
                        
                        tryingToSlam = false;
                        
                        
                        PlayDirSlam();
                        
                        pv.RPC("PlayDirSlam", RpcTarget.Others);
                        InGameConsole.Log("<color=yellow>DOING SLAM DIRECT DAMAGE 2</color>");
                        //Vector3 randomOffset = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
                        //Vector3 playerBoostDir = 0.5f * randomOffset + hit.normal;
                        //playerBoostDir.Normalize();
                        //BoostVelocity(playerBoostDir * 20);
                        
                        
                    }
                }
            }
        }
    }
    
    [PunRPC]
    void PlayDirSlam()
    {
         playerAudioSource.PlayOneShot(slamDirectDamageClip);
    }
    
    float groundSlammed_timeStamp;
    const float slamJumpTimeWindow = 1.15f;
    
    float GetGroundSlamDelay()
    {
        if(UberManager.IsOnlineMode())
        {
            return 0.075F;
        }
        
        return 0.160F;
    }
    
    //public float slam_delay = 0.120F;
    
    Collider[] slam_pounded_limbs = new Collider[64];
    //const float slam_pound_limbs_radius = 2.5F;
    
    const float box_size_x = 1.0F;
    const float box_size_y = 0.33F;
    const float box_size_z = 1.0F;
    
    [PunRPC]
    void OnGroundSlammed(Vector3 slamImpactPos)
    {
        Vector3 slamFXDir = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
        ParticlesManager.PlayPooled(ParticleType.groundSlam1_ps, slamImpactPos, slamFXDir);
        AudioManager.Play3D(SoundType.Explosion_1, slamImpactPos, 0.5f);
        
        int slammed_limbs_len = Physics.OverlapBoxNonAlloc(slamImpactPos, new Vector3(box_size_x, box_size_y, box_size_z), slam_pounded_limbs, thisTransform.localRotation, slamMask);
            
        for(int  i = 0; i < slammed_limbs_len; i++)
        {
            DamagableLimb limb = slam_pounded_limbs[i].GetComponent<DamagableLimb>();
            if(limb && limb.CanBeStompedOn())
            {
                limb.TakeDamageLimb(3500);
                //InGameConsole.LogFancy("DoingDamageToLimb");
            }
        }
        
        //InGameConsole.LogOrange("<color=red><b>OnGroundSlammed()</b></color>");
        
        Vector3 slam_pos = slamImpactPos + new Vector3(0, 0.05f, 0);
        
        GameObject groundSlam_go = ObjectPool.s().Get(ObjectPoolKey.DeferredGroundSlam, false);
        DeferredGroundSlam deferredSlam = groundSlam_go.GetComponent<DeferredGroundSlam>();
        
        float _slamForce = Math.Abs(groundSlamStartPos.y - slamImpactPos.y);// Globals.NPC_gravity;
        _slamForce = Mathf.Max(16f, _slamForce);
        _slamForce *= 1.1F;
        deferredSlam.DoDeferredSlam(slam_pos, (float)UberManager.GetPhotonTimeDelayedBy(GetGroundSlamDelay()), _slamForce, pv.IsMine);
        
        if(pv.IsMine)
        {
            tryingToSlam = false;
            CameraShaker.ShakeY(7.5f);
            CameraShaker.MakeTrauma(0.85f);
            
            
            groundSlammed_timeStamp = Time.time;
        }
        
    }
    
    public bool tryingToSlam = false;
    public FPSWeaponPlaceMover fpsWeaponPlaceMover;
    
    bool canControlPlayer = true;
    
    public bool CanControlPlayer()
    {
        if(InGameMenu.IsVisible())
        {
            return false;
        }
        
        if(canControlPlayer)
        {
            if(isAlive)
            {
                return true;
            }
            else
                return false;
        }
        else
            return false;
        
    }
    
    public static float ClampAngle(float angle, float min, float max)
	{
		if (angle < -360F)
			angle += 360F;
		if (angle > 360F)
			angle -= 360F;
            
		return Mathf.Clamp(angle, min, max);
	}
    
    
    
    public bool secondJumpHappened = false;
    bool jumpedThisFrame = false;
    
    
    //This vodoo is used for case when you are sliding and then not grounded:
    float groundCheckLength = 0.33f;
    
    static readonly Vector3 vDown = new Vector3(0, -1, 0);
    
    bool CheckGroundUnderPlayer()
    {
        bool Result = false;
        
        if(Physics.Raycast(thisTransform.localPosition, vDown, groundCheckLength, groundMask))
        {
            Result = true;
        }
        
        return Result;
    }
    
    float timeInAir = 0f;
    
    
    public AudioClip OnBecomeGroundedClip;
    
    
    
    void OnBecomeGrounded()
    {
        if(pv.IsMine)
        {
            CameraShaker.ShakeY(4.4f);
            
            velocityBeforeGrounded = fpsVelocity;
            //InGameConsole.LogOrange(string.Format("<b>VelocityBeforeGrounded:</b> <color=blue>{0}</color>", velocityBeforeGrounded));
            
            if(Math.SqrMagnitude(velocityBeforeGrounded) > 8f * 8f)
            {
                playerAudioSource.PlayOneShot(OnBecomeGroundedClip, 0.2f);
            }
        }
        
        
    }
    
    Vector3 velocityBeforeGrounded;
    //Collider colOnGrounded;
    
    void OnBecomeAirbourne()
    {
        if(fpsVelocity.y < 0)
            fpsVelocity.y = 0;
            
        timeInAir = 0;
    }
    
    public Transform fpsCam_anim_recoil;
    
    
    
    void LocalNormalState_UpdateFPS()
    {
        
        float dt  = UberManager.DeltaTime();
        
        if(skipUpdatedsTimer > 0)
        {
            skipUpdatedsTimer -= dt;
            if(skipUpdatedsTimer <= 0)
            {
                skipUpdatedsTimer = 0;
            }
            return; 
        }
        
        StaminaTick(dt);
        
        if(Input.GetKeyDown(KeyCode.T))
        {
            fpsCam_weaponView_cam.enabled = !fpsCam_weaponView_cam.enabled;
        }
        if(Input.GetKeyDown(KeyCode.X))
        {
            if(Cursor.visible)
            {
                LockCursor();
            }
            else
            {
                ReleaseCursor();
            }
        }
        
        //ScanInteractablesFPS();
        
        float horizontalRotation = LookSensitivityHor() * Input.GetAxisRaw("Mouse X");
        float verticalRotation = LookSensitivityVert() * fpsMouseSens * (invertVertical ? -1 : 1) * Input.GetAxisRaw("Mouse Y");
        
        float deltaRotHorizontal = LookSensitivityHor() * Input.GetAxisRaw("Mouse X");
        float deltaRotVertical = LookSensitivityVert() * fpsMouseSens * (invertVertical ? -1 : 1) * Input.GetAxisRaw("Mouse Y");
        
        
        if(!CanControlPlayer())
        {
            horizontalRotation = 0;
            verticalRotation = 0;
            deltaRotVertical = 0;
        }
        
        
        ProcessMaxSpeed(dt);
        
        fpsCam_transform.Rotate(verticalRotation, 0, 0);
        float vertClamped = Mathf.Clamp(fpsCam_transform.localRotation.eulerAngles.x, -90f, 90f);
        vertClamped = fpsCam_transform.localRotation.eulerAngles.x;
        
        fpsCam_transform.localRotation = Quaternion.Euler(vertClamped, fpsCam_transform.localRotation.y, fpsCam_transform.localRotation.z);
        
        thisTransform.Rotate(0, horizontalRotation, 0);
        
        Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        
        if(!CanControlPlayer())
        {
            input *= 0;
        }
        tiltingTargetAngle = input.x == 0 ? 0 : Mathf.Sign(input.x) * -tiltAngle;
        tiltingTargetAngle *= 1 - Math.Abs(fpsCameraPlace.forward.y);
        
        if(input.x * input.z != 0f)
        {
            input = Math.Normalized(input);
        }
        Vector3 inputDirWorldSpace = thisTransform.TransformDirection(input);
        
        desiredVelWorld = inputDirWorldSpace * fpsMaxMoveSpeedCurrent * moveSpeedMultiplier * moveSpeedMultiplier_RevolverUlt;
        
        capsuleP1 = GetTopCapsuleP();
        capsuleP2 = GetBottomCapsuleP();
        
        jumpedThisFrame = false;
        
        
        if(controller.isGrounded)
        {
            if(!wasGroundedBefore)
            {
                OnBecomeGrounded();
                wasGroundedBefore = true;
            }
            
            
            secondJumpHappened = false;
            
            if(Inputs.GetJumpKey())
            {
                jumpedThisFrame = true;
                FPSJump();
            }
            else
            {
                if(fpsVelocity.y <= 0)
                    fpsVelocity.y = MIN_GRAVITY;
                    
                    
                if(isSliding)
                {
                    if(!Inputs.GetSlamAndSlide_Key())
                    {
                        OnSlideEnded();
                    }
                }
                
                
                if(!isSliding && canSlide)
                {
                    if(Inputs.GetSlamAndSlide_KeyDown())
                    {
                        if(fpsVelocity.x == 0 && fpsVelocity.z == 0)
                        {
                            slideVel = GetFPSDirection();
                        }
                        else
                        {
                            slideVel = fpsVelocity;
                        }
                        slideVel.y = 0;
                        slideVel = Math.Normalized(slideVel);
                        
                        slideVel *= fpsSlideSpeed * moveSpeedMultiplier;
                        OnSlideStarted();
                    }
                }
                
                if(!isSliding)
                {
                    if(Inputs.GetDashKeyDown())
                    {
                        // InGameConsole.LogFancy("FPSDash() 1");
                        FPSDash(inputDirWorldSpace);
                    }
                    
                    Vector3 fpsVelocityXZ = Math.GetXZ(fpsVelocity);
                    //fpsVelocityXZ = Vector3.MoveTowards(fpsVelocityXZ, desiredVelWorld, fpsFriction * dt * moveSpeedMultiplier);
                    fpsVelocityXZ = Vector3.MoveTowards(fpsVelocityXZ, desiredVelWorld, fpsFriction * dt);
                    
                    fpsVelocity.x = fpsVelocityXZ.x;
                    fpsVelocity.z = fpsVelocityXZ.z;
                }
                else
                {
                    //fpsVelocity.x = Mathf.MoveTowards(fpsVelocity.z, fpsMoveSpeed, dt * 2);;
                    //fpsVelocity.z = Mathf.MoveTowards(fpsVelocity.z, fpsMoveSpeed, dt * 2);
                    fpsVelocity.x = slideVel.x;
                    fpsVelocity.z = slideVel.z;
                }
            }
        }
        else
        {
            
            if(wasGroundedBefore)
            {
                OnBecomeAirbourne();
                wasGroundedBefore = false;
            }
            
            timeInAir += dt;
            if(!isSliding)
            {
                if(Inputs.GetDashKeyDown())
                {
                    // InGameConsole.LogFancy("FPSDash() 2");
                    FPSDash(inputDirWorldSpace);
                }
            }
            
            if(isSliding && !CheckGroundUnderPlayer())
            {
                OnSlideEnded();
            }
            
            if(Input.GetKeyDown(KeyCode.LeftControl) && !isSliding)
            {
                if(fpsVelocity.y > -39)
                {
                    Vector3 slamPosition = thisTransform.localPosition;
                    GroundSlam(slamPosition);
                    pv.RPC("GroundSlam", RpcTarget.All, slamPosition);
                }
            }
            
            
            
            fpsVelocity.y += fpsGravity * dt * player_gravity_multiplier;
            fpsVelocity.y = Mathf.Clamp(fpsVelocity.y, MAX_NEGATIVE_GRAVITY, 200);
            
            Vector3 fpsVelocityXZ = Math.GetXZ(fpsVelocity);
            Vector3 targetVel = Math.SqrMagnitude(desiredVelWorld) > 0.1f ? desiredVelWorld : fpsVelocityXZ;
            
            fpsVelocityXZ = Vector3.MoveTowards(fpsVelocityXZ, targetVel, fpsFriction * fpsAirbourneFrictionMult * dt);
            
            fpsVelocity.x = fpsVelocityXZ.x;
            fpsVelocity.z = fpsVelocityXZ.z;
            
            float fpsVelMagnitude = Math.Magnitude(fpsVelocity);
            
            if(!jumpedThisFrame)
            {
                Ray wallJumpRay = GetFPSRay();
                RaycastHit hit;
                
                if(Physics.Raycast(wallJumpRay, out hit, 1.25f, groundMask))
                {
                    // controller.ClosestPoint(hit.point);
                    
                    if(Math.Abs(hit.normal.y) < 0.2)
                    {
                        if(Inputs.GetJumpKeyDown())
                        {
                            Vector3 wallJumpBoostVel = hit.normal * fpsWallJumpForceXZ;
                            wallJumpBoostVel.y = fpsWallJumpForceY;
                            
                            playerAudioSource.PlayOneShot(jumpWallLocalClip, 0.8f);
                            // Debug.DrawRay(hit.point, Math.Normalized(wallJumpBoostVel) * 1f, Color.yellow, 3f);
                            // InGameConsole.LogFancy("WallJump vel: " + wallJumpBoostVel);
                            BoostVelocity(wallJumpBoostVel);
                        }
                    }
                }
                else
                {
                    if(!secondJumpHappened && Stamina >= secondJumpStaminaCost && Inputs.GetJumpKeyDown() && timeInAir > 0.25f /*&& fpsVelocity.y < 0*/)
                    {
                        jumpedThisFrame = true;
                        SecondJumpFPS();
                    }
                }
            }
            
            RaycastHit capsuleCastHit;
            
            Vector3 capsuleDir = Math.Normalized(fpsVelocity);
            
            if(Physics.CapsuleCast(capsuleP1, capsuleP2, controller.radius * 1.0F, capsuleDir, out capsuleCastHit, fpsVelMagnitude * dt, groundMask))
            {
                float dot = Vector3.Dot(capsuleCastHit.normal, capsuleDir);
                
                Vector3 calculatedV = fpsVelocity + capsuleCastHit.normal * fpsVelMagnitude * Math.Abs(dot);
                if(fpsVelocity.y < 0)
                {
                    if(calculatedV.y == 0)
                    {
                        calculatedV.y = MIN_GRAVITY;
                    }
                }
                
                //InGameConsole.LogFancy(string.Format("PlayerController(): hit something, <color=green>v1{0}</color>, <color=yellow>v2{1}</color>", fpsVelocity, calculatedV));
                fpsVelocity = calculatedV;
                //InGameConsole.LogFancy(string.Format("PlayerController(): dot: <color=green>{0}</color> magnitude<color=yellow>{1}</color>", dot, fpsVelMagnitude));
            }
        }
        
        if(tryingToSlam)
        {
            RaycastHit slamHit;
            Vector3 rayOrigin = capsuleP2;
            rayOrigin.y -= controller.radius;
            Ray ray = new Ray(rayOrigin, -vUp);
            DoDamageSlamAirbourne(dt);
            
            if(Physics.Raycast(ray, out slamHit, Math.Abs(slamVelocityY) * dt, groundAndNPCMask))
            {
                bool isKeyPressed = Inputs.GetSlamAndSlide_Key();
                if(isKeyPressed && timeInAir > 0.125f)
                {
                    pv.RPC("OnGroundSlammed", RpcTarget.All, slamHit.point);
                }
                else
                {
                    ////InGameConsole.Log(string.Format("<color=red>Failed to slam ground</color> <color=green>LeftControl: {0}, timeInAir: {1} </color>", isKeyPressed, timeInAir.ToString("f")));
                }
                tryingToSlam = false;
            }
        }
        
        controller.Move(fpsVelocity * dt);
        
        fpsWeaponPlaceMover.Tick(thisTransform.InverseTransformVector(-fpsVelocity));
    }
    
    
    public ParticleSystem slamLaunch_ps;
    
    void FPSJump()
    {
        float magnitudeVelocity = Math.Magnitude(fpsVelocity);
        float _bunnyHopIncrease = bunnyHopIncreasePerJump * Mathf.InverseLerp(0, baseMoveSpeed, magnitudeVelocity);
        if(desiredVelWorld.x == 0 && desiredVelWorld.z == 0)
            _bunnyHopIncrease = 0;
        bunnyHopSpeedMult += _bunnyHopIncrease;
        
        if(isSliding)
        {
            fpsVelocity.y = fpsJumpForce * 0.45f;
            fpsVelocity.x = slideVel.x * 1.45f;
            fpsVelocity.z = slideVel.z * 1.45f;
            
            OnSlideEnded();
            playerAudioSource.PlayOneShot(jumpSlideLocalClip, 0.875f);
            
            //InGameConsole.LogFancy("FPSJump() Sliding");
        }
        else
        {
            // if(Math.SqrMagnitude(Math.GetXZ(fpsVelocity)) < 22f * 22)
            // {
            //     fpsVelocity.x *= bunnyHopMult;
            //     fpsVelocity.z *= bunnyHopMult;
            // }
            
            
            if((Math.Abs(Time.time  - groundSlammed_timeStamp) > slamJumpTimeWindow))
            {
                fpsVelocity.y = fpsJumpForce;
            }
            else
            {
                // if((velocityBeforeGrounded.y <= MAX_NEGATIVE_GRAVITY))
                // {
                    float slamLaunchForce = Math.Max(fpsJumpForce * 2.5f * moveSpeedMultiplier, 28f);
                    fpsVelocity.y = slamLaunchForce;
                    slamLaunch_ps.Play();
                    
                    // if(thisTransform.localPosition.y < groundSlamStartPos.y)
                    // {
                    //     float height_travelled = Math.Abs(thisTransform.localPosition.y - groundSlamStartPos.y);
                    //     slamLaunchForce = fpsGravity * 0.5f * 0.5f + height_travelled / 0.5f;
                    // }
                // }
            }
            playerAudioSource.PlayOneShot(jumpLocalClip, 0.275f);
        }
        
        
        //InGameConsole.LogFancy("FPSJump()");
    }
    
    int secondJumpStaminaCost;
    
    public void SecondJumpFPS()
    {
        if(tryingToSlam)
        {
            return;
        }
        
        ConsumeStamina(secondJumpStaminaCost);
        
        secondJumpHappened = true;
        if(fpsVelocity.y < fpsJumpForceSecond)
        {
            fpsVelocity.y = fpsJumpForceSecond;
        }
        else
        {
            fpsVelocity.y += fpsJumpForceSecond;
        }
        
        // InGameConsole.LogFancy("SecondJumpFPS()");
        playerAudioSource.PlayOneShot(jumpSecondLocalClip, 0.8f);
    }
    
    public Vector3 GetFPSDirection()
    {
        float x = Screen.width / 2f;
        float y = Screen.height / 2f;

        Ray ray = fpsCam_camera.ScreenPointToRay(new Vector3(x, y, 0));
        
        //Debug.DrawRay(ray.origin, ray.direction, Color.yellow, 5);
        
        return ray.direction;
    }
    
    public float fpsRayDistance = 1.1f;
    
    public Transform GetFPSCameraTransform()
    {
        if(fpsCam_camera)
        {
            return fpsCam_camera.transform;
        }
        else
            return null;
    }
    
    const float fpsRayOffset = 0.125F;
    
    public Transform GunLowerPoint_FPS;
    public Transform GunMiddlePoint_FPS;
    
    public Ray GetLowerFPSRay()
    {
        return new Ray(GunLowerPoint_FPS.position, GunLowerPoint_FPS.forward);
    }
    
    public Ray GetMiddleFPSRay()
    {
        return new Ray(GunMiddlePoint_FPS.position, GunLowerPoint_FPS.forward);
    }
    
    
    
    public bool CheckIfFPSRayObscure()
    {
        float x = Screen.width / 2f;
        float y = Screen.height / 2f;

        Ray ray = fpsCam_camera.ScreenPointToRay(new Vector3(x, y, 0));
        RaycastHit hit;
        
        if(Physics.Raycast(ray, out hit, fpsRayDistance, obscuranceFPSMask))
        {
            //InGameConsole.LogFancy("Hit nothing!");
            ray.origin += ray.direction * fpsRayOffset;
            
            InGameConsole.LogFancy("FPS ray obscured with " + hit.collider.name);
            return true;
        }
        
        return false;
    }
    
    public Ray GetFPSRay()
    {
        float x = Screen.width / 2f;
        float y = Screen.height / 2f;

        Ray ray = fpsCam_camera.ScreenPointToRay(new Vector3(x, y, 0));
        RaycastHit hit;
        
        if(!Physics.Raycast(ray, out hit, fpsRayDistance, ~0))
        {
            //InGameConsole.LogFancy("Hit nothing!");
            ray.origin += ray.direction * fpsRayOffset;
        }
        
        //Debug.DrawRay(ray.origin, ray.direction, Color.yellow, 5);
        
        return ray;
    }
    
    public Vector3 GetFPSPosition()
    {
        if(fpsCam_camera == null)
        {
            return GetGroundPosition();
        }
        return GetFPSRay().origin;
    }
    
    public Vector3 GetFPSPositionPredicted()
    {
        return GetFPSPosition() + GetFPSVelocity() * rttInSeconds;
    }
    
    public Vector3 GetCenterPosition()
    {
        return thisTransform.localPosition + new Vector3(0, controller.height/2 , 0);
    }
    
    
    public Vector3 GetGroundPosition()
    {
        return thisTransform.localPosition;
    }
    
    public Vector3 GetGroundPositionPredicted()
    {
        return GetGroundPosition() + GetFPSVelocity() * rttInSeconds;
    }
    
    public Vector3 GetFPSBulletStartPos()
    {
        float x = Screen.width / 2f;
        float y = Screen.height / 2f;

        Ray ray = fpsCam_camera.ScreenPointToRay(new Vector3(x, y, 0));
        
        return ray.origin + ray.direction * 0.33f;
    }
    
    void OnGUI()
    {
        return;
        
        if(pv.IsMine)
        {
            
            GUIStyle style = new GUIStyle();
            style.alignment = TextAnchor.MiddleCenter;
            
            float w = 300;
            float h = 50;
            
            style.normal.textColor = Color.green;
            GUI.Label(new Rect(Screen.width/2, Screen.height - h*1.6f, w, h), "bunnyHopSpeedMult: " + bunnyHopSpeedMult.ToString(), style);
            GUI.Label(new Rect(Screen.width/2, Screen.height - h*1.3f, w, h), "currentARFireRate: " + gunController.currentARFireRate.ToString(), style);
        }
    }
    
    
    
    void DoImpact(Vector3 pos, Vector3 upDir)
    {
        Impact(pos, upDir);
        pv.RPC("Impact", RpcTarget.Others, pos, upDir);
    }
    
    [PunRPC]
    void Impact(Vector3 pos, Vector3 upDir)
    {
        // AudioManager.PlayClip(SoundType.shoot_npc_1, 0.7f, 1);
        ParticlesManager.PlayPooledUp(ParticleType.dust_impact_1, pos, upDir);
        
    }
    
    void SyncRTTWithMaster(float dt)
    {
        if(rttSendTimer > rttSendCD)
        {
            // InGameConsole.LogFancy("UpdateRTT");
            if(PhotonNetwork.IsMasterClient)
            {
                ReceiveRTT(PhotonNetwork.NetworkingClient.LoadBalancingPeer.RoundTripTime);
            }
            else
            {
                int ping = PhotonNetwork.NetworkingClient.LoadBalancingPeer.RoundTripTime;
                pv.RPC("ReceiveRTT", RpcTarget.MasterClient, ping);
            }
            rttSendTimer = 0;
        }
        else
        {
            rttSendTimer += dt;
        }
    }
    
    public AudioSource wind_audioSource;
    
    public ParticleSystem windPsUp;
    public ParticleSystem windPsDown;
    
    ParticleSystem.MainModule windPsUp_main;
    ParticleSystem.MainModule windPsDown_main;
    
    void WindTick()
    {
        float vol = 0;
        float vel_y_abs = Math.Abs(fpsVelocity.y);
        float vol_t = Mathf.InverseLerp(16f, 50f, vel_y_abs);
        vol = Mathf.Lerp(0f, 1f, vol_t);
        
        wind_audioSource.volume = vol;
        
        if(fpsVelocity.y > 30)
        {
            windPsDown.Stop(); 
            if(!windPsUp.isPlaying)
            {
                windPsUp.Play();
            }
            else
            {
                //
                windPsUp_main.simulationSpeed = 3 * Mathf.InverseLerp(30f, 50f, fpsVelocity.y);
            }
        }
        else if(fpsVelocity.y < -30)
        {
           windPsUp.Stop(); 
           windPsDown_main.simulationSpeed = 3 * Mathf.InverseLerp(-30f, -50f, fpsVelocity.y);
        }
        else
        {
           windPsUp.Stop();
           windPsDown.Stop(); 
        }
    }
    
    bool touchedGroundAfterSpawn = false;
    
    void Update()
    {
        float dt = UberManager.DeltaTime();
        SyncRTTWithMaster(dt);
        
        
        if(pv.IsMine)
        {
            damageImmuneTimer -= dt;
            if(damageImmuneTimer < 0)
            {
                damageImmuneTimer = 0;
            }
            
            if(!touchedGroundAfterSpawn)
            {
                if(IsGrounded())
                {
                    PlayerGUI_In_Game.ShowPlayerGUI();
                    touchedGroundAfterSpawn = true;
                }
            }
            
            
            CheckOutOfBounds();
            WindTick();
            switch(aliveState)
            {
                case PlayerAliveState.Normal:
                {
                    // LocalNormalState_Update();
                    LocalNormalState_UpdateFPS();
                    break;    
                }
            }
        }
        else
        {
            SyncedMovement();
        }
        
        //HurtMaterialSwap();
    }
    
    void HurtMaterialSwap()
    {
        timeSinceHurt += UberManager.DeltaTime();
        
        if(timeSinceHurt < timeForHurtMaterial)
        {
            if(player_renderer.sharedMaterial.GetInstanceID() != hurtMaterial.GetInstanceID())
            {
                player_renderer.sharedMaterial = hurtMaterial;
            }
        }
        else
        {
            if(player_renderer.sharedMaterial.GetInstanceID() != mainMaterial.GetInstanceID())
            {
                player_renderer.sharedMaterial = mainMaterial;
            }
        }
    }
    
    
    
    [Header("Platform settings:")]
    // public float rayLength = MIN_GRAVITY_ABS * 2f;  
    public LayerMask platformMask;
    public Transform currentPlatform;
    public float heigthOffset = 0.2f;
    
    MovingPlatform platform;
    
    
            
    Vector3 inputGizmo;
    
    float F_airbourne_lerped = 0;
    
    Vector3 capsuleP1;
    Vector3 capsuleP2;
    
#if true && UNITY_EDITOR
    
    

    void OnDrawGizmos()
    {
        
        if(UnityEditor.EditorApplication.isPlaying)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(capsuleP1, controller.radius/3);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(capsuleP2, controller.radius/3);
        }
        // if(currentPlatform != null)
        // {
        //     UnityEditor.Handles.Label(transform.position + transform.up * 2.2f, "<b>Standing on " + currentPlatform.name + "</b>", style);
        // }
        // else
        // {
        //     UnityEditor.Handles.Label(transform.position + transform.up * 2.2f, "<b>null</b>", style);
        // }
        
        // UnityEditor.Handles.color = Colors.LightBlue;
        // UnityEditor.Handles.DrawWireDisc(transform.position, transform.up, 1f);
        
    }
#endif

    
    public GameObject mousePointerPrefab;
    public GameObject mousePointerCirclePrefab;
    
    

    void GetDirectionFromMouse2(Vector3 mousePos, ref Vector3 outDirection)
    {
        Vector3 dir = Vector3.forward;

        Ray ray = playerCamera.ScreenPointToRay(Inputs.MousePosition());
        Plane groundPlane = new Plane(Vector3.up, thisTransform.position);
        float rayDistance;
        Vector3 point = thisTransform.position;
        
        if(groundPlane.Raycast(ray, out rayDistance))
        {
            point = ray.GetPoint(rayDistance);
            
            // if(photonView.IsMine)
            //     PlacePointer(point);
                
            dir = new Vector3(point.x, 0f, point.z) - new Vector3(transform.position.x, 0f, transform.position.z);
            if(dir == Vector3.zero)
            {
                dir =  Vector3.right;
            }
            else
            {
                dir.Normalize();
            }
            outDirection = dir;
        }
        
    }
    
    [Header("Stats:")]
    const int MaxHealth = 100;
    public int HitPoints;
    const float MaxStamina = 100;
    public float Stamina;
    const float staminaRegenDelay = 0.6f;
    
    
    float timeSinceHurt = 4f;
    const float timeForHurtMaterial = 0.225f;
    
    public int GetMaxHealth()
    {
        return MaxHealth;
    }
    
    public void BoostVelocityAdditive(Vector3 boostVel)
    {
        if(pv.IsMine)
        {
            
            tryingToSlam = false;
            
            fpsVelocity += boostVel;
        }
    }
    
    public void ZeroOutNegGravity()
    {
        if(fpsVelocity.y < 0)
        {
            fpsVelocity.y = 0.1f;
        }
    }
    
    
    [PunRPC]
    public void BoostVelocity(Vector3 boostVel)
    {
        if(pv.IsMine)
        {
            if(isSliding)
            {
                OnSlideEnded();
            }
            float magnitude = Math.Magnitude(boostVel);
            CameraShaker.MakeTrauma(0.15f * Mathf.InverseLerp(0, 10, magnitude));
            tryingToSlam = false;
            fpsVelocity = boostVel;
        }
    }
    
    public void TakeDamageOnline(int dmg)
    {
        pv.RPC("TakeDamage", RpcTarget.All, dmg);
    }
    
    
    float TimeWhenTookDamage = 0;
    const float takeDamageImmunityDuration = 0.2F;
    
    public bool CanTakeDamageFromProjectile()
    {
        if(Math.Abs(Time.time - TimeWhenTookDamage) < takeDamageImmunityDuration)
        {
            return false;
        }
        
        if(damageImmuneTimer > 0)
        {
            return false;
        }
        
        return true;
    }
    
    public void TakeDamageNonLethal(int dmg)
    {
        HitPoints -= dmg;
        if(HitPoints <= 1)
        {
            // InGameConsole.LogOrange("HitPoints <= 0");
            HitPoints = 1;
            //Die();    
        }
        else
        {
            // InGameConsole.LogOrange("Trying to call OnTakeDamage");
            OnTakeDamage(dmg);
        }
    }
    
    [PunRPC]
    public void TakeDamage(int dmg)
    {
        if(Math.Abs(Time.time - TimeWhenTookDamage) < takeDamageImmunityDuration)
        {
            //InGameConsole.LogFancy("DAMAGE WAS IGNORED");
            return;
        }
        
        if(damageImmuneTimer > 0)
        {
            //InGameConsole.LogFancy("IMMUNE TO DAMAGE");
            return;
        }
        
        TimeWhenTookDamage = Time.time;
        dmg = 0;
        HitPoints -= dmg;
        if(HitPoints <= 0)
        {
            // InGameConsole.LogOrange("HitPoints <= 0");
            HitPoints = 0;
            Die();    
        }
        else
        {
            // InGameConsole.LogOrange("Trying to call OnTakeDamage");
            OnTakeDamage(dmg);
        }
    }
    
    OnDamageTakenEffects damage_fx;
    
    public AudioClip hurtLocalClip;
    public AudioClip jumpLocalClip;
    public AudioClip jumpSlideLocalClip;
    public AudioClip jumpSecondLocalClip;
    public AudioClip jumpWallLocalClip;
    
    void OnTakeDamage(int dmg)
    {
        if(pv.IsMine)
        {
            //PostProcessingController.AddSaturation(-0.15f);
            CameraShaker.MakeTrauma(0.45f * Mathf.InverseLerp(1, 100, dmg));
            playerAudioSource.PlayOneShot(hurtLocalClip, 0.4f);
            HurtGUI.ShowHurt();
        }
        
        timeSinceHurt = 0f;
    }
    
    public void OnDestroyCustom()
    {
        // if(NPCManager.Singleton() != null)
        // {
        //     if(this.transform != null)
        //     {
        //         NPCManager.Singleton().UnregisterAiTarget(this.transform);
        //     }
        // }
       if(pv.IsMine && FollowingCamera.Singleton() && fpsCameraPlace.childCount > 0)
       {
            Transform _camTr = FollowingCamera.Singleton().transform;
            _camTr.SetParent(null);
            _camTr.localPosition  = new Vector3(0, 8, 0);
            _camTr.localRotation  = Quaternion.Euler(45f, 0, 0);
            
            if(_camTr)
            {
                Rigidbody _camRb = _camTr.gameObject.GetComponent<Rigidbody>();
                SphereCollider _camCol = _camTr.gameObject.GetComponent<SphereCollider>();
                if(_camRb)
                    Destroy(_camRb);
                if(_camCol)
                    Destroy(_camCol);
            }
            // fpsCam_transform = null;
            fpsCam_camera = null;
            fpsCam_weaponView_cam = null;
       }
        
       if(playerLight)
       {
            Destroy(playerLight.gameObject);
       } 
       
       if(dash)
       {
           Destroy(dash.gameObject);
       }
       
    }
    
    void OnDestroy()
    {
        if(NPCManager.Singleton())
        {
            NPCManager.Singleton().aiTargets.Remove(this.transform);
        }
        
        if(pv.IsMine == false)
        {
            if(playerLight)
            {
                Destroy(playerLight.gameObject);
            } 
            
            if(dash)
            {
                Destroy(dash.gameObject);
            }
            
        }
    }
    
    [PunRPC]
    void DashFXStart()
    {
        if(!pv.IsMine)
            player_renderer.enabled = false;
        dash.Play();
    }
    
    [PunRPC]
    void DashFXEnd()
    {
        if(!pv.IsMine)
            player_renderer.enabled = true;
        dash.Stop();
    }
    
    
    
    public PlayerDash dash;
    //const float dash_cooldown = 0.55f;
    const float dash_duration = 0.25f;
    float dash_timer = 0f;
    float dash_speed = 36f;
    
    Vector3 dash_dir;
    
    void Dashing_Update()
    {
        if(isAlive)
        {
            float dt = UberManager.DeltaTime();
            
            Vector3 _dash_dir = new Vector3(0, 0, 1);
            GetDirectionFromMouse2(Input.mousePosition, ref _dash_dir);
            
            if(aliveState == PlayerAliveState.Dashing)
            {
                dash_timer += dt;
                if(dash_timer > dash_duration)
                {
                    aliveState = PlayerAliveState.Normal;
                    pv.RPC("DashFXEnd", RpcTarget.All);
                    //DashFXEnd();
                }
                else
                {
                    Vector3 dash_vel = dash_dir * dash_speed;
                    controller.Move(dash_vel * dt);
                }
            }
            else
            {
                if(Input.GetMouseButtonDown(1))    
                {
                    Vector3 mouse_pos = Input.mousePosition;
                    
                    Dash(_dash_dir);
                }
            }
        }
    }
    
   
    
    void Dash(Vector3 _dir)
    {
        // InGameConsole.LogFancy(string.Format("Dashing! {0}", _dir));
        dash_dir = _dir;
        //externalVelocity = velocity = Vector3.zero;
        
        velocity.y = MIN_GRAVITY;
        velocity.x = velocity.z = 0;
        
        aliveState = PlayerAliveState.Dashing;
        dash_timer = 0f;
        
        pv.RPC("DashFXStart", RpcTarget.All);
    }
    
    //[PunRPC]
    void Die()
    {
        if(!isAlive)
            return;
        
        if(NPCManager.Singleton() != null)
        {
            NPCManager.Singleton().UnregisterAiTarget(this.transform);
        }
        
        if(pv.IsMine)
        {
            gunController.DestroyFPSGuns();
            PlayerGUI_In_Game.Singleton().ReleasePlayerGUI();
            PostProcessingController2.SetState(PostProcessingState.PlayerDead);
            OrthoCamera.Show();
            DeadGUI.Show();
            Transform _camTr = FollowingCamera.Singleton().transform;
            Rigidbody _camRb = _camTr.gameObject.AddComponent<Rigidbody>();
            
            _camRb.drag = 1.5f;
            _camRb.mass = 50;
            _camRb.AddForce(Vector3.up * 0.01f, ForceMode.VelocityChange);
            //_camRb.AddTorque(Random.onUnitSphere * 0.01f, ForceMode.VelocityChange);
            SphereCollider _camCol = _camRb.gameObject.AddComponent<SphereCollider>();
            _camCol.radius = 1;
            _camTr.SetParent(null);
            
            Camera _cam = FollowingCamera.Singleton().GetComponent<Camera>();
        }
        
        
        
        isAlive = false;
        InGameConsole.LogFancy(string.Format("Player {0} has <color=red>died</color>", this.gameObject.name));
        //GlobalShooter.MakeGibs(thisTransform.localPosition, 32);
    }
}