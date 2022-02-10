using UnityEngine;
using System.Collections.Generic;

namespace Megalith
{
    public static class TransformExtensions
    {
        public static void SetLayer(this Transform trans, int layer)
        {
            Stack<Transform> moveTargets = new Stack<Transform>();
            moveTargets.Push(trans);
            Transform currentTarget;
            while (moveTargets.Count != 0)
            {
                currentTarget = moveTargets.Pop();
                currentTarget.gameObject.layer = layer;
                foreach (Transform child in currentTarget)
                    moveTargets.Push(child);
            }
        }
    }
}