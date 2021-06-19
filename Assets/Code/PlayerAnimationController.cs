using UnityEngine;
using System.Collections;
using UnityEditor;
using Photon.Pun;

public class PlayerAnimationController : MonoBehaviour
{
    public GameObject dwarf_main_object;

    PlayerController controller;
    // GunController gunController;
    Animator animator;
    PhotonView main_photonView;

    [Header("Hands IK:")]
    public FastIKFabric ik_r_hand;
    public FastIKFabric ik_l_hand;

    void Awake()
    {
        if(transform.parent == null)
        {
            dwarf_main_object = this.gameObject;
        }

        controller = dwarf_main_object.GetComponent<PlayerController>();
        //gunController = dwarf_main_object.GetComponent<GunController>();
        animator = dwarf_main_object.GetComponent<Animator>();
        
        InitAnimatorHashes();
    }
    
    void InitAnimatorHashes()
    {
        xVelocityHash = Animator.StringToHash("xVelocity");
        zVelocityHash = Animator.StringToHash("zVelocity");
    }

    void Start()
    {
        main_photonView = dwarf_main_object.GetComponent<PhotonView>();
        pistolIK_R_originLocalPosition = pistolTargetIK_R.localPosition;
        
        rifleIK_R_originalLocalPosition = rifleTargetIK_R.localPosition;
        rifleIK_L_originalLocalPosition = rifleTargetIK_L.localPosition;
        
        ReleaseIK_r_hand();
        ReleaseIK_l_hand();
        
        bonesToShake = GetComponent<BodyShakeIKAnimator>();
    }

    Weapon weaponInPrevFrame;

    void Update()
    {
        // Vector3 v = controller.velocity;
        //Vector3 v = controller.finalVelocity;
        Vector3 v = controller.GetFPSVelocity();
        Vector3 dir = transform.forward;
        MovementAnimation(v, dir);

        //TODO: Need to sync this:
        HandsAnimation();
        
        float tremorMultiplier = v.sqrMagnitude > 1f ? tremorMovementFreqMultiplier : 1f;

        EvaluateTargetIK(tremorMultiplier);
    }
    
    BodyShakeIKAnimator bonesToShake;
    public Transform chest;
    
    void LateUpdate()
    {
        // chest.localRotation = Quaternion.Euler(-30, 0f, 0f);
        bonesToShake.Evaluate();
        ik_r_hand.ResolveIK();
        ik_l_hand.ResolveIK();
    }
    
    [Header("Damp settings:")]
    public float movementDampValue = 0.05f;
    const float timeScale = 1f;
    
    [Header("Weapon IK targets:")]
    public Transform pistolTargetIK_R;
    public Transform rifleTargetIK_R;
    public Transform rifleTargetIK_L;
    
    
    public bool useTremor = true;
    public float tremorScale = 0.1f;
    public float tremorFrequency = 0.25f;
    public AnimationCurve tremorCurve;
    
    Vector3 pistolIK_R_originLocalPosition;
    Vector3 rifleIK_R_originalLocalPosition;
    Vector3 rifleIK_L_originalLocalPosition;
    
    
    public float tremorMovementFreqMultiplier = 1.6f;
    Vector3 tremorizedHandPosition;
    float tremorTimer = 0f;
    
    //[Header("Pistol shoot IK:")]
    //Pistol:
    const float pistolDampingPos = 1f;
    const float pistolDampingRot = 360f;
    const float pistolRecoilPosAmount = 0.17f;
    const float pistolRecoilRotAmount = -55f;
    static Quaternion correctPistolRotation = Quaternion.Euler(-44f, 1f, -4.5f);
    
    //Shotgun:
    const float shotgunDampingPos = 1.75f;
    public float shotgunDampingRot = 360f;
    const float shotgunRecoilPosAmount = 0.32f;
    public float shotgunRecoilRotAmount = -55f;
    const float bolterRecoilRotAmount = -30f;
    static Quaternion correctShotgunRotation = Quaternion.Euler(-44.33f, 6.341f, -12.2f);
    
    
    //Hashes:
    int xVelocityHash;
    int zVelocityHash;
    
    readonly static Vector3 vRight = new Vector3(1, 0, 0);
    
    public void DoShotgunRecoilOnce()
    {
        //InGameConsole.Log("<color=red>ShotgunRecoil()</color>");
        
        rifleTargetIK_R.localPosition += Vector3.back * shotgunRecoilPosAmount;
        rifleTargetIK_R.Rotate(vRight * shotgunRecoilRotAmount, Space.Self);
        
        // rifleTargetIK_L.localPosition += Vector3.back * handScalePistolPosFire;
        // rifleTargetIK_L.Rotate(Vector3.right * handScalePistolRotFire, Space.Self);
    }
    
    
    public void DoBolterRecoilOnce()
    {
        //InGameConsole.Log("<color=red>ShotgunRecoil()</color>");
        
        rifleTargetIK_R.localPosition += Vector3.back * 0.01F;
        rifleTargetIK_R.Rotate(vRight * bolterRecoilRotAmount, Space.Self);
        
        // rifleTargetIK_L.localPosition += Vector3.back * handScalePistolPosFire;
        // rifleTargetIK_L.Rotate(Vector3.right * handScalePistolRotFire, Space.Self);
    }
    
    public void DoPistolRecoilOnce()
    {
        pistolTargetIK_R.localPosition += Vector3.back * pistolRecoilPosAmount;
        pistolTargetIK_R.Rotate(vRight * pistolRecoilRotAmount, Space.Self);
    }

    void EvaluateTargetIK(float tremorFreqMultipler)
    {
        float dt = UberManager.DeltaTime();
        
        if(useTremor)
        {
            tremorTimer += dt * tremorFrequency * tremorFreqMultipler;
            
            tremorizedHandPosition = Vector3.up * tremorCurve.Evaluate(tremorTimer) * tremorScale;
            
            //Note(Marat): this is for really shaky hands:
            //tremorizedHandPosition = Random.onUnitSphere * tremorCurve.Evaluate(tremorTimer) * tremorScale;
        }
        else
        {
            tremorizedHandPosition = Vector3.zero;
        }
       
       
       rifleTargetIK_R.localPosition = Vector3.MoveTowards(rifleTargetIK_R.localPosition, rifleIK_R_originalLocalPosition, shotgunDampingPos * dt);
       rifleTargetIK_R.localRotation = Quaternion.RotateTowards(rifleTargetIK_R.localRotation, correctShotgunRotation, shotgunDampingRot * dt);
    //    rifleTargetIK_L.localPosition = Vector3.MoveTowards(rifleTargetIK_L.localPosition, rifleIK_L_originalLocalPosition, handDampPistolPosIK * dt);
        
       pistolTargetIK_R.localPosition = Vector3.MoveTowards(pistolTargetIK_R.localPosition, pistolIK_R_originLocalPosition + tremorizedHandPosition, pistolDampingPos * dt);
       pistolTargetIK_R.localRotation = Quaternion.RotateTowards(pistolTargetIK_R.localRotation, correctPistolRotation, pistolDampingRot * dt);
    }
    
    
    

    void MovementAnimation(Vector3 v, Vector3 dir)
    {
        float targetX, targetZ;
        Vector2 animDir = Vector2.up;

        Vector2 vNorm = new Vector2(v.x, v.z).normalized;
        Vector2 dNorm = new Vector2(dir.x, dir.z).normalized;

        // float dotproduct = Vector2.Dot(vNorm, dNorm);
        float angle = Vector2.SignedAngle(dNorm, vNorm);
        
        if(Math.Abs(angle) > 90f)
            angle = -angle;
        animDir = GetVectorRotated(animDir, angle);


        if(v.x == 0f && v.z == 0f)
        {
            targetX = 0f;
            targetZ = 0f;
        }
        else
        {
            targetX = animDir.x;
            targetZ = animDir.y;
        }




        float currentXv = animator.GetFloat(xVelocityHash);
        float currentZv = animator.GetFloat(zVelocityHash);


        //This is smooth animation:
        animator.SetFloat(xVelocityHash, targetX, movementDampValue, timeScale * Time.deltaTime);
        animator.SetFloat(zVelocityHash, targetZ, movementDampValue, timeScale * Time.deltaTime);

        //This is choppy animation, still good tho 12.08.2019
        // animator.SetFloat("xVelocity", targetX);
        // animator.SetFloat("zVelocity", targetZ);
        
    }

    Vector2 GetVectorRotated(Vector2 v, float angle = 90f)
    {
        angle *= Mathf.Deg2Rad;
        float sin = Mathf.Sin(angle);
        float cos = Mathf.Cos(angle);

        Vector2 r = new Vector2();
        r.x = v.x * cos - v.y * sin;
        //r.y = v.y;
        r.y = v.x * sin + v.y * cos;

        return r;
    }

    
    void CorrectRightHand()
    {
        //-44 -25 28


        // pistolTargetIK_R.localRotation  = Quaternion.Euler(correctEulerPistol);
        pistolTargetIK_R.localRotation  = correctPistolRotation;
    }

    void ReleaseIK_r_hand()
    {
        ik_r_hand.Target = null;
        pistolTargetIK_R.localRotation = Quaternion.identity;
    }
    
    void ReleaseIK_l_hand()
    {
        ik_l_hand.Target = null;
        
    }


    [PunRPC]
    void PlayAnimation(int animation_cmd)
    {
        PlayerAnimationCommand cmd = (PlayerAnimationCommand)animation_cmd;
        switch(cmd)
        {
            case PlayerAnimationCommand.UNWIELD:
                ReleaseIK_r_hand();
                ReleaseIK_l_hand();

             
                if(!main_photonView.IsMine)
                {
                 //   gunController.WieldWeapon(WeaponManager.Singleton().GetWeapon(EntityType.UNDEFINED_ENTITY));
                }
                break;

            case PlayerAnimationCommand.Wield_Pistol:
                ReleaseIK_l_hand();
                
                ik_r_hand.Target = pistolTargetIK_R;
                // CorrectRightHand();

                if(!main_photonView.IsMine)
                {
                 //   gunController.WieldWeapon(WeaponManager.Singleton().GetWeapon(EntityType.GLOCK_ENTITY));
                }
                break;
            case PlayerAnimationCommand.Wield_Shotgun:
            {
                ik_r_hand.Target = rifleTargetIK_R;
                //ik_l_hand.Target = rifleTargetIK_L;
                
                
                if(!main_photonView.IsMine)
                {
                 //   gunController.WieldWeapon(WeaponManager.Singleton().GetWeapon(EntityType.SHOTGUN_ENTITY));
                }
                break;
            }
            case PlayerAnimationCommand.Wield_HeavyBolter:
            {
                ik_r_hand.Target = rifleTargetIK_R;
                //ik_l_hand.Target = rifleTargetIK_L;
                
                
                if(!main_photonView.IsMine)
                {
                  //  gunController.WieldWeapon(WeaponManager.Singleton().GetWeapon(EntityType.HEAVYBOLTER_ENTITY));
                }
                break;
            }
            case PlayerAnimationCommand.Wield_Crossbow:
            {
                ik_r_hand.Target = rifleTargetIK_R;
                //ik_l_hand.Target = rifleTargetIK_L;
                
                
                if(!main_photonView.IsMine)
                {
                 //   gunController.WieldWeapon(WeaponManager.Singleton().GetWeapon(EntityType.CROSSBOW_ENTITY));
                }
                break;
            }
            case PlayerAnimationCommand.Wield_Revolver:
            {
                ik_r_hand.Target = rifleTargetIK_R;
                //ik_l_hand.Target = rifleTargetIK_L;
                
                
                if(!main_photonView.IsMine)
                {
                 //   gunController.WieldWeapon(WeaponManager.Singleton().GetWeapon(EntityType.REVOLVER_ENTITY));
                }
                break;
            }
            case PlayerAnimationCommand.Wield_RocketLauncher:
            {
                ik_r_hand.Target = rifleTargetIK_R;
                //ik_l_hand.Target = rifleTargetIK_L;
                
                
                if(!main_photonView.IsMine)
                {
                 //   gunController.WieldWeapon(WeaponManager.Singleton().GetWeapon(EntityType.ROCKETLAUNCHER_ENTITY));
                }
                break;
            }
            default:
            {
                // ReleaseIK_r_hand();
                // ReleaseIK_l_hand();

                break;
            }
        }
    }

    void HandsAnimation()
    {
        // Weapon currentWeaponInSlot = gunController.GetWeaponInSlot();
        // if(weaponInPrevFrame != currentWeaponInSlot)
        // {
        //     if(currentWeaponInSlot == null)
        //     {
        //         main_photonView.RPC("PlayAnimation", RpcTarget.Others, (int)PlayerAnimationCommand.UNWIELD);
        //         PlayAnimation((int)PlayerAnimationCommand.UNWIELD);   
        //     }
        //     else
        //     {
        //         switch(currentWeaponInSlot.ent_type)
        //         {
        //             case(EntityType.GLOCK_ENTITY):
        //             {
        //                 main_photonView.RPC("PlayAnimation", RpcTarget.Others, (int)PlayerAnimationCommand.Wield_Pistol);
        //                 PlayAnimation((int)PlayerAnimationCommand.Wield_Pistol);        
                        
        //                 break;
        //             }
        //             case(EntityType.SHOTGUN_ENTITY):
        //             {
        //                 // InGameConsole.Log("<color=green>Wield shotgun entity</color>");
        //                 main_photonView.RPC("PlayAnimation", RpcTarget.Others, (int)PlayerAnimationCommand.Wield_Shotgun);
        //                 PlayAnimation((int)PlayerAnimationCommand.Wield_Shotgun);        
                        
        //                 break;
        //             }
        //             case(EntityType.CROSSBOW_ENTITY):
        //             {
        //                 // InGameConsole.Log("<color=green>Wield shotgun entity</color>");
        //                 main_photonView.RPC("PlayAnimation", RpcTarget.Others, (int)PlayerAnimationCommand.Wield_Crossbow);
        //                 PlayAnimation((int)PlayerAnimationCommand.Wield_Crossbow);        
                        
        //                 break;
        //             }
        //             case(EntityType.HEAVYBOLTER_ENTITY):
        //             {
        //                 main_photonView.RPC("PlayAnimation", RpcTarget.Others, (int)PlayerAnimationCommand.Wield_HeavyBolter);
        //                 PlayAnimation((int)PlayerAnimationCommand.Wield_HeavyBolter);
                        
        //                 break;
        //             }
        //             case(EntityType.REVOLVER_ENTITY):
        //             {
        //                 main_photonView.RPC("PlayAnimation", RpcTarget.Others, (int)PlayerAnimationCommand.Wield_Revolver);
        //                 PlayAnimation((int)PlayerAnimationCommand.Wield_Revolver);
                        
        //                 break;
        //             }
        //             case(EntityType.ROCKETLAUNCHER_ENTITY):
        //             {
        //                 main_photonView.RPC("PlayAnimation", RpcTarget.Others, (int)PlayerAnimationCommand.Wield_RocketLauncher);
        //                 PlayAnimation((int)PlayerAnimationCommand.Wield_RocketLauncher);
                        
        //                 break;
        //             }
        //         }
        //     }
        // }
        // weaponInPrevFrame = gunController.GetWeaponInSlot();
    }
}
