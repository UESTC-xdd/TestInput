using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioMgr : MonoBehaviour
{
    [Header("SE")]
    public AudioClip SE_Jump;
    public AudioClip SE_Dead;
    public AudioClip SE_CheckPoint;

    [Header("BGM")]
    public AudioClip BGM_Day;

    [Header("AudioSource")]
    public AudioSource BGMSource;
    public AudioSource SESource;

    private static AudioMgr instance;
    public static AudioMgr Instance
    {
        get { return instance; }
    }

    public static bool IsValid
    {
        get { return instance != null; }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            if(instance!=this)
            {
                Destroy(gameObject);
                return;
            }
        }

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        BGMSource.clip = BGM_Day;
    }

    public void PlaySEClipOnce(AudioClip clip)
    {
        SESource.PlayOneShot(clip);
    }
}
