using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DirectCallSprite : MonoBehaviour
{
    // UI ������Ʈ�� ������ �� �ڽ��� �ٷ� ȣ�����ִ� �Լ� ����.. UI�� ������ �Ǵ� ��쿡�� ����ϸ� �ȵ� ���� �� ����� �´��� �𸣰���

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
