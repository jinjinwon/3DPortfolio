using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// RequireComponent(Type)은 Type에 적혀있는 컴포넌트와 같이 사용되어야 할 때 선언합니다.
// 또한 이 스크립트르를 게임 오브젝트에 추가할 때 Type 컴포넌트가 존재하지 않으면 같이 추가됩니다.
// 이는 의존성을 명확하게 하고 필요한 컴포넌트가 누락되는 오류를 방지합니다.
[RequireComponent(typeof(Entity))]
public class Stats : MonoBehaviour
{
    #region 6-5
    [SerializeField]
    private Stat hpStat;
    [SerializeField]
    private Stat skillCostStat;

    [SerializeField]
    private Stat goldStat;
    [SerializeField]
    private Stat cashStat;

    [Space]
    [SerializeField]
    private StatOverride[] statOverrides;

    private Stat[] stats;

    public Stat SetHPStat { get => hpStat; set => hpStat = value; }
    public Stat SetSkillCostStat { get => skillCostStat; set => skillCostStat = value; }

    public StatOverride[] SetStatOverride { get => statOverrides; set => statOverrides = value; }


    public Entity Owner { get; private set; }
    public Stat HPStat { get; private set; }
    public Stat GoldStat { get; private set; }
    public Stat CashStat { get; private set; }
    public Stat SkillCostStat { get; private set; }
    #endregion

    #region 6-8
    private void OnGUI()
    {
        return;

        if (Owner == null || !Owner.IsPlayer)
            return;

        // 좌측 상단에 넓은 Box를 그려줌
        GUI.Box(new Rect(2f, 2f, 250f, 250f), string.Empty);

        // 박스 윗 부분에 Player Stat Text를 뜨워줌
        GUI.Label(new Rect(4f, 2f, 100f, 30f), "Player Stat");

        var textRect = new Rect(4f, 22f, 200f, 30f);
        // Stat 증가를 위한 + Button의 기준 위치
        var plusButtonRect = new Rect(textRect.x + textRect.width, textRect.y, 20f, 20f);
        // Stat 감소를 위한 - Button의 기준 위치
        var minusButtonRect = plusButtonRect;
        minusButtonRect.x += 22f;

        foreach (var stat in stats)
        {
            // % Type이면 곱하기 100을 해서 0~100으로 출력
            // 0.##;-0.## format은 소숫점 2번째짜리까지 출력하되
            // 양수면 그대로 출력, 음수면 -를 붙여서 출력하라는 것
            string defaultValueAsString = stat.IsPercentType ?
                $"{stat.DefaultValue * 100f:0.##;-0.##}%" :
                stat.DefaultValue.ToString("0.##;-0.##");

            string bonusValueAsString = stat.IsPercentType ?
                $"{stat.BonusValue * 100f:0.##;-0.##}%" :
                stat.BonusValue.ToString("0.##;-0.##");

            GUI.Label(textRect, $"{stat.DisplayName}: {defaultValueAsString} ({bonusValueAsString})");
            // + Button을 누르면 Stat 증가
            if (GUI.Button(plusButtonRect, "+"))
            {
                if (stat.IsPercentType)
                    stat.DefaultValue += 0.01f;
                else
                    stat.DefaultValue += 1f;
            }

            // - Button을 누르면 Stat 감소
            if (GUI.Button(minusButtonRect, "-"))
            {
                if (stat.IsPercentType)
                    stat.DefaultValue -= 0.01f;
                else
                    stat.DefaultValue -= 1f;
            }

            // 다음 Stat 정보 출력을 위해 y축으로 한칸 내림
            textRect.y += 22f;
            plusButtonRect.y = minusButtonRect.y = textRect.y;
        }
    }
    #endregion

    #region 테스트
    [ContextMenu("이동속도 테스트 (+)")]
    public void MoveSpeedUp()
    {
        stats[5].DefaultValue -= 1;
    }

    [ContextMenu("이동속도 테스트 (-)")]
    public void MoveSpeedDown()
    {
        stats[5].DefaultValue += 1;
    }
    #endregion

    #region 6-6
    public void Setup(Entity entity)
    {
        Owner = entity;

        stats = statOverrides.Select(x => x.CreateStat()).ToArray();
        HPStat = hpStat ? GetStat(hpStat) : null;
        SkillCostStat = skillCostStat ? GetStat(skillCostStat) : null;
        GoldStat = goldStat ? GetStat(goldStat) : null;
        CashStat = cashStat ? GetStat(cashStat) : null;
    }
    
    private void OnDestroy()
    {
        foreach (var stat in stats)
            Destroy(stat);
        stats = null;
    }

    public Stat GetStat(Stat stat)
    {
        Debug.Assert(stat != null, $"Stats::GetStat - stat은 null이 될 수 없습니다.");
        return stats.FirstOrDefault(x => x.ID == stat.ID);
    }

    public bool TryGetStat(Stat stat, out Stat outStat)
    {
        Debug.Assert(stat != null, $"Stats::TryGetStat - stat은 null이 될 수 없습니다.");

        outStat = stats.FirstOrDefault(x => x.ID == stat.ID);
        return outStat != null;
    }

    public float GetValue(Stat stat)
        => GetStat(stat).Value;

    public bool HasStat(Stat stat)
    {
        Debug.Assert(stat != null, $"Stats::HasStat - stat은 null이 될 수 없습니다.");
        return stats.Any(x => x.ID == stat.ID);
    }
    #endregion

    #region 6-7
    public void SetDefaultValue(Stat stat, float value)
        => GetStat(stat).DefaultValue = value;

    public float GetDefaultValue(Stat stat)
        => GetStat(stat).DefaultValue;

    public void IncreaseDefaultValue(Stat stat, float value)
        => GetStat(stat).DefaultValue += value;

    public void SetBonusValue(Stat stat, object key, float value)
        => GetStat(stat).SetBonusValue(key, value);
    public void SetBonusValue(Stat stat, object key, object subKey, float value)
        => GetStat(stat).SetBonusValue(key, subKey, value);

    public float GetBonusValue(Stat stat)
        => GetStat(stat).BonusValue;
    public float GetBonusValue(Stat stat, object key)
        => GetStat(stat).GetBonusValue(key);
    public float GetBonusValue(Stat stat, object key, object subKey)
        => GetStat(stat).GetBonusValue(key, subKey);
    
    public void RemoveBonusValue(Stat stat, object key)
        => GetStat(stat).RemoveBonusValue(key);
    public void RemoveBonusValue(Stat stat, object key, object subKey)
        => GetStat(stat).RemoveBonusValue(key, subKey);

    public bool ContainsBonusValue(Stat stat, object key)
        => GetStat(stat).ContainsBonusValue(key);
    public bool ContainsBonusValue(Stat stat, object key, object subKey)
        => GetStat(stat).ContainsBonusValue(key, subKey);
    #endregion

    #region 6-9
#if UNITY_EDITOR
    [ContextMenu("LoadStats")]
    private void LoadStats()
    {
        var stats = Resources.LoadAll<Stat>("Stat").OrderBy(x => x.ID);
        statOverrides = stats.Select(x => new StatOverride(x)).ToArray();
    }
#endif
    #endregion
}
