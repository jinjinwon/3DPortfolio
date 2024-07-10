using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitEffect : MonoBehaviour
{
    [SerializeField]
    private int delay = 2;

    public void OnEnable()
    {
        Invoke("Release", delay);
    }

    private void Release()
    {
        PoolManager.Instance.Recycle(this.gameObject);
    }
}
