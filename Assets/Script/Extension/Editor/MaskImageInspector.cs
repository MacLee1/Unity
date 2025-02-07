using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UI;

[CustomEditor(typeof(MaskImage), true), CanEditMultipleObjects]

public class MaskImageInspector : ImageEditor
{
    // SerializedProperty m_MaskBorderProp;
    // new SerializedProperty m_Material;
    MaskImage m_MaskImageComponent;

    protected override void OnEnable()
    {
        base.OnEnable();
        m_MaskImageComponent = (MaskImage)target;

        // m_MaskBorderProp = serializedObject.FindProperty("maskBorder");
    }

    override public void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        // EditorGUILayout.PropertyField(m_MaskBorderProp);
        Vector4 border = EditorGUILayout.Vector4Field("Border:", m_MaskImageComponent.maskBorder);

        // if (EditorGUI.EndChangeCheck() && m_MaskImageComponent.maskBorder.Equals(m_MaskBorderProp.vector4Value))
        // {
        //     m_MaskImageComponent.maskBorder = m_MaskBorderProp.vector4Value;
        // }
        EditorGUILayout.Space();
        base.OnInspectorGUI();

        
        // EditorGUILayout.Space();
        
        // m_MaskBorderProp.vector4Value = EditorGUILayout.Vector4Field("Border:", m_MaskBorderProp.vector4Value);
        
        if (EditorGUI.EndChangeCheck() && !border.Equals(m_MaskImageComponent.maskBorder))
        {
            m_MaskImageComponent.maskBorder = border;
            m_MaskImageComponent.SetVerticesDirty();
        }

        
    
        //RaycastControlsGUI();
        // SetShowNativeSize(false);
        // NativeSizeButtonGUI();
          
        
        
        // serializedObject.ApplyModifiedProperties();
    }
}
