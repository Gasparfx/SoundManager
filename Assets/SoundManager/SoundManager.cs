using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    const bool AutoPause = true;

    const float DefaultMusicVolume = 1f;
    const float DefaultSoundVolume = 0.9f;
    const float DefaultVoiceVolume = 1f;

    const float MusicFadeTime = 2f;

    public string languageFolder = "";

    public AudioMixerGroup musicAudioMixerGroup;
    public AudioMixerGroup soundAudioMixerGroup;
    public AudioMixerGroup voiceAudioMixerGroup;

    List<AudioSource> _sounds = new List<AudioSource>();
    AudioSource _currentMusicSource;
    AudioSource _currentVoiceSource;

    bool _voiceBusy;
    bool _currentVoiceKilled;
    string _currentMusicName;

    bool isFading;
    float fadeTimer;

    struct NextVoice
    {
        public string name;
        public float delay;
    }

    List<NextVoice> _voicesQueue = new List<NextVoice>();

    struct MusicFader
    {
        public AudioSource audio;
        public float timer;
        public float fadingTime;
        public float startVolume;
        public float targetVolume;
        public bool destroyOnComplete;
    }

    List<MusicFader> _musicFadings = new List<MusicFader>();

    float _volumeMusic;
    float _volumeSound;
    float _volumeVoice;

    bool _mutedMusic;
    bool _mutedSound;
    bool _mutedVoice;

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

    public static void PlayVoice(string voiceName, float delay = 0, bool killOtherVoices = true)
    {
        Instance.PlayVoiceInternal(voiceName, delay, killOtherVoices);
    }

    public static void StopVoices()
    {
        Instance.StopVoicesInternal();
    }

    public static bool IsVoicePlaying()
    {
        return Instance.IsVoicePlayingInternal();
    }

    public static void Pause()
    {
        if (AutoPause)
            return;

        // Supress Unreachable code warning
#pragma warning disable
        AudioListener.pause = true;
#pragma warning restore
    }

    public static void UnPause()
    {
        if (AutoPause)
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

    // Volume [0 - 1]
    public static void SetVoiceVolume(float volume)
    {
        Instance.SetVoiceVolumeInternal(volume);
    }

    // Volume [0 - 1]
    public static float GetVoiceVolume()
    {
        return Instance.GetVoiceVolumeInternal();
    }

    public static void SetVoiceMuted(bool mute)
    {
        Instance.SetVoiceMutedInternal(mute);
    }

    public static bool GetVoiceMuted()
    {
        return Instance.GetVoiceMutedInternal();
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

            // Do not modify _instance here. It will be assigned in awake
            GameObject prefabFromResources = Resources.Load<GameObject>("SoundManager/SoundManager");
            if (prefabFromResources != null)
            {
                prefabFromResources = Instantiate<GameObject>(prefabFromResources);
                prefabFromResources.name = "SoundManager (singleton)";
                return prefabFromResources.GetComponent<SoundManager>();
            }

            Debug.LogWarning("SoundManager prefab not found in Resources. It will be created with default settings.");
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

    void SetVoiceVolumeInternal(float volume)
    {
        _volumeVoice = volume;
        SaveSettings();
        ApplyVoiceVolume();
    }

    float GetVoiceVolumeInternal()
    {
        return _volumeVoice;
    }

    void SetVoiceMutedInternal(bool mute)
    {
        _mutedVoice = mute;
        SaveSettings();
        ApplyVoiceMuted();
    }

    bool GetVoiceMutedInternal()
    {
        return _mutedVoice;
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

        musicSource.outputAudioMixerGroup = musicAudioMixerGroup;
        
        musicSource.loop = true;
        musicSource.priority = 0;
        musicSource.playOnAwake = false;
        musicSource.mute = _mutedMusic;
        musicSource.ignoreListenerPause = true;
        musicSource.clip = musicClip;
        musicSource.Play();

        musicSource.volume = 0;
        StartFadeMusic(musicSource, MusicFadeTime, _volumeMusic * DefaultMusicVolume, false);
        //musicSource.DOFade(_volumeMusic * DefaultMusicVolume, MusicFadeTime).SetUpdate(true);

        _currentMusicSource = musicSource;
    }

    void StopMusicInternal()
    {
        _currentMusicName = "";
        if (_currentMusicSource != null)
        {
            StartFadeMusic(_currentMusicSource, MusicFadeTime, 0, true);
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
        foreach (AudioSource audioSource in _sounds)
        {
            if (audioSource.clip.name == soundName)
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

        //PlaySoundInternalNow(soundName, pausable);
        StartCoroutine(PlaySoundInternalSoon(soundName, pausable));
    }

    void PlaySoundInternalNow(string soundName, bool pausable)
    {
        AudioClip soundClip = LoadClip("Sounds/" + soundName);
        if (null == soundClip)
        {
            Debug.Log("Sound not loaded: " + soundName);
        }

        GameObject sound = new GameObject("Sound: " + soundName);
        AudioSource soundSource = sound.AddComponent<AudioSource>();
        sound.transform.parent = transform;

        soundSource.outputAudioMixerGroup = soundAudioMixerGroup;
        soundSource.priority = 128;
        soundSource.playOnAwake = false;
        soundSource.mute = _mutedSound;
        soundSource.volume = _volumeSound * DefaultSoundVolume;
        soundSource.clip = soundClip;
        soundSource.Play();
        soundSource.ignoreListenerPause = !pausable;

        _sounds.Add(soundSource);
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

        soundSource.outputAudioMixerGroup = soundAudioMixerGroup;
        soundSource.priority = 128;
        soundSource.playOnAwake = false;
        soundSource.mute = _mutedSound;
        soundSource.volume = _volumeSound * DefaultSoundVolume;
        soundSource.clip = soundClip;
        soundSource.Play();
        soundSource.ignoreListenerPause = !pausable;

        _sounds.Add(soundSource);
    }

    void PlaySoundWithDelayInternal(string soundName, float delay, bool pausable)
    {
        StartCoroutine(PlaySoundWithDelayCoroutine(soundName, delay, pausable));
    }

    void StopAllPausableSoundsInternal()
    {
        foreach (AudioSource sound in _sounds)
        {
            if (!sound.ignoreListenerPause)
                sound.Stop();
        }
    }

    #endregion // Sound

#region Voice

    void StopVoicesInternal()
    {
        KillAllVoices();
    }

    void PlayVoiceInternal(string voiceName, float delay = 0, bool killOtherVoices = true)
    {
        if (killOtherVoices)
            KillAllVoices();

        if (string.IsNullOrEmpty(voiceName))
            return;

        NextVoice next = new NextVoice();
        next.name = voiceName;
        next.delay = delay;

        _voicesQueue.Add(next);
    }

    bool IsVoicePlayingInternal()
    {
        return _voiceBusy;
    }

#endregion // Voice

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

        LoadSettings();
    }

    void Start()
    {
        StartCoroutine(VoicesCoroutine());
    }


    IEnumerator VoicesCoroutine()
    {
        while (true)
        {
            _voiceBusy = false;
            // Wait next voice
            while (_voicesQueue.Count == 0)
            {
                yield return null;
            }
            _voiceBusy = true;

            NextVoice next = _voicesQueue[0];
            _voicesQueue.RemoveAt(0);
            _currentVoiceKilled = false;

            float timer = next.delay;
            while (timer > 0 && !_currentVoiceKilled)
            {
                timer -= Time.deltaTime;
                yield return null;
            }

            if (_currentVoiceKilled)
                continue;


            string voiceFile;
            if (string.IsNullOrEmpty(languageFolder))
            {
                voiceFile = "Voices/" + next.name;
            } else
            {
                voiceFile = "Voices/" + languageFolder + "/" + next.name;
            }

            // Play voice
            ResourceRequest request = LoadClipAsync(voiceFile);
            while (!request.isDone)
            {
                yield return null;
            }

            AudioClip voiceClip = (AudioClip) request.asset;

            if (_currentVoiceKilled)
                continue;

            GameObject voice = new GameObject("Voice: " + next.name);

            voice.transform.parent = transform;

            _currentVoiceSource = voice.AddComponent<AudioSource>();
            _currentVoiceSource.outputAudioMixerGroup = voiceAudioMixerGroup;
            _currentVoiceSource.priority = 64;
            _currentVoiceSource.playOnAwake = false;
            _currentVoiceSource.mute = _mutedVoice;
            _currentVoiceSource.volume = _volumeVoice * DefaultVoiceVolume;
            _currentVoiceSource.ignoreListenerPause = false;
            _currentVoiceSource.clip = voiceClip;
            _currentVoiceSource.Play();

            // Wait voice over or killed
            while (_currentVoiceSource.isPlaying)
            {
                if (_currentVoiceKilled)
                {
                    _currentVoiceSource.Stop();
                    break;
                }
                yield return null;
            }

            _currentVoiceSource = null;
            Destroy(voice);

            yield return null;
        }

    }

    void Update()
    {
        var soundsToDelete = _sounds.FindAll(sound => !sound.isPlaying);

        foreach (AudioSource sound in soundsToDelete)
        {
            _sounds.Remove(sound);
            Destroy(sound.gameObject);
        }

        if (AutoPause)
        {
            bool curPause = Time.timeScale < 0.1f;
            if (curPause != AudioListener.pause)
            {
                AudioListener.pause = curPause;
            }
        }

        for (int i = 0; i < _musicFadings.Count ; i++)
        {
            MusicFader music = _musicFadings[i];
            if (music.audio == null)
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
                    music.audio.volume = music.targetVolume;
                    if (music.destroyOnComplete)
                    {
                        Destroy(music.audio.gameObject);
                    }
                    _musicFadings.RemoveAt(i);
                    i--;
                }
                else
                {
                    float k = Mathf.Clamp01(music.timer / music.fadingTime);
                    music.audio.volume = Mathf.Lerp(music.startVolume, music.targetVolume, k);
                }
            }
        }
    }

    void StopFadingForMusic(AudioSource music)
    {
        for (int i = 0; i < _musicFadings.Count; i++)
        {
            MusicFader fader = _musicFadings[i];
            if (fader.audio == music)
            {
                if (fader.destroyOnComplete)
                {
                    Destroy(fader.audio.gameObject);
                }
                _musicFadings.RemoveAt(i);
                return;
            }
        }
    }
    void StartFadeMusic(AudioSource music, float duration, float targetVolume, bool destroyOnComplete)
    {
        MusicFader fader;
        fader.audio = music;
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

    void KillAllVoices()
    {
        _currentVoiceKilled = true;
        _voicesQueue.Clear();
    }

    void SaveSettings()
    {
        PlayerPrefs.SetFloat("SM_MusicVolume", _volumeMusic);
        PlayerPrefs.SetFloat("SM_SoundVolume", _volumeSound);
        PlayerPrefs.SetFloat("SM_VoiceVolume", _volumeVoice);

        PlayerPrefs.SetInt("SM_MusicMute", _mutedMusic ? 1 : 0);
        PlayerPrefs.SetInt("SM_SoundMute", _mutedSound ? 1 : 0);
        PlayerPrefs.SetInt("SM_VoiceMute", _mutedVoice ? 1 : 0);
    }

    void LoadSettings()
    {
        _volumeMusic = PlayerPrefs.GetFloat("SM_MusicVolume", 1);
        _volumeSound = PlayerPrefs.GetFloat("SM_SoundVolume", 1);
        _volumeVoice = PlayerPrefs.GetFloat("SM_VoiceVolume", 1);

        _mutedMusic = PlayerPrefs.GetInt("SM_MusicMute", 0) == 1;
        _mutedSound = PlayerPrefs.GetInt("SM_SoundMute", 0) == 1;
        _mutedVoice = PlayerPrefs.GetInt("SM_VoiceMute", 0) == 1;

        ApplySoundVolume();
        ApplyMusicVolume();
        ApplyVoiceVolume();

        ApplySoundMuted();
        ApplyMusicMuted();
        ApplyVoiceMuted();
    }

    void ApplySoundVolume()
    {
        foreach (AudioSource sound in _sounds)
        {
            sound.volume = _volumeSound * DefaultSoundVolume;
        }
    }

    void ApplyMusicVolume()
    {
        if (_currentMusicSource != null)
        {
            StopFadingForMusic(_currentMusicSource);
            _currentMusicSource.volume = _volumeMusic * DefaultMusicVolume;
        }
    }

    void ApplyVoiceVolume()
    {
        if (_currentMusicSource != null)
        {
            _currentVoiceSource.volume = _volumeMusic * DefaultVoiceVolume;
        }
    }

    void ApplySoundMuted()
    {
        foreach (AudioSource sound in _sounds)
        {
            sound.mute = _mutedSound;
        }
    }

    void ApplyMusicMuted()
    {
        if (_currentMusicSource != null)
        {
            _currentMusicSource.mute = _mutedMusic;
        }
    }

    void ApplyVoiceMuted()
    {
        if (_currentVoiceSource != null)
        {
            _currentVoiceSource.mute = _mutedVoice;
        }
    }

#endregion // Internal
}
