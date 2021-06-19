using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public struct Audio3dPooled
{
    public Transform tr;
    public AudioSource src;
}

[System.Serializable]
public struct ClipAndType
{
    public string name;
    public SoundType type;
    public AudioClip clip;
}

public enum AudioState
{
    Normal,
    PlayerDead
}

public class AudioManager : MonoBehaviour
{
    static AudioManager instance;
    public static AudioManager Singleton()
    {
        if(instance == null)
        {
            instance = FindObjectOfType<AudioManager>();
        }
        
        
        
        return instance;
    }
    
    
    public GameObject audio3dPrefab;
    
    public Audio3dPooled[] audio_pool;
    int audio_pool_len = 32;
    int audioPoolIndex = 0;
    
    void InitAudioPool()
    {
        audio_pool = new Audio3dPooled[audio_pool_len];
        for(int i = 0; i < audio_pool_len; i++)
        {
            GameObject audio3d = Instantiate(audio3dPrefab, new Vector3(2000, 2000, 2000), Quaternion.identity, this.transform);
            
            audio_pool[i].tr = audio3d.transform;
            audio_pool[i].src = audio3d.GetComponent<AudioSource>();
            
            audio3d.gameObject.SetActive(false);
        }
        
        audioPoolIndex = 0;
    }
    
    AudioClip GetClip(SoundType type)
    {
        int key = (int)type;
        return audioLib[key];
    }
    
    void _Play3D(SoundType type, Vector3 pos, float pitch, float vol, float doppler)
    {
        // InGameConsole.LogFancy("Play3D");
        audio_pool[audioPoolIndex].tr.localPosition = pos;
        audio_pool[audioPoolIndex].src.pitch = pitch;
        audio_pool[audioPoolIndex].src.volume = vol;
        
        audio_pool[audioPoolIndex].src.clip = GetClip(type);
        audio_pool[audioPoolIndex].tr.gameObject.SetActive(true);
        
        audio_pool[audioPoolIndex].src.dopplerLevel = doppler;
        
        audio_pool[audioPoolIndex].src.Play();
        
        
        audioPoolIndex++;
        if(audioPoolIndex >= audio_pool.Length)
        {
            audioPoolIndex = 0;
        }
    }
    
    
    public static void Play3D(SoundType type, Vector3 pos, float pitch = 1, float vol = 1, float doppler = 1)
    {
        Singleton()._Play3D(type, pos, pitch, vol, doppler);
    }
    
    
    static string masterPitchParam = "MP";
    
    public AudioState state = AudioState.Normal;
    bool isMuted;    
    public static float GetMasterPitch()
    {
        float Result = 1;
        Singleton().masterMixer.audioMixer.GetFloat(masterPitchParam, out Result);
        
        return Result;
    }
    
    public static void SetTargetMasterPitch(float targetPitch, float startSpeed, float acceleration)
    {
        float currentPitch = 0;
        
        if(Singleton().masterMixer.audioMixer.GetFloat(masterPitchParam, out currentPitch))
        {
            Singleton().targetMasterPitch = targetPitch;
            Singleton().pitchInterpStartSpeed = startSpeed;
            Singleton().pitchInterpAcceleration = acceleration;
            // InGameConsole.LogFancy("Master pitch: " + currentPitch);
        }
//        Singleton().masterMixer.audioMixer.SetFloat(masterPitchParam, targetPitch);
    }
    
    public static void SetMasterPitch(float pitch)
    {
        Singleton().masterMixer.audioMixer.SetFloat(masterPitchParam, pitch);
    }
    
    const float LOUD = 1;
    
    public AudioMixerGroup masterMixer;
    public AudioMixerGroup musicMixer;
    public AudioMixerGroup effectsMixer;
    // public Mixe
    
    // public void ReceiveCommand(NetworkCommand command, params object[] args)
    // {
    //     switch(command)
    //     {
    //         case(NetworkCommand.Ability1):
    //         {
    //             string arg = (string)args[0];
                
    //             SetMusic(arg);
                
    //             break;
    //         }
    //     }
    // }
    
    public ClipAndType[] clipsAndTypes;
    
        
    Dictionary<int, AudioClip> audioLib = new Dictionary<int, AudioClip>();
    
    void InitAudioLib()
    {
        if(clipsAndTypes.Length > 0)
        {
            for(int i = 0; i < clipsAndTypes.Length; i++)
            {
                audioLib.Add((int)clipsAndTypes[i].type, clipsAndTypes[i].clip);
            }
        }
    }
    
    
    
    [Header("Audio settings:")]
    public AudioClip ambience01;
    [Header("Music:")]
    public AudioClip children_of_the_omnissiah;
    public AudioClip daycore;
    public AudioClip cyber_grind;
    public AudioClip mainMenu_theme;
    public AudioSource music_src;
    public AudioSource source2;
    [Header("Globals clips:")]
    public AudioClip[] globalClips;
    
    void Awake()
    {
        if(instance != null && instance.GetInstanceID() != this.GetInstanceID())
        {
            Destroy(this.gameObject);
        }
        
        _EmissionShaderID = Shader.PropertyToID("_Emission");
        
        music_src = GetComponent<AudioSource>();
        
        InitAudioLib();
        InitAudioPool();
    }
    
    static float globalClipAvailableTime = 0f;
    const float globalClipCd = 0.0025F;
    
    public static void PlayGlobalClip(int id, float vol, float pitch)
    {
        if(UberManager.TimeSinceStart() > globalClipAvailableTime)
        {
            if(Singleton().globalClips == null || id < 0 || id >= Singleton().globalClips.Length)
                return;
            
            Singleton().source2.pitch = pitch;    
            Singleton().source2.PlayOneShot(Singleton().globalClips[id], vol);
            
            globalClipAvailableTime = UberManager.TimeSinceStart() + globalClipCd;
        }
    }
    
    public static void PlayClip(SoundType type, float vol, float pitch)
    {
        if(UberManager.TimeSinceStart() > globalClipAvailableTime)
        {
            int key = (int)type;
            
            if(Singleton().audioLib.ContainsKey(key))
            {
                AudioSource fx_source = Singleton().source2;
                fx_source.pitch = pitch;
                // fx_source.volume = vol;
                
                fx_source.PlayOneShot(Singleton().audioLib[key], vol);
            }
            globalClipAvailableTime = UberManager.TimeSinceStart() + globalClipCd;
        }
    }
    
    public float currentMasterPitch = 1f;
    public float targetMasterPitch = 1f;
    public float pitchInterpStartSpeed = 0.5f;
    public float pitchInterpAcceleration = 0.1f;
    
    [Header("Normal pitch settings:")]
    float normal_targetPitch = 1;
    float normal_interpSpeed = 0f;
    float normal_interpAcceleration = 3.35f;
    [Header("Dead pitch settings:")]
    float dead_targetPitch = 0.5f;
    float dead_interpSpeed = 0f;
    float dead_interpAcceleration = 2f;
    
    
    
    void PitchInterpolation()
    {
        //Master:
        if(currentMasterPitch != targetMasterPitch)
        {
            float dt  = UberManager.DeltaTime();
            float dPitch = pitchInterpStartSpeed * dt;
            
            pitchInterpStartSpeed += pitchInterpAcceleration * dt;
            
            currentMasterPitch = Mathf.MoveTowards(currentMasterPitch, targetMasterPitch, pitchInterpStartSpeed * dt);
            
            // if(dPitch > 0)
            // {
            //     if(currentMasterPitch + dPitch > targetMasterPitch)
            //     {
            //         currentMasterPitch = targetMasterPitch;
            //     }
            //     else
            //     {
            //         currentMasterPitch =+ dPitch;
            //     }
            // }
            // else
            // {
            //     if(currentMasterPitch + dPitch < targetMasterPitch)
            //     {
            //         currentMasterPitch = targetMasterPitch;
            //     }
            //     else
            //     {
            //         currentMasterPitch =+ dPitch;
            //     }
            // }
            
            // InGameConsole.LogOrange(string.Format("currentMasterPitch: {0}, dPitch: {1}", currentMasterPitch.ToString("f"), dPitch.ToString("f")));
            SetMasterPitch(currentMasterPitch);
        }
    }
    
    void DebugInput()
    {
        float dt = 0.125f * UberManager.DeltaTime();
        
        // if(state == AudioState.Normal)
        // {
        //     if(Input.GetKey(KeyCode.Keypad7))
        //     {
        //         normal_targetPitch -= dt;
        //     }
        //     if(Input.GetKey(KeyCode.Keypad9))
        //     {
        //         normal_targetPitch += dt;
        //     }
        //     if(Input.GetKey(KeyCode.Keypad4))
        //     {
        //         normal_interpSpeed -= dt;
        //     }
        //     if(Input.GetKey(KeyCode.Keypad6))
        //     {
        //         normal_interpSpeed += dt;
        //     }
        //     if(Input.GetKey(KeyCode.Keypad1))
        //     {
        //         normal_interpAcceleration -= dt;
        //     }
        //     if(Input.GetKey(KeyCode.Keypad3))
        //     {
        //         normal_interpAcceleration += dt;
        //     }
        // }
        
        // if(state == AudioState.PlayerDead)
        // {
        //     if(Input.GetKey(KeyCode.Keypad7))
        //     {
        //         dead_targetPitch -= dt;
        //     }
        //     if(Input.GetKey(KeyCode.Keypad9))
        //     {
        //         dead_targetPitch += dt;
        //     }
        //     if(Input.GetKey(KeyCode.Keypad4))
        //     {
        //         dead_interpSpeed -= dt;
        //     }
        //     if(Input.GetKey(KeyCode.Keypad6))
        //     {
        //         dead_interpSpeed += dt;
        //     }
        //     if(Input.GetKey(KeyCode.Keypad1))
        //     {
        //         dead_interpAcceleration -= dt;
        //     }
        //     if(Input.GetKey(KeyCode.Keypad3))
        //     {
        //         dead_interpAcceleration += dt;
        //     }
        // }
        
        
        // if(Inputs.GetKeyDown(KeyCode.P))
        // {
        //     if(state == AudioState.Normal)
        //     {
        //         SetState(AudioState.PlayerDead);
        //     }
        //     else
        //     {
        //         if(state == AudioState.PlayerDead)
        //         {
        //             SetState(AudioState.Normal);
        //         }
        //     }
        // }
    }
    
    public static void SetState(AudioState _state)
    {
        Singleton()._SetState(_state);
        
    }
    
    public void _SetState(AudioState _state)
    {
        if(state == _state)
        {
            return;
        }
        
        switch(_state)
        {
            case(AudioState.Normal):
            {
                SetTargetMasterPitch(normal_targetPitch, normal_interpSpeed, normal_interpAcceleration);
                break;
            }
            case(AudioState.PlayerDead):
            {
                SetTargetMasterPitch(dead_targetPitch, dead_interpSpeed, dead_interpAcceleration);
                break;
            }
        }
        state = _state;
    }
    
    public float[] samples = new float[512];
    public float[] freqBands = new float[8];
    public float[] freqBands_smoothed =  new float[8];
    
    
    [SerializeField] float currentVolume_music = 0;
    
    void CalculateFreqBands()
    {
        int count = 0;
        for(int i = 0; i < 8; i++)
        {
            float avg = 0;
            int sampleCount = (int)Mathf.Pow(2, i) * 2;
            if(i == 7)
            {
                sampleCount += 2;
            }
            for(int j = 0; j < sampleCount; j++)
            {
                avg += samples[count] * (count + 1);
                count++;
            }
            avg /= count;
            freqBands[i] = avg * 10;
        }
    }
    
    public float freqSmoothRate = 1;
    
    void CalculateSmoothFreqBands(float dt)
    {
        for(int i = 0; i < 8; i++)
        {
            float freq = freqBands[i];
            float freqSmoothed = freqBands_smoothed[i];
            if(freq > freqSmoothed)
            {
                freqSmoothed = freq;
            }
            else
            {
                freqSmoothed -= dt * freqSmoothRate;
                if(freqSmoothed < freq)
                {
                    freqSmoothed = freq;
                }
            }
            freqBands_smoothed[i] = freqSmoothed;
        }
    }
    
    public static float GetCurrentVolume()
    {
        if(Singleton() == null)
        {
            return 0;
        }
        
        return Singleton().currentVolume_music;
    }
    
    
    void GetVolumeBands()
    {
        currentVolume_music = 0;
        
        for(int i = 0; i < 8; i++)
        {
            currentVolume_music += freqBands[i];
        }
    }
    
    bool edit_audio_mat = false;
    public Material audio_mat;
    public Color audio_base_color = Color.red;
    public float audio_emission = 1;
    
    public float audio_emission_min = 0.05F;
    public float audio_emission_max = 1.4F;
    
    public float audio_emission_MaxVolume = 24F;
    
    public float emissionSmoothRate = 3f;
    
    
    int _EmissionShaderID;
    
    void EditAudioMaterials(float dt)
    {
        float t = Mathf.InverseLerp(0, audio_emission_MaxVolume, currentVolume_music);
        float lerped_audio_emission = Mathf.Lerp(audio_emission_min, audio_emission_max, t);
        
        if(lerped_audio_emission > audio_emission)
        {
            audio_emission = Mathf.MoveTowards(audio_emission, lerped_audio_emission, 6 * emissionSmoothRate * dt);
        }
        else
        {
            audio_emission = Mathf.MoveTowards(audio_emission, lerped_audio_emission, emissionSmoothRate * dt);
        }
        
        audio_mat.SetFloat(_EmissionShaderID, audio_emission);
    }
    
    void Update()
    {
        // DebugInput();
        if(Inputs.GetKeyDown(KeyCode.M))
        {
            SwitchMusicVolume();
            // source.mute = !source.mute;
        }
        
        float dt = UberManager.DeltaTime();
        
        music_src.GetSpectrumData(samples, 0, FFTWindow.Blackman);
        
        if(edit_audio_mat)
        {
            EditAudioMaterials(dt);
        }
        
        GetVolumeBands();
        CalculateFreqBands();
        CalculateSmoothFreqBands(dt);
        
        PitchInterpolation();
    }
    
    public AudioMixerSnapshot snapshot_normal;
    public AudioMixerSnapshot snapshot_muted;
    
    void SwitchMusicVolume()
    {
        if(isMuted)
        {
            snapshot_normal.TransitionTo(0.5f);
        }
        else
        {
            snapshot_muted.TransitionTo(0.5f);
        }
        
        
        isMuted = !isMuted;
        
        
        // if(music_src.volume == LOUD)
        // {
        //     music_src.volume = 0;
        //     SetMusicVolume(0);
        // }
        // else
        // {
        //     if(music_src.volume == 0f)
        //     {
        //         music_src.volume = LOUD;
        //         SetMusicVolume(LOUD);
        //     }
        // }
    }
    
    void SetMusicVolume(float vol)
    {
        music_src.volume = vol;
    }
    
    // void OnGUI()
    // {
    //     GUIStyle style = new GUIStyle(GUI.skin.label);
    //     style.alignment = TextAnchor.MiddleCenter;
    //     float rectWidth = 600;
    //     string text0 = string.Format("Current pitch: {0}; target: {1}; interp speed: {2}; acceleration: {3}", currentMasterPitch.ToString("f"), targetMasterPitch.ToString("f"), pitchInterpStartSpeed.ToString("f"), pitchInterpAcceleration.ToString("f"));
    //     GUI.Label(new Rect((Screen.width-rectWidth)/2, Screen.height - 52*1f, rectWidth, 50), text0, style);
    //     string text = string.Format("<color=yellow>Normal target pitch: {0}; Normal start speed: {1}; Normal acceleration: {2}</color>", normal_targetPitch.ToString("f"), normal_interpSpeed .ToString("f"), normal_interpAcceleration.ToString("f"));
    //     GUI.Label(new Rect((Screen.width-rectWidth)/2, Screen.height - 52*2f, rectWidth, 50), text, style);
    //     string text2 = string.Format("<color=red>Dead target pitch: {0}; Dead start speed: {1}; Dead acceleration: {2} </color>", dead_targetPitch.ToString("f"), dead_interpSpeed .ToString("f"), dead_interpAcceleration.ToString("f"));
    //     GUI.Label(new Rect((Screen.width-rectWidth)/2, Screen.height - 52*1.5f, rectWidth, 50), text2, style);
    // }
    
    public void SetMusic(string name)
    {
        return;
    }
    
    public static void SetMusicDaycore()
    {
        AudioManager inst = Singleton();
        inst.music_src.clip = inst.daycore;
        inst.music_src.pitch = 1.0f;
        inst.music_src.volume = LOUD;
        inst.music_src.loop = true;
        
        inst.music_src.Play();
    }
    
    public static void SetMusicMainMenu()
    {
        //Debug.Log("<color=yellow>SetMusicMainMenu()</color>");
        
        AudioManager inst = Singleton();
        inst.music_src.clip = inst.children_of_the_omnissiah;
        inst.music_src.pitch = 1.0f;
        
        inst.music_src.volume = LOUD;
        inst.music_src.loop = true;
        
        inst.music_src.Play();
    }
    
    public static void SetMusicOmnissiah()
    {
        AudioManager inst = Singleton();
        inst.music_src.clip = inst.daycore;
        inst.music_src.pitch = 1.0f;
        
        inst.music_src.volume = LOUD;
        inst.music_src.loop = true;
        
        inst.music_src.Play();
    }
    
    void Start()
    {
        currentMasterPitch = GetMasterPitch();
        
        music_src.clip = mainMenu_theme;
        
        music_src.loop = true;
        music_src.Stop();
        
        music_src.Play();
        
        isMuted = false;
        SwitchMusicVolume();
        
        //music_src.mute = true;
        
        SetTargetMasterPitch(normal_targetPitch, normal_interpSpeed, normal_interpAcceleration);
    }
}


public enum SoundType : int
{
    None,
    Explosion_1,
    PlayerSpawn,
    gun_pick_up,
    level_start1,
    take_dmg_npc1,
    die_npc1,
    bullet_explode_1,
    shoot_npc_1,
    player_take_damage_1,
    spawn_npc_1_sound,
    death1_npc,
    witch_death1,
    projectile_launch1,
    hurt_generic1,
    onCharTyped1,
    revolver_alt_defer,
    blink_player,
    ricochet1,
    shot2_big,
    punch_impact1,
    punch_whoosh1,
    limb_gib1,
    death_impact_gib_distorted,
    bullet_impact_sound1,
    bullet_reflect_sound
}