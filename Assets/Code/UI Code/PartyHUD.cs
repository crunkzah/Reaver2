using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public struct PartyMember
{
    public string nick;
    public PlayerController pController;
    public PartyMember(string _nick, PlayerController controller)
    {
        nick = _nick;
        pController = controller;
    }
}

[System.Serializable]
public struct PartyMemberSingleHUD
{
    public GameObject holder;
    public RectTransform img_rect;
    public TextMeshProUGUI tmp_nick;
    public TextMeshProUGUI tmp_ping;
}



public class PartyHUD : MonoBehaviour
{
    
    static PartyHUD instance;
    public static PartyHUD Singleton()
    {
        if(instance == null)
        {
            instance = FindObjectOfType<PartyHUD>();
        }
        
        return instance;
    }
    
    public List<PartyMember> party = new List<PartyMember>(3);    
    public List<PartyMemberSingleHUD> party_huds;
    
    public GameObject hud_holder;
    
    public static void RebuildPartyHUD()
    {
        ref List<PlayerController> pcs = ref UberManager.Singleton().players_controller;
        int otherPlayersCount = 0;
        for(int i = 0; i < pcs.Count; i++)
        {
            if(pcs[i])
            {
                if(!pcs[i].pv.IsMine)
                {
                    otherPlayersCount++;
                }
            }
        }
        
        SetPlayersInScene(otherPlayersCount);
    }
    
    
    public static void SetPlayersInScene(int otherPlayersInSceneCount)
    {
        Singleton()._SetPlayersInScene(otherPlayersInSceneCount);          
    }
    
    void _SetPlayersInScene(int otherPlayerInSceneCount)
    {
        InGameConsole.LogFancy("_SetPlayersInScene " + otherPlayerInSceneCount);
        party.Clear();
        
        ref List<PlayerController> pcs = ref UberManager.Singleton().players_controller;
        int j = 0;
        
        for(int i = 0; i < pcs.Count; i++)
        {
            if(pcs[i])
            {
                if(!pcs[i].pv.IsMine)
                {
                    party.Add(new PartyMember(pcs[i].pv.Owner.NickName, pcs[i]));
                    InGameConsole.LogFancy("_SetPlayersInScene() NickName of player is " + pcs[i].pv.Owner.NickName);
                    party_huds[j].tmp_nick.SetText(pcs[i].pv.Owner.NickName);
                    //party_huds[j].tmp_ping.SetText(pcs[i].rtt.ToString());
                    party_huds[j].holder.SetActive(true);
                    j++;
                }
            }
        }
        
        for(; j < party_huds.Count; j++)
        {
            party_huds[j].holder.SetActive(false);
            //party_huds[j].tmp_nick.SetText(pcs[i].pv.Owner.NickName);
            //party_huds[j].tmp_nick.SetText(pcs[i].rtt.ToString());
        }
    }
    
    void Start()
    {
        hud_holder.SetActive(false);
    }
    
    void Update()
    {
        if(party != null && party.Count > 0)
        {
            int len = party.Count;
            
            for(int i = 0; i < len; i++)
            {
                float hpPercentage = party[i].pController.GetHitpointsPercentageRemote();
                
                Vector3 localScale = party_huds[i].img_rect.localScale;
                float dt = UberManager.DeltaTime();
                party_huds[i].img_rect.localScale = Vector3.MoveTowards(party_huds[i].img_rect.localScale, new Vector3(hpPercentage, 1, 1), dt * 3);
                
                if(localScale.x != hpPercentage)
                {
                    float currentLocalScaleX = localScale.x;
                    
                    localScale.x = hpPercentage;
                    //currentLocalScaleX = Mathf.MoveTowards(currentLocalScaleX, localScale.x, dt * 3);
                    //localScale.x = currentLocalScaleX;
                    party_huds[i].img_rect.localScale = localScale;
                }
                
                //party_huds[i].tmp_ping.SetText(party[i].pController.rtt.ToString());
            }
            
            
            if(!hud_holder.activeSelf)
            {
                hud_holder.SetActive(true);
            }
        }
        else
        {
            if(hud_holder.activeSelf)
            {
                hud_holder.SetActive(false);
            }
        }
    }
}
