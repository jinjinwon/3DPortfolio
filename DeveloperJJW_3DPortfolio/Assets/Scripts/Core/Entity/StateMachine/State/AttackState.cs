using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackState : State<Entity>
{
    private EntityMovement movement;
    protected override void Setup()
    {
        movement = Entity.GetComponent<EntityMovement>();
    }

    public override void Enter()
    {
        if (movement)
        {
            movement.enabled = false;
        }
    }

    public override void Exit()
    {
        if (movement)
            movement.enabled = true;
    }
}
