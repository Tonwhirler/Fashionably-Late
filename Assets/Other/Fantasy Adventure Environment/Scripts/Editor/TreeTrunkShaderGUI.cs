// Fantasy Adventure Environment
// Copyright Staggart Creations
// staggart.xyz

using UnityEngine;
using UnityEditor;
using System.Collections;

namespace FAE
{
    public class TreeTrunkShaderGUI : ShaderGUI
    {

        //Main maps
        MaterialProperty _MainTex;
        MaterialProperty _BumpMap;

        MaterialProperty _UseLegacyVertexColors;
        MaterialProperty _UseSpeedTreeWind;

        //Color
        MaterialProperty _AmbientOcclusion;
        MaterialProperty _GradientBrightness;

        MaterialEditor m_MaterialEditor;

        //Meta
        bool showHelp;

        GUIContent mainTexName = new GUIContent("Diffuse", "Diffuse (RGB) and Transparency (A)");
        GUIContent normalMapName = new GUIContent("Normal Map");

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
        {
            this.FindProperties(props);

            this.m_MaterialEditor = materialEditor;

            //Style similar to Standard shader
            m_MaterialEditor.SetDefaultGUIWidths();
            m_MaterialEditor.UseDefaultMargins();
            EditorGUIUtility.labelWidth = 0f;

            EditorGUI.BeginChangeCheck();

            //Draw fields
            DoHeader();

            DoMapsArea();
            DoColorArea();

            if (EditorGUI.EndChangeCheck())
            {
                //Apply changes
            }

            GUILayout.Label("Advanced Options", EditorStyles.boldLabel);

            GUIHelper.DrawExtraFields(m_MaterialEditor);

            m_MaterialEditor.ShaderProperty(_UseSpeedTreeWind, new GUIContent("Sample SpeedTree wind"));
            if (showHelp) EditorGUILayout.HelpBox("If this is a tree created using the SpeedTree Unity Modeler, this toggle will make the shader read the wind information as stored by SpeedTree.", MessageType.None);

            EditorGUI.BeginDisabledGroup(_UseSpeedTreeWind.floatValue == 1);
            m_MaterialEditor.ShaderProperty(_UseLegacyVertexColors, new GUIContent("Legacy vertex color layout"));
            if (_UseSpeedTreeWind.floatValue == 0) EditorGUILayout.HelpBox("Since version 1.2.1 the Blue and Alpha channel have been swapped. If your custom tree uses the old layout, check this.", MessageType.None);
            EditorGUI.EndDisabledGroup();

            if(showHelp) GUIHelper.DrawWindInfo();

            GUIHelper.DrawFooter();

        }

        void DoHeader()
        {
            EditorGUILayout.BeginHorizontal();
            showHelp = GUILayout.Toggle(showHelp, "Toggle help", "Button");
            GUILayout.Label("FAE Tree Trunk Shader", GUIHelper.Header);
            EditorGUILayout.EndHorizontal();
            if (showHelp) EditorGUILayout.HelpBox("Tree trunk shader, featuring global wind motion", MessageType.Info);
        }

        void DoMapsArea()
        {
            GUILayout.Label("Main maps", EditorStyles.boldLabel);
            this.m_MaterialEditor.TexturePropertySingleLine(mainTexName, this._MainTex);
            this.m_MaterialEditor.TexturePropertySingleLine(normalMapName, this._BumpMap);

            EditorGUILayout.Space();
        }

        void DoColorArea()
        {
            m_MaterialEditor.ShaderProperty(_AmbientOcclusion, _AmbientOcclusion.displayName);
            if (showHelp) EditorGUILayout.HelpBox("Darkens the areas of the mesh where red vertex colors are applied", MessageType.None);
            m_MaterialEditor.ShaderProperty(_GradientBrightness, _GradientBrightness.displayName);
            if (showHelp) EditorGUILayout.HelpBox("Adds a gradient to the branch mesh from bottom to top. This information is stored in the alpha vertex color channel.\n\nWithout this information, the parameter will have no effect.", MessageType.None);

            EditorGUILayout.Space();
        }


        public void FindProperties(MaterialProperty[] props)
        {
            _UseLegacyVertexColors = FindProperty("_UseLegacyVertexColors", props);
            _UseSpeedTreeWind = FindProperty("_UseSpeedTreeWind", props);

            //Main maps
            _MainTex = FindProperty("_MainTex", props);
            _BumpMap = FindProperty("_BumpMap", props);

            //Color
            _AmbientOcclusion = FindProperty("_AmbientOcclusion", props);
            _GradientBrightness = FindProperty("_GradientBrightness", props);

        }

    }
}
