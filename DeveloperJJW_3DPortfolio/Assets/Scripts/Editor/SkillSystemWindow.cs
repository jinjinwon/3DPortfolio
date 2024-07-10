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

    // static 으로 선언되어 있는 이유 : 창을 껏다 켜도 정보를 저장하고 있게 하기 위해

    // 현재 보고 있는 database의 index
    private static int toolbarIndex = 0;
    // Database List의 Scroll Position
    private static Dictionary<Type, Vector2> scrollPositionsByType = new();
    // 현재 보여주고 있는 data의 Scroll Posiiton
    private static Vector2 drawingEditorScrollPosition;
    // 현재 선택한 Data
    private static Dictionary<Type, IdentifiedObject> selectedObjectsByType = new();

    // Type별 Database(Category, Stat, Skill 등등...)
    private readonly Dictionary<Type, IODatabase> databasesByType = new();
    // Database Data들의 Type들
    private Type[] databaseTypes;
    // 위 Type들의 string 이름
    private string[] databaseTypeNames;

    // 현재 보여주고 있는 data의 Editor class
    private Editor cachedEditor;

    // Database List의 Selected Background Texture
    private Texture2D selectedBoxTexture;
    // Database List의 Selected Style
    private GUIStyle selectedBoxStyle;

    // 리스트의 높이 값
    private readonly float listHeight = 40f;

    // 화면에 보이는 리스트들의 높이 합
    private float visibleTotalHeight = 0f;
    #endregion

    #region 3-3
    // Editor Tools 탭에 Skill System 항목이 추가되고, Click시 Window가 열림
    [MenuItem("Tools/Create System")]
    private static void OpenWindow()
    {
        // Skill System이란 명칭을 가진 Window를 생성
        var window = GetWindow<SkillSystemWindow>("Create System");
        // Window의 최소 사이즈는 800x700
        window.minSize = new Vector2(800, 700);
        // Window를 보여줌
        window.Show();
    }

    private void SetupStyle()
    {
        // 1x1 Pixel의 Texture를 만듬
        selectedBoxTexture = new Texture2D(1, 1);
        // Pixel의 Color(=청색)를 설정해줌
        selectedBoxTexture.SetPixel(0, 0, new Color(0.31f, 0.40f, 0.50f));
        // 위에서 설정한 Color값을 실제로 적용함
        selectedBoxTexture.Apply();
        // 이 Texture는 Window에서 관리할 것이기 때문에 Unity에서 자동 관리하지말라(DontSave) Flag를 설정해줌
        // 이 flag가 없다면 Editor에서 Play를 누른채로 SetupStyle 함수가 실행되면
        // texture가 Play 상태에 종속되어 Play를 중지하면 texture가 자동 Destroy되버림
        selectedBoxTexture.hideFlags = HideFlags.DontSave;

        selectedBoxStyle = new GUIStyle();
        // Normal 상태의 Backgorund Texture를 위 Texture로 설정해줌으로써 이 Style을 쓰는 GUI는 Background가 청색으로 나올 것임
        // 즉, Select된 Data의 Background는 청색으로 나와서 강조됨
        selectedBoxStyle.normal.background = selectedBoxTexture;
    }

    private void SetupDatabases(Type[] dataTypes)
    {
        if (databasesByType.Count == 0)
        {
            // Resources Folder에 Database Folder가 있는지 확인
            if (!AssetDatabase.IsValidFolder("Assets/Resources/Database"))
            {
                // 없다면 Database Folder를 만들어줌
                AssetDatabase.CreateFolder("Assets/Resources", "Database");
            }

            foreach (var type in dataTypes)
            {
                var database = AssetDatabase.LoadAssetAtPath<IODatabase>($"Assets/Resources/Database/{type.Name}Database.asset");
                if (database == null)
                {
                    database = CreateInstance<IODatabase>();
                    // 지정한 주소에 IODatabase를 생성
                    AssetDatabase.CreateAsset(database, $"Assets/Resources/Database/{type.Name}Database.asset");
                    // 지정한 주소의 하위 Folder를 생성, 이 Folder는 Window에 의해 생성된 IdentifiedObject가 저장될 장소임
                    AssetDatabase.CreateFolder("Assets/Resources", type.Name);
                }

                // 불러온 or 생성된 Database를 Dictionary에 보관
                databasesByType[type] = database;
                // ScrollPosition Data 생성
                scrollPositionsByType[type] = Vector2.zero;
                // SelectedObject Data 생성
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
        // Database들이 관리 중인 IdentifiedObject들의 Type Name으로 Toolbar를 그려줌
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
        // Dictionary에서 Type에 맞는 Database를 찾아옴
        var database = databasesByType[dataType];
        // Editor에 Caching되는 Preview Texture의 수를 최소 32개, 최대 database의 Count까지 늘림
        // 이 작업을 안해주면 그려야하는 IO 객체의 Icon들이 많을 경우 제대로 그려지지 않는 문제가 발생함
        AssetPreview.SetPreviewTextureCacheSize(Mathf.Max(32, database.Count));

        // Database의 Data 목록을 그려주기 시작
        // (1) 가로 정렬 시작
        EditorGUILayout.BeginHorizontal();
        {
            // (2) 세로 정렬 시작, Style은 HelpBox, 넓이는 300f
            EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(300f));
            {
                // 지금부터 그릴 GUI는 초록색
                GUI.color = Color.green;
                // 새로운 Data를 만드는 Button을 그려줌
                if (GUILayout.Button($"New {dataType.Name}"))
                {
                    // System Namespace의 Guid 구조체를 이용해서 고유 식별자를 생성
                    // 고유 식별자라는건 절때로 겹칠 수 없는 어떤 값
                    var guid = Guid.NewGuid();
                    var newData = CreateInstance(dataType) as IdentifiedObject;
                    // Reflection을 이용해 codeName Field를 찾아와서 newData의 codeName을 임시 codeName인 guid로 Set
                    dataType.BaseType.GetField("codeName", BindingFlags.NonPublic | BindingFlags.Instance)
                        .SetValue(newData, guid.ToString());
                    // newData를 Asset 폴더에 저장함 (ScriptableObject)
                    AssetDatabase.CreateAsset(newData, $"Assets/Resources/{dataType.Name}/{dataType.Name.ToUpper()}_{guid}.asset");

                    database.Add(newData);
                    // database에서 data를 추가했으니(= Serialize 변수인 datas 변수에 변화가 생김)
                    // SetDirty를 설정하여 Unity에 database의 Serialize 변수가 변했다고 알림
                    EditorUtility.SetDirty(database);
                    // Dirty flag 대상을 저장함
                    AssetDatabase.SaveAssets();

                    // 현재 보고 있는 IdentifiedObject는 새로 만들어진 IdentifiedObject로 설정
                    selectedObjectsByType[dataType] = newData;
                }

                // 지금부터 그릴 GUI는 빨간색
                GUI.color = Color.red;
                // 마지막 순번의 Data를 삭제하는 Button을 그려줌
                if (GUILayout.Button($"Remove Last {dataType.Name}"))
                {
                    var lastData = database.Count > 0 ? database.Datas.Last() : null;
                    if (lastData)
                    {
                        database.Remove(lastData);

                        // Data의 Asset 폴더 내 위치를 찾아와서 삭제
                        AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(lastData));
                        // database에서 data를 제거했으니 SetDirty를 설정하여 Unity에 database에 변화가 생겼다고 알림
                        EditorUtility.SetDirty(database);
                        AssetDatabase.SaveAssets();
                    }
                }

                // 지금부터 그릴 GUI는 Cyan
                GUI.color = Color.yellow;
                // Data를 이름 순으로 정렬하는 Button을 그림
                if (GUILayout.Button($"Sort By Name"))
                {
                    // 정렬 실행
                    database.SortByCodeName();
                    // database의 data들의 순서가 바뀌었으니 SetDirty를 설정하여 Unity에 database에 변화가 생겼다고 알림
                    EditorUtility.SetDirty(database);
                    AssetDatabase.SaveAssets();
                }
                // 지금부터 그릴 GUI는 하얀색(=원래색)
                GUI.color = Color.white;

                EditorGUILayout.Space(2f);
                CustomEditorUtility.DrawUnderline();
                EditorGUILayout.Space(4f);

                // 지금부터 Scroll 가능한 Box를 그림, UI 중 ScrollView와 동일함
                // 첫번째 인자는 현재 Scroll Posiiton
                // 두번째 인자는 수평 Scroll 막대를 그릴 것인가?, 세번째 인자는 항상 수직 Scroll 막대를 그릴 것인가?
                // 네번째 인자는 항상 수평 Scroll 막대의 Style, 다섯번째 인자는 수직 Scroll 막대의 Style
                // none을 넘겨주게되면 해당 막대는 아예 없애버림
                // 여섯번째 인자는 Background Style
                // return 값은 사용자의 조작에 의해 바뀌게된 Scroll Posiiton
                // ScrollView의 크기는 위에서 BeginVertical 함수에 넣은 넓이 300과 동일함
                // BeginScrollView는 여러 Overloading이 있기 때문에 그냥 현재 Scroll Position만 넣어도 ScrollView가 만들어짐
                // 여기서는 수평 막대를 쓰지 않으려고 인자가 많은 함수를 씀

                scrollPositionsByType[dataType] = EditorGUILayout.BeginScrollView(scrollPositionsByType[dataType], false, true,
                    GUIStyle.none, GUI.skin.verticalScrollbar, GUIStyle.none);
                {
                    // Database의 목록을 그림
                    foreach (var data in database.Datas)
                    {
                        // CodeName을 그려줄 넓이을 정함, 만약 Icon이 존재한다면 Icon의 크기를 고려하며 좁은 넓이를 가짐
                        float labelWidth = data.Icon != null ? 200f : 245f;

                        // 현재 Data가 유저가 선택한 Data면 selectedBoxStyle(=배경이 청색)을 가져옴
                        var style = selectedObjectsByType[dataType] == data ? selectedBoxStyle : GUIStyle.none;
                        // (3) 수평 정렬 시작
                        EditorGUILayout.BeginHorizontal(style, GUILayout.Height(listHeight));
                        {
                            // Data에 Icon이 있다면 40x40 사이즈로 그려줌
                            if (data.Icon)
                            {
                                // Icon의 Preview Texture를 가져옴.
                                // 한번 가져온 Texture는 Unity 내부에 Caching되며, 
                                // Cache된 Texture 수가 위에서 설정한 TextureCacheSize에 도달하면 오래된 Texture부터 지워짐
                                var preview = AssetPreview.GetAssetPreview(data.Icon);
                                GUILayout.Label(preview, GUILayout.Height(40f), GUILayout.Width(40f));
                            }
                            // Data의 CodeName을 그려줌
                            EditorGUILayout.LabelField(data.CodeName, GUILayout.Width(labelWidth), GUILayout.Height(40f));
                            // (4) 수직 정렬 시작, 이건 그려줄 Labe을 중앙 정렬을 하기 위해서임
                            EditorGUILayout.BeginVertical();
                            {
                                // 현재 수직 정렬을 시작한 상태기 때문에 위에서 10칸을 띄우게됨
                                EditorGUILayout.Space(10f);

                                GUI.color = Color.red;
                                // data를 삭제할 수 있는 X Button을 그림
                                if (GUILayout.Button("x", GUILayout.Width(20f)))
                                {
                                    database.Remove(data);
                                    // data의 Asset 폴더 내 위치를 찾아와서 삭제
                                    AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(data));
                                    EditorUtility.SetDirty(database);
                                    AssetDatabase.SaveAssets();
                                }
                            }
                            // (4) 수직 정렬 종료
                            EditorGUILayout.EndVertical();

                            GUI.color = Color.white;
                        }
                        // (3) 수평 정렬 D종료
                        EditorGUILayout.EndHorizontal();

                        // data가 삭제되었다면 즉시 Database 목록을 그리는걸 멈추고 빠져나옴
                        if (data == null)
                            break;

                        // 마지막으로 그린 GUI의 좌표와 크기를 가져옴
                        // 이 경우 바로 위에 그린 GUI의 좌표와 사이즈임(=BeginHorizontal)
                        var lastRect = GUILayoutUtility.GetLastRect();

                        // MosueDown Event고 mosuePosition이 GUI안에 있다면(=Click) Data를 선택한 것으로 처리함
                        if (Event.current.type == EventType.MouseDown && lastRect.Contains(Event.current.mousePosition))
                        {
                            selectedObjectsByType[dataType] = data;
                            drawingEditorScrollPosition = Vector2.zero;
                            // Event에 대한 처리를 했다고 Unity에 알림
                            Event.current.Use();
                        }

                        // 선택된 상태이며 키 다운이 눌러진 경우
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

                                #region 위치에 따른 스크롤 뷰 위치 변화

                                // 스크롤 위치 초기화
                                scrollPositionsByType[dataType] = CalculateNewScrollPosition(scrollPositionsByType[dataType], database.Datas, selectedObjectsByType[dataType], (visibleTotalHeight - 30));
                                #endregion

                                // 이벤트 처리를 완료했다고 알림
                                Event.current.Use();
                            }
                        }
                    }
                }
                // ScrollView 종료
                EditorGUILayout.EndScrollView();
                if (GUILayoutUtility.GetLastRect().height > 1)
                {
                    visibleTotalHeight = GUILayoutUtility.GetLastRect().height;
                }
            }
            // (2) 수직 정렬 종료
            EditorGUILayout.EndVertical();

            // 선택된 Data가 존재한다면 해당 Data의 Editor를 그려줌
            if (selectedObjectsByType[dataType])
            {
                // ScrollView를 그림, 이번에는 Scroll Position 정보만 넘겨줘서 수직, 수평 막대 다 있는 일반적인 ScrollView를 그림
                // 단, always 옵션이 없으므로 수직, 수평 막대는 Scroll이 가능한 상태일 때만 나타남
                drawingEditorScrollPosition = EditorGUILayout.BeginScrollView(drawingEditorScrollPosition);
                {
                    EditorGUILayout.Space(2f);
                    // 첫번째 인자는 Editor를 만들 Target
                    // 두번째 인자는 Target의 Type
                    // 따로 넣어주지 않으면 Target의 기본 Type이 적용됨
                    // 세번째는 내부에서 만들어진 Editor를 담을 Editor 변수
                    // CreateCachedEditor는 내부에서 만들어야할 Editor와 cachedEditor가 같다면
                    // Editor를 새로 만들지 않고 그냥 cachedEditor를 그대로 반환함
                    // 만약 내부에서 만들어야할 Editor와 cachedEditor가 다르다면
                    // cachedEditor를 Destroy하고 새로 만든 Editor를 넣음
                    Editor.CreateCachedEditor(selectedObjectsByType[dataType], null, ref cachedEditor);
                    // Editor를 그려줌
                    cachedEditor.OnInspectorGUI();
                }
                // ScrollView 종료
                EditorGUILayout.EndScrollView();
            }
        }
        // (1) 수평 정렬 종료
        EditorGUILayout.EndHorizontal();
    }
    #endregion

    #region 선택한 데이터의 위치에 따라 스크롤 뷰 이동
    private Vector2 CalculateNewScrollPosition(Vector2 currentScrollPosition, IReadOnlyList<IdentifiedObject> datas, IdentifiedObject selectedData, float viewHeight)
    {
        float currentYPosition = 0;
        int index = 0;
        float dataHeight = 0.0f;
        float endPosition = 0.0f;
        float newScrollPosition = 0.0f;

        // 하단으로 내려갈때에는 이미 보이는 (int)(visibleTotalHeight / listHeight)개의 대한 인덱스 만큼 빼준다.
        // 상단으로 올라갈때에는 이미 하단에 (int)(visibleTotalHeight / listHeight)개가 보이므로 인덱스를 빼주지 않는다.

        int count = (int)(visibleTotalHeight / listHeight);

        foreach (var data in datas)
        {
            index++;
            dataHeight = GetDataHeight(data); // 데이터 높이 계산
            if (data == selectedData)
            {
                #region 수정전
                //// 선택된 데이터의 상단이 스크롤 뷰 아래에 있는 경우
                //if (currentYPosition < currentScrollPosition.y)
                //{
                //    return new Vector2(currentScrollPosition.x, currentYPosition);
                //}
                //// 선택된 데이터의 하단이 스크롤 뷰 위에 있는 경우
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
                // 선택된 데이터가 스크롤 뷰의 하단 경계보다 아래에 있는 경우
                if (endPosition + (index - count) * 2 > currentScrollPosition.y + viewHeight)
                {
                    newScrollPosition = endPosition - viewHeight; // 데이터의 하단이 스크롤 뷰의 하단과 일치하도록 조정

                    if(index > count)
                        newScrollPosition += (index - count) * 2;

                    // 새 스크롤 위치가 현재 위치보다 큰 경우만 업데이트 (불필요한 스크롤 위치 이동 방지)
                    if (newScrollPosition > currentScrollPosition.y)
                    {
                        return new Vector2(currentScrollPosition.x, newScrollPosition);
                    }
                }
                // 선택된 데이터가 스크롤 뷰의 상단 경계보다 위에 있는 경우
                else if (currentYPosition + index * 2 < currentScrollPosition.y)
                {
                    currentYPosition += index * 2;
                    return new Vector2(currentScrollPosition.x, currentYPosition);
                }
                break; // 선택된 데이터에 대한 처리가 완료되면 반복 종료
            }
            currentYPosition += dataHeight;
        }

        return currentScrollPosition; // 변경이 없는 경우 현재 스크롤 위치 반환
    }

    private float GetDataHeight(IdentifiedObject data)
    {
        return listHeight; // 스크롤을 생성할 때 리스트아이템의 높이를 40으로 고정시켰음
    }
    #endregion
}