using UnityEditor;
using MD_Plugin;

namespace MD_PluginEditor
{
    [CustomEditor(typeof(MD_MeshEditorRuntimeVR))]
    [CanEditMultipleObjects]
    public class MD_MeshEditorRuntimeVR_Editor : MD_EditorUtilities
    {
        public override void OnInspectorGUI()
        {
            MD_MeshEditorRuntimeVR m = (MD_MeshEditorRuntimeVR)target;

            ps();
            phb("Mesh Editor Runtime VR should be added to one of the VR controllers");
            phb("Add proper MDInputVR component to customize VR input for specific platform");

            ps();

            pv();
            pv();
            ppDrawProperty("ppVertexControlMode", "Vertex Control Mode", "Choose one of the feature modes of the Mesh Editor at runtime");
            if(m.ppVertexControlMode != MD_MeshEditorRuntimeVR.VertexControlMode.GrabDropVertex)
            {
                pv();
                ppDrawProperty("ppPullPushVertexSpeed", "Motion Speed", "Pull/Push effect speed", default);
                ppDrawProperty("ppPullPushType", "Motion Type", "Select one of the motion types of Pull/Push effect", default);
                pve();
                ps(3);
                if (m.ppVertexControlMode == MD_MeshEditorRuntimeVR.VertexControlMode.PullVertex)
                    ppDrawProperty("ppMaxMinPullPushDistance", "Minimum Distance", "How close can the points be?", default, true);
                else
                    ppDrawProperty("ppMaxMinPullPushDistance", "Maximum Distance", "How far can the points go?", default, true);
                ps(3);
                ppDrawProperty("ppContinuousPullPushDetection", "Continuous Detection", "If enabled, the potential points will be refreshed every frame", default, true);
            }
            pve();
            ps();
            pl("Vertex Selection Appearance", true);
            pv();
            ppDrawProperty("ppSwitchAppearance", "Use Appearance Feature", "If enabled, you will be able to customize vertex appearance");
            if (m.ppSwitchAppearance)
            {
                ppDrawProperty("ppCustomMaterial", "Use Custom Material", "If enabled, you will be able to use custom material instance instead of color");
                if (m.ppCustomMaterial)
                {
                    ppDrawProperty("ppTargetMaterial", "Target Material", "Target material when selected", default, true);
                    ppDrawProperty("ppInitialMaterial", "Initial Material", "Original material", default, true);
                }
                else
                {
                    ppDrawProperty("ppToColor", "Change To Color", "Target color if system catches potential vertexes", default, true);
                    ppDrawProperty("ppFromColor", "Change From Color", "Initial original color if system releases potential vertexes", default, true);
                }
            }
            pve();
            ps();
            pl("Conditions", true);
            pv();
            ppDrawProperty("ppAllowSpecificPoints", "Allow Specific Points", "If disabled, all points will be interactive");
            if(m.ppAllowSpecificPoints)
                ppDrawProperty("ppAllowedTag", "Allowed Tag", default, default, true);
            pve();
            ps();
            pl("Editor Method", true);
            pv();
            ppDrawProperty("ppUseRaycasting", "Use Raycasting", "If enabled, the system will use the raycasting technique [more precise], otherwise the system will use trigger system [less precise]");
            if (m.ppUseRaycasting)
            {
                pv();
                ppDrawProperty("ppAllowedLayerMask", "Allowed Layer Masks", default, default, true);
                ppDrawProperty("ppRaycastDistance", "Raycast Distance", default, default, true);
                ppDrawProperty("ppRaycastRadius", "Raycast Radius", default, default, true);
                pve();
                ppDrawProperty("ppAllowBackfaces", "Allow Backfaces", "Allow points behind the point of view", default, true);
            }
            else phb("The system is set to Trigger System editor method. The object should contain primitive collider checked to IsTrigger");
            pve();
            ps(15);
            ppDrawProperty("ppShowDebug", "Show Scene Debug", default, true);
            pve();

            serializedObject.Update();
        }
    }
}