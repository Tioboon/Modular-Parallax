using System;
using System.Collections;
using System.Collections.Generic;
using Global;
using Modularization;
using UnityEngine;

namespace Environment.Parallax
{
    public class LayerCollider : MonoBehaviour
    {
        public EdgeCollider2D collider;
        private CollisionAngleInfo angleInfo;
        private IController controller;
        private int layerIdx;
        private bool sentMessage;

        public void Initialize(IController controller, int layerIdx, CollisionAngleInfo angleInfo, EdgeCollider2D collider2D, Transform scenery)
        {
            this.controller = controller;
            this.layerIdx = layerIdx;
            this.collider = collider2D;
            this.angleInfo = angleInfo;
        }
        
        private void OnTriggerExit2D(Collider2D col)
        {
            if(!col.CompareTag("Player")) return;
            var side = CollisionAngleInfo.CheckSideOfCollision(collider, col, angleInfo);
            Debug.Log($"{transform.parent.parent.name} hitted from the side: {side}");
            controller.ReceiveMessage(side, layerIdx);
        }
    }
}