using UnityEngine;

public class PipingAnimator : MonoBehaviour, Interactable
{
    Material material_instance;
    void Start()
    {
        material_instance = GetComponent<MeshRenderer>().material;
        initialColor = material_instance.GetColor(EmissionColor);
        currentIntensity = isEmitting ? EmissionIntensityMinMax.y : EmissionIntensityMinMax.x;

        material_instance.SetColor(EmissionColor, initialColor * currentIntensity);

        speed = 1f / activatingTime;
    }

    static string EmissionColor = "_EmissionColor";

    public Vector2 EmissionIntensityMinMax = new Vector2(0, 0.75f);
    public float activatingTime = 0.75f;

    Color initialColor;
    float speed;
    
    
    public bool isEmitting = false;

    

    public void Interact() 
    {
        isEmitting = !isEmitting;
    }

    float targetIntensity, currentIntensity;
    
    void Update()
    {
        
        targetIntensity = isEmitting ? EmissionIntensityMinMax.y : EmissionIntensityMinMax.x;
        if(currentIntensity != targetIntensity)
        {
            currentIntensity = Mathf.MoveTowards(currentIntensity, targetIntensity, speed * Time.deltaTime);
            
            material_instance.SetColor(EmissionColor, initialColor * currentIntensity);
        }
        
        // if(isEmitting)
        // {
            
        //     // material_instance.SetColor(EmissionColor, initialColor * EmissionIntensityMinMax.y);
        //     material_instance.SetColor(EmissionColor, initialColor * Mathf.LinearToGammaSpace(emission));
        // }
        // else
        // {
        //     material_instance.SetColor(EmissionColor, initialColor * EmissionIntensityMinMax.x);
        // }
    }
}
