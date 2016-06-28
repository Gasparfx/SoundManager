using UnityEngine;
using System.Collections;


public class SMSoundHandler
{
    private bool _valid;
    private SoundManager.SMSound _smSound;

    public SMSoundHandler(SoundManager.SMSound sound)
    {
        _valid = sound != null;
        _smSound = sound;
    }

    public float GetVolume()
    {
        return SoundManager.Instance.GetVolume(_smSound);
    }

    public SMSoundHandler SetVolume(float volume)
    {
        SoundManager.Instance.SetVolume(_smSound, volume);
        return this;
    }

    public SMSoundHandler SetLooped(bool looped = true)
    {
        SoundManager.Instance.SetLooped(_smSound, looped);
        return this;
    }

    public SMSoundHandler SetPausable(bool pausable)
    {
        SoundManager.Instance.SetPausable(_smSound, pausable);
        return this;
    }

    public SMSoundHandler Set3D(bool is3D)
    {
        SoundManager.Instance.Set3D(_smSound, is3D);
        return this;
    }

    public SMSoundHandler AttachToObject(Transform objectToAttach)
    {
        SoundManager.Instance.AttachToObject(_smSound, objectToAttach);
        return this;
    }

    public SMSoundHandler SetPosition(Vector3 position)
    {
        SoundManager.Instance.SetPosition(_smSound, position);
        return this;
    }

    // SoundHandler not valid after this call
    public void Stop()
    {
        SoundManager.Instance.Stop(_smSound);
    }
}