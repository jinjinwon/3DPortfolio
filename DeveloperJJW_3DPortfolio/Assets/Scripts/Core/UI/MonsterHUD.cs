using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MonsterHUD : MonoBehaviour
{
    [Header("Stat View")]
    [SerializeField]
    private Image hpFillImage;
    [SerializeField]
    private TextMeshProUGUI hpValueText;

    [Header("Effecf List View")]
    [SerializeField]
    private SkillEffectListView effectListView;

    private Entity target;
    private RectTransform rect;

    public Vector3 offset;             // UI와 몬스터 간의 오프셋

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    private void OnDisable() => Hide();

    private void OnDestroy() => ReleaseEvents();

    public void Show(Entity target)
    {
        ReleaseEvents();

        this.target = target;
        target.onDead += OnEntityDead;
        target.onDisable += Hide;

        var stats = target.Stats;
        stats.HPStat.onValueChanged += OnHPStatChanged;


        UpdateStatView(stats.HPStat, hpFillImage, hpValueText);

        effectListView.Target = target.SkillSystem;

        gameObject.SetActive(true);
    }

    private void LateUpdate()
    {

        if(target != null)
        {
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(target.transform.position);
            rect.position = screenPosition + offset;
        }
    }

    public void Hide()
    {
        ReleaseEvents();

        target = null;
        effectListView.Target = null;

        gameObject.SetActive(false);
    }

    private void UpdateStatView(Stat stat, Image statFillAmount, TextMeshProUGUI statText)
    {
        statFillAmount.fillAmount = stat.Value / stat.MaxValue;
        statText.text = $"{Mathf.RoundToInt(stat.Value)} / {stat.MaxValue}";
    }

    private void ReleaseEvents()
    {
        if (!target)
            return;

        target.onDead -= OnEntityDead;
        target.onDisable -= Hide;
        target.Stats.HPStat.onValueChanged -= OnHPStatChanged;
    }

    private void OnHPStatChanged(Stat stat, float currentValue, float prevValue)
    => UpdateStatView(stat, hpFillImage, hpValueText);

    private void OnEntityDead(Entity entity) => Hide();
}
