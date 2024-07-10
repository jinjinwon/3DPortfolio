using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneChanedTest : MonoBehaviour
{
    public void OnClickChanedScene(string scene)
    {
        SceneChanged.Instance.LoadScene(scene);
    }
}
