using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class SceneChanged : MonoSingleton<SceneChanged>
{
    [SerializeField]
    private ChangedObj loadingScreen; // �ε� ȭ�� UI
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

        // �� ������ �񵿱� �ε�
        AsyncOperation asyncLoadEmpty = SceneManager.LoadSceneAsync("MemoryClearScene");
        while (!asyncLoadEmpty.isDone)
        {
            yield return null;
        }

        // �޸� ����
        yield return Resources.UnloadUnusedAssets();
        System.GC.Collect();

        // ���ϴ� ���� �񵿱� �ε�
        AsyncOperation asyncLoadTarget = SceneManager.LoadSceneAsync(scene);
        asyncLoadTarget.allowSceneActivation = false;

        float displayProgress = 0f;

        while (!asyncLoadTarget.isDone)
        {
            // ���� �ε� ���� ��Ȳ
            float targetProgress = Mathf.Clamp01(asyncLoadTarget.progress / 0.9f);

            // �ε巴�� ������Ű��
            displayProgress = Mathf.Lerp(displayProgress, targetProgress, smoothingSpeed);

            loadingScreen.UpdateGauge(displayProgress);

            // �ε��� ���� �Ϸ�Ǹ� �� Ȱ��ȭ
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

        // �ε� ȭ�� �����
        if (loadingScreen != null)
        {
            loadingScreen.ActiveSet(false);
            loadingScreen.Clear();
        }

        this.gameObject.SetActive(false);
    }
}
