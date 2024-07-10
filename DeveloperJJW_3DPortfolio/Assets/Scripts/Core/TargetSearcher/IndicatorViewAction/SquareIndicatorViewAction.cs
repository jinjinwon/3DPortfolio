using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 보류
//[System.Serializable]
public class SquareIndicatorViewAction : IndicatorViewAction
{
    [SerializeField]
    private GameObject indicatorPrefab;
    // 생성한 indicator의 size 값
    // 만약 0이면 targetSearcher의 range를 대신 사용함
    [SerializeField]
    private float indicatorSizeOverride;
    // Indicator의 속을 채우는 fillAmount Property를 사용할 것인가?
    [SerializeField]
    private bool isUseIndicatorFillAmount;
    // Indicator를 requesterObject의 자식 Object로 만들 것인가?
    [SerializeField]
    private bool isAttachIndicatorToRequester;

    // ShowIndicator 함수로 생성한 Indicator
    private Indicator spawnedRangeIndicator;

    public override void ShowIndicator(TargetSearcher targetSearcher, GameObject requesterObject,
        object range, float angle, float fillAmount)
    {
        Debug.Assert(range is float, "SquareIndicatorViewAction::ShowIndicator - range는 null 또는 float형만 허용됩니다.");

        // 이미 Indicator를 보여주고 있다면 먼저 Hide 처리를 해줌
        HideIndicator();

        // isUseIndicatorFillAmount Option이 true가 아니면 fillAmount 값으로 0을 씀
        fillAmount = isUseIndicatorFillAmount ? fillAmount : 0f;
        // isAttachIndicatorToRequester Option이 true라면 requesterObject의 transform을 가져옴
        var attachTarget = isAttachIndicatorToRequester ? requesterObject.transform : null;
        // indicatorSizeOverride가 0이라면 인자로 받은 targetSearcher의 range를,
        // 아니라면 indicatorSizeOverride를 Indicator의 size로 씀
        float size = Mathf.Approximately(indicatorSizeOverride, 0f) ? (float)range : indicatorSizeOverride;

        // Indicator를 생성하고, Setup 함수로 위에서 정한 값들을 Setting해줌
        spawnedRangeIndicator = GameObject.Instantiate(indicatorPrefab).GetComponent<Indicator>();
        spawnedRangeIndicator.TypeChanger(IndicatorType.Square);
        spawnedRangeIndicator.Setup(size, fillAmount, attachTarget);
    }

    public override void HideIndicator()
    {
        if (!spawnedRangeIndicator)
            return;

        GameObject.Destroy(spawnedRangeIndicator.gameObject);
    }

    public override void SetFillAmount(float fillAmount)
    {
        if (!isUseIndicatorFillAmount || spawnedRangeIndicator == null)
            return;

        spawnedRangeIndicator.FillAmount = fillAmount;
    }

    public override object Clone()
    {
        return new SquareIndicatorViewAction()
        {
            indicatorPrefab = indicatorPrefab,
            indicatorSizeOverride = indicatorSizeOverride,
            isUseIndicatorFillAmount = isUseIndicatorFillAmount,
            isAttachIndicatorToRequester = isAttachIndicatorToRequester
        };
    }
}
