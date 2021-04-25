using System;
using System.Collections.Generic;
using UnityEngine;

namespace CoreLib
{
    public partial class Core
    {
        public static class Math
        {
            public const float GoldenRatio = 1.61803398875f;

            //////////////////////////////////////////////////////////////////////////
            public static float Fib(float baseFib, int iterations, float stepLimit = Single.MaxValue)
            {
                var a = 0.0f;
                var b = baseFib;
                var c = 0.0f;

                for (var n = 0; n < iterations; n++)
                {
                    c = Mathf.Min(a, stepLimit) + b;
                    a = b;
                    b = c;
                }

                return c;
            }

            public static void Fib(float baseFib, int iterations, List<float> values, float stepLimit = Single.MaxValue)
            {
                var a = 0.0f;
                var b = baseFib;

                values.Add(a);
                values.Add(b);

                for (var n = 0; n < iterations; n++)
                {
                    var c = Mathf.Min(a, stepLimit) + b;
                    a = b;
                    b = c;
                    values.Add(c);
                }
            }
        }
    }
}