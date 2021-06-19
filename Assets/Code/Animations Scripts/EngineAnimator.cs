using UnityEngine;

public enum EngineState { Off, Activating, On, Deactivating}

[RequireComponent(typeof(Animator))]
public class EngineAnimator : MonoBehaviour, Interactable
{

    

    public float activatingTime = 5f;
    public float deactivatingTime = 2f;

    public float working_speed = -1f;
    public float current_speed = 0f;


    float acceleration;
    float decceleration;

    public EngineState initial_state = EngineState.Off;
    EngineState state;

    Animator animator;

    void Start()
    {
        state = initial_state;
        animator = GetComponent<Animator>();
        animator.SetFloat("Speed", current_speed);

        acceleration = -working_speed / activatingTime;
        decceleration = -working_speed / deactivatingTime;

        source = GetComponent<AudioSource>();
    }

    [Header("Sounds:")]
    public AudioClip workBegin;
    public AudioClip stableWorkFinished;
    AudioSource source;


    //[PunRPC]
    public void Interact()
    {
        //SwitchState();
        ToggleActiveness();
    }

    void Update()
    {

        Process_State();
    }

    //This works every Update()
    void Process_State()
    {
        switch(state)
        {
            
            case EngineState.Off: break;
            case EngineState.On: break;
            
            case EngineState.Activating:
                current_speed = Mathf.MoveTowards(current_speed, working_speed, acceleration * Time.deltaTime);
                animator.SetFloat("Speed", current_speed);
                
                
                if(current_speed == working_speed)
                    SwitchState();
                break;
            case EngineState.Deactivating:
                current_speed = Mathf.MoveTowards(current_speed, 0f, decceleration * Time.deltaTime);
                animator.SetFloat("Speed", current_speed);
                if(current_speed == 0f)
                    SwitchState();
                break;
        }
    }

    void ToggleActiveness()
    {
        switch(state)
        {
            case EngineState.Off:
            {
                state = EngineState.Activating;
                    OnWorkBegin();
                //interactable_by_player = false;
                break;
            }
            case EngineState.Activating:
            {
                state = EngineState.Deactivating;
                break;
            }
            case EngineState.On:
            {
                OnStableWorkFinished();
                state = EngineState.Deactivating;
                break;
            }
            case EngineState.Deactivating:
            {
                state = EngineState.Activating;
                break;
            }
        }
    }

    void OnWorkBegin()
    {
        
        // print("OnWorkBegin");
        source.PlayOneShot(workBegin);
    }

    void OnStableWorkBegin()
    {
        source.Play();
        print("OnStableBegin");
    }

    void OnStableWorkFinished()
    {
        source.Stop();
        source.PlayOneShot(stableWorkFinished);
        print("OnStableFinished");
    }

    void OnWorkFinished()
    {
        
        // print("OnWorkFinished");
    }


    void SwitchState()
    {
        switch(state)
        {
            case EngineState.Off:
            {
                state = EngineState.Activating;
                
                //interactable_by_player = false;
                break;
            }
            case EngineState.Activating:
            {
                OnStableWorkBegin();
                state = EngineState.On;
                break;
            }
            case EngineState.On:
            {
                
                state = EngineState.Deactivating;
                break;
            }
            case EngineState.Deactivating:
            {
                state = EngineState.Off;
                OnWorkFinished();
                break;
            }
        }

    }
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        GUIStyle style = new GUIStyle();
        style.alignment = TextAnchor.MiddleCenter;
        style.normal.textColor = Color.yellow;
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.75f, this.gameObject.name +  "\n" + this.state.ToString() + " " + this.current_speed.ToString(), style);
    }
#endif
}
