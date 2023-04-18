using System.Collections.Generic;
using Helpers;
using UnityEngine;

namespace Environment.Parallax
{
    public struct LayerCreationInfo
    {
        public string baseName;
        public int idx;
        public Vector3 position;
        public Bounds bounds;
        public float proportion;

        public LayerCreationInfo(Vector3 position, Bounds bounds, float proportion, string baseName, int idx)
        {
            this.position = position;
            this.bounds = new Bounds(bounds.center, bounds.size * proportion);

            this.proportion = proportion;
            this.baseName = baseName;
            this.idx = idx;
        }
        

        public void SetBound(Bounds adjustLayerTransform)
        {
            bounds = adjustLayerTransform;
        }
    }
}