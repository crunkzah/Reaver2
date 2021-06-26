using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;

public enum DialogueState : byte
{
    Hidden,
    Starting,
    Shown,
    Ending
}

public enum DialogueAvatar : byte
{
    Kate,
    MrCoal,
    Civ2
}

public class DialogueManager : MonoBehaviour
{
    DialogueState state = DialogueState.Hidden;
    
    public RectTransform blackBar_top;
    public RectTransform blackBar_bottom;
    
    public GameObject cinematic_canvas;
    public Image avatar_img;
    public TextMeshProUGUI name_tmp;
    public TextMeshProUGUI text_tmp;
    
    const float shown_height = 250;
    const float shutter_speed = 450;
    
    static DialogueManager _instance;
    
    public static DialogueManager Singleton()
    {
        if(_instance == null)
        {
            _instance = FindObjectOfType<DialogueManager>();
        }
        
        return _instance;
    }
    
    public static DialogueState GetState()
    {
        if(_instance == null)
        {
            return DialogueState.Hidden;
        }
        
        return _instance.state;
    }
    
    void Awake()
    {
        _instance = this;
        
        HideShutter_Immediate();
    }
    
    public static void ShowShutter()
    {
        _instance._ShowShutter();
    }
    
    void _ShowShutter()
    {
        
        SetState(DialogueState.Starting);
    }
    
    public static void HideShutter()
    {
        _instance._HideShutter();
    }
    
    void HideShutter_Immediate()
    {
        blackBar_top.sizeDelta    = new Vector2(blackBar_top.sizeDelta.x, 0);
        blackBar_bottom.sizeDelta = new Vector2(blackBar_bottom.sizeDelta.x, 0);
        
        SetState(DialogueState.Ending);        
    }
    
    void _HideShutter()
    {
        SetState(DialogueState.Ending);
    }
    public static void ShowText(string txt)
    {
        _instance._ShowText(txt);
    }
    
    StringBuilder sb = new StringBuilder(64);
    
    public DialogueController current_dialogue;
    
    public static bool HasActiveDialogue()
    {
        if(_instance == null)
            return true;
            
        return (_instance.current_dialogue != null);
    }
    
    public static DialogueController GetCurrentDialogue()
    {
        return _instance.current_dialogue;
    }
    
    public static void ReleaseDialogue()
    {
        _instance._ReleaseDialogue();
    }
    
    void _ReleaseDialogue()
    {
        if(current_dialogue)
        {
            current_dialogue.OnDialogueEnded();
        }
        current_dialogue = null;
    }
    
    public static void SetDialogue(DialogueController dialogueController)
    {
        _instance._SetDialogue(dialogueController);
    }
    
    void _SetDialogue(DialogueController dialogueController)
    {
        current_dialogue = dialogueController;
        
        SetDialogueAvatar(current_dialogue.GetMonologue().avatar);
        
        currentLineIndex = 0;        
        ShowText(current_dialogue.GetMonologue().GetLine(0));
        ShowShutter();
    }
   
    
    int currentLineIndex = 0;
    
    public string GetNextLine()
    {
        if(current_dialogue == null)
        {
            return "NO MORE LINES";
        }
        
        if(currentLineIndex >= current_dialogue.GetMonologue().lines_eng.Length)
        {
            return "ARRAY INDEX OVERSHOOT";
        }
        
        return current_dialogue.GetMonologue().GetLine(currentLineIndex);
    }
    
    string currentTextToType = "";
    //const float charTypeSpeedNormal = 33F / 1000F;
    const float charTypeSpeedNormal = 16F / 1000F;
    float timer = 0;
    int charIndex = 0;
    
    
    string GetSubstringOfTitle(ref string str, int length)
    {
        sb.Clear();
        
        for(int i = 0; i <= length; i++)
        {
            sb.Append(str[i]);
        }
        
        return sb.ToString();
    }
    
    void OnCharTyped()
    {
        float pitch = Random.Range(0.9f, 1.05f);
        AudioManager.PlayClip(SoundType.onCharTyped1, 0.3f, pitch);
    }
    
    public void PrintWholeLine()
    {
        charIndex = currentTextToType.Length;
        
        text_tmp.SetText(currentTextToType);
    }
    
    void ProcessMainText(float dt)
    {
        if(charIndex >= currentTextToType.Length)
        {
            return;
        }
        
        timer -= dt;
        
        if(timer <= 0)
        {
            timer += charTypeSpeedNormal;
            charIndex++;
            if(charIndex < currentTextToType.Length)
            {
                if(currentTextToType[charIndex] == ' ')
                {
                    timer = charTypeSpeedNormal * 2;
                }
                
                string text_substring = GetSubstringOfTitle(ref currentTextToType, charIndex);
                //InGameConsole.LogOrange("SubString: " + text_substring);
                text_tmp.SetText(text_substring);
                OnCharTyped();
            }
            else
            {
                
                if(state == DialogueState.Starting)
                {
                    SetState(DialogueState.Shown);
                }
            }
        }
    }
    
// #if UNITY_EDITOR
    
//     void OnGUI()
//     {
//             GUIStyle style = new GUIStyle();
//             style.alignment = TextAnchor.MiddleCenter;
            
//             float w = 300;
//             float h = 50;
            
//             style.normal.textColor = Color.green;
//             GUI.Label(new Rect(Screen.width/2, Screen.height - h*4.0f, w, h), "DialogueState: " + state.ToString(), style);
//     }
    
// #endif

    void ShowGUIElements()
    {
        avatar_img.gameObject.SetActive(true);
        name_tmp.gameObject.SetActive(true);
        text_tmp.gameObject.SetActive(true);
    }
    
    void HideGUIElements()
    {
        avatar_img.gameObject.SetActive(false);
        name_tmp.gameObject.SetActive(false);
        text_tmp.gameObject.SetActive(false);
    }
    
    void SetState(DialogueState _state)
    {
        InGameConsole.LogOrange("SetState: " + _state.ToString());
        switch(_state)
        {
            case(DialogueState.Hidden):
            {
                
                blackBar_top.sizeDelta = new Vector2(blackBar_top.sizeDelta.x, 0);
                blackBar_bottom.sizeDelta = new Vector2(blackBar_top.sizeDelta.x, 0);
                
                cinematic_canvas.SetActive(false);
                
                break;
            }
            case(DialogueState.Starting):
            {
                cinematic_canvas.SetActive(true);       
                
                break;
            }
            case(DialogueState.Shown):
            {
                ShowGUIElements();
                blackBar_top.sizeDelta = new Vector2(blackBar_top.sizeDelta.x, shown_height);
                blackBar_bottom.sizeDelta = new Vector2(blackBar_top.sizeDelta.x, shown_height);     
                
                
                cinematic_canvas.SetActive(true);       
                
                break;
            }
            case(DialogueState.Ending):
            {
                HideGUIElements();
                break;
            }
        }
        
        state = _state;
    }
    
    void OnShutterShown()
    {
        
    }
    
    void OnShutterHidden()
    {
    }
    
    void ClearText()
    {
        currentTextToType = "";
        text_tmp.SetText(currentTextToType);
    }
    
    public void _ShowText(string txt)
    {
        //InGameConsole.LogOrange("Trying to show text: " + txt);
        ClearText();
        timer = charTypeSpeedNormal;
        SetCurrentLine(txt);
        
        SetState(DialogueState.Starting);
    }
    
    void ToggleState()
    {
        if(state == DialogueState.Hidden)
        {
            ShowShutter();
        }
        else if(state == DialogueState.Shown)
        {
            HideShutter();
        }
    }
    
    static Vector2 v2Zero = new Vector2(0, 0);
    
    string alphabet = "abcdef ghijklmn opqrstuvw xyzABCDE FGHIJKLMNOPQ RSTUVWXYZ";
    
    void ShowRandomText()
    {
        int len = Random.Range(16, 48);
        
        StringBuilder _sb = new StringBuilder(len);
        for(int i = 0; i < len; i++)
            _sb.Append(alphabet[Random.Range(0, alphabet.Length)]);
        
        InGameConsole.LogFancy(string.Format("Random text: {0}", _sb.ToString()));
        
        ShowText(_sb.ToString());
    }
    
    bool DEBUGFlag = true;
    
    void SetCurrentLine(string new_line)
    {
        currentLineIndex++;
        currentTextToType = new_line;
        charIndex = 0;
    }
    
    void Update()
    {
        float dt = UberManager.DeltaTime();
        
        // if(Input.GetKeyDown(KeyCode.T))
        // {
        //     if(DEBUGFlag)
        //     {
        //         ToggleState();
        //     }
        //     else
        //         ShowRandomText();
                
        //     DEBUGFlag = !DEBUGFlag;
        // }
        
        if(Inputs.GetInteractKeyDown())
        {
            if(state != DialogueState.Starting && current_dialogue != null)
            {
                if(charIndex < currentTextToType.Length)
                {
                    PrintWholeLine();
                }
                else
                {
                    if(current_dialogue == null)
                    {
                        InGameConsole.LogFancy("current_dialogue is null");
                    }
                    else
                    {
                        if(current_dialogue.GetMonologue() == null)
                        {
                            InGameConsole.LogFancy("current_dialogue.GetMonologue() is null");
                        }
                        else
                        {
                            if(current_dialogue.GetMonologue().lines_eng == null)
                            {
                                InGameConsole.LogFancy("current_dialogue.GetMonologue().lines_eng is null");
                            }
                        }
                    }
                    
                    if(currentLineIndex < current_dialogue.GetMonologue().lines_eng.Length)
                    {
                        //currentLineIndex++;
                        ShowText(GetNextLine());
                    }
                    else
                    {
                        HideShutter();
                    }
                }
            }
        }
        
        switch(state)
        {
            case(DialogueState.Hidden):
            {
                
                break;
            }
            case(DialogueState.Starting):
            {
                float dHeight = shutter_speed * dt;
                
                blackBar_top.sizeDelta    = Vector2.MoveTowards(blackBar_top.sizeDelta, v2_set_y(blackBar_top.sizeDelta, shown_height), dHeight);
                blackBar_bottom.sizeDelta = Vector2.MoveTowards(blackBar_bottom.sizeDelta, v2_set_y(blackBar_bottom.sizeDelta, shown_height), dHeight);
                
                if(blackBar_bottom.sizeDelta == v2_set_y(blackBar_bottom.sizeDelta, shown_height))
                {
                    SetState(DialogueState.Shown);
                }
                
                break;
            }
            case(DialogueState.Shown):
            {
                ProcessMainText(dt);
                
                break;
            }
            case(DialogueState.Ending):
            {
                float dHeight = shutter_speed * dt;
                
                blackBar_top.sizeDelta    = Vector2.MoveTowards(blackBar_top.sizeDelta, v2_set_y(blackBar_top.sizeDelta, 0), dHeight);
                blackBar_bottom.sizeDelta = Vector2.MoveTowards(blackBar_bottom.sizeDelta, v2_set_y(blackBar_top.sizeDelta, 0), dHeight);
                
                if(blackBar_bottom.sizeDelta == v2_set_y(blackBar_bottom.sizeDelta, 0))
                {
                    SetState(DialogueState.Hidden);
                    ReleaseDialogue();
                }
                
                break;
            }
        }
    }


    Vector2 v2_set_y(Vector2 v, float new_y_value)
    {
        return new Vector2(v.x, new_y_value);
    }

    [Header("Dialogue avatars:")]
    public Sprite Default_avatar;
    public Sprite Kate_avatar;
    public Sprite MrCoal_avatar;
    public Sprite Civ2_avatar; 
    
    public void SetDialogueAvatar(DialogueAvatar avatar)
    {
        switch(avatar)
        {
            case(DialogueAvatar.Kate):
            {
                name_tmp.SetText("Kate");
                avatar_img.sprite = Kate_avatar;
                break;
            }
            case(DialogueAvatar.MrCoal):
            {
                avatar_img.sprite = MrCoal_avatar;
                name_tmp.SetText("Mr Coal");
                break;
            }
            case(DialogueAvatar.Civ2):
            {
                avatar_img.sprite = Civ2_avatar;
                name_tmp.SetText("Mr Civ");
                break;
            }
            default:
            {
                name_tmp.SetText("???");
                avatar_img.sprite = Default_avatar;
                break;
            }
        }
    }
}
