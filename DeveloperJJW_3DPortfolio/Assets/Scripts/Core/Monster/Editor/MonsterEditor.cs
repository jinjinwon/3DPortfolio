using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;
using static UnityEditor.Rendering.FilterWindow;
using UnityEditorInternal;

[CustomEditor(typeof(Monster))]
public class MonsterEditor : IdentifiedObjectEditor
{
    private SerializedProperty categoryProperty;
    private SerializedProperty typeProperty;
    private SerializedProperty attackClipProperty;
    private SerializedProperty deathClipProperty;
    private SerializedProperty patternProperty;
    private SerializedProperty prefabProperty;
    private SerializedProperty colliderCenterProperty;
    private SerializedProperty animatorOverrideControllerProperty;
    private SerializedProperty colliderRadiusProperty;
    private SerializedProperty colliderHeightProperty;
    private SerializedProperty attackRangeProperty;
    private SerializedProperty statOverrideProperty;
    private SerializedProperty isbossProperty;
    private SerializedProperty skillProperty;
    private SerializedProperty appearActionProperty;
    private SerializedProperty dgDeadActionProperty;

    private ReorderableList statOverridesList;

    protected override void OnEnable()
    {
        base.OnEnable();

        categoryProperty = serializedObject.FindProperty("category");
        typeProperty = serializedObject.FindProperty("type");
        attackClipProperty = serializedObject.FindProperty("attackClip");
        deathClipProperty = serializedObject.FindProperty("deathClip");
        patternProperty = serializedObject.FindProperty("pattern");
        prefabProperty = serializedObject.FindProperty("prefab");
        animatorOverrideControllerProperty = serializedObject.FindProperty("animatorOverrideController");
        colliderCenterProperty = serializedObject.FindProperty("center");
        colliderRadiusProperty = serializedObject.FindProperty("radius");
        colliderHeightProperty = serializedObject.FindProperty("height");
        attackRangeProperty = serializedObject.FindProperty("attackRange");
        isbossProperty = serializedObject.FindProperty("bossMonster");
        statOverrideProperty = serializedObject.FindProperty("statOverrides");
        skillProperty = serializedObject.FindProperty("skills");
        appearActionProperty = serializedObject.FindProperty("customActionsOnAppear");
        dgDeadActionProperty = serializedObject.FindProperty("dgActionsOnDead");

        #region 몬스터 스텟
        statOverridesList = new ReorderableList(serializedObject, statOverrideProperty, true, true, true, true)
        {
            drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "Stat Overrides");
            },
            drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = statOverrideProperty.GetArrayElementAtIndex(index);
                rect.y += 2;

                var statRect = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(statRect, element.FindPropertyRelative("stat"), GUIContent.none);

                var isUseOverrideRect = new Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight + 2, rect.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(isUseOverrideRect, element.FindPropertyRelative("isUseOverride"), new GUIContent("Use Override"));

                if (element.FindPropertyRelative("isUseOverride").boolValue)
                {
                    var overrideDefaultValueRect = new Rect(rect.x, rect.y + (EditorGUIUtility.singleLineHeight + 2) * 2, rect.width, EditorGUIUtility.singleLineHeight);
                    EditorGUI.PropertyField(overrideDefaultValueRect, element.FindPropertyRelative("overrideDefaultValue"), new GUIContent("Override Default Value"));
                }
            },
            elementHeightCallback = (int index) =>
            {
                var element = statOverrideProperty.GetArrayElementAtIndex(index);
                var baseHeight = EditorGUIUtility.singleLineHeight + 4;
                if (element.FindPropertyRelative("isUseOverride").boolValue)
                {
                    baseHeight += EditorGUIUtility.singleLineHeight + 2;
                }
                return baseHeight + EditorGUIUtility.singleLineHeight + 2;
            },
            onAddCallback = (ReorderableList list) =>
            {
                statOverrideProperty.arraySize++;
                var newElement = statOverrideProperty.GetArrayElementAtIndex(statOverrideProperty.arraySize - 1);
                newElement.boxedValue = new StatOverride(null);
            }
        };
        #endregion
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.Update();

        float prevLabelWidth = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 220f;

        DrawSettings();

        EditorGUIUtility.labelWidth = prevLabelWidth;

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawSettings()
    {
        if (!DrawFoldoutTitle("Setting"))
            return;

        EditorGUILayout.PropertyField(categoryProperty);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("몬스터 타입 및 패턴", EditorStyles.boldLabel);
        CustomEditorUtility.DrawUnderline();

        CustomEditorUtility.DrawEnumToolbar(typeProperty);
        CustomEditorUtility.DrawEnumToolbar(patternProperty);

        EditorGUILayout.PropertyField(attackClipProperty);
        EditorGUILayout.PropertyField(deathClipProperty);

        EditorGUILayout.PropertyField(animatorOverrideControllerProperty);
        EditorGUILayout.PropertyField(prefabProperty);
        EditorGUILayout.PropertyField(attackRangeProperty);
        EditorGUILayout.PropertyField(isbossProperty);

        EditorGUILayout.PropertyField(colliderCenterProperty);
        EditorGUILayout.PropertyField(colliderRadiusProperty);
        EditorGUILayout.PropertyField(colliderHeightProperty);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("몬스터 스펙", EditorStyles.boldLabel);
        CustomEditorUtility.DrawUnderline();
        statOverridesList.DoLayoutList();

        EditorGUILayout.PropertyField(skillProperty);

        EditorGUILayout.PropertyField(appearActionProperty, new GUIContent("CustomActionsOnAppear"), true);
        EditorGUILayout.PropertyField(dgDeadActionProperty, new GUIContent("DGActionsOnDead"), true);
    }
}
