using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public enum LabelData
{
    DOTween,
    Sound,
    SpriteAtlas,
    Sprites,
    Stage,
}

public enum LabelSubData
{
    BGM,
    SFX_Wav,
    SkillAtlas,
    UiAtlas,
}

public class AddressableData : MonoSingleton<AddressableData>
{
    public void LoadAssets<T>(LabelData label,string name, Action<T> action)
    {
        LoadAssets($"{label}/{name}", action);
    }

    public void LoadAssets<T>(LabelData label, LabelSubData label2, string name, Action<T> action)
    {
        string temp = label2.ToString().Replace("_", "/");

        LoadAssets($"{label}/{temp}/{name}", action);
    }

    public void LoadAssets<T>(string name, Action<T> action)
    {
        T t = default(T);
        Addressables.LoadAssetAsync<T>(name).Completed += (result) =>
        {
            if (result.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
            {
                action?.Invoke(result.Result);
            }
            else
            {
            }
        };
    }
}
