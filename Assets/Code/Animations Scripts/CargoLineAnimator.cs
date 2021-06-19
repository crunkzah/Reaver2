using UnityEngine;
using PathCreation;
using System.Collections.Generic;

public class CargoLineAnimator : MonoBehaviour
{
    
#if UNITY_EDITOR
    
    [Header("Place objects along path:")]
    public Transform populatedObjectsHolder;
    public GameObject objectToPopulate;
    [Range(2,80)]
    public int num = 2;
    public float normalOffset = -0.4f;
    
    
    public List<GameObject> placedObjects;
    
    public void DestroyPlacedObjects()
    {
        if(placedObjects != null && placedObjects.Count > 0)
        {
            for(int i = placedObjects.Count - 1; i >= 0; i--)
            {
                GameObject objToDelete = placedObjects[i];        
                DestroyImmediate(objToDelete);
            }
        }
        
        placedObjects.Clear();
    }
    
    public void PlaceObjectsAtPath()
    {
        
        if(populatedObjectsHolder == null)
        {
            Debug.Log("<color=red>populatedObjectsHolder</color> is <color=red>null</color>");
            return;
        }
        
        if(objectToPopulate != null && placedObjects.Count > 0)
        {
            DestroyPlacedObjects();
        }
        
        
        if(objectToPopulate != null && placedObjects != null && placedObjects.Count == 0)
        {
            num = Mathf.Clamp(num, 2, 100);
            PathCreator pc = GetComponent<PathCreator>();
            float step = 1f / (float)num;
            float t = 0f;
            
            
            Vector3 pos;
            Quaternion rot;
            
            for(int i = 0; i < num; i++)
            {
                pos = pc.path.GetPointAtTime(t) + pc.path.GetNormal(t) * normalOffset;
                rot = pc.path.GetRotation(t);
                                
                t += step;
                
                GameObject obj = Instantiate(objectToPopulate, pos, rot, populatedObjectsHolder);
                placedObjects.Add(obj);
            }
            
            
            Debug.Log(string.Format("<color=green> Placed {0} {1} on path.</color>", num, objectToPopulate.name));
            
        }   
        else
        {
            Debug.Log("Can't populate an object.");
        }     
    }
#endif
    
    
    public bool isWorking = false;
    public bool shouldBeAnimating = false;
    public float maxAnimationSpeed = 2f;
    public float cargoHeightOffset = -0.8f;
    public float animationAcceleration = 1f;
    public float tiesSpeedMultiplier = 1f;
    
    float currentAnimationSpeed = 0f;
    float targetAnimationSpeed  = 0f;
    
    Transform[] cargos;
    Transform[] railTies;
    
    public Transform railTiesHolder;
    public float railTieHeightOffset = -0.8f;
    
    float[] cargosDistanceTravelled;
    public float[] railTiesDistanceTravelled;
    
    PathCreator pathCreator;
    float pathLength;
    
    void Awake()
    {
       InitAndGetCargosInChildren();
    }
    
    void Start()
    {
        PlaceCargos(pathLength);
        
        
        //Hack: this is to get it working from start:
        if(isWorking)
            targetAnimationSpeed = maxAnimationSpeed;
    }
    
    
    public void Interact()
    {
        isWorking = !isWorking;
        shouldBeAnimating = true;
        targetAnimationSpeed = isWorking ? maxAnimationSpeed : 0f;
    }
    
    void InitAndGetCargosInChildren()
    {
        pathCreator = GetComponent<PathCreator>();
        pathLength = pathCreator.path.length;
        
        cargos = new Transform[transform.childCount];
        cargosDistanceTravelled = new float[cargos.Length];
        
        for(int i = 0; i < transform.childCount; i++)
        {
            cargos[i] = transform.GetChild(i);
        }
        
        
        railTies = new Transform[railTiesHolder.childCount];
        railTiesDistanceTravelled = new float[railTies.Length];
        
        for(int i = 0; i < railTiesHolder.childCount; i++)
        {
            railTies[i] = railTiesHolder.GetChild(i);
        }
    }
    
    public void PlaceCargos(float pathLength)
    {
        InitAndGetCargosInChildren();
        
        Vector3 pos;
        Quaternion rot;
        float distanceTravelled = 0;
        float distanceStep = pathLength / cargos.Length;
        
        
        for(int i = 0; i < cargos.Length; i++)
        {
            pos = pathCreator.path.GetPointAtDistance(distanceTravelled) + cargoHeightOffset * pathCreator.path.GetNormalAtDistance(distanceTravelled);
            rot = pathCreator.path.GetRotationAtDistance(distanceTravelled);
            
            cargos[i].SetPositionAndRotation(pos, rot);
            cargosDistanceTravelled[i] = distanceStep * i;
            
            distanceTravelled += distanceStep;
        }
        
        distanceTravelled = 0f;
        distanceStep = pathLength / railTies.Length;
        
        
        for(int i = 0; i < railTies.Length; i++)
        {
            pos = pathCreator.path.GetPointAtDistance(distanceTravelled) + railTieHeightOffset * pathCreator.path.GetNormalAtDistance(distanceTravelled);
            rot = pathCreator.path.GetRotationAtDistance(distanceTravelled);
            
            railTies[i].SetPositionAndRotation(pos, rot);
            railTiesDistanceTravelled[i] = distanceStep * i;            
            
            distanceTravelled += distanceStep;
        }
        
        
    }
        
    void UpdateAnimation(float dt)
    {
        for(int i = 0; i < cargos.Length; i++)
        {
            float distanceForThisCargo = cargosDistanceTravelled[i];
            cargos[i].position = pathCreator.path.GetPointAtDistance(distanceForThisCargo) + cargoHeightOffset * pathCreator.path.GetNormalAtDistance(distanceForThisCargo);
            cargos[i].rotation = pathCreator.path.GetRotationAtDistance(distanceForThisCargo);
            
            cargosDistanceTravelled[i] += currentAnimationSpeed * dt;
            
            if(cargosDistanceTravelled[i] > pathLength)
            {
                cargosDistanceTravelled[i] -= pathLength;
            }
        }        
        
        
        
        for(int i = 0; i < railTies.Length; i++)
        {
            float distanceForThisTie = railTiesDistanceTravelled[i];
            railTies[i].position = pathCreator.path.GetPointAtDistance(distanceForThisTie) + railTieHeightOffset * pathCreator.path.GetNormalAtDistance(distanceForThisTie);
            railTies[i].rotation = pathCreator.path.GetRotationAtDistance(distanceForThisTie);
            
            railTiesDistanceTravelled[i] += currentAnimationSpeed * dt * tiesSpeedMultiplier;
            
            if(railTiesDistanceTravelled[i] > pathLength)
            {
                railTiesDistanceTravelled[i] -= pathLength;
            }
        }
    }
    
    void Update()
    {
        currentAnimationSpeed = Mathf.MoveTowards(currentAnimationSpeed, targetAnimationSpeed, animationAcceleration * UberManager.DeltaTime());
        
        if(currentAnimationSpeed == 0f && targetAnimationSpeed == 0f)
        {
            isWorking = false;
            shouldBeAnimating = false;
            // InGameConsole.Log("<color=red>KEK</color>");
        }
        
        if(shouldBeAnimating)
        {
            UpdateAnimation(UberManager.DeltaTime());
        }
    }
}
