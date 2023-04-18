using UnityEngine;

namespace Environment.Parallax
{
    public struct LayerInstance
    {
        //O objeto que contém cada peça do cenário em uma layer especifica
        public Transform transform;
        public LayerCollider layerCol;

        public LayerInstance(Transform transform, LayerCollider layerCol)
        {
            this.transform = transform;
            this.layerCol = layerCol;
        }
    }
}