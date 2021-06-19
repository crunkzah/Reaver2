using System.Collections.Generic;
using UnityEngine;

public class DetectorBounds : MonoBehaviour
{
    static List<AutoDetector> detectors = new List<AutoDetector>();
    static List<Detectable> targets  = new List<Detectable>();


    static DetectorBounds _instance;

    void Start()
    {
        if(_instance == null)
            _instance = this;
        else
            if(_instance != this)
            {
                Debug.LogWarning("(Marat): Destroying this " + this.name + " since it's singleton.");
                Destroy(this);
            }
    }

    public static void RegisterDetector(AutoDetector detector)
    {
        detectors.Add(detector);
    }

    public static void UnregisterDetector(AutoDetector detector)
    {
        detectors.Remove(detector);
    }

    public static void RegisterTarget(Detectable target)
    {
        targets.Add(target);
    }

    public static void UnregisterTarget(Detectable target)
    {
        targets.Remove(target);
    }


    void Update()
    {
       
        for(int i = 0; i < detectors.Count; i++)
        {
            bool detectedSomething = false;

            for(int j = 0; j < targets.Count; j++)
            {
                
                if(detectors[i].bounds.Contains(targets[j].WorldPos + Vector3.up * 0.1f))
                {
                        detectedSomething = true;
                        break;
                }
                
            }

            if(detectedSomething)
            {
                if(!detectors[i].isDetecting)
                    detectors[i].Detect();
            }
            else
            {
                if(detectors[i].isDetecting)
                    detectors[i].Undetect();
            }
        }
    }
}
