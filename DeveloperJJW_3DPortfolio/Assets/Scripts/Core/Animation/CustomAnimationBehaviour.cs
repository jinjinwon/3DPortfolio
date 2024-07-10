using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class CustomAnimationBehaviour : StateMachineBehaviour
{
    private EntityAI entityAI;
    public float normalizedTime = 1;

    private bool hasTriggered = false;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (entityAI != null)
            return;

        entityAI = animator.GetComponent<EntityAI>();
    }

    // 매 프레임마다 애니메이션 상태가 업데이트될 때 호출됩니다.
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        float currentTime = stateInfo.normalizedTime % 1;

        // 애니메이션의 진행 시간이 n%에 도달하고 함수가 아직 트리거되지 않았다면 함수를 호출합니다.
        if ((1.0f - currentTime) < 0.1f && !hasTriggered)
        {
            TriggerCustomFunction();
            hasTriggered = true;
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        hasTriggered = false;
    }

    void TriggerCustomFunction()
    {
        if (!entityAI.Owner.IsDead && entityAI.isSelectTargetAlive)
        {
            entityAI.Owner.OnAttack();
            entityAI.Owner.Target.TakeDamage(entityAI.Owner, null, 1);
            entityAI.Owner.IsAttack = false;
        }
    }
}
