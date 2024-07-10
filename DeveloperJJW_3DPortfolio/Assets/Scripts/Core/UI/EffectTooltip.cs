using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class EffectTooltip : MonoSingleton<EffectTooltip>
{
    [SerializeField]
    private TextMeshProUGUI descriptionText;

    private void Start() => gameObject.SetActive(false);

    private void Update() => transform.position = Input.mousePosition;

    public void Show(Effect effect)
    {
        descriptionText.text = effect.Description_Tooltip;

        transform.position = Input.mousePosition;

        float xPivot = transform.localPosition.x > 0f ? 1f : 0f;
        float yPivot = transform.localPosition.y > 0f ? 1f : 0f;
        GetComponent<RectTransform>().pivot = new(xPivot, yPivot);

        gameObject.SetActive(true);
    }

    public void Hide() => gameObject.SetActive(false);
}
