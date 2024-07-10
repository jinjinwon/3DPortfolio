using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class FloorHUD : MonoSingleton<FloorHUD>
{
    [SerializeField]
    private TextMeshProUGUI floorText;

    [Header("Stat View")]
    [SerializeField]
    private Image killFillImage;
    [SerializeField]
    private TextMeshProUGUI killValueText;

    [SerializeField]
    private Button action_Button;
    [SerializeField]
    private TextMeshProUGUI actionText;

    private Stage stage;


    public void Show(Stage stage)
    {
        ReleaseEvents();

        this.stage = stage;

        stage.onNextFloorGoalGauge += UpdateNextFloor;
        stage.onFloorChange += UpdateFloor;
        stage.onFloorGoalGaugeChange += UpdateKillView;
        stage.onBossSummon += UpdateBossSummon;
    }

    private void ReleaseEvents()
    {
        if (!stage)
            return;

        stage.onNextFloorGoalGauge -= UpdateNextFloor;
        stage.onFloorGoalGaugeChange -= UpdateKillView;
        stage.onFloorChange -= UpdateFloor;
        stage.onBossSummon -= UpdateBossSummon;
    }

    private void UpdateNextFloor(Stage stage)
    {
        action_Button.gameObject.SetActive(true);

        action_Button.image.sprite = SpriteManager.Instance.GetSprite(SpriteAtlasEnums.UiAtlas, "icons_18");
        actionText.text = $"´ÙÀ½ Ãþ";

        action_Button.onClick.RemoveAllListeners();
        action_Button.onClick.AddListener(() => StageSystem.Instance.NextFloor());
    }

    private void UpdateBossSummon(Stage stage)
    {
        action_Button.gameObject.SetActive(true);

        action_Button.image.sprite = SpriteManager.Instance.GetSprite(SpriteAtlasEnums.UiAtlas, "icons_27");
        actionText.text = $"º¸½º ¼ÒÈ¯";

        action_Button.onClick.RemoveAllListeners();
        action_Button.onClick.AddListener(()  => StageSystem.Instance.CreateBossNpc());
    }

    private void UpdateFloor(StageData stageData)
    {
        action_Button.gameObject.SetActive(false);
        floorText.text = $"{stageData.floor} Ãþ";
    }

    private void UpdateKillView(Stage stage, int currentKillCount, int maxKillCount)
    {
        killFillImage.fillAmount = (float)currentKillCount / maxKillCount;
        killValueText.text = $"{Mathf.RoundToInt(currentKillCount)} / {maxKillCount}";
    }
}
