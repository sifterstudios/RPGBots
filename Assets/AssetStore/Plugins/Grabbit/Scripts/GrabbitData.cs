#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Grabbit
{
    [ExecuteInEditMode]
    public class GrabbitData : MonoBehaviour
    {
        [HideInInspector] public List<int> ExistingCollidersLayers = new List<int>();
        [HideInInspector] public List<Collider> NonGrabbitColliders = new List<Collider>();
        [HideInInspector] public List<Collider> NonGrabbitCollidersStaticOnly = new List<Collider>();
        [HideInInspector] public List<MeshCollider> AddedStaticColliders = new List<MeshCollider>();
        [HideInInspector] public List<MeshCollider> AddedDynamicColliders = new List<MeshCollider>();

        [HideInInspector] public Bounds bounds;

        public bool HasExistingColliders => NonGrabbitColliders.Count > 0;
        public bool HasAddedStaticColliders => AddedStaticColliders.Count > 0;
        public bool HasAddedDynamicColliders => AddedDynamicColliders.Count > 0;
        [HideInInspector] public Rigidbody Body;
        [HideInInspector] public bool WasBodyAdded = false;
        [HideInInspector] public RbSaveState SaveState;
        [HideInInspector] public List<GrabbitData> CreatedSubDatas = new List<GrabbitData>();
        [HideInInspector] public List<GrabbitData> SubDatas = new List<GrabbitData>();
        [HideInInspector] [SerializeField] private List<FixedJoint> AddedJoints = new List<FixedJoint>();
        [HideInInspector] public bool isStaticConfigured;
        [HideInInspector] public bool isDynamicConfigured;
        private bool isBoundInitialized;

        public void OnDestroy()
        {
            foreach (var col in NonGrabbitColliders)
            {
                if (col)
                    col.enabled = true;
            }

            foreach (Collider staticCol in NonGrabbitCollidersStaticOnly)
            {
                if (staticCol)
                    staticCol.enabled = true;
            }
            
            for (var i = AddedStaticColliders.Count - 1; i >= 0; i--)
            {
                var addedStaticCollider = AddedStaticColliders[i];
                if (addedStaticCollider)
                    DestroyImmediate(addedStaticCollider);
            }

            for (var i = AddedDynamicColliders.Count - 1; i >= 0; i--)
            {
                var addedDynamicCollider = AddedDynamicColliders[i];
                if (addedDynamicCollider)
                    DestroyImmediate(addedDynamicCollider);
            }

            for (var i = AddedJoints.Count - 1; i >= 0; i--)
            {
                var addedJoint = AddedJoints[i];
                if (addedJoint)
                    DestroyImmediate(addedJoint);
            }

            NonGrabbitColliders.Clear();
            AddedDynamicColliders.Clear();
            AddedStaticColliders.Clear();

            if (WasBodyAdded)
                DestroyImmediate(Body);
            else
                RestoreBody();

            DestroyAllJoints();

            foreach (var data in CreatedSubDatas)
            {
                DestroyImmediate(data);
            }
        }

        public void RemoveCollidersFromSet(HashSet<Collider> colliders, bool recursive = true)
        {
            foreach (var col in AddedStaticColliders)
            {
                colliders.Remove(col);
            }

            foreach (var col in AddedDynamicColliders)
            {
                colliders.Remove(col);
            }

            if (recursive)
            {
                foreach (var data in SubDatas)
                {
                    data.RemoveCollidersFromSet(colliders, false);
                }
            }
        }

        public void RegisterBody(Rigidbody body)
        {
            Body = body;
            SaveState = new RbSaveState();
            SaveState.RegisterRigidBody(body);
        }

        public void RestoreBody()
        {
            SaveState.RestoreRigidBody(Body);
        }

        public void Awake()
        {
            PrepareStatic();
        }

        public int NonGrabbitColliderCount
        {
            get
            {
                int count = NonGrabbitColliders.Count;
                foreach (var data in SubDatas)
                {
                    count += data.NonGrabbitColliders.Count;
                }

                return count;
            }
        }


        public void PrepareStatic()
        {
            if (isStaticConfigured)
                return;

            RegisterRigidBody();
            SetBodiesAsStatic();
            RegisterExistingColliders();
            RegisterExistingMeshes();
            EncapsulateAllSubDatas();

            isStaticConfigured = true;
        }


        public void RegisterRigidBody()
        {
            var bodies = GetComponentsInChildren<Rigidbody>();

            Body = GetComponent<Rigidbody>();
            if (!Body)
            {
                Body = gameObject.AddComponent<Rigidbody>();

                WasBodyAdded = true;
            }

            foreach (Rigidbody body in bodies)
            {
                if (body.gameObject == gameObject)
                {
                    continue;
                }
                else
                {
                    var data = body.GetComponent<GrabbitData>();
                    if (!data)
                    {
                        data = body.gameObject.AddComponent<GrabbitData>();
                        CreatedSubDatas.Add(data);
                    }

                    if (!SubDatas.Contains(data))
                        SubDatas.Add(data);
                }
            }
        }

        public void RegisterExistingColliders()
        {
            var colliders = GetComponentsInChildren<Collider>();

            if (colliders.Length > 0)
            {
                bounds = colliders[0].bounds;
                isBoundInitialized = true;
            }

            foreach (var col in colliders)
            {
                if (!col.enabled || col.isTrigger)
                    continue;

                if (col.gameObject == gameObject)
                {
                    var meshCol = col as MeshCollider;
                    
                    if (meshCol && meshCol.convex == false)
                    {
                        NonGrabbitCollidersStaticOnly.Add(col);
                        col.enabled = false;
                    }
                    else
                    {
                        NonGrabbitColliders.Add(col);
                    }
                    
                }
                else
                {
                    var data = col.GetComponent<GrabbitData>();
                    if (!data)
                    {
                        data = col.gameObject.AddComponent<GrabbitData>();
                        CreatedSubDatas.Add(data);
                    }


                    if (!SubDatas.Contains(data))
                        SubDatas.Add(data);
                }
            }
        }

        public void RegisterExistingMeshes()
        {
            var meshes = GetComponentsInChildren<MeshFilter>();

            foreach (var mesh in meshes)
            {
                if (!mesh.sharedMesh || mesh.sharedMesh.triangles.Length <= 1)
                    continue;

                var go = mesh.gameObject;


                if (go == gameObject)
                {
                    var col = mesh.gameObject.AddComponent<MeshCollider>();
                    col.sharedMesh = mesh.sharedMesh;

                    AddedStaticColliders.Add(col);

                    if (!isBoundInitialized)
                    {
                        bounds = col.bounds;
                        isBoundInitialized = true;
                    }
                    else
                    {
                        bounds.Encapsulate(col.bounds);
                    }
                }
                else
                {
                    GrabbitData data = go.GetComponent<GrabbitData>();

                    if (!data)
                    {
                        data = mesh.gameObject.AddComponent<GrabbitData>();
                        CreatedSubDatas.Add(data);
                    }

                    if (!SubDatas.Contains(data))
                        SubDatas.Add(data);
                }
            }
        }

        public void EncapsulateAllSubDatas()
        {
            foreach (var subData in SubDatas)
            {
                bounds.Encapsulate(subData.bounds);
            }
        }

        public void PrepareDynamic()
        {
            if (isDynamicConfigured)
                return;

            var settings = GrabbitEditor.Instance.CurrentSettings;
            var colliderMeshContainer = GrabbitEditor.Instance.ColliderMeshContainer;

            if (settings.UseDynamicNonConvexColliders && colliderMeshContainer)
            {
                //averaging the estimated added convex mesh colliders to 3
                AddedDynamicColliders = new List<MeshCollider>(AddedStaticColliders.Count * 3);

                foreach (var col in AddedStaticColliders)
                {
                    if (!colliderMeshContainer.IsMeshDefined(col.sharedMesh))
                        //then it needs to be generated first
                        colliderMeshContainer.RegisterCollidersFromSelection(col, settings);

                    var meshes = colliderMeshContainer.GetMeshListAndRegenerateIfNeeded(col.sharedMesh, settings);

                    if (meshes.Count > 0)
                    {
                        foreach (var mesh in meshes)
                        {
                            AddMeshColliderToDynamicColliders(settings, col, mesh);
                        }
                    }
                    else
                    {
                        AddMeshColliderToDynamicColliders(settings, col, col.sharedMesh);
                    }
                }
            }
            else
            {
                foreach (var meshCollider in AddedStaticColliders)
                {
                    var dyna = meshCollider.gameObject.AddComponent<MeshCollider>();
                    dyna.convex = true;
                    AddedDynamicColliders.Add(dyna);
                }
            }

            foreach (var subData in SubDatas)
            {
                subData.PrepareDynamic();
            }

            isDynamicConfigured = true;
        }

        private void AddMeshColliderToDynamicColliders(GrabbitSettings settings, MeshCollider col, Mesh mesh)
        {
            var existingColliders = col.gameObject.GetComponents<MeshCollider>();

            //check for existing colliders to not have to duplicate the ones of other handlers, if subobjects are involved
            var existing = existingColliders.FirstOrDefault(_ => _.sharedMesh == mesh);

            var mc = existing ? existing : col.gameObject.AddComponent<MeshCollider>();

            if (!existing)
            {
                if (!settings.useLowQualityConvexCollidersOnSelection)
                    mc.cookingOptions &= ~MeshColliderCookingOptions.UseFastMidphase;
                mc.sharedMesh = mesh;
                mc.convex = true;
            }

            AddedDynamicColliders.Add(mc);
        }

        public void SetBodiesAsStatic()
        {
            if (Body)
            {
                Body.collisionDetectionMode = CollisionDetectionMode.Discrete;
                Body.isKinematic = true;
                Body.useGravity = false;
                Body.Sleep();
            }

            foreach (var data in SubDatas)
            {
                data.SetBodiesAsStatic();
            }
        }

        public void SetBodiesAsDynamic(bool alsoSubs = true)
        {
            if (Body)
            {
                Body.useGravity = false;
                Body.detectCollisions = true;
                Body.isKinematic = false;
                Body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                Body.angularDrag = 99999;
                Body.WakeUp();
            }

            if (alsoSubs)
            {
                foreach (var data in SubDatas)
                {
                    FixedJoint join = gameObject.AddComponent<FixedJoint>();
                    join.connectedBody = data.Body;
                    join.enableCollision = false;
                    AddedJoints.Add(join);
                    data.SetBodiesAsDynamic(false);
                }
            }
        }

        public void DisableColliders(bool alsoSubs = false)
        {
            foreach (var col in NonGrabbitColliders)
            {
                if (col)
                    col.enabled = false;
            }

            foreach (var col in AddedDynamicColliders)
            {
                if (col)
                    col.enabled = false;
            }

            foreach (var col in AddedStaticColliders)
            {
                if (col)
                    col.enabled = false;
            }

            if (alsoSubs)
            {
                foreach (var data in SubDatas)
                {
                    data.DisableColliders();
                }
            }
        }

        public void ActivateDynamicColliders(bool alsoSubs = true)
        {
            foreach (var col in AddedDynamicColliders)
            {
                if (col)
                    col.enabled = true;
            }

            if (alsoSubs)
                foreach (var data in SubDatas)
                {
                    data.ActivateDynamicColliders(false);
                }
        }

        public void ActivateStaticColliders(bool alsoSubs = true)
        {
            foreach (var col in AddedStaticColliders)
            {
                if (col)
                    col.enabled = true;
            }

            if (alsoSubs)
                foreach (var data in SubDatas)
                {
                    data.ActivateStaticColliders(false);
                }
        }

        public void ActivateNonGrabbitColliders(bool alsoSubs = true)
        {
            foreach (var col in NonGrabbitColliders)
            {
                if (col)
                    col.enabled = true;
            }

            if (alsoSubs)
                foreach (var data in SubDatas)
                {
                    data.ActivateNonGrabbitColliders(false);
                }
        }

        public void DeActivateDynamicColliders(bool alsoSubs = true)
        {
            foreach (var col in AddedDynamicColliders)
            {
                if (col)
                    col.enabled = false;
            }

            if (alsoSubs)
                foreach (var data in SubDatas)
                {
                    data.DeActivateDynamicColliders(false);
                }
        }

        public void DeActivateStaticColliders(bool alsoSubs = true)
        {
            foreach (var col in AddedStaticColliders)
            {
                if (col)
                    col.enabled = false;
            }

            if (alsoSubs)
                foreach (var data in SubDatas)
                {
                    data.DeActivateStaticColliders(false);
                }
        }

        public void DeActivateNonGrabbitColliders(bool alsoSubs = true)
        {
            foreach (var col in NonGrabbitColliders)
            {
                if (col)
                    col.enabled = false;
            }

            if (alsoSubs)
                foreach (var data in SubDatas)
                {
                    data.DeActivateNonGrabbitColliders(false);
                }
        }

        public void DestroyAllJoints()
        {
            for (var i = AddedJoints.Count - 1; i >= 0; i--)
            {
                var addedJoint = AddedJoints[i];
                DestroyImmediate(addedJoint);
            }

            AddedJoints.Clear();
        }
    }
}
#endif