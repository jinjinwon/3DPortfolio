using Google.Apis.Sheets.v4.Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;
using static Cinemachine.DocumentationSortingAttribute;

public class Stage : IdentifiedObject
{
    #region EventsHandler
    public delegate void NextFloorGoalGaugeHandler(Stage stage);                                                       // 다음 층 목표 게이지가 전부 찬 경우 실행되는 Event
    public delegate void FloorGoalGaugeChangeHandler(Stage stage, int currentKillCount, int maxKillCount);             // 다음 층 목표 게이지가 변동이 있는 경우 실행되는 Event
    public delegate void FloorChangeHandler(StageData stageData);                                                      // 층의 변동이 생긴 경우 실행되는 Event
    public delegate void BossSummonHandler(Stage stage);                                                               // 보스 소환이 가능할 때 실행되는 Event
    #endregion

    #region Events
    public event NextFloorGoalGaugeHandler onNextFloorGoalGauge;
    public event FloorGoalGaugeChangeHandler onFloorGoalGaugeChange;
    public event FloorChangeHandler onFloorChange;
    public event BossSummonHandler onBossSummon;
    #endregion

    [SerializeField]
    private int maxFloor;

    private int floor;

    [SerializeField]
    private bool isAllowFloorExceedDatas;

    [SerializeField]
    private int defaultFloor = 1;

    [SerializeField]
    private StageData[] stageDatas;
    private StageData currentData;
    private bool bossKill;


    private int currentKillCount;
    private readonly Dictionary<NextFloorType, NextFloorAction[]> customActionsByType = new();

    public int MaxFloor => maxFloor;

    public StageData[] StageDatas => stageDatas;
    public StageData CurrentStageData => currentData;

    // 몇 층인지
    public int Floor
    {
        get => floor;
        set
        {
            Debug.Assert(value >= 1 && value <= MaxFloor,
                $"Stage.Floor = {value} - value는 1과 MaxFloor({MaxFloor}) 사이 값이여야합니다.");

            // 테스트용 조건문
            if (IsMaxFloor)
            {
                Debug.Log("최대 층입니다.");
                return;
            }

            if (floor == value)
                return;

            int prevFloor = floor;
            floor = value;

            // 새로운 Level과 가장 가까운 Level Data를 찾아옴
            var newData = stageDatas.Last(x => x.floor <= floor);
            ChaneFloor(newData);
            onFloorChange?.Invoke(newData);
        }
    }

    // 보스 라운드의 경우에 목표 횟수를 다 채운 경우
    public bool isRoundClear => BossKill && IsBoss == true;

    // 현재 몇 마리를 잡았는지
    public int CurrentKillCount
    {
        get => currentKillCount;
        set
        {
            if (currentKillCount == value)
                return;

            int prevCount = currentKillCount;
            currentKillCount = value;

            // 현재 레벨을 가져와 그 스테이지의 목표 수를 비교합니다.
            var newData = stageDatas.Last(x => x.floor <= floor);
            if (newData.nextFloorKill <= currentKillCount)
            {
                currentKillCount = newData.nextFloorKill;
                
                if(IsBoss && !BossKill)
                {
                    onBossSummon?.Invoke(this);
                }
                else
                {
                    onNextFloorGoalGauge?.Invoke(this);
                }
            }
            onFloorGoalGaugeChange?.Invoke(this, currentKillCount, newData.nextFloorKill);
        }
    }

    public bool BossKill
    {
        get => bossKill;
        set
        {
            if (bossKill == value) return;
            bossKill = value;

            if (IsBoss && value == true)
            {
                onNextFloorGoalGauge?.Invoke(this);
            }
        }
    }

    public void UpdateBossKill(Entity entity)
    {
        BossKill = true;
    }

    // 보스 라운드 인지 ?
    public bool IsBoss => currentKillCount >= currentData.nextFloorKill && currentData.isBossRound;

    // 현재 층이 마지막 층인지 ?
    public bool IsMaxFloor => floor == maxFloor;

    public void Setup()
    {
        floor = 1;
        currentKillCount = 0;
        var newData = stageDatas.Last(x => x.floor <= floor);
        currentData = newData;

        onFloorChange?.Invoke(CurrentStageData);
        onFloorGoalGaugeChange?.Invoke(this, currentKillCount, CurrentStageData.nextFloorKill);

        UpdateCustomActions();
    }

    private void ChaneFloor(StageData stageData)
    {
        RunCustomActions(NextFloorType.FadeIn_Out);
        currentData = stageData;
        CurrentKillCount = 0;
        UpdateCustomActions();

        StageSystem.Instance.Setup();
    }

    public void RunCustomActions(NextFloorType type)
    {
        foreach (var customAction in customActionsByType[type])
            customAction.Run(this);
    }

    public void ReleaseCustomActions(NextFloorType type)
    {
        foreach (var customAction in customActionsByType[type])
            customAction.Release(this);
    }

    private void UpdateCustomActions()
    {
        customActionsByType[NextFloorType.FadeIn_Out] = currentData.customActionsFade;
    }
}
