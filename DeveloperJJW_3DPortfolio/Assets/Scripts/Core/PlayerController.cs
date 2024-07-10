using Google.Apis.Sheets.v4.Data;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
#if UNITY_EDITOR
using UnityEditor.PackageManager;
#endif
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    private Entity entity;

    [SerializeField]
    private AudioClip attackClip;
    [SerializeField]
    private AudioClip deathClip;

    private void Start()
    {
        entity = GetComponent<Entity>();
        entity.SkillSystem.onSkillTargetSelectionCompleted += ReserveSkill;
        entity.onAttack += AttackClip;
        entity.onDead += DeathClip;
    }

    private void SelectTarget(Vector2 mousePosition)
    {
        var ray = Camera.main.ScreenPointToRay(mousePosition);
        if (Physics.Raycast(ray, out var hitInfo, Mathf.Infinity))
        {
            var entity = hitInfo.transform.GetComponent<Entity>();
            if (entity)
                EntityHUD.Instance.Show(entity);
        }
    }

    private void MoveToPosition(Vector2 mousePosition)
    {
        var ray = Camera.main.ScreenPointToRay(mousePosition);
        if (Physics.Raycast(ray, out var hitInfo, Mathf.Infinity, LayerMask.GetMask("Ground")))
        {
            entity.Movement.Destination = hitInfo.point;
            entity.SkillSystem.CancelReservedSkill();
        }
    }

    private void ReserveSkill(SkillSystem skillSystem, Skill skill, TargetSearcher targetSearcher, TargetSelectionResult result)
    {
        if (result.resultMessage != SearchResultMessage.OutOfRange ||
            !skill.IsInState<SearchingTargetState>())
            return;
        
        entity.SkillSystem.ReserveSkill(skill);

        var selectionResult = skill.TargetSelectionResult;

        if (selectionResult.selectedTarget)
            entity.Movement.TraceTarget = selectionResult.selectedTarget.transform;
        else
            entity.Movement.Destination = selectionResult.selectedPosition;
    }

    private void DeathClip(Entity entity)
    {
        AudioManager.Instance.StartPlayOneShotClip(deathClip);
    }

    private void AttackClip(Entity entity)
    {
        AudioManager.Instance.StartPlayOneShotClip(attackClip);
    }
}
