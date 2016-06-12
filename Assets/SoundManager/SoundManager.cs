using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    private SoundManagerSettings _settings;


    List<SMSoundHandler> _sounds = new List<SMSoundHandler>();
    AudioSource _currentMusicSource;

    string _currentMusicName;

    List<SMMusicHandler> _musicFadings = new List<SMMusicHandler>();

    float _volumeMusic;
    float _volumeSound;

    bool _mutedMusic;
    bool _mutedSound;

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
        Instance.SetMusicVolumeInternal(volume);
    }

    // Volume [0 - 1]
    public static float GetMusicVolume()
    {
        return Instance.GetMusicVolumeInternal();
    }

    public static void SetMusicMuted(bool mute)
    {
        Instance.SetMusicMutedInternal(mute);
    }

    public static bool GetMusicMuted()
    {
        return Instance.GetMusicMutedInternal();
    }

    // Volume [0 - 1]
    public static void SetSoundVolume(float volume)
    {
        Instance.SetSoundVolumeInternal(volume);
    }

    // Volume [0 - 1]
    public static float GetSoundVolume()
    {
        return Instance.GetSoundVolumeInternal();
    }

    public static void SetSoundMuted(bool mute)
    {
        Instance.SetSoundMutedInternal(mute);
    }

    public static bool GetSoundMuted()
    {
        return Instance.GetSoundMutedInternal();
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

    void SetMusicVolumeInternal(float volume)
    {
        _volumeMusic = volume;
        SaveSettings();
        ApplyMusicVolume();
    }

    float GetMusicVolumeInternal()
    {
        return _volumeMusic;
    }

    void SetMusicMutedInternal(bool mute)
    {
        _mutedMusic = mute;
        SaveSettings();
        ApplyMusicMuted();
    }

    bool GetMusicMutedInternal()
    {
        return _mutedMusic;
    }

    void SetSoundVolumeInternal(float volume)
    {
        _volumeSound = volume;
        SaveSettings();
        ApplySoundVolume();
    }

    float GetSoundVolumeInternal()
    {
        return _volumeSound;
    }

    void SetSoundMutedInternal(bool mute)
    {
        _mutedSound = mute;
        SaveSettings();
        ApplySoundMuted();
    }

    bool GetSoundMutedInternal()
    {
        return _mutedSound;
    }

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
        musicSource.mute = _mutedMusic;
        musicSource.ignoreListenerPause = true;
        musicSource.clip = musicClip;
        musicSource.Play();

        musicSource.volume = 0;
        StartFadeMusic(musicSource, _settings.MusicFadeTime, _volumeMusic * _settings.DefaultMusicVolume, false);
        //musicSource.DOFade(_volumeMusic * DefaultMusicVolume, MusicFadeTime).SetUpdate(true);

        _currentMusicSource = musicSource;
    }

    void StopMusicInternal()
    {
        _currentMusicName = "";
        if (_currentMusicSource != null)
        {
            StartFadeMusic(_currentMusicSource, _settings.MusicFadeTime, 0, true);
            _currentMusicSource = null;
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
        soundSource.mute = _mutedSound;
        soundSource.volume = _volumeSound * _settings.DefaultSoundVolume;
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

        LoadSettings();
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

        for (int i = 0; i < _musicFadings.Count ; i++)
        {
            SMMusicHandler music = _musicFadings[i];
            if (music.Source == null)
            {
                _musicFadings.RemoveAt(i);
                i--;
            }
            else
            {
                music.timer += Time.unscaledDeltaTime;
                _musicFadings[i] = music;
                if (music.timer >= music.fadingTime)
                {
                    music.Source.volume = music.targetVolume;
                    if (music.destroyOnComplete)
                    {
                        Destroy(music.Source.gameObject);
                    }
                    _musicFadings.RemoveAt(i);
                    i--;
                }
                else
                {
                    float k = Mathf.Clamp01(music.timer / music.fadingTime);
                    music.Source.volume = Mathf.Lerp(music.startVolume, music.targetVolume, k);
                }
            }
        }
    }

    void StopFadingForMusic(AudioSource music)
    {
        for (int i = 0; i < _musicFadings.Count; i++)
        {
            SMMusicHandler fader = _musicFadings[i];
            if (fader.Source == music)
            {
                if (fader.destroyOnComplete)
                {
                    Destroy(fader.Source.gameObject);
                }
                _musicFadings.RemoveAt(i);
                return;
            }
        }
    }
    void StartFadeMusic(AudioSource music, float duration, float targetVolume, bool destroyOnComplete)
    {
        SMMusicHandler fader = new SMMusicHandler();
        fader.Source = music;
        fader.fadingTime = duration;
        fader.timer = 0;
        fader.startVolume = music.volume;
        fader.targetVolume = targetVolume;
        fader.destroyOnComplete = destroyOnComplete;
        _musicFadings.Add(fader);
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

    void SaveSettings()
    {
        PlayerPrefs.SetFloat("SM_MusicVolume", _volumeMusic);
        PlayerPrefs.SetFloat("SM_SoundVolume", _volumeSound);

        PlayerPrefs.SetInt("SM_MusicMute", _mutedMusic ? 1 : 0);
        PlayerPrefs.SetInt("SM_SoundMute", _mutedSound ? 1 : 0);
    }

    void LoadSettings()
    {
        _volumeMusic = PlayerPrefs.GetFloat("SM_MusicVolume", 1);
        _volumeSound = PlayerPrefs.GetFloat("SM_SoundVolume", 1);

        _mutedMusic = PlayerPrefs.GetInt("SM_MusicMute", 0) == 1;
        _mutedSound = PlayerPrefs.GetInt("SM_SoundMute", 0) == 1;

        ApplySoundVolume();
        ApplyMusicVolume();

        ApplySoundMuted();
        ApplyMusicMuted();
    }

    void ApplySoundVolume()
    {
        foreach (SMSoundHandler sound in _sounds)
        {
            sound.Source.volume = _volumeSound * _settings.DefaultSoundVolume;
        }
    }

    void ApplyMusicVolume()
    {
        if (_currentMusicSource != null)
        {
            StopFadingForMusic(_currentMusicSource);
            _currentMusicSource.volume = _volumeMusic * _settings.DefaultMusicVolume;
        }
    }

    void ApplySoundMuted()
    {
        foreach (SMSoundHandler sound in _sounds)
        {
            sound.Source.mute = _mutedSound;
        }
    }

    void ApplyMusicMuted()
    {
        if (_currentMusicSource != null)
        {
            _currentMusicSource.mute = _mutedMusic;
        }
    }

#endregion // Internal
}
