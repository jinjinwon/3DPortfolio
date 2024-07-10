using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[System.Serializable]
public abstract class DGAction : ICloneable
{
    public virtual void Start(Entity entity ,Vector3 position) { }
    public virtual void Release() { }

    public abstract object Clone();
}
