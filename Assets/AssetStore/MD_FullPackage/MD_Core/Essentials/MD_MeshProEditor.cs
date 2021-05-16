using System.Collections.Generic;
using UnityEngine;

namespace MD_Plugin
{
    /// <summary>
    /// MD(Mesh Deformation) Essential Component: Mesh Pro Editor
    /// Essential base component for general mesh processing
    /// </summary>
    [ExecuteInEditMode]
    [AddComponentMenu(MD_Debug.ORGANISATION + MD_Debug.PACKAGENAME + "Mesh Pro Editor")]
    public class MD_MeshProEditor : MonoBehaviour 
    {
        //Essential Params
        public enum SelectedModification { None, Vertices, Collider, Identity, Mesh };
        public SelectedModification ppSelectedModification;

        public bool ppNewReferenceAfterCopy = true;
        public bool ppUpdateEveryFrame = true;
        public bool ppAnimationMode = false;
        public bool ppOptimizeMesh = false;

        #region Vertices Editor Variables
        public Transform ppVerticesRoot;
        public bool ppCustomVerticePattern = false;
        public bool ppUseCustomColor = true;
        public Color ppCustomVerticeColor = Color.red;
        public GameObject ppVerticePatternObject;
        #endregion

        #region Basic Mesh Info Variables
        public string ppINFO_MeshName;
        public int ppINFO_Vertices = 0;
        public int ppINFO_Triangles = 0;
        public int ppINFO_Normals = 0;
        public int ppINFO_Uvs = 0;
        //---Stored Original Mesh
        [SerializeField] public Vector3[] originalVertices;
        [SerializeField] public int[] originalTriangles;
        [SerializeField] public Vector3[] originalNormals;
        [SerializeField] public Vector2[] originalUVS;
        #endregion

        //Vertices & points for editing
        public Vector3[] workingVertices;
        public List<Transform> workingTargetPoints = new List<Transform>();

        //Default material
        private Material ppDefaultMaterial;
        //-------------------------------------------------------------------------------------
        private bool ppDeselectObjectAfterVerticeLimit;

        public bool _AlreadyAwake = false;
        public bool _BornAsSkinnedMesh = false;

        public MeshFilter meshFilter;

        //Zone Generator utility
        public bool ppEnableZoneGenerator = false;
        public Vector3 ppZoneGenerator;
        public float ppZoneGeneratorRadius = 0.5f;

        public Vector3 myStartupBounds;

        private void Awake () 
        {
            meshFilter = GetComponent<MeshFilter>();
            if(!meshFilter)
                return;
            if (!meshFilter.sharedMesh)
            {
                MD_Debug.Debug(this, "Mesh Filter doesn't contain any mesh data. The behaviour will be destroyed.", MD_Debug.DebugType.Error);
                DestroyImmediate(this);
                return;
            }
            if (!MeshProEditor_Utilities.util_CheckFellowModifiers(this.gameObject, this))
            {
                DestroyImmediate(this);
                return;
            }

            if(ppDefaultMaterial == null)
                ppDefaultMaterial = new Material(Shader.Find("Standard"));

            if (!_AlreadyAwake)
            {
                if (string.IsNullOrEmpty(ppINFO_MeshName))
                    ppINFO_MeshName = "NewMesh" + Random.Range(1, 99999).ToString();

#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    if (MD_GlobalPreferences.popupEditorWindow)
                    {
                        if (UnityEditor.EditorUtility.DisplayDialog("Create a New Reference?", "Would you like to create a new reference? If you agree [recommended], you will create a new mesh reference... If you disagree, exist mesh references will share the same data as the new mesh reference...", "yes", "no"))
                            Internal_MPE_ResetReference();
                    }
                    else if(MD_GlobalPreferences.createNewReference)
                        Internal_MPE_ResetReference();
                }
                else if (!ppAnimationMode && MD_GlobalPreferences.createNewReference)
                    Internal_MPE_ResetReference();
#else
                if (!ppAnimationMode && MD_GlobalPreferences.createNewReference)
                     Internal_MPE_ResetReference();
#endif
                myStartupBounds = meshFilter.sharedMesh.bounds.max;
                Internal_MPE_ReceiveMeshInfo();

                _AlreadyAwake = true;
            }
            else if (ppNewReferenceAfterCopy)
                Internal_MPE_ResetReference();
        }

        /// <summary>
        /// Additional mesh utilities mostly for internal use
        /// </summary>
        public static class MeshProEditor_Utilities
        {
            /// <summary>
            /// Check vertice count limit. If the limitation is passed, the window/ debug will popout
            /// </summary>
            /// <param name="inputVertCount">Input Vertex Count</param>
            /// <returns>Returns true if everything is okay (the limitation wasn't passed), returns false if vertex count is over the limitation</returns>
            public static bool util_CheckVerticeCount(int inputVertCount, GameObject sender = null)
            {
                if (inputVertCount <= MD_GlobalPreferences.vertexLimit) return true;

#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    if(MD_GlobalPreferences.popupEditorWindow)
                        return UnityEditor.EditorUtility.DisplayDialog("Mesh has more than " + MD_GlobalPreferences.vertexLimit.ToString() + " vertices", "Your selected mesh has more than " + MD_GlobalPreferences.vertexLimit.ToString() + " vertices [" + inputVertCount.ToString() + "]. This may slow the editor performance. Would you like to continue?", "Yes", "No");
                    else
                    {
                        Debug.Log($"Mesh '{sender?.name}' has more than recommended vertices count. This may slow down the performance");
                        return true;
                    }
                } 
                else
                {
                    Debug.Log($"Mesh '{sender?.name}' has more than recommended vertices count. This may slow down the performance");
                    return true;
                }
#else
                    Debug.Log($"Mesh '{sender?.name}' has more than recommended vertices count. This may slow down the performance");
                    return true;
#endif
            }

            /// <summary>
            /// Check for other modifiers on the specific object. The object can't contain more than one modifier
            /// </summary>
            /// <param name="sender">Sender gameObject</param>
            /// <param name="senderBehaviour">Sender behaviour</param>
            /// <returns>Returns false if there are any fellow modifiers, returns true if everything is alright</returns>
            public static bool util_CheckFellowModifiers(GameObject sender, MonoBehaviour senderBehaviour)
            {
                foreach (MonoBehaviour beh in sender.GetComponents<MonoBehaviour>())
                {
                    if (beh == null) continue;
                    if (beh.GetType().Name == senderBehaviour.GetType().Name) continue;

                    if (beh.GetType().Name == "AdvancedPlane"
                        || beh.GetType().Name == "AdvancedShape"
                        || beh.GetType().Name.Contains("MDM_"))
                    {
                        #if UNITY_EDITOR
                            if (!Application.isPlaying) UnityEditor.EditorUtility.DisplayDialog("Error", "The modifier cannot be applied to this object, because the object already contains modifiers. Please, remove exists modifier to access selected modifier...", "OK");
                        #else
                            Debug.Log("The object contains another modifier, which is prohibited. The modifier will be destroyed.");
                        #endif
                        return false;
                    }
                }
                return true;
            }

            /// <summary>
            /// Create brand new mesh reference, returns mesh
            /// </summary>
            /// <param name="entryMesh">Mesh entry</param>
            /// <returns>Returns brand new mesh reference with all refreshed attributes</returns>
            public static Mesh util_CreateNewMeshReference(Mesh entryMesh, bool recalculateBounds = true, bool recalculateNormals = true, bool recalculateTangents = true)
            {
                Mesh newMesh = new Mesh();
                newMesh.name = entryMesh.name;
                newMesh.indexFormat = entryMesh.indexFormat;
                newMesh.vertices = entryMesh.vertices;
                newMesh.triangles = entryMesh.triangles;
                newMesh.tangents = entryMesh.tangents;
                newMesh.normals = entryMesh.normals;
                newMesh.bounds = entryMesh.bounds;
                newMesh.uv = entryMesh.uv;
                newMesh.uv2 = entryMesh.uv2;
                newMesh.uv3 = entryMesh.uv3;
                newMesh.uv4 = entryMesh.uv4;
                newMesh.uv5 = entryMesh.uv5;
                newMesh.uv6 = entryMesh.uv6;
                newMesh.uv7 = entryMesh.uv7;
                newMesh.uv8 = entryMesh.uv8;
                newMesh.subMeshCount = entryMesh.subMeshCount;
                newMesh.MarkDynamic();

                if (recalculateTangents)    newMesh.RecalculateTangents();
                if (recalculateNormals)     newMesh.RecalculateNormals();
                if (recalculateBounds)      newMesh.RecalculateBounds();

                return newMesh;
            }

            /// <summary>
            /// Create brand new mesh reference, does some additional work (if shared mesh exists)
            /// </summary>
            public static void util_CreateNewMeshReference(MeshFilter entryMeshFilter, bool recalculateBounds = true, bool recalculateNormals = true, bool recalculateTangents = true)
            {
                if (entryMeshFilter == null)
                {
                    MD_Debug.Debug(null, "Creating a new mesh reference was unsuccessful. The object entry was empty!");
                    return;
                }
                if (entryMeshFilter.sharedMesh == null)
                {
                    MD_Debug.Debug(null, "Creating a new mesh reference of object " + entryMeshFilter.name + " was unsuccessful. The shared mesh was empty!");
                    return;
                }
                entryMeshFilter.sharedMesh = util_CreateNewMeshReference(entryMeshFilter.sharedMesh, recalculateBounds, recalculateNormals, recalculateTangents);
            }


            /// <summary>
            /// Prepare specific modifier for further use
            /// </summary>
            public static void util_PrepareMeshDeformationModifier(MonoBehaviour sender, MeshFilter senderFilter, bool checkVertCount = true)
            {
                if (!util_CheckFellowModifiers(sender.gameObject, sender))
                {
                    DestroyImmediate(sender);
                    return;
                }

                if (!senderFilter.sharedMesh)
                {
                    MD_Debug.Debug(sender, "Mesh Filter doesn't contain any mesh data. The modifier will be destroyed.", MD_Debug.DebugType.Error);
                    DestroyImmediate(sender);
                    return;
                }

                //---This is by default set to true - it's very recommended to create a new mesh reference
                if(MD_GlobalPreferences.createNewReference)
                    util_CreateNewMeshReference(senderFilter, MD_GlobalPreferences.autoRecalcBounds, MD_GlobalPreferences.autoRecalcNormals);
                senderFilter.sharedMesh.MarkDynamic();

                if (!checkVertCount) return;
                if (!util_CheckVerticeCount(senderFilter.sharedMesh.vertices.Length, senderFilter.gameObject))
                {
                    DestroyImmediate(sender);
                    return;
                }
            }
        }

        #region Internal Mesh Editor Methods

        /// <summary>
        /// Reset current mesh reference
        /// </summary>
        private void Internal_MPE_ResetReference()
        {
            if (meshFilter == null) return;

            if (ppSelectedModification == SelectedModification.Vertices)
                MPE_CreateVerticeEditor();
            else
                MPE_ClearVerticeEditor();

            Mesh newMesh = MeshProEditor_Utilities.util_CreateNewMeshReference(meshFilter.sharedMesh, MD_GlobalPreferences.autoRecalcBounds, MD_GlobalPreferences.autoRecalcNormals);
            newMesh.name = ppINFO_MeshName;
            meshFilter.sharedMesh = newMesh;

            Internal_MPE_ReceiveMeshInfo();
        }

       /// <summary>
       /// Refresh current mesh info such as vertex count, triangle count etc
       /// </summary>
        private void Internal_MPE_ReceiveMeshInfo(bool passAlreadyAwake = false)
        {
            if (!meshFilter) return;

            Mesh myMesh = meshFilter.sharedMesh;
            ppINFO_Vertices = myMesh.vertexCount;
            ppINFO_Triangles = myMesh.triangles.Length;
            ppINFO_Normals = myMesh.normals.Length;
            ppINFO_Uvs = myMesh.uv.Length;
            if (!_AlreadyAwake || passAlreadyAwake)
            {
                originalVertices = myMesh.vertices;
                originalTriangles = myMesh.triangles;
                originalNormals = myMesh.normals;
                originalUVS = myMesh.uv;
            }
        }

#if UNITY_EDITOR

        /// <summary>
        /// Save mesh to assets [Editor only]
        /// </summary>
        public void Internal_MPE_SaveMeshToAssetsDatabase()
        {
            if (meshFilter == null || (meshFilter && meshFilter.sharedMesh == null))
            {
                MD_Debug.Debug(this, "The object doesn't contain Mesh Filter or shared mesh is empty", MD_Debug.DebugType.Error);
                return;
            }

            string path = UnityEditor.EditorUtility.SaveFilePanelInProject("Please enter the path to save your Mesh to Assets as Prefab", ppINFO_MeshName, "asset", "Please enter path");

            if (string.IsNullOrEmpty(path))
                return;

            string UniquePath = UnityEditor.AssetDatabase.GenerateUniqueAssetPath(path);
            try
            {
                UnityEditor.AssetDatabase.CreateAsset(meshFilter.sharedMesh, UniquePath);
                UnityEditor.AssetDatabase.SaveAssets();
                UnityEditor.AssetDatabase.Refresh();

                MD_Debug.Debug(this, "Mesh has been successfully saved to: " + path, MD_Debug.DebugType.Information);
            }
            catch (UnityException e)
            {
                Debug.LogError(e.Message + ", [Error while saving asset...] Unique Path: " + UniquePath);
            }
        }

#endif

        #endregion

        private void Update () 
        {
            if (ppUpdateEveryFrame) MPE_UpdateMesh();
        }

        /// <summary>
        /// Update current mesh state
        /// </summary>
        public void MPE_UpdateMesh()
        {
            if (meshFilter == null || (meshFilter && meshFilter.sharedMesh == null))
            {
                if(Application.isPlaying)
                    MD_Debug.Debug(this, "The object doesn't contain Mesh Filter or shared mesh is empty", MD_Debug.DebugType.Error);
                return;
            }

            if (ppVerticesRoot  == null) return;
            if (workingVertices == null) return;

            if (workingVertices.Length == workingTargetPoints.Count)
            {
                transform.localRotation = Quaternion.identity;
                transform.localScale = Vector3.one;

                for (int i = 0; i < workingVertices.Length; i++)
                {
                    if (workingTargetPoints.Count > 0 && workingTargetPoints.Count > i)
                    {
                        if(workingTargetPoints[i] != null)
                            workingVertices[i] = new Vector3(workingTargetPoints[i].position.x - (meshFilter.transform.position.x - Vector3.zero.x), workingTargetPoints[i].position.y - (meshFilter.transform.position.y - Vector3.zero.y), workingTargetPoints[i].position.z - (meshFilter.transform.position.z - Vector3.zero.z));
                    }
                }
                meshFilter.sharedMesh.vertices = workingVertices;
            }

            if (!ppOptimizeMesh)
            {
                meshFilter.sharedMesh.RecalculateNormals();
                meshFilter.sharedMesh.RecalculateBounds();
            }
        }

        #region Public Mesh Editor Methods

        //Mesh Editor Vertices

        /// <summary>
        /// Hide/Show generated points of the mesh
        /// </summary>
        public void MPE_ShowHideVertices(bool Activation)
        {
            if (ppVerticesRoot == null) return;
            foreach (Renderer r in ppVerticesRoot.GetComponentsInChildren<Renderer>())
                if (r != null) r.enabled = Activation;
        }

        /// <summary>
        /// Ignore raycast layer for vertices
        /// </summary>
        public void MPE_IgnoreRaycastVertices(bool IgnoreRaycast)
        {
            if (ppVerticesRoot == null) return;
            foreach (Renderer r in ppVerticesRoot.GetComponentsInChildren<Renderer>())
                if (r != null) r.gameObject.layer = IgnoreRaycast ? 2 : 0;
        }

        /// <summary>
        /// Create vertice editor on the current mesh
        /// </summary>
        /// <param name="PassTheVerticeLimit">Notification box will show up if the vertices limit is greater than the constant [only in Unity Editor]</param>
        public void MPE_CreateVerticeEditor(bool PassTheVerticeLimit = false)
        {
            if (meshFilter == null || (meshFilter && meshFilter.sharedMesh == null))
            {
                MD_Debug.Debug(this, "The object doesn't contain Mesh Filter or shared mesh is empty", MD_Debug.DebugType.Error);
                return;
            }

            //Don't continue if animation mode is enabled
            if (ppAnimationMode) return;

            MPE_ClearVerticeEditor();

            if (meshFilter.sharedMesh.vertexCount > MD_GlobalPreferences.vertexLimit && !PassTheVerticeLimit)
            {
                ppDeselectObjectAfterVerticeLimit = true;
                ppSelectedModification = SelectedModification.Vertices;
                ppEnableZoneGenerator = true;
                ppZoneGenerator = transform.position + Vector3.one;
                return;
            }

            transform.parent = null;

            Vector3 LastScale = transform.localScale;
            Quaternion LastRotation = transform.rotation;

            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;

            GameObject _VertexRoot = new GameObject(name + "_VertexRoot");
            ppVerticesRoot = _VertexRoot.transform;
            _VertexRoot.transform.position = Vector3.zero;

            workingVertices = null;
            workingTargetPoints.Clear();

            //---Generating Points & Vertices
            var vertices = meshFilter.sharedMesh.vertices;
            for (int i = 0; i < vertices.Length; i++)
            {
                GameObject gm = null;

                if (ppCustomVerticePattern && ppVerticePatternObject != null)
                {
                    gm = Instantiate(ppVerticePatternObject);
                    if (gm.GetComponent<Renderer>() && ppUseCustomColor)
                        gm.GetComponent<Renderer>().sharedMaterial.color = ppCustomVerticeColor;
                }
                else
                {
                    Material new_Mat = new Material(Shader.Find("Unlit/Color"));
                    new_Mat.color = ppCustomVerticeColor;
                    gm = MD_Octahedron.Generate();
                    gm.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
                    gm.GetComponentInChildren<Renderer>().material = new_Mat;
                }

                gm.transform.parent = _VertexRoot.transform;

                gm.transform.position = vertices[i];
                workingVertices = new Vector3[vertices.Length];
                System.Array.Copy(vertices, workingVertices, vertices.Length);
                workingTargetPoints.Add(gm.transform);

                gm.name = "P" + i.ToString();
            }

            //---Fixing Point Hierarchy & renaming
            int counter = 0;
            foreach (Transform vertice in workingTargetPoints)
            {
                if (vertice.gameObject.activeInHierarchy == false) continue;
                foreach (Transform vertice2 in workingTargetPoints)
                {
                    if (vertice2.name == vertice.name) continue;
                    if (vertice2.transform.position == vertice.transform.position)
                    {
                        vertice2.hideFlags = HideFlags.HideInHierarchy;
                        vertice2.transform.parent = vertice.transform;
                        vertice2.gameObject.SetActive(false);
                    }
                }
                counter++;
                vertice.hideFlags = HideFlags.None;
                vertice.gameObject.SetActive(true);
                vertice.name = "P" + counter.ToString();
            }

            meshFilter.sharedMesh.vertices = vertices;
            meshFilter.sharedMesh.MarkDynamic();

            _VertexRoot.transform.parent = meshFilter.transform;
            _VertexRoot.transform.localPosition = Vector3.zero;

            if (!_BornAsSkinnedMesh)
            {
                _VertexRoot.transform.localScale = LastScale;
                _VertexRoot.transform.rotation = LastRotation;
            }

            Internal_MPE_ReceiveMeshInfo();

#if UNITY_EDITOR
            if (ppDeselectObjectAfterVerticeLimit)
            {
                UnityEditor.Selection.activeObject = null;
                foreach (Transform p in workingTargetPoints)
                    p.gameObject.SetActive(false);
            }
            ppDeselectObjectAfterVerticeLimit = false;
#endif
        }

        /// <summary>
        /// Clear vertice editor if possible
        /// </summary>
        public void MPE_ClearVerticeEditor()
        {
            //Don't continue if animation mode is enabled
            if (ppAnimationMode) return;

            if (workingTargetPoints.Count > 0)
                workingTargetPoints.Clear();

            if (ppVerticesRoot != null)
                DestroyImmediate(ppVerticesRoot.gameObject);
        }

        //Mesh Combine

        /// <summary>
        /// Combine all sub-meshes with current mesh. This will create a brand new gameObject & notification will show up (Safer method)
        /// </summary>
        public void MPE_CombineMesh()
        {
            if (meshFilter == null || (meshFilter && meshFilter.sharedMesh == null))
            {
                MD_Debug.Debug(this, "The object doesn't contain Mesh Filter or shared mesh is empty", MD_Debug.DebugType.Error);
                return;
            }

            MPE_ClearVerticeEditor();

#if UNITY_EDITOR
            if(!Application.isPlaying)
            {
                if(UnityEditor.EditorUtility.DisplayDialog("Are you sure to combine meshes?", "If you combine mesh with its sub-meshes, materials & all components will be lost. Are you sure to combine meshes?", "Yes, proceed", "No, cancel"))
                return;
            }
#endif
            transform.parent = null;
            Vector3 Last_POS = transform.position;
            transform.position = Vector3.zero;

            MeshFilter[] meshes_ = GetComponentsInChildren<MeshFilter>();
            CombineInstance[] combiners_ = new CombineInstance[meshes_.Length];

            int counter_ = 0;
            while (counter_ < meshes_.Length)
            {
                combiners_[counter_].mesh = meshes_[counter_].sharedMesh;
                combiners_[counter_].transform = meshes_[counter_].transform.localToWorldMatrix;
                if (meshes_[counter_].gameObject != this.gameObject)
                    DestroyImmediate(meshes_[counter_].gameObject);
                counter_++;
            }

            GameObject newgm = new GameObject();
            MeshFilter f = newgm.AddComponent<MeshFilter>();
            newgm.AddComponent<MeshRenderer>();
            newgm.name = name;

            Mesh newMesh = new Mesh();
            newMesh.CombineMeshes(combiners_);

            f.sharedMesh = newMesh;
            f.sharedMesh.name = ppINFO_MeshName;
            newgm.GetComponent<Renderer>().material = ppDefaultMaterial;
            newgm.AddComponent<MD_MeshProEditor>().ppINFO_MeshName = ppINFO_MeshName;

            newgm.transform.position = Last_POS;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.Selection.activeGameObject = newgm;
                UnityEditor.EditorUtility.DisplayDialog("Successfully combined. Notice please...", "If your mesh has been successfully combined, please notice that the prefab of the 'old' mesh in Assets Folder is no more valid for the new one. " +
                    "If you want to store the new mesh, you have to save your mesh prefab again.", "OK");
            }
#endif
            DestroyImmediate(this.gameObject);
        }

        /// <summary>
        /// Combine all sub-meshes with current mesh. This will NOT create a new gameObject & any notification will show up (Less safer method)
        /// </summary>
        public void MPE_CombineMeshQuick()
        {
            if (meshFilter == null || (meshFilter && meshFilter.sharedMesh == null))
            {
                MD_Debug.Debug(this, "The object doesn't contain Mesh Filter or shared mesh is empty", MD_Debug.DebugType.Error);
                return;
            }

            MPE_ClearVerticeEditor();

            transform.parent = null;

            Vector3 Last_POS = transform.position;
            transform.position = Vector3.zero;

            MeshFilter[] meshes_ = GetComponentsInChildren<MeshFilter>();
            CombineInstance[] combiners_ = new CombineInstance[meshes_.Length];

            long counter_ = 0;
            while (counter_ < meshes_.Length)
            {
                combiners_[counter_].mesh = meshes_[counter_].sharedMesh;
                combiners_[counter_].transform = meshes_[counter_].transform.localToWorldMatrix;
                if (meshes_[counter_].gameObject != this.gameObject)
                    DestroyImmediate(meshes_[counter_].gameObject);
                counter_++;
            }

            Mesh newMesh = new Mesh();
            newMesh.CombineMeshes(combiners_);

            meshFilter.sharedMesh = newMesh;
            meshFilter.sharedMesh.name = ppINFO_MeshName;
            ppSelectedModification = SelectedModification.None;

            transform.position = Last_POS;
            Internal_MPE_ReceiveMeshInfo();
        }

        //Mesh References

        /// <summary>
        /// Create a brand new object with new mesh reference. All your components on the object will be lost!
        /// </summary>
        public void MPE_CreateNewReference()
        {
            if (meshFilter == null || (meshFilter && meshFilter.sharedMesh == null))
            {
                MD_Debug.Debug(this, "The object doesn't contain Mesh Filter or shared mesh is empty", MD_Debug.DebugType.Error);
                return;
            }

            MPE_ClearVerticeEditor();

            GameObject newgm = new GameObject();
            MeshFilter f = newgm.AddComponent<MeshFilter>();
            newgm.AddComponent<MeshRenderer>();
            newgm.name = name;

            Material[] Materials = GetComponent<Renderer>().sharedMaterials;

            Vector3 Last_POS = transform.position;
            transform.position = Vector3.zero;

            CombineInstance[] combine = new CombineInstance[1];
            combine[0].mesh = meshFilter.sharedMesh;
            combine[0].transform = meshFilter.transform.localToWorldMatrix;

            Mesh newMesh = new Mesh();
            newMesh.CombineMeshes(combine);

            f.sharedMesh = newMesh;
            newgm.SetActive(false);
            newgm.AddComponent<MD_MeshProEditor>()._AlreadyAwake = true;
            newgm.GetComponent<MD_MeshProEditor>().ppINFO_MeshName = ppINFO_MeshName;
            f.sharedMesh.name = ppINFO_MeshName;

            if (Materials.Length > 0) newgm.GetComponent<Renderer>().sharedMaterials = Materials;
            newgm.transform.position = Last_POS;
            newgm.SetActive(true);

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.Selection.activeGameObject = newgm;
                if(MD_GlobalPreferences.popupEditorWindow)
                     UnityEditor.EditorUtility.DisplayDialog("Notice please...", "If you change the reference of your mesh, please notice that the prefab of the 'old' mesh in Assets Folder is no more valid for the new one. " +
                    "If you would like to store a new mesh, you have to save your mesh prefab again.", "OK");
            }
#endif
            newgm.GetComponent<MD_MeshProEditor>().meshFilter = f;
            newgm.GetComponent<MD_MeshProEditor>().Internal_MPE_ReceiveMeshInfo(true);
            DestroyImmediate(this.gameObject);
        }

        /// <summary>
        /// Restore current mesh to the original form
        /// </summary>
        public void MPE_RestoreMeshToOriginal()
        {
            if (meshFilter == null || (meshFilter && meshFilter.sharedMesh == null))
            {
                MD_Debug.Debug(this, "The object doesn't contain Mesh Filter or shared mesh is empty", MD_Debug.DebugType.Error);
                return;
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                if(ppAnimationMode)
                {
                    UnityEditor.EditorUtility.DisplayDialog("Can't continue", "Couldn't restore original mesh data, because the Animation Mode is enabled.", "OK");
                    return;
                }
                if(!UnityEditor.EditorUtility.DisplayDialog("Are you sure?", "Are you sure to restore original mesh data?", "Restore", "Cancel"))
                    return;
            }
#endif
            if (ppAnimationMode)
            {
                MD_Debug.Debug(this, "Couldn't restore original mesh data, because the Animation Mode is enabled", MD_Debug.DebugType.Warning);
                return;
            }

            MPE_ClearVerticeEditor();

            try
            {
                meshFilter.sharedMesh.vertices = originalVertices;
                meshFilter.sharedMesh.triangles = originalTriangles;
                meshFilter.sharedMesh.normals = originalNormals;
                meshFilter.sharedMesh.uv = originalUVS;
            }
            catch(UnityException e)
            { MD_Debug.Debug(this, "Couldn't restore your original mesh. An error occured: " + e.Message, MD_Debug.DebugType.Error); }
            Internal_MPE_ReceiveMeshInfo();

            ppSelectedModification = SelectedModification.None;
        }

        /// <summary>
        /// Convert skinned mesh renderer to mesh renderer & mesh filter. This will create a new object, so none of the components will remain!
        /// </summary>
        public void MPE_ConvertFromSkinnedToFilter()
        {
            if (!GetComponent<SkinnedMeshRenderer>()) return;
            if (GetComponent<SkinnedMeshRenderer>().sharedMesh == null) return;

            GameObject newgm = new GameObject();
            MeshFilter f = newgm.AddComponent<MeshFilter>();
            newgm.AddComponent<MeshRenderer>();
            newgm.name = name + "_ConvertedMesh";

            Material[] mater = null;

            if (GetComponent<SkinnedMeshRenderer>().sharedMaterials.Length > 0)
                mater = GetComponent<Renderer>().sharedMaterials;

            Vector3 Last_POS = transform.root.transform.position;
            Vector3 Last_SCA = transform.localScale;
            Quaternion Last_ROT = transform.rotation;

            transform.position = Vector3.zero;

            Mesh newMesh = GetComponent<SkinnedMeshRenderer>().sharedMesh;

            f.sharedMesh = newMesh;
            f.sharedMesh.name = ppINFO_MeshName;
            if (mater.Length != 0)
                newgm.GetComponent<Renderer>().sharedMaterials = mater;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.Selection.activeGameObject = newgm;
                if(MD_GlobalPreferences.popupEditorWindow)
                    UnityEditor.EditorUtility.DisplayDialog("Successfully Converted!", "Your skinned mesh renderer has been successfully converted to the Mesh Filter and Mesh Renderer.", "OK");
            }
#endif

            newgm.AddComponent<MD_MeshProEditor>()._BornAsSkinnedMesh = true;

            newgm.transform.position = Last_POS;
            newgm.transform.rotation = Last_ROT;
            newgm.transform.localScale = Last_SCA;

            DestroyImmediate(this.gameObject);
        }

        //Quick Internal Modifiers

        private Mesh intern_modif_sourceMesh;
        private Mesh intern_modif_workingMesh;

        /// <summary>
        /// Internal quick modifier - mesh smooth
        /// </summary>
        public void MPE_SmoothMesh(float intensity = 0.5f)
        {
            if (meshFilter == null || (meshFilter && meshFilter.sharedMesh == null))
            {
                MD_Debug.Debug(this, "The object doesn't contain Mesh Filter or shared mesh is empty", MD_Debug.DebugType.Error);
                return;
            }

            MPE_ClearVerticeEditor();

            //Mesh Smooth will not pass if the recommended vertex count is passed
            if (!MeshProEditor_Utilities.util_CheckVerticeCount(meshFilter.sharedMesh.vertexCount))
                return;

            intern_modif_sourceMesh = new Mesh();
            intern_modif_sourceMesh = meshFilter.sharedMesh;

            Mesh clone = new Mesh();

            clone.vertices = intern_modif_sourceMesh.vertices;
            clone.normals = intern_modif_sourceMesh.normals;
            clone.tangents = intern_modif_sourceMesh.tangents;
            clone.triangles = intern_modif_sourceMesh.triangles;

            clone.uv = intern_modif_sourceMesh.uv;
            clone.uv2 = intern_modif_sourceMesh.uv2;
            clone.uv2 = intern_modif_sourceMesh.uv2;

            clone.bindposes = intern_modif_sourceMesh.bindposes;
            clone.boneWeights = intern_modif_sourceMesh.boneWeights;
            clone.bounds = intern_modif_sourceMesh.bounds;

            clone.colors = intern_modif_sourceMesh.colors;
            clone.name = intern_modif_sourceMesh.name;

            intern_modif_workingMesh = clone;
            meshFilter.mesh = intern_modif_workingMesh;

            intern_modif_workingMesh.vertices = MD_MeshMathUtilities.smoothing_HCFilter.HCFilter(intern_modif_sourceMesh.vertices, intern_modif_workingMesh.triangles, 0.0f, intensity);

            Mesh m = new Mesh();

            m.name = ppINFO_MeshName;
            m.vertices = meshFilter.sharedMesh.vertices;
            m.triangles = meshFilter.sharedMesh.triangles;
            m.uv = meshFilter.sharedMesh.uv;
            m.normals = meshFilter.sharedMesh.normals;

            workingVertices = null;
            workingTargetPoints.Clear();

            m = intern_modif_workingMesh;

            meshFilter.sharedMesh = m;

            Internal_MPE_ReceiveMeshInfo();

#if UNITY_EDITOR
            if (ppDeselectObjectAfterVerticeLimit)
                UnityEditor.Selection.activeObject = null;
            ppDeselectObjectAfterVerticeLimit = false;
#endif
        }

        /// <summary>
        /// Internal quick modifier - mesh subdivision
        /// </summary>
        public void MPE_SubdivideMesh(int Level)
        {
            if (meshFilter == null || (meshFilter && meshFilter.sharedMesh == null))
            {
                MD_Debug.Debug(this, "The object doesn't contain Mesh Filter or shared mesh is empty", MD_Debug.DebugType.Error);
                return;
            }

            MPE_ClearVerticeEditor();

            //Mesh Subdivision will not pass if the recommended vertex count is passed
            if (!MeshProEditor_Utilities.util_CheckVerticeCount(meshFilter.sharedMesh.vertexCount))
                return;

            intern_modif_sourceMesh = new Mesh();
            intern_modif_sourceMesh = meshFilter.sharedMesh;
            MD_MeshMathUtilities.mesh_Subdivision.Subdivide(intern_modif_sourceMesh, Level);
            meshFilter.sharedMesh = intern_modif_sourceMesh;

            Mesh m = new Mesh();

            m.name = ppINFO_MeshName;
            m.vertices = meshFilter.sharedMesh.vertices;
            m.triangles = meshFilter.sharedMesh.triangles;
            m.uv = meshFilter.sharedMesh.uv;
            m.normals = meshFilter.sharedMesh.normals;

            workingVertices = null;
            workingTargetPoints.Clear();

            m = intern_modif_sourceMesh;

            meshFilter.sharedMesh = m;

            Internal_MPE_ReceiveMeshInfo();

#if UNITY_EDITOR
            if (ppDeselectObjectAfterVerticeLimit)
                UnityEditor.Selection.activeObject = null;
            ppDeselectObjectAfterVerticeLimit = false;
#endif
        }

        #endregion
    }
}