using System.Collections.Generic;
using UnityEngine;

namespace Environment.Parallax
{
    public struct SceneryInstance
    {
        public Transform transform;
        public List<LayerInstance> layers;

        public SceneryInstance(Transform transform, List<LayerInstance> layersInfo)
        {
            this.transform = transform;
            layers = layersInfo;
        }
    }
}