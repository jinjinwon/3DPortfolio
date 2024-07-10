using System;
using System.Threading.Tasks;
using UnityEngine;

[System.Serializable]
public class FadeinFadeOut : NextFloorAction
{
    [SerializeField]
    private GameObject prefab;
    [SerializeField]
    private float fadeInTime;
    [SerializeField]
    private float fadeOutTime;

    private GameObject spawnedObject;
    private bool playing = false;
    private float elapsedTime = 0f;
    private CanvasGroup canvasGroup;
    private enum FadeState { None, FadingIn, Waiting, FadingOut }
    private FadeState currentState = FadeState.None;

    public override void Run(object data)
    {
        playing = true;
        FadeAction();
    }

    private async void FadeAction()
    {
        if (prefab == null)
        {
            Debug.LogError("Prefab is not assigned.");
            return;
        }

        // Canvas 찾기
        Canvas targetCanvas = GameObject.FindObjectOfType<Canvas>();
        if (targetCanvas == null)
        {
            Debug.LogError("No Canvas found in the scene.");
            return;
        }

        // Prefab을 Canvas의 자식으로 생성
        spawnedObject = GameObject.Instantiate(prefab, targetCanvas.transform);
        canvasGroup = spawnedObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        elapsedTime = 0f;
        currentState = FadeState.FadingIn;

        await UpdateFade();
    }

    private async Task UpdateFade()
    {
        while (playing && currentState != FadeState.None)
        {
            elapsedTime += Time.deltaTime;

            switch (currentState)
            {
                case FadeState.FadingIn:
                    if (elapsedTime < fadeInTime)
                    {
                        canvasGroup.alpha = Mathf.Clamp01(elapsedTime / fadeInTime);
                    }
                    else
                    {
                        canvasGroup.alpha = 1f;
                        elapsedTime = 0f;
                        currentState = FadeState.Waiting;
                    }
                    break;

                case FadeState.Waiting:
                    if (elapsedTime >= 1f) // 1초 대기 후 페이드 아웃 시작
                    {
                        elapsedTime = 0f;
                        currentState = FadeState.FadingOut;
                    }
                    break;

                case FadeState.FadingOut:
                    if (elapsedTime < fadeOutTime)
                    {
                        canvasGroup.alpha = Mathf.Clamp01(1f - (elapsedTime / fadeOutTime));
                    }
                    else
                    {
                        canvasGroup.alpha = 0f;
                        currentState = FadeState.None;
                        Release(null);
                    }
                    break;
            }

            await Task.Yield(); // 다음 프레임까지 대기
        }
    }

    public override void Release(object data)
    {
        playing = false;
        currentState = FadeState.None;
        if (spawnedObject != null)
        {
            GameObject.Destroy(spawnedObject);
        }
    }

    public override object Clone()
    {
        return new FadeinFadeOut()
        {
            prefab = prefab,
            fadeInTime = fadeInTime,
            fadeOutTime = fadeOutTime,
        };
    }
}