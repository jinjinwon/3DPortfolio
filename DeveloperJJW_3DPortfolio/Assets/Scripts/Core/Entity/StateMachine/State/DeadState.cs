using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadState : State<Entity>
{
    private PlayerController playerController;
    private EntityMovement movement;
    private MonsterPool monsterPool;
    protected override void Setup()
    {
        playerController = Entity.GetComponent<PlayerController>();
        movement = Entity.GetComponent<EntityMovement>();
        monsterPool = Entity.GetComponent<MonsterPool>();
    }

    public override void Enter()
    {
        Debug.Log("Dead");

        if (playerController)
            playerController.enabled = false;

        if (movement)
            movement.enabled = false;

        if (monsterPool)
            monsterPool.Dead();
    }

    public override void Exit()
    {
        if (playerController)
            playerController.enabled = true;

        if (movement)
            movement.enabled = true;
    }

}
