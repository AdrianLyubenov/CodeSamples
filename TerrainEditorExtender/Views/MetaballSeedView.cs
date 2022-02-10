using System;
using UnityEngine;

namespace Megalith
{
    public class MetaballSeedView : MegalithSceneView<MetaballSeedModel>
    {        
        [HideInInspector]
        public GameObject[] boneNodes = new GameObject[0];
        [HideInInspector]
        public Transform boneRoot;
        [HideInInspector]
        public MeshFilter meshFilter;
        [HideInInspector]
        public Material sharedMaterial;
        [HideInInspector]
        public Material instanceMaterial;
        [HideInInspector]
        public bool isSealed;
        [HideInInspector]
        public string prefabName = "Metaball prefab";
        [HideInInspector]
        public bool pivotSet = false;
        

        public override void OnSceneUpdate()
        {
#if UNITY_EDITOR            
            var style = new GUIStyle(UnityEditor.EditorStyles.label);            
            if(isSealed)
            {
                UnityEditor.Handles.Label(transform.position, new GUIContent(" Sealed", Resources.Load<Texture2D>("lock")), style);
            }
            else
            {
                UnityEditor.Handles.Label(transform.position, new GUIContent(" Active", Resources.Load<Texture2D>("unlock")), style);
            }
#endif
        }
	}

}
