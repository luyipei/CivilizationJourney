using UnityEngine;
using UnityEditor;

namespace CivilizationJourney.Dialogue.Editor
{
    /// <summary>
    /// DialogueData的自定义Inspector
    /// 提供快捷操作按钮
    /// </summary>
    [CustomEditor(typeof(DialogueData))]
    public class DialogueDataEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DialogueData dialogue = (DialogueData)target;

            // 标题
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("对话数据", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            // 快捷信息
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField($"标题: {dialogue.dialogueTitle}");
            EditorGUILayout.LabelField($"场景数: {dialogue.scenes.Count}");
            EditorGUILayout.LabelField($"对话总数: {dialogue.TotalLineCount}");
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            // 打开编辑器按钮
            if (GUILayout.Button("在对话编辑器中打开", GUILayout.Height(30)))
            {
                DialogueEditorWindow.ShowWindow();
                Selection.activeObject = dialogue;
            }

            EditorGUILayout.Space(10);

            // 默认Inspector
            DrawDefaultInspector();
        }
    }

    /// <summary>
    /// CharacterData的自定义Inspector
    /// </summary>
    [CustomEditor(typeof(CharacterData))]
    public class CharacterDataEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            CharacterData character = (CharacterData)target;

            // 标题
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("角色数据", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            // 立绘预览
            if (character.defaultPortrait != null)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("默认立绘预览:");
                var rect = GUILayoutUtility.GetRect(150, 200);
                rect.x = rect.x + (rect.width - 150) / 2;
                rect.width = 150;
                GUI.DrawTexture(rect, character.defaultPortrait.texture, ScaleMode.ScaleToFit);
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Space(10);

            // 默认Inspector
            DrawDefaultInspector();
        }
    }
}
