using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class StageSystem : MonoSingleton<StageSystem>
{
    public Stage stage;   
    public Entity Owner { get; private set; }

    private bool isBoss = false;

    private bool mapCreated = false;

    public bool IsMapCreated => mapCreated;
    public bool IsRegen => SpawnedCount() < stage.CurrentStageData.regenCount && !IsBoss;
    public int RegenCount => stage.CurrentStageData.regenCount - SpawnedCount();
    public bool IsBoss => isBoss;

    private void Start()
    {
        FloorHUD.Instance.Show(stage);
        stage.Setup();
        Setup();
    }


    private void Update()
    {
        Spawning();
    }

    public void Setup()
    {
        Release();
        CreateMapStart();
        PlayBGM();
    }

    private void Release()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
        mapCreated = false;
        isBoss = false;
        stage.BossKill = false;
    }

    private void PlayBGM()
    {
        AudioManager.Instance.StartPlayBGM(stage.CurrentStageData.audioClip);
        //AudioManager.Instance.PlayBGM(stage.CurrentStageData.audioClip);
    }

    private void Create(GameObject go)
    {
        Instantiate(go, this.transform).SetActive(true);
        mapCreated = true;
        CretaeNpc();
    }

    private void CreateMapStart()
    {
        AddressableData.Instance.LoadAssets<GameObject>(LabelData.Stage, stage.CurrentStageData.mapPrefab.name, Create);
    }

    private void CretaeNpc()
    {
        GameObject[] spwanerPoint = new GameObject[4];

        for(int i = 1; i <= 4; i++)
        {
            if(GameObject.Find($"Spwaner_{i}") == true)
            {
                spwanerPoint[i - 1] = GameObject.Find($"Spwaner_{i}");
            }
        }

        // ���� ��ġ�� �ϴ� ���� �������� ����;
        // �ٽ� �����ϴ� �κ��� ���Ϳ��� regen üũ�� ����;
        for (int i = 0; i < stage.CurrentStageData.regenCount; i++)
        {
            int random = Random.Range(0, 4);
            int randomMonster = Random.Range(0, stage.CurrentStageData.monsters.Length);
            PoolManager.Instance.Spwan(stage.CurrentStageData.monsters[randomMonster], spwanerPoint[random].transform);
        }
    }

    private void CretaeNpc(int count)
    {
        GameObject[] spwanerPoint = new GameObject[4];

        for (int i = 1; i <= 4; i++)
        {
            if (GameObject.Find($"Spwaner_{i}") == true)
            {
                spwanerPoint[i - 1] = GameObject.Find($"Spwaner_{i}");
            }
        }

        // ���� ��ġ�� �ϴ� ���� �������� ����;
        // �ٽ� �����ϴ� �κ��� ���Ϳ��� regen üũ�� ����;
        for (int i = 0; i < count; i++)
        {
            int random = Random.Range(0, 4);
            int randomMonster = Random.Range(0, stage.CurrentStageData.monsters.Length);
            PoolManager.Instance.Spwan(stage.CurrentStageData.monsters[randomMonster], spwanerPoint[random].transform);
        }
    }

    private void Spawning()
    {
        if(IsMapCreated && IsRegen)
        {
            CretaeNpc(RegenCount);
        }
    }

    private int SpawnedCount()
    {
        GameObject[] gameObjects = new GameObject[stage.CurrentStageData.monsters.Length];
        int count = 0;
        foreach (var pair in stage.CurrentStageData.monsters)
        {
            gameObjects[count] = pair.Prefab;
            count++;
        }

        count = ObjectPool.CountSpawned(gameObjects);
        gameObjects = null;
        return count;
    }

    public void CreateBossNpc()
    {
        if (isBoss == true)
            return;

        isBoss = true;
        SummonBoss();
    }

    public void SummonBoss()
    {
        // ���� ��ȯ�Ǿ� �ִ� ������ ���� ���͵��� ��� �����մϴ�.
        if(stage.CurrentStageData.bossMonsterGen)
        {
            MonsterPool[] monsters = FindObjectsOfType<MonsterPool>();
            foreach(var pair in monsters)
            {
                if(pair.gameObject.activeSelf == true)
                    pair.Dead(0);
            }
        }

        // ���� ��ȯ
        GameObject[] spwanerPoint = new GameObject[4];

        for (int i = 1; i <= 4; i++)
        {
            if (GameObject.Find($"Spwaner_{i}") == true)
            {
                spwanerPoint[i - 1] = GameObject.Find($"Spwaner_{i}");
            }
        }

        int random = Random.Range(0, 4);
        PoolManager.Instance.Spwan(stage.CurrentStageData.bossmonster, spwanerPoint[random].transform);
    }

    public void NextFloor()
    {
        LevelUp();
    }

    [ContextMenu("���� ��")]
    public void LevelUp()
    {
        stage.Floor++;
    }

    public int testValue;
    [ContextMenu("��ǥ óġ ��")]
    public void KillCountUp()
    {
        stage.CurrentKillCount = testValue;
    }
}