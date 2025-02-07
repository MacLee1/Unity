using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UI;

[CustomEditor(typeof(EmblemBake), true), CanEditMultipleObjects]

public class EmblemBakeInspector : RawImageEditor
{
    //[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        // [PerRendererData] _PatternTex ("Pattern Texture", 2D) = "white" {}
        // [PerRendererData] _SimbolTex ("Simbol Texture", 2D) = "white" {}

    SerializedProperty m_BaseMaskProp;
    SerializedProperty m_PatternProp;
    SerializedProperty m_SimbolProp;
    SerializedProperty m_SimbolColorProp;
    SerializedProperty m_PatternColor1Prop;
    SerializedProperty m_PatternColor2Prop;
    // new SerializedProperty m_Material;
    EmblemBake m_EmblemBakeComponent;

    protected override void OnEnable()
    {
        base.OnEnable();
        m_EmblemBakeComponent = (EmblemBake)target;

        m_BaseMaskProp = serializedObject.FindProperty("baseMaskTexture");
        m_PatternProp = serializedObject.FindProperty("patternTexture");
        m_SimbolProp = serializedObject.FindProperty("simbolTexture");
        m_SimbolColorProp = serializedObject.FindProperty("simbolColor");
        m_PatternColor1Prop = serializedObject.FindProperty("pattern1Color");
        m_PatternColor2Prop = serializedObject.FindProperty("pattern2Color");
        m_Material = serializedObject.FindProperty("m_Material");
    }

    override public void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.Space();
        
        EditorGUILayout.PropertyField(m_BaseMaskProp);
        EditorGUILayout.PropertyField(m_PatternProp);
        EditorGUILayout.PropertyField(m_PatternColor1Prop);
        EditorGUILayout.PropertyField(m_PatternColor2Prop);
        EditorGUILayout.PropertyField(m_SimbolProp);
        EditorGUILayout.PropertyField(m_SimbolColorProp);

        EditorGUILayout.PropertyField(m_Material);
        
        RaycastControlsGUI();
        // SetShowNativeSize(false);
        // NativeSizeButtonGUI();
          
        // if (EditorGUI.EndChangeCheck() && m_EmblemBakeComponent.material != null && m_EmblemBakeComponent.baseMaskTexture != null && m_EmblemBakeComponent.patternTexture != null && m_EmblemBakeComponent.simbolTexture != null)
        if (EditorGUI.EndChangeCheck() && m_EmblemBakeComponent.baseMaskTexture != null && m_EmblemBakeComponent.patternTexture != null && m_EmblemBakeComponent.simbolTexture != null)
        {
            m_EmblemBakeComponent.Bake();
        }
        serializedObject.ApplyModifiedProperties();
    }

    // void SetShowNativeSize(bool instant)
    // {
    //     base.SetShowNativeSize(m_BaseMaskProp.objectReferenceValue != null, instant);
    // }
}
