using System.Collections;
using UnityEngine;

public class FadableObject : MonoBehaviour
{
    public Material fading_material_shared;
    public Material opaque_material_shared;
    public static Transform localPlayer_transform;

    public Transform pivot_for_distance;
    MeshRenderer rend;

    public float fadeDistance = 9f;
    // float sqrFadeDistance;
    public float fadeTime = 0.3f;
    
    public bool shouldFade = false;

    
    
    float timer = 0f;
    float alphaValue = 1;

    void Awake()
    {
        // sqrFadeDistance = fadeDistance * fadeDistance;
        rend = GetComponent<MeshRenderer>();
    }

    void Start()
    {
        #if UNITY_EDITOR
        if(pivot_for_distance == null)
        {
            
            Debug.LogError("PivotForDistance not set for " + this.gameObject.name);
        }
        #endif

        //fading_material_shared = rend.sharedMaterial;
        rend.material = opaque_material_shared;
        
        // fading_material_shared.color = MathHelper.ModifyAlpha(fading_material_shared.color, 1f);
        Color col = fading_material_shared.color;
        col.a = 1f;
        
        fading_material_shared.color = col;
    }

    Coroutine currentCoroutine;
    

    // Color ModifyAlpha(Color _col, float newAlpha)
    // {
    //     newAlpha = Mathf.Clamp(newAlpha, 0f, 1f);
    //     return new Color(_col.r, _col.g, _col.b, newAlpha);
    // }

    void Update()
    {
        if(localPlayer_transform == null)
            return;

       

        //Use squared distance instead of common distance - 05.08.2019
        //if(Vector3.Distance(pivot_for_distance.position, localPlayer_transform.position) <= fadeDistance)
        if(shouldFade || MathHelper.SqrDistance(pivot_for_distance.position, localPlayer_transform.position) < fadeDistance * fadeDistance)
        {
            if(timer == 0f && alphaValue == 1f)
                currentCoroutine = StartCoroutine(FadeOut());         
        }
        else
        {
            if(timer == 0f && alphaValue == 0f)
                currentCoroutine = StartCoroutine(FadeIn());
        }
        

        // Debug.DrawLine(pivotForDistance.position, player_transform.position, rayColor, 0f);
    }

    // void FadeOutInstant()
    // {
    //     //fading_material_shared.color = ModifyAlpha(fading_material_shared.color, 0f);
    //     fading_material_shared.color = MathHelper.ModifyAlpha(fading_material_shared.color, 0f);
    // }

    // void FadeInInstant()
    // {
    //     fading_material_shared.color = MathHelper.ModifyAlpha(fading_material_shared.color, 1f);
    // }
    
    
    
    
    //TODO: Dont use COROUTINES ! Much GC happening. -01.02.2019

    IEnumerator FadeOut()
    {
        rend.material = fading_material_shared;

        while(timer < fadeTime)
        {
            timer += Time.deltaTime;
            alphaValue -= Time.deltaTime / fadeTime;

            // fading_material_shared.color = MathHelper.ModifyAlpha(fading_material_shared.color, alphaValue);
            
            Color _col = fading_material_shared.color;
            _col.a = alphaValue;
            
            fading_material_shared.color = _col;
            
            yield return null;      
        }
        
        alphaValue = 0f;
        Color col = fading_material_shared.color;
        col.a = alphaValue;
        fading_material_shared.color = col;
        // fading_material_shared.color = MathHelper.ModifyAlpha(fading_material_shared.color, alphaValue);
        

        timer = 0f;
    }

    IEnumerator FadeIn()
    {
        
        while(timer < fadeTime)
        {
            timer += Time.deltaTime;
            alphaValue += Time.deltaTime / fadeTime;
            
            Color _col = fading_material_shared.color;
            _col.a = alphaValue;
            
            fading_material_shared.color = _col;
            // fading_material_shared.color = MathHelper.ModifyAlpha(fading_material_shared.color, alphaValue);
            
            yield return null;
        }

        alphaValue = 1f;
        
        Color col = fading_material_shared.color;
        col.a = alphaValue;
        fading_material_shared.color = col;
        
        // fading_material_shared.color = MathHelper.ModifyAlpha(fading_material_shared.color, alphaValue);
        rend.material = opaque_material_shared;
        timer = 0f;
    }

}
