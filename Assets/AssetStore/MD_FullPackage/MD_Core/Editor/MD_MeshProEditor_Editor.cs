using UnityEngine;
using UnityEditor;
using MD_Plugin;

namespace MD_PluginEditor
{
    [CustomEditor(typeof(MD_MeshProEditor))]
    public class MD_MeshProEditor_Editor : MD_EditorUtilities
    {
        private bool _HaveMeshFilter;
        private bool _HaveMeshSkinned;

        private void OnEnable()
        {
            Foldout = new bool[3];
        }

        //----GUI Stuff-------------
        GUIStyle style = new GUIStyle();

        public GUIStyle styleTest;
        public Texture2D VerticesIcon;
        public Texture2D ColliderIcon;
        public Texture2D IdentityIcon;
        public Texture2D ModifyIcon;
        [Space]
        public Texture2D SmoothIcon;
        public Texture2D SubdivisionIcon;

        //----Adds-------------------
        private bool[] Foldout = new bool[3];
        public float SmoothMeshIntens = 0.5f;
        public int[] DivisionLevel = new int[] { 2, 3, 4, 6, 8};
        public int DivisionlevelSelection = 1;

        private void OnSceneGUI()
        {
            MD_MeshProEditor m = (MD_MeshProEditor)target;

            if (m.ppEnableZoneGenerator && m.ppSelectedModification == MD_MeshProEditor.SelectedModification.Vertices)
            {
                Vector3 Zone = m.ppZoneGenerator;
                float radius = m.ppZoneGeneratorRadius;

                Handles.color = Color.magenta;
                Handles.CircleHandleCap(0, Zone, Quaternion.identity, radius, EventType.DragUpdated);
                Handles.CircleHandleCap(0, Zone, Quaternion.Euler(0, 90, 0), radius, EventType.DragUpdated);

                EditorGUI.BeginChangeCheck();
                Handles.color = Color.magenta;
                Handles.DrawWireDisc(Zone, Vector3.up, radius);
                Handles.DrawWireDisc(Zone, Vector3.right, radius);
                Zone = Handles.DoPositionHandle(Zone, m.transform.rotation);
                if (EditorGUI.EndChangeCheck())
                {
                    m.ppZoneGeneratorRadius = radius;
                    m.ppZoneGenerator = Zone;
                }
            }
        }

        private void ClearVerticeEditor()
        {
            MD_MeshProEditor m = (MD_MeshProEditor)target;

            if (m == null)
                return;

            if (m.ppAnimationMode)
                return;

            if (m.workingTargetPoints.Count > 0)
                m.workingTargetPoints.Clear();

            if (m.ppVerticesRoot != null)
                DestroyImmediate(m.ppVerticesRoot.gameObject);
        }

        public override void OnInspectorGUI()
        {
            if (target == null)
                return;

            style.richText = true;
            MD_MeshProEditor m = (MD_MeshProEditor)target;

            _HaveMeshFilter = m.GetComponent<MeshFilter>();
            _HaveMeshSkinned = m.GetComponent<SkinnedMeshRenderer>();

            if (m == null)
            {
                GUIUtility.ExitGUI();
                return;
            }

            if (!_HaveMeshFilter)
            {
                if (_HaveMeshSkinned)
                {
                    if (pb("Convert to Mesh Filter"))
                        m.MPE_ConvertFromSkinnedToFilter();
                    phb("Skinned Mesh Renderer is a component to control your mesh by bones. Press 'Convert To Mesh Filter' to start editing its mesh source.", MessageType.Info);
                }
                else phb("No Mesh Identity... Object must contains Mesh Filter or Skinned Mesh Renderer component to access mesh editor.", MessageType.Error);
                return;
            }

            ps(20);

            #region UpperCategories
            ph(false);
            if (pb(new GUIContent("Vertices", VerticesIcon, "Vertices Modification")))
            {
                if (m.ppSelectedModification == MD_MeshProEditor.SelectedModification.Vertices)
                {
                    m.ppSelectedModification = MD_MeshProEditor.SelectedModification.None;
                    ClearVerticeEditor();
                }
                else
                {
                    m.ppSelectedModification = MD_MeshProEditor.SelectedModification.Vertices;
                    m.MPE_CreateVerticeEditor();
                }
            }
            if (pb(new GUIContent("Collider", ColliderIcon, "Collider Modification")))
            {
                ClearVerticeEditor();
                if (m.ppSelectedModification == MD_MeshProEditor.SelectedModification.Collider)
                    m.ppSelectedModification = MD_MeshProEditor.SelectedModification.None;
                else
                    m.ppSelectedModification = MD_MeshProEditor.SelectedModification.Collider;
            }
            if (pb(new GUIContent("Identity", IdentityIcon, "Identity Modification")))
            {
                ClearVerticeEditor();
                if (m.ppSelectedModification == MD_MeshProEditor.SelectedModification.Identity)
                    m.ppSelectedModification = MD_MeshProEditor.SelectedModification.None;
                else
                    m.ppSelectedModification = MD_MeshProEditor.SelectedModification.Identity;
            }
            if (pb(new GUIContent("Mesh", ModifyIcon, "Mesh Modification")))
            {
                ClearVerticeEditor();
                if (m.ppSelectedModification == MD_MeshProEditor.SelectedModification.Mesh)
                    m.ppSelectedModification = MD_MeshProEditor.SelectedModification.None;
                else
                    m.ppSelectedModification = MD_MeshProEditor.SelectedModification.Mesh;
            }
            phe();
            #endregion


            #region Category_Vertices
            if (m.ppSelectedModification == MD_MeshProEditor.SelectedModification.Vertices)
            {
                Color c;
                ColorUtility.TryParseHtmlString("#f2d3d3", out c);
                GUI.color = c;
                pl("| Vertices Modification");
                pv();
                ph();
                ppDrawProperty("ppAnimationMode", "Animation Mode", "If enabled, the program will not refresh vertices and the mesh will keep the generated points");
                phe();
                ppDrawProperty("ppCustomVerticePattern", "Custom Vertice Pattern", "If enabled, you will be able to choose your own vertice object pattern");
                if (m.ppCustomVerticePattern)
                {
                    ppDrawProperty("ppVerticePatternObject", "Vertice Object Pattern");
                    ppDrawProperty("ppUseCustomColor", "Enable Custom Color");
                    if (m.ppUseCustomColor)
                        ppDrawProperty("ppCustomVerticeColor", "Custom Vertice Color");
                    phb("To show new vertice pattern, refresh vertice editor by clicking the 'Vertices Modification' button");
                }
                ps(5);
                if (pb("Open Vertex Tool Window")) MD_VertexTool.Init();
                ps(5);
                if (m.ppINFO_Vertices > MD_GlobalPreferences.vertexLimit)
                {
                    GUI.color = Color.yellow;
                    pv();
                    phb("Your mesh has more than " + MD_GlobalPreferences.vertexLimit.ToString() + " vertices. All points have been automatically hidden to prevent performance dropdown. If the mesh is below the 10 000 vertex count, use Zone Generator (if possible) to show specific points only.");
                    if (pb("Activate All Points"))
                    {
                        if (m.meshFilter.sharedMesh.vertices.Length > 10000)
                        {
                            EditorUtility.DisplayDialog("I'm Sorry", "The mesh has too many vertices [" + m.meshFilter.sharedMesh.vertexCount + "]. You won't be able to process this function due to incredible long freeze. [This message can be disabled in the code on your own risk and responsibility]", "OK");
                            return;
                        }
                        if (m.workingTargetPoints.Count > 0)
                        {
                            foreach (Transform p in m.workingTargetPoints)
                                p.gameObject.SetActive(true);
                        }
                        else if(EditorUtility.DisplayDialog("Are you sure?","Are you sure to continue? This will first generate new points (Which may take a while) and then you can activate/ deactivate points on your own performance risk.","Yes","No"))
                            m.MPE_CreateVerticeEditor(true);
                    }
                    if (pb("Deactivate All Points"))
                    {
                        if (m.meshFilter.sharedMesh.vertices.Length > 10000)
                        {
                            EditorUtility.DisplayDialog("I'm Sorry", "The mesh has too many vertices [" + m.meshFilter.sharedMesh.vertexCount + "]. You won't be able to process this function due to incredible long freeze. [This message can be disabled in the code on your own risk and responsibility]", "OK");
                            return;
                        }
                        if (m.workingTargetPoints.Count > 0)
                        {
                            foreach (Transform p in m.workingTargetPoints)
                                p.gameObject.SetActive(false);
                        }
                    }
                    pve();
                }
                ColorUtility.TryParseHtmlString("#f2d3d3", out c);
                GUI.color = c;
                ps(5);
                ppDrawProperty("ppEnableZoneGenerator", "Enable Zone Generator", "If enabled, you will be able to generate points in specific 'zone radius'");
                if (m.ppEnableZoneGenerator)
                {
                    pv();
                    ppDrawProperty("ppZoneGenerator", "Zone Location");
                    ppDrawProperty("ppZoneGeneratorRadius", "Field Radius");

                    ph();
                    if (pb("Generate Points In Zone"))
                    {
                        if (m.meshFilter.sharedMesh.vertices.Length > 10000)
                        {
                            EditorUtility.DisplayDialog("I'm Sorry", "The mesh has too many vertices [" + m.meshFilter.sharedMesh.vertexCount + "]. You won't be able to process this function due to incredible long freeze. [This message can be disabled in the code on your own risk & responsibility]", "OK");
                            return;
                        }
                        m.MPE_CreateVerticeEditor(true);
                        if (m.workingTargetPoints.Count > 0)
                            for (int i = 0; i < m.workingTargetPoints.Count; i++)
                            {
                                if (Vector3.Distance(m.workingTargetPoints[i].transform.position, m.ppZoneGenerator) > m.ppZoneGeneratorRadius)
                                    m.workingTargetPoints[i].gameObject.SetActive(false);
                                else
                                    m.workingTargetPoints[i].gameObject.SetActive(true);
                            }
                        return;
                    }
                    if (m.workingTargetPoints.Count > 0)
                    {
                        if (pb("Show All Points"))
                        {
                            if (m.meshFilter.sharedMesh.vertices.Length > 10000)
                            {
                                EditorUtility.DisplayDialog("I'm Sorry", "The mesh has too many vertices [" + m.meshFilter.sharedMesh.vertexCount + "]. You won't be able to process this function due to incredible long freeze. [This message can be disabled in the code on your own risk & responsibility]", "OK");
                                return;
                            }
                            if (m.workingTargetPoints.Count > 0)
                                for (int i = 0; i < m.workingTargetPoints.Count; i++)
                                    m.workingTargetPoints[i].gameObject.SetActive(true);
                            return;
                        }
                    }
                    phe();
                    ps(5);
                    if (pb("Reset Zone Position"))
                        m.ppZoneGenerator = m.transform.position;
                    pve();
                }
                ps(5);
                if (m.ppAnimationMode)
                {
                    pv();
                    pl("Animation Mode | Vertices Manager");
                    ph();
                    if (pb("Show Vertices"))
                        m.MPE_ShowHideVertices(true);
                    if (pb("Hide Vertices"))
                        m.MPE_ShowHideVertices(false);
                    phe();
                    ps(5);
                    ph();
                    if (pb("Ignore Raycast"))
                        m.MPE_IgnoreRaycastVertices(true);
                    if (pb("Default Layer"))
                        m.MPE_IgnoreRaycastVertices(false);
                    phe();
                    pve();
                }
                pve();
            }
            #endregion

            #region Category_Collider
            if (m.ppSelectedModification == MD_MeshProEditor.SelectedModification.Collider)
            {
                Color c;
                ColorUtility.TryParseHtmlString("#7beb99", out c);
                GUI.color = c;
                pl("| Collider Modification");
                if (!m.GetComponent<MD_MeshColliderRefresher>())
                {
                    pv();

                    if (pb("Add Mesh Collider Refresher"))
                        m.gameObject.AddComponent<MD_MeshColliderRefresher>();

                    pve();
                }
                else
                    phb("The selected object already contains Mesh Collider Refresher component", MessageType.Info);
            }
            #endregion

            #region Category_Identity
            if (m.ppSelectedModification == MD_MeshProEditor.SelectedModification.Identity)
            {
                Color c;
                ColorUtility.TryParseHtmlString("#baefff", out c);
                GUI.color = c;
                pl("| Identity Modification");

                pv();

                if (pb(new GUIContent("Create New Mesh Reference","Create a brand new object with new mesh reference. This will create a new mesh reference and all your components & behaviours on this gameObject will be removed")))
                {
                    if (!EditorUtility.DisplayDialog("Are you sure?", "Are you sure to create a new mesh reference? This will create a brand new object with new mesh reference and all your components and behaviours on this gameObject will be lost.", "Yes", "No"))
                        return;
                    m.MPE_CreateNewReference();
                    return;
                }
                if (m.transform.childCount > 0 && m.transform.GetChild(0).GetComponent<MeshFilter>())
                {
                    if (pb("Combine All SubMeshes"))
                    {
                        m.MPE_CombineMesh();
                        return;
                    }
                }
                if (pb("Save Mesh To Assets"))
                    m.Internal_MPE_SaveMeshToAssetsDatabase();

                ps(5);
                if (m.ppOptimizeMesh)
                {
                    if (pb("Recalculate Bounds & Normals"))
                    {
                        m.meshFilter.sharedMesh.RecalculateBounds();
                        m.meshFilter.sharedMesh.RecalculateNormals();
                    }
                }
                if (!m.ppUpdateEveryFrame)
                {
                    if (pb("Update Mesh"))
                        m.MPE_UpdateMesh();
                }
                ps(5);

                ppDrawProperty("ppNewReferenceAfterCopy", "Create New Reference On Copy-Paste", "If enabled, the new mesh reference will be created with clearly new mesh data on copy-paste process");
                ppDrawProperty("ppUpdateEveryFrame", "Update Every Frame", "If enabled, the mesh will be updated every frame and you will be able to deform the mesh at runtime if it's possible");
                ppDrawProperty("ppOptimizeMesh", "Optimize Mesh", "If enabled, the mesh will stop refreshing and recalculating Bounds and Normals & you will be able to recalculate them manually");
                pve();
            }
            #endregion

            #region Category_Mesh
            if (m.ppSelectedModification == MD_MeshProEditor.SelectedModification.Mesh)
            {
                Color c;
                ColorUtility.TryParseHtmlString("#dee7ff", out c);
                GUI.color = c;
                pl("| Mesh Modification");

                pv();

                pl("Internal Mesh Modifiers");
                EditorGUI.indentLevel++;
                Foldout[0] = EditorGUILayout.Foldout(Foldout[0], new GUIContent("Mesh Smooth", SmoothIcon, "Smooth mesh by the smooth level"));
                if (Foldout[0])
                {
                    EditorGUI.indentLevel++;
                    SmoothMeshIntens = EditorGUILayout.Slider("Smooth Level", SmoothMeshIntens, 0.5f, 0.05f);
                    ph(false);
                    ps(EditorGUI.indentLevel * 10);
                    if (pb(new GUIContent("Smooth Mesh", SmoothIcon)))
                        m.MPE_SmoothMesh(SmoothMeshIntens);
                    phe();

                    EditorGUI.indentLevel--;
                }
                Foldout[1] = EditorGUILayout.Foldout(Foldout[1], new GUIContent("Mesh Subdivide", SubdivisionIcon, "Subdivide mesh by the subdivision level"));
                if (Foldout[1])
                {
                    EditorGUI.indentLevel++;
                    DivisionlevelSelection = EditorGUILayout.IntSlider("Subdivision Level", DivisionlevelSelection, 0, DivisionLevel[DivisionLevel.Length - 1]);
                    ph(false);
                    ps(EditorGUI.indentLevel * 10);
                    if (pb(new GUIContent("Subdivide Mesh", SubdivisionIcon)))
                        m.MPE_SubdivideMesh(DivisionlevelSelection);
                    phe();

                    EditorGUI.indentLevel--;
                }
                EditorGUI.indentLevel--;
                serializedObject.Update();
                ps(10);

                pl("External Mesh Modifiers");
                EditorGUI.indentLevel++;
                Foldout[2] = EditorGUILayout.Foldout(Foldout[2], "Modifiers");
                if (Foldout[2])
                {
                    EditorGUI.indentLevel++;

                    ColorUtility.TryParseHtmlString("#e3badb", out c);
                    GUI.color = c;
                    pl("Logical Deformers");
                    if (pb(new GUIContent("Mesh Morpher")))
                    {
                        m.gameObject.AddComponent<MDM_Morpher>();
                        DestroyImmediate(m);
                        return;
                    }
                    if (pb(new GUIContent("Mesh Effector")))
                    {
                        m.gameObject.AddComponent<MDM_MeshEffector>();
                        DestroyImmediate(m);
                        return;
                    }
                    if (pb(new GUIContent("Mesh FFD")))
                    {
                        m.gameObject.AddComponent<MDM_FFD>();
                        DestroyImmediate(m);
                        return;
                    }

                    ColorUtility.TryParseHtmlString("#dedba0", out c);
                    GUI.color = c;
                    pl("World Interactive");
                    if (pb(new GUIContent("Interactive Surface [CPU]")))
                    {
                        m.gameObject.AddComponent<MDM_InteractiveSurface>();
                        DestroyImmediate(m);
                        return;
                    }
                    if (pb(new GUIContent("Surface Tracking [GPU]")))
                    {
                        m.gameObject.AddComponent<MDM_SurfaceTracking>();
                        DestroyImmediate(m);
                        return;
                    }
                    if (pb(new GUIContent("Mesh Damage")))
                    {
                        m.gameObject.AddComponent<MDM_MeshDamage>().ppCreateNewReference = false;
                        DestroyImmediate(m);
                        return;
                    }
                    if (pb(new GUIContent("Mesh Fit")))
                    {
                        m.gameObject.AddComponent<MDM_MeshFit>();
                        DestroyImmediate(m);
                        return;
                    }
                    if (pb(new GUIContent("Melt Controller")))
                    {
                        m.gameObject.AddComponent<MDM_MeltController>();
                        DestroyImmediate(m);
                        return;
                    }
                    if (pb(new GUIContent("Mesh Slime")))
                    {
                        m.gameObject.AddComponent<MDM_MeshSlime>();
                        DestroyImmediate(m);
                        return;
                    }

                    ColorUtility.TryParseHtmlString("#aae0b2", out c);
                    GUI.color = c;
                    pl("Basics");
                    if (pb(new GUIContent("Twist")))
                    {
                        m.gameObject.AddComponent<MDM_Twist>();
                        DestroyImmediate(m);
                        return;
                    }
                    if (pb(new GUIContent("Bend")))
                    {
                        m.gameObject.AddComponent<MDM_Bend>();
                        DestroyImmediate(m);
                        return;
                    }
                    if (pb(new GUIContent("Mesh Noise")))
                    {
                        m.gameObject.AddComponent<MDM_MeshNoise>();
                        DestroyImmediate(m);
                        return;
                    }

                    ColorUtility.TryParseHtmlString("#aad2e0", out c);
                    GUI.color = c;
                    pl("Sculpting");
                    if (pb(new GUIContent("Sculpting Pro")))
                    {
                        m.gameObject.AddComponent<MDM_SculptingPro>();
                        DestroyImmediate(m);
                        return;
                    }

                    ColorUtility.TryParseHtmlString("#ebebeb", out c);
                    GUI.color = c;
                    pl("Additional Events");
                    if (pb(new GUIContent("Raycast Event")))
                    {
                        m.gameObject.AddComponent<MDM_RaycastEvent>();
                        DestroyImmediate(m);
                        return;
                    }

                    ColorUtility.TryParseHtmlString("#dee7ff", out c);
                    GUI.color = c;
                    EditorGUI.indentLevel--;
                }
                EditorGUI.indentLevel--;
                pve();
            }
            #endregion


            #region Bottom Categories
            ps(20);
            GUI.color = Color.white;
            pl("Mesh Information");
            pv();

            ph(false);
            ppDrawProperty("ppINFO_MeshName", "Mesh Name", "Mesh name... Change mesh name and Refresh Identity.");
            phe();

            ph(false);
            pl("Vertices:");
            GUILayout.TextField(m.ppINFO_Vertices.ToString());
            pl("Triangles:");
            GUILayout.TextField(m.ppINFO_Triangles.ToString());
            pl("Normals:");
            GUILayout.TextField(m.ppINFO_Normals.ToString());
            pl("UVs:");
            GUILayout.TextField(m.ppINFO_Uvs.ToString());
            phe();

            if (pb("Restore Original Mesh"))
                m.MPE_RestoreMeshToOriginal();
            pve();
            #endregion

            ps(10);

            if (target != null)
                serializedObject.Update();
        }
    }
}