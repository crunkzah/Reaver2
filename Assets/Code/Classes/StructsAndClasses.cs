using UnityEngine;

[System.Serializable]
public struct Gear
{
    public Transform transform;
    public Vector3 axis;
    public float ratio;
}

// public struct Gear2
// {
//     public Quaternion rotation;
    
// }

public interface ITriggerable
{
    void OnTrigger();
}


public interface ISceneSyncable
{
    void OnSceneSynchronization();
}

public interface IDamagableLocal
{
    void TakeDamageLocally(int dmg, Vector3 hitPosition, Vector3 hitDirection);
    int GetCurrentHP();
    bool IsDead();
}

public interface ILaunchableAirbourne
{
    bool CanBeLaunched();
    bool CanBeLaunchedUp();
    bool IsCurrentlyAirborne();
}

public interface IRemoteAgent
{
    void RemoteAgentOnSpawn(Vector3 spawnPos);
}

public interface IPooledObject
{
    
    void InitialState();
}

public interface IActivatable
{
    void Activate();
    void Deactivate();
}

public interface IGuidable
{
    void SetSpawnDirection(Vector3 dir);
}

public interface ISpawnable
{
    void SetSpawnPosition(Vector3 pos);
}

public class GaussianDistribution 
{
 
  // Marsaglia Polar
 
   
    float _spareResult;
    bool _nextResultReady = false;
 
    public float Next() 
    {
        float result;
    
        if(_nextResultReady) 
        {
            result = _spareResult;
            _nextResultReady = false;
    
        }
        else
        {
            float s = -1f, x, y;
        
            do 
            {
                x = 2f * Random.value - 1f;
                y = 2f * Random.value - 1f;
                s = x * x + y * y;
            } while(s < 0f || s >= 1f);
        
            s = Mathf.Sqrt((-2f * Mathf.Log(s)) / s);
        
            _spareResult = y * s;
            _nextResultReady = true;
        
            result = x * s;
        }
    
        return result;
    }
 
  public float Next(float mean, float sigma = 1f) => mean + sigma * Next();
 
  public float Next(float mean, float sigma, float min, float max) {
    float x = min - 1f; while(x < min || x > max) x = Next(mean, sigma);
    return x;
  }
 
}