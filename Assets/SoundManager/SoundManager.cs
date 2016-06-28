using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    private SoundManagerSettings _settings;

    List<SMSound> _sounds = new List<SMSound>();

    SMMusic _music;
    string _currentMusicName;

    List<SMMusicFadingOut> _musicFadingsOut = new List<SMMusicFadingOut>();

    private bool _loadingInProgress;


#region Public functions

    public static void PlayMusic(string name)
    {
        Instance.PlayMusicInternal(name);
    }

    public static void StopMusic()
    {
        Instance.StopMusicInternal();
    }

    public static void PlaySound(string name, AssetBundle bundle)
    {
        Instance.PlaySoundInternal(name, true);
    }

    public static void PlaySoundUI(string name, AssetBundle bundle)
    {
        Instance.PlaySoundInternal(name, false);
    }

    public static void PlaySound(string name)
    {
        Instance.PlaySoundInternal(name, true);
    }

    public static void PlaySoundUI(string name)
    {
        Instance.PlaySoundInternal(name, false);
    }

    public static void PlaySoundWithDelay(string name, float delay, bool pausable = true)
    {
        Instance.PlaySoundWithDelayInternal(name, delay, pausable);
    }

    public static void Pause()
    {
        if (Instance._settings.AutoPause)
            return;

        AudioListener.pause = true;
    }

    public static void UnPause()
    {
        if (Instance._settings.AutoPause)
            return;

        AudioListener.pause = false;
    }

    public static void StopAllPausableSounds()
    {
        Instance.StopAllPausableSoundsInternal();
    }

    // Volume [0 - 1]
    public static void SetMusicVolume(float volume)
    {
        Instance._settings.SetMusicVolume(volume);
        Instance.ApplyMusicVolume();
    }

    // Volume [0 - 1]
    public static float GetMusicVolume()
    {
        return Instance._settings.GetMusicVolume();
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
        Instance._settings.SetSoundVolume(volume);
        Instance.ApplySoundVolume();
    }

    // Volume [0 - 1]
    public static float GetSoundVolume()
    {
        return Instance._settings.GetSoundVolume();
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

    #region Sound Handler methods
    public void SetVolume(SMSound smSound, float volume)
    {
        smSound.SelfVolume = volume;
        smSound.Source.volume = volume * _settings.GetSoundVolumeCorrected();
    }

    public float GetVolume(SMSound smSound)
    {
        return smSound.SelfVolume;
    }

    public void SetLooped(SMSound smSound, bool looped)
    {
        smSound.Source.loop = looped;
    }

    public void SetPausable(SMSound smSound, bool pausable)
    {
        smSound.Source.ignoreListenerPause = !pausable;
    }

    public void Set3D(SMSound smSound, bool is3D)
    {
        smSound.Source.spatialBlend = is3D ? 1 : 0;
    }

    public void Stop(SMSound smSound)
    {
        StopSoundInternal(smSound);
    }

    public void AttachToObject(SMSound smSound, Transform objectToAttach)
    {
        smSound.IsAttachedToTransform = true;
        smSound.Attach = objectToAttach;
        smSound.Source.transform.position = objectToAttach.position;
    }

    public void SetPosition(SMSound smSound, Vector3 position)
    {
        smSound.Source.transform.position = position;
    }
    #endregion

    #region Internal Classes
    public class SMMusic
    {
        private string _name;
        public AudioSource Source;

        public float Timer;
        public float FadingTime;
        public float TargetVolume;
        public bool FadingIn;
    }

    public class SMMusicFadingOut
    {
        private string _name;
        public AudioSource Source;

        public float Timer;
        public float FadingTime;
        public float StartVolume;
    }

    public class SMSound
    {
        public string Name;
        public AudioSource Source;

        public bool IsLoading;
        public IEnumerator LoadingCoroutine;
        public bool IsPossessedLoading;
        public float SelfVolume;

        public bool IsAttachedToTransform;
        public Transform Attach;
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

        music.transform.SetParent(transform);

        musicSource.outputAudioMixerGroup = _settings.MusicAudioMixerGroup;
        
        musicSource.loop = true;
        musicSource.priority = 0;
        musicSource.playOnAwake = false;
        musicSource.mute = _settings.GetMusicMuted();
        musicSource.ignoreListenerPause = true;
        musicSource.clip = musicClip;
        musicSource.Play();

        musicSource.volume = 0;

        _music = new SMMusic();
        _music.Source = musicSource;
        _music.FadingIn = true;
        _music.TargetVolume = _settings.GetMusicVolumeCorrected();
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

    SMSoundHandler PlaySoundInternal(string soundName, bool pausable, AssetBundle bundle = null)
    {
        if (string.IsNullOrEmpty(soundName)) {
            Debug.Log("Sound null or empty");
            return null;
        }

        int sameCountGuard = 0;
        foreach (SMSound smSound in _sounds)
        {
            if (smSound.Name == soundName)
                sameCountGuard++;
        }

        if (sameCountGuard > 8)
        {
            Debug.Log("Too much duplicates for sound: " + soundName);
            return null;
        }

        if (_sounds.Count > 16) {
            Debug.Log("Too much sounds");
            return null;
        }


        GameObject soundGameObject = new GameObject("Sound: " + soundName);
        AudioSource soundSource = soundGameObject.AddComponent<AudioSource>();
        soundGameObject.transform.parent = transform;

        soundSource.outputAudioMixerGroup = _settings.SoundAudioMixerGroup;
        soundSource.priority = 128;
        soundSource.playOnAwake = false;
        soundSource.mute = _settings.GetSoundMuted();
        soundSource.volume = _settings.GetSoundVolumeCorrected();
        soundSource.ignoreListenerPause = !pausable;

        SMSound sound = new SMSound();
        sound.Source = soundSource;
        sound.Name = soundName;
        sound.IsLoading = true;
        sound.SelfVolume = 1;

        _sounds.Add(sound);

        sound.LoadingCoroutine = PlaySoundInternalAfterLoad(sound, soundName, bundle);
        StartCoroutine(sound.LoadingCoroutine);
        return new SMSoundHandler(sound);
    }

    IEnumerator PlaySoundInternalAfterLoad(SMSound smSound, string soundName, AssetBundle bundle)
    {
        // Need to wait others sounds to be loaded to avoid Android LoadingPersistentStorage lags
        while (_loadingInProgress)
        {
            yield return null;
        }

        _loadingInProgress = true;
        smSound.IsPossessedLoading = true;
        AudioClip soundClip = null;
        if (bundle == null)
        {
            ResourceRequest request = LoadClipAsync("Sounds/" + soundName);
            while (!request.isDone)
                yield return null;
            soundClip = (AudioClip)request.asset;
        }
        else
        {
            AssetBundleRequest request = LoadClipFromBundleAsync(bundle, soundName);
            while (!request.isDone)
                yield return null;
            soundClip = (AudioClip)request.asset;
        }
        smSound.IsPossessedLoading = false;
        _loadingInProgress = false;

        if (null == soundClip)
        {
            Debug.Log("Sound not loaded: " + soundName);
        }

        smSound.IsLoading = false;
        smSound.Source.clip = soundClip;
        smSound.Source.Play();
    }

    void PlaySoundWithDelayInternal(string soundName, float delay, bool pausable)
    {
        StartCoroutine(PlaySoundWithDelayCoroutine(soundName, delay, pausable));
    }

    void StopAllPausableSoundsInternal()
    {
        foreach (SMSound sound in _sounds)
        {
            if (!sound.Source.ignoreListenerPause)
            {
                StopSoundInternal(sound);
            }
        }
    }

    void StopSoundInternal(SMSound sound)
    {
        if (sound.IsLoading)
        {
            StopCoroutine(sound.LoadingCoroutine);
            if (sound.IsPossessedLoading)
                _loadingInProgress = false;

            sound.IsLoading = false;
        }
        else
            sound.Source.Stop();
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
        // Destory only one sound per frame
        SMSound soundToDelete = null;

        foreach (SMSound sound in _sounds)
        {
            if (!sound.IsLoading && !sound.Source.isPlaying)
            {
                soundToDelete = sound;
                break;
            }
        }

        if (soundToDelete != null)
        {
            _sounds.Remove(soundToDelete);
            Destroy(soundToDelete.Source.gameObject);
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

    void LateUpdate()
    {
        foreach (SMSound sound in _sounds)
        {
            if (sound.IsAttachedToTransform && sound.Attach != null)
            {
                sound.Source.transform.position = sound.Attach.position;
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

        PlaySoundInternal(name, pausable);
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

    AssetBundleRequest LoadClipFromBundleAsync(AssetBundle bundle, string name)
    {
        return bundle.LoadAssetAsync<AudioClip>(name);
    }

    void ApplySoundVolume()
    {
        foreach (SMSound sound in _sounds)
        {
            sound.Source.volume = _settings.GetSoundVolumeCorrected() * sound.SelfVolume;
        }
    }

    void ApplyMusicVolume()
    {
        if (_music != null)
        {
            _music.FadingIn = false;
            _music.TargetVolume = _settings.GetMusicVolumeCorrected();
            _music.Source.volume = _music.TargetVolume;
        }
    }

    void ApplySoundMuted()
    {
        foreach (SMSound sound in _sounds)
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
