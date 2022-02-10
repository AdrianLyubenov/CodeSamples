using UnityEngine;
using System;

namespace Megalith
{
    public abstract class MegalithSceneViewBase : MonoBehaviour
    {
        [HideInInspector]
        public ModelBase ModelBase;

        public abstract void OnSceneUpdate();

        public virtual void Setup(ModelBase model)
        {
            ModelBase = model;
        }
    }

    public abstract class MegalithSceneView<M> : MegalithSceneViewBase
        where M : ModelBase
    {        
        [HideInInspector]
        public M Model;

        public override void Setup(ModelBase model)
        {
            base.Setup(model);
            Model = (M)model;
        }        
    }
}
