using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class AddressableTest : MonoBehaviour
{
    public Image img;
    public TextMeshProUGUI txtResult;

    public string TargetSpriteKey;

    // Start is called before the first frame update
    void Start()
    {
        Addressables.LoadAssetAsync<Sprite>(TargetSpriteKey).Completed += (result) =>
        {
            if(result.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
            {
                txtResult.text =  "성공";
                img.sprite = result.Result;
            }
            else
            {
                txtResult.text = "실패";
                img.sprite = null;
                txtResult.text += "\nFailed to load asset: " + TargetSpriteKey;
                txtResult.text += "\nError: " + result.OperationException;
            }
        };
    }

    // Update is called once per frame
    public void OnClickGoDownloadScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}
