using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BossAppearanceEvent : AppearanceAction
{
    private CinemachineVirtualCamera bossCamera;
    private Transform bossTransform;

    [UnderlineTitle("줌인 거리")]
    public float distance;
    [UnderlineTitle("줌인 지속 시간")]
    public float duration;

    private Transform baseTransform;
    public override void Start(object data, MonoBehaviour context, Transform transform)
    {
        bossTransform = transform;

        if(bossCamera == null)
            bossCamera = Camera.main.GetComponent<Cinemachine.CinemachineVirtualCamera>();

        baseTransform = bossCamera.Follow;
        context.StartCoroutine(ZoomInOnBoss());
    }

    private IEnumerator ZoomInOnBoss()
    {
        bossCamera.enabled = true;
        bossCamera.LookAt = bossTransform;
        bossCamera.Follow = bossTransform;

        CinemachineComponentBase component = bossCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        CinemachineFramingTransposer transposer = component as CinemachineFramingTransposer;

        float initialDistance = transposer.m_CameraDistance;
        float targetDistance = initialDistance / distance; // 줌인 거리

        float elapsedTime = 0f;

        // 줌인 애니메이션
        while (elapsedTime < duration)
        {
            transposer.m_CameraDistance = Mathf.Lerp(initialDistance, targetDistance, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transposer.m_CameraDistance = targetDistance;

        // 잠시 대기 후 원래 거리로 복귀
        yield return new WaitForSeconds(1); // 이 대기 시간을 원하는 대로 조정할 수 있습니다.
        transposer.m_CameraDistance = initialDistance; // 원래 카메라 거리로 복귀

        bossCamera.LookAt = baseTransform;
        bossCamera.Follow = baseTransform;
    }

    public override object Clone() => new BossAppearanceEvent();
}
