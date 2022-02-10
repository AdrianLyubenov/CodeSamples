using UnityEngine;

namespace Megalith
{
    public class MetaballBrushView : BrushViewBase<MetaballStamp.MetaballStampModel>
    {
        public MegalithEditor Megalith { get; set; }

        public override void OnSceneUpdate()
        {
            if (Model.splinePath.Count > 1)
            {
                Color color = Color.red * Megalith.megalithModel.SceneUITransparency;
                MegalithSplineUtils.DisplayCatmullRomSpline(Model.splinePath.ToArray(), color);
            }
                
            if (Model.slopeSelectionPath.Count > 0)
            {
                var currentPos = transform.position + (Vector3.down * 1000f);
#if UNITY_EDITOR
                UnityEditor.Handles.color = Color.green * Megalith.megalithModel.SceneUITransparency;
                UnityEditor.Handles.DrawLine(Model.slopeSelectionPath[0], currentPos);
                var style = new GUIStyle(UnityEditor.EditorStyles.label);
                style.normal.textColor = Color.white * Megalith.megalithModel.SceneUITransparency;
                style.contentOffset = new Vector2(20f, 0f);
                UnityEditor.Handles.Label(currentPos, $"{GetSlopeAtPoint(currentPos)}°", style);
                UnityEditor.Handles.Label(Model.slopeSelectionPath[0], $"{GetSlopeAtPoint(Model.slopeSelectionPath[0])}°", style);
#else
                Gizmos.color = Color.green * Megalith.megalithModel.SceneUITransparency;
                Gizmos.DrawLine(Model.slopeSelectionPath[0], currentPos);
#endif
            }
        }

        public float GetSlopeAtPoint(Vector3 point)
        {
            foreach(var terrain in Terrain.activeTerrains)
            {
                var pos = terrain.transform.InverseTransformPoint(point);
                if (pos.x < 0 || pos.z < 0 || pos.x > terrain.terrainData.size.x || pos.z > terrain.terrainData.size.z)
                    continue;
                pos.x /= terrain.terrainData.size.x;
                pos.z /= terrain.terrainData.size.z;
                return terrain.terrainData.GetSteepness(pos.x, pos.z);
            }
            return 0f;
        }
    }
}
