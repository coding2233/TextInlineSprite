using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

namespace Wanderer.EmojiText
{
    [CustomEditor(typeof(InlineText), true)]
    [CanEditMultipleObjects]
    public class TextEditor : GraphicEditor
    {
        #region 属性
        private InlineText _inlineText;
        private string _lastText;
        SerializedProperty _text;
        SerializedProperty m_Text;
        SerializedProperty m_FontData;
        GUIContent _inputGUIContent;
        GUIContent _outputGUIContent;
        #endregion
        protected override void OnEnable()
        {
            base.OnEnable();
            _lastText = "";
            _inputGUIContent = new GUIContent("Input Text");
            _outputGUIContent = new GUIContent("Output Text");

            _text = serializedObject.FindProperty("_text");
            m_Text = serializedObject.FindProperty("m_Text");
            m_FontData = serializedObject.FindProperty("m_FontData");

            _inlineText = target as InlineText;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(_text, _inputGUIContent);
            EditorGUILayout.PropertyField(m_Text, _outputGUIContent);
            EditorGUILayout.PropertyField(m_FontData);
            AppearanceControlsGUI();
            RaycastControlsGUI();
            serializedObject.ApplyModifiedProperties();
            //更新字符
            if (_inlineText != null && _lastText != _text.stringValue)
            {
                _inlineText.text = _text.stringValue;
                _lastText = _text.stringValue;
            }
        }
    }

}