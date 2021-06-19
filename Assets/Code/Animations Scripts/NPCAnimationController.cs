using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCAnimationController : MonoBehaviour
{
    public float ratio = 1f;
    public Animator animator;
    NavMeshAgent agent;

    void Awake()
    {
        Init();
    }

    public float vel;
    public float speedPercent;
    void Update()
    {
        vel = agent.velocity.magnitude;
        speedPercent = vel / agent.speed;
        animator.SetFloat("Velocity", vel * ratio);
        animator.SetFloat("MoveSpeed", vel * ratio);
    
    }

    public void Init()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        
        if(animator == null)
        {
            Debug.LogError("No animator in children on " + this.gameObject.name);
        }
        // 0 - velocity float in animator
        //animator.GetFloat(0);
    }

}
