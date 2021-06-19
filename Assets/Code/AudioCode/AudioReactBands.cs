using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioReactBands : MonoBehaviour
{
    const  int bandsNum = 8;
    
    Transform[] bands = new Transform[bandsNum];
    
    public bool flipYAxis = false;
    
    void Start()
    {
        if(flipYAxis)
        {
            transform.Rotate(new Vector3(0, 180f, 0), Space.Self);
        }
        
        int len = Mathf.Min(transform.childCount, bandsNum);
        
        for(int i = 0; i < len; i++)
        {
            bands[i] = transform.GetChild(i);
        }
    }
    
    public float mult = 3;
    
    void Update()
    {
        AudioManager audio_manager = AudioManager.Singleton();
        if(audio_manager)
        {
            for(int i = 0; i < bandsNum; i++)
            {
                Vector3 reacted_scale = bands[i].localScale;
                reacted_scale.y = Mathf.Max(0.2f, audio_manager.freqBands_smoothed[i] * mult);
                bands[i].localScale = reacted_scale;
            }
        }
    }
}
