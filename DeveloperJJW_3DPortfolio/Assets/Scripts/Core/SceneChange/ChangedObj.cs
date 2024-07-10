using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChangedObj : MonoBehaviour
{
    public TextMeshProUGUI txtProgressBar;         // �ε� ���� �� Text
    public Image progressBar;                      // �ε� ���� �� Image

    public void Clear()
    {
        progressBar.fillAmount = 0;
        txtProgressBar.text = $"{(0).ToString("F0")} %";
    }

    public void ActiveSet(bool active)
    {
        this.gameObject.SetActive(active);
    }

    public void UpdateGauge(float gauge)
    {
        progressBar.fillAmount = gauge;
        txtProgressBar.text = $"{(gauge * 100f).ToString("F0")} %";
    }
}
