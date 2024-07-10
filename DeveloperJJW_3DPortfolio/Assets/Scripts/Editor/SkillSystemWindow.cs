using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using static UnityEditor.LightingExplorerTableColumn;
using System.Diagnostics;
using Unity.VisualScripting;

public class SkillSystemWindow : EditorWindow
{
    #region 3-2

    // static ���� ����Ǿ� �ִ� ���� : â�� ���� �ѵ� ������ �����ϰ� �ְ� �ϱ� ����

    // ���� ���� �ִ� database�� index
    private static int toolbarIndex = 0;
    // Database List�� Scroll Position
    private static Dictionary<Type, Vector2> scrollPositionsByType = new();
    // ���� �����ְ� �ִ� data�� Scroll Posiiton
    private static Vector2 drawingEditorScrollPosition;
    // ���� ������ Data
    private static Dictionary<Type, IdentifiedObject> selectedObjectsByType = new();

    // Type�� Database(Category, Stat, Skill ���...)
    private readonly Dictionary<Type, IODatabase> databasesByType = new();
    // Database Data���� Type��
    private Type[] databaseTypes;
    // �� Type���� string �̸�
    private string[] databaseTypeNames;

    // ���� �����ְ� �ִ� data�� Editor class
    private Editor cachedEditor;

    // Database List�� Selected Background Texture
    private Texture2D selectedBoxTexture;
    // Database List�� Selected Style
    private GUIStyle selectedBoxStyle;

    // ����Ʈ�� ���� ��
    private readonly float listHeight = 40f;

    // ȭ�鿡 ���̴� ����Ʈ���� ���� ��
    private float visibleTotalHeight = 0f;
    #endregion

    #region 3-3
    // Editor Tools �ǿ� Skill System �׸��� �߰��ǰ�, Click�� Window�� ����
    [MenuItem("Tools/Create System")]
    private static void OpenWindow()
    {
        // Skill System�̶� ��Ī�� ���� Window�� ����
        var window = GetWindow<SkillSystemWindow>("Create System");
        // Window�� �ּ� ������� 800x700
        window.minSize = new Vector2(800, 700);
        // Window�� ������
        window.Show();
    }

    private void SetupStyle()
    {
        // 1x1 Pixel�� Texture�� ����
        selectedBoxTexture = new Texture2D(1, 1);
        // Pixel�� Color(=û��)�� ��������
        selectedBoxTexture.SetPixel(0, 0, new Color(0.31f, 0.40f, 0.50f));
        // ������ ������ Color���� ������ ������
        selectedBoxTexture.Apply();
        // �� Texture�� Window���� ������ ���̱� ������ Unity���� �ڵ� ������������(DontSave) Flag�� ��������
        // �� flag�� ���ٸ� Editor���� Play�� ����ä�� SetupStyle �Լ��� ����Ǹ�
        // texture�� Play ���¿� ���ӵǾ� Play�� �����ϸ� texture�� �ڵ� Destroy�ǹ���
        selectedBoxTexture.hideFlags = HideFlags.DontSave;

        selectedBoxStyle = new GUIStyle();
        // Normal ������ Backgorund Texture�� �� Texture�� �����������ν� �� Style�� ���� GUI�� Background�� û������ ���� ����
        // ��, Select�� Data�� Background�� û������ ���ͼ� ������
        selectedBoxStyle.normal.background = selectedBoxTexture;
    }

    private void SetupDatabases(Type[] dataTypes)
    {
        if (databasesByType.Count == 0)
        {
            // Resources Folder�� Database Folder�� �ִ��� Ȯ��
            if (!AssetDatabase.IsValidFolder("Assets/Resources/Database"))
            {
                // ���ٸ� Database Folder�� �������
                AssetDatabase.CreateFolder("Assets/Resources", "Database");
            }

            foreach (var type in dataTypes)
            {
                var database = AssetDatabase.LoadAssetAtPath<IODatabase>($"Assets/Resources/Database/{type.Name}Database.asset");
                if (database == null)
                {
                    database = CreateInstance<IODatabase>();
                    // ������ �ּҿ� IODatabase�� ����
                    AssetDatabase.CreateAsset(database, $"Assets/Resources/Database/{type.Name}Database.asset");
                    // ������ �ּ��� ���� Folder�� ����, �� Folder�� Window�� ���� ������ IdentifiedObject�� ����� �����
                    AssetDatabase.CreateFolder("Assets/Resources", type.Name);
                }

                // �ҷ��� or ������ Database�� Dictionary�� ����
                databasesByType[type] = database;
                // ScrollPosition Data ����
                scrollPositionsByType[type] = Vector2.zero;
                // SelectedObject Data ����
                selectedObjectsByType[type] = null;
            }

            databaseTypeNames = dataTypes.Select(x => x.Name).ToArray();
            databaseTypes = dataTypes;
        }
    }
    #endregion

    #region 3-5
    private void OnEnable()
    {
        SetupStyle();
        SetupDatabases(new[] { typeof(Category) , typeof(Stat), typeof(Effect),typeof(Skill), typeof(SkillTree), typeof(Stage), typeof(Monster)});
    }

    private void OnDisable()
    {
        DestroyImmediate(cachedEditor);
        DestroyImmediate(selectedBoxTexture);
    }

    private void OnGUI()
    {
        // Database���� ���� ���� IdentifiedObject���� Type Name���� Toolbar�� �׷���
        toolbarIndex = GUILayout.Toolbar(toolbarIndex, databaseTypeNames);
        EditorGUILayout.Space(4f);
        CustomEditorUtility.DrawUnderline();
        EditorGUILayout.Space(4f);

        DrawDatabase(databaseTypes[toolbarIndex]);
    }
    #endregion

    #region 3-4
    private void DrawDatabase(Type dataType)
    {
        // Dictionary���� Type�� �´� Database�� ã�ƿ�
        var database = databasesByType[dataType];
        // Editor�� Caching�Ǵ� Preview Texture�� ���� �ּ� 32��, �ִ� database�� Count���� �ø�
        // �� �۾��� �����ָ� �׷����ϴ� IO ��ü�� Icon���� ���� ��� ����� �׷����� �ʴ� ������ �߻���
        AssetPreview.SetPreviewTextureCacheSize(Mathf.Max(32, database.Count));

        // Database�� Data ����� �׷��ֱ� ����
        // (1) ���� ���� ����
        EditorGUILayout.BeginHorizontal();
        {
            // (2) ���� ���� ����, Style�� HelpBox, ���̴� 300f
            EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(300f));
            {
                // ���ݺ��� �׸� GUI�� �ʷϻ�
                GUI.color = Color.green;
                // ���ο� Data�� ����� Button�� �׷���
                if (GUILayout.Button($"New {dataType.Name}"))
                {
                    // System Namespace�� Guid ����ü�� �̿��ؼ� ���� �ĺ��ڸ� ����
                    // ���� �ĺ��ڶ�°� ������ ��ĥ �� ���� � ��
                    var guid = Guid.NewGuid();
                    var newData = CreateInstance(dataType) as IdentifiedObject;
                    // Reflection�� �̿��� codeName Field�� ã�ƿͼ� newData�� codeName�� �ӽ� codeName�� guid�� Set
                    dataType.BaseType.GetField("codeName", BindingFlags.NonPublic | BindingFlags.Instance)
                        .SetValue(newData, guid.ToString());
                    // newData�� Asset ������ ������ (ScriptableObject)
                    AssetDatabase.CreateAsset(newData, $"Assets/Resources/{dataType.Name}/{dataType.Name.ToUpper()}_{guid}.asset");

                    database.Add(newData);
                    // database���� data�� �߰�������(= Serialize ������ datas ������ ��ȭ�� ����)
                    // SetDirty�� �����Ͽ� Unity�� database�� Serialize ������ ���ߴٰ� �˸�
                    EditorUtility.SetDirty(database);
                    // Dirty flag ����� ������
                    AssetDatabase.SaveAssets();

                    // ���� ���� �ִ� IdentifiedObject�� ���� ������� IdentifiedObject�� ����
                    selectedObjectsByType[dataType] = newData;
                }

                // ���ݺ��� �׸� GUI�� ������
                GUI.color = Color.red;
                // ������ ������ Data�� �����ϴ� Button�� �׷���
                if (GUILayout.Button($"Remove Last {dataType.Name}"))
                {
                    var lastData = database.Count > 0 ? database.Datas.Last() : null;
                    if (lastData)
                    {
                        database.Remove(lastData);

                        // Data�� Asset ���� �� ��ġ�� ã�ƿͼ� ����
                        AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(lastData));
                        // database���� data�� ���������� SetDirty�� �����Ͽ� Unity�� database�� ��ȭ�� ����ٰ� �˸�
                        EditorUtility.SetDirty(database);
                        AssetDatabase.SaveAssets();
                    }
                }

                // ���ݺ��� �׸� GUI�� Cyan
                GUI.color = Color.yellow;
                // Data�� �̸� ������ �����ϴ� Button�� �׸�
                if (GUILayout.Button($"Sort By Name"))
                {
                    // ���� ����
                    database.SortByCodeName();
                    // database�� data���� ������ �ٲ������ SetDirty�� �����Ͽ� Unity�� database�� ��ȭ�� ����ٰ� �˸�
                    EditorUtility.SetDirty(database);
                    AssetDatabase.SaveAssets();
                }
                // ���ݺ��� �׸� GUI�� �Ͼ��(=������)
                GUI.color = Color.white;

                EditorGUILayout.Space(2f);
                CustomEditorUtility.DrawUnderline();
                EditorGUILayout.Space(4f);

                // ���ݺ��� Scroll ������ Box�� �׸�, UI �� ScrollView�� ������
                // ù��° ���ڴ� ���� Scroll Posiiton
                // �ι�° ���ڴ� ���� Scroll ���븦 �׸� ���ΰ�?, ����° ���ڴ� �׻� ���� Scroll ���븦 �׸� ���ΰ�?
                // �׹�° ���ڴ� �׻� ���� Scroll ������ Style, �ټ���° ���ڴ� ���� Scroll ������ Style
                // none�� �Ѱ��ְԵǸ� �ش� ����� �ƿ� ���ֹ���
                // ������° ���ڴ� Background Style
                // return ���� ������� ���ۿ� ���� �ٲ�Ե� Scroll Posiiton
                // ScrollView�� ũ��� ������ BeginVertical �Լ��� ���� ���� 300�� ������
                // BeginScrollView�� ���� Overloading�� �ֱ� ������ �׳� ���� Scroll Position�� �־ ScrollView�� �������
                // ���⼭�� ���� ���븦 ���� �������� ���ڰ� ���� �Լ��� ��

                scrollPositionsByType[dataType] = EditorGUILayout.BeginScrollView(scrollPositionsByType[dataType], false, true,
                    GUIStyle.none, GUI.skin.verticalScrollbar, GUIStyle.none);
                {
                    // Database�� ����� �׸�
                    foreach (var data in database.Datas)
                    {
                        // CodeName�� �׷��� ������ ����, ���� Icon�� �����Ѵٸ� Icon�� ũ�⸦ �����ϸ� ���� ���̸� ����
                        float labelWidth = data.Icon != null ? 200f : 245f;

                        // ���� Data�� ������ ������ Data�� selectedBoxStyle(=����� û��)�� ������
                        var style = selectedObjectsByType[dataType] == data ? selectedBoxStyle : GUIStyle.none;
                        // (3) ���� ���� ����
                        EditorGUILayout.BeginHorizontal(style, GUILayout.Height(listHeight));
                        {
                            // Data�� Icon�� �ִٸ� 40x40 ������� �׷���
                            if (data.Icon)
                            {
                                // Icon�� Preview Texture�� ������.
                                // �ѹ� ������ Texture�� Unity ���ο� Caching�Ǹ�, 
                                // Cache�� Texture ���� ������ ������ TextureCacheSize�� �����ϸ� ������ Texture���� ������
                                var preview = AssetPreview.GetAssetPreview(data.Icon);
                                GUILayout.Label(preview, GUILayout.Height(40f), GUILayout.Width(40f));
                            }
                            // Data�� CodeName�� �׷���
                            EditorGUILayout.LabelField(data.CodeName, GUILayout.Width(labelWidth), GUILayout.Height(40f));
                            // (4) ���� ���� ����, �̰� �׷��� Labe�� �߾� ������ �ϱ� ���ؼ���
                            EditorGUILayout.BeginVertical();
                            {
                                // ���� ���� ������ ������ ���±� ������ ������ 10ĭ�� ���Ե�
                                EditorGUILayout.Space(10f);

                                GUI.color = Color.red;
                                // data�� ������ �� �ִ� X Button�� �׸�
                                if (GUILayout.Button("x", GUILayout.Width(20f)))
                                {
                                    database.Remove(data);
                                    // data�� Asset ���� �� ��ġ�� ã�ƿͼ� ����
                                    AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(data));
                                    EditorUtility.SetDirty(database);
                                    AssetDatabase.SaveAssets();
                                }
                            }
                            // (4) ���� ���� ����
                            EditorGUILayout.EndVertical();

                            GUI.color = Color.white;
                        }
                        // (3) ���� ���� D����
                        EditorGUILayout.EndHorizontal();

                        // data�� �����Ǿ��ٸ� ��� Database ����� �׸��°� ���߰� ��������
                        if (data == null)
                            break;

                        // ���������� �׸� GUI�� ��ǥ�� ũ�⸦ ������
                        // �� ��� �ٷ� ���� �׸� GUI�� ��ǥ�� ��������(=BeginHorizontal)
                        var lastRect = GUILayoutUtility.GetLastRect();

                        // MosueDown Event�� mosuePosition�� GUI�ȿ� �ִٸ�(=Click) Data�� ������ ������ ó����
                        if (Event.current.type == EventType.MouseDown && lastRect.Contains(Event.current.mousePosition))
                        {
                            selectedObjectsByType[dataType] = data;
                            drawingEditorScrollPosition = Vector2.zero;
                            // Event�� ���� ó���� �ߴٰ� Unity�� �˸�
                            Event.current.Use();
                        }

                        // ���õ� �����̸� Ű �ٿ��� ������ ���
                        if ((Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.DownArrow || Event.current.keyCode == KeyCode.UpArrow)) && selectedObjectsByType[dataType] != null)
                        {
                            IdentifiedObject tempData = null;
                            int index = database.IndexOf(selectedObjectsByType[dataType]);

                            if (Event.current.keyCode == KeyCode.DownArrow) tempData = database.GetNextData(index);
                            else if (Event.current.keyCode == KeyCode.UpArrow) tempData = database.GetPrevData(index);
                            drawingEditorScrollPosition = Vector2.zero;

                            if (tempData != null)
                            {
                                selectedObjectsByType[dataType] = tempData;

                                #region ��ġ�� ���� ��ũ�� �� ��ġ ��ȭ

                                // ��ũ�� ��ġ �ʱ�ȭ
                                scrollPositionsByType[dataType] = CalculateNewScrollPosition(scrollPositionsByType[dataType], database.Datas, selectedObjectsByType[dataType], (visibleTotalHeight - 30));
                                #endregion

                                // �̺�Ʈ ó���� �Ϸ��ߴٰ� �˸�
                                Event.current.Use();
                            }
                        }
                    }
                }
                // ScrollView ����
                EditorGUILayout.EndScrollView();
                if (GUILayoutUtility.GetLastRect().height > 1)
                {
                    visibleTotalHeight = GUILayoutUtility.GetLastRect().height;
                }
            }
            // (2) ���� ���� ����
            EditorGUILayout.EndVertical();

            // ���õ� Data�� �����Ѵٸ� �ش� Data�� Editor�� �׷���
            if (selectedObjectsByType[dataType])
            {
                // ScrollView�� �׸�, �̹����� Scroll Position ������ �Ѱ��༭ ����, ���� ���� �� �ִ� �Ϲ����� ScrollView�� �׸�
                // ��, always �ɼ��� �����Ƿ� ����, ���� ����� Scroll�� ������ ������ ���� ��Ÿ��
                drawingEditorScrollPosition = EditorGUILayout.BeginScrollView(drawingEditorScrollPosition);
                {
                    EditorGUILayout.Space(2f);
                    // ù��° ���ڴ� Editor�� ���� Target
                    // �ι�° ���ڴ� Target�� Type
                    // ���� �־����� ������ Target�� �⺻ Type�� �����
                    // ����°�� ���ο��� ������� Editor�� ���� Editor ����
                    // CreateCachedEditor�� ���ο��� �������� Editor�� cachedEditor�� ���ٸ�
                    // Editor�� ���� ������ �ʰ� �׳� cachedEditor�� �״�� ��ȯ��
                    // ���� ���ο��� �������� Editor�� cachedEditor�� �ٸ��ٸ�
                    // cachedEditor�� Destroy�ϰ� ���� ���� Editor�� ����
                    Editor.CreateCachedEditor(selectedObjectsByType[dataType], null, ref cachedEditor);
                    // Editor�� �׷���
                    cachedEditor.OnInspectorGUI();
                }
                // ScrollView ����
                EditorGUILayout.EndScrollView();
            }
        }
        // (1) ���� ���� ����
        EditorGUILayout.EndHorizontal();
    }
    #endregion

    #region ������ �������� ��ġ�� ���� ��ũ�� �� �̵�
    private Vector2 CalculateNewScrollPosition(Vector2 currentScrollPosition, IReadOnlyList<IdentifiedObject> datas, IdentifiedObject selectedData, float viewHeight)
    {
        float currentYPosition = 0;
        int index = 0;
        float dataHeight = 0.0f;
        float endPosition = 0.0f;
        float newScrollPosition = 0.0f;

        // �ϴ����� ������������ �̹� ���̴� (int)(visibleTotalHeight / listHeight)���� ���� �ε��� ��ŭ ���ش�.
        // ������� �ö󰥶����� �̹� �ϴܿ� (int)(visibleTotalHeight / listHeight)���� ���̹Ƿ� �ε����� ������ �ʴ´�.

        int count = (int)(visibleTotalHeight / listHeight);

        foreach (var data in datas)
        {
            index++;
            dataHeight = GetDataHeight(data); // ������ ���� ���
            if (data == selectedData)
            {
                #region ������
                //// ���õ� �������� ����� ��ũ�� �� �Ʒ��� �ִ� ���
                //if (currentYPosition < currentScrollPosition.y)
                //{
                //    return new Vector2(currentScrollPosition.x, currentYPosition);
                //}
                //// ���õ� �������� �ϴ��� ��ũ�� �� ���� �ִ� ���
                //else if (currentYPosition + dataHeight > currentScrollPosition.y + viewHeight)
                //{
                //    UnityEngine.Debug.Log($"currentYPosition({currentYPosition}) : dataHeight({dataHeight}) : currentScrollPosition.y({currentScrollPosition.y})");
                //    return new Vector2(currentScrollPosition.x, currentYPosition + dataHeight - viewHeight);
                //}
                //else
                //{

                //}
                #endregion
                endPosition = currentYPosition + dataHeight;
                // ���õ� �����Ͱ� ��ũ�� ���� �ϴ� ��躸�� �Ʒ��� �ִ� ���
                if (endPosition + (index - count) * 2 > currentScrollPosition.y + viewHeight)
                {
                    newScrollPosition = endPosition - viewHeight; // �������� �ϴ��� ��ũ�� ���� �ϴܰ� ��ġ�ϵ��� ����

                    if(index > count)
                        newScrollPosition += (index - count) * 2;

                    // �� ��ũ�� ��ġ�� ���� ��ġ���� ū ��츸 ������Ʈ (���ʿ��� ��ũ�� ��ġ �̵� ����)
                    if (newScrollPosition > currentScrollPosition.y)
                    {
                        return new Vector2(currentScrollPosition.x, newScrollPosition);
                    }
                }
                // ���õ� �����Ͱ� ��ũ�� ���� ��� ��躸�� ���� �ִ� ���
                else if (currentYPosition + index * 2 < currentScrollPosition.y)
                {
                    currentYPosition += index * 2;
                    return new Vector2(currentScrollPosition.x, currentYPosition);
                }
                break; // ���õ� �����Ϳ� ���� ó���� �Ϸ�Ǹ� �ݺ� ����
            }
            currentYPosition += dataHeight;
        }

        return currentScrollPosition; // ������ ���� ��� ���� ��ũ�� ��ġ ��ȯ
    }

    private float GetDataHeight(IdentifiedObject data)
    {
        return listHeight; // ��ũ���� ������ �� ����Ʈ�������� ���̸� 40���� ����������
    }
    #endregion
}