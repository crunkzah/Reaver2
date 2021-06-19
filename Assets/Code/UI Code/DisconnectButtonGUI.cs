using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;


public class DisconnectButtonGUI : MonoBehaviour
{
    public TextMeshProUGUI tmp;
    Button btn;
    
    public Color normalColorConnect;
    public Color highlightedColorConnect;
    
    public Color normalColorConnecting;
    public Color highlightedColorConnecting;
    
    public Color normalColorDisconnect;
    public Color highlightedColorDisconnect;
    
    void Awake()
    {
        btn = GetComponent<Button>();
        colorBlock = btn.colors;
        
    }
    
    public ClientState networkState;
    
    ColorBlock colorBlock;
    
    void Update()
    {
        ClientState newNetworkState = PhotonManager.Singleton().networkClientState;
        
        switch(networkState)
        {
            case(ClientState.ConnectingToMasterserver):
            {
                // if(networkState != newNetworkState)
                {
                    tmp.SetText("Connecting...");
                    colorBlock.normalColor = normalColorConnecting;
                    colorBlock.highlightedColor = highlightedColorConnecting;
                    btn.colors = colorBlock;
                }
                break;
            }
            case(ClientState.ConnectedToMasterserver):
            {
                //if(networkState != newNetworkState)
                {
                    tmp.SetText("Disconnect");
                    colorBlock.normalColor = normalColorDisconnect;
                    colorBlock.highlightedColor = highlightedColorDisconnect;
                    btn.colors = colorBlock;
                }
                break;
            }
            case(ClientState.Disconnected):
            {
                //if(networkState != newNetworkState)
                {
                    tmp.SetText("Connect");
                    colorBlock.normalColor = normalColorConnect;
                    colorBlock.highlightedColor = highlightedColorConnect;
                    btn.colors = colorBlock;
                }
                break;
            }
            default:
            {
                break;
            }
        }
        networkState = newNetworkState;
    }
    
}
