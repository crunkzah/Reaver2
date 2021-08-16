using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class LocalSettings
{
    public float sensitivityX = 9 / 16;
    public float sensitivityY = 16 / 9;

    public float mouseSensitivity = 1;
    
    public float masterVolume = 0.5F;
    public float musicVolume = 0.8F;
    public float effectsVolume = 1F;
    public float fov = 110F;
    
    public bool ambientOcclusion = true;
    public bool colorGrading     = true;

    public void ReadLocalSettings()
    {
        mouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 1F);
        masterVolume     = PlayerPrefs.GetFloat("MasterVolume", 0.5F);
        musicVolume      = PlayerPrefs.GetFloat("MusicVolume", 0.8F);
        effectsVolume    = PlayerPrefs.GetFloat("EffectsVolume", 1F);
        fov              = PlayerPrefs.GetFloat("Fov", 110F);
    }
    
    public void SetFov()
    {
        PlayerPrefs.SetFloat("Fov", fov);
    }

    public void SetMouseSens()
    {
        PlayerPrefs.SetFloat("MouseSensitivity", mouseSensitivity);   
    }

    public void SetMasterVolume(float v)
    {
        masterVolume = v;
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
    }

    public void SetMusicVolume(float v)
    {
        
        musicVolume = v;
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
    }

    public void SetEffectsVolume(float v)
    {
        effectsVolume = v;
        PlayerPrefs.SetFloat("EffectsVolume", effectsVolume);
    }

    public void SaveLocalSettings()
    {
        PlayerPrefs.SetFloat("MouseSensitivity", mouseSensitivity);
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.SetFloat("EffectsVolume", effectsVolume);
        PlayerPrefs.SetFloat("Fov", fov);
        PlayerPrefs.SetInt("AO", 1);
        PlayerPrefs.SetInt("colorGrading", 1);
        
    }
}
