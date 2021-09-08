using UnityEngine;

public class MessageTrigger : MonoBehaviour
{
    public BoxCollider boxCol;
    Bounds bounds;
    
    [Header("Amir <3 Marat")]
    public float timeToBeShown = 12;
    public string messageToShowEng;
    public string messageToShowRus;
    
    void Awake()
    {
        if(!boxCol)
        {
            boxCol = GetComponent<BoxCollider>();
        }
        bounds = boxCol.bounds;
    }
    
    
    void Start()
    {
        if(boxCol)
        {
            Destroy(boxCol);
        }
    }
    bool wasTriggered = false;
    PlayerController local_pc;
    

    void Update()
    {
        if(wasTriggered)
        {
            return;
        }
        
        if(local_pc)
        {
            if(bounds.Contains(local_pc.GetCenterPosition()))
            {
                wasTriggered = true;
                
                switch(UberManager.lang)
                {
                    case(Language.English):
                    {
                        MessagePanel.Singleton().ShowMessage(messageToShowEng, timeToBeShown);
                        break;
                    }
                    case(Language.Russian):
                    {
                        MessagePanel.Singleton().ShowMessage(messageToShowRus, timeToBeShown);
                        break;
                    }
                }
            }
        }
        else
        {
            local_pc = PhotonManager.GetLocalPlayer();
        }
    }    
    
}
