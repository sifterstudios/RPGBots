using UnityEngine;
using UnityEditor;
using MD_Plugin;

namespace MD_PluginEditor
{
    [CustomEditor(typeof(MDM_SculptingPro))]
    public class MDM_SculptingPro_Editor : MD_EditorUtilities
    {
        private MDM_SculptingPro ss;

        private void OnEnable()
        {
            ss = (MDM_SculptingPro)target;
        }

        public override void OnInspectorGUI()
        {
            int gSize = GUI.skin.font.fontSize;
            ps();
            GUI.skin.label.fontSize = 21;
            pl("Sculpting Pro v1.5", true);
            phb("v1.5 Change-Log\n- Added History Record [Undo/Redo]\n- Major optimizations\n- Minor refactor");
            GUI.skin.label.fontSize = gSize;

            ps();

            pl("Essential Settings", true);
            pv();
            pv();
            ppDrawProperty("SS_AtRuntime", "Sculpting At Runtime", "If enabled, the script will work only at runtime");
            ps(3);
            ppDrawProperty("SS_MobileSupport", "Sculpting For Mobile", "If enabled, the system will work only for mobile devices");
            pve();
            ps(5);
            pv();
            ppDrawProperty("SS_AutoRecalculateNormals", "Auto Recalculate Normals", "If enabled, the system will recalculate mesh normals automatically");
            ppDrawProperty("SS_AutoRecalculateBounds", "Auto Recalculate Bounds", "If enabled, the system will recalculate mesh bounds automatically");
            if (ss.SS_UseInput && !ss.SS_MultithreadingSupported)
                ppDrawProperty("SS_UpdateColliderAfterRelease", "Update Collider After Release", "If disabled, the collider will be updated every frame if the mouse is down");
            else if (ss.SS_MultithreadingSupported && !ss.SS_UpdateColliderAfterRelease)
                ss.SS_UpdateColliderAfterRelease = true;
            pve();
            ps(5);
            pv();
            ppDrawProperty("SS_RecordHistory", "Record History", "If enabled, the system will record every vertex step and you will be able to make a step back");
            if (ss.SS_RecordHistory)
            {
                ppDrawProperty("SS_MaxHistoryRecords", "Max History Records");
                phb("The history will be automatically recorded on control up. Call 'Undo' method to make step backward or forward");
            }
            pve();
            pve();

            if (!ss.SS_AtRuntime)
            {
                pv();
                ppDrawProperty("SS_InEditMode", "In Edit Mode", "If enabled, the selection will be locked to the object and you are free to sculpt the mesh");
                phb("LMouse: Raise\nLShift+LMouse: Lower\nLControl+LMouse: Revert\nR: Restore Mesh" + (!ss.SS_RecordHistory ? "" : "\nZ: Undo"), MessageType.Info);
                pve();
            }

            ps(10);

            pl("Multithreading Settings", true);
            pv();
            ppDrawProperty("SS_MultithreadingSupported", "Multithreading Supported", "If enabled, the sculpting system will be ready for advanced & complex meshes. See more 'Multithreading' in docs.");
            if (ss.SS_MultithreadingSupported)
                ppDrawProperty("SS_MultithreadingProcessDelay", "Process Delay [millisec]", "Multithreading process delay [default: 10] - the bigger number is, the smoother result should be.");
            if (ss.SS_MultithreadingSupported)
            {
                if (GUILayout.Button(new GUIContent("Fix Brush Incorrection", "If the multithreading method is enabled, sculpting brush might be in another scale-ratio. This could be fixed after clicking this button. If not, you must adjust brush scale by yourself [Create empty object, group empty object with target brush graphic and assign empty object to the Brush Projection].")))
                {
                    if (EditorUtility.DisplayDialog("Info", "You can choose between 2 methods\n1. Would you like to reset the mesh matrix transform? This will set the current transform scale to ONE, but it could take more than 1 minute.\n\n2. Adjust child brush scale if it's possible. This will adjust child brush scale to 0.1.", "I'll choose 1", "I'll choose 2"))
                        ss.SS_Funct_BakeMesh();
                    else
                    {
                        if (ss.SS_BrushProjection.transform.childCount > 0)
                        {
                            foreach (Transform t in ss.SS_BrushProjection.transform)
                                t.transform.localScale = Vector3.one * 0.1f;
                        }
                    }
                }
            }
            pve();

            ps(10);

            pl("Brush Settings", true);
            pv();
            ppDrawProperty("SS_UseBrushProjection", "Show Brush Projection");
            ppDrawProperty("SS_BrushProjection", "Brush Projection");
            ps(5);
            pv();
            ppDrawProperty("SS_BrushSize", "Brush Size");
            ppDrawProperty("SS_BrushStrength", "Brush Strength");
            pve();
            ps(5);
            ppDrawProperty("SS_State", "Brush State", "Current brush state. The field is visible in case of keeping the only ONE brush state");

            pve();

            ps(10);

            pl("Sculpting Settings", true);
            pv();
            ppDrawProperty("SS_MeshSculptMode", "Sculpt Mode");
			ppDrawProperty("SS_SculptingLayerMask", "Sculpting Layer Mask");
            if (ss.SS_MeshSculptMode == MDM_SculptingPro.SS_MeshSculptMode_Internal.CustomDirection)
                ppDrawProperty("SS_CustomDirection", "Custom Direction", "Choose a custom direction in world space");
            else if (ss.SS_MeshSculptMode == MDM_SculptingPro.SS_MeshSculptMode_Internal.CustomDirectionObject)
            {
                ppDrawProperty("SS_CustomDirectionObject", "Custom Direction Object");
                ppDrawProperty("SS_CustomDirObjDirection", "Direction Towards Object", "Choose a direction of the included object above in local space");
            }
            ps(5);
            pv();
            ppDrawProperty("SS_EnableHeightLimitations", "Height Limitations", "If enabled, you will be able to set vertices Y Limit [height] in world space (great for planar terrains)");
            if (ss.SS_EnableHeightLimitations)
                ppDrawProperty("SS_HeightLimitations", "Height Limitations", "Minimum[X] and Maximum[Y] height limitation in world space");
            pve();
            pv();
            ppDrawProperty("SS_EnableDistanceLimitations", "Distance Limitations", "If enabled, you will be able to limit the vertices sculpting range in both inside depth and outside depth");
            if (ss.SS_EnableDistanceLimitations) ppDrawProperty("SS_DistanceLimitation", "Distance Limitation", "How far can the vertice go?");
            pve();
            ps(5);
            ppDrawProperty("SS_SculptingType", "Sculpting Type", "Choose a sculpting type.");
            pve();

            if (ss.SS_AtRuntime)
            {
                ps(10);

                pl("Input & Feature Settings", true);
                if (!ss.SS_MobileSupport)
                {
                    pv();
                    ppDrawProperty("SS_UseInput", "Use Input", "Use custom sculpt input controls. Otherwise, you can use internal API functions to interact the mesh sculpt.");
                    if (ss.SS_UseInput)
                    {
                        ppDrawProperty("SS_VRInput", "Is VR Input", "If enabled, the Sculpting Pro will be ready for simple sculpting in VR");
                        if (!ss.SS_VRInput)
                        {
                            pv();
                            ppDrawProperty("SS_UseRaiseFunct", "Use Raise", "Use Raise sculpting function");
                            ppDrawProperty("SS_UseLowerFunct", "Use Lower", "Use Lower sculpting function");
                            ppDrawProperty("SS_UseRevertFunct", "Use Revert", "Use Revert sculpting function");
                            ppDrawProperty("SS_UseNoiseFunct", "Use Noise", "Use Noise sculpting function");
                            if (ss.SS_UseNoiseFunct)
                            {
                                pv();
                                ppDrawProperty("SS_NoiseFunctionDirections", "Noise Direction", "Choose a noise direction in world space");
                                pve();
                            }
                            ppDrawProperty("SS_UseSmoothFunct", "Use Smooth", "Use Smooth sculpting function");
                            if (ss.SS_UseSmoothFunct)
                            {
                                pv();
                                ppDrawProperty("SS_smoothType", "Smoothing Type", "Choose between two smoothing types... HCfilter is less problematic, but takes more time to process. Laplacian is more problematic, but takes much less time. In general, the HCfilter is recommended for spatial meshes, the Laplacian for planar meshes.");
                                if (ss.SS_smoothType == MDM_SculptingPro.SS_SmoothType.LaplacianFilter) ppDrawProperty("SS_SmoothIntensity", "Smooth Intensity");
                                pve();
                            }
                            ppDrawProperty("SS_UseStylizeFunct", "Use Stylize", "Use Stylize sculpting function");
                            if (ss.SS_UseStylizeFunct)
                            {
                                pv();
                                ppDrawProperty("SS_StylizeIntensity", "Stylize Intensity");
                                pve();
                            }

                            pve();

                            if ((ss.SS_UseSmoothFunct || ss.SS_UseStylizeFunct) && !ss.SS_MultithreadingSupported)
                                phb("The Smooth & Stylize functions are not supported for non-multithreaded modifiers. Please enable Multithreading to make the Smooth or Stylize function work.", MessageType.Warning);

                            ps(2);
                            if (ss.SS_UseRaiseFunct)
                                ppDrawProperty("SS_SculptingRaiseInput", "Raise Input");
                            if (ss.SS_UseLowerFunct)
                                ppDrawProperty("SS_SculptingLowerInput", "Lower Input");
                            if (ss.SS_UseRevertFunct)
                                ppDrawProperty("SS_SculptingRevertInput", "Revert Input");
                            if (ss.SS_UseNoiseFunct)
                                ppDrawProperty("SS_SculptingNoiseInput", "Noise Input");
                            if (ss.SS_UseSmoothFunct)
                                ppDrawProperty("SS_SculptingSmoothInput", "Smooth Input");
                            if (ss.SS_UseStylizeFunct)
                                ppDrawProperty("SS_SculptingStylizeInput", "Stylize Input");

                            ps();
                            ppDrawProperty("SS_SculptFromCursor", "Sculpt Origin Is Cursor", "If enabled, the raycast origin will be cursor");
                        }
                        else
                        {
                            phb("The Sculpting Pro is now ready for VR sculpting. You will need to create some events to switch between 'Brush States' such as Raise, Lower, Revert etc. Default state is 'Raise'. The target controller should contain specific MDInputVR component.");
                            pv();
                            pl("Noise advanced settings");
                            pv();
                            ppDrawProperty("SS_NoiseFunctionDirections", "Noise Direction", "Choose a noise direction in world space");
                            pve();
                            pl("Smooth advanced settings");
                            pv();
                            ppDrawProperty("SS_smoothType", "Smoothing Type", "Choose between two smoothing types... HCfilter is less problematic, but takes more time to process. Laplacian is more problematic, but takes much less time. In general, the HCfilter is recommended for spatial meshes, the Laplacian for planar meshes.");
                            if (ss.SS_smoothType == MDM_SculptingPro.SS_SmoothType.LaplacianFilter) ppDrawProperty("SS_SmoothIntensity", "Smooth Intensity");
                            pve();
                            pl("Stylize advanced settings");
                            pv();
                            ppDrawProperty("SS_StylizeIntensity", "Stylize Intensity");
                            pve();

                            pve();
                        }
                        if (ss.SS_SculptFromCursor && !ss.SS_VRInput)
                        {
                            ps(5);
                            pv();
                            ppDrawProperty("SS_MainCamera", "Main Camera Source", "Assign main camera to make the raycast work properly");
                            ppDrawProperty("SS_SculptingLayerMask", "Sculpting Layer Mask", "Set custom sculpting layer mask");
                            pve();
                        }
                        else
                            ppDrawProperty("SS_SculptOrigin", ss.SS_VRInput ? "Target Controller" : "Sculpt Origin Object", "The raycast origin - transform forward [If VR enabled, assign your target controller]");

                    }
                    pve();
                }
                else
                {
                    ps(3);
                    pv();
                    ppDrawProperty("SS_MainCamera", "Main Camera Source", "Assign main camera to make the raycast work properly");
                    ppDrawProperty("SS_SculptingLayerMask", "Sculpting Layer Mask", "Set custom sculpting layer mask");
                    pve();
                }
            }
            ps(15);
            ppAddMeshColliderRefresher(ss.gameObject);
            ppBackToMeshEditor(ss);
            ps();

            if (ss != null) serializedObject.Update();
        }

        private void OnSceneGUI()
        {
            if (ss.SS_AtRuntime) return;
            if (ss.SS_InEditMode == false) return;

            if ((ss.SS_BrushProjection == null || !ss.SS_InEditMode || ss.SS_AtRuntime) && ss.SS_MultithreadingSupported)
            {
                if (!Application.isPlaying)
                    ss.CheckForThread(false);
                return;
            }

            if (ss.SS_BrushProjection.GetComponent<Collider>())
                DestroyImmediate(ss.SS_BrushProjection.GetComponent<Collider>());

            if (!Application.isPlaying)
            {
                if (!ss.SS_MultithreadingSupported)
                    ss.CheckForThread(false);
            }

            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            RaycastHit hit;
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            Tools.current = Tool.None;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider == ss.transform.GetComponent<Collider>())
                {
                    if (!ss.SS_UseBrushProjection)
                    {
                        if (ss.SS_BrushProjection.activeInHierarchy)
                            ss.SS_BrushProjection.SetActive(false);
                    }
                    else
                        ss.SS_BrushProjection.SetActive(true);
                    ss.SS_BrushProjection.transform.position = hit.point;
                    ss.SS_BrushProjection.transform.rotation = Quaternion.FromToRotation(-Vector3.forward, hit.normal);
                    ss.SS_BrushProjection.transform.localScale = new Vector3(ss.SS_BrushSize, ss.SS_BrushSize, ss.SS_BrushSize);

                    switch (ss.SS_State)
                    {
                        case MDM_SculptingPro.SS_State_Internal.Raise:
                            ss.SS_Funct_DoSculpting(hit.point, ss.SS_BrushProjection.transform.forward, ss.SS_BrushSize, ss.SS_BrushStrength, MDM_SculptingPro.SS_State_Internal.Raise);
                            if (!ss.SS_UpdateColliderAfterRelease)
                                ss.SS_Funct_RefreshMeshCollider();
                            break;
                        case MDM_SculptingPro.SS_State_Internal.Lower:
                            ss.SS_Funct_DoSculpting(hit.point, ss.SS_BrushProjection.transform.forward, ss.SS_BrushSize, ss.SS_BrushStrength, MDM_SculptingPro.SS_State_Internal.Lower);
                            if (!ss.SS_UpdateColliderAfterRelease)
                                ss.SS_Funct_RefreshMeshCollider();
                            break;

                        case MDM_SculptingPro.SS_State_Internal.Revert:
                            ss.SS_Funct_DoSculpting(hit.point, ss.SS_BrushProjection.transform.forward, ss.SS_BrushSize, ss.SS_BrushStrength, MDM_SculptingPro.SS_State_Internal.Revert);
                            if (!ss.SS_UpdateColliderAfterRelease)
                                ss.SS_Funct_RefreshMeshCollider();
                            break;
                    }
                }
                else
                    ss.SS_BrushProjection.SetActive(false);
            }
            else
                ss.SS_BrushProjection.SetActive(false);

            #region Editor Hotkeys
            if (Application.isPlaying)
                return;
            if (ss.SS_InEditMode)
            {
                //---Mouse
                if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && !Event.current.alt)
                {
                    ss.SS_Funct_RecordOnControlsDown();
                    if (!Event.current.control)
                    {
                        if (!Event.current.shift)
                            ss.SS_State = MDM_SculptingPro.SS_State_Internal.Raise;
                        else
                            ss.SS_State = MDM_SculptingPro.SS_State_Internal.Lower;
                    }
                    else
                        ss.SS_State = MDM_SculptingPro.SS_State_Internal.Revert;
                }
                else if (Event.current.type == EventType.MouseUp && Event.current.button == 0)
                {
                    ss.SS_State = MDM_SculptingPro.SS_State_Internal.None;
                    ss.SS_Funct_RecordOnControlsUp();

                    if (!Application.isPlaying)
                    {
                        if (ss.SS_MultithreadingSupported)
                            ss.CheckForThread();
                    }
                }

                //---Keys
                if (Event.current.type == EventType.KeyDown)
                {
                    switch (Event.current.keyCode)
                    {
                        case KeyCode.R:
                            ss.SS_Funct_RestoreOriginal();
                            break;
                        case KeyCode.Z:
                            ss.SS_Funct_Undo();
                            break;
                    }
                }
            }

            #endregion
        }
    }
}