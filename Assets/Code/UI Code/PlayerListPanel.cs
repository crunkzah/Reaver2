using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class PlayerListPanel : MonoBehaviourPunCallbacks
{
    static PlayerListPanel _instance;
    
    public PlayerListPanel Singleton()
    {
        if(_instance == null)
        {
            _instance = FindObjectOfType<PlayerListPanel>();
        }
        
        return _instance;
    }
    
    
    
    public TextMeshProUGUI player1_label;
    public TextMeshProUGUI player2_label;
    public TextMeshProUGUI player3_label;
    public TextMeshProUGUI player4_label;
    
    TextMeshProUGUI[] player_labels = new TextMeshProUGUI[4];
    
    bool labelsWereInit = false;
    
    void InitLabels()
    {
        if(labelsWereInit)
        {
            return;
        }
        
        player_labels[0] = player1_label;
        player_labels[1] = player2_label;
        player_labels[2] = player3_label;
        player_labels[3] = player4_label;
        
        labelsWereInit = true;
    }
    
    public override void OnEnable()
    {
        base.OnEnable();
        RefreshPlayers();
    }
    
    void Update()
    {
        if(PhotonNetwork.InRoom)
        {
            //PhotonNetwork.CurrentRoom.players
            
        }
    }
    
    void RefreshPlayers()
    {
        InitLabels();
        Debug.Log("<color=yellow>RefreshPlayers()</color>");
        int i = 0;
        
        for(int j = 0; j < player_labels.Length; j++)
        {
            player_labels[j].SetText(string.Empty);
        }
        
        if(PhotonNetwork.InRoom)
        {
            foreach(KeyValuePair<int, Player> p in PhotonNetwork.CurrentRoom.Players)
            {
                player_labels[i].SetText(p.Value.NickName);
                i++;
            }
        }
    }
    
    
    
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        RefreshPlayers();
    }
    
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        RefreshPlayers();
    }
    
}
