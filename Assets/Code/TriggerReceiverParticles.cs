using UnityEngine;

public class TriggerReceiverParticles : MonoBehaviour, INetworkObject
{
    [Header("Particles:")]
    public ParticleSystem[] targets_to_call;
    [Header("Sounds:")]
    public SoundType soundToPlay;
    [Header("Title:")]
    public Title titleToShow;
    
    NetworkObject net_comp;
    
    void Awake()
    {
        net_comp = GetComponent<NetworkObject>();
    }
    
    public void ReceiveCommand(NetworkCommand command, params object[] args)
    {
        switch(command)
        {
            default:
            {
                InGameConsole.LogOrange(string.Format("Illegal command {0} was called on {1} with netId {2}", command.ToString(), this.gameObject.name, net_comp.networkId));
                break;
            }
            case(NetworkCommand.DoTrigger):
            {
                InGameConsole.LogOrange(string.Format("<color=yellow>ReceiveTrigger</color> was called on <color=yellow>{1}</color> with netId <color=blue>{2}</color>", command.ToString(), this.gameObject.name, net_comp.networkId));
                ReceiveTrigger();
                break;
            }
        }
    }
    
    void ReceiveTrigger()
    {
        if(targets_to_call != null)
        {
            int len = targets_to_call.Length;
            for(int i = 0; i < len; i++)
            {
                targets_to_call[i].Play(true);
            }
        }
        
        if(soundToPlay != SoundType.None)
        {
            AudioManager.PlayClip(soundToPlay, 1, 0.4f);
        }
        
        TitlesManager.ShowTitle(titleToShow);
    }
}
