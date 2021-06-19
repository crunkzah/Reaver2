using UnityEngine;
using System.Collections;
using TMPro;

public class UITweening_Label : MonoBehaviour
{
    TextMeshProUGUI tmp;
    float timer = 0f;


    public enum LabelState{IDLE, ValueChanged}
    [SerializeField] LabelState state = LabelState.IDLE;
    [Header("Animation settings:")]
    public AnimationCurve curve;
    public float bounceScale = 1f;
    public float timeToAnimate = 0.6f;

    

    RectTransform rectTransform;

    [Header("Color:")]
    public bool animateColor = false;
    public Color activeColor = Color.red;
    Color initColor;
    

    public void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        tmp = GetComponent<TextMeshProUGUI>();
        initColor = tmp.color;
    }
    
    TextMeshProUGUI GetTMP()
    {
        if(this.tmp == null)
            tmp = GetComponent<TextMeshProUGUI>();
        return tmp;
    }

    public void ChangeValue(string newText, bool animate = true)
    {
        
        if(GetTMP().text.Equals(newText))
            return;
        GetTMP().text = newText;
        if(animate)
            SwitchToAnimating();
    }

    void Update()
    {

        //if(Input.GetKeyDown(KeyCode.Space))
        //   SwitchToAnimating();
        

        switch(this.state)
        {
            case LabelState.IDLE:

                break;

            case LabelState.ValueChanged:
                Animate();
                break;
        }
    }

    public void SwitchToIdle()
    {
        this.state = LabelState.IDLE;
        timer = 0f;

        rectTransform.localScale = Vector3.one;
    }

    public void SwitchToAnimating()
    {
        this.state = LabelState.ValueChanged;
        timer = 0f;
    }


    public void OnValueChanged()
    {
        SwitchToAnimating();
    }

    public virtual void Animate()
    {
        float percentage = Mathf.InverseLerp(0f, timeToAnimate, timer);

        float vColor = curve.Evaluate(percentage);

        //Color:
        if(animateColor)
        {
            Color newColor = Color.Lerp(initColor, activeColor, vColor);

            tmp.color = newColor;
        }
        //

        float v = curve.Evaluate(percentage) * bounceScale;

        Vector3 newScale = v * Vector3.one + Vector3.one;
        rectTransform.localScale = newScale;

        timer += Time.deltaTime;

        if(timer >= timeToAnimate)
        {
            SwitchToIdle();
        }
    }
    
}
