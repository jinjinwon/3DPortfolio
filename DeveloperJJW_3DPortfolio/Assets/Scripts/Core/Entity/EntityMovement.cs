using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.AI;
#if UNITY_EDITOR
using static UnityEditor.Searcher.SearcherWindow.Alignment;
#endif
using static UnityEngine.UI.GridLayoutGroup;

public class EntityMovement : MonoBehaviour
{
    #region 7-1
    #region Events
    public delegate void SetDestinationHandler(EntityMovement movement, Vector3 destination);
    #endregion

    // 이동 속도로 쓸 Stat
    [SerializeField]
    private Stat moveSpeedStat;
    [SerializeField]
    private float rollTime = 0.5f;

    private NavMeshAgent agent;
    // Entity가 추적하여 움직일 대상
    private Transform traceTarget;
    // 위 moveSpeedStat으로 Entity의 Stats에서 찾아온 Stat
    private Stat entityMoveSpeedStat;

    public Stat SetMoveSpeed { get => moveSpeedStat; set => moveSpeedStat = value; }

    public Entity Owner { get; private set; }
    public float MoveSpeed => agent.speed;
    public bool IsRolling { get; private set; }

    public Transform TraceTarget
    {
        get => traceTarget;
        set
        {
            if (traceTarget == value)
                return;

            Stop();

            traceTarget = value;
            if (traceTarget)
                StartCoroutine("TraceUpdate");
        }
    }

    public Vector3 Destination
    {
        get => agent.destination;
        set
        {
            // traceTarget을 추적하는 것을 멈춤
            TraceTarget = null;
            SetDestination(value);
        }
    }

    public event SetDestinationHandler onSetDestination;
    #endregion

    #region 7-2
    public void Setup(Entity owner)
    {
        Owner = owner;

        agent = Owner.GetComponent<NavMeshAgent>();
        agent.updateRotation = false;

        var animator = Owner.Animator;
        if (animator)
            animator.SetFloat("rollSpeed", 1 / rollTime);

        entityMoveSpeedStat = moveSpeedStat ? Owner.Stats.GetStat(moveSpeedStat) : null;
        if (entityMoveSpeedStat)
        {
            agent.speed = entityMoveSpeedStat.Value;
            entityMoveSpeedStat.onValueChanged += OnMoveSpeedChanged;
        }
    }

    private void OnDisable() => Stop();

    private void OnDestroy()
    {
        if (entityMoveSpeedStat)
            entityMoveSpeedStat.onValueChanged -= OnMoveSpeedChanged;
    }

    private void SetDestination(Vector3 destination)
    {
        agent.destination = destination;
        LookAt(destination);

        onSetDestination?.Invoke(this, destination);
    }

    public void Stop()
    {
        traceTarget = null;
        StopCoroutine("TraceUpdate");

        if (agent.isOnNavMesh)
            agent.ResetPath();

        agent.velocity = Vector3.zero;
    }
    #endregion

    #region 7-3
    public void LookAt(Vector3 position)
    {
        StopCoroutine("LookAtUpdate");
        StartCoroutine("LookAtUpdate", position);
    }

    public void LookAtImmediate(Vector3 position)
    {
        // y축을 기준으로 바라보기 위해서 인자로 받은 position과 내 position의 y축을 일치시킴
        position.y = transform.position.y;
        var lookDirection = (position - transform.position).normalized;
        // 이미 바라보고 있는 방향이 아니라면 Direction으로 Rotation 값을 구하고, 이미 바라보고 있는 방향이라면 지금 그대로 유지
        var rotation = lookDirection != Vector3.zero ? Quaternion.LookRotation(lookDirection) : transform.rotation;
        transform.rotation = rotation;
    }

    private IEnumerator LookAtUpdate(Vector3 position)
    {
        position.y = transform.position.y;
        var lookDirection = (position - transform.position).normalized;
        var rotation = lookDirection != Vector3.zero ? Quaternion.LookRotation(lookDirection) : transform.rotation;
        // 속도는 180도를 움직이는데 0.15초가 걸림
        var speed = 180f / 0.01f;

        while (true)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation, speed * Time.deltaTime);
            if (transform.rotation == rotation)
                break;

            yield return null;
        }
    }
    #endregion

    #region 7-4
    public void Roll(float distance, Vector3 direction)
    {
        Stop();

        // 바라볼 방향이 존재하면 해당 방향을 바라봄
        if (direction != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(direction);

        IsRolling = true;
        StopCoroutine("RollUpdate");
        StartCoroutine("RollUpdate", distance);
    }

    public void Roll(float distance)
        => Roll(distance, transform.forward);

    private IEnumerator RollUpdate(float rollDistance)
    {
        // 현재까지 구른 시간
        float currentRollTime = 0f;
        // 이전 Frame에 이동한 거리
        float prevRollDistance = 0f;

        while (true)
        {
            currentRollTime += Time.deltaTime;

            float timePoint = currentRollTime / rollTime;
            // Easing InOutSine https://easings.net/ko#easeInOutSine
            // -(Math.cos(Math.PI * x) - 1) / 2;
            float inOutSine = -(Mathf.Cos(Mathf.PI * timePoint) - 1f) / 2f;
            float currentRollDistance = Mathf.Lerp(0f, rollDistance, inOutSine);
            // 이번 Frame에 움직여야할 거리를 구함
            float deltaValue = currentRollDistance - prevRollDistance;

            transform.position += (transform.forward * deltaValue);
            prevRollDistance = currentRollDistance;

            if (currentRollTime >= rollTime)
                break;
            else
                yield return null;
        }

        IsRolling = false;
    }
    #endregion

    #region 7-5
    // 추적 대상의 위치를 계속 Destination으로 설정해줌
    private IEnumerator TraceUpdate()
    {
        while (Owner.EntityAI.isSelectTargetAlive)
        {
            if (Vector3.SqrMagnitude(TraceTarget.position - transform.position) > 5.0f)
            {
                SetDestination(TraceTarget.position);
                Owner.IsAttack = false;
                yield return null;
            }
            else
            {
                if (Owner.IsInState<EntityDefaultState>() == true && !Owner.IsSkill)
                    Owner.IsAttack = true;
                else if (Owner.IsSkill)
                    Owner.IsAttack = false;

                Stop();
                break;
            }
        }
    }

    private void OnMoveSpeedChanged(Stat stat, float currentValue, float prevValue)
        => agent.speed = currentValue;
    #endregion
}
