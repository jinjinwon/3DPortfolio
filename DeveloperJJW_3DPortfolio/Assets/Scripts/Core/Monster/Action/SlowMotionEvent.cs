using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SlowMotionEvent : AppearanceAction
{
    private CinemachineVirtualCamera bossCamera;
    private Transform bossTransform;

    [UnderlineTitle("줌인 거리")]
    public float distance;
    [UnderlineTitle("줌인 지속 시간")]
    public float duration;

    private Transform baseTransform;

    public override void Start(object data ,MonoBehaviour context, Transform transform)
    {
        bossTransform = transform;

        if (bossCamera == null)
            bossCamera = Camera.main.GetComponent<Cinemachine.CinemachineVirtualCamera>();


        baseTransform = bossCamera.Follow;
        context.StartCoroutine(SlowMotionZoomIn());
    }

    private IEnumerator SlowMotionZoomIn()
    {
        bossCamera.enabled = true;
        bossCamera.LookAt = bossTransform;
        bossCamera.Follow = bossTransform;

        CinemachineComponentBase component = bossCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        CinemachineFramingTransposer transposer = component as CinemachineFramingTransposer;

        float initialDistance = transposer.m_CameraDistance;
        float targetDistance = initialDistance / distance; // 줌인 거리

        float elapsedTime = 0f;

        Time.timeScale = 0.5f; // 슬로우 모션 효과
        while (elapsedTime < duration)
        {
            transposer.m_CameraDistance = Mathf.Lerp(initialDistance, targetDistance, elapsedTime / duration);
            elapsedTime += Time.deltaTime / Time.timeScale;
            yield return null;
        }
        transposer.m_CameraDistance = targetDistance;

        // 슬로우 모션 및 줌인 완료 후 초기 상태로 복구
        yield return new WaitForSeconds(1.0f);  // 원하는 시간 동안 줌 상태를 유지하려면 이 대기 시간을 조정하세요.

        // 원래 속도와 카메라 거리로 복원
        Time.timeScale = 1.0f;
        transposer.m_CameraDistance = initialDistance;


        bossCamera.LookAt = baseTransform;
        bossCamera.Follow = baseTransform;
    }

    public override object Clone() => new SlowMotionEvent();
}
