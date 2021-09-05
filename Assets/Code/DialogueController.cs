using UnityEngine;
using Photon.Pun;



public class DialogueController : MonoBehaviour, INetworkObject
{
    [SerializeField] Monologue monologue;
    
    NetworkObject net_comp;
    
    Transform thisTransform;
    
    bool hasTalkedThroughThisDialogue = false;
    
    void Awake()
    {
        thisTransform = transform;
        net_comp = GetComponent<NetworkObject>();
    }
    
    public float dialogueRadius = 2F;
    
    bool canSendCommands = true;
    
    void LockSendingCommands()
    {
        canSendCommands = false;
    }
    
    void UnlockSendingCommands()
    {
        canSendCommands = true;
    }
    
    public void ReceiveCommand(NetworkCommand command, params object[] args)
    {
        switch(command)
        {
            case(NetworkCommand.Ability1):
            {
                //Start dialogue:
                UnlockSendingCommands();
                SetCurrentDialogueThis();
                
                break;
            }
            case(NetworkCommand.InteractCmd):
            {
                UnlockSendingCommands();
                //Show next line:
                break;
            }
            case(NetworkCommand.ClearTarget):
            {
                UnlockSendingCommands();
                
                for(int i = 0; i < gates_to_open.Length; i++)
                {
                    gates_to_open[i].Unlock();
                }
                //Finish dialogue:
                break;
            }
            default:
            {
                break;
            }
        }
    }
    
    public GatesController[] gates_to_open;
    
    public void SetCurrentDialogueThis()
    {
        //InGameConsole.LogFancy("SetCurrentDialogueThis()");
        DialogueManager.SetDialogue(this);
    }    
    
    public Monologue GetMonologue()
    {
        return monologue;
    }
    
    
    
    public void OnDialogueEnded()
    {
        hasTalkedThroughThisDialogue = true;
        InGameConsole.LogFancy("Dialogue ended");
        if(PhotonNetwork.IsMasterClient)
        {
            LockSendingCommands();
            NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.ClearTarget);
        }
    }
    
    public bool showMsg = false;
    
    public string msgToShow = "MSG TO SHOW";
    public bool isInDialogueOrNearIt = false; 
    
    void Update()
    {
        if(hasTalkedThroughThisDialogue)
        {
            this.enabled = false;
            return;
        }
        
        if(!canSendCommands || DialogueManager.HasActiveDialogue())
        {
            return;
        }
        
        PlayerController local_player = PhotonManager.GetLocalPlayer();
        if(local_player != null)
        {
            Vector3 localPlayer_pos = local_player.GetHeadPosition();
            bool isInRange = Math.SqrDistance(localPlayer_pos, thisTransform.position) < dialogueRadius * dialogueRadius;
            if(isInRange)
            {
                if(DialogueManager.GetState() == DialogueState.Hidden)
                {
                    //local_player.gunController.isInDialogueOrInRange = true;
                    if(showMsg)
                    {
                        showMsg = false;
                        MessagePanel.Singleton().ShowMessage(msgToShow, 4);
                    }
                    //Show here message 'Press 'E' to chat'
                    //InGameConsole.LogOrange("Press E to chat");
                    if(Inputs.GetInteractKeyDown())
                    {
                        LockSendingCommands();
                        NetworkObjectsManager.CallNetworkFunction(net_comp.networkId, NetworkCommand.Ability1);
                    }
                }
            }
        }
    }
    
}
