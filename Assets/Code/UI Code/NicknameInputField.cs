using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;

public class NicknameInputField : MonoBehaviour
{
    
    TMP_InputField tmp_inputField;
    
    void Awake()
    {
        
    }
    
    void OnEnable()
    {
        if(!tmp_inputField)
        {
            tmp_inputField = GetComponent<TMP_InputField>();
            string savedNickName = PlayerPrefs.GetString("Nickname", "Player");
            tmp_inputField.SetTextWithoutNotify(savedNickName);
        }
    }
    
    void OnDisable()
    {
        if(!tmp_inputField)
        {
            tmp_inputField = GetComponent<TMP_InputField>();
        }
        string nickNameToSave = tmp_inputField.text;
        PlayerPrefs.SetString("Nickname", nickNameToSave);
    }
    
    public void OnValueChanged()
    {
        if(!tmp_inputField)
            tmp_inputField = GetComponent<TMP_InputField>();
            
        
        PhotonNetwork.NickName = tmp_inputField.text;
        InGameConsole.LogFancy("NickName is <color=yellow>" + PhotonNetwork.NickName + "</color>");
    }
    
    
    
}
