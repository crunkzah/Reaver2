using UnityEngine;

public enum HookState : byte
{
    None,
    Launched,
    Hooked,
    PullingBack
}

public class GrapplingHook : MonoBehaviour
{
    public Transform target;
    IKillableThing target_ikillable;
    public CapsuleCollider target_col;
    PlayerController  master;
    bool isMine;
    
    public HookState state;
    
    
    public Transform hookTPS;
    ParticleSystem.EmissionModule emission;
    ParticleSystem hook_ps;
    MeshRenderer rendHookTPS;
    LineRenderer hook_lr;
    Transform sensor;
    Vector3 hookDir;
    
    const float sensorFlySpeed = 110;
    //Vector3 launchDir;
    
    static int hookMask = -1;
    static int obstacleMask = -1;
    
    void Awake()
    {
        if(hookMask == -1)
        {
            hookMask = LayerMask.GetMask("NPC2");
            obstacleMask = LayerMask.GetMask("Ground", "Ceiling");
        }
        
        
        hook_lr = hookTPS.GetComponent<LineRenderer>();
        hook_lr.enabled = false;
        hook_lr.SetPositions(new Vector3[]{new Vector3(0,0,0), new Vector3(0, 0, 0)});
        rendHookTPS = hookTPS.GetComponent<MeshRenderer>();
        hook_ps = hookTPS.GetComponent<ParticleSystem>();
        emission = hook_ps.emission;
        emission.rateOverDistance = 0;
        rendHookTPS.enabled = false;
    }
    
    void OnGUI()
    {
        return;
        if(isMine)
        {
            GUIStyle style = new GUIStyle();
            style.alignment = TextAnchor.MiddleCenter;
            
            float w = 350;
            float h = 50;
            
            style.normal.textColor = Color.green;
            GUI.Label(new Rect(Screen.width/2 - w/2, Screen.height - h*2.9f, w, h), "HookState: " + state.ToString());
            GUI.Label(new Rect(Screen.width/2 - w/2, Screen.height - h*2.6f, w, h), "hookDir: " + hookDir.ToString());
        }
    }
    
    public void LaunchHook(PlayerController _master, Vector3 _hookDir, bool _isMine)
    {
        master = _master;
        //target = _target;
        isMine = _isMine;
        
        if(!sensor)
        {
            sensor = new GameObject(string.Format("HookSensor ({0})", isMine ? "Local" : "Remote")).transform;
        }
        
        hookDir = _hookDir;
        hookTPS.SetParent(null);
        
        sensor.localPosition = _master.GetCenterPosition();
        
        launchedTimer = 0;
        hookedTimer = 0;
        hook_lr.enabled = true;
        
        
        state = HookState.Launched;
    }
    
    float pullingForce = 36;
    
    public void HookToTarget(PlayerController _master, CapsuleCollider _target_col)
    {
        target_col = _target_col;
        target = _target_col.transform;
        
        sensor.position = target_col.center + target.transform.position;
        state = HookState.Hooked;
        if(isMine)
        {
            master.gunController.fastWeaponSwitch = true;
        }
        target_ikillable = _target_col.GetComponent<IKillableThing>();
    }
    
    public void PullHookBack()
    {
        
    }
    
    public void ReleaseHook()
    {
        master = null;
        target = null;
        state = HookState.None;
    }
    
    float launchedTimer = 0;
    const float maxFlyTime = 1.5f;
    float hookedTimer = 0;
    const float maxHookedTime = 2;
    
    
    
    
    void Update()
    {
       
        switch(state)
        {
            case(HookState.None):
            {
                if(hook_lr)
                {
                    hook_lr.enabled = false;
                    //hook_lr.emitting = false;
                    rendHookTPS.enabled = false;
                    emission.rateOverDistance = 0;
                }
                break;
            }
            case(HookState.Launched):
            {
                if(master)
                {
                    Ray ray = new Ray(sensor.localPosition, hookDir);
                    RaycastHit hit;
                    
                    float dt = UberManager.DeltaTime();
                    launchedTimer += dt;
                    
                    if(Physics.Raycast(ray, out hit, sensorFlySpeed * dt, obstacleMask))
                    {
                        if(isMine)
                        {
                            Vector3 boostDir = (hit.point - master.GetCenterPosition()).normalized;
                            master.BoostVelocity(boostDir * 32, false);
                        }
                        ParticlesManager.Play(ParticleType.shot, hit.point, Vector3.forward);
                        InGameConsole.LogFancy(string.Format("Hook hit {0}, pulling back", hit.collider.gameObject.name));
                        state = HookState.PullingBack;
                        break;
                    }
                    
                    //if(Physics.Raycast(ray, out hit, sensorFlySpeed * dt, hookMask))
                    if(Physics.SphereCast(ray, 0.33f, out hit, sensorFlySpeed * dt, hookMask))                    
                    {
                        InGameConsole.LogFancy(string.Format("Hook hit {0}, hooking to him", hit.collider.gameObject.name));
                        CapsuleCollider npcCol = hit.collider.GetComponent<CapsuleCollider>();
                        if(npcCol)
                        {
                            HookToTarget(master, npcCol);
                        }
                    }
                    
                    sensor.Translate(hookDir * sensorFlySpeed * dt, Space.World);
                    hookTPS.position = sensor.position;
                    hookTPS.forward = hookDir;
                    
                    if(launchedTimer >= maxFlyTime)
                    {
                        state = HookState.PullingBack;
                    }
                    
                    if(hook_lr && master)
                    {
                        hook_lr.SetPosition(0, master.GetCenterPosition());
                        hook_lr.SetPosition(1, sensor.position - hookDir * 0.4f);
                        //hook_lr.emitting = true;
                        rendHookTPS.enabled = true;
                        emission.rateOverDistance = 0;
                    }
                }
                
                
                break;
            }
            case(HookState.Hooked):
            {
                if(master)
                {
                    float dt = UberManager.DeltaTime();
                    hookedTimer += dt;
                    
                    if(hookedTimer > maxHookedTime)
                    {
                        state = HookState.PullingBack;
                    }
                    
                    if(target_col)
                    {
                        Vector3 targetPos = target.position + target_col.center;
                        Vector3 dirToTarget = (targetPos - master.GetCenterPosition()).normalized;
                        master.fpsVelocity  = dirToTarget * pullingForce;
                        
                        if(Math.SqrDistance(master.GetCenterPosition(), target_col.ClosestPointOnBounds(master.GetCenterPosition())) < 1f)
                        {
                            master.fpsVelocity = new Vector3(0, 0, 0);
                            state = HookState.PullingBack;
                            master.secondJumpHappened = false;
                            //ReleaseHook();
                        }
                        if(!target_ikillable.CanBeBounceHit())
                        {
                            master.fpsVelocity = new Vector3(0, 0, 0);
                            state = HookState.PullingBack;
                            master.secondJumpHappened = false;
                        }
                    }
                    else
                    {
                        state = HookState.PullingBack;
                        
                    }
                    if(hook_lr && master)
                    {
                        hook_lr.SetPosition(0, master.GetCenterPosition());
                        hook_lr.SetPosition(1, sensor.position - hookDir * 0.4f);
                        emission.rateOverDistance = 0;
                        rendHookTPS.enabled = true;
                    }
                }
                
                
                break;
            }
            
            case(HookState.PullingBack):
            {
                if(master)
                {
                    if(hookTPS)
                    {
                        float dt = UberManager.DeltaTime();
                        
                        Vector3 dirToMaster = (master.GetCenterPosition() - hookTPS.position).normalized;
                        
                        if(Math.SqrDistance(master.GetCenterPosition(), hookTPS.position) < 0.33f * 0.33f)
                        {
                            hookTPS.SetParent(master.transform);
                            hookTPS.localPosition = Vector3.zero;
                            // if(master)
                            // {
                            //     gunController
                            //    OnBlueHookReturned();
                            //}
                            ReleaseHook();
                        }
                        
                        hookTPS.forward = -dirToMaster;
                        hookTPS.Translate(dirToMaster * sensorFlySpeed * 1.33f * dt, Space.World);
                    }
                    
                    if(hook_lr && master)
                    {
                        hook_lr.SetPosition(0, master.GetCenterPosition());
                        hook_lr.SetPosition(1, sensor.position - hookDir * 0.4f);
                        rendHookTPS.enabled = true;
                        emission.rateOverDistance = 1.5f;
                    }
                }
                
                break;
            }
        }
    }
}
