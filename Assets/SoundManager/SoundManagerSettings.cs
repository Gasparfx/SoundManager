using UnityEngine;
using System.Collections;
using UnityEngine.Audio;

public class SoundManagerSettings : ScriptableObject {

    public bool AutoPause = true;

    public float DefaultMusicVolume = 1f;
    public float DefaultSoundVolume = 1f;

    public float MusicFadeTime = 2f;

    public AudioMixerGroup MusicAudioMixerGroup;
    public AudioMixerGroup SoundAudioMixerGroup;

    private float _volumeMusic;
    private float _volumeSound;

    private bool _mutedMusic;
    private bool _mutedSound;


    public void SaveSettings()
    {
        PlayerPrefs.SetFloat("SM_MusicVolume", _volumeMusic);
        PlayerPrefs.SetFloat("SM_SoundVolume", _volumeSound);

        PlayerPrefs.SetInt("SM_MusicMute", _mutedMusic ? 1 : 0);
        PlayerPrefs.SetInt("SM_SoundMute", _mutedSound ? 1 : 0);
    }

    public void LoadSettings()
    {
        _volumeMusic = PlayerPrefs.GetFloat("SM_MusicVolume", 1);
        _volumeSound = PlayerPrefs.GetFloat("SM_SoundVolume", 1);

        _mutedMusic = PlayerPrefs.GetInt("SM_MusicMute", 0) == 1;
        _mutedSound = PlayerPrefs.GetInt("SM_SoundMute", 0) == 1;
    }

    public void SetMusicVolumeInternal(float volume)
    {
        _volumeMusic = volume;
        SaveSettings();
    }

    public float GetMusicVolumeInternal()
    {
        return _volumeMusic;
    }

    public void SetSoundVolumeInternal(float volume)
    {
        _volumeSound = volume;
        SaveSettings();
    }

    public float GetSoundVolumeInternal()
    {
        return _volumeSound;
    }

    public void SetMusicMuted(bool mute)
    {
        _mutedMusic = mute;
        SaveSettings();
    }

    public bool GetMusicMuted()
    {
        return _mutedMusic;
    }

    public void SetSoundMuted(bool mute)
    {
        _mutedSound = mute;
        SaveSettings();
    }

    public bool GetSoundMuted()
    {
        return _mutedSound;
    }


    public float GetSoundVolume()
    {
        return _volumeSound * DefaultSoundVolume;
    }

    public float GetMusicVolume()
    {
        return _volumeMusic * DefaultMusicVolume;
    }




    //[MenuItem("SoundManager/Create SoundManagerSettings")]
    //public static void CreateAsset()
    //{
    //    SoundManagerSettings asset = ScriptableObject.CreateInstance<SoundManagerSettings>();
    //    string assetPathAndName = "Assets/SoundManager/Resources/SoundManagerSettings.asset";
    //    AssetDatabase.CreateAsset(asset, assetPathAndName);
    //    AssetDatabase.SaveAssets();
    //    AssetDatabase.Refresh();
    //    EditorUtility.FocusProjectWindow();
    //    Selection.activeObject = asset;
    //}
}
