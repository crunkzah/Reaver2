using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ButtonText : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    
    TextMeshProUGUI tmp;
    
    
    
    public Color selectedColor = new Color(58, 128, 171);
    Color unselectedColor;
    
    public AudioClip onPointerEnterAudioClip;
    public AudioClip onButtonClickAudioClip;
    Button btn;
    
    
    void Awake()
    {
        tmp = GetComponentInChildren<TextMeshProUGUI>();
        btn = GetComponent<Button>();
        if(btn)
        {
            btn.onClick.AddListener(OnPointerClick);
        }
        
        unselectedColor = tmp.color;
    }
    
    public void OnPointerClick()
    {
        if(onButtonClickAudioClip)    
        {
            AudioManager.Singleton().source2.PlayOneShot(onButtonClickAudioClip, 1f);
        }
    }
    
    void OnEnable()
    {
        if(tmp)
        {
            tmp.color = unselectedColor;
            transform.localScale = new Vector3(1, 1, 1);
        }
    }
    
    float scaleSpeed = 5f;
    float colorSpeed = 7f;
    
    bool isPointerOnThisButton = false;
    
    
    Vector4 ColorToVec4(Color col)
    {
        return new Vector4(col.r, col.g, col.b, col.a);
    }
    
    void Update()
    {
        float dt = UberManager.DeltaTime();
        if(isPointerOnThisButton)
        {
            tmp.color = Vector4.MoveTowards(tmp.color, selectedColor, dt * colorSpeed);//. unselectedColor;
            transform.localScale = Vector3.MoveTowards(transform.localScale, new Vector3(1.15f, 1.15f, 1.15f), dt * scaleSpeed);
        }
        else
        {
            tmp.color = Vector4.MoveTowards(tmp.color, unselectedColor, dt * colorSpeed);//. unselectedColor;
            transform.localScale = Vector3.MoveTowards(transform.localScale, new Vector3(1f, 1f, 1f), dt * scaleSpeed);
        }
    }
    
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if(tmp)
        {
            if(onPointerEnterAudioClip)    
            {
                AudioManager.Singleton().source2.PlayOneShot(onPointerEnterAudioClip, 1f);
            }
            isPointerOnThisButton = true;
           // tmp.color = selectedColor;
            //transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
        }
        
        
        
        // InGameConsole.LogOrange(string.Format("MouseEnter from {0}", this.gameObject.name));
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        if(tmp)
        {
            isPointerOnThisButton = false;
            //tmp.color = unselectedColor;
            //transform.localScale = new Vector3(1, 1, 1);
        }
    }
    
}
