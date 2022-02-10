using UnityEngine;
using System.Collections.Generic;

namespace Megalith
{
    public class DebugHelpers : MonoBehaviour
    {
        protected List<GameObject> debugObjects;        

        static DebugHelpers instance;

        public static DebugHelpers Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<DebugHelpers>();
                    if (instance == null)
                    {
                        GameObject obj = new GameObject();
                        obj.hideFlags = HideFlags.HideAndDontSave;
                        instance = obj.AddComponent<DebugHelpers>();
                        instance.debugObjects = new List<GameObject>();
                    }
                }
                return instance;
            }
        }

        public static void CreateDebugSphere(Vector3 pos, Color color, float scale = 0.2f)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ScaleAndPositionDebugObject(sphere, pos, color, scale);
            Destroy(sphere.GetComponent<SphereCollider>());
            Instance.debugObjects.Add(sphere);
        }

        public static void CreateDebugCube(Vector3 pos, Color color, float scale = 0.2f)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ScaleAndPositionDebugObject(cube, pos, color, scale);
            Destroy(cube.GetComponent<BoxCollider>());
            Instance.debugObjects.Add(cube);
        }

        private static void ScaleAndPositionDebugObject(GameObject obj, Vector3 pos, Color color, float scale)
        {
            obj.transform.localScale = new Vector3(scale, scale, scale);
            obj.transform.position = pos;

            Renderer rend = obj.GetComponent<Renderer>();
            rend.material.color = color;
            rend.receiveShadows = false;
            rend.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }                

        public static void VisualizeTerrainAreaVertices(LocalHeightMap localHeightMap, Area tArea)
        {
            foreach (Vertex v in tArea.vertices)
            {
                Color c;

                bool isFirstOrLastVert = (v.areaX == 0 && v.areaZ == 0) ||
                    (v.areaX == tArea.vertSizeX - 1 && v.areaZ == tArea.vertSizeZ - 1);

                if (v.distanceFromNearestBorder == 0 && !isFirstOrLastVert)
                {
                    c = new Color32(171, 123, 113, 1);
                }
                else
                if (v.distanceFromNearestBorder == 1)
                {
                    c = new Color32(100, 165, 151, 1);
                }
                else
                if (v.isCenter)
                {
                    c = Color.black;
                }
                else if (localHeightMap.localHeightmapVertices[v.areaZ, v.areaX].insideStampCollider)
                {
                    c = Color.red;
                }
                else if (isFirstOrLastVert)
                {
                    c = Color.blue;
                }
                else
                {
                    c = Color.gray;
                }
                CreateDebugSphere(v.WorldPos(), c);
            }
        }

        public static void CreateDebugSpheresForTerrainBounds(Terrain terrain)
        {
            foreach (Vertex pos in TerrainHelpers.TerrainBounds(terrain))
            {
                CreateDebugSphere(pos.WorldPos(), Color.green);
            }
        }

        public static void DestroyObjects()
        {
            foreach (GameObject debugObj in Instance.debugObjects)
            {
                DestroyImmediate(debugObj);
            }
        }

        public static void CreateTextMesh(Transform textMesh, Vector3 position, float value)
        {
            Transform txtMeshTransform = Instantiate(textMesh);
            txtMeshTransform.position = position;
            TextMesh txtMesh = txtMeshTransform.GetComponent<TextMesh>();
            txtMesh.text = GetTwoFractionalDigitString(value);
            txtMesh.color = Color.red;
        }

        private static string GetTwoFractionalDigitString(float input)
        {
            float absValue = Mathf.Abs(input);
            float fraction = (absValue - Mathf.Floor(absValue));
            string s1 = fraction.ToString("E1");

            int exponent = int.Parse(s1.Substring(5)) + 1;

            string s = input.ToString("F" + exponent.ToString());

            return s;
        }
    }

}
