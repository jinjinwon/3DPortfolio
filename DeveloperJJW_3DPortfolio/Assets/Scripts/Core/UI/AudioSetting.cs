using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AudioSetting : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI textMeshPro_BGM;
    [SerializeField]
    private Scrollbar scrollbar_BGM;

    [SerializeField]
    private TextMeshProUGUI textMeshPro_SFX;
    [SerializeField]
    private Scrollbar scrollbar_SFX;

    private void Start()
    {
        AudioManager.Instance.onBGMValueChanged += UpdateBGMGauge;
        AudioManager.Instance.onSFXValueChanged += UpdateSFXGauge;
    }

    public void Initialized()
    {
        UpdateBGMGauge(AudioManager.Instance.BGMVolume);
        UpdateSFXGauge(AudioManager.Instance.SFXVolume);
    }

    public void UpdateBGMGauge(float value)
    {
        textMeshPro_BGM.text = $"BGM Value \n {(value * 100).ToString("F1")} %";
        scrollbar_BGM.value = value ;
    }

    public void UpdateSFXGauge(float value)
    {
        textMeshPro_SFX.text = $"SFX Value \n {(value * 100).ToString("F1")} %";
        scrollbar_SFX.value = value ;
    }

    public void OnValueChangeBGM(float value)
    {
        Debug.Log("BGM Value: " + value);
        AudioManager.Instance.BGMVolume = value;
    }

    public void OnValueChangeSFX(float value)
    {
        Debug.Log("SFX Value: " + value);
        AudioManager.Instance.SFXVolume = value;
    }
}
