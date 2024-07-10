using UnityEditor;
using UnityEngine;

public class ConvertMaterialsToURP
{
    [MenuItem("Tools/Convert Materials to URP")]
    public static void ConvertToURP()
    {
        string[] guids = AssetDatabase.FindAssets("t:Material");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat.shader.name == "Standard")
            {
                mat.shader = Shader.Find("Universal Render Pipeline/Lit");
            }
            else if(mat.shader.name.Contains("Animmal/") == true)
            {
                string suffix = mat.shader.name.Substring(mat.shader.name.LastIndexOf("/") + 1);

                Shader shader = Shader.Find($"Animmal (URP)/{suffix}");

                if (shader != null)
                {
                    mat.shader = shader;
                    Debug.Log("변환 성공");
                }
            }
            else if(mat.shader.name.Contains("Animmal (URP)/Custom") == true)
            {
                mat.shader = Shader.Find("Universal Render Pipeline/Lit");
            }
        }
        AssetDatabase.SaveAssets();
    }
}