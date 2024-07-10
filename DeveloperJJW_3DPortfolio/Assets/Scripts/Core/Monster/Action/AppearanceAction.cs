using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class AppearanceAction : ICloneable
{
    public virtual void Start(object data, MonoBehaviour mono, Transform transform) { }
    public virtual void Release(object data ,MonoBehaviour mono, Transform transform) { }

    public abstract object Clone();
}
