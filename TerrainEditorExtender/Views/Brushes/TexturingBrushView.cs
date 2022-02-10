using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Megalith
{
    public class TexturingBrushView : BrushViewBase<TextureLayerModel>
    {
        public MegalithEditor Megalith { get; set; }

        public override void OnSceneUpdate()
        {
            if (Model.SplinePath == null)
                Model.SplinePath = new List<Vector3>();

            if (Model.SelectionPath == null)
                Model.SelectionPath = new List<Vector3>();

            if (Model.SplinePath.Count > 1)
            {
                Color color = Color.red * Megalith.megalithModel.SceneUITransparency;
                MegalithSplineUtils.DisplayCatmullRomSpline(Model.SplinePath.ToArray(), color);
            }


            if (Model.SelectionPath.Count > 0)
            {
                var currentPos = transform.position + (Vector3.down * 1000f);
#if UNITY_EDITOR
                Handles.color = Color.red * Megalith.megalithModel.SceneUITransparency;
                Handles.DrawLine(Model.SelectionPath[0], currentPos);
                var style = new GUIStyle(EditorStyles.label) {normal = {textColor = Color.red * Megalith.megalithModel.SceneUITransparency}, fontSize = 25, contentOffset = new Vector2(20f, 0f)};
                if (Model.inSlopeSelectionMode)
                {
                    Handles.Label(currentPos,             $"{GetSlopeAtPoint(currentPos),2:F}°",             style);
                    Handles.Label(Model.SelectionPath[0], $"{GetSlopeAtPoint(Model.SelectionPath[0]),2:F}°", style);
                }
                else if (Model.InHeightSelectionMode)
                {
                    var height1 = GetHeightAtPoint(currentPos);
                    Handles.Label(currentPos, $"{height1,2:F}", style);

                    var height2 = GetHeightAtPoint(Model.SelectionPath[0]);
                    Handles.Label(Model.SelectionPath[0], $"{height2,2:F}", style);
                }
                // #else
                //                 Gizmos.color = Color.green * Megalith.megalithModel.SceneUITransparency;
                //                 Gizmos.DrawLine(Model.selectionPath[0], currentPos);
#endif
            }
        }

        public float GetSlopeAtPoint(Vector3 point)
        {
            var pos = Terrain.activeTerrain.transform.InverseTransformPoint(point);
            pos.x /= Terrain.activeTerrain.terrainData.size.x;
            pos.z /= Terrain.activeTerrain.terrainData.size.z;
            return Terrain.activeTerrain.terrainData.GetSteepness(pos.x, pos.z);
        }

        // public float GetHeightAtPoint(Vector3 point)
        // {
        //     var activeTerrain = Terrain.activeTerrain;
        //     var pos           = activeTerrain.transform.InverseTransformPoint(point);
        //     var height        = activeTerrain.SampleHeight(pos) / activeTerrain.terrainData.size.y;
        //     return height;
        // }

        public float GetHeightAtPoint(Vector3 point)
        {
            var activeTerrain = Terrain.activeTerrain;

            var pos = activeTerrain.transform.InverseTransformPoint(point);

            var height = activeTerrain.SampleHeight(pos); /// - activeTerrain.terrainData.bounds.min.y;
            //var terrainHeight = activeTerrain.terrainData.bounds.max.y; // - activeTerrain.terrainData.bounds.min.y;
            return height; // / terrainHeight;
        }
    }
}