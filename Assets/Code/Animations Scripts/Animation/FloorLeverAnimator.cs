using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorLeverAnimator : MonoBehaviour, Interactable
{
    
    
    
    public GameObject[] interactablesToCall;
    
    public enum LeverState : int
    {
        ON,
        OFF    
    }
    
    public bool isWorking = false;
    
    
    public float gearsHolderRotSpeed = 180;
    
    public AudioSource audio_source;
    
    
    Vector3 localPos_on = new Vector3(0, 0, 0);
    Vector3 localPos_off = new Vector3(0, 0.269f, 0);
    
    public Material on_mat;
    public Material off_mat;
    
    public ParticleSystem on_ps;
    public ParticleSystem off_ps;
    
    public MeshRenderer indicator_rend;
    
    public Transform gears_holder;
    public Transform gear1_l;
    public Transform gear2_l;
    public Transform gear3_r;
    public Transform gear4_r;
    
    public LeverState state;
    
    public float timeToAnimate = 0.6f;
    public float rotationAmount = 100f;
    public float gear1Multiplier = 3f;
    
    public bool interactableOnlyOnce = false;
    public bool callInteractablesOnTurningOff = false;
    
    float rotationSpeed = 0f;
    float distanceTravelled = 0f;
    
    void Start()
    {
        rotationSpeed = rotationAmount / timeToAnimate;
    }
    
    static Vector3 axis = Vector3.right;
    
    void OnLeverTurnedOn()
    {
        state = LeverState.ON;
        if(interactablesToCall != null)
        {
            for(int i = 0; i < interactablesToCall.Length; i++)
            {
                if(interactablesToCall[i] != null)
                {
                    interactablesToCall[i].GetComponent<Interactable>().Interact();
                }
                //GameObject fancy = ObjectPool.s().Get(ObjectPoolKey.InteractableFancy1);
                //Vector3 fancy_pos = interactablesToCall[i].transform.position;
                
                //fancy_pos.y = transform.position.y + 0.6f;
                
                //fancy.GetComponent<Interactable_ps_fancy1>().SetDestination(transform.position + Vector3.up * 0.6f, fancy_pos, 12);
            }
            
            
        }
        
        if(interactableOnlyOnce)
        {
            this.enabled = false;
        }
        
        // if(audio_source)
        // {
        //     audio_source.Play();
        // }
        
        indicator_rend.sharedMaterial = on_mat;
        
        
        // InGameConsole.Log(string.Format("Lever turned <color=green>ON</color>"));
    }
    
    void OnLeverTurnedOff()
    {
        state = LeverState.OFF;
        
        if(callInteractablesOnTurningOff)
        {
            if(interactablesToCall != null)
            {
                for(int i = 0; i < interactablesToCall.Length; i++)
                {
                    interactablesToCall[i].GetComponent<Interactable>().Interact();
                }
            }
        }
        
        indicator_rend.sharedMaterial = off_mat;
        // InGameConsole.Log(string.Format("Lever turned <color=red>OFF</color>"));
    }
    
    
    
    
    public void Interact()
    {        
        if(isWorking)
        {
            return;
        }
        else
        {
            switch(state)
            {
                case(LeverState.OFF):
                {
                    indicator_rend.transform.localPosition = localPos_on;
                    on_ps.Stop();
                    on_ps.Play();
                    break;
                }
                case(LeverState.ON):
                {
                    indicator_rend.transform.localPosition = localPos_off;
                    off_ps.Stop();
                    off_ps.Play();
                    break;
                }
            }
            isWorking = true;
        }
    }
    
    readonly static Vector3 vUp = new Vector3(0, 1, 0);
    
    public void Update()
    {
        // if(Input.GetKeyDown(KeyCode.E))
        // {
        //     Interact();
        // }
        float dt = UberManager.DeltaTime();
        
        if(isWorking)
        {
            float direction = 1f;
            if(state == LeverState.OFF)
            {
                direction = -1f;
            }
            
            Vector3 rotation = direction * axis * rotationSpeed;
            
            distanceTravelled += rotationSpeed * dt;
            
            if(distanceTravelled > rotationAmount)
            {
                distanceTravelled = 0f;
                isWorking = false;
                
                if(state == LeverState.OFF)
                {
                    OnLeverTurnedOn();
                }
                else
                {
                    OnLeverTurnedOff();
                }
            }
            
            Vector3 dr = rotation * gear1Multiplier * dt;
            
            gears_holder.Rotate(vUp * direction * gearsHolderRotSpeed * dt);
            
            gear1_l.Rotate(-dr, Space.Self);
            gear2_l.Rotate(-dr, Space.Self);
            gear3_r.Rotate(dr, Space.Self);
            gear4_r.Rotate(dr, Space.Self);
        }
        else
        {
            Vector3 rotation = axis * -20;
            Vector3 dr = rotation * gear1Multiplier * dt;
            
            
            gear1_l.Rotate(-dr, Space.Self);
            gear2_l.Rotate(-dr, Space.Self);
            gear3_r.Rotate(dr, Space.Self);
            gear4_r.Rotate(dr, Space.Self);
        }
    }
}
