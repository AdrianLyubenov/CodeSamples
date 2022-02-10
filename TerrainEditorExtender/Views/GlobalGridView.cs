using UnityEngine;

namespace Megalith
{
    [RequireComponent(typeof(Projector))]
    public class GlobalGridView : MegalithSceneView<GlobalGridModel>
    {
        public override void Setup(ModelBase model)
        {
            base.Setup(model);
        }

        public void SetProjectorSize(float size)
        {
            GetComponent<Projector>().orthographicSize = size;
        }

        public void SetProjectorColor(Color color)
        {
            GetComponent<Projector>().material.SetColor("_GridColour", color);
            GetComponent<Projector>().material.SetFloat("_GridTransparency", color.a);
        }

        public void SetSubdivisions(float subdivisions)
        {
            GetComponent<Projector>().material.SetFloat("_Subdivisions", subdivisions);
        }

        public void ToggleVisibility(bool isVisible)
        {
            GetComponent<Projector>().enabled = isVisible;
        }

        public void SetGridSize(float gridSize)
        {
            GetComponent<Projector>().material.SetFloat("_GridSpacing", gridSize);
        }

        public override void OnSceneUpdate()
        {

        }
    }

}
