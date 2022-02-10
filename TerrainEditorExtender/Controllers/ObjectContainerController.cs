using System.Collections.Generic;
using UnityEngine;
using System;
using Testing.Mesh_Deform;
using System.Linq;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;
using Megalith.Scripts.Models.AssetModels;

namespace Megalith
{
#if UNITY_EDITOR  
	[ExecuteInEditMode]
	public class ObjectContainerController : MonoBehaviour
	{
        [Serializable]
        public class LodSet
        {
            public GameObject gameObject;
            public float distance;

            public LodSet(GameObject gameObject, float distance)
            {
                this.gameObject = gameObject;
                this.distance = distance;
            }
        }

		[HideInInspector]
		public MegalithEditor megalith;
        [HideInInspector]
        public List<GameObject> objects = new List<GameObject>();
        [HideInInspector]
        public List<GameObject> objectPrefabs = new List<GameObject>();
        [HideInInspector]
        public List<LodSet> lodSets = new List<LodSet>();

        private Dictionary<string, List<GameObject>> objectPartitions;

        [NonSerialized] public Dictionary<GameObject, Bounds> objectVolumes;
        [NonSerialized] public HashSet<GameObject> validSurfaces;

        private void Awake()
        {
            Setup();
        }

        public void Setup()
		{
            megalith = GameObject.FindObjectOfType<MegalithEditor>();
            if (megalith == null)
				throw new InvalidOperationException("MegalithEditor not found!");
			RefreshPools();            
		}		

		private void RefreshPools()
		{
			objects.Clear();
            objectPartitions = new Dictionary<string, List<GameObject>>();

            objectVolumes = new Dictionary<GameObject, Bounds>();
            validSurfaces = new HashSet<GameObject>();
            if(!Application.isPlaying)
            {
                lodSets = new List<LodSet>();
            }            

            foreach(Transform root in transform)
			{
                foreach(Transform child in root)
                {
                    AddObject(child.gameObject);                    
                }                
            }
        }
      

        public void RefreshValidSurfaces()
        {

            validSurfaces = new HashSet<GameObject>();
            foreach(var go in objects)
            {
                var source = UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(go);
                if (source != null && megalith != null)
                {
                    var pairs = megalith.megalithModel.objectStampingModel.objectPairs;
                    if (pairs != null && pairs.ContainsKey(source))
                    {
                        var stamp = pairs[source];
                        if (stamp.isSurface)
                            validSurfaces.Add(go);
                    }
                }
            }

        }

        public void UpdateObjectPrefabs()
        {

            if (objects == null)
                return;
            objectPrefabs = new List<GameObject>(objects.Count);
            foreach(var obj in objects) 
            {
                var source = UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(obj);
                objectPrefabs.Add(source);
            }

        }

        public void UpdateLayers(List<ObjectBrushModel> models)
        {
            foreach(var model in models)
            {
                UpdateLayers(model);
            }
        }

        public void UpdateLayers(ObjectBrushModel model)
        {
            string guid;
            long localId;
            UnityEditor.AssetDatabase.TryGetGUIDAndLocalFileIdentifier(model.objectPrefab, out guid, out localId);
            if (objectPartitions.ContainsKey(guid))
            {
                for(int i = objectPartitions[guid].Count - 1; i >= 0; i--)
                {
                    var obj = objectPartitions[guid][i];
                    if(obj == null)
                    {
                        objectPartitions[guid].RemoveAt(i);
                        continue;
                    }
                    var cols = obj.GetComponentsInChildren<Collider>();
                    if (!model.BlocksRaycasting)
                    {
                        foreach (var col in cols)
                        {
                            col.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
                        }
                    }
                    else
                    {
                        foreach (var col in cols)
                        {
                            col.gameObject.layer = LayerMask.NameToLayer("Default");
                        }
                    }
                }                               
            }
        }


        public void ResetObjectLayers()
        {
            for(int i = objects.Count - 1; i >= 0; i--)
            {
                var obj = objects[i];
                if(obj == null)
                {
                    objects.RemoveAt(i);
                    continue;
                }
                var cols = obj.GetComponentsInChildren<Collider>();
                foreach (var col in cols)
                {
                    col.gameObject.layer = LayerMask.NameToLayer("Default");
                }
            }
        }

		public void AddObject(GameObject go)
		{
            //TODO do this only if we are in texture creation
            if(FindObjectOfType<CreateTextureObject>())
                RemoveGameObjectCollidersAndSetMeshColliders(go);

            if (objectPartitions == null)
			    Setup();
			if (!objects.Contains(go))
			{
				objects.Add(go);
                if (!megalith.megalithModel.enableLOD || !megalith.megalithModel.affectEditMode)
                    go.SetActive(true);
              
                var source = UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(go);
                var renderers = go.GetComponentsInChildren<MeshRenderer>();
                ObjectBrushModel model = null;
                if(renderers.Length > 0)
                {
                    var bounds = renderers[0].bounds;
                    for(int i = 1; i < renderers.Length; i++)
                    {
                        bounds.Encapsulate(renderers[i].bounds);
                    }
                    objectVolumes.Add(go, bounds);
                    if(source != null && megalith != null)
                    {
                        var pairs = megalith.megalithModel.objectStampingModel.objectPairs;
                        if(pairs != null && pairs.ContainsKey(source))
                        {
                            model = pairs[source];
                            if (model.isSurface)
                                validSurfaces.Add(go);
                            var cols = go.GetComponentsInChildren<Collider>();
                            if (!model.BlocksRaycasting)
                            {                                
                                foreach(var col in cols)
                                {
                                    col.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
                                }
                            }
                            else
                            {
                                foreach (var col in cols)
                                {
                                    col.gameObject.layer = LayerMask.NameToLayer("Default");
                                }
                            }
                        }
                        
                    }
                }
                                
                if (source == null)
                {
                    return;
                }
                if (model != null && !Application.isPlaying)
                {
                    lodSets.Add(new LodSet(go, model.lodMaxDistance));
                }
                string guid;
                long localId;
                UnityEditor.AssetDatabase.TryGetGUIDAndLocalFileIdentifier(source, out guid, out localId);
                if (!objectPartitions.ContainsKey(guid))
                {
                    objectPartitions.Add(guid, new List<GameObject>());                                            
                }
                objectPartitions[guid].Add(go);
                string rootName = $"{source.name} Container";
                var root = transform.Find(rootName);
                if(root == null)
                {
                    root = new GameObject(rootName).transform;
                    root.parent = transform;
                }
                go.transform.parent = root;
            }
        }

        void RemoveGameObjectCollidersAndSetMeshColliders(GameObject go)
        {
            Collider[] colliders = go.transform.GetComponentsInChildren<Collider>();

            foreach (var col in colliders)
            {
                DestroyImmediate(col);
            }

            MeshRenderer[] meshes = go.transform.GetComponentsInChildren<MeshRenderer>();

            foreach (var mesh in meshes)
            {
                MeshCollider mc = mesh.gameObject.AddComponent<MeshCollider>();
                mc.sharedMesh = mc.gameObject.GetComponent<MeshFilter>().sharedMesh;
                if(!mc.gameObject.GetComponent<Sliceable>())
                    mc.gameObject.AddComponent<Sliceable>();
            }
        }

        public List<GameObject> GetObjectsByPrefabSource(GameObject source)
        {
            string guid;
            long localId;
            UnityEditor.AssetDatabase.TryGetGUIDAndLocalFileIdentifier(source, out guid, out localId);
            if (objectPartitions.ContainsKey(guid))
            {
                return new List<GameObject>(objectPartitions[guid]);
            }
            return new List<GameObject>();
        }

        public List<GameObject> GetObjectsByPrefabSourceInRadius(GameObject source, Vector3 position, float radius)
        {
            string guid;
            long localId;
            UnityEditor.AssetDatabase.TryGetGUIDAndLocalFileIdentifier(source, out guid, out localId);
            if (objectPartitions.ContainsKey(guid))
            {
                var list = new List<GameObject>(objectPartitions[guid]);
                for(int i = list.Count - 1; i >= 0; i--)
                {
                    var obj = list[i];
                    if(!objectVolumes.ContainsKey(obj))
                    {
                        if (Vector3.Distance(position, obj.transform.position) > radius)
                        {
                            list.RemoveAt(i);
                            continue;
                        }
                    }
                    else
                    {
                        var size = Mathf.Max(objectVolumes[obj].size.x, objectVolumes[obj].size.y, objectVolumes[obj].size.z) / 2f;
                        if (Vector3.Distance(position, obj.transform.position) > radius + size)
                        {
                            list.RemoveAt(i);
                            continue;
                        }
                    }
                    
                }
                return list;
            }
            return new List<GameObject>();
        }


        public void RemoveObject(GameObject go)
		{
            if(objectPartitions == null)
			    Setup();
            var root = go.transform.parent;
            int index = objects.IndexOf(go);
			objects.Remove(go);

            validSurfaces.Remove(go);
            objectVolumes.Remove(go);
            GameObject source = null;
            if(Application.isPlaying && objectPrefabs.Count > index)
            {
                source = objectPrefabs[index];
                objectPrefabs.RemoveAt(index);                
            }
            else
            {
                source = UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(go);
            }            
            if (source == null)
            {                
                return;
            }
            string guid;
            long localId;
            UnityEditor.AssetDatabase.TryGetGUIDAndLocalFileIdentifier(source, out guid, out localId);
            if (!objectPartitions.ContainsKey(guid))
            {
                var keys = objectPartitions.Keys.ToArray();
                foreach(var key in keys)
                {
                    if (objectPartitions[key].Contains(go))
                        objectPartitions[key].Remove(go);
                }
            }
            else
            {
                objectPartitions[guid].Remove(go);
            }
            UnityEditor.Undo.DestroyObjectImmediate(go);
            if (root.childCount <= 0)
                UnityEditor.Undo.DestroyObjectImmediate(root.gameObject);             

        }

        private void OnRenderObject()
		{
            ApplyLOD();            			
		}        
        
        public void ApplyLOD()
        {
            if (megalith == null)
                return;
            if (!megalith.megalithModel.enableLOD)
                return;
            Vector3? cameraPosition = null;

            if (!Application.isPlaying)
            {
                if (!megalith.megalithModel.affectEditMode)
                    return;
                cameraPosition = UnityEditor.SceneView.lastActiveSceneView.camera.transform.position;
            }                
            else
                cameraPosition = Camera.main?.transform?.position;

				cameraPosition = Camera.main?.transform?.position;
            if (cameraPosition == null)
                return;
            if (lodSets == null || lodSets.Count <= 0)
                return;
            foreach(var set in lodSets)
            {
                if (set.gameObject == null)
                    continue;
                set.gameObject.SetActive(Vector3.Distance(set.gameObject.transform.position, cameraPosition.Value) <= set.distance);
            }                       
        }

        public List<GameObject> SphereCastAll(Ray ray, float radius, float distance, int layerMask)
        {
            var objects = new HashSet<GameObject>();            
            var gos = Physics.SphereCastAll(ray, radius, distance, layerMask);
            foreach(var col in gos)
            {
                var go = ResolveParent(col.transform);
                objects.Add(go);
            }                        
            return new List<GameObject>(objects);
        }

        public bool Raycast(Ray ray, out GameObject hit, float distance)
        {
            RaycastHit rayHit;
            if (Physics.Raycast(ray, out rayHit, distance))
            {
                hit = ResolveParent(rayHit.transform);
                return true;
            }
            hit = null;
            return false;
        }

        public bool SphereCast(Ray ray, float radius, out GameObject hit, float distance)
        {            
            RaycastHit rayHit;
            if(Physics.SphereCast(ray, radius, out rayHit, distance))
            {
                hit = ResolveParent(rayHit.transform);
                return true;
            }
            hit = null;
            return false;
        }
        
        private GameObject ResolveParent(Transform hitTransform)
        {
            Transform parent = hitTransform;
            while(parent.parent != null && parent.parent.parent != null && parent.parent.parent != transform)
            {
                parent = parent.parent;
            }
            return parent.gameObject;
        }
    }
    #endif
}