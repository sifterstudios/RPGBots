using UnityEngine;
using UnityEditor;
using MD_Plugin;

namespace MD_PluginEditor
{
    [CustomEditor(typeof(MD_MeshEditorRuntime))]
    [CanEditMultipleObjects]
    public class MD_MeshEditorRuntime_Editor : MD_EditorUtilities
    {
        public override void OnInspectorGUI()
        {
            MD_MeshEditorRuntime m = (MD_MeshEditorRuntime)target;

            ps();

            pv();
            ppDrawProperty("ppMainCam", "Main Camera", "Assign main camera in the scene that will represent 'Origin'");
            pve();

            pv();
            ppDrawProperty("ppAXIS_EDITOR", "Axis Editor Mode", "If enabled, the script will be set to the AXIS EDITOR");
            pve();

            if (m.ppAXIS_EDITOR)
            {
                phb("Axis editor works for PC platform only.");

                ps(10);

                pv();
                ppDrawProperty("ppAXIS_TargetObject", "Target Object", "Required target object to edit");
                ps(5);
                ppDrawProperty("ppAXIS_AxisObject", "Editor Axis Object", "Required 'Movable' Axis object for Axis Editor");
                GUIStyle st = new GUIStyle();
                st.normal.textColor = Color.gray;
                st.richText = true;
                st.fontSize = 10;
                st.fontStyle = FontStyle.Italic;
                if (m.ppAXIS_AxisObject != null)
                    GUILayout.Label("Required axis child naming: <color=red>AXIS_X</color> - <color=lime>AXIS_Y</color> - <color=cyan>AXIS_Z</color>", st);
                else
                {
                    if (pb("Create Axis Object Automatically"))
                    {
                        GameObject AxisRoot = new GameObject("AxisObject_Root");
                        AxisRoot.transform.position = Vector3.zero;
                        AxisRoot.transform.rotation = Quaternion.identity;

                        GameObject X_Axis = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        GameObject Y_Axis = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        GameObject Z_Axis = GameObject.CreatePrimitive(PrimitiveType.Cube);

                        X_Axis.name = "AXIS_X";
                        Y_Axis.name = "AXIS_Y";
                        Z_Axis.name = "AXIS_Z";

                        X_Axis.transform.parent = AxisRoot.transform;
                        Y_Axis.transform.parent = AxisRoot.transform;
                        Z_Axis.transform.parent = AxisRoot.transform;

                        X_Axis.transform.localPosition = new Vector3(0.6f, 0, 0);
                        X_Axis.transform.localRotation = Quaternion.Euler(-90, 0, -90);
                        X_Axis.transform.localScale = new Vector3(0.15f, 1, 0.15f);

                        Y_Axis.transform.localPosition = new Vector3(0, 0.6f, 0);
                        Y_Axis.transform.localRotation = Quaternion.Euler(0, 90, 0);
                        Y_Axis.transform.localScale = new Vector3(0.15f, 1, 0.15f);

                        Z_Axis.transform.localPosition = new Vector3(0, 0, -0.6f);
                        Z_Axis.transform.localRotation = Quaternion.Euler(-90, 0, 0);
                        Z_Axis.transform.localScale = new Vector3(0.15f, 1, 0.15f);

                        Material mat1 = new Material(Shader.Find("Diffuse"));
                        mat1.color = Color.red;
                        X_Axis.GetComponent<Renderer>().material = mat1;
                        Material mat2 = new Material(Shader.Find("Diffuse"));
                        mat2.color = Color.green;
                        Y_Axis.GetComponent<Renderer>().material = mat2;
                        Material mat3 = new Material(Shader.Find("Diffuse"));
                        mat3.color = Color.blue;
                        Z_Axis.GetComponent<Renderer>().material = mat3;

                        m.ppAXIS_AxisObject = AxisRoot;
                        return;
                    }
                }

                pve();

                ps(10);

                pv();

                ppDrawProperty("ppAXIS_SelectionInput", "Selection Input");
                ps(3);
                ppDrawProperty("ppAXIS_AddPointsInput", "Add Input", "If you have selected points, you can add more points from selection by holding this input and holding the selection input.");
                ppDrawProperty("ppAXIS_RemovePointsInput", "Remove Input", "If you have selected points, you can remove points from selection by holding this input and holding the selection input.");
                ps(5);
                ppDrawProperty("ppAXIS_LocalSpace", "Local Space", "Axis object orientation");
                ppDrawProperty("ppAXIS_Speed", "Move Speed", "Axis Object move speed");
                ppDrawProperty("ppAXIS_SelectedPointColor", "Selection Color");
                ppDrawProperty("ppAXIS_SelectionGridColor", "Selection Grid Color");

                pve();

                serializedObject.Update();
                return;
            }

            ps(10);

            ppDrawProperty("ppVertexControlMode", "Editor Control Mode", "Choose a control mode for editor at runtime");
            if (m.ppVertexControlMode != MD_MeshEditorRuntime.VertexControlMode.GrabDropVertex)
            {
                pv();
                ppDrawProperty("ppPullPushVertexSpeed", "Motion Speed", "Pull/Push effect speed", default);
                ppDrawProperty("ppPullPushType", "Motion Type", "Select one of the motion types of Pull/Push effect", default);
                ps(3);
                if (m.ppVertexControlMode == MD_MeshEditorRuntime.VertexControlMode.PullVertex)
                    ppDrawProperty("ppMaxMinPullPushDistance", "Minimum Distance", "How close can the points be?", default);
                else
                    ppDrawProperty("ppMaxMinPullPushDistance", "Maximum Distance", "How far can the points go?", default);
                ps(3);
                ppDrawProperty("ppContinuousPullPushDetection", "Continuous Detection", "If enabled, the potential points will be refreshed every frame", default);
                pve();
            }
            ppDrawProperty("ppMobileSupported", "Mobile Support", "If enabled, the system will be ready for mobile devices");
            serializedObject.ApplyModifiedProperties();

            ps(5);

            GUI.color = Color.white;

            if (m.ppVertexControlMode == MD_MeshEditorRuntime.VertexControlMode.GrabDropVertex)
            {
                pv();
                pl("Locks", true);
                ppDrawProperty("ppLockAxis_X", "Lock X Axis", "If the axis is locked, selected point won't be able to move in the axis direction",default,true);
                ppDrawProperty("ppLockAxis_Y", "Lock Y Axis", "If the axis is locked, selected point won't be able to move in the axis direction", default, true);
                pve();
            }

            ps();

            pl("Vertex Selection Appearance", true);
            pv();
            ppDrawProperty("ppSwitchAppearance", "Use Appearance Feature", "If enabled, you will be able to customize vertex appearance");
            if (m.ppSwitchAppearance)
            {
                ppDrawProperty("ppCustomMaterial", "Use Custom Material", "If enabled, you will be able to use custom material instance instead of color");
                if (m.ppCustomMaterial)
                    ppDrawProperty("ppTargetMaterial", "Material Instance", default, default, true);
                else
                    ppDrawProperty("ppToColor", "Change To Color", "Target color if system catches potential vertexes", default, true);
            }
            pve();
            ps();

            pv();

            if (!m.ppMobileSupported)
            {
                pl("Controls", true);
                pv();
                ppDrawProperty("ppPCInput", "Control Input", "Enter input key for vertex selection");
                pve();
                ps(5);
            }

            pl("Conditions", true);
            pv();
            ppDrawProperty("ppAllowSpecificPoints", "Raycast Specific Points", "If enabled, the raycast will allow only colliders with tag below");
            if (m.ppAllowSpecificPoints)
                ppDrawProperty("ppAllowedTag", "Allowed Tag", "Specific allowed tag for raycast", default, true);

            if (!m.ppMobileSupported)
            {
                ppDrawProperty("ppCursorIsOrigin", "Raycast from Cursor", "If disabled, the raycast origin will be the transforms position [direction = transform.forward]");
                if (!m.ppCursorIsOrigin)
                    ppDrawProperty("ppLockAndHideCursor", "Hide & Lock Cursor", default, default, true);
                ps(5);
            }
            pve();

            pl("Raycast Settings", true);
            pv();
            pv();
            ppDrawProperty("ppAllowedLayerMask", "Allowed Layer Masks", default, default);
            ppDrawProperty("ppRaycastDistance", "Raycast Distance", default, default);
            ppDrawProperty("ppRaycastRadius", "Raycast Radius", default, default);
            pve();
            ppDrawProperty("ppAllowBackfaces", "Allow Backfaces", "Allow points behind the point of view", default);
            pve();

            pve();

            ps(5);

            ppDrawProperty("ppShowDebug", "Show Scene Debug", default, true);

            serializedObject.Update();
        }
    }
}