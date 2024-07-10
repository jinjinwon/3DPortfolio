using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StatMonsterOverride
{
    [SerializeField]
    private Stat stat;
    [SerializeField]
    private bool isUseOverride;
    [SerializeField]
    private float overrideDefaultValue;

    public Stat Stat => stat;

    public StatMonsterOverride(Stat stat)
        => this.stat = stat;

    public Stat CreateStat()
    {
        var newStat = stat.Clone() as Stat;
        if (isUseOverride)
            newStat.DefaultValue = overrideDefaultValue;
        return newStat;
    }
}
