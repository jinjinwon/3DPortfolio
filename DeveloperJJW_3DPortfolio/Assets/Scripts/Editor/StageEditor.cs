using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;
using static UnityEditor.Rendering.FilterWindow;

[CustomEditor(typeof(Stage))]
public class StageEditor : IdentifiedObjectEditor
{
    private SerializedProperty maxFloorProperty;
    private SerializedProperty stageDatasProperty;
    private SerializedProperty isAllowFloorExceedDatasProperty;
    private SerializedProperty defaultFloorProperty;

    // Toolbar Button들의 이름
    private readonly string[] customActionsToolbarList = new[] { "NextFloor"};
    // Skill Data마다 선택한 Toolbar Button의 Index 값
    private Dictionary<int, int> customActionToolbarIndexesByLevel = new();

    protected override void OnEnable()
    {
        base.OnEnable();

        maxFloorProperty = serializedObject.FindProperty("maxFloor");
        defaultFloorProperty = serializedObject.FindProperty("defaultFloor");
        isAllowFloorExceedDatasProperty = serializedObject.FindProperty("isAllowFloorExceedDatas");
        stageDatasProperty = serializedObject.FindProperty("stageDatas");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.Update();

        float prevLabelWidth = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 220f;

        DrawSkillDatas();

        EditorGUIUtility.labelWidth = prevLabelWidth;

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawSkillDatas()
    {
        // Skill의 Data가 아무것도 존재하지 않으면 1개를 자동적으로 만들어줌
        if (stageDatasProperty.arraySize == 0)
        {
            // 배열 길이를 늘려서 새로운 Element를 생성
            stageDatasProperty.arraySize++;
            // 추가한 Data의 Level을 1로 설정
            stageDatasProperty.GetArrayElementAtIndex(0).FindPropertyRelative("floor").intValue = 1;
        }

        if (!DrawFoldoutTitle("Data"))
            return;

        EditorGUILayout.PropertyField(isAllowFloorExceedDatasProperty);

        // Level 상한 제한이 없다면 MaxLevel을 그대로 그려주고,
        // 상한 제한이 있다면 MaxLevel을 상한으로 고정 시키는 작업을 함
        if (isAllowFloorExceedDatasProperty.boolValue)
            EditorGUILayout.PropertyField(maxFloorProperty);
        else
        {
            // Property를 수정하지 못하게 GUI Enable의 false로 바꿈
            GUI.enabled = false;
            var lastIndex = stageDatasProperty.arraySize - 1;
            // 마지막 SkillData(= 가장 높은 Level의 Data)를 가져옴
            var lastSkillData = stageDatasProperty.GetArrayElementAtIndex(lastIndex);
            // maxFloor을 마지막 Data의 Floor로 고정
            maxFloorProperty.intValue = lastSkillData.FindPropertyRelative("floor").intValue;
            // maxFloor Property를 그려줌
            EditorGUILayout.PropertyField(maxFloorProperty);
            GUI.enabled = true;
        }

        EditorGUILayout.PropertyField(defaultFloorProperty);

        for (int i = 0; i < stageDatasProperty.arraySize; i++)
        {
            var property = stageDatasProperty.GetArrayElementAtIndex(i);
            SerializedProperty bossRoundProperty = property.FindPropertyRelative("isBossRound");
            bool isBossRound = bossRoundProperty != null && bossRoundProperty.propertyType == SerializedPropertyType.Boolean && bossRoundProperty.boolValue;

            EditorGUILayout.BeginVertical("HelpBox");
            {
                // Data의 Level과 Data 삭제를 위한 X Button을 그려주는 Foldout Title을 그려줌
                // 단, 첫번째 Data(= index 0) 지우면 안되기 때문에 X Button을 그려주지 않음
                // X Button을 눌러서 Data가 지워지면 true를 return함
                if (DrawRemovableLevelFoldout_Floor(stageDatasProperty, property, i, i != 0))
                {
                    // Data가 삭제되었으며 더 이상 GUI를 그리지 않고 바로 빠져나감
                    // 다음 Frame에 처음부터 다시 그리기 위함
                    EditorGUILayout.EndVertical();
                    break;
                }

                EditorGUI.indentLevel += 1;

                if (property.isExpanded)
                {
                    // SkillData Property 내부로 들어감 -> Property == level field;
                    property.NextVisible(true);

                    DrawAutoSortLevelProperty(stageDatasProperty, property, i, i != 0);

                    // Level Up
                    for (int j = 0; j < 8; j++)
                    {
                        property.NextVisible(false);

                        if (property.name == "bossMonsterGen" && !isBossRound)
                        {
                            continue; // 조건을 만족하면 필드를 그리지 않음
                        }

                        if (property.name == "bossmonster" && !isBossRound)
                        {
                            continue; // 조건을 만족하면 필드를 그리지 않음
                        }
                        EditorGUILayout.PropertyField(property);
                    }

                    // Custom Action - UnderlineTitle
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("NextFloor Action", EditorStyles.boldLabel);
                    CustomEditorUtility.DrawUnderline();

                    // Custom Action - Toolbar
                    // 한번에 모든 Array 변수를 다 그리면 보기 번잡하니 Toolbar를 통해 보여줄 Array를 선택할 수 있게함.
                    var customActionToolbarIndex = customActionToolbarIndexesByLevel.ContainsKey(i) ? customActionToolbarIndexesByLevel[i] : 0;
                    // Toolbar는 자동 들여쓰기(EditorGUI.indentLevel)이 먹히지 않아서 직접 들여쓰기를 해줌
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Space(12);
                        customActionToolbarIndex = GUILayout.Toolbar(customActionToolbarIndex, customActionsToolbarList);
                        customActionToolbarIndexesByLevel[i] = customActionToolbarIndex;
                    }
                    GUILayout.EndHorizontal();

                    property.NextVisible(false);
                    EditorGUILayout.PropertyField(property);
                }
                EditorGUI.indentLevel -= 1;
            }
            EditorGUILayout.EndVertical();
        }

        if (GUILayout.Button("Add New Floor"))
        {
            // Level Change
            var lastArraySize = stageDatasProperty.arraySize++;
            var prevElementalProperty = stageDatasProperty.GetArrayElementAtIndex(lastArraySize - 1);
            var newElementProperty = stageDatasProperty.GetArrayElementAtIndex(lastArraySize);
            var newElementLevel = prevElementalProperty.FindPropertyRelative("floor").intValue + 1;
            newElementProperty.FindPropertyRelative("floor").intValue = newElementLevel;
            newElementProperty.isExpanded = true;

            // 몬스터는 그때 그때 설정하도록 
            CustomEditorUtility.DeepCopySerializeReferenceArray(newElementProperty.FindPropertyRelative("customActionsFade"));
            //CustomEditorUtility.DeepCopySerializeReferenceArray(newElementProperty.FindPropertyRelative("monsters"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
