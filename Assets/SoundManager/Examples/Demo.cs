using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Demo : MonoBehaviour {

    public GameObject gamePanel;
    public GameObject pausePanel;

    public void Start()
    {
        pausePanel.transform.FindChild("MusicSlider").GetComponent<Slider>().value = SoundManager.GetMusicVolume();
        pausePanel.transform.FindChild("SoundSlider").GetComponent<Slider>().value = SoundManager.GetSoundVolume();

        SoundManager.PlayMusic("Swinging Pants");
    }

    public void Click()
    {
        // Click1 sound preloaded in SoundManagerSettings
        SoundManager.PlaySoundUI("click1").SetVolume(0.8f);
    }

    public void TogglePause()
    {
        bool needPause = Time.timeScale > 0.5;
        Time.timeScale = needPause ? 0 : 1;

        gamePanel.SetActive(!needPause);
        pausePanel.SetActive(needPause);
    }

    void OnEnable()
    {
        SoundManager.LoadSound("phaserUp1");
    }

    void OnDisable()
    {
        SoundManager.UnloadSound("phaserUp1");
    }
}
