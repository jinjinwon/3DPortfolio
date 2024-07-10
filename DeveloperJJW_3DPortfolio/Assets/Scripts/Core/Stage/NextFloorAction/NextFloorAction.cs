using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


[System.Serializable]
public abstract class NextFloorAction : ICloneable
{
    public virtual void Run(object data) { }
    public virtual void Release(object data) { }
    public abstract object Clone();
}
