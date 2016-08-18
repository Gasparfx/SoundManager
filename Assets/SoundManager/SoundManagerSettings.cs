using UnityEngine;
using System.Collections;
using UnityEngine.Audio;

public class SoundManagerSettings : ScriptableObject {

    public bool AutoPause = true;

    public float MusicVolumeCorrection = 1f;
    public float SoundVolumeCorrection = 1f;

    public float MusicFadeTime = 2f;

    public AudioMixerGroup MusicAudioMixerGroup;
    public AudioMixerGroup SoundAudioMixerGroup;

    public AudioClip[] PreloadedLoadedClips;

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

    public void SetMusicVolume(float volume)
    {
        _volumeMusic = volume;
        SaveSettings();
    }

    public float GetMusicVolume()
    {
        return _volumeMusic;
    }

    public void SetSoundVolume(float volume)
    {
        _volumeSound = volume;
        SaveSettings();
    }

    public float GetSoundVolume()
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


    public float GetSoundVolumeCorrected()
    {
        return _volumeSound * SoundVolumeCorrection;
    }

    public float GetMusicVolumeCorrected()
    {
        return _volumeMusic * MusicVolumeCorrection;
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
