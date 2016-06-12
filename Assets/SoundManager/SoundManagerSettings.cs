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
