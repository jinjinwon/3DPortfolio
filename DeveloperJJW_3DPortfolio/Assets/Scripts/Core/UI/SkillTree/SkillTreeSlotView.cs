using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class SkillTreeSlotView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    #region Events
    public delegate void SkillAcquiredHandler(SkillTreeSlotView slotView, Skill skill);
    #endregion

    [SerializeField]
    private Image iconImage;
    [SerializeField]
    private TextMeshProUGUI levelText;
    [SerializeField]
    private GameObject normalBorder;
    [SerializeField]
    private GameObject acquiredBorder;
    [SerializeField]
    private GameObject blind;

    private Entity requester;
    private Skill requesterOwnedSkill;

    public Entity Requester => requester;
    // Requester의 SkillSystem에서 찾아온 SlotSkill => Entity가 소유한 Skill
    public Skill RequesterOwnedSkill => requesterOwnedSkill;
    public Skill SlotSkill => SlotNode.Skill;
    public SkillTreeSlotNode SlotNode { get; private set; }

    public event SkillAcquiredHandler onSkillAcquired;

    private void OnDestroy()
    {
        if (!requester)
            return;

        requester.SkillSystem.onSkillRegistered -= OnSkillRegistered;
    }

    private void Update()
    {
        if (requesterOwnedSkill)
            return;

        bool isAcquirable = SlotNode.IsSkillAcquirable(requester);
        blind.SetActive(!isAcquirable);
    }

    public void SetViewTarget(Entity requester, SkillTreeSlotNode slotNode)
    {
        if (requester)
            requester.SkillSystem.onSkillRegistered -= OnSkillRegistered;

        this.requester = requester;
        SlotNode = slotNode;

        var skill = slotNode.Skill;

        requesterOwnedSkill = requester.SkillSystem.Find(skill);
        if (!requesterOwnedSkill)
            requester.SkillSystem.onSkillRegistered += OnSkillRegistered;

        iconImage.sprite = skill.Icon;

        UpdateAcquisitionUI();
        UpdateLevelText();
    }

    private void UpdateAcquisitionUI()
    {
        normalBorder.SetActive(!requesterOwnedSkill);
        acquiredBorder.SetActive(requesterOwnedSkill);
        blind.SetActive(!requesterOwnedSkill && !SlotNode.IsSkillAcquirable(requester));
    }

    private void UpdateLevelText()
    {
        int level = requesterOwnedSkill ? requesterOwnedSkill.Level : 0;
        levelText.text = $"{level} / {SlotNode.Skill.MaxLevel}";
        levelText.color = (requesterOwnedSkill && requesterOwnedSkill.IsMaxLevel) ? Color.yellow : Color.white;
    }

    private void ShowTooltip()
    {
        SkillTooltip.Instance.Show(this);
    }

    private void HideTooltip()
    {
        SkillTooltip.Instance.Hide();
    }

    private void OnSkillRegistered(SkillSystem skillSystem, Skill skill)
    {
        if (skill.ID != SlotNode.Skill.ID)
            return;

        requesterOwnedSkill = skill;

        UpdateAcquisitionUI();
        UpdateLevelText();

        skillSystem.onSkillRegistered -= OnSkillRegistered;

        onSkillAcquired?.Invoke(this, requesterOwnedSkill);
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        => ShowTooltip();

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        => HideTooltip();

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        // Entity가 skill을 소유했고, Level Up 조건을 달성 했다면 Level Up을 시킴
        if (requesterOwnedSkill && requesterOwnedSkill.IsCanLevelUp)
        {
            requesterOwnedSkill.LevelUp();
            UpdateLevelText();
            ShowTooltip();
        }
        // Entity가 Skill을 소유하지 않았고, Skill을 습득할 수 있는 상태라면 습득함
        else if (!requesterOwnedSkill && SlotNode.IsSkillAcquirable(requester))
        {
            SlotNode.AcquireSkill(requester);
            ShowTooltip();
        }
    }
}
