using UnityEngine;

namespace Megalith
{
	public class ObjectBrushView : BrushViewBase<ObjectBrushModel>
	{
        public MegalithEditor Megalith { get; set; }

        public override void SetSize(float size)
        {
            if (!Model.IsSingleDeleting)
                base.SetSize(size);
            else
                base.SetSize(0.1f);
        }

        public override void OnSceneUpdate()
		{
            if (Model.paintMode == EPaintMode.Swap)
                return;
#if UNITY_EDITOR
            if (Model.splinePath.Count > 1)
            {
                Color color = Color.red * Megalith.megalithModel.SceneUITransparency;
                MegalithSplineUtils.DisplayCatmullRomSpline(Model.splinePath.ToArray(), color);
            }
                
            var currentPos = transform.position + (Vector3.down * 1000f);
            UnityEditor.Handles.color = Color.white * Megalith.megalithModel.SceneUITransparency;
            Vector2 surfaceWidth;
            if (Model.paintMode == EPaintMode.Paint)
                surfaceWidth = Model.placedSurfaceWidth;
            else
                surfaceWidth = Model.erasedSurfaceWidth;
            UnityEditor.Handles.DrawWireDisc(currentPos, Vector3.up, surfaceWidth.x / 2f);
            UnityEditor.Handles.DrawWireDisc(currentPos, Vector3.up, surfaceWidth.y / 2f);
            UnityEditor.Handles.DrawDottedLine(currentPos + Vector3.right * surfaceWidth.x / 2f, currentPos + Vector3.right * surfaceWidth.y / 2f, 5f);
            if (Model.slopeSelectionPath.Count > 0)
            {                
                UnityEditor.Handles.color = Color.green * Megalith.megalithModel.SceneUITransparency;
                UnityEditor.Handles.DrawLine(Model.slopeSelectionPath[0], currentPos);
                var style = new GUIStyle(UnityEditor.EditorStyles.label);
                style.normal.textColor = Color.white * Megalith.megalithModel.SceneUITransparency;
                style.contentOffset = new Vector2(20f, 0f); 
                if(Model.inSlopeSelectionMode)
                {
                    UnityEditor.Handles.Label(currentPos, $"{GetSlopeAtPoint(currentPos)}°", style);
                    UnityEditor.Handles.Label(Model.slopeSelectionPath[0], $"{GetSlopeAtPoint(Model.slopeSelectionPath[0])}°", style);
                }
                else
                {
                    UnityEditor.Handles.Label(currentPos, $"{Vector3.Distance(currentPos, Model.slopeSelectionPath[0])}m", style);                    
                }
            }
#endif
        }

        public float GetSlopeAtPoint(Vector3 point)
        {
            foreach (var terrain in Terrain.activeTerrains)
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