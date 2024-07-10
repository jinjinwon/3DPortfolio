using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CameraShakeEvent : AppearanceAction
{
    public override void Start(object data, MonoBehaviour mono, Transform transform)
        => Camera.main.GetComponent<Cinemachine.CinemachineImpulseSource>().GenerateImpulse();

    public override object Clone() => new CameraShakeEvent();
}