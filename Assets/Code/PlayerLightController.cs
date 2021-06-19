using UnityEngine;

public enum PlayerLightState : int
{
    Normal,
    Rage_1
}

public class PlayerLightController : MonoBehaviour
{
    Light playerLight;
    
    public PlayerLightState state;
    
    
    float normalIntensity = 0;
    public float normalRange = 8.2f;
    public float normalIntensityRate = 5f;
    public float normalMaxIntensity = 3.2f;
    public Color normalColor = Color.yellow;
    public float normalColorChangeRate = 1.5f;
    
    
    void Awake()
    {
        playerLight = GetComponent<Light>();
    }
    
    public void ShootRocketLauncher()
    {
        playerLight.intensity += 1.55f;
        playerLight.intensity = Mathf.Clamp(playerLight.intensity, 0f, normalMaxIntensity);
    }
    
    public void ShootRevolver()
    {
        playerLight.intensity += 1.55f;
        playerLight.intensity = Mathf.Clamp(playerLight.intensity, 0f, normalMaxIntensity);
        // playerLight.color = new Color(playerLight.color.r + 0.07f, playerLight.color.g, playerLight.color.b, 1);
    }
    
    public void ShootBolter()
    {
        
        playerLight.intensity += 1.25f;
        playerLight.intensity = Mathf.Clamp(playerLight.intensity, 0f, normalMaxIntensity);
        // playerLight.color = new Color(playerLight.color.r + 0.15f, playerLight.color.g, playerLight.color.b, 1);
    }
    
    public void ShootShotgun()
    {
        
        playerLight.intensity += 1.95f;
        playerLight.intensity = Mathf.Clamp(playerLight.intensity, 0f, normalMaxIntensity);
        // playerLight.color = new Color(playerLight.color.r + 0.07f, playerLight.color.g, playerLight.color.b, 1);
    }
    
    void Start()
    {
        // playerLight.enabled = false;
    }
    
    float vel;
    public float smoothTime = 0.1f;
    
    void Update()
    {
        float dt = UberManager.DeltaTime();
        
        switch(state)
        {
            case PlayerLightState.Normal:
            {
                //playerLight.intensity = Mathf.MoveTowards(playerLight.intensity, normalIntensity, normalIntensityRate * dt);
                playerLight.intensity =  Mathf.SmoothDamp(playerLight.intensity, normalIntensity, ref vel, smoothTime);
                
                // float dir = 1f;
                
                // float r = playerLight.color.r;
                // if(r > normalColor.r)
                // {
                //     dir = -1f;
                // }
                // playerLight.color = new Color(playerLight.color.r + dir * r * dt * normalColorChangeRate, playerLight.color.g, playerLight.color.b, 1);
                
                break;
            }
            case PlayerLightState.Rage_1:
            {
                
                
                break;
            }
        }
    }
}
