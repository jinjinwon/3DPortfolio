using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoSingleton<AudioManager>
{
    public delegate void ValueChangedHandler(float currentValue);

    private AudioSource bgmSource;
    private AudioSource sfxSource;

    public event ValueChangedHandler onBGMValueChanged;
    public event ValueChangedHandler onSFXValueChanged;

    public float BGMVolume
    {
        get { return bgmSource.volume; }
        set { bgmSource.volume = value;
            onBGMValueChanged?.Invoke(bgmSource.volume);
        }
    }

    public float SFXVolume
    {
        get { return sfxSource.volume; }
        set { sfxSource.volume = value;
            onSFXValueChanged?.Invoke(sfxSource.volume);
        }
    }

    protected void Awake()
    {
        GameObject bgmObject = new GameObject("BGM Source");
        bgmSource = bgmObject.AddComponent<AudioSource>();
        bgmSource.loop = true;
        bgmObject.transform.parent = transform;

        GameObject sfxObject = new GameObject("SFX Source");
        sfxSource = sfxObject.AddComponent<AudioSource>();
        sfxObject.transform.parent = transform;
    }

    public void PlayBGM(AudioClip clip)
    {
        if (bgmSource.clip != clip)
        {
            StopBGM();

            bgmSource.clip = clip;
            bgmSource.Play();
        }
    }

    public void StartPlayBGM(AudioClip clip)
    {
        AddressableData.Instance.LoadAssets<AudioClip>(LabelData.Sound, LabelSubData.BGM, clip.name,PlayBGM);
    }

    public void StopBGM()
    {
        bgmSource.Stop();
        bgmSource.clip = null;
    }

    public void PlayOneShotClip(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }

    public void StartPlayOneShotClip(AudioClip clip)
    {
        AddressableData.Instance.LoadAssets<AudioClip>(LabelData.Sound, LabelSubData.SFX_Wav, clip.name, PlayOneShotClip);
    }

    public void PauseBGM()
    {
        bgmSource.Pause();
    }

    public void ResumeBGM()
    {
        bgmSource.UnPause();
    }
}
