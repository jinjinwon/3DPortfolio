using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.U2D;

public class SpriteManager : MonoSingleton<SpriteManager>
{
    public static Dictionary<SpriteAtlasEnums, SpriteAtlas> dic_Atlas = new();

    // 다운로드 받고 난 이후
    public void AddAtlas()
    {
        Addressables.LoadAssetAsync<SpriteAtlas>($"SpriteAtlas/{SpriteAtlasEnums.UiAtlas.ToString()}").Completed += (result) =>
        {
            if (result.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
            {
                if (dic_Atlas.ContainsKey(SpriteAtlasEnums.UiAtlas) == false)
                {
                    dic_Atlas.Add(SpriteAtlasEnums.UiAtlas, result.Result);
                }
                else
                {
                    dic_Atlas[SpriteAtlasEnums.UiAtlas] = result.Result;
                }
            }
        };

        Addressables.LoadAssetAsync<SpriteAtlas>($"SpriteAtlas/{SpriteAtlasEnums.SkillAtlas.ToString()}").Completed += (result) =>
        {
            if (result.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
            {
                if (dic_Atlas.ContainsKey(SpriteAtlasEnums.SkillAtlas) == false)
                {
                    dic_Atlas.Add(SpriteAtlasEnums.SkillAtlas, result.Result);
                }
                else
                {
                    dic_Atlas[SpriteAtlasEnums.SkillAtlas] = result.Result;
                }
            }
        };
    }

    // Sprite 호출 함수
    public Sprite GetSprite(SpriteAtlasEnums enums, string name)
    {
        return dic_Atlas[enums].GetSprite(name);
    }
}
