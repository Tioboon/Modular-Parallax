using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Environment.Parallax
{
    [Serializable, HideLabel]
    public class LayerSerializedInfo
    {
        public LayerSerializedInfo()
        {
            z = 0f;
            startSortingOrder = 0;
            sortingLayerVariation = 0;
        }
        public float z;
        public int startSortingOrder;
        [MaxValue(-2)]
        public int sortingLayerVariation;
    }
}