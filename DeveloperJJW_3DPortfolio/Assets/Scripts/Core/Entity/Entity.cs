using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;

// Entitiy의 Control 주체를 나타내기 위한 enum
public enum EntityControlType
{
    Player,
    AI
}

public class Entity : MonoBehaviour
{
    #region 6-11
    #region Events
    public delegate void TakeDamageHandler(Entity entity, Entity instigator, object causer, float damage);
    public delegate void DeadHandler(Entity entity);
    public delegate void AttackHandler(Entity entity);
    public delegate void DisableHandler();
    #endregion
    #endregion

    // 여기서 Category는 적과 아군을 구분하기 위한 용도로 사용됨
    [SerializeField]
    private Category[] categories;
    [SerializeField]
    private EntityControlType controlType = EntityControlType.AI;

    // socket은 Entity Script를 가진 GameObject의 자식 GameObject를 의미함
    // 스킬의 발사 위치나, 어떤 특정 위치를 저장해두고 외부에서 찾아오기위해 존재
    private Dictionary<string, Transform> socketsByName = new();

    private bool isAttack = false;
    private bool isSkill = false;

    public EntityControlType ControlType { get => controlType; set => controlType = value; }
    public IReadOnlyList<Category> Categories => categories;
    public bool IsPlayer => controlType == EntityControlType.Player;

    [HideInInspector]
    public bool UserClickedRevive;

    public Animator Animator { get; private set; }
    #region 6-10
    public Stats Stats { get; private set; }
    public bool IsDead => Stats.HPStat != null && Mathf.Approximately(Stats.HPStat.DefaultValue, 0f);

    public bool isRevive => IsDead == true && UserClickedRevive == true;

    public bool isVictory => StageSystem.Instance.stage.isRoundClear && IsPlayer;

    public bool IsSkill { get => isSkill; set => isSkill = value; }
    public bool IsAttack { get => isAttack; set => isAttack = value; }

    public int AttackNumber => UnityEngine.Random.Range(1, 4);

    #endregion
    #region 7-1
    public EntityMovement Movement { get; private set; }
    #endregion
    #region 8-1
    public MonoStateMachine<Entity> StateMachine { get; private set; }
    #endregion
    #region 14-1
    public SkillSystem SkillSystem { get; private set; }

    public EntityAI EntityAI { get; private set; }
    #endregion
    // Target은 말 그대로 목표 대상으로 Entity가 공격해야하는 Target일 수도 있고, 치유해야하는 Target일 수도 있음
    public Entity Target { get ; set; }

    #region 6-12
    public event TakeDamageHandler onTakeDamage;
    public event DeadHandler onDead;
    public event AttackHandler onAttack;
    public event DisableHandler onDisable;
    #endregion

    public void OnDisable() => onDisable?.Invoke();

    public void Initialized()
    {
        Animator = GetComponent<Animator>();

        #region 6-13
        Stats = GetComponent<Stats>();
        Stats.Setup(this);
        #endregion

        #region 7-2
        Movement = GetComponent<EntityMovement>();
        Movement?.Setup(this);
        #endregion

        #region 8-2
        StateMachine = GetComponent<MonoStateMachine<Entity>>();
        StateMachine?.Setup(this);
        #endregion

        #region 14-2
        SkillSystem = GetComponent<SkillSystem>();
        SkillSystem?.SkillReset();
        SkillSystem?.Setup(this);
        #endregion

        EntityAI = GetComponent<EntityAI>();
        EntityAI?.Setup(this);
    }

    private void Awake()
    {
        if (IsPlayer)
        {
            Animator = GetComponent<Animator>();

            #region 6-13
            Stats = GetComponent<Stats>();
            Stats.Setup(this);
            #endregion

            #region 7-2
            Movement = GetComponent<EntityMovement>();
            Movement?.Setup(this);
            #endregion

            #region 8-2
            StateMachine = GetComponent<MonoStateMachine<Entity>>();
            StateMachine?.Setup(this);
            #endregion

            #region 14-2
            SkillSystem = GetComponent<SkillSystem>();
            SkillSystem?.Setup(this);
            #endregion

            EntityAI = GetComponent<EntityAI>();
            EntityAI?.Setup(this);
        }
    }

    #region 6-14
    // 데미지 공식도 모듈식으로 만들어서 지정할 수 있게 만든다면
    // 만드는 게임마다 다른 데미지 공식을 사용한다던가, 캐릭터마다 다른 데미지 공식을 사용하는 등
    // 굉장히 유연한 Entity 클래스를 만들 수 있음.
    public void TakeDamage(Entity instigator, object causer, float damage)
    {
        if (IsDead)
            return;

        float prevValue = Stats.HPStat.DefaultValue;
        Stats.HPStat.DefaultValue -= damage;

        onTakeDamage?.Invoke(this, instigator, causer, damage);
        PoolManager.Instance.Spawn_Object(PoolManager.Instance.HitPrefab,this.transform);

        if (Mathf.Approximately(Stats.HPStat.DefaultValue, 0f))
            OnDead();
    }

    public void GetGold(int gold)
    {
        float prevValue = Stats.GoldStat.DefaultValue;
        Stats.GoldStat.DefaultValue += gold;
    }

    public void GetCash(int cash)
    {
        float prevValue = Stats.CashStat.DefaultValue;
        Stats.CashStat.DefaultValue += cash;
    }

    private void OnDead()
    {
        #region 7-3
        if (Movement)
            Movement.enabled = false;
        #endregion

        SkillSystem.CancelAll(true);

        onDead?.Invoke(this);
    }

    public void OnAttack()
    {
        onAttack?.Invoke(this);
    }
    #endregion

    // root transform의 자식 transform들을 순회하며 이름이 socketName인 GameObject의 Transform을 찾아옴 
    private Transform GetTransformSocket(Transform root, string socketName)
    {
        if (root.name == socketName)
            return root;

        // root transform의 자식 transform들을 순회
        foreach (Transform child in root)
        {
            // 재귀함수를 통해 자식들 중에 socketName이 있는지 검색함
            var socket = GetTransformSocket(child, socketName);
            if (socket)
                return socket;
        }

        return null;
    }

    // 저장되있는 Socket을 가져오거나 순회를 통해 찾아옴
    public Transform GetTransformSocket(string socketName)
    {
        // dictionary에서 socketName을 검색하여 있다면 return
        if (socketsByName.TryGetValue(socketName, out var socket))
            return socket;

        // dictionary에 없으므로 순회 검색
        socket = GetTransformSocket(transform, socketName);
        // socket을 찾으면 dictionary에 저장하여 이후에 다시 검색할 필요가 없도록 함
        if (socket)
            socketsByName[socketName] = socket;

        return socket;
    }

    public void CategorySet(Category[] category)
    {
        categories = category;
    }

    public bool HasCategory(Category category) => categories.Any(x => x.ID == category.ID);

    #region 8-3
    public bool IsInState<T>() where T : State<Entity>
        => StateMachine.IsInState<T>();

    public bool IsInState<T>(int layer) where T : State<Entity>
        => StateMachine.IsInState<T>(layer);
    #endregion
}