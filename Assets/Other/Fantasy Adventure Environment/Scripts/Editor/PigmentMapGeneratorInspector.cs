// Fantasy Adventure Environment
// Copyright Staggart Creations
// staggart.xyz

using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditor.Callbacks;
using FAE;
using System;

namespace FAE
{
    [CustomEditor(typeof(PigmentMapGenerator))]
    public class PigmentMapGeneratorInspector : Editor
    {
        PigmentMapGenerator pmg;

        new SerializedObject serializedObject;

        SerializedProperty isMultiTerrain;
        SerializedProperty isMegaSplat;

        SerializedProperty terrainObjects;

        SerializedProperty useCustomPigmentMap;
        SerializedProperty inputPigmentMap;

        // Use this for initialization
        void OnEnable()
        {
            pmg = (PigmentMapGenerator)target;



            GetProperties();
        }

        private void GetProperties()
        {
            serializedObject = new SerializedObject(pmg);

            isMultiTerrain = serializedObject.FindProperty("isMultiTerrain");
            isMegaSplat = serializedObject.FindProperty("isMegaSplat");
            terrainObjects = serializedObject.FindProperty("terrainObjects");

            useCustomPigmentMap = serializedObject.FindProperty("useCustomPigmentMap");
            inputPigmentMap = serializedObject.FindProperty("inputPigmentMap");

        }

        //Meta
        private string terrainInfo;
        private bool showHelp;

        public override void OnInspectorGUI()
        {
            DoHeader();

            EditorGUI.BeginChangeCheck();

            serializedObject.Update();

            DrawTerrainInfo();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Custom pigment map");
            EditorGUILayout.PropertyField(useCustomPigmentMap, new GUIContent(""));
            EditorGUILayout.EndHorizontal();
            if (showHelp) EditorGUILayout.HelpBox("This option allows you to assign a custom pigment map, rather than rendering one.", MessageType.Info);

            EditorGUILayout.Space();

            //Custom pigment map
            if (useCustomPigmentMap.boolValue)
            {
                EditorGUILayout.PropertyField(inputPigmentMap, new GUIContent("Input"));

                if (showHelp) EditorGUILayout.HelpBox("Grass heightmap should be stored in the alpha channel.", MessageType.Info);

                if (pmg.inputPigmentMap)
                {
                    //Check if input heightmap is readable
                    try
                    {
                        pmg.inputPigmentMap.GetPixel(0, 0);
                    }
                    catch (UnityException e)
                    {
                        if (e.Message.StartsWith("Texture '" + pmg.inputPigmentMap.name + "' is not readable"))
                        {

                            EditorGUILayout.HelpBox("Please enable Read/Write on texture \"" + pmg.inputPigmentMap.name + "\"\n\nWith the texture selected, choose the \"Advanced\" texture type to show this option.", MessageType.Error);
                        }
                    }
                }

                EditorGUILayout.Space();

            }

            DoTerrainList();

            EditorGUILayout.Space();

            if (!useCustomPigmentMap.boolValue)
            {
                //Safety check, required for preventing errors when updating from 1.1.2 to 1.2.0
                //if (pmg.terrains == null || pmg.terrains.Length == 0) return;

                //If single terrain without any textures
                if (!isMegaSplat.boolValue)
                {
                    if (!isMultiTerrain.boolValue && pmg.workflow == TerrainUVUtil.Workflow.Terrain && pmg.terrains[0].terrainData.splatPrototypes.Length == 0)
                    {
                        EditorGUILayout.HelpBox("Assign at least one texture to your terrain", MessageType.Error);
                        return;
                    }
                }

                DoHeightMap();

                DoTexTransforms();

                DoRenderer();
            }

            EditorGUILayout.Space();

            //Button
            //If multi terrain, with no objects assigned, don't show Generate button
            if (isMultiTerrain.boolValue && terrainObjects.arraySize == 0)
            {
                EditorGUILayout.HelpBox("Assign at least one terrain object", MessageType.Error);
                terrainObjects.isExpanded = true;
            }

            EditorGUI.BeginDisabledGroup(isMultiTerrain.boolValue && terrainObjects.arraySize == 0);
            string buttonLabel = (useCustomPigmentMap.boolValue) ? "Assign" : "Generate";
            if (GUILayout.Button(buttonLabel, GUILayout.Height(40f)))
            {
                pmg.Generate();
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.Space();

            DoPreview();

            GUIHelper.DrawFooter();

            serializedObject.ApplyModifiedProperties();

            if (GUI.changed || EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty((PigmentMapGenerator)target);
                EditorUtility.SetDirty(this);

            }
        }

        private void DoHeader()
        {

            EditorGUILayout.BeginHorizontal();
            showHelp = GUILayout.Toggle(showHelp, "Toggle help", "Button");
            GUILayout.Label("FAE Pigmentmap Generator", GUIHelper.Header);
            EditorGUILayout.EndHorizontal();
            if (showHelp) EditorGUILayout.HelpBox("This renders a color map from your terrain, which is used by the \"FAE/Grass\" shader to blend the grass color with the terrain.", MessageType.Info);
            if (showHelp) EditorGUILayout.HelpBox("If you'd like some objects to be included, like cliffs, parent them under your terrain object.", MessageType.Info);
            if (showHelp) EditorGUILayout.HelpBox("For a multi-terrain setup, add this script to the parent object of your terrain tiles/chunks.", MessageType.Info);

            EditorGUILayout.Space();
        }

        private void DrawTerrainInfo()
        {
                terrainInfo = string.Format("Size: {0}x{1}x{2} \nCenter: x: {3} z: {4} \nPivot: x: {5} z: {6}",
                    Mathf.Round(pmg.targetSize.x),
                    Mathf.Round(pmg.targetSize.y),
                    Mathf.Round(pmg.targetSize.z),
                    Mathf.Round(pmg.targetCenterPosition.x),
                    Mathf.Round(pmg.targetCenterPosition.z),
                    Mathf.Round(pmg.terrainScaleOffset.z),
                    Mathf.Round(pmg.terrainScaleOffset.w)
                 );

                EditorGUILayout.HelpBox(terrainInfo, MessageType.Info);
        }

        private void DoTerrainList()
        {
            if (isMultiTerrain.boolValue)
            {
                EditorGUILayout.PropertyField(terrainObjects, new GUIContent("Terrain objects"), true);

                if (terrainObjects.isExpanded)
                {
                    EditorGUILayout.HelpBox("The first object in the array must be the corner chunk, where the terrain continues on the postive X and Z axis", MessageType.Warning);

                    if (GUILayout.Button("Assign all child objects"))
                    {
                        pmg.GetChildTerrainObjects(pmg.transform);
                    }
                }
            }
            else
            {
                //Single terrain, don't show array    
            }
        }

        private void DoHeightMap()
        {

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            //If is single terrain
            if (!isMultiTerrain.boolValue && pmg.workflow == TerrainUVUtil.Workflow.Terrain && !isMegaSplat.boolValue)
            {
                EditorGUILayout.LabelField("Grass heightmap", EditorStyles.boldLabel);
                EditorGUILayout.Space();

                pmg.heightmapChannel = (PigmentMapGenerator.HeightmapChannel)EditorGUILayout.EnumPopup("Height source material", pmg.heightmapChannel);
                if (showHelp) EditorGUILayout.HelpBox("This is the texture whose painted weight will determine the grass height \n\nThe effect can be controlled through the \"Heightmap influence\" parameter on the FAE/Grass shader", MessageType.None);

                if (pmg.terrains[0].terrainData.alphamapResolution >= 2048 && pmg.heightmapChannel > 0)
                {
                    EditorGUILayout.HelpBox("Your splatmap size is 2048px or larger, this can cause severe slowdowns", MessageType.Warning);
                }

            }
            //If mesh or multi terrain
            else
            {
                //Field to assign heightmap texture
                EditorGUILayout.LabelField("Input grass heightmap (optional)", EditorStyles.toolbarButton);
                EditorGUILayout.Space();

                pmg.inputHeightmap = EditorGUILayout.ObjectField("Heightmap", pmg.inputHeightmap, typeof(Texture2D), false) as Texture2D;

                if (showHelp) EditorGUILayout.HelpBox("This information is used in the \"FAE/Grass\" shader to make the grass shorter where desired", MessageType.Info);

                if (pmg.inputHeightmap)
                {
                    //Check if input heightmap is readable
                    try
                    {
                        pmg.inputHeightmap.GetPixel(0, 0);
                    }
                    catch (UnityException e)
                    {
                        if (e.Message.StartsWith("Texture '" + pmg.inputHeightmap.name + "' is not readable"))
                        {

                            EditorGUILayout.HelpBox("Please enable Read/Write on texture \"" + pmg.inputHeightmap.name + "\"", MessageType.Error);
                        }
                    }
                }

            }

            EditorGUILayout.Space();

            EditorGUILayout.EndVertical();
        }

        private void DoTexTransforms()
        {
            if (pmg.workflow == TerrainUVUtil.Workflow.Terrain) return;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField("Texture transformation", EditorStyles.toolbarButton);
            EditorGUILayout.Space();
            if (showHelp) EditorGUILayout.HelpBox("The UV's of your mesh terrain may differ, so these options allow you to compensate for this.", MessageType.Info);

            pmg.flipHortizontally = EditorGUILayout.Toggle("Flip horizontally", pmg.flipHortizontally);
            pmg.flipVertically = EditorGUILayout.Toggle("Flip vertically", pmg.flipVertically);
            pmg.textureRotation = (PigmentMapGenerator.TextureRotation)EditorGUILayout.EnumPopup("Rotation", pmg.textureRotation);

            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();
        }

        private void DoRenderer()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField("Renderer", EditorStyles.toolbarButton);
            EditorGUILayout.Space();

            pmg.useAlternativeRenderer = EditorGUILayout.ToggleLeft("Using third-party terrain shader", pmg.useAlternativeRenderer);

            if (showHelp) EditorGUILayout.HelpBox("Some third-party terrain shaders require you to use this, otherwise the result may be black.", MessageType.Info);

            if (pmg.useAlternativeRenderer) pmg.renderLightBrightness = EditorGUILayout.Slider("Brightness adjustment", pmg.renderLightBrightness, 0f, 1f);
            if (pmg.useAlternativeRenderer && showHelp) EditorGUILayout.HelpBox("To compensate for any shader variations on the terrain, you can use this to increase the brightness of the pigment map, in case it turns out too dark/bright.", MessageType.Info);

            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();
        }

        private void DoPreview()
        {
            if (!useCustomPigmentMap.boolValue)
            {
                //Pigment map preview
                if (!pmg.pigmentMap) return;

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(string.Format("Output pigment map ({0}x{0})", pmg.pigmentMap.height), EditorStyles.boldLabel);
                EditorGUILayout.EndHorizontal();

                Texture nothing = EditorGUILayout.ObjectField(pmg.pigmentMap, typeof(Texture), false, GUILayout.Width(150f), GUILayout.Height(150f)) as Texture;
                nothing.name = "Pigmentmap";


                //Single terrain
                if (!isMultiTerrain.boolValue)
                {
                    if (pmg.workflow == TerrainUVUtil.Workflow.Terrain)
                    {
                        if (pmg.hasTerrainData)
                        {
                            EditorGUILayout.LabelField("The output texture file is stored next to the TerrainData asset", EditorStyles.helpBox);
                        }
                        else
                        {
                            EditorGUILayout.LabelField("The output texture file is stored next to the scene file", EditorStyles.helpBox);
                        }
                    }
                    else if (pmg.workflow == TerrainUVUtil.Workflow.Mesh)
                    {
                        EditorGUILayout.LabelField("The output texture file is stored next to the material file", EditorStyles.helpBox);
                    }
                }
                //Multi terrain
                else
                {
                    if (pmg.workflow == TerrainUVUtil.Workflow.Terrain)
                    {
                        EditorGUILayout.LabelField("The output texture file is stored next to the scene file", EditorStyles.helpBox);
                    }
                    else if (pmg.workflow == TerrainUVUtil.Workflow.Mesh)
                    {
                        EditorGUILayout.LabelField("The output texture file is stored next to the material file", EditorStyles.helpBox);
                    }
                }
            }
        }

    }
}
