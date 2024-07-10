using UnityEngine;
using UnityEditor;

public class SnapToGrid : EditorWindow
{
    private float gridSize = 1.0f;

    [MenuItem("Tools/Snap To Grid")]
    public static void ShowWindow()
    {
        GetWindow<SnapToGrid>("Snap To Grid");
    }

    void OnGUI()
    {
        GUILayout.Label("Grid Settings", EditorStyles.boldLabel);
        gridSize = EditorGUILayout.FloatField("Grid Size", gridSize);

        if (GUILayout.Button("Snap Selected Objects"))
        {
            SnapSelectedObjects();
        }
    }

    private void SnapSelectedObjects()
    {
        foreach (GameObject obj in Selection.gameObjects)
        {
            Undo.RecordObject(obj.transform, "Snap Objects");
            obj.transform.position = RoundToGrid(obj.transform.position);
        }
    }

    private Vector3 RoundToGrid(Vector3 position)
    {
        position.x = Mathf.Round(position.x / gridSize) * gridSize;
        position.y = Mathf.Round(position.y / gridSize) * gridSize;
        position.z = Mathf.Round(position.z / gridSize) * gridSize;
        return position;
    }
}