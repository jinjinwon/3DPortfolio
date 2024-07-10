using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DirectCallSprite : MonoBehaviour
{
    // UI 컴포넌트에 부착한 뒤 자신을 바로 호출해주는 함수 ㅇㅇ.. UI가 변동이 되는 경우에는 사용하면 안됨 아직 이 방식이 맞는지 모르겠음

    private Image image;

    [SerializeField]
    private SpriteAtlasEnums enums;
    [SerializeField]
    private string sprite;

    private void Start()
    {
        image = GetComponent<Image>();
        image.sprite = SpriteManager.Instance.GetSprite(enums, sprite);
    }

    private void OnEnable()
    {
        if(image != null)
        {
            image.sprite = SpriteManager.Instance.GetSprite(enums, sprite);
        }
    }

    private void OnDisable()
    {
        if(image != null)
        {
            image.sprite = null;
        }
    }
}
