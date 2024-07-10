using Google.Apis.Sheets.v4.Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EntityAI : MonoBehaviour
{
    private Dictionary<Entity,float> sortedTargets = new Dictionary<Entity,float>();
    // 감지할 범위
    [SerializeField]
    private float detectionRadius;
    public Entity Owner { get; private set; }
    public Dictionary<Entity, float> SortedTargets { get => sortedTargets;}

    private Collider[] hitColliders;

    // 타겟 조건에 맞는지
    public bool isTarget(Entity entity) => Owner.ControlType != entity.ControlType && entity.IsDead == false && sortedTargets.ContainsKey(entity) == false;
    // 선택된 타겟이 죽어있는지 살아 있는지
    public bool isSelectTargetAlive => Owner.Target != null && Owner.Target.IsDead == false;
    // 내가 몬스터인지?
    public bool isMonster => Owner.ControlType == EntityControlType.AI;

    public void Setup(Entity entity)
    {
        Owner = entity;
        detectionRadius = 200f;
        StartCoroutine("UpdateTarget");
    }

    private IEnumerator UpdateTarget()
    {
        // 내가 죽기전까지
        while(!Owner.IsDead)
        {
            // 캐시해서 사용하는걸로 변경해야함 ㅇㅇ..
            // 일단 테스트용
            yield return new WaitForSeconds(1f);
            TargetSerching();
            if (isSelectTargetAlive) StartTracking();
        }
    }

    private void TargetSerching()
    {
        sortedTargets.Clear();
        // 타겟 검색
        hitColliders = Physics.OverlapSphere(transform.position, detectionRadius);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.TryGetComponent(out Entity entity))
            {
                if (entity != null && isTarget(entity))
                {
                    float distance = Vector3.Distance(transform.position, hitCollider.transform.position);

                    sortedTargets.Add(entity, distance);
                }
            }
        }

        // 거리별 오름차순 정렬
        sortedTargets.OrderBy(pair => pair.Value);

        if(sortedTargets.Count <= 0 && Owner.Target != null)
        {
            Owner.Target = null;
            Owner.Movement.TraceTarget = null;
            Owner.IsAttack = false;
        }

        // 비활성화 된 경우
        if(Owner.Target != null && Owner.Target.gameObject.activeSelf == false)
        {
            Owner.Target = null;
            Owner.Movement.TraceTarget = null;
            Owner.IsAttack = false;
        }

        // 가장 가까운 타겟 설정
        if (isSelectTargetAlive == false)
            Targeting();
    }    

    private bool Targeting()
    {
        if(sortedTargets.Count > 0 && sortedTargets.FirstOrDefault().Key.IsDead == false)
        {
            // 처음 타겟이 잡히거나 타겟이 바뀐 경우 내 공격 범위안에 타겟이 존재하면 그 방향을 안보는 버그가 있음 -> 수정 코드 : Owner.Movement.LookAt(Owner.Target.transform.position);
            Owner.Target = sortedTargets.FirstOrDefault().Key;
            Owner.Movement.LookAt(Owner.Target.transform.position);
            return true;
        }
        else
        {
            Owner.Target = null;
            Owner.Movement.TraceTarget = null;

            if (Owner.IsAttack)
                Owner.IsAttack = false;

            return false;
        }
    }

    // 추적 시작 (추적 중지는 Movement에서 알아서 중지 시켜줌)
    private void StartTracking()
    {
        Owner.Movement.TraceTarget = Owner.Target.gameObject.transform;
    }
}
