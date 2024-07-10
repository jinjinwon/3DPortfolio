using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor.Animations;
#endif
using System;

[System.Serializable]
public class Monster : IdentifiedObject
{
    #region EventsHandler

    #endregion

    #region Event

    #endregion

    [UnderlineTitle("ī�װ�")]
    [SerializeField]
    private Category[] category;

    [SerializeField]
    private MonsterType type;
    [SerializeField]
    private MonsterActionPattern pattern;

    [UnderlineTitle("���� ������")]
    [SerializeField]
    private GameObject prefab;

    [UnderlineTitle("���� ���� ����")]
    [SerializeField]
    private AudioClip attackClip;

    [UnderlineTitle("���� �״� ����")]
    [SerializeField]
    private AudioClip deathClip;

    [UnderlineTitle("���� �ִϸ����� �������̵� ��Ʈ�ѷ�")]
    [SerializeField]
    private AnimatorOverrideController animatorOverrideController;

    [UnderlineTitle("Collider Setting")]
    [SerializeField]
    private Vector3 center;

    [SerializeField]
    private float radius;

    [SerializeField]
    private float height;

    [UnderlineTitle("���� ��������")]
    [SerializeField]
    private bool bossMonster;

    [UnderlineTitle("���� ��Ÿ�")]
    [SerializeField]
    private float attackRange;

    [UnderlineTitle("���� ����")]
    [SerializeReference]
    private StatOverride[] statOverrides;

    [UnderlineTitle("���� ��ų")]
    [SerializeReference]
    private Skill[] skills;

    [UnderlineTitle("���� �̺�Ʈ")]
    [SerializeReference, SubclassSelector]
    private AppearanceAction[] customActionsOnAppear;

    [UnderlineTitle("DoTween �̺�Ʈ")]
    [SerializeReference, SubclassSelector]
    private DGAction[] dgActionsOnDead;

    public GameObject Prefab => prefab;
    public AnimatorOverrideController AnimatorOverrideController => animatorOverrideController;
    public Category[] Category => category;
    public Vector3 Center => center;
    public float Radius => radius;
    public float Height => height;

    public bool BossMonster => bossMonster;

    public float AttackRange
    {
        get { return attackRange; }
        set
        {
            AttackRange = value;
        }
    }

    public StatOverride[] StatOverrides => statOverrides;
    public Skill[] Skills => skills;

    public AppearanceAction[] CustomActionsOnAppear => customActionsOnAppear;

    public DGAction[] DgActionsOnDead => dgActionsOnDead;


    public void StartCustomActions(MonoBehaviour monoBehaviour, Transform transform)
    {
        foreach (var customAction in customActionsOnAppear)
            customAction.Start(this, monoBehaviour,transform);
    }

    public void ReleaseCustomActions(MonoBehaviour monoBehaviour, Transform transform)
    {
        foreach (var customAction in customActionsOnAppear)
            customAction.Release(this,monoBehaviour, transform);
    }

    public void StartDGActions(Entity entity, Vector3 position)
    {
        foreach (var dgAction in dgActionsOnDead)
            dgAction.Start(entity, position);
    }

    public void ReleaseDGActions()
    {
        foreach (var dgAction in dgActionsOnDead)
            dgAction.Release();
    }

    public void DeadClip(Entity entity)
    {
        AudioManager.Instance.StartPlayOneShotClip(deathClip);
    }

    public void AttackClip(Entity entity)
    {
        AudioManager.Instance.StartPlayOneShotClip(attackClip);
    }

    public void SetScale(Entity entity)
    {
        if(bossMonster == true)
        {
            entity.transform.localScale = Vector3.one;
        }
        else
        {
            entity.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        }
    }
}
