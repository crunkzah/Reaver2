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

public enum MusicMode
{
    Static,
    Dynamic_1,
    LowPass,
    TwoTracks
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
        
        if(audio3dPrefab == null)
        {
            InGameConsole.LogFancy("audio3dPrefab is null");
        }
        
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
    
    float gibTimeStamp = 0;
    
    void _Play3D(SoundType type, Vector3 pos, float pitch, float vol, float doppler)
    {
        if(type == SoundType.limb_gib1)
        {
            float diff = Math.Abs(Time.time - gibTimeStamp);
            if(diff < 0.033f)
            {
                
                //InGameConsole.LogFancy(string.Format("TOO MUCH GIB AUDIO, diff: <color=yellow>{0}</color>", diff));
                return;
            }
            
            gibTimeStamp = Time.time;
        }
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
    
    public AudioMixerGroup musicMasterMixer;
    
    public AudioMixerGroup musicCalmNoFilterMixer;
    public AudioMixerGroup musicCalmLowPassMixer;
    public AudioMixerGroup musicBattleMixer;
    
    
    public AudioMixerGroup effectsMixer;
    
    public ClipAndType[] clipsAndTypes;
    
    [Header("Explosions:")]
    public GameObject explosion_pooled_prefab;
    int pooled_explosions_num = 2;
    int current_pooled_explosion = 0;
    public AudioSource[] explosionsPooled;
    
    void InitExplosionsPool()
    {
        explosionsPooled = new AudioSource[pooled_explosions_num];
        
        for(int i = 0; i < pooled_explosions_num; i++)
        {
            GameObject g = Instantiate(explosion_pooled_prefab, new Vector3(2000, 2000, 2000), Quaternion.identity, this.transform);
            AudioSource a = g.GetComponent<AudioSource>();
            
            explosionsPooled[i] = a;
        }
    }
    
    public static void MakeExplosionAt(Vector3 pos, float vol, float pitch)
    {
        Singleton()._MakeExplosionAt(pos, vol, pitch);
    }
    
    void _MakeExplosionAt(Vector3 pos, float vol, float pitch)
    {
        AudioSource explosion_audio = explosionsPooled[current_pooled_explosion];
        
        explosion_audio.transform.localPosition = pos;
        
        explosion_audio.volume = vol;
        explosion_audio.pitch = pitch;
        
        explosion_audio.Play();
        current_pooled_explosion++;
        if(current_pooled_explosion >= pooled_explosions_num)
        {
            current_pooled_explosion = 0;
        }
    }
        
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
    
    
    
    [Header("Main menu theme:")]
    public AudioClip mainMenu_theme;
    [Header("Clouds:")]
    public AudioClip clouds_main;
    [Header("Prologue:")]
    public AudioClip prologue_music;
    
    
    public AudioSource main_MusicSrc;
    public AudioSource battle_MusicSrc;
    public AudioSource source2;
    [Header("Globals clips:")]
    public AudioClip[] globalClips;
    
    void Awake()
    {
        if(instance != null && instance.GetInstanceID() != this.GetInstanceID())
        {
            Destroy(this.gameObject);
        }
        else
        {
            _EmissionShaderID = Shader.PropertyToID("_Emission");
            
            //  
            
            InitAudioLib();
            InitAudioPool();
            InitExplosionsPool();
        }
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
    
    
    
    void PitchInterpolation(float dt)
    {
        //Master:
        if(currentMasterPitch != targetMasterPitch)
        {
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
    
    public static void PlayHeadshotSmall()
    {
        Singleton()._PlayHeadshotSmall();
    }
    
    public AudioClip headShotClip_small;
    public AudioSource audio2D;
    
    void _PlayHeadshotSmall()
    {
        audio2D.PlayOneShot(headShotClip_small, 1);
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
    
    public float freqSmoothRate = 1.33f;
    
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
    
    //static bool dynamicMusicMode = false;
    public MusicMode mode;
    
    public static void SetMusicMode(MusicMode _mode)
    {
        Singleton()._SetMusicMode(_mode);
    }
    
    public void _SetMusicMode(MusicMode _mode)
    {
        mode = _mode;
        
        switch(_mode)
        {
            case(MusicMode.Static):
            {
                main_MusicSrc.outputAudioMixerGroup = musicCalmNoFilterMixer;
                snapshotCalmNoFilter.TransitionTo(0.1f);
                
                break;
            }
            case(MusicMode.Dynamic_1):
            {
                snapshotCalmNoFilter.TransitionTo(0.1f);
                
                break;
            }
            case(MusicMode.LowPass):
            {
                lowPassFlag = false;
                main_MusicSrc.outputAudioMixerGroup = musicCalmLowPassMixer;
                battle_MusicSrc.outputAudioMixerGroup = musicBattleMixer;
                snapshotCalmLowPass.TransitionTo(0.1f);
                
                break;
            }
            case(MusicMode.TwoTracks):
            {
                main_MusicSrc.outputAudioMixerGroup = musicCalmNoFilterMixer;
                battle_MusicSrc.outputAudioMixerGroup = musicBattleMixer;
                snapshotTwoTracksCalm.TransitionTo(0.1f);
                
                break;
            }
        }
    }
    
    static int enemies_alive = 0;
    
    public static void ResetEnemiesAlive()
    {
        enemies_alive = 0;
        timeStampWhenEnemiesDiedOrSpawned = Time.time;
    }
    
    public static void AddEnemiesAlive(int x = 1)
    {
        if(enemies_alive <= 0)
        {
            timeStampWhenEnemiesDiedOrSpawned = Time.time;
        }
        enemies_alive += x;
    }
    
    public static void RemoveEnemiesAlive(int x = -1)
    {
        enemies_alive += x;
        if(enemies_alive <= 0)
        {
            timeStampWhenEnemiesDiedOrSpawned = Time.time;
        }
    }
    
    const float crossFadeSpeedToBattle = 3.0f;
    const float crossFadeTimeToBattle = 1 / crossFadeSpeedToBattle;
    float battleVel1;
    float battleVel2;
    
    const float crossFadeSpeedToCalm = 3f;
    const float crossFadeTimeToCalm = 1 / crossFadeSpeedToCalm;
    float calmVel1;
    float calmVel2;
    
    const float crossFadeDelay = 0f;
    const float decreaseFadeMult = 1.0f;
    static float timeStampWhenEnemiesDiedOrSpawned = float.MaxValue;
    
    
    // void CrossFadeDynamicMusic(float dt)
    // {
    //     if(enemies_alive > 0)
    //     {
    //         battle_MusicSrc.volume = Mathf.SmoothDamp(battle_MusicSrc.volume, 1, ref battleVel1, crossFadeTimeToBattle);
    //         calm_MusicSrc.volume = Mathf.SmoothDamp(calm_MusicSrc.volume, 0, ref battleVel2, crossFadeTimeToBattle);
    //     }
    //     else
    //     {
    //         if(Time.time - timeStampWhenEnemiesDiedOrSpawned < crossFadeDelay)
    //         {
    //             return;
    //         }
    //         battle_MusicSrc.volume = Mathf.SmoothDamp(battle_MusicSrc.volume, 0, ref calmVel1, crossFadeTimeToCalm);
    //         calm_MusicSrc.volume = Mathf.SmoothDamp(calm_MusicSrc.volume, 1, ref calmVel2, crossFadeTimeToCalm);
    //     }
    // }
    
    
    
    [Header("DIMA DIMA:")]
    float lowPassTransitionDurationToBattle = 0f;
    float lowPassTransitionDurationToCalm = 0.1f;
    bool lowPassFlag = false;
    
    void CrossFadeLowPassMusic()
    {
        if(enemies_alive > 0)
        {
            if(!lowPassFlag)
            {
                InGameConsole.LogFancy("Transitioning to BATTLE MUSIC!!!!");
                InGameConsole.LogFancy(snapshotBattle.name);
                
                
                snapshotBattle.TransitionTo(lowPassTransitionDurationToBattle);
                
                lowPassFlag = true;
            }
        }
        else
        {
            // if(Time.time - timeStampWhenEnemiesDiedOrSpawned < crossFadeDelay)
            // {
            //     return;
            // }
            if(lowPassFlag)
            {
                snapshotCalmLowPass.TransitionTo(lowPassTransitionDurationToCalm);
                
                InGameConsole.LogFancy("Transitioning to calm low pass music...");
                InGameConsole.LogFancy(snapshotCalmLowPass.name);
                
                lowPassFlag = false;
                
            }
        }
    }
    
    void Update()
    {
        // DebugInput();
        // if(Inputs.GetKeyDown(KeyCode.M))
        // {
        //     SwitchMusicVolume();
        //     // source.mute = !source.mute;
        // }
        
        float dt = UberManager.DeltaTime();
        
        //musicMixer.GetS
        main_MusicSrc.GetSpectrumData(samples, 0, FFTWindow.Blackman);
        
        // if(edit_audio_mat)
        // {
        //     EditAudioMaterials(dt);
        // }
        
        switch(mode)
        {
            case(MusicMode.Static):
            {
                break;
            }
            case(MusicMode.Dynamic_1):
            {
                // CrossFadeDynamicMusic(dt);
                break;
            }
            case(MusicMode.LowPass):
            {
                CrossFadeLowPassMusic();
                break;
            }
            case(MusicMode.TwoTracks):
            {
                break;                
            }
        }
        
        GetVolumeBands();
        CalculateFreqBands();
        CalculateSmoothFreqBands(dt);
        
        PitchInterpolation(dt);
    }
    
    public AudioMixerSnapshot snapshotCalmNoFilter;
    public AudioMixerSnapshot snapshotCalmLowPass;
    public AudioMixerSnapshot snapshotBattle;
    
    public AudioMixerSnapshot snapshotTwoTracksCalm;
    public AudioMixerSnapshot snapshotTwoTracksBattle;
    
    void SwitchMusicVolume()
    {
        
        
        // if(isMuted)
        // {
        //     snapshot_normal.TransitionTo(0.5f);
        // }
        // else
        // {
        //     snapshot_muted.TransitionTo(0.5f);
        // }
        
        
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
    
    void SetMusicVolume(float vol_db)
    {
        musicMasterMixer.audioMixer.SetFloat("MusicVolume", vol_db);
        //calm_MusicSrc.volume = vol;
    }
    
    void OnGUI()
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.alignment = TextAnchor.MiddleCenter;
        style.fontSize = 25;
        float rectWidth = 600;
     //   string text0 = string.Format("Current pitch: {0}; target: {1}; interp speed: {2}; acceleration: {3}", currentMasterPitch.ToString("f"), targetMasterPitch.ToString("f"), pitchInterpStartSpeed.ToString("f"), pitchInterpAcceleration.ToString("f"));
        
    //     GUI.Label(new Rect((Screen.width-rectWidth)/2, Screen.height - 52*1f, rectWidth, 50), calm_MusicSrc.volume.ToString("f"), style);
    //    // string text = string.Format("<color=yellow>Normal target pitch: {0}; Normal start speed: {1}; Normal acceleration: {2}</color>", normal_targetPitch.ToString("f"), normal_interpSpeed .ToString("f"), normal_interpAcceleration.ToString("f"));
    //     GUI.Label(new Rect((Screen.width-rectWidth)/2, Screen.height - 52*2f, rectWidth, 50), battle_MusicSrc.volume.ToString("f"), style);
      //  string text2 = string.Format("<color=red>Dead target pitch: {0}; Dead start speed: {1}; Dead acceleration: {2} </color>", dead_targetPitch.ToString("f"), dead_interpSpeed .ToString("f"), dead_interpAcceleration.ToString("f"));
      //  GUI.Label(new Rect((Screen.width-rectWidth)/2, Screen.height - 52*1.5f, rectWidth, 50), text2, style);
    }
    
    public void SetMusic(string name)
    {
        return;
    }
    
    public static void SetMusicHaze()
    {
        // AudioManager inst = Singleton();
        // inst.calm_MusicSrc.clip = inst.haze;
        // inst.calm_MusicSrc.pitch = 1.0f;
        // inst.calm_MusicSrc.volume = LOUD;
        // inst.calm_MusicSrc.loop = true;
        
        // inst.calm_MusicSrc.Play();
    }
    
    public static AudioSource GetMainMusicSrc()
    {
        return Singleton().main_MusicSrc;
    }
    
    
    public static AudioSource GetBattleMusicSrc()
    {
        return Singleton().battle_MusicSrc;
    }
    
    public static void StopMusic()
    {
        AudioManager inst = Singleton();
        
        inst.main_MusicSrc.Stop();
        inst.battle_MusicSrc.Stop();
    }
    
    public static void SetMusicClouds()
    {
        SetMusicMode(MusicMode.Static);
        
        AudioManager inst = Singleton();
         
        inst.main_MusicSrc.Stop();
        inst.main_MusicSrc.clip = inst.clouds_main;
        inst.main_MusicSrc.Play();
        
        
        //inst.calm_MusicSrc.Play();
        
        inst.battle_MusicSrc.Stop();
        inst.battle_MusicSrc.loop = true;
        
        //inst.battle_MusicSrc.Play();
    }
    
    public static void SetMusicPrologue()
    {
        //SetDynamicMusicModeOn();
        SetMusicMode(MusicMode.LowPass);
        AudioManager inst = Singleton();
        
        inst.main_MusicSrc.Stop();
        inst.main_MusicSrc.clip = inst.prologue_music;
        inst.main_MusicSrc.Play();
        
        
        inst.battle_MusicSrc.clip = inst.prologue_music;
        inst.battle_MusicSrc.Play();
    }
    
    
    
    public static void SetMusicMainMenu()
    {
        InGameConsole.LogFancy("<color=yellow>SetMusicMainMenu()</color>");
        InGameConsole.LogFancy("<color=yellow>SetMusicMainMenu()</color>");
        InGameConsole.LogFancy("<color=yellow>SetMusicMainMenu()</color>");
        InGameConsole.LogFancy("<color=yellow>SetMusicMainMenu()</color>");
        InGameConsole.LogFancy("<color=yellow>SetMusicMainMenu()</color>");
        
        SetMusicMode(MusicMode.Static);
        AudioManager inst = Singleton();
        
        inst.main_MusicSrc.Stop();
        inst.main_MusicSrc.clip = inst.mainMenu_theme;
        inst.main_MusicSrc.Play();
        
        inst.battle_MusicSrc.Stop();
    }
    
    void Start()
    {
        currentMasterPitch = GetMasterPitch();
        
        isMuted = false;
        SetMusicMainMenu();
        //SwitchMusicVolume();
        
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
    bullet_reflect_sound,
    ui_blip1,
    checkpoint_sound,
    shoot2,
    spawn_npc_2_sound
}