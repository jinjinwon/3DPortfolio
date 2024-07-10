using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserSetting : MonoBehaviour
{
    [SerializeField]
    private AudioSetting audioSetting;

    public void Active()
    {
        if (this.gameObject.activeSelf == false)
        {
            this.gameObject.SetActive(true);
            Time.timeScale = 0;
        }
        else
        {
            this.gameObject.SetActive(false);
            Time.timeScale = 1.0f;
        }
    }

    private void OnEnable()
    {
        audioSetting.Initialized();
    }

    public void ClickExit()
    {
        Application.Quit();
    }
}
