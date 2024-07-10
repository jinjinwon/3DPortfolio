using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReviveState : State<Entity>
{
    private PlayerController playerController;
    private EntityMovement movement;
    protected override void Setup()
    {
        playerController = Entity.GetComponent<PlayerController>();
        movement = Entity.GetComponent<EntityMovement>();
    }

    public override void Enter()
    {
        Debug.Log("Revive");

        if (playerController)
            playerController.enabled = false;

        if (movement)
            movement.enabled = false;
    }

    // UI 버튼에 이벤트로 넣어주면 됨
    public void Revive()
    {
        // HP 값을 바꿔주면 Entity.isDead의 값이 false로 바뀜
        movement.Owner.Stats.HPStat.DefaultValue = movement.Owner.Stats.HPStat.MaxValue;
        movement.Owner.UserClickedRevive = false;
    }

    public override void Exit()
    {
        if (playerController)
            playerController.enabled = true;

        if (movement)
        {
            movement.enabled = true;
            movement.Owner.UserClickedRevive = false;
        }
    }
}
