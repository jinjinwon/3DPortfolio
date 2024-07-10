using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserRevive : MonoBehaviour
{
    [SerializeField]
    private Entity entity;

    public void Start()
    {
        entity.onDead += ReviveMessage;
        this.gameObject.SetActive(false);
    }

    public void ReviveMessage(Entity entity)
    {
        this.gameObject.SetActive(true);
    }

    public void Revive()
    {
        entity.UserClickedRevive = true;
        Invoke("DelayRevive", 1f);
    }

    private void DelayRevive()
    {
        entity.Stats.HPStat.DefaultValue = entity.Stats.HPStat.MaxValue;
        entity.UserClickedRevive = false;

        entity.EntityAI.Setup(entity);
        this.gameObject.SetActive(false);
    }
}
