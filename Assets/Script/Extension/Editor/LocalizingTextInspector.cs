using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro.EditorUtilities;
using ALF;

[CustomEditor(typeof(LocalizingText), true), CanEditMultipleObjects]
public class LocalizingTextInspector : TMP_EditorPanelUI
{
    static readonly GUIContent k_LocalizingLabel = new GUIContent("Localizing", "텍스트 로컬라이징 설정.");
    static readonly GUIContent k_LocalizingKeyLabel = new GUIContent("Key", "로컬라이징 ID");

    protected SerializedProperty m_IsLocalizingProp;
    protected SerializedProperty m_LocalizingKeyProp;
    protected LocalizingText m_LocalizingComponent;

    protected bool m_LocalizingChanged = false;

    protected override void OnEnable()
    {
        base.OnEnable();
        m_LocalizingComponent = (LocalizingText)target;
        m_LocalizingKeyProp = serializedObject.FindProperty("Key");
        m_IsLocalizingProp = serializedObject.FindProperty("IsLocalizing");
        
    }
    override public void OnInspectorGUI()
    {
        DrawLocalizingInfo();

        base.OnInspectorGUI();
    }

    void DrawLocalizingInfo()
    {
        EditorGUILayout.Space();

        EditorGUI.BeginChangeCheck();

        EditorGUILayout.PropertyField(m_IsLocalizingProp, k_LocalizingLabel);

        
        if (EditorGUI.EndChangeCheck() && m_IsLocalizingProp.boolValue != m_LocalizingComponent.IsLocalizing)
        {
            m_LocalizingComponent.IsLocalizing = m_IsLocalizingProp.boolValue;
            if(!m_LocalizingComponent.IsLocalizing)
            {
                m_LocalizingComponent.Key = null;
            }
        }

        if(m_LocalizingComponent.IsLocalizing)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_LocalizingKeyProp, k_LocalizingKeyLabel);

            // Need to also compare string content due to issue related to scroll bar drag handle
            if (EditorGUI.EndChangeCheck() && m_LocalizingKeyProp.stringValue != m_LocalizingComponent.Key)
            {
                m_LocalizingComponent.Key = m_LocalizingKeyProp.stringValue;
            }
        }
    
        EditorGUILayout.Space();
    }

}
