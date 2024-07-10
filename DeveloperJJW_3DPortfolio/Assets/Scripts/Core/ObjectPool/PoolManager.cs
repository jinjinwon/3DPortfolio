using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

// 보류.. 이펙트 관련되어 생각이 정리되면 작업

public class PoolManager : MonoSingleton<PoolManager>
{
    [Header("Monster")]
    [SerializeField]
    private int monsterMaxSize;
    [SerializeField]
    private GameObject monsterPrefab;

    [Header("Player"),Space(1)]
    [SerializeField]
    private int playerMaxSize;
    [SerializeField]
    private GameObject playerPrefab;

    [Header("HitEffect"), Space(1)]
    [SerializeField]
    private int HitEffectMaxSize;
    public GameObject HitPrefab;

    [Header("MonsterHUD"), Space(1)]
    [SerializeField]
    private int MonsterHUDMaxSize;
    public GameObject MonsterHUDPrefab;

    private void Start()
    {
        ObjectPool.CreatePool(HitPrefab, HitEffectMaxSize);
        ObjectPool.CreatePool(MonsterHUDPrefab, MonsterHUDMaxSize);
    }
    
    // 처음 생성된 것들만 이 부분을 타서 컴포넌트를 부착합니다.
    // 재활용시에는 데이터 세팅만 ㅇㅇ;
    public void Setting(Monster monster, GameObject go)
    {
        Entity tempEntity = null;
        #region Script Setting

        if(go.TryGetComponent(out NavMeshAgent navMesh) == false) go.AddComponent<NavMeshAgent>();

        if (go.TryGetComponent(out Entity entity) == false)
        {
            tempEntity = go.AddComponent<Entity>();
            tempEntity.CategorySet(monster.Category);
            tempEntity.ControlType = EntityControlType.AI;
        }
        else
        {
            tempEntity = entity;
            tempEntity.CategorySet(monster.Category);
            tempEntity.ControlType = EntityControlType.AI;
        }

        if (go.TryGetComponent(out Stats stats) == false)
        {
            Stats temp = go.AddComponent<Stats>();
            temp.SetHPStat = monster.StatOverrides.Where(statOverride => statOverride.Stat.CodeName == "HP").Select(statOverride => statOverride.Stat).FirstOrDefault();
            temp.SetSkillCostStat = monster.StatOverrides.Where(statOverride => statOverride.Stat.CodeName == "MP").Select(statOverride => statOverride.Stat).FirstOrDefault();
            temp.SetStatOverride = monster.StatOverrides;
        }
        else
        {
            stats.SetHPStat = monster.StatOverrides.Where(statOverride => statOverride.Stat.CodeName == "HP").Select(statOverride => statOverride.Stat).FirstOrDefault();
            stats.SetSkillCostStat = monster.StatOverrides.Where(statOverride => statOverride.Stat.CodeName == "MP").Select(statOverride => statOverride.Stat).FirstOrDefault();
            stats.SetStatOverride = monster.StatOverrides;
        }

        if (go.TryGetComponent(out EntityMovement entityMovement) == false)
        {
            EntityMovement temp = go.AddComponent<EntityMovement>();
            temp.SetMoveSpeed = monster.StatOverrides.Where(statOverride => statOverride.Stat.CodeName == "MOVE_SPEED").Select(statOverride => statOverride.Stat).FirstOrDefault();
        }
        else
        {
            entityMovement.SetMoveSpeed = monster.StatOverrides.Where(statOverride => statOverride.Stat.CodeName == "MOVE_SPEED").Select(statOverride => statOverride.Stat).FirstOrDefault();
        }

        if (go.TryGetComponent(out EntityStateMachine entityStateMachine) == false) go.AddComponent<EntityStateMachine>();

        if (go.TryGetComponent(out SkillSystem skillSystem) == false)
        {
            SkillSystem temp = go.AddComponent<SkillSystem>();
            temp.DefaultSkills = monster.Skills;
        }
        else
        {
            skillSystem.DefaultSkills = monster.Skills;
        }

        if (go.TryGetComponent(out EntityFloatingTextConnector entityFloatingTextConnector) == false) go.AddComponent<EntityFloatingTextConnector>();

        // if (go.TryGetComponent(out AI ai) == false) go.AddComponent<AI>();

        if (go.TryGetComponent(out MonsterPool monsterPool))
        {
            monsterPool.Initialize();
            monsterPool.Setup(monster);
        }
        else
        {
            MonsterPool tempMonsterPool = go.AddComponent<MonsterPool>();

            if (tempMonsterPool == null)
            {
                go.Recycle();
                return;
            }

            tempMonsterPool.Initialize();
            tempMonsterPool.Setup(monster);
        }

        if (go.TryGetComponent(out EntityAI entityAI) == false) go.AddComponent<EntityAI>();

        if(tempEntity != null)
        {
            tempEntity.Initialized();

            GameObject temp = ObjectPool.Spawn(MonsterHUDPrefab, GameObject.Find("Canvas").transform);
            if(temp.TryGetComponent(out MonsterHUD monsterHUD))
            {
                monsterHUD.Show(tempEntity);
            }
            tempEntity.onAttack += monster.AttackClip;
            tempEntity.onDead += monster.DeadClip;

            if(monster.BossMonster == true)
            {
                tempEntity.onDead += StageSystem.Instance.stage.UpdateBossKill;
            }

            tempEntity = null;
        }
        #endregion
    }

    public void Spwan(Monster monster, Transform parent = null, Vector3 vector = new(),Quaternion quaternion = new())
    {
        if (parent == null)
            parent = this.transform;

        GameObject go = ObjectPool.Spawn(monster.Prefab, parent, vector, quaternion);

        Setting(monster, go);
    }

    public void Spawn_Object(GameObject go, Transform parent = null, Vector3 vector = new(), Quaternion quaternion = new())
    {
        if (parent == null)
            parent = this.transform;

        ObjectPool.Spawn(go, parent, vector, quaternion);
    }

    public void Recycle(GameObject go)
    {
        go.Recycle();
    }

    public void RecycleAll()
    {
        ObjectPool.RecycleAll();
    }

    public void DestroyPooled(GameObject go)
    {
        go.DestroyPooled();
    }

    public void DestroyAll(GameObject go)
    {
        ObjectPool.DestroyAll(go);
    }
}
