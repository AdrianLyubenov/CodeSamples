using UnityEngine;

namespace Megalith
{
    public class MetaballNodeView : MegalithSceneView<MetaballNodeModel>
    {
        [HideInInspector]
        public Mesh boneMesh;

        public override void OnSceneUpdate()
        {
            
        }


#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (Model.seedModel == null)
            {
                Model.seedModel = GetComponentInParent<MetaballSeedView>().Model;                
            }

            if (Model.Density == 0.0f)
            {
                return;
            }

            if (Model.seedModel == null)
                return;

            if (!Model.seedModel.displayNodes)
                return;

            float drawRadius = Model.Radius;

            if (Model.seedModel != null)
            {
                drawRadius *= 1.0f + Mathf.Sqrt(Model.seedModel.powerThreshold) / 2f;
            }

            Matrix4x4 oldMtx = Gizmos.matrix;

            Gizmos.matrix = transform.localToWorldMatrix;

            if (Model.displayPreview)
            {
                if (Model.subtract == true)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireSphere(Vector3.zero, drawRadius);
                }
                else
                {
                    UnityEditor.Handles.color = new Color(1, 1, 1, 0.5f);
                    UnityEditor.Handles.SphereHandleCap(0, transform.position, Quaternion.identity, drawRadius, EventType.Repaint);
                }

            }

            Gizmos.color = Color.white;
            Gizmos.matrix = oldMtx;
        }
#endif
    }
}
