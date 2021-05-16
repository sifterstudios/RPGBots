#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace MD_Plugin
{
    public class MD_TunnelCreatorEditorWindow : EditorWindow
    {
        public static void Init(MD_TunnelCreator tcc)
        {
            MD_TunnelCreatorEditorWindow newWin = (MD_TunnelCreatorEditorWindow)GetWindow(typeof(MD_TunnelCreatorEditorWindow));
            newWin.minSize = new Vector2(400, 500);
            newWin.maxSize = new Vector2(500, 600);
            newWin.titleContent = new GUIContent("Tunnel Creator Editor");
            tc = tcc;
            newWin.Show();
        }

        public static MD_TunnelCreator tc;

        public Texture2D tc_ICON_Add;
        public Texture2D tc_ICON_Rem;

        private GameObject obj2;
        private GameObject obj1;
        private GameObject obj_UngroupTo;

        private bool groupAfterCreation = true;

        private float value0 = 0.5f;

        private int selectedEditorFeature = 0;

        private void OnGUI()
        {
            EditorGUIUtility.labelWidth -= 40;

            s(20);
            l("Nodes Creation");
            bh();
            if (b("Add Node",0, tc_ICON_Add))
                tc.PUBLICtFunct_AddNode(Vector3.zero, groupAfterCreation);
            if (b("Remove Node",0, tc_ICON_Rem))
                tc.PUBLICtFunct_RemoveNode();
            eh();
            groupAfterCreation = GUILayout.Toggle(groupAfterCreation, "Group new node to the last added node on Add");

            s(15);

            l("Nodes Management");
            bv();
            bh();
            if (b("Group All Together") && tc.tNodes.Count > 0)
                tc.PUBLICtFunct_GroupNodesTogether();
            if (b("Ungroup All") && tc.tNodes.Count > 0)
                tc.PUBLICtFunct_UngroupNodes((obj_UngroupTo) ? obj_UngroupTo.transform : null);
            eh();
            obj_UngroupTo = EditorGUILayout.ObjectField(new GUIContent("Ungroup To", "Leave this field empty if the parent will be null"), (Object)obj_UngroupTo, typeof(GameObject), true) as GameObject;

            s(25);

            l("Nodes Editor Features");
            if (tc.tNodes.Count < 4)
            {
                EditorGUILayout.HelpBox("To access nodes editor features, there must be more than 3 nodes...", MessageType.Warning);
                ev();
                return;
            }
            bv();
            bh();
            if (b("Make Turn") && tc.tNodes.Count > 0)
            {
                selectedEditorFeature = 1;
                value0 = 0f;
            }
            if (b("Make Straight Line") && tc.tNodes.Count > 0)
            {
                selectedEditorFeature = 2;
                value0 = 2.5f;
            }
            if (b("Connect Tunnel") && tc.tNodes.Count > 0)
                selectedEditorFeature = 3;
            eh();
            s();

            switch (selectedEditorFeature)
            {
                case 1:
                    value0 = EditorGUILayout.Slider(new GUIContent("Turn Value"), value0, -5f, 5f);
                    bh();
                    EditorGUIUtility.labelWidth -= 15;
                    obj2 = EditorGUILayout.ObjectField(new GUIContent("From Node"), (Object)obj2, typeof(GameObject), true) as GameObject;
                    obj1 = EditorGUILayout.ObjectField(new GUIContent("To Node"), (Object)obj1, typeof(GameObject), true) as GameObject;
                    EditorGUIUtility.labelWidth += 15;
                    ev();
                    bh();
                    if (b("Assign Selected to From Node", 180) && Selection.activeGameObject)
                        obj2 = Selection.activeGameObject;
                    if (b("Assign Selected to To Node", 180) && Selection.activeGameObject)
                        obj1 = Selection.activeGameObject;
                    eh();
                    break;
                case 2:
                    value0 = EditorGUILayout.Slider(new GUIContent("Distance"), value0, 0.1f, 5f);
                    bh();
                    EditorGUIUtility.labelWidth -= 15;
                    obj2 = EditorGUILayout.ObjectField(new GUIContent("From Node"), (Object)obj2, typeof(GameObject), true) as GameObject;
                    obj1 = EditorGUILayout.ObjectField(new GUIContent("To Node"), (Object)obj1, typeof(GameObject), true) as GameObject;
                    EditorGUIUtility.labelWidth += 15;
                    ev();
                    bh();
                    if (b("Assign Selected to From Node", 180) && Selection.activeGameObject)
                        obj2 = Selection.activeGameObject;
                    if (b("Assign Selected to To Node", 180) && Selection.activeGameObject)
                        obj1 = Selection.activeGameObject;
                    eh();
                    break;
                case 3:
                    bh();
                    EditorGUIUtility.labelWidth += 20;
                    obj2 = EditorGUILayout.ObjectField(new GUIContent("Ending Node"), (Object)obj2, typeof(GameObject), true) as GameObject;
                    obj1 = EditorGUILayout.ObjectField(new GUIContent("Starting Node"), (Object)obj1, typeof(GameObject), true) as GameObject;
                    EditorGUIUtility.labelWidth -= 20;
                    ev();
                    break;
            }
            if (selectedEditorFeature != 0)
                if (b("Apply Editor Feature"))
                    ProcessEditorFeature();
            ev();

            ev();
            s(10);

            EditorGUIUtility.labelWidth += 40;
        }

        private void ProcessEditorFeature()
        {
            float val0 = 0;
            int iFrom = -1;
            int iTo = 0;
            int c = 0;
            Transform referen;

            if (obj1 == null || obj2 == null)
            {
                Debug.LogError("MDM_TunnelCreatorEditorWindow - From Node or To Node object field is null!");
                return;
            }

            switch (selectedEditorFeature)
            {
                case 1:  //---Make Turn
                    for (int i = 0; i < tc.tNodes.Count; i++)
                    {
                        if (obj2 == tc.tNodes[i].gameObject && iFrom == -1)
                            iFrom = i + 1;
                        else if (iFrom != -1)
                            c++;
                        if (obj1 == tc.tNodes[i].gameObject && iTo == 0)
                        {
                            iTo = i;
                            break;
                        }
                    }
                    referen = tc.tNodes[iFrom];
                    for (int i = iFrom + 1; i <= iTo; i++)
                    {
                        val0 += val0 + value0 / c;
                        tc.tNodes[i].transform.position += referen.right * val0;
                    }
                    break;


                case 2:  //---Make Straight Line
                    for (int i = 0; i < tc.tNodes.Count; i++)
                    {
                        if (obj2 == tc.tNodes[i].gameObject && iFrom == -1)
                            iFrom = i;
                        if (obj1 == tc.tNodes[i].gameObject && iTo == 0)
                            iTo = i;
                    }
                    referen = tc.tNodes[iFrom];
                    for (int i = iFrom + 1; i <= iTo; i++)
                    {
                        val0 += value0;
                        tc.tNodes[i].transform.position = referen.position + referen.forward * val0;
                        tc.tNodes[i].transform.rotation = Quaternion.identity;
                    }
                    break;

                case 3:  //---Connect Tunnel
                    obj2.transform.position = obj1.transform.position;
                    obj2.transform.rotation = obj1.transform.rotation;
                    break;
            }

        }

        private bool b(string txt, int size = 0, Texture2D icon = null)
        {
            if (size == 0)
                return GUILayout.Button(new GUIContent(txt,(icon!=null)?icon:null));
            else
                return GUILayout.Button(txt, GUILayout.Width(size));
        }
        private void l(string label)
        {
            GUILayout.Label(label);
        }
        private void bh()
        {
            GUILayout.BeginHorizontal("Box");
        }
        private void eh()
        {
            GUILayout.EndHorizontal();
        }
        private void bv()
        {
            GUILayout.BeginVertical("Box");
            EditorGUI.indentLevel++;
        }
        private void ev()
        {
            EditorGUI.indentLevel--;
            GUILayout.EndVertical();
        }

        private void s(float sp = 5)
        {
            GUILayout.Space(sp);
        }
    }
}
#endif