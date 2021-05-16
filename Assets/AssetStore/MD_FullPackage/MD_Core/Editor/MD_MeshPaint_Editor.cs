using UnityEngine;
using UnityEditor;
using MD_Plugin;

namespace MD_PluginEditor
{
    [CustomEditor(typeof(MD_MeshPaint))]
    public class MD_MeshPaint_Editor : MD_EditorUtilities
    {
        public Texture IMG_BrushSettings;
        public Texture IMG_LogicSettings;
        public Texture IMG_AppearanceSettings;

        public override void OnInspectorGUI()
        {
            MD_MeshPaint mp = (MD_MeshPaint)target;

            ps();

            pv();
            ppDrawProperty("MP_Platform", "Target Platform", "Please choose one of the target platforms");

            if (mp.MP_Platform == MD_MeshPaint.MP_PlatformInternal.PC)
            {
                ps(5);
                pv();
                ppDrawProperty("MP_INPUT_PC_MeshPaintInput", "Paint Input");
                pve();
            }
            else if (mp.MP_Platform == MD_MeshPaint.MP_PlatformInternal.VR)
            {
                ps(5);
                phb("Add proper MDInputVR component to customize VR input for specific platform");
            }

            pve();

            ps(10);

            pl(IMG_BrushSettings);
            pv();
            ppDrawProperty("MP_BrushSize", "Brush Size");

            ps(5);
            pv();
            ppDrawProperty("MP_SmoothBrushMovement", "Brush Smooth Movement");
            if (mp.MP_SmoothBrushMovement)
                ppDrawProperty("MP_BSmoothMSpeed", "Smooth Movement Speed");
            ppDrawProperty("MP_SmoothBrushRotation", "Brush Smooth Rotation");
            if (mp.MP_SmoothBrushRotation)
                ppDrawProperty("MP_BSmoothRSpeed", "Smooth Rotation Speed");
            pve();
            ps(5);

            ppDrawProperty("MP_DistanceLimitation", "Distance Limitation", "If enabled, mesh will be refreshed & created after some values typed below");
            if (mp.MP_DistanceLimitation)
                ppDrawProperty("MP_MinDistanceLimit", "Minimal Distance Limit", "Minimal distance limit - how smooth will be the mesh");

            ps(5);

            ppDrawProperty("MP_ConnectMeshOnRelease", "Connect Mesh On Release", "Created mesh will be connected on release");

            ps(5);

            ppDrawProperty("MP_RotationMode", "Brush Rotation Mode", "Choose one of the rotation modes. Each mode is unique. One Axis - better for 2D drawing, Spatial Axis - better for 3D drawing");
            if (mp.MP_RotationMode == MD_MeshPaint.MP_RotationModeInternal.FollowOneAxis)
                ppDrawProperty("MP_RotationmodeOffset", "Rotation Offset", "Additional rotation parameters [default: 0 0 1 = FORWARD]");

            ps(5);
            pv();
            ppDrawProperty("MP_ShapeType", "Shape Type", "Choose one of the shapes to draw");
            pve();
            pve();

            ps(10);

            pl(IMG_LogicSettings);
            pv();
            ppDrawProperty("MP_MeshPaintType", "Mesh Painting Type", "Choose one of the mesh painting types.");
            if (mp.MP_MeshPaintType == MD_MeshPaint.MP_MeshPaintTypeInternal.DrawOnScreen)
            {
                pl("Type: On Screen");
                pv();
                ppDrawProperty("MP_TypeScreen_UseMainCamera", "Use MainCamera", "If enabled, script will find Camera.main object [camera with tag MainCamera]. Otherwise you can choose your own camera");
                if (!mp.MP_TypeScreen_UseMainCamera)
                    ppDrawProperty("MP_TypeScreen_TargetCamera", "Target Camera");
                ppDrawProperty("MP_TypeScreen_Depth", "Painting Depth", "Z Value [distance from camera]");
                pve();
            }
            else if (mp.MP_MeshPaintType == MD_MeshPaint.MP_MeshPaintTypeInternal.DrawOnRaycastHit)
            {
                pl("Type: On Raycast Hit");
                pv();

                ppDrawProperty("MP_TypeRaycast_AllowedLayers", "Allowed Layers");
                ppDrawProperty("MP_TypeRaycast_CastAllObjects", "Cast All Objects", "If enabled, all objects will receive raycast function");
                if (!mp.MP_TypeRaycast_CastAllObjects)
                    ppDrawProperty("MP_TypeRaycast_TagForRaycast", "Tag For Raycast Objects");

                ps(5);

                ppDrawProperty("MP_TypeRaycast_RaycastFromCursor", "Raycast From Cursor", "If enabled, raycast origin will be set to cursor");
                if (!mp.MP_TypeRaycast_RaycastFromCursor)
                    ppDrawProperty("MP_TypeRaycast_RaycastOriginFORWARD", "Raycast Origin [FORWARD Direction]", "Assign target direction for raycast [raycast direction will be this object FORWARD]");

                ps(5);

                ppDrawProperty("MP_TypeRaycast_BrushOffset", "Brush Offset");
                ppDrawProperty("MP_TypeRaycast_IgnoreSelfCasting", "Ignore Self Casting", "If enabled, raycast will ignore painted meshes");

                pve();
            }
            else if (mp.MP_MeshPaintType == MD_MeshPaint.MP_MeshPaintTypeInternal.CustomDraw)
            {
                pl("Type: Custom");
                pv();

                ppDrawProperty("MP_TypeCustom_DRAW", "PAINT", "If enabled, the script will start painting the mesh");
                ppDrawProperty("MP_TypeCustom_CustomBrushTransform", "Customize Brush Transform", "If enabled, you will be able to customize brush parent and its rotation behaviour");
                if (mp.MP_TypeCustom_CustomBrushTransform)
                {
                    ppDrawProperty("MP_TypeCustom_EnableSmartRotation", "Smart Rotation", "If enabled, smart rotation will be allowed - brush will rotate to the direction of its movement");
                    GUI.color = Color.gray;
                    ppDrawProperty("MP_TypeCustom_BrushParent", "Brush Parent", "If you won't to parent brush, leave it empty. If the parent is assigned, brush will be automatically set to ZERO local position");
                    GUI.color = Color.white;
                }

                pve();
            }
            pve();

            ps(10);

            pl(IMG_AppearanceSettings);
            pv();
            ppDrawProperty("MP_CurrentlySelectedAppearanceSlot", "Appearance Index", "Index of the selected appearance slot - Material/ Color [according on the boolean below]");
            ppDrawProperty("MP_MaterialSlots", "Material Slots", "If enabled, color slots will be hidden");
            if (mp.MP_MaterialSlots)
                ppDrawProperty("MP_Color_AvailableMaterials", "Available Materials", "", true);
            else
                ppDrawProperty("MP_Color_AvailableColors", "Available Colors", "", true);

            ps(5);

            ppDrawProperty("MP_RefreshMeshCollider", "Add & Refresh Mesh Collider");

            pve();

            ps(10);

            pl("Additional");
            pv();
            ppDrawProperty("MP_FollowBrushTransform", "Custom Brush Transform", "If enabled, you can assign your own custom brush to follow hidden brush");
            if (mp.MP_FollowBrushTransform)
            {
                ppDrawProperty("MP_ObjectForFollowing", "Custom Brush", "Custom brush that will follow hidden brush");
                if (mp.MP_MeshPaintType == MD_MeshPaint.MP_MeshPaintTypeInternal.DrawOnRaycastHit)
                    ppDrawProperty("MP_HideCustomBrushIfNotRaycasting", "Hide Brush if not Raycasting", "Custom brush will be hidden if there is no raycast hit available");
            }

            pve();

            ps(10);

            pl("Presets");
            if (pb("2D Ready Preset"))
                SetTo2D();
            if (pb("3D Ready Preset"))
                SetTo3D();
            if (pb("VR Ready Preset"))
                SetTo3D(true);
            serializedObject.Update();
        }

        private void SetTo2D()
        {
            MD_MeshPaint mp = (MD_MeshPaint)target;

            mp.MP_RotationMode = MD_MeshPaint.MP_RotationModeInternal.FollowOneAxis;
            mp.MP_MeshPaintType = MD_MeshPaint.MP_MeshPaintTypeInternal.DrawOnScreen;
            mp.MP_TypeScreen_UseMainCamera = true;
            mp.MP_TypeScreen_Depth = 10;

            mp.MP_CurrentlySelectedAppearanceSlot = 0;
            mp.MP_MaterialSlots = false;
            mp.MP_Color_AvailableColors = new Color[] { Color.white, Color.black, Color.blue, Color.red, Color.yellow, Color.green, Color.cyan };

            mp.MP_RefreshMeshCollider = false;
        }
        private void SetTo3D(bool vr = false)
        {
            MD_MeshPaint mp = (MD_MeshPaint)target;

            if (vr) mp.MP_Platform = MD_MeshPaint.MP_PlatformInternal.VR;
            mp.MP_RotationMode = MD_MeshPaint.MP_RotationModeInternal.FollowSpatialAxis;
            mp.MP_MeshPaintType = vr ? MD_MeshPaint.MP_MeshPaintTypeInternal.CustomDraw : MD_MeshPaint.MP_MeshPaintTypeInternal.DrawOnRaycastHit;
            mp.MP_TypeRaycast_AllowedLayers = -1;
            mp.MP_TypeRaycast_CastAllObjects = true;
            mp.MP_TypeRaycast_RaycastFromCursor = true;
            mp.MP_TypeRaycast_BrushOffset = new Vector3(0, 0.2f, 0);
            mp.MP_TypeRaycast_IgnoreSelfCasting = true;

            mp.MP_CurrentlySelectedAppearanceSlot = 0;
            mp.MP_MaterialSlots = false;
            mp.MP_Color_AvailableColors = new Color[] { Color.white, Color.black, Color.blue, Color.red, Color.yellow, Color.green, Color.cyan };

            mp.MP_RefreshMeshCollider = true;
        }
    }
}