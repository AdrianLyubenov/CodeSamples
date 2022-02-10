using UnityEngine;

namespace Megalith
{
    [RequireComponent(typeof(Projector))]
    public abstract class BrushViewBase<T> : MegalithSceneView<T>
        where T : ModelBase
    {
        public Projector projector;

        [HideInInspector]
        public Material material;

        [HideInInspector]
        public bool IsOnGrid = false;

        public override void Setup(ModelBase model)
        {
            base.Setup(model);
            material           = new Material(projector.material);
            projector.material = material;
        }

        public virtual void SetBrush(Texture2D brushIcon)
        {
            material.SetTexture("_ShadowTex", brushIcon);
        }

        public Texture2D GetBrush()
        {
            return (Texture2D) material.GetTexture("_ShadowTex");
        }

        public virtual void SetSize(float size)
        {
            if (projector != null)
                projector.orthographicSize = size * 10f;
        }

        public virtual void SetIgnoreLayer(int layer)
        {
            projector.ignoreLayers = layer;
        }

        private void OnDestroy()
        {
            DestroyImmediate(material, true);
        }
    }
}