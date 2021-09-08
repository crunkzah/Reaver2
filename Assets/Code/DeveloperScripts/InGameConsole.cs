using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InGameConsole : MonoBehaviour
{
    static InGameConsole _instance;
    public static InGameConsole singleton
    {
        get
        {
            if(_instance == null)
                _instance = FindObjectOfType<InGameConsole>();
            return _instance;
        }
    }

    public TextMeshProUGUI tmp_console;
    public GameObject console_panel;
    
    public static TextMeshProUGUI[] lines;
    
    bool enable_on_start = false;
    
    static string[] buffer = new string[256];
    
    static int linesCount;
    
    static int currentBufferIndex = 0;
    static int viewingBufferIndex = 0;
    
    static bool isEnabled = true;
    
    public bool IsActive()
    {
        return console_panel.activeSelf;
    }
    

    void Awake()
    {
        if(_instance == null)
        {
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            this.enabled = false;
            Destroy(this.gameObject);
        }
    }
    
    List<string> commands = new List<string>{"level menu", "music waltz", "music primrose", "reload"};
    
    // public TMP_InputField cmd_input_field;
    
    public void ProcessCommand(string cmd)
    {
        return;
        
        cmd = cmd.ToLower();
        
        if(commands.Contains(cmd))
        {
            InGameConsole.LogFancy(string.Format("Command - <color=yellow>{0}</color>", cmd));
            if(cmd.Equals("level menu"))
            {
                UberManager.Singleton().CommandLevel("menu");
            }
            else
            if(cmd.Equals("music waltz"))
            {
                AudioManager.Singleton().SetMusic("waltz");
            }
            else
            if(cmd.Equals("music primrose"))
            {
                AudioManager.Singleton().SetMusic("primrose");
            }
            else
            if(cmd.Equals("reload"))
            {
                UberManager.Singleton().ReloadLevel();
            }
        }
        else
        {
            if(!string.IsNullOrEmpty(cmd))
            {
                InGameConsole.LogFancy(string.Format("<color=yellow>Unknown command \"{0}\"</color>", cmd));
            }
        }
        // cmd_input_field.text = "";
        
    }

    void Start()
    {
        if(console_panel == null)
        {
            Debug.LogError("console_panel is null !");
        }
        else
        {
            List<TextMeshProUGUI> tmps = new List<TextMeshProUGUI>();
            
            foreach(Transform child in console_panel.transform)
            {
                TextMeshProUGUI tmp_line = child.GetComponent<TextMeshProUGUI>();
                
                if(tmp_line != null)
                {
                    tmps.Add(tmp_line);
                }
                
            }
            
            lines = new TextMeshProUGUI[tmps.Count];
            linesCount = tmps.Count;
            
            for(int i = 0; i < tmps.Count; i++)
            {
                lines[i] = tmps[i];
            }
            console_panel.gameObject.SetActive(enable_on_start);
            
        }
    }

    static int log_count = 0;
    
    static bool logToUnityConsole = true;

    void Update()
    {
        // if(Inputs.MouseButtonDown(0) && cmd_input_field.isFocused)
        // {
        //     InGameConsole.LogFancy("Unfocusing input field");
        //     cmd_input_field.DeactivateInputField();
        // }
        
        // if(!cmd_input_field.isFocused && Input.GetKeyDown(KeyCode.C))
        
        if(Input.GetKeyDown(KeyCode.C))
        {
            console_panel.gameObject.SetActive(!console_panel.gameObject.activeSelf);
            // Use this to take screenshots without player gui:
            // PlayerGUI_In_Game.Singleton.Hide();
            DrawConsole();
        }
        
        
        if(!isEnabled)
        {
            return;
        }
        
        if(singleton.console_panel.activeSelf && Inputs.IsCursorOverUI())
        {
            float scroll = Input.GetAxisRaw("Mouse ScrollWheel");
            if(scroll != 0f)
            {
                viewingBufferIndex += (scroll > 0f) ? 1 : -1;
                viewingBufferIndex = Mathf.Clamp(viewingBufferIndex, 0, buffer.Length - linesCount);
                
                DrawConsole();
                
                //Debug.Log("<color=cyan>viewingBufferIndex </color>" + viewingBufferIndex.ToString());
            }
        }
        //ReassignBuffer();
    }
    
    
    
    static void DrawConsole()
    {
        if(!isEnabled)
            return;
            
        for(int i = 0; i < linesCount; i++)
        {
            //int index = (currentBufferIndex - i - 1);
            int index = (currentBufferIndex - i - 1) - viewingBufferIndex;
            
            if(index < 0)
            {
                index = buffer.Length - Mathf.Abs(index);
                //index = 16 - 1
            }            
            
            lines[i].SetText(buffer[index]);
        }
    }
    
    static void AddToBuffer(ref string[] buffer, string msg)
    {
        log_count++;
        buffer[currentBufferIndex] = msg;
        
        currentBufferIndex++;
        
        if(currentBufferIndex == buffer.Length)
        {
            currentBufferIndex = 0;
        }
        if(singleton.console_panel != null && singleton.console_panel.activeSelf)
        {
            DrawConsole();
        }
    }

    public static void Log(string logMsg)
    {
        if(!isEnabled)
        {
            return;
        }
#if DEBUG_BUILD
        logMsg = string.Format("{0}" + logMsg, "<color=#769ede>" + log_count.ToString() + ": </color>");
        
        AddToBuffer(ref buffer, logMsg);
        if(logToUnityConsole)
            Debug.Log(logMsg);
        
        singleton.tmp_console.SetText(logMsg);
#endif
    }
    
    public static void LogFancy(string logMsg)
    {
        if(!isEnabled)
        {
            return;
        }
#if DEBUG_BUILD
        logMsg = string.Format("<color=#769ede>{0}: <color=#1FE892>" + logMsg + "</color></color>", log_count.ToString());
        
        AddToBuffer(ref buffer, logMsg);
        if(logToUnityConsole)
            Debug.Log(logMsg);
        
        singleton.tmp_console.SetText(logMsg);
#endif
    }
    
    public static void LogOrange(string logMsg)
    {
        if(!isEnabled)
        {
            return;
        }
#if DEBUG_BUILD
        logMsg = string.Format("<color=#ff9d00>{0}: " + logMsg + "</color>", log_count.ToString());
        
        AddToBuffer(ref buffer, logMsg);
        if(logToUnityConsole)
            Debug.Log(logMsg);
        
        singleton.tmp_console.SetText(logMsg);
#endif
    }


    public static void LogWarning(string warningMsg)
    {
        if(!isEnabled)
        {
            return;
        }
#if DEBUG_BUILD
        warningMsg = string.Format("{0}<color=yellow>" + warningMsg + "</color>", "<color=#769ede>" + log_count.ToString() + ": </color>");
        
        AddToBuffer(ref buffer, warningMsg);
        
        
        Debug.LogWarning(warningMsg);
        
        singleton.tmp_console.SetText(warningMsg);
#endif
    }
    
    
    public static void LogError(string errorMsg)
    {
        if(!isEnabled)
        {
            return;
        }
#if DEBUG_BUILD
        errorMsg = string.Format(string.Format("{0}<color=red>" + errorMsg + "</color>", "<color=#769ede>" + log_count.ToString() + ": </color>"));
        
        AddToBuffer(ref buffer, errorMsg);
        
        singleton.tmp_console.SetText(errorMsg);
        Debug.LogError(errorMsg);
#endif
    }
}
