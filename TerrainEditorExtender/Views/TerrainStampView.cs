using System.Collections.Generic;
using UnityEngine;

namespace Megalith
{
    public class TerrainStampView : MegalithSceneView<TerrainStampModel>
    {
        public MegalithEditor Megalith { get; set; }

        public override void OnSceneUpdate()
        {
            if (Model.splinePath == null)
                Model.splinePath = new List<Vector3>();

            if (Model.splinePath.Count > 1)
            {
                Color color = Color.red * Megalith.megalithModel.SceneUITransparency;
                MegalithSplineUtils.DisplayCatmullRomSpline(Model.splinePath.ToArray(), color);
            }
        }
    }
}