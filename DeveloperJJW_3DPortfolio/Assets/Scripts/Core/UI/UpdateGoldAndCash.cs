using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.CullingGroup;

public class UpdateGoldAndCash : MonoBehaviour
{
    [SerializeField]
    private Entity entity;

    public TextMeshProUGUI goldTM;
    public TextMeshProUGUI cashTM;

    public void Start()
    {
        entity.Stats.CashStat.onValueChanged += UpdateCash;
        entity.Stats.GoldStat.onValueChanged += UpdateGold;

        // 원래는 기존값에서 변경된 값만큼 차이를 보여줘야 합니다 ㅇㅇ... 시간나면 작업하도록...
        // 일단 작동되게 ㅇㅇ..
        UpdateGold(entity.Stats.CashStat,0,0);
        UpdateCash(entity.Stats.CashStat, 0, 0);
    }

    public void UpdateGold(Stat stat, float currentValue, float prevValue)
    {
        goldTM.text = entity.Stats.GoldStat.DefaultValue.ToString("#,##0");
    }

    public void UpdateCash(Stat stat, float currentValue, float prevValue)
    {
        cashTM.text = entity.Stats.CashStat.DefaultValue.ToString("#,##0");
    }

    public void OnDestroy()
    {
        entity.Stats.CashStat.onValueChanged -= UpdateCash;
        entity.Stats.GoldStat.onValueChanged -= UpdateGold;
    }
}
