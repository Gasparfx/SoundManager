# SoundManager
Simple sound manager for Unity

Put your sounds in Resources\SoundManager\Sounds and music in Resources\SoundManager\Music.

* Use SoundManager.PlaySound("sound_name") to play sounds from gameplay(This sounds can be paused)
* Use SoundManager.PlaySoundUI("sound_name") to play sounds from UI.
* Use SoundManager.PlayMusic("music_name") to play music.

### Features:
* AutoPause. Just set Time.timeScale = 0 and all pausable sounds will be paused(AutoPause can be turned off in settings)
* Simple and intuitive
* Asynchronius. No lags after PlaySound


