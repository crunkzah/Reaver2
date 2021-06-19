using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using UnityEngine.Animations;

public enum state_ZOMBIE_1 { IDLE, CURIOUS, CHASING, ATTACKING}


public class EnemyController : MonoBehaviour, ISoundListener 
{
    // Warning: npcProfile must be set in editor !
    public EnemyAiSettings npcProfile;

    NavMeshAgent agent;
    [SerializeField] Transform target;
    Vector3 initialPos;
    //Vector3 destination;

    //TODO: Wrap npc settings in scriptableObject
    public float viewDistance = 8f;

    [SerializeField] state_ZOMBIE_1 state = state_ZOMBIE_1.IDLE;

    Coroutine aiCoroutine;
    

    public void SetInitialPos(Vector3 pos)
    {
        initialPos = pos;
    }


    // void OnTakeDamage()
    // {
    //     agent.enabled = false;
    // }
    
    [SerializeField] MeshCollider interactionCollider;

	void Start ()
    {
        if (npcProfile == null)
            Debug.LogError("npcProfile scriptableObject not set!");
        else
        {
            agent = GetComponent<NavMeshAgent>();
            aiCoroutine = StartCoroutine(UpdateAiPath());
            SetInitialPos(transform.position);
        }

        if(interactionCollider == null)
        {
            Debug.LogError("InteractionCollider not set on " + this.gameObject.name);
        }
        
	}


    public void OnDeath(Vector3 force)
    {
        var ragdoll = GetComponent<RagdollController>();
        if(ragdoll != null)
        {
            ragdoll.ToggleRagdoll(false);
            ragdoll.ApplyForceToRootBone(force);
        }

        agent.enabled = false;
        var npcAnimator = GetComponent<NPCAnimationController>();
        if(npcAnimator != null)
        {
            npcAnimator.animator.enabled = false;
            npcAnimator.enabled = false;
        }

        

        interactionCollider.enabled = false;

        Debug.Log("<color=orange>OnDeath</color>");

        Destroy(agent);
        Destroy(npcAnimator);
        Destroy(this);
    }

    float GetRefreshRate(Vector3 targetPos)
    {
        float distance = Vector3.Distance(transform.position, targetPos);
        
        if(distance < 3f)
            return 0.125f;

        if(distance < 5f)
            return 0.15f;

        return GameSettings.Singleton().aiRefreshRate;
    }

    private void OnDisable()
    {
        if(aiCoroutine != null)
            StopCoroutine(aiCoroutine);
    }

    [SerializeField] float time_next_idle = 0f;


    float ViewRadius
    {
        get
        {
            if (this.state == state_ZOMBIE_1.IDLE)
                return npcProfile.viewRadiusIdle;
            else
                return npcProfile.viewRadiusChase;
        }
    }
    public bool CheckDirectVision(Vector3 seekerPos, Transform target, LayerMask mask, float maxDistance)
    {
        bool result = false;

        RaycastHit hit;
        Vector3 dir = (target.position + Vector3.up - seekerPos).normalized;
        Ray ray = new Ray(seekerPos, dir);
        if(Physics.Raycast(ray, out hit, maxDistance, mask))
        {
            if (hit.collider.transform == target)
                result = true;
            //Debug.Log(hit.collider.name);
        }

        Color col = result ? Color.red : Color.blue;
        float distance = result ? hit.distance : maxDistance;
        Debug.DrawRay(ray.origin, ray.direction * distance, col);

        return result;
    }
    //Here we update path and change state:
    IEnumerator UpdateAiPath()
    {
        while(true)
        {
            if (GameSettings.Singleton().ai_Disabled)
                yield return new WaitForSeconds(GameSettings.Singleton().aiRefreshRate);

            Collider[] targets = Physics.OverlapSphere(transform.position, ViewRadius, GameSettings.Singleton().enemyTargetMask);
            gizmoSphere = new BoundingSphere(transform.position, ViewRadius);

            if(targets.Length > 0)
            {
                target = targets[0].transform;

                if (this.state == state_ZOMBIE_1.IDLE || this.state == state_ZOMBIE_1.CURIOUS)
                {
                    //bool isSeeing = EnemyAiManager.CheckDirectVision(transform.position, target, GameSettings.singleton.enemyVisionMask, ViewRadius);
                    
                    bool isSeeing = CheckDirectVision(transform.position, target, GameSettings.Singleton().enemyVisionMask, ViewRadius);

                    if (isSeeing)
                    {
                        NavMeshPath path = new NavMeshPath();
                        agent.CalculatePath(target.position, path);
                        if(path.status != NavMeshPathStatus.PathComplete)
                        {
                            SwitchState(state_ZOMBIE_1.IDLE);
                            Debug.Log("<color=red>Destination is not reachable</color>");
                        }
                        else
                        {
                            agent.destination = target.position;
                            SwitchState(state_ZOMBIE_1.CHASING);
                        }

                    }
                }
                else
                {
                    if (this.state == state_ZOMBIE_1.CHASING )
                    {
                            agent.SetDestination(target.position);
                    }
                }

            }
            else
            {
                if (this.state == state_ZOMBIE_1.CHASING)
                {
                    target = null;
                    SwitchState(state_ZOMBIE_1.IDLE);
                }
            }
            //Chase player if we know were he is

            //Attack player if we in range of attack

            yield return new WaitForSeconds(GameSettings.Singleton().aiRefreshRate);
        }
    }


    public bool drawGizmos = true;
    BoundingSphere gizmoSphere;
    
    private void OnDrawGizmos()
    {
        if (drawGizmos == false)
            return;
        Gizmos.DrawWireSphere(gizmoSphere.position, gizmoSphere.radius);
    }


    void SwitchState(state_ZOMBIE_1 newState)
    {
        if (this.state == newState)
            return;

        switch(newState)
        {
            case state_ZOMBIE_1.IDLE:
                agent.isStopped = false;
                agent.speed = npcProfile.walkSpeed;
                agent.angularSpeed = npcProfile.walkAngularSpeed;
                break;
            case state_ZOMBIE_1.CURIOUS:
                agent.isStopped = false;
                agent.speed = npcProfile.curiousSpeed;
                agent.angularSpeed = npcProfile.curiousAngularSpeed;

                time_exit_curious = Time.time + curiousDuration;
                break;
            case state_ZOMBIE_1.CHASING:
                agent.isStopped = false;
                agent.speed = npcProfile.runSpeed;
                agent.angularSpeed = npcProfile.runAngularSpeed;
                break;
            case state_ZOMBIE_1.ATTACKING:
                agent.isStopped = true;
                
                agent.speed = 0;
                break;
        }

        Debug.Log("Switching state of " + this.gameObject.name + " from " + this.state.ToString() + " to " + newState.ToString());
        this.state = newState;
    }

    [Header("Debug vars:")]
    [SerializeField] float distanceToTarget;
    [SerializeField] float time_next_attack;

    public void SetAgentDestSpeed(Vector3 destination, float speed, float angularSpeed)
    {
        this.agent.destination = destination;
        this.agent.speed = speed;
        this.agent.angularSpeed = angularSpeed;
    }

    public void GetNotified(Vector3 pos)
    {
        //TODO: Maybe notify nearby ai's to attack player(like swarm)
        switch(this.state)
        {
            case state_ZOMBIE_1.IDLE:
                agent.destination = pos;
                SwitchState(state_ZOMBIE_1.CURIOUS);
                break;
            case state_ZOMBIE_1.CURIOUS:
                agent.destination = pos;
                break;
            default:
                break;
        }
    }


    [SerializeField] float curiousDuration = 8f;
    [SerializeField] float time_exit_curious;
    private void Update()
    {
        if (GameSettings.Singleton().ai_Disabled)
            return;
            
        switch(this.state)
        {
            case state_ZOMBIE_1.IDLE:
                if(Time.time > time_next_idle)
                {

                    //TODO: Sync this:
                    time_next_idle = Time.time + Random.Range(0f, npcProfile.idleChangePositionRate);

                    //agent.speed = npcProfile.walkSpeed;
                    Vector3 destination;

                    //EnemyAiManager.singleton.RandomPoint(transform.position, npcProfile.idleWalkRadius, out destination);
                    NPCManager.RandomPoint(initialPos, npcProfile.idleWalkRadius, out destination);
                    //SetAgentDestSpeed should be RPC
                    SetAgentDestSpeed(destination, npcProfile.walkSpeed, npcProfile.walkAngularSpeed);
                }
                break;
            case state_ZOMBIE_1.CURIOUS:
                if (Time.time > time_exit_curious)
                {
                    SwitchState(state_ZOMBIE_1.IDLE);
                }

                break;
            case state_ZOMBIE_1.CHASING:
                if (target == null)
                {
                    Debug.LogWarning(this.gameObject.name + " is trying to chase null target...");
                    return;
                }
                //EnemyAiManager.CheckDirectVision(transform.position, target, GameSettings.singleton.enemyVisionMask, npcProfile.viewRadius);
                if ((transform.position - target.position).sqrMagnitude < npcProfile.attackRange * npcProfile.attackRange)
                {
                    SwitchState(state_ZOMBIE_1.ATTACKING);
                }
                
                

                break;
            case state_ZOMBIE_1.ATTACKING:

                if (target == null) //hack when player dies
                    break;

                if ((transform.position - target.position).sqrMagnitude > npcProfile.attackRange * npcProfile.attackRange)
                {
                    //Debug.Log("Distance is too high");
                    SwitchState(state_ZOMBIE_1.CHASING);
                }
                else
                    if(Time.time > time_next_attack)
                    {
                        time_next_attack = Time.time + npcProfile.attackRate;
                    //Debug.Log("DEALING DAMAGE");
                        target.GetComponent<LivingObject>().TakeDamage(npcProfile.damageDealt, Vector3.zero);
                    }

                break;
        }
    }
    

}
