using System.Collections.Generic;
using UnityEngine;

namespace MD_Plugin
{
    /// <summary>
    /// MD(Mesh Deformation) Essential Component: Mesh Editor Runtime VR
    /// Essential component for general mesh-vertex-editing at runtime [VR]
    /// </summary>
    [AddComponentMenu(MD_Debug.ORGANISATION + MD_Debug.PACKAGENAME + "Mesh Editor Runtime VR")]
    public class MD_MeshEditorRuntimeVR : MonoBehaviour
    {
        // Runtime Editor Type - Point Manipulation
        public enum VertexControlMode { GrabDropVertex, PushVertex, PullVertex };
        public VertexControlMode ppVertexControlMode = VertexControlMode.GrabDropVertex;

        // Appearance
        public bool ppSwitchAppearance = true;
        public Color ppToColor = Color.green;
        public Color ppFromColor = Color.red;
        public bool ppCustomMaterial = false;
        public Material ppTargetMaterial;
        public Material ppInitialMaterial;

        // Pull-Push Settings
        public float ppPullPushVertexSpeed = 0.15f;
        public float ppMaxMinPullPushDistance = Mathf.Infinity;
        public bool ppContinuousPullPushDetection = false;
        public enum PullPushType { Radial, Directional };
        public PullPushType ppPullPushType = PullPushType.Directional;

        // Conditions
        public bool ppAllowSpecificPoints = false;
        public string ppAllowedTag;

        // Raycast
        public bool ppUseRaycasting = true;
        public bool ppAllowBackfaces = true;
        public LayerMask ppAllowedLayerMask = -1;
        public float ppRaycastDistance = 5.0f;
        public float ppRaycastRadius = 0.25f;

        // DEBUG
        public bool ppShowDebug = true;

        // VR Input - for debug purposes
        public bool INPUT_DOWN = false;

        private struct PotentialPoints
        {
            public Transform parent;
            public Transform point;
        }
        private List<PotentialPoints> potentialPoints = new List<PotentialPoints>();


        private void Start()
        {
            //It's required to have rigidbody while using Trigger version
            if(!ppUseRaycasting)
            {
                if (!GetComponent<Rigidbody>())
                    gameObject.AddComponent<Rigidbody>().isKinematic = true;
            }
        }

        private void Update()
        {
            //Process raycast editor if enabled
            if(ppUseRaycasting)
                InternalProcess_RaycastingRuntimeEditor();
            //Otherwise process trigger editor
            else
            {
                if (!VREditor_GetControlInput())
                {
                    if(INPUT_DOWN)
                    {
                        if(ppVertexControlMode == VertexControlMode.GrabDropVertex)
                            foreach(PotentialPoints tr in potentialPoints)
                                tr.point.parent = tr.parent;
                        INPUT_DOWN = false;
                    }
                    return;
                }
                //Reset all potential points if input is UP
                if (INPUT_DOWN)
                {
                    if (ppVertexControlMode != VertexControlMode.GrabDropVertex)
                        InternalProcess_ProcessPullPush();
                    return;
                }
                if (ppVertexControlMode == VertexControlMode.GrabDropVertex)
                    foreach (PotentialPoints tr in potentialPoints)
                        tr.point.parent = this.transform;
                INPUT_DOWN = true;
            }
        }

        private void OnDrawGizmos()
        {
            if (!ppShowDebug)       return;
            if (!ppUseRaycasting)   return;
            Gizmos.color = Color.white;
            Gizmos.DrawLine(transform.position, transform.position + transform.forward * ppRaycastDistance);
            Gizmos.DrawWireSphere(transform.position + transform.forward * ppRaycastDistance, ppRaycastRadius);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (ppUseRaycasting)    return;
            if (INPUT_DOWN)         return;

            if (ppAllowSpecificPoints)
                if (!other.CompareTag(ppAllowedTag))
                    return;
            if (other.transform.GetComponentInParent<MD_MeshProEditor>() == false) return;
            Renderer r = other.gameObject.GetComponent<Renderer>();
            PotentialPoints ppp = new PotentialPoints() { point = other.transform, parent = other.transform.parent };
            potentialPoints.Add(ppp);
            VREditor_ChangeMaterialToPoints(ppp, true);
        }

        private void OnTriggerExit(Collider other)
        {
            if (ppUseRaycasting) return;

            for (int i = potentialPoints.Count - 1; i >= 0; i--)
                if (other.transform == potentialPoints[i].point)
                {
                    VREditor_ChangeMaterialToPoints(potentialPoints[i], false);
                    potentialPoints.RemoveAt(i);
                }
        }

        private void InternalProcess_RaycastingRuntimeEditor()
        {
            //If input is pressed/down, process the runtime editor
            if (INPUT_DOWN && potentialPoints.Count > 0)
            {
                if (ppVertexControlMode != VertexControlMode.GrabDropVertex)
                {
                    InternalProcess_ProcessPullPush();
                    if (ppContinuousPullPushDetection) INPUT_DOWN = false;
                }

                //Check for input-UP
                if (!VREditor_GetControlInput())
                {
                    foreach (PotentialPoints tr in potentialPoints)
                        tr.point.parent = tr.parent;
                    INPUT_DOWN = false;
                }
                if (INPUT_DOWN) return;
            }

            //If input is up, raycast for potential points in sphere radius
            Ray ray = new Ray(transform.position, transform.forward);
            RaycastHit[] raycast = Physics.SphereCastAll(ray, ppRaycastRadius, ppRaycastDistance, ppAllowedLayerMask);

            //Reset a potential points list
            if(potentialPoints.Count > 0)
            {
                if (ppSwitchAppearance)
                    foreach (PotentialPoints tr in potentialPoints)
                        VREditor_ChangeMaterialToPoints(tr, false);
                potentialPoints.Clear();
            }

            if (raycast.Length == 0) return;

            //Declare a new potential points chain
            foreach (RaycastHit h in raycast)
            {
                if (!h.transform.GetComponentInParent<MD_MeshProEditor>())
                    continue;
                if (ppAllowSpecificPoints && !h.transform.CompareTag(ppAllowedTag))
                    continue;
                if(!ppAllowBackfaces && Vector3.Distance(transform.position + transform.forward * ppRaycastDistance, h.transform.position) > ppRaycastRadius)
                    continue;
                Renderer r = h.transform.gameObject.GetComponent<Renderer>();
                PotentialPoints ppp = new PotentialPoints() { point = h.transform, parent = h.transform.parent };
                potentialPoints.Add(ppp);
                VREditor_ChangeMaterialToPoints(ppp, true);
            }

            //Manage final control_down = if pressed, process the runtime editor next frame
            if (VREditor_GetControlInput())
            {
                foreach (PotentialPoints tr in potentialPoints)
                {
                    VREditor_ChangeMaterialToPoints(tr, false);
                    if (ppVertexControlMode == VertexControlMode.GrabDropVertex)
                        tr.point.parent = transform;
                }
                INPUT_DOWN = true;
            }
        }

        private void InternalProcess_ProcessPullPush()
        {
            foreach (PotentialPoints tr in potentialPoints)
            {
                Vector3 raycastPos = transform.position + (transform.forward * ppRaycastDistance);
                Vector3 tvector = ppPullPushType == PullPushType.Radial ? (tr.point.position - raycastPos) : transform.forward;
                float dist = (tr.point.position - raycastPos).magnitude;
                if (ppVertexControlMode == VertexControlMode.PushVertex && dist > ppMaxMinPullPushDistance)
                    continue;
                if (ppVertexControlMode == VertexControlMode.PullVertex && dist < ppMaxMinPullPushDistance && ppMaxMinPullPushDistance != Mathf.Infinity)
                    continue;
                tr.point.position += (ppVertexControlMode == VertexControlMode.PushVertex ? tvector : -tvector) * ppPullPushVertexSpeed * Time.deltaTime;
            }
        }

        private void VREditor_ChangeMaterialToPoints(PotentialPoints p, bool selected)
        {
            if (!ppSwitchAppearance)
                return;

            Renderer r = p.point.GetComponent<Renderer>();
            if (selected)
            {
                if (ppCustomMaterial)
                    r.material = ppTargetMaterial;
                else
                    r.material.color = ppToColor;
            }
            else
            {
                if (ppCustomMaterial)
                    r.material = ppInitialMaterial;
                else
                    r.material.color = ppFromColor;
            }
        }

        #region Available Public Methods

        /// <summary>
        /// Switch current control mode by index [1-Grab/Drop,2-Push,3-Pull]
        /// </summary>
        public void VREditor_SwitchControlMode(int index)
        {
            ppVertexControlMode = (VertexControlMode)index;
        }

        /// <summary>
        /// Get current built-in control VR input of the specified attributes
        /// </summary>
        /// <returns>returns true if pressed</returns>
        public bool VREditor_GetControlInput()
        {
            if (!Application.isPlaying) return false;

            return INPUT_DOWN_Secondary;
        }

        private bool INPUT_DOWN_Secondary = false;

        /// <summary>
        /// Set control input from 3rd party source (such as SteamVR, Oculus or other)
        /// </summary>
        /// <param name="setInputTo">Input down or up?</param>
        public void GlobalReceived_SetControlInput(bool setInputTo)
        {
            INPUT_DOWN_Secondary = setInputTo;
        }

        #endregion
    }
}
