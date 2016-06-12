using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    private SoundManagerSettings _settings;

    List<SMSoundHandler> _sounds = new List<SMSoundHandler>();

    SMMusicHandler _music;
    string _currentMusicName;

    List<SMMusicFadingOut> _musicFadingsOut = new List<SMMusicFadingOut>();


#region Public functions

    public static void PlayMusic(string name)
    {
        Instance.PlayMusicInternal(name);
    }

    public static void StopMusic()
    {
        Instance.StopMusicInternal();
    }

    public static void PlaySound(string name, bool pausable = true)
    {
        Instance.PlaySoundInternal(name, pausable);
    }

    public static void PlaySoundWithDelay(string name, float delay, bool pausable = true)
    {
        Instance.PlaySoundWithDelayInternal(name, delay, pausable);
    }

    public static void Pause()
    {
        if (Instance._settings.AutoPause)
            return;

        // Supress Unreachable code warning
#pragma warning disable
        AudioListener.pause = true;
#pragma warning restore
    }

    public static void UnPause()
    {
        if (Instance._settings.AutoPause)
            return;

        // Supress Unreachable code warning
#pragma warning disable
        AudioListener.pause = false;
#pragma warning restore
    }

    public static void StopAllPausableSounds()
    {
        Instance.StopAllPausableSoundsInternal();
    }

    // Volume [0 - 1]
    public static void SetMusicVolume(float volume)
    {
        Instance._settings.SetMusicVolumeInternal(volume);
        Instance.ApplyMusicVolume();
    }

    // Volume [0 - 1]
    public static float GetMusicVolume()
    {
        return Instance._settings.GetMusicVolumeInternal();
    }

    public static void SetMusicMuted(bool mute)
    {
        Instance._settings.SetMusicMuted(mute);
        Instance.ApplyMusicMuted();
    }

    public static bool GetMusicMuted()
    {
        return Instance._settings.GetMusicMuted();
    }

    // Volume [0 - 1]
    public static void SetSoundVolume(float volume)
    {
        Instance._settings.SetSoundVolumeInternal(volume);
        Instance.ApplySoundVolume();
    }

    // Volume [0 - 1]
    public static float GetSoundVolume()
    {
        return Instance._settings.GetSoundVolumeInternal();
    }

    public static void SetSoundMuted(bool mute)
    {
        Instance._settings.SetSoundMuted(mute);
        Instance.ApplySoundMuted();
    }

    public static bool GetSoundMuted()
    {
        return Instance._settings.GetSoundMuted();
    }

#endregion

#region Singleton
    private static SoundManager _instance;

    public static SoundManager Instance
    {
        get
        {
            if (applicationIsQuitting)
            {
                Debug.LogWarning("[Singleton] Instance '" + typeof(SoundManager) +
                    "' already destroyed on application quit." +
                    " Won't create again - returning null.");
                return null;
            }

            if (_instance != null)
            {
                return _instance;
            }

            return new GameObject("SoundManager (singleton)").AddComponent<SoundManager>();
        }
    }

    private static bool applicationIsQuitting = false;

    /// <summary>
    /// When Unity quits, it destroys objects in a random order.
    /// In principle, a Singleton is only destroyed when application quits.
    /// If any script calls Instance after it have been destroyed, 
    ///   it will create a buggy ghost object that will stay on the Editor scene
    ///   even after stopping playing the Application. Really bad!
    /// So, this was made to be sure we're not creating that buggy ghost object.
    /// </summary>
    public void OnDestroy()
    {
        applicationIsQuitting = true;
    }
#endregion

#region Settings


#endregion // Settings

#region Music

    void PlayMusicInternal(string musicName)
    {
        if (string.IsNullOrEmpty(musicName)) {
            Debug.Log("Music empty or null");
            return;
        }

        if (_currentMusicName == musicName) {
            Debug.Log("Music already playing: " + musicName);
            return;
        }

        StopMusicInternal();

        _currentMusicName = musicName;

        AudioClip musicClip = LoadClip("Music/" + musicName);

        GameObject music = new GameObject("Music: " + musicName);
        AudioSource musicSource = music.AddComponent<AudioSource>();

        music.transform.parent = transform;

        musicSource.outputAudioMixerGroup = _settings.MusicAudioMixerGroup;
        
        musicSource.loop = true;
        musicSource.priority = 0;
        musicSource.playOnAwake = false;
        musicSource.mute = _settings.GetMusicMuted();
        musicSource.ignoreListenerPause = true;
        musicSource.clip = musicClip;
        musicSource.Play();

        musicSource.volume = 0;

        _music = new SMMusicHandler();
        _music.Source = musicSource;
        _music.FadingIn = true;
        _music.TargetVolume = _settings.GetMusicVolume();
        _music.Timer = 0;
        _music.FadingTime = _settings.MusicFadeTime;
    }

    void StopMusicInternal()
    {
        _currentMusicName = "";
        if (_music != null)
        {
            StartFadingOutMusic();
            _music = null;
        }
    }

#endregion // Music

#region Sound

    void PlaySoundInternal(string soundName, bool pausable)
    {
        if (string.IsNullOrEmpty(soundName)) {
            Debug.Log("Sound null or empty");
            return;
        }

        int sameCountGuard = 0;
        foreach (SMSoundHandler sound in _sounds)
        {
            if (sound.Source.clip.name == soundName)
                sameCountGuard++;
        }

        if (sameCountGuard > 8)
        {
            Debug.Log("Too much duplicates for sound: " + soundName);
            return;
        }

        if (_sounds.Count > 16) {
            Debug.Log("Too much sounds");
            return;
        }

        StartCoroutine(PlaySoundInternalSoon(soundName, pausable));
    }

    IEnumerator PlaySoundInternalSoon(string soundName, bool pausable)
    {
        ResourceRequest request = LoadClipAsync("Sounds/" + soundName);
        while (!request.isDone)
        {
            yield return null;
        }

        AudioClip soundClip = (AudioClip)request.asset;
        if (null == soundClip)
        {
            Debug.Log("Sound not loaded: " + soundName);
        }

        GameObject sound = new GameObject("Sound: " + soundName);
        AudioSource soundSource = sound.AddComponent<AudioSource>();
        sound.transform.parent = transform;

        soundSource.outputAudioMixerGroup = _settings.SoundAudioMixerGroup;
        soundSource.priority = 128;
        soundSource.playOnAwake = false;
        soundSource.mute = _settings.GetSoundMuted();
        soundSource.volume = _settings.GetSoundVolume();
        soundSource.clip = soundClip;
        soundSource.Play();
        soundSource.ignoreListenerPause = !pausable;

        SMSoundHandler soundHandler = new SMSoundHandler();
        soundHandler.Source = soundSource;

        _sounds.Add(soundHandler);
    }

    void PlaySoundWithDelayInternal(string soundName, float delay, bool pausable)
    {
        StartCoroutine(PlaySoundWithDelayCoroutine(soundName, delay, pausable));
    }

    void StopAllPausableSoundsInternal()
    {
        foreach (SMSoundHandler sound in _sounds)
        {
            if (!sound.Source.ignoreListenerPause)
                sound.Source.Stop();
        }
    }

    #endregion // Sound

#region Internal

    void Awake()
    {
        // Only one instance of SoundManager at a time!
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        _settings = Resources.Load<SoundManagerSettings>("SoundManagerSettings");
        if (_settings == null)
        {
            Debug.LogWarning("SoundManagerSettings not foundin resources. Using default settings");
            _settings = ScriptableObject.CreateInstance<SoundManagerSettings>();
        }

        _settings.LoadSettings();

        ApplySoundVolume();
        ApplyMusicVolume();

        ApplySoundMuted();
        ApplyMusicMuted();
    }

    void Update()
    {
        var soundsToDelete = _sounds.FindAll(sound => !sound.Source.isPlaying);

        foreach (SMSoundHandler sound in soundsToDelete)
        {
            _sounds.Remove(sound);
            Destroy(sound.Source.gameObject);
        }

        if (_settings.AutoPause)
        {
            bool curPause = Time.timeScale < 0.1f;
            if (curPause != AudioListener.pause)
            {
                AudioListener.pause = curPause;
            }
        }

        for (int i = 0; i < _musicFadingsOut.Count ; i++)
        {
            SMMusicFadingOut music = _musicFadingsOut[i];
            if (music.Source == null)
            {
                _musicFadingsOut.RemoveAt(i);
                i--;
            }
            else
            {
                music.Timer += Time.unscaledDeltaTime;
                _musicFadingsOut[i] = music;
                if (music.Timer >= music.FadingTime)
                {
                    Destroy(music.Source.gameObject);
                    _musicFadingsOut.RemoveAt(i);
                    i--;
                }
                else
                {
                    float k = Mathf.Clamp01(music.Timer / music.FadingTime);
                    music.Source.volume = Mathf.Lerp(music.StartVolume, 0, k);
                }
            }
        }

        if (_music != null && _music.FadingIn)
        {
            _music.Timer += Time.unscaledDeltaTime;
            if (_music.Timer >= _music.FadingTime)
            {
                _music.Source.volume = _music.TargetVolume;
                _music.FadingIn = false;
            }
            else
            {
                float k = Mathf.Clamp01(_music.Timer / _music.FadingTime);
                _music.Source.volume = Mathf.Lerp(0, _music.TargetVolume, k);
            }
        }
    }

    void StartFadingOutMusic()
    {
        if (_music != null)
        {
            SMMusicFadingOut fader = new SMMusicFadingOut();
            fader.Source = _music.Source;
            fader.FadingTime = _settings.MusicFadeTime;
            fader.Timer = 0;
            fader.StartVolume = _music.Source.volume;
            _musicFadingsOut.Add(fader);
        }
    }

    private IEnumerator PlaySoundWithDelayCoroutine(string name, float delay, bool pausable)
    {
        float timer = delay;
        while (timer > 0)
        {
            timer -= pausable ? Time.deltaTime : Time.unscaledDeltaTime;
            yield return null;
        }

        PlaySound(name, pausable);
    }

    AudioClip LoadClip(string name)
    {
        string path = "SoundManager/" + name;
        AudioClip clip = Resources.Load<AudioClip>(path);
        return clip;
    }

    ResourceRequest LoadClipAsync(string name)
    {
        string path = "SoundManager/" + name;
        return Resources.LoadAsync<AudioClip>(path);
    }


    void ApplySoundVolume()
    {
        foreach (SMSoundHandler sound in _sounds)
        {
            sound.Source.volume = _settings.GetSoundVolume();
        }
    }

    void ApplyMusicVolume()
    {
        if (_music != null)
        {
            _music.FadingIn = false;
            _music.Source.volume = _music.TargetVolume;
        }
    }

    void ApplySoundMuted()
    {
        foreach (SMSoundHandler sound in _sounds)
        {
            sound.Source.mute = _settings.GetSoundMuted();
        }
    }

    void ApplyMusicMuted()
    {
        if (_music != null)
        {
            _music.Source.mute = _settings.GetMusicMuted();
        }
    }

#endregion // Internal
}
