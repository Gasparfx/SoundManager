using UnityEngine;
using System.Collections;

public class SMMusicHandler
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
