using System;
using UnityEngine;

namespace CoreLib
{
    [Serializable]
    public class Matrix2x3
    {
        public float m00
        {
            get => m_Matrix[0, 0];
            set => m_Matrix[0, 0] = value;
        }

        public float m01
        {
            get => m_Matrix[1, 0];
            set => m_Matrix[1, 0] = value;
        }

        public float m02
        {
            get => m_Matrix[2, 0];
            set => m_Matrix[2, 0] = value;
        }

        public float m10
        {
            get => m_Matrix[0, 1];
            set => m_Matrix[0, 1] = value;
        }

        public float m11
        {
            get => m_Matrix[1, 1];
            set => m_Matrix[1, 1] = value;
        }

        public float m12
        {
            get => m_Matrix[2, 1];
            set => m_Matrix[2, 1] = value;
        }

        public float[,] m_Matrix = new float[3, 2];

        // Array subscripts
        public float this[int row, int column]
        {
            get => m_Matrix[column, row];
            set => m_Matrix[column, row] = value;
        }

        //////////////////////////////////////////////////////////////////////////
        public Matrix2x3(Matrix2x3 m)
        {
            SetValue(m[0, 0], m[0, 1], m[0, 2], m[1, 0], m[1, 1], m[1, 2]);
        }

        public Matrix2x3(float m00, float m01, float m02, float m10, float m11, float m12)
        {
            SetValue(m00, m01, m02, m10, m11, m12);
        }

        public void SetValue(float m00, float m01, float m02, float m10, float m11, float m12)
        {
            this[0, 0] = m00;
            this[0, 1] = m01;
            this[0, 2] = m02;
            this[1, 0] = m10;
            this[1, 1] = m11;
            this[1, 2] = m12;
        }

        //////////////////////////////////////////////////////////////////////////
        public static Vector2 operator *(Vector2 point, Matrix2x3 r)
        {
            return new Vector2(point.x * r.m00 + point.y * r.m01 + r.m02, point.x * r.m10 + point.y * r.m11 + r.m12);
        }

        public static Matrix2x3 operator *(Matrix2x3 l, Matrix2x3 r)
        {
            var result = new Matrix2x3(l);
            for (var row = 0; row < 3; row++)
            for (var column = 0; column < 3; column++)
            {
                result[row, column] = l[row, 0] * r[0, column];
                for (var i = 1; i < 3; i++)
                    result[row, column] += l[row, i] * r[i, column];
            }

            return result;
        }

        public static Matrix2x3 Rotation(float degree)
        {
            var rad = degree * Mathf.Deg2Rad;
            var sin = Mathf.Sin(rad);
            var cos = Mathf.Cos(rad);
            return new Matrix2x3(cos, -sin, 0, sin, cos, 0);
        }

        public static Matrix2x3 Scale(float scale)
        {
            return new Matrix2x3(scale, 0, 0,
                                 0, scale, 0);
        }
        
        public static Matrix2x3 Scale(Vector2 scale)
        {
            return new Matrix2x3(scale.x, 0, 0,
                                 0, scale.y, 0);
        }

        public static Matrix2x3 Identity { get; } = new Matrix2x3(1, 0, 0, 0, 1, 0);

        public static Matrix2x3 TranslationRotationScale(Vector2 translation, float degree, Vector2 scale)
        {
            var rad = degree * Mathf.Deg2Rad;
            var sin = Mathf.Sin(rad);
            var cos = Mathf.Cos(rad);
            return new Matrix2x3(cos * scale.y, -sin * scale.y, sin * scale.x, cos * scale.x, translation.x, translation.y);
        }
    }
}