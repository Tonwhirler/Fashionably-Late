// Fantasy Adventure Environment
// Copyright Staggart Creations
// staggart.xyz

using UnityEngine;
using System.Collections;
using System.IO;
using System;

namespace FAE
{
    using System.Collections.Generic;
#if UNITY_EDITOR
    using UnityEditor;
    using UnityEditor.SceneManagement;
    [ExecuteInEditMode]
#endif

    public class PigmentMapGenerator : MonoBehaviour
    {
        //Dev
        public bool debug = false;
        public bool performCleanup = true;

        //Terrain objects
        public GameObject[] terrainObjects;

        //Terrain utils
        public TerrainUVUtil util;
        public TerrainUVUtil.Workflow workflow;

        private int pigmentmapSize = 1024;
        public Vector3 targetSize;
        public Vector3 targetOriginPosition;
        public Vector3 targetCenterPosition;

        //Runtime
        [SerializeField]
        public Vector4 terrainScaleOffset;

        //Terrain terrain
        public Terrain[] terrains;

        //Mesh terrain
        private MeshRenderer[] meshes;
        private Material material;

        #region Rendering
        //Constants
        const int HEIGHTOFFSET = 1000;
        const int CLIP_PADDING = 10;

        //Render options
        public float renderLightBrightness = 0.25f;
        public bool useAlternativeRenderer = false;

        //Rendering
        private Camera renderCam;
        private Light renderLight;
        private Light[] lights;
        #endregion

        #region Inputs
        //Inputs 
        public Texture2D inputHeightmap;
        public Texture2D inputPigmentMap;
        public bool useCustomPigmentMap;

        //Texture options
        public bool flipVertically;
        public bool flipHortizontally;

        public enum TextureRotation
        {
            None,
            Quarter,
            Half,
            ThreeQuarters
        }
        public TextureRotation textureRotation;
        #endregion

        //Textures
        public Texture2D pigmentMap;
        private Texture2D newPigmentMap;
        private Texture2D heightmap;
        private Texture2D HeightmapChannelTexture;

        //Meta
        public bool isMultiTerrain;
        public string savePath;
        private float originalTargetYPos;

        //MegaSplat
        public bool hasTerrainData = true;
        public bool isMegaSplat = false;

        //Reset lighting settings
        UnityEngine.Rendering.AmbientMode ambientMode;
        Color ambientColor;
        bool enableFog;

        public enum HeightmapChannel
        {
            None,
            Slot1,
            Slot2,
            Slot3,
            Slot4,
            Slot5,
            Slot6,
            Slot7,
            Slot8
        }
        public HeightmapChannel heightmapChannel = HeightmapChannel.None;

        //Used at runtime
        private void OnEnable()
        {
            Init();
        }

        private void OnDisable()
        {
            //This is to avoid the pigment map remaining in the shader
            Shader.SetGlobalTexture("_PigmentMap", null);
        }


        private void OnDrawGizmosSelected()
        {
            if (debug)
            {
                Color32 color = new Color(0f, 0.66f, 1f, 0.25f);
                Gizmos.color = color;
                Gizmos.DrawCube(targetCenterPosition, targetSize);
                color = new Color(0f, 0.66f, 1f, 1f);
                Gizmos.color = color;
                Gizmos.DrawWireCube(targetCenterPosition, targetSize);
            }
        }

        public void Init()
        {
#if UNITY_EDITOR

            CheckMegaSplat();

            if (GetComponent<Terrain>() || GetComponent<MeshRenderer>())
            {
                isMultiTerrain = false;
                //Single terrain, use first element
                terrainObjects = new GameObject[1];
                terrainObjects[0] = this.gameObject;

            }
            else
            {
                isMultiTerrain = true;
                //Init array
                if (terrainObjects == null) terrainObjects = new GameObject[0];
            }

            //Create initial pigment map
            if (pigmentMap == null)
            {
                Generate();
            }

#endif
            SetPigmentMap();

        }

        private void CheckMegaSplat()
        {
#if __MEGASPLAT__
            if(workflow == TerrainUVUtil.Workflow.Terrain)
            {
                if (terrains[0].materialType == Terrain.MaterialType.Custom)
                {
                    if (terrains[0].materialTemplate.shader.name.Contains("MegaSplat"))
                    {
                        isMegaSplat = true;
                        useAlternativeRenderer = true;
                    }
                    else
                    {
                        isMegaSplat = false;
                    }
                }
            }
#else
                isMegaSplat = false;
#endif
        }

        public void GetChildTerrainObjects(Transform parent)
        {
            //All childs, recursive
            Transform[] children = parent.GetComponentsInChildren<Transform>();

            int childCount = 0;

            //Count first level transforms
            for (int i = 0; i < children.Length; i++)
            {
                if (children[i].parent == parent) childCount++;
            }

            //Temp list
            List<GameObject> terrainList = new List<GameObject>();

            //Init array with childcount length
            this.terrainObjects = new GameObject[childCount];

            //Fill array with first level transforms
            for (int i = 0; i < children.Length; i++)
            {
                if (children[i].parent == parent)
                {
                    terrainList.Add(children[i].gameObject);
                }
            }

            terrainObjects = terrainList.ToArray();
        }

        //Grab the terrain position and size and pass it to the shaders
        public void GetTargetInfo()
        {
            if (debug) Debug.Log("Getting target info for " + terrainObjects.Length + " object(s)");

            if (!util) util = ScriptableObject.CreateInstance<TerrainUVUtil>();


            util.GetObjectPlanarUV(terrainObjects);

            //Terrain UV
            terrainScaleOffset = util.terrainScaleOffset;

            //Determine if the object is a terrain or mesh
            workflow = util.workflow;
            if (debug) Debug.Log("Terrain type: " + workflow);

            //If using Unity Terrains
            terrains = util.terrains;

            //Avoid unused variable warning
            material = null;

            //based on first terrain's splatmap resolution, or hardcoded to 1024px for meshes
            pigmentmapSize = util.pigmentMapSize;

            //Summed size
            targetSize = util.size;

            //First terrain makes up the corner
            targetOriginPosition = util.originPosition;

            //Center of terrain(s)
            targetCenterPosition = util.centerPostion;

            SetPigmentMap();
        }

        //Set the pigmentmap texture on all shaders that utilize it
        public void SetPigmentMap()
        {
            if (pigmentMap)
            {
                //Set this at runtime to account for different instances having different pigment maps
                Shader.SetGlobalVector("_TerrainUV", terrainScaleOffset);
                Shader.SetGlobalTexture("_PigmentMap", pigmentMap);
            }
        }

        //Editor functions
#if UNITY_EDITOR
        //Primary function
        public void Generate()
        {
            if (terrainObjects.Length == 0) return;

            GetTargetInfo();

            //If a custom map is assigned, don't generate one, only assign
            if (useCustomPigmentMap)
            {
                pigmentMap = inputPigmentMap;
                SetPigmentMap();
                return;
            }

            LightSetup();

            CameraSetup();

            RenderToTexture();

            if (performCleanup) Cleanup();

            ResetLights();

        }

        //Position a camera above the terrain(s) so that the world positions line up perfectly with the texture UV
        public void CameraSetup()
        {
            //Create camera
            if (!renderCam)
            {
                renderCam = new GameObject().AddComponent<Camera>();
            }
            renderCam.name = this.name + " renderCam";

            //Set up a square camera rect
            float rectWidth = pigmentmapSize;
            rectWidth /= Screen.width;
            renderCam.rect = new Rect(0, 0, rectWidth, 1);

            //Camera set up
            renderCam.orthographic = true;
            renderCam.orthographicSize = (targetSize.x / 2);

            renderCam.farClipPlane = 5000f;
            renderCam.useOcclusionCulling = false;

            //Rendering in Forward mode is a tad darker, so a Directional Light is used to make up for the difference
            renderCam.renderingPath = (useAlternativeRenderer || workflow == TerrainUVUtil.Workflow.Mesh) ? RenderingPath.Forward : RenderingPath.VertexLit;

            //Camera position
            float camPosX = targetCenterPosition.x;
            float camPosZ = targetCenterPosition.z;

            if (workflow == TerrainUVUtil.Workflow.Terrain)
            {
                //Hide tree objects
                foreach (Terrain terrain in terrains)
                {
                    terrain.drawTreesAndFoliage = false;
                }
            }

            //Position cam in given center of terrain
            renderCam.transform.position = new Vector3(camPosX, targetOriginPosition.y + targetSize.y + HEIGHTOFFSET + CLIP_PADDING, camPosZ);

            //Rotate camera
            int camRotation = -(int)textureRotation * 90;

            renderCam.transform.localEulerAngles = new Vector3(90, camRotation, 0);

            //Store terrain position value, to revert to
            //Safe to assum all terrains transforms have the same height, usually the case
            originalTargetYPos = targetOriginPosition.y;

            //Move terrain object way up so it's rendered on top of all other objects
            foreach (GameObject terrain in terrainObjects)
            {
                terrain.transform.position = new Vector3(terrain.transform.position.x, HEIGHTOFFSET, terrain.transform.position.z);
            }

        }

        private void RenderToTexture()
        {
            if (!renderCam) return;

            pigmentMap = null;

            //If this is terrain with no textures, abort
            if (workflow == TerrainUVUtil.Workflow.Terrain && terrains[0].terrainData.splatPrototypes.Length == 0 && !isMegaSplat) return;

            //Set up render texture
            RenderTexture rt = new RenderTexture(pigmentmapSize, pigmentmapSize, 24);
            renderCam.targetTexture = rt;

            savePath = GetTargetFolder();

            EditorUtility.DisplayProgressBar("PigmentMapGenerator", "Rendering texture", 1);
            //Render camera into a texture
            Texture2D render = new Texture2D(pigmentmapSize, pigmentmapSize, TextureFormat.ARGB32, false);
            renderCam.Render();
            RenderTexture.active = rt;
            render.ReadPixels(new Rect(0, 0, pigmentmapSize, pigmentmapSize), 0, 0);

            //If a channel is chosen, add heightmap to the pigment map's alpha channel
            if (workflow == TerrainUVUtil.Workflow.Terrain)
            {
                if ((int)heightmapChannel > 0)
                {
                    EditorUtility.DisplayProgressBar("PigmentMapGenerator", "Adding heightmap", 1);
                    render = AddHeightmapToAlpha(render);
                }
            }
            if (workflow == TerrainUVUtil.Workflow.Mesh)
            {
                if (inputHeightmap != null)
                {
                    EditorUtility.DisplayProgressBar("PigmentMapGenerator", "Adding heightmap", 1);
                    render = AddHeightmapToAlpha(render, inputHeightmap);
                }
                else
                {
                    //Debug.Log("No heightmap assigned");
                }

                //Transformations
                if (flipHortizontally)
                {
                    EditorUtility.DisplayProgressBar("PigmentMapGenerator", "Flipping horizontally", 1);
                    render = FlipTextureHorizontally(render);
                }
                if (flipVertically)
                {
                    EditorUtility.DisplayProgressBar("PigmentMapGenerator", "Flipping vertically", 1);

                    render = FlipTextureVertically(render);
                }

                int rotation = 0;

                switch (textureRotation)
                {
                    case TextureRotation.Quarter:
                        rotation = 90;
                        break;
                    case TextureRotation.Half:
                        rotation = 180;
                        break;
                    case TextureRotation.ThreeQuarters:
                        rotation = 270;
                        break;
                }

                if (rotation != 0)
                {
                    //render = RotateTexture(rotation);
                }

            }

            //Cleanup
            renderCam.targetTexture = null;
            RenderTexture.active = null;
            DestroyImmediate(rt);

            //Encode
            byte[] bytes = render.EncodeToPNG();

            //Create file
            EditorUtility.DisplayProgressBar("PigmentMapGenerator", "Saving texture...", 1);
            File.WriteAllBytes(savePath, bytes);

            //Import file
            AssetDatabase.Refresh();

            //Load the file
            pigmentMap = new Texture2D(pigmentmapSize, pigmentmapSize, TextureFormat.ARGB32, true);
            pigmentMap = AssetDatabase.LoadAssetAtPath(savePath, typeof(Texture2D)) as Texture2D;

            //Pass it to all shaders utilizing the global texture parameter
            Shader.SetGlobalTexture("_PigmentMap", pigmentMap);

            EditorUtility.ClearProgressBar();

        }

        //Store pigment map next to TerrainData asset, or mesh's material
        private string GetTargetFolder()
        {
            string m_targetPath = null;

            //Compose target file path

            //For single terrain
            if (terrainObjects.Length == 1)
            {
                if (workflow == TerrainUVUtil.Workflow.Terrain)
                {
                    //If there is a TerraData asset, use its file location
                    if (terrains[0].terrainData.name != string.Empty)
                    {
                        hasTerrainData = true;
                        m_targetPath = AssetDatabase.GetAssetPath(terrains[0].terrainData) + string.Format("{0}_pigmentmap.png", terrains[0].terrainData.name);
                        m_targetPath = m_targetPath.Replace(terrains[0].terrainData.name + ".asset", string.Empty);
                    }
                    //If there is no TerrainData, store it next to the scene. Some terrain systems don't use TerrainData
                    else
                    {
                        hasTerrainData = false;
                        string scenePath = EditorSceneManager.GetActiveScene().path.Replace(".unity", string.Empty);
                        m_targetPath = scenePath + "_pigmentmap.png";
                    }
                }
                //If the target is a mesh, use the location of its material
                else if (workflow == TerrainUVUtil.Workflow.Mesh)
                {
                    material = terrainObjects[0].GetComponent<MeshRenderer>().sharedMaterial;
                    m_targetPath = AssetDatabase.GetAssetPath(material) + string.Format("{0}_pigmentmap.png", string.Empty);
                    m_targetPath = m_targetPath.Replace(".mat", string.Empty);
                }
            }
            //For multi-terrain, use scene folder or material
            else
            {
                if (workflow == TerrainUVUtil.Workflow.Mesh)
                {
                    material = terrainObjects[0].GetComponent<MeshRenderer>().sharedMaterial;
                    m_targetPath = AssetDatabase.GetAssetPath(material) + string.Format("{0}_pigmentmap.png", string.Empty);
                    m_targetPath = m_targetPath.Replace(".mat", string.Empty);
                }
                else
                {
                    string scenePath = EditorSceneManager.GetActiveScene().path.Replace(".unity", string.Empty);
                    m_targetPath = scenePath + "_pigmentmap.png";

                }
            }

            return m_targetPath;
        }

        //Add the height info to the pigment map's alpha channel
        private Texture2D AddHeightmapToAlpha(Texture2D pigmentMap, Texture2D inputHeightmap = null)
        {

            int spatmapIndex = ((int)heightmapChannel >= 5) ? 1 : 0;
            int channelIndex = (spatmapIndex > 0) ? (int)heightmapChannel - 4 : (int)heightmapChannel;

            //Debug.Log("Splatmap index: " + spatmapIndex);
            //Debug.Log("Channel index: " + channelIndex);

            heightmap = new Texture2D(pigmentmapSize, pigmentmapSize, TextureFormat.RGB24, false);

            if (workflow == TerrainUVUtil.Workflow.Terrain)
            {
                Texture2D splatmap = terrains[0].terrainData.alphamapTextures[spatmapIndex];

                //Use the selected channel from the splatmap to create a heightmap
                for (int x = 0; x < pigmentmapSize; x++)
                {
                    for (int y = 0; y < pigmentmapSize; y++)
                    {
                        Color splatmapPixel = splatmap.GetPixel(x, y);

                        switch (channelIndex)
                        {
                            //Red
                            case 1:
                                heightmap.SetPixel(x, y, Color.red * splatmapPixel.r);
                                break;
                            //Green
                            case 2:
                                heightmap.SetPixel(x, y, Color.red * splatmapPixel.g);
                                break;
                            //Blue
                            case 3:
                                heightmap.SetPixel(x, y, Color.red * splatmapPixel.b);
                                break;
                            //Alpha
                            case 4:
                                heightmap.SetPixel(x, y, Color.red * splatmapPixel.a);
                                break;
                        }
                    }

                    heightmap.Apply();
                }
            }
            else if (workflow == TerrainUVUtil.Workflow.Mesh)
            {
                //If the input heightmap is of a lower/higher resolution, rescale it
                if (inputHeightmap.height != pigmentmapSize)
                {
                    inputHeightmap = ScaleTexture(inputHeightmap, pigmentmapSize, pigmentmapSize);
                }

                for (int x = 0; x < pigmentmapSize; x++)
                {
                    for (int y = 0; y < pigmentmapSize; y++)
                    {
                        Color heightmapPixel = inputHeightmap.GetPixel(x, y);

                        heightmap.SetPixel(x, y, Color.red * heightmapPixel.r);
                    }

                    heightmap.Apply();
                }
            }

            //Create a new pigment map texture
            if (newPigmentMap)
            {
                DestroyImmediate(newPigmentMap);
            }
            newPigmentMap = new Texture2D(pigmentmapSize, pigmentmapSize, TextureFormat.ARGB32, false);

            //Store the heightmap in the alpha channel
            for (int x = 0; x < pigmentmapSize; x++)
            {
                for (int y = 0; y < pigmentmapSize; y++)
                {
                    float heightPixel = heightmap.GetPixel(x, y).r;
                    Color pigmentmapPixel = pigmentMap.GetPixel(x, y);

                    newPigmentMap.SetPixel(x, y, new Color(pigmentmapPixel.r, pigmentmapPixel.g, pigmentmapPixel.b, heightPixel));
                }
            }
            newPigmentMap.Apply();

            return newPigmentMap;
        }

        //Slow!!! TODO: Do this on GPU instead
        private Texture2D FlipTextureVertically(Texture2D inputTex)
        {
            Texture2D flippedPigmentmap = new Texture2D(pigmentmapSize, pigmentmapSize, TextureFormat.RGB24, false);

            for (int i = 0; i < pigmentmapSize; i++)
            {
                for (int j = 0; j < pigmentmapSize; j++)
                {
                    flippedPigmentmap.SetPixel(i, pigmentmapSize - j - 1, inputTex.GetPixel(i, j));
                }
            }

            flippedPigmentmap.Apply();

            return flippedPigmentmap;
        }

        private Texture2D FlipTextureHorizontally(Texture2D inputTex)
        {
            Texture2D flippedPigmentmap = new Texture2D(pigmentmapSize, pigmentmapSize, TextureFormat.RGB24, false);

            for (int i = 0; i < pigmentmapSize; i++)
            {
                for (int j = 0; j < pigmentmapSize; j++)
                {
                    flippedPigmentmap.SetPixel(pigmentmapSize - i - 1, j, inputTex.GetPixel(i, j));
                }
            }

            flippedPigmentmap.Apply();

            return flippedPigmentmap;
        }

        //Rescale function by user petersvp
        public static Texture2D ScaleTexture(Texture2D src, int width, int height)
        {
            Rect texRect = new Rect(0, 0, width, height);

            //We need the source texture in VRAM because we render with it
            src.filterMode = FilterMode.Trilinear;
            src.Apply(true);

            RenderTexture rt = new RenderTexture(width, height, 32);

            Graphics.SetRenderTarget(rt);

            //Setup 2D matrix in range 0..1, so nobody needs to care about sized
            GL.LoadPixelMatrix(0, 1, 1, 0);

            //Then clear & draw the texture to fill the entire RTT.
            GL.Clear(true, true, new Color(0, 0, 0, 0));
            Graphics.DrawTexture(new Rect(0, 0, 1, 1), src);

            //Get rendered data back to a new texture
            Texture2D result = new Texture2D(width, height, TextureFormat.ARGB32, true);
            result.Resize(width, height);
            result.ReadPixels(texRect, 0, 0, true);
            result.Apply();
            return result;
        }

        void Cleanup()
        {
            DestroyImmediate(renderCam.gameObject);

            if (renderLight) DestroyImmediate(renderLight.gameObject);

            //Reset terrains
            foreach (GameObject terrain in terrainObjects)
            {
                //Reset terrain position(s)
                terrain.transform.position = new Vector3(terrain.transform.position.x, originalTargetYPos, terrain.transform.position.z);
            }

            //Reset draw foliage
            if (workflow == TerrainUVUtil.Workflow.Terrain)
            {
                foreach (Terrain terrain in terrains)
                {
                    terrain.drawTreesAndFoliage = true;
                }
            }

            renderCam = null;
            renderLight = null;

        }

        //Disable directional light and set ambient color to white for an albedo result
        void LightSetup()
        {

            //Set up lighting for a proper albedo color
            lights = FindObjectsOfType<Light>();
            foreach (Light light in lights)
            {
                if (light.type == LightType.Directional)
                    light.gameObject.SetActive(false);
            }

            //Store current settings to revert to
            ambientMode = RenderSettings.ambientMode;
            ambientColor = RenderSettings.ambientLight;
            enableFog = RenderSettings.fog;

            //Flat lighting 
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = Color.white;
            RenderSettings.fog = false;

            //To account for Forward rendering being slightly darker, add a light
            if (useAlternativeRenderer)
            {
                if (!renderLight) renderLight = new GameObject().AddComponent<Light>();
                renderLight.name = "renderLight";
                renderLight.type = LightType.Directional;
                renderLight.transform.localEulerAngles = new Vector3(90, 0, 0);
                renderLight.intensity = renderLightBrightness;
            }

        }

        //Re-enable directional light and reset ambient mode
        void ResetLights()
        {
            foreach (Light light in lights)
            {
                if (light.type == LightType.Directional)
                    light.gameObject.SetActive(true);
            }

            RenderSettings.ambientMode = ambientMode;
            RenderSettings.ambientLight = ambientColor;
            RenderSettings.fog = enableFog;

        }
#endif
    }
}