using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class SelectTarget : TargetSelectionAction
{
    [Header("Data")]
    // 검색 범위, 0일 경우 무한대를 의미함.
    [Min(0f)]
    [SerializeField]
    private float range;
    [Range(0f, 360f)]
    [SerializeField]
    private float angle;

    private MouseController mouseController;
    private TargetSearcher targetSearcher;
    private Entity requesterEntity;
    private GameObject requesterObject;
    private SelectCompletedHandler onSelectCompleted;

    public override object Range => range;
    public override object ScaledRange => range * Scale;
    public override float Angle => angle;

    public SelectTarget() { }

    public SelectTarget(SelectTarget copy)
        : base(copy)
    {
        range = copy.range;
        angle = copy.angle;
    }


    // SelectImmidiateByPlayer 함수의 오버로딩 추상 함수, Posiiton 대신 Screen Position를 받음.
    protected abstract TargetSelectionResult SelectImmediateByPlayer(Vector2 screenPoint, TargetSearcher targetSearcher, Entity requesterEntity,
    GameObject requesterObject);

    // 인자로 받은 position을 Screen Position으로 변환하여, 위 SelectImmidiateByPlayer 함수를 실행함
    protected sealed override TargetSelectionResult SelectImmediateByPlayer(TargetSearcher targetSearcher, Entity requesterEntity,
        GameObject requesterObject, Vector3 position)
        => SelectImmediateByPlayer(Camera.main.WorldToScreenPoint(position), targetSearcher, requesterEntity, requesterObject);

    private void ResetMouseController()
    {
        mouseController.ChangeCursor(CursorType.Default);
        mouseController.onLeftClicked -= OnMouseLeftClick;
        mouseController.onRightClicked -= OnMouseRightClick;
        mouseController = null;
    }

    public override void Select(TargetSearcher targetSearcher, Entity requesterEntity,
        GameObject requesterObject, SelectCompletedHandler onSelectCompleted)
    {
        //if (requesterEntity.IsPlayer)
        //{
        //    this.targetSearcher = targetSearcher;
        //    this.requesterEntity = requesterEntity;
        //    this.requesterObject = requesterObject;
        //    this.onSelectCompleted = onSelectCompleted;

        //    mouseController = Camera.main.GetComponent<MouseController>();
        //    mouseController.ChangeCursor(CursorType.BlueArrow);
        //    mouseController.onLeftClicked += OnMouseLeftClick;
        //    mouseController.onRightClicked += OnMouseRightClick;
        //}
        //else
        //    onSelectCompleted.Invoke(SelectImmediateByAI(targetSearcher, requesterEntity,
        //        requesterObject, requesterEntity.Target.transform.position));

        onSelectCompleted.Invoke(SelectImmediateByAI(targetSearcher, requesterEntity,
                requesterObject, requesterEntity.Target.transform.position));
    }

    public override void CancelSelect(TargetSearcher targetSearcher)
    {
        if (mouseController)
            ResetMouseController();
    }

    public override bool IsInRange(TargetSearcher targetSearcher, Entity requesterEntity, GameObject requesterObject, Vector3 targetPosition)
    {
        #region 강사 풀이
        var requesterTransform = requesterObject.transform;
        targetPosition.y = requesterTransform.position.y;

        float sqrRange = range * range * (IsUseScale ? Scale : 1f);
        Vector3 relativePosition = targetPosition - requesterTransform.position;
        float angle = Vector3.Angle(relativePosition, requesterTransform.forward);
        bool IsInAngle = angle <= (Angle / 2f);

        // 검색 범위가 무한이거나, target이 Range와 Angle안에 있다면 true
        return Mathf.Approximately(0f, range) ||
            (Vector3.SqrMagnitude(relativePosition) <= sqrRange && IsInAngle);
        #endregion

        #region 내 풀이
        //var requesterTransform = requesterObject.transform;

        //// y축 무시
        //targetPosition.y = requesterTransform.position.y;

        //float scaledRange = range * (IsUseScale ? Scale : 1f);
        //float sqrRange = scaledRange * scaledRange;

        //// y축 무시 이후 상대 위치 계산
        //Vector3 relativePosition = targetPosition - requesterTransform.position;
        //relativePosition.y = 0; // Ensure y is zero to ignore y axis

        //// 각도 계산
        //Vector3 forward2D = new Vector3(requesterTransform.forward.x, 0, requesterTransform.forward.z);
        //Vector3 relativePosition2D = new Vector3(relativePosition.x, 0, relativePosition.z);
        //float angle = Vector3.Angle(forward2D, relativePosition2D);
        //bool IsInAngle = angle <= (Angle / 2f);

        //// 대상이 존재하는지에 대한 체크
        //bool isInRange = Vector3.SqrMagnitude(relativePosition2D) <= sqrRange;

        //// 범위가 무한으로 설정되었거나 범위내에 상대가 존재한다
        //return Mathf.Approximately(0f, range) || (isInRange && IsInAngle);
        #endregion
    }

    protected override IReadOnlyDictionary<string, string> GetStringsByKeyword()
    {
        var dictionary = new Dictionary<string, string>() { { "range", range.ToString("0.##") } };
        return dictionary;
    }

    private void OnMouseLeftClick(Vector2 mousePosition)
    {
        ResetMouseController();

        // SelectImmidiateByPlayer 함수에 Mouse Position을 넣어서 결과 값을 Delegate로 전달함 
        onSelectCompleted?.Invoke(SelectImmediateByPlayer(mousePosition, targetSearcher, requesterEntity, requesterObject));
    }

    private void OnMouseRightClick(Vector2 mousePosition)
    {
        ResetMouseController();

        // 결과 값으로 실패를 Delegate로 전달함
        onSelectCompleted?.Invoke(new TargetSelectionResult(Vector3.zero, SearchResultMessage.Fail));
    }
}
