using UnityEngine;

public class BasementHatchAnimator : MonoBehaviour, Interactable
{
    public Transform hatch;
    public bool isWorking = false;
    
    public float speed = 1.25f;
    static Quaternion closedRotation = new Quaternion(0f, 0f, 0f, 1f);
    static Quaternion openRotation = new Quaternion(0f, 0f, -0.9238795f, 0.3826834f); // vec3(0, 0, -135)
    Quaternion targetRotation;
    Quaternion startRotation;
    
    void Start()
    {
        if(hatch == null)
        {
            hatch = transform.GetChild(0);
        }
        
        startRotation = closedRotation;
        targetRotation = closedRotation;
    }
    
    [SerializeField] float t;
    
    public void Interact()
    {
        if(isWorking)
        {
            return;
        }
        
        isWorking = true;
        // isOpen = !isOpen;
        if(targetRotation == closedRotation)
        {
            targetRotation = openRotation;
            startRotation = closedRotation;
        }
        else
        {
            targetRotation = closedRotation;
            startRotation = openRotation;
            
        }
        
        
        t = 0f;
    }
    
    
    
    void Update()
    {
        // if(Input.GetKeyDown(KeyCode.E))
        // {
        //     Interact();
        // }
        
        if(isWorking)
        {
            t += speed * Time.deltaTime;
            
            if(t >= 1f)
            {
                isWorking = false;
            }
            
            t = Math.Clamp01(t);
            
            
            float funkyT;
            
            if(targetRotation == openRotation)
            {
                funkyT = Mathf.SmoothStep(0f, 1f, t);    
            }
            else
            {
                funkyT = Math.SmoothHyperbola2(t);
                // funkyT = Math.UglyBounce(t);
            }
            
            hatch.localRotation = Quaternion.Slerp(startRotation, targetRotation, funkyT);
        }
    }
    
    
}
