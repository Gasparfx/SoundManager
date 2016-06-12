using UnityEngine;
using System.Collections;

public class SMSoundHandler
{
    public string Name;
    public AudioSource Source;

    public bool IsLoading;
    public IEnumerator LoadingCoroutine;
    public bool IsPossessedLoading;

}
