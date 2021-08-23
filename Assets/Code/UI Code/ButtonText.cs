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
    
    
    void Awake()
    {
        tmp = GetComponentInChildren<TextMeshProUGUI>();
        
        unselectedColor = tmp.color;
    }
    
    void OnEnable()
    {
        if(tmp)
        {
            tmp.color = unselectedColor;
            transform.localScale = new Vector3(1, 1, 1);
        }
    }
    
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if(tmp)
        {
            if(onPointerEnterAudioClip)    
            {
                AudioManager.Singleton().source2.PlayOneShot(onPointerEnterAudioClip, 0.1f);
            }
            tmp.color = selectedColor;
            transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
        }
        
        
        
        // InGameConsole.LogOrange(string.Format("MouseEnter from {0}", this.gameObject.name));
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        
        if(tmp)
        {
            tmp.color = unselectedColor;
            transform.localScale = new Vector3(1, 1, 1);
        }
    }
    
}
