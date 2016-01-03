Sound Manager by Alexander Oplesnin

// Install

- Just import the package
- SoundManager ready to use. No need to create GameObject with it on Scene.
- After exploring examples you can delete sounds and music from "SoundManager/Resources/Sounds/Sounds" and "SoundManager/Resources/Sounds/Music" to keep your project clean


// Import sound files

- Copy your sound files to "SoundManager/Resources/Sounds/Sounds" or any "Resources/Sounds/Sounds" folder in your project
- For music files and voices(if you want to add narrator) use "Resources/Sounds/Music" and "Resources/Sounds/Voices" folders
- If you want to localize voices move it to language folder. For example "Resources/Sounds/Voices/ENG/" and set SoundManager.languageFolder to "ENG" from script

// Start from example

- Open "SoundManager/Examples/Demo" scene to see Sound Manager in use

// Use from Unity Editor

- Add SoundManagerComponent to your object. You can call its methods from Unity UI, Animations and other Unity Actions.

// Use from Script

- Just call SoundManager.PlaySound and other methods.


// Notes

- Use SoundManager.PlaySound("sound_name", false) to create UI sounds. This sounds will still be active when game paused
- By default SoundManager use autopause. So just set Time.timeScale = 0, and all pausable sounds and voices will pause. Music and unpausable sounds will remain active.
- SoundManager.Pause() Not work when autopause is on.
