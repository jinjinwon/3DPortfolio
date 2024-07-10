using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// RequireComponent(Type)�� Type�� �����ִ� ������Ʈ�� ���� ���Ǿ�� �� �� �����մϴ�.
// ���� �� ��ũ��Ʈ���� ���� ������Ʈ�� �߰��� �� Type ������Ʈ�� �������� ������ ���� �߰��˴ϴ�.
// �̴� �������� ��Ȯ�ϰ� �ϰ� �ʿ��� ������Ʈ�� �����Ǵ� ������ �����մϴ�.
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

        // ���� ��ܿ� ���� Box�� �׷���
        GUI.Box(new Rect(2f, 2f, 250f, 250f), string.Empty);

        // �ڽ� �� �κп� Player Stat Text�� �߿���
        GUI.Label(new Rect(4f, 2f, 100f, 30f), "Player Stat");

        var textRect = new Rect(4f, 22f, 200f, 30f);
        // Stat ������ ���� + Button�� ���� ��ġ
        var plusButtonRect = new Rect(textRect.x + textRect.width, textRect.y, 20f, 20f);
        // Stat ���Ҹ� ���� - Button�� ���� ��ġ
        var minusButtonRect = plusButtonRect;
        minusButtonRect.x += 22f;

        foreach (var stat in stats)
        {
            // % Type�̸� ���ϱ� 100�� �ؼ� 0~100���� ���
            // 0.##;-0.## format�� �Ҽ��� 2��°¥������ ����ϵ�
            // ����� �״�� ���, ������ -�� �ٿ��� ����϶�� ��
            string defaultValueAsString = stat.IsPercentType ?
                $"{stat.DefaultValue * 100f:0.##;-0.##}%" :
                stat.DefaultValue.ToString("0.##;-0.##");

            string bonusValueAsString = stat.IsPercentType ?
                $"{stat.BonusValue * 100f:0.##;-0.##}%" :
                stat.BonusValue.ToString("0.##;-0.##");

            GUI.Label(textRect, $"{stat.DisplayName}: {defaultValueAsString} ({bonusValueAsString})");
            // + Button�� ������ Stat ����
            if (GUI.Button(plusButtonRect, "+"))
            {
                if (stat.IsPercentType)
                    stat.DefaultValue += 0.01f;
                else
                    stat.DefaultValue += 1f;
            }

            // - Button�� ������ Stat ����
            if (GUI.Button(minusButtonRect, "-"))
            {
                if (stat.IsPercentType)
                    stat.DefaultValue -= 0.01f;
                else
                    stat.DefaultValue -= 1f;
            }

            // ���� Stat ���� ����� ���� y������ ��ĭ ����
            textRect.y += 22f;
            plusButtonRect.y = minusButtonRect.y = textRect.y;
        }
    }
    #endregion

    #region �׽�Ʈ
    [ContextMenu("�̵��ӵ� �׽�Ʈ (+)")]
    public void MoveSpeedUp()
    {
        stats[5].DefaultValue -= 1;
    }

    [ContextMenu("�̵��ӵ� �׽�Ʈ (-)")]
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
        Debug.Assert(stat != null, $"Stats::GetStat - stat�� null�� �� �� �����ϴ�.");
        return stats.FirstOrDefault(x => x.ID == stat.ID);
    }

    public bool TryGetStat(Stat stat, out Stat outStat)
    {
        Debug.Assert(stat != null, $"Stats::TryGetStat - stat�� null�� �� �� �����ϴ�.");

        outStat = stats.FirstOrDefault(x => x.ID == stat.ID);
        return outStat != null;
    }

    public float GetValue(Stat stat)
        => GetStat(stat).Value;

    public bool HasStat(Stat stat)
    {
        Debug.Assert(stat != null, $"Stats::HasStat - stat�� null�� �� �� �����ϴ�.");
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