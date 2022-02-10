using UnityEngine;

namespace Megalith
{
    [RequireComponent(typeof(Renderer))]
    [RequireComponent(typeof(Collider))]
    public class GridView : MegalithSceneView<GridModel>
    {
        private Color32 m_BaseColor = new Color32(255, 132, 0, 255);

        public Color32 BaseColor
        {
            get
            {
                return m_BaseColor;
            }
            set
            {
                if (!value.Equals(m_BaseColor))
                {
                    m_BaseColor = value;
                    ApplyColor();
                }
            }
        }

        public override void Setup(ModelBase model)
        {
            base.Setup(model);
        }

        private void ApplyColor()
        {
			GetComponent<Renderer>().sharedMaterial.SetColor("_GridColour", BaseColor);
            if (GetComponent<Collider>().enabled == true)
            {
				GetComponent<Renderer>().sharedMaterial.SetFloat("_GridTransparency", 1f * ((Color)BaseColor).a);
            }
            else
            {
				GetComponent<Renderer>().sharedMaterial.SetFloat("_GridTransparency", 0.4f * ((Color)BaseColor).a);
            }
        }

        public void SetGrounded(bool toggle)
        {
            if (toggle)
            {
				GetComponent<Renderer>().sharedMaterial.SetFloat("_ViewRadius", 500f);
				GetComponent<Renderer>().sharedMaterial.SetFloat("_GridTransparency", 1f * ((Color)BaseColor).a);
				GetComponent<Collider>().enabled = true;
            }
            else
            {
				GetComponent<Renderer>().sharedMaterial.SetFloat("_ViewRadius", 35f);
				GetComponent<Renderer>().sharedMaterial.SetFloat("_GridTransparency", 0.4f * ((Color)BaseColor).a);
				GetComponent<Collider>().enabled = false;
            }
        }

        public void SetSize(float size)
        {
			GetComponent<Renderer>().sharedMaterial.SetFloat("_GridSpacing", size);
        }

        public override void OnSceneUpdate()
        {
            
        }
    }

}
