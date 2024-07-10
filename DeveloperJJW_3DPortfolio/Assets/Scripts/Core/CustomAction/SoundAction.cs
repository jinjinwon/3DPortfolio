using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SoundAction : CustomAction
{
    private enum MethodType { Start, Run }

    [SerializeField]
    private MethodType methodType;
    [SerializeField]
    private AudioClip audioclip;

    public override void Start(object data)
    {
        if (methodType == MethodType.Start)
            Play();
    }

    public override void Run(object data)
    {
        if (methodType == MethodType.Run)
            Play();
    }

    public override void Release(object data)
    {

    }

    public void Play()
    {
        AudioManager.Instance.StartPlayOneShotClip(audioclip);
    }

    public override object Clone()
    {
        return new SoundAction()
        {
            audioclip = audioclip,
            methodType = methodType,
        };
    }
}
