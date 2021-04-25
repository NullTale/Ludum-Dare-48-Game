using System;
using System.Collections.Generic;
using UnityEngine;

namespace CoreLib
{
    public class TextureTransform
    {
        public Texture2D Texture;
        public float     Rotation;
        //Vector2             Translation;
        public Vector2 Scale;
        public RectInt Frame;
        public Vector2 Pivot = new Vector2(0.5f, 0.5f);

        public class Pixel
        {
            public Vector2Int Position;
            public Color Color;

            public Pixel(Vector2Int position, Color color)
            {
                Position = position;
                Color = color;
            }
        }

        //////////////////////////////////////////////////////////////////////////
        public IEnumerable<Pixel> GetPixels()
        {
            var trans       = Matrix2x2.RotationScale(Rotation, Scale);
            var w           = (int)(Frame.width * Scale.x);
            var h           = (int)(Frame.height * Scale.y);
            var pivot       = new Vector2Int((int)(Frame.width * Pivot.x), (int)(Frame.height * Pivot.y));
            var pivotScaled = new Vector2(Frame.width * Pivot.x * Scale.x, Frame.height * Pivot.y * Scale.y);
            var invScaleX   = 1.0f / Scale.x;
            var invScaleY   = 1.0f / Scale.y;

            for (var x = 0; x < w; x++)
            for (var y = 0; y < h; y++)
            {
                var scaleX = x * invScaleX;
                var scaleY = y * invScaleY;
                yield return new Pixel((new Vector2(scaleX - pivot.x, scaleY - pivot.y) * trans + pivotScaled).ToVector2Int(),
                                       Texture.GetPixel((int)scaleX, (int)scaleY));
            }
        }

        public TextureTransform(Texture2D tex, RectInt frame, float degRotation, float scale)
        {
            Texture  = tex;
            Frame    = frame;
            Rotation = degRotation;
            Scale.Set(scale, scale);
        }
    }
}