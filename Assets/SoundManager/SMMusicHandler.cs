using UnityEngine;
using System.Collections;

public class SMMusicHandler
{
    private string _name;
    public AudioSource Source;

    public float timer;
    public float fadingTime;
    public float startVolume;
    public float targetVolume;
    public bool destroyOnComplete;

}
