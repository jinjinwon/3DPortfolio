using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EntityBaseLayerBehaviour : StateMachineBehaviour
{
    private readonly static int kSpeedHash = Animator.StringToHash("speed");
    private readonly static int kAttackHash = Animator.StringToHash("attack");
    private readonly static int kIsAttack = Animator.StringToHash("isAttack");
    // private readonly static int kIsRollingHash = Animator.StringToHash("isRolling"); -> ±¸¸£±â ¾È¾¸
    private readonly static int kIsDeadHash = Animator.StringToHash("isDead");
    private readonly static int kIsVictoryHash = Animator.StringToHash("isVictory");
    private readonly static int kIsReviveHash = Animator.StringToHash("isRevive");
    private readonly static int kIsStunningHash = Animator.StringToHash("isStunning");
    // private readonly static int kIsSleepingHash = Animator.StringToHash("isSleeping"); -> ¼ö¸éÀº ¾È¾¸

    private Entity entity;
    private NavMeshAgent agent;
    private EntityMovement movement;

    // OnStateEnter is called before OnStateEnter is called on any state inside this state machine
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (entity != null)
            return;
        
        entity = animator.GetComponent<Entity>();
        agent = animator.GetComponent<NavMeshAgent>();
        movement = animator.GetComponent<EntityMovement>();
    }

    // OnStateUpdate is called before OnStateUpdate is called on any state inside this state machine
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (agent)
            animator.SetFloat(kSpeedHash, agent.desiredVelocity.sqrMagnitude / (agent.speed * agent.speed));

        //if (movement)
        //    animator.SetBool(kIsRollingHash, movement.IsRolling);

        animator.SetBool(kIsAttack, entity.IsAttack);

        if (entity.IsAttack)
            animator.SetInteger(kAttackHash, entity.AttackNumber);

        animator.SetBool(kIsVictoryHash, entity.isVictory);
        animator.SetBool(kIsDeadHash, entity.IsDead);
        animator.SetBool(kIsReviveHash, entity.isRevive);

        animator.SetBool(kIsStunningHash, entity.IsInState<StunningState>());
        //animator.SetBool(kIsSleepingHash, entity.IsInState<SleepingState>());
    }

    // OnStateExit is called before OnStateExit is called on any state inside this state machine
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateMove is called before OnStateMove is called on any state inside this state machine
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateIK is called before OnStateIK is called on any state inside this state machine
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateMachineEnter is called when entering a state machine via its Entry Node
    //override public void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
    //{
    //    
    //}

    // OnStateMachineExit is called when exiting a state machine via its Exit Node
    //override public void OnStateMachineExit(Animator animator, int stateMachinePathHash)
    //{
    //    
    //}
}
