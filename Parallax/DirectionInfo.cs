using System.Collections.Generic;
using Helper;
using Helpers;
using Helpers.Enums;
using UnityEngine;

namespace Environment.Parallax
{
    public static class DirectionInfo
    {
        public static readonly Dictionary<BaseDirection, Vector2> vectors;
        public static readonly Dictionary<BaseDirection, Vector2> normalizedVectors;
        static DirectionInfo()
        {
            var res = Camera.main.GetCameraSizeInUnits();
            vectors = new Dictionary<BaseDirection, Vector2>()
            {
                { BaseDirection.Center, new Vector2(0, 0).normalized },
                { BaseDirection.Right, new Vector2(res.x, 0).normalized },
                { BaseDirection.Left, new Vector2(-res.x, 0).normalized },
                { BaseDirection.Up, new Vector2(0, res.y).normalized },
                { BaseDirection.Down, new Vector2(0, -res.y).normalized },

                { BaseDirection.RightUp, new Vector2(res.x, res.y).normalized },
                { BaseDirection.LeftUp, new Vector2(-res.x, res.y).normalized },
                { BaseDirection.RightDown, new Vector2(res.x, -res.y).normalized },
                { BaseDirection.LeftDown, new Vector2(-res.x, -res.y).normalized },
            };
            normalizedVectors = new Dictionary<BaseDirection, Vector2>()
            {
                { BaseDirection.Center, new Vector2(0, 0)},
                { BaseDirection.Right, new Vector2(1, 0) },
                { BaseDirection.Left, new Vector2(-1, 0) },
                { BaseDirection.Up, new Vector2(0, 1) },
                { BaseDirection.Down, new Vector2(0, -1) },

                { BaseDirection.RightUp, new Vector2(1, 1) },
                { BaseDirection.LeftUp, new Vector2(-1, 1) },
                { BaseDirection.RightDown, new Vector2(1, -1) },
                { BaseDirection.LeftDown, new Vector2(-1, -1) },
            };
        }

        public static BaseDirection GetOpposite(this BaseDirection dir)
        {
            //O inverso do valor da tela retorna a posição oposta
            Vector2 value = -(vectors[dir]);
            return vectors.GetKeyByValue(value);
        }

        public static BaseDirection GetNext(this BaseDirection dir, BaseDirection onGoingDir)
        {
            Vector2 value = vectors[dir];
            Vector2 positive = vectors[onGoingDir];

            float fValue = positive.y == 0 ? value.x : positive.x == 0 ? value.y : 0f;

            return fValue < 0 ? BaseDirection.Center : fValue != 0 ? 
                GetOpposite(dir) : positive.x - value.x == 0 ? 
                    BaseDirection.Center : GetOpposite(dir);
            
        }
    }
}