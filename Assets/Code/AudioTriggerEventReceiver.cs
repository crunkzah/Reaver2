using UnityEngine;

public enum MusicToPlay
{
    Omnissiah,
    Daycore
}
public class AudioTriggerEventReceiver : MonoBehaviour, INetworkObject
{
    NetworkObject net_comp;
    public MusicToPlay musicToPlay;
    
    public void ReceiveCommand(NetworkCommand command, params object[] args)
    {
        switch(command)
        {
            case(NetworkCommand.DoTrigger):
            {
                SetMusic();
                break;
            }
        }
    }
    
    
    
    void SetMusic()
    {
        switch(musicToPlay)
        {
            case(MusicToPlay.Omnissiah):
            {
                AudioManager.SetMusicOmnissiah();
                break;
            }
            case(MusicToPlay.Daycore):
            {
                AudioManager.SetMusicDaycore();
                break;
            }
        }
    }
    
}
