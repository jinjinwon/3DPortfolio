using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class SceneChanged : MonoSingleton<SceneChanged>
{
    [SerializeField]
    private ChangedObj loadingScreen; // 로딩 화면 UI
    [SerializeField]
    private float smoothingSpeed = 0.1f;

    public void LoadScene(string scene)
    {
        DontDestoryObject();

        this.gameObject.SetActive(true);
        StartCoroutine(LoadSceneRoutine(scene));
    }

    private IEnumerator LoadSceneRoutine(string scene)
    {
        if (loadingScreen != null)
        {
            loadingScreen.ActiveSet(true);
        }

        // 빈 씬으로 비동기 로드
        AsyncOperation asyncLoadEmpty = SceneManager.LoadSceneAsync("MemoryClearScene");
        while (!asyncLoadEmpty.isDone)
        {
            yield return null;
        }

        // 메모리 정리
        yield return Resources.UnloadUnusedAssets();
        System.GC.Collect();

        // 원하는 씬을 비동기 로드
        AsyncOperation asyncLoadTarget = SceneManager.LoadSceneAsync(scene);
        asyncLoadTarget.allowSceneActivation = false;

        float displayProgress = 0f;

        while (!asyncLoadTarget.isDone)
        {
            // 실제 로딩 진행 상황
            float targetProgress = Mathf.Clamp01(asyncLoadTarget.progress / 0.9f);

            // 부드럽게 증가시키기
            displayProgress = Mathf.Lerp(displayProgress, targetProgress, smoothingSpeed);

            loadingScreen.UpdateGauge(displayProgress);

            // 로딩이 거의 완료되면 씬 활성화
            if (targetProgress >= 0.9f && displayProgress >= 0.99f)
            {
                asyncLoadTarget.allowSceneActivation = true;
            }

            yield return null;
        }

        while (!StageSystem.Instance.IsMapCreated)
        {
            yield return null;
        }

        // 로딩 화면 숨기기
        if (loadingScreen != null)
        {
            loadingScreen.ActiveSet(false);
            loadingScreen.Clear();
        }

        this.gameObject.SetActive(false);
    }
}
