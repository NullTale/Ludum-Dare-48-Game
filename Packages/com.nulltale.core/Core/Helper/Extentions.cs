using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

namespace CoreLib
{
    public static class Extentions
    {
        private class CircularEnumarator<T> : IEnumerator<T>
        {
            private readonly IEnumerator _wrapedEnumerator;

            public CircularEnumarator(IEnumerator wrapedEnumerator)
            {
                this._wrapedEnumerator = wrapedEnumerator;
            }

            public object Current => _wrapedEnumerator.Current;

            T IEnumerator<T>.Current =>  (T)Current;

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (!_wrapedEnumerator.MoveNext())
                {
                    _wrapedEnumerator.Reset();
                    return _wrapedEnumerator.MoveNext();
                }
                return true;
            }

            public void Reset()
            {
                _wrapedEnumerator.Reset();
            }
        }

        //////////////////////////////////////////////////////////////////////////
        #region Comparison, logic operation

        public static bool Check(this global::CoreLib.Core.ConditionBool condition, bool value)
        {
            switch (condition)
            {
                case global::CoreLib.Core.ConditionBool.True:
                    return value == true;
                case global::CoreLib.Core.ConditionBool.False:
                    return value == false;
                case global::CoreLib.Core.ConditionBool.AlwaysTrue:
                    return true;
                case global::CoreLib.Core.ConditionBool.AlwaysFalse:
                    return false;
                default:
                    throw new ArgumentOutOfRangeException(nameof(condition), condition, null);
            }
        }
    
        public static bool Check(this global::CoreLib.Core.LogicOperation operation, bool a, bool b)
        {
            switch (operation)
            {
                case global::CoreLib.Core.LogicOperation.And:
                    return a && b;
                case global::CoreLib.Core.LogicOperation.Or:
                    return a || b;
                case global::CoreLib.Core.LogicOperation.Equal:
                    return a == b;
                case global::CoreLib.Core.LogicOperation.NotEqual:
                    return a != b;
                default:
                    throw new ArgumentOutOfRangeException(nameof(operation), operation, null);
            }
        }

        public static bool Check(this global::CoreLib.Core.ComparisonOperation comparison, float a, float b)
        {
            switch (comparison)
            {
                case global::CoreLib.Core.ComparisonOperation.Less:
                    return a < b;
                case global::CoreLib.Core.ComparisonOperation.Greater:
                    return a > b;
                case global::CoreLib.Core.ComparisonOperation.Equal:
                    return a == b;
                case global::CoreLib.Core.ComparisonOperation.NotEqual:
                    return a != b;
                case global::CoreLib.Core.ComparisonOperation.LessOrEqual:
                    return a <= b;
                case global::CoreLib.Core.ComparisonOperation.GreaterOrEqual:
                    return a >= b;
                case global::CoreLib.Core.ComparisonOperation.Any:
                    return true;
                case global::CoreLib.Core.ComparisonOperation.None:
                    return false;
                default:
                    throw new ArgumentOutOfRangeException(nameof(comparison), comparison, null);
            }
        }
    
        public static bool Check<T>(this global::CoreLib.Core.ComparisonOperation comparison, T a, T b) where T : IComparable
        {
            switch (comparison)
            {
                case global::CoreLib.Core.ComparisonOperation.Less:
                    return a.CompareTo(b) < 0;
                case global::CoreLib.Core.ComparisonOperation.Greater:
                    return a.CompareTo(b) > 0;
                case global::CoreLib.Core.ComparisonOperation.Equal:
                    return a.CompareTo(b) == 0;
                case global::CoreLib.Core.ComparisonOperation.NotEqual:
                    return a.CompareTo(b) != 0;
                case global::CoreLib.Core.ComparisonOperation.LessOrEqual:
                    return a.CompareTo(b) <= 0;
                case global::CoreLib.Core.ComparisonOperation.GreaterOrEqual:
                    return a.CompareTo(b) >= 0;
                case global::CoreLib.Core.ComparisonOperation.Any:
                    return true;
                case global::CoreLib.Core.ComparisonOperation.None:
                    return false;
                default:
                    throw new ArgumentOutOfRangeException(nameof(comparison), comparison, null);
            }
        }
    
        public static bool Check(this global::CoreLib.Core.ComparisonOperation comparison, int a, int b)
        {
            switch (comparison)
            {
                case global::CoreLib.Core.ComparisonOperation.Less:
                    return a < b;
                case global::CoreLib.Core.ComparisonOperation.Greater:
                    return a > b;
                case global::CoreLib.Core.ComparisonOperation.Equal:
                    return a == b;
                case global::CoreLib.Core.ComparisonOperation.NotEqual:
                    return a != b;
                case global::CoreLib.Core.ComparisonOperation.LessOrEqual:
                    return a <= b;
                case global::CoreLib.Core.ComparisonOperation.GreaterOrEqual:
                    return a >= b;
                case global::CoreLib.Core.ComparisonOperation.Any:
                    return true;
                case global::CoreLib.Core.ComparisonOperation.None:
                    return false;
                default:
                    throw new ArgumentOutOfRangeException(nameof(comparison), comparison, null);
            }
        }

        #endregion

        #region Coroutine extantions

        //////////////////////////////////////////////////////////////////////////
        // with delay
        public static Coroutine StartCoroutine(this MonoBehaviour obj, float delay, Action action)
        {
            return obj.StartCoroutine(DelayRun(delay, action));
        }
        public static IEnumerator DelayRun(float delay, Action action) 
        {
            yield return new WaitForSeconds(delay);
            action();
        }
        public static Coroutine StartCoroutine(this MonoBehaviour obj, int frameDelay, Action action)
        {
            return obj.StartCoroutine(DelayRun(frameDelay, action));
        }
        public static IEnumerator DelayRun(int frameCount, Action action) 
        {
            while (frameCount-- > 0)
                yield return null;

            action();
        }

        // repeat
        public static Coroutine StartCoroutine(this MonoBehaviour obj, int repeat, Action action, float repeatInterval) 
        {
            return obj.StartCoroutine(RepeatRun(repeat, action, repeatInterval));
        }
        public static IEnumerator RepeatRun(int repeat, Action action, float repeatInterval) 
        {
            if (repeat <= 0)	yield break;
        
            var interval = new WaitForSeconds(repeatInterval);

            // repeat with interval
            do
            {
                action();
                yield return interval;
            }   
            while (repeat-- >= 0);
        }

        public static Coroutine StartCoroutine(this MonoBehaviour obj, int repeat, float startInteval, Action action, float repeatInterval, Action finish) 
        {
            return obj.StartCoroutine(RepeatRun(repeat, action, repeatInterval, finish));
        }
        public static IEnumerator RepeatRun(int repeat, Action action, float repeatInterval, Action finish) 
        {
            if(repeat <= 0)	yield break;
        
            var interval = new WaitForSeconds(repeatInterval);
            // repeat with interval
            do
            {
                action();
                yield return interval;
            }
            while(repeat-- >= 0);
		
            // finish action
            finish();
        }
        public static Coroutine StartCoroutine(this MonoBehaviour obj, Action action, float repeatInterval) 
        {
            return obj.StartCoroutine(ForeverRun(action, repeatInterval));
        }
        public static IEnumerator ForeverRun(Action action, float repeatInterval) 
        {		
            var interval = new WaitForSeconds(repeatInterval);

            // run forever
            while (true)
            {
                action();
                yield return interval;
            }
        }

        public static Coroutine StartCoroutine(this MonoBehaviour obj, Func<object> action) 
        {
            return obj.StartCoroutine(ForeverRun(action));
        }
        public static IEnumerator ForeverRun(Func<object> action) 
        {
            // run forever
            while (true)
                yield return action();
        }
        
        // while
        public static Coroutine StartCoroutine(this MonoBehaviour obj, Func<bool> condition, Func<object> action)
        {
            return obj.StartCoroutine(WhileRun(condition, action));
        }
        public static IEnumerator WhileRun(Func<bool> condition, Func<object> action) 
        {
            // while run
            while (condition())
                yield return action();
        }

        public static IEnumerator ActionRun(Action action) 
        {
            action();
            yield break;
        }

        public static IEnumerator YieldRun(object y) 
        {
            yield return y;
        }

        // sequence
        public static Coroutine StartCoroutine(this MonoBehaviour obj, params IEnumerator[] coroutines) 
        {
            return obj.StartCoroutine(SequenceRun(coroutines));
        }
        public static IEnumerator SequenceRun(params IEnumerator[] coroutines)
        {
            // run sequence
            return coroutines.GetEnumerator();
        }
        

        #endregion

        #region External

        
#if UNITY_WEBGL
        [DllImport("__Internal")]
        public static extern void SyncFiles();

        [DllImport("__Internal")]
        public static extern void WindowAlert(string message);
#else
        public static void SyncFiles(){ }

        public static void WindowAlert(string message){ }
#endif

        #endregion

        #region Array2D

        public static IEnumerable<T> Take<T>(this T[,] array, in RectInt square)
        {
            return Take(array, square.xMin, square.yMin, square.xMax, square.yMax);
        }


        public static IEnumerable<T> Take<T>(this T[,] array, Vector2Int at, Vector2Int to)
        {
            return Take(array, at.x, to.x, at.y, to.y);
        }

        public static IEnumerable<T> Take<T>(this T[,] array, int xMin, int yMin, int xMax, int yMax)
        {
            xMin = Mathf.Clamp(xMin, 0, array.GetLength(0));
            xMax = Mathf.Clamp(xMax, 0, array.GetLength(0));
            yMin = Mathf.Clamp(yMin, 0, array.GetLength(1));
            yMax = Mathf.Clamp(yMax, 0, array.GetLength(1));

            for (var y = yMin; y < yMax; y++)
            for (var x = xMin; x < xMax; x++)
                yield return array[x, y];
        }

        public static T GetValue<T>(this T[,] array, in Vector2Int index)
        {
            return array[index.x, index.y];
        }

        public static T GetValueSafe<T>(this T[,] array, in Vector2Int index)
        {
            return array.GetValueSafe(index.x, index.y);
        }
        
        public static T GetValueSafe<T>(this T[,] array, int x, int y)
        {
            return InBounds(array, x, y) ? array[x, y] : default;
        }

        public static void SetValue<T>(this T[,] array, T value, in Vector2Int index)
        {
            array[index.x, index.y] = value;
        }

        public static void SetValueSafe<T>(this T[,] array, in Vector2Int index, T value)
        {
            SetValueSafe(array, index.x, index.y, value);
        }

        public static void SetValueSafe<T>(this T[,] array, int x, int y, T value)
        {
            if (InBounds(array, x, y))
                array[x, y] = value;
        }

        public static bool InBounds<T>(this T[,] array, in Vector2Int index)
        {
            return InBounds(array, index.x, index.y);
        }

        public static bool InBounds<T>(this T[,] array, int x, int y)
        {
            return x >= 0 && x < array.GetLength(0) && y >= 0 && y < array.GetLength(1);
        }

        public static bool TrySetValue<T>(this T[,] array, in Vector2Int index, T value)
        {
            return TrySetValue(array, index.x, index.y, value);
        }

        public static bool TrySetValue<T>(this T[,] array, int x, int y, T value)
        {
            if (InBounds(array, x, y))
            {
                array[x, y] = value;
                return true;
            }
            
            return false;
        }

        public static bool TryGetValue<T>(this T[,] array, in Vector2Int index, out T value)
        {
            return TryGetValue(array, index.x, index.y, out value);
        }

        public static bool TryGetValue<T>(this T[,] array, int x, int y, out T value)
        {
            if (InBounds(array, x, y))
            {
                value = array[x, y];
                return true;
            }

            value = default;
            return false;
        }

        public static IEnumerable<T> ToEnumerable<T>(this T[,] array)
        {
            for (var y = 0; y < array.GetLength(1); y++)
            for (var x = 0; x < array.GetLength(0); x++)
                yield return array[x, y];
        }
        public static IEnumerable<(int x, int y, T value)> Enumerate<T>(this T[,] array)
        {
            for (var y = 0; y < array.GetLength(1); y++)
            for (var x = 0; x < array.GetLength(0); x++)
                yield return (x, y, array[x, y]);
        }

        public static List<T> ToList<T>(this T[,] array)
        {
            var result = new List<T>(array.GetLength(0) * array.GetLength(1));

            foreach (var element in ToEnumerable(array))
                result.Add(element);

            return result;
        }
        
        public static void Initialize<T>(this T[,] array, Action<int, int, T[,]> action)
        {
            for (var x = 0; x < array.GetLength(0); x++)
            for (var y = 0; y < array.GetLength(1); y++)
                action(x, y, array);
        }

        public static void Initialize<T>(this T[,] array, Func<int, int, T> action)
        {
            for (var x = 0; x < array.GetLength(0); x++)
            for (var y = 0; y < array.GetLength(1); y++)
                array[x, y] = action(x, y);
        }

        #endregion

        #region Zip

        public static byte[] Zip(this string str) 
        {
            var bytes = Encoding.UTF8.GetBytes(str);

            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream()) 
            {
                using (var gs = new GZipStream(mso, CompressionMode.Compress)) 
                    msi.CopyTo(gs);

                return mso.ToArray();
            }
        }

        public static string UnzipString(this byte[] data)
        {
            return Encoding.UTF8.GetString(data.Unzip());
        }
        
        public static byte[] Zip(this byte[] data) 
        {
            using (var msi = new MemoryStream(data))
            using (var mso = new MemoryStream()) 
            {
                using (var gs = new GZipStream(mso, CompressionMode.Compress)) 
                    msi.CopyTo(gs);

                return mso.ToArray();
            }
        }

        public static byte[] Unzip(this byte[] bytes) 
        {
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                    gs.CopyTo(mso);

                return mso.ToArray();
            }
        }

        #endregion

        #region Rect

        public static RectInt ToRectInt(this Rect rect)
        {
            return new RectInt((int)rect.xMin, (int)rect.yMin, Mathf.CeilToInt(rect.width), Mathf.CeilToInt(rect.height));
        }
        
        public static Rect ToRect(this RectInt rect)
        {
            return new Rect(rect.xMin, rect.yMin, rect.width, rect.height);
        }
        
        public static Rect WithXY(this Rect rect, float xMin, float yMin)
        {
            return new Rect(xMin, yMin, rect.width, rect.height);
        }

        public static Rect WithWH(this Rect rect, float width, float height)
        {
            return new Rect(rect.xMin, rect.yMin, width, height);
        }
        
        public static Rect GetRowDown(this Rect rect, int line, float lineHeight)
        {
            return new Rect(rect.xMin, rect.yMax - lineHeight * (line + 1), rect.width, lineHeight);
        }
        
        public static Rect GetRowUp(this Rect rect, int line, float lineHeight)
        {
            return new Rect(rect.xMin, rect.yMin + lineHeight * (line + 1), rect.width, lineHeight);
        }
        
#if UNITY_EDITOR
        public static Rect DrawerLine(this Rect rect, int line)
        {
            return rect.GetRowDown(line, UnityEditor.EditorGUIUtility.singleLineHeight);
        }
        
        public static T GetSerializedValue<T>(this UnityEditor.SerializedProperty property)
        {
            object targetObject = property.serializedObject.targetObject;
            string[] propertyNames = property.propertyPath.Split('.');
     
            // Clear the property path from "Array" and "data[i]".
            if (propertyNames.Length >= 3 && propertyNames[propertyNames.Length - 2] == "Array")
                propertyNames = propertyNames.Take(propertyNames.Length - 2).ToArray();
     
            // Get the last object of the property path.
            foreach (string path in propertyNames)
            {
                targetObject = targetObject.GetType()
                    .GetField(path, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                    .GetValue(targetObject);
            }
     
            if (targetObject.GetType().GetInterfaces().Contains(typeof(IList<T>)))
            {
                int propertyIndex = int.Parse(property.propertyPath[property.propertyPath.Length - 2].ToString());
     
                return ((IList<T>) targetObject)[propertyIndex];
            }
            else return (T) targetObject;
        }
#endif

        public static RectInt FrameCrop(this RectInt rect, RectInt region)
        {
            if (region.xMin < rect.xMin)
                region.xMin = rect.xMin;
            
            if (region.xMax > rect.xMax)
                region.xMax = rect.xMax;
            
            if (region.yMin < rect.yMin)
                region.yMin = rect.yMin;

            if (region.yMax > rect.yMax)
                region.yMax = rect.yMax;

            return region;
        }

        public static RectInt FrameOf(this RectInt rect, RectInt region)
        {
            if (rect.width < region.width)
                region.width = rect.width;
            
            if (rect.height < region.height)
                region.height = rect.height;
            
            if (region.x < rect.xMin)
                region.x = rect.xMin;
            
            if (region.xMax > rect.xMax)
                region.x = rect.xMax - region.width;
            
            if (region.y < rect.yMin)
                region.y = rect.yMin;

            if (region.yMax > rect.yMax)
                region.y = rect.yMax - region.height;

            return region;
        }

        public static RectInt WithXY(this RectInt rect, int xMin, int yMin)
        {
            return new RectInt(xMin, yMin, rect.width, rect.height);
        }

        public static RectInt WithWH(this RectInt rect, int width, int height)
        {
            return new RectInt(rect.xMin, rect.yMin, width, height);
        }


        #endregion

        #region Texture

        public static RectInt Rect(this Texture2D texture)
        {
            return new RectInt(0, 0, texture.width, texture.height);
        }

        public static Texture2D Copy(this Texture2D texture, TextureFormat format = TextureFormat.RGBA32)
        {
            return texture.Copy(new RectInt(0, 0, texture.width, texture.height), format);
        }

        public static Texture2D Copy(this Texture2D texture, RectInt rect, TextureFormat format = TextureFormat.RGBA32)
        {
            var dst = new Texture2D(rect.width, rect.height, format, false, false);
            try
            {
                dst.filterMode = texture.filterMode;
                dst.SetPixels(0, 0, rect.width, rect.height, texture.GetPixels(rect.x, rect.y, rect.width, rect.height), 0);
                dst.Apply();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Can't copy texture {e}");
            }
            /*try
            {
                Graphics.CopyTexture(texture, 0, 0, rect.x, rect.y, rect.width, rect.height, dst, 0, 0, 0, 0);
            }
            catch 
            {
            }*/

            return dst;
        }

        #endregion

        //////////////////////////////////////////////////////////////////////////
        public static IEnumerable<T> ToIEnumerable<T>(this IEnumerator<T> enumerator) 
        {
            while (enumerator.MoveNext()) 
                yield return enumerator.Current;
        }

        public static IEnumerator<T> ToCircular<T>(this IEnumerable<T> t) 
        {
            return new CircularEnumarator<T>(t.GetEnumerator());
        }

        public static T Next<T>(this IEnumerator<T> enumerator)
        {
            if (enumerator.MoveNext())
                return enumerator.Current;

            return default;
        }

        
        public static IEnumerable<IEnumerable<T>> GetPermutations<T>(this IEnumerable<T> items)
        {
            var itemsArray = items as T[] ?? items.ToArray();
            return GetPermutations(itemsArray, itemsArray.Length);
        }

        public static IEnumerable<IEnumerable<T>> GetPermutations<T>(this IEnumerable<T> items, int count)
        {
            var itemsArray = items as T[] ?? items.ToArray();
            return GetPermutations(itemsArray, count);
        }

        public static IEnumerable<IEnumerable<T>> GetPermutations<T>(T[] items, int count)
        {
            if (count == 1)
            {
                foreach (var item in items)
                    yield return new T[] {item};

                yield break;
            }

            foreach(var item in items)
            {
                foreach (var result in GetPermutations(items.Except(new [] { item }).ToArray(), count - 1))
                    yield return new T[] { item }.Concat(result);
            }
        }

        public static string FullActionName(this InputAction inputAction)
        {
            if (inputAction.actionMap == null)
                return inputAction.name;

            return inputAction.actionMap.name + '/' + inputAction.name;
        }


        private class GeneralPropertyComparer<T,TKey> : IEqualityComparer<T>
        {
            private Func<T, TKey> expr { get; }

            public GeneralPropertyComparer (Func<T, TKey> expr)
            {
                this.expr = expr;
            }

            public bool Equals(T left, T right)
            {
                var leftProp  = expr.Invoke(left);
                var rightProp = expr.Invoke(right);

                if (leftProp == null && rightProp == null)
                    return true;

                if (leftProp == null ^ rightProp == null)
                    return false;

                return leftProp.Equals(rightProp);
            }
            public int GetHashCode(T obj)
            {
                var prop = expr.Invoke(obj);
                return (prop==null)? 0:prop.GetHashCode();
            }
        }

        public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> items, Func<T, TKey> property)
        {
            return items.Distinct(new GeneralPropertyComparer<T,TKey>(property));
        }

        public static LinkedListNode<T> FirstOrDefault<T>(this LinkedList<T> source, Func<LinkedListNode<T>, bool> predicate)
        {
            for (var current = source.First; current != null;  current = current.Next)
                if (predicate(current))
                    return current;

            return null;
        }
        
        public static bool Implements<T>(this Type source) where T : class
        {
            return typeof(T).IsAssignableFrom(source);
        }

        public static IEnumerable<Type> GetBaseTypes(this Type source)
        {
            var current = source;

            while (current != null)
            {
                yield return current;
                current = current.BaseType;
            }
        }

        public static IEnumerable<T> GetEnum<T>() where T : Enum
        {
            return Enum.GetValues(typeof(T)).OfType<T>();
        }

        public static void DrawEllipse(Vector3 pos, float radius, Color color, int segments = 20, float duration = 0)
        {
            DrawEllipse(pos, Vector3.forward, Vector3.up, radius, radius, color, segments, duration);
        }

        public static void DrawEllipse(Vector3 pos, Vector3 forward, Vector3 up, float radiusX, float radiusY, Color color, int segments, float duration = 0)
        {
            var angle     = 0f;
            var rot       = Quaternion.LookRotation(forward, up);
            var lastPoint = Vector3.zero;
            var thisPoint = Vector3.zero;
 
            for (int i = 0; i < segments + 1; i++)
            {
                thisPoint.x = Mathf.Sin(Mathf.Deg2Rad * angle) * radiusX;
                thisPoint.y = Mathf.Cos(Mathf.Deg2Rad * angle) * radiusY;
 
                if (i > 0)
                {
                    Debug.DrawLine(rot * lastPoint + pos, rot * thisPoint + pos, color, duration);
                }
 
                lastPoint =  thisPoint;
                angle     += 360f / segments;
            }
        }

        public static IEnumerable<T> GetValues<T>() where T : Enum
        {
            return GetEnum<T>();
        }

        public static IEnumerable<T> GetValues<T>(this T en) where T : Enum
        {
            return GetEnum<T>();
        }

        public static IEnumerable<T> GetFlags<T>(this T en) where T : Enum
        {
            foreach (T value in Enum.GetValues(typeof(T)))
                if (en.HasFlag(value))
                    yield return value;
        }

        public static T NextEnum<T>(this T en) where T : Enum
        {
            // get values
            var valueList = GetEnum<T>().ToList();

            // get value index
            var index = valueList.IndexOf(en);

            // reset or increment index
            if (index < 0 || (index + 1) >= valueList.Count)
                index = 0;
            else
                index ++;

            return valueList[index];
        }

        public static void RemoveAllBut<T>(this List<T> source, Predicate<T> predicate)
        {
            source.RemoveAll(inverse);

            bool inverse(T item) => !predicate(item);
        }

        public static bool IsEmpty<T>(this ICollection<T> collection)
        {
            return collection.Count == 0;
        }

        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> other)
        {
            //nothing to add
            if (other == null)
                return;

            foreach (var obj in other)
            {
                collection.Add(obj);
            }
        }
    
        public static IEnumerable<Transform> GetChildren(this GameObject obj)
        {
            for (var n = 0; n < obj.transform.childCount; n++)
                yield return obj.transform.GetChild(n);
        }

        public static void DestroyChildren(this GameObject obj)
        {
            var childList = obj.GetChildren().ToList();

#if UNITY_EDITOR
            if (Application.isPlaying)
                foreach (var child in childList)
                    UnityEngine.Object.Destroy(child.gameObject);
            else
                foreach (var child in childList)
                    UnityEngine.Object.DestroyImmediate(child.gameObject);
#else
        foreach (var child in childList)
            UnityEngine.Object.Destroy(child.gameObject);
#endif
        
        }

        public static bool Has<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            return source.Any(predicate);
        }
    
        public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, TSource noOptionsValue = default)
        {
            return source.MinBy(selector, Comparer<TKey>.Default, noOptionsValue);
        }
    
        public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, IComparer<TKey> comparer, TSource noOptionsValue = default)
        {
            using (var sourceIterator = source.GetEnumerator())
            {
                if (sourceIterator.MoveNext() == false)
                    return noOptionsValue;

                var min = sourceIterator.Current;
                var minKey = selector(min);
	
                while (sourceIterator.MoveNext())
                {
                    var candidate = sourceIterator.Current;
                    var candidateProjected = selector(candidate);

                    if (comparer.Compare(candidateProjected, minKey) < 0)
                    {
                        min = candidate;
                        minKey = candidateProjected;
                    }
                }
                return min;
            }
        }

        public static TSource MaxBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, TSource noOptionsValue = default)
        {
            return source.MaxBy(selector, Comparer<TKey>.Default, noOptionsValue);
        }
    
        public static TSource MaxBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, IComparer<TKey> comparer, TSource noOptionsValue = default)
        {
            using (var sourceIterator = source.GetEnumerator())
            {
                if (sourceIterator.MoveNext() == false)
                    return noOptionsValue;

                var max = sourceIterator.Current;
                var maxKey = selector(max);
	
                while (sourceIterator.MoveNext())
                {
                    var candidate = sourceIterator.Current;
                    var candidateProjected = selector(candidate);

                    if (comparer.Compare(candidateProjected, maxKey) > 0)
                    {
                        max = candidate;
                        maxKey = candidateProjected;
                    }
                }
                return max;
            }
        }

        public static List<Transform> GetChildren(this Transform transform)
        {
            var result = new List<Transform>(transform.childCount);
            for (var n = 0; n < transform.childCount; n++)
                result.Add(transform.GetChild(n));

            return result;
        }

        public static void AddUnique<T>(this IList list, T item)
        {
            if (list.Contains(item))
                return;

            list.Add(item);
        }

        public static T RandomItem<T>(this IEnumerable<T> list)
        {
            return UnityRandom.RandomFromList(list.ToList());
        }

        public static T RandomItem<T>(this IEnumerable<T> list, T noOptionsValue)
        {
            return UnityRandom.RandomFromList(list.ToList(), noOptionsValue);
        }

        public static T RandomItem<T>(this IEnumerable<T> list, T noOptionsValue, params T[] except)
        {
            return UnityRandom.RandomFromList(list.ToList(), noOptionsValue, except);
        }

        public static T RandomItem<T>(this IList<T> list)
        {
            return UnityRandom.RandomFromList(list);
        }

        public static T RandomItem<T>(this IList<T> list, T noOptionsValue)
        {
            return UnityRandom.RandomFromList(list, noOptionsValue);
        }

        public static T RandomItem<T>(this IList<T> list, T noOptionsValue, params T[] except)
        {
            return UnityRandom.RandomFromList(list, noOptionsValue, except);
        }

        public static IList<T> RandomizeList<T>(this IList<T> list)
        {
            UnityRandom.RandomizeList(list);
            return list;
        }

        public static bool TryGetValue<T>(this IList<T> list, int index, out T value)
        {
            if (index < 0 || index >= list.Count)
            {
                value = default;
                return false;
            }

            value = list[index];
            return true;
        }
        
        public static T PrevItem<T>(this IList<T> list, T item)
        {
            var index = list.IndexOf(item);
            if (index == -1 || index - 1 < 0)
                return default;

            return list[index - 1];
        }

        public static T NextItem<T>(this IList<T> list, T item)
        {
            var index = list.IndexOf(item);
            if (index == -1 || list.Count <= index + 1)
                return default;

            return list[index + 1];
        }

        public static T NextItem<T>(this IList<T> list, T item, out int index)
        {
            index = list.IndexOf(item);
            if (index == -1 || list.Count <= ++index)
                return default;

            return list[index];
        }

        public static T NextItem<T>(this IList<T> list, ref int index)
        {
            if (list.Count <= ++index)
            {
                index = -1;
                return default;
            }

            return list[index];
        }
        
        public static IEnumerable<Vector2Int> ToEnumerableSquare(this Vector2Int vec)
        {
            for (var x = 0; x < vec.x; x++)
            for (var y = 0; y < vec.y; y++)
                yield return new Vector2Int(x, y);
        }

        public static Vector2Int RandomSquare(this Vector2Int vec)
        {
            return new Vector2Int(UnityEngine.Random.Range(0, vec.x), UnityEngine.Random.Range(0, vec.y));
        }

        public static Vector2Int RandomSquare(this Vector2Int vec, Vector2Int increment)
        {
            return new Vector2Int(UnityEngine.Random.Range(0, vec.x + increment.x), UnityEngine.Random.Range(0, vec.y + increment.y));
        }

        /// <summary>
        /// Returns a copy of this vector with the given x-coordinate.
        /// </summary>
        public static Vector2 WithX(this Vector2 vector, float x)
        {
            return new Vector2(x, vector.y);
        }

        /// <summary>
        /// Returns a copy of this vector with the given y-coordinate.
        /// </summary>
        public static Vector2 WithY(this Vector2 vector, float y)
        {
            return new Vector2(vector.x, y);
        }

        /// <summary>
        /// Returns a copy of this vector with the given x-coordinate.
        /// </summary>
        public static Vector3 WithX(this Vector3 vector, float x)
        {
            return new Vector3(x, vector.y, vector.z);
        }

        /// <summary>
        /// Returns a copy of this vector with the given y-coordinate.
        /// </summary>
        public static Vector3 WithY(this Vector3 vector, float y)
        {
            return new Vector3(vector.x, y, vector.z);
        }

        /// <summary>
        /// Returns a copy of this vector with the given z-coordinate.
        /// </summary>
        public static Vector3 WithZ(this Vector3 vector, float z)
        {
            return new Vector3(vector.x, vector.y, z);
        }

        /// <summary>
        /// Returns a copy of the vector with the x-coordinate incremented
        /// with the given value.
        /// </summary>
        public static Vector2 WithIncX(this Vector2 vector, float xInc)
        {
            return new Vector2(vector.x + xInc, vector.y);
        }

        /// <summary>
        /// Returns a copy of the vector with the y-coordinate incremented
        /// with the given value.
        /// </summary>
        public static Vector2 WithIncY(this Vector2 vector, float yInc)
        {
            return new Vector2(vector.x, vector.y + yInc);
        }

        /// <summary>
        /// Returns a copy of the vector with the x-coordinate incremented
        /// with the given value.
        /// </summary>
        public static Vector3 WithIncX(this Vector3 vector, float xInc)
        {
            return new Vector3(vector.x + xInc, vector.y, vector.z);
        }

        /// <summary>
        /// Returns a copy of the vector with the y-coordinate incremented
        /// with the given value.
        /// </summary>
        public static Vector3 WithIncY(this Vector3 vector, float yInc)
        {
            return new Vector3(vector.x, vector.y + yInc, vector.z);
        }

        /// <summary>
        /// Returns a copy of the vector with the z-coordinate incremented
        /// with the given value.
        /// </summary>
        public static Vector3 WithIncZ(this Vector3 vector, float zInc)
        {
            return new Vector3(vector.x, vector.y, vector.z + zInc);
        }

        /// <summary>
        /// Converts a 2D vector to a 3D vector using the vector 
        /// for the x and z coordinates, and the given value for the y coordinate.
        /// </summary>
        public static Vector3 To3DXZ(this Vector2 vector, float y)
        {
            return new Vector3(vector.x, y, vector.y);
        }

        /// <summary>
        /// Converts a 2D vector to a 3D vector using the vector 
        /// for the x and z coordinates, and 0 for the y coordinate.
        /// </summary>
        public static Vector3 To3DXZ(this Vector2 vector)
        {
            return vector.To3DXZ(0);
        }

        /// <summary>
        /// Converts a 2D vector to a 3D vector using the vector 
        /// for the x and y coordinates, and the given value for the z coordinate.
        /// </summary>
        public static Vector3 To3DXY(this Vector2 vector, float z)
        {
            return new Vector3(vector.x, vector.y, z);
        }

        /// <summary>
        /// Converts a 2D vector to a 3D vector using the vector 
        /// for the x and y coordinates, and 0 for the z coordinate.
        /// </summary>
        public static Vector3 To3DXY(this Vector2 vector)
        {
            return vector.To3DXY(0);
        }

        /// <summary>
        /// Converts a 2D vector to a 3D vector using the vector 
        /// for the y and z coordinates, and the given value for the x coordinate.
        /// </summary>
        public static Vector3 To3DYZ(this Vector2 vector, float x)
        {
            return new Vector3(x, vector.x, vector.y);
        }

        /// <summary>
        /// Converts a 2D vector to a 3D vector using the vector 
        /// for the y and z coordinates, and 0 for the x coordinate.
        /// </summary>
        public static Vector3 To3DYZ(this Vector2 vector)
        {
            return vector.To3DYZ(0);
        }

        /// <summary>
        /// Converts a 3D vector to a 2D vector taking the x and z coordinates.
        /// </summary>
        public static Vector2 To2DXZ(this Vector3 vector)
        {
            return new Vector2(vector.x, vector.z);
        }

        /// <summary>
        /// Converts a 3D vector to a 2D vector taking the x and y coordinates.
        /// </summary>
        public static Vector2 To2DXY(this Vector3 vector)
        {
            return new Vector2(vector.x, vector.y);
        }

        /// <summary>
        /// Converts a 3D vector to a 2D vector taking the y and z coordinates.
        /// </summary>
        public static Vector2 To2DYZ(this Vector3 vector)
        {
            return new Vector2(vector.y, vector.z);
        }

        /// <summary>
        /// Swaps the x and y coordinates of the vector.
        /// </summary>
        public static Vector2 YX(this Vector2 vector)
        {
            return new Vector2(vector.y, vector.x);
        }

        /// <summary>
        /// Creates a new vector by permuting the given vector's coordinates in the order YZX.
        /// </summary>
        public static Vector3 YZX(this Vector3 vector)
        {
            return new Vector3(vector.y, vector.z, vector.x);
        }

        /// <summary>
        /// Creates a new vector by permuting the given vector's coordinates in the order XZY.
        /// </summary>
        public static Vector3 XZY(this Vector3 vector)
        {
            return new Vector3(vector.x, vector.z, vector.y);
        }

        /// <summary>
        /// Creates a new vector by permuting the given vector's coordinates in the order ZXY.
        /// </summary>
        public static Vector3 ZXY(this Vector3 vector)
        {
            return new Vector3(vector.z, vector.x, vector.y);
        }

        /// <summary>
        /// Creates a new vector by permuting the given vector's coordinates in the order YXZ.
        /// </summary>
        public static Vector3 YXZ(this Vector3 vector)
        {
            return new Vector3(vector.y, vector.x, vector.z);
        }

        /// <summary>
        /// Creates a new vector by permuting the given vector's coordinates in the order ZYX.
        /// </summary>
        public static Vector3 ZYX(this Vector3 vector)
        {
            return new Vector3(vector.z, vector.y, vector.x);
        }

        /// <summary>
        /// Reflects the vector about x-axis.
        /// </summary>
        public static Vector2 ReflectAboutX(this Vector2 vector)
        {
            return new Vector2(vector.x, -vector.y);
        }

        /// <summary>
        /// Reflects the vector about y-axis.
        /// </summary>
        public static Vector2 ReflectAboutY(this Vector2 vector)
        {
            return new Vector2(-vector.x, vector.y);
        }
	
        /// <summary>
        /// Rotates a vector by a given angle.
        /// </summary>
        /// <param name="vector">vector to rotate</param>
        /// <param name="angleInDeg">angle in degrees.</param>
        /// <returns>Rotated vector.</returns>
        public static Vector2 Rotate(this Vector2 vector, float angleInDeg)
        {
            float angleInRad = Mathf.Deg2Rad * angleInDeg;
            float cosAngle = Mathf.Cos(angleInRad);
            float sinAngle = Mathf.Sin(angleInRad);

            float x = vector.x * cosAngle - vector.y * sinAngle;
            float y = vector.x * sinAngle + vector.y * cosAngle;

            return new Vector2(x, y);
        }

        /// <summary>
        /// Rotates a vector by a given angle around a given point.
        /// </summary>
        public static Vector2 RotateAround(this Vector2 vector, float angleInDeg, Vector2 axisPosition)
        {
            return (vector - axisPosition).Rotate(angleInDeg) + axisPosition;
        }

        /// <summary>
        /// Rotates a vector by a 90 degrees.
        /// </summary>
        public static Vector2 Rotate90(this Vector2 vector)
        {
            return new Vector2(-vector.y, vector.x);
        }

        /// <summary>
        /// Rotates a vector by a 180 degrees.
        /// </summary>
        public static Vector2 Rotate180(this Vector2 vector)
        {
            return new Vector2(-vector.x, -vector.y);
        }

        /// <summary>
        /// Rotates a vector by a 270 degrees.
        /// </summary>
        public static Vector2 Rotate270(this Vector2 vector)
        {
            return new Vector2(vector.y, -vector.x);
        }

        /// <summary>
        /// Returns the vector rotated 90 degrees counter-clockwise.
        /// </summary>
        /// <remarks>
        /// 	<para>The returned vector is always perpendicular to the given vector. </para>
        /// 	<para>The perp dot product can be calculated using this: <c>var perpDotPorpduct = Vector2.Dot(v1.Perp(), v2);</c></para>
        /// </remarks>
        /// <param name="vector"></param>
        public static Vector2 Perp(this Vector2 vector)
        {
            return vector.Rotate90();
        }

        /// <summary>
        /// Equivalent to Vector2.Dot(v1.Perp(), v2).
        /// </summary>
        /// <param name="vector1">The first operand.</param>
        /// <param name="vector2">The second operand.</param>
        /// <returns>Vector2.</returns>
        public static float PerpDot(this Vector2 vector1, Vector2 vector2)
        {
            return -vector1.y*vector2.x + vector1.x*vector2.y;
        }

        /// <summary>
        /// Equivalent to Vector2.Dot(v1, v2).
        /// </summary>
        /// <param name="vector1">The first operand.</param>
        /// <param name="vector2">The second operand.</param>
        /// <returns>Vector2.</returns>
        public static float Dot(this Vector2 vector1, Vector2 vector2)
        {
            return vector1.x * vector2.x + vector1.y * vector2.y;
        }

        /// <summary>
        /// Equivalent to Vector3.Dot(v1, v2).
        /// </summary>
        /// <param name="vector1">The first operand.</param>
        /// <param name="vector2">The second operand.</param>
        /// <returns>Vector3.</returns>
        public static float Dot(this Vector3 vector1, Vector3 vector2)
        {
            return vector1.x * vector2.x + vector1.y * vector2.y + vector1.z * vector2.z;
        }

        /// <summary>
        /// Equivalent to Vector4.Dot(v1, v2).
        /// </summary>
        /// <param name="vector1">The first operand.</param>
        /// <param name="vector2">The second operand.</param>
        /// <returns>Vector4.</returns>
        public static float Dot(this Vector4 vector1, Vector4 vector2)
        {
            return vector1.x * vector2.x + vector1.y * vector2.y + vector1.z * vector2.z + vector1.w * vector2.w;
        }

        /// <summary>
        /// Returns the projection of this vector onto the given base.
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="baseVector"></param>
        public static Vector2 Proj(this Vector2 vector, Vector2 baseVector)
        {
            var direction = baseVector.normalized;
            var magnitude = Vector2.Dot(vector, direction);

            return direction * magnitude;
        }

        /// <summary>
        /// Returns the rejection of this vector onto the given base.
        /// </summary>
        /// <remarks>
        /// 	<para>The sum of a vector's projection and rejection on a base is equal to
        /// the original vector.</para>
        /// </remarks>
        /// <param name="vector"></param>
        /// <param name="baseVector"></param>
        public static Vector2 Rej(this Vector2 vector, Vector2 baseVector)
        {
            return vector - vector.Proj(baseVector);
        }

        /// <summary>
        /// Returns the projection of this vector onto the given base.
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="baseVector"></param>

        public static Vector3 Proj(this Vector3 vector, Vector3 baseVector)
        {
            var direction = baseVector.normalized;
            var magnitude = Vector2.Dot(vector, direction);

            return direction * magnitude;
        }

        /// <summary>
        /// Returns the rejection of this vector onto the given base.
        /// </summary>
        /// <remarks>
        /// 	<para>The sum of a vector's projection and rejection on a base is equal to
        /// the original vector.</para>
        /// </remarks>
        /// <param name="vector"></param>
        /// <param name="baseVector"></param>
        public static Vector3 Rej(this Vector3 vector, Vector3 baseVector)
        {
            return vector - vector.Proj(baseVector);
        }

        /// <summary>
        /// Returns the projection of this vector onto the given base.
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="baseVector"></param>
        public static Vector4 Proj(this Vector4 vector, Vector4 baseVector)
        {
            var direction = baseVector.normalized;
            var magnitude = Vector2.Dot(vector, direction);

            return direction * magnitude;
        }

        /// <summary>
        /// Returns the rejection of this vector onto the given base.
        /// The sum of a vector's projection and rejection on a base is
        /// equal to the original vector.
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="baseVector"></param>
        public static Vector4 Rej(this Vector4 vector, Vector4 baseVector)
        {
            return vector - vector.Proj(baseVector);
        }

        /// <summary>
        /// Turns the vector 90 degrees anticlockwise as viewed from the top (keeping the y coordinate intact).
        /// Equivalent to <code>v.To2DXZ().Perp().To3DXZ(v.y);</code>
        /// </summary>
        public static Vector3 PerpXZ(this Vector3 v)
        {
            return new Vector3(-v.z, v.y, v.x);
        }

        /// <summary>
        /// Turns the vector 90 degrees anticlockwise as viewed from the front (keeping the z coordinate intact).
        /// Equivalent to <code>v.To2DXY().Perp().To3DXY(v.z);</code>
        /// </summary>

        public static Vector3 PerpXY(this Vector3 v)
        {
            return new Vector3(-v.y, v.x, v.z);
        }
    
        public static Vector2 HadamardMul(this Vector2 thisVector, Vector2 otherVector)
        {
            return new Vector2(thisVector.x * otherVector.x, thisVector.y * otherVector.y);
        }

        /// <summary>
        /// Divides one vector component by component by another.
        /// </summary>
        public static Vector2 HadamardDiv(this Vector2 thisVector, Vector2 otherVector)
        {
            return new Vector2(thisVector.x / otherVector.x, thisVector.y / otherVector.y);
        }
    
        public static Vector3 HadamardMul(this Vector3 thisVector, Vector3 otherVector)
        {
            return new Vector3(
                thisVector.x * otherVector.x, 
                thisVector.y * otherVector.y,
                thisVector.z * otherVector.z);
        }
    
        public static Vector3 HadamardDiv(this Vector3 thisVector, Vector3 otherVector)
        {
            return new Vector3(
                thisVector.x / otherVector.x, 
                thisVector.y / otherVector.y,
                thisVector.z / otherVector.z);
        }
    
        public static Vector4 HadamardMul(this Vector4 thisVector, Vector4 otherVector)
        {
            return new Vector4(
                thisVector.x * otherVector.x,
                thisVector.y * otherVector.y,
                thisVector.z * otherVector.z,
                thisVector.w * otherVector.w);
        }
        public static Vector4 HadamardDiv(this Vector4 thisVector, Vector4 otherVector)
        {
            return new Vector4(
                thisVector.x / otherVector.x,
                thisVector.y / otherVector.y,
                thisVector.z / otherVector.z,
                thisVector.w / otherVector.w);
        }

			
        public static Vector2Int To2DXY(this Vector3Int v)
        {
            return new Vector2Int(v.x, v.y);
        }


        public static Vector2 ToVector2(this Vector2Int v)
        {
            return new Vector2(v.x, v.y);
        }

        public static Vector3Int To3DXY(this Vector2Int v)
        {
            return new Vector3Int(v.x, v.y, 0);
        }
        public static Vector3Int To3DXZ(this Vector2Int v)
        {
            return new Vector3Int(v.x, 0, v.y);
        }

        public static void SetMax(this Vector2Int v, Vector2Int max)
        {
            v.Set(v.x > max.x ? max.x : v.x,
                v.y > max.y ? max.y : v.y);
        }

        public static void SetMin(this Vector2Int v, Vector2Int min)
        {
            v.Set(v.x < min.x ? min.x : v.x, 
                v.y < min.y ? min.y : v.y);
        }

        public static Vector2Int Center(this Vector2Int v)
        {
            return new Vector2Int(v.x / 2, v.y / 2);
        }

        public static Vector2Int CenterRound(this Vector2Int v)
        {
            return new Vector2Int(Mathf.RoundToInt((float)v.x / 2.0f), Mathf.RoundToInt((float)v.y / 2.0f));
        }

        /// <summary>From min inclusive, to max exclusive </summary>
        public static bool InRange(this Vector2Int v, Vector2Int min, Vector2Int max)
        {
            return v.x >= min.x && v.y >= min.y 
                                 && v.x < max.x && v.y < max.y;
        }

        /// <summary>From 0 to max exclusive </summary>
        public static bool InRange(this Vector2Int v, Vector2Int max)
        {
            return v.x >= 0 && v.y >= 0 
                            && v.x < max.x && v.y < max.y;
        }
        /// <summary>From 0 to max exclusive </summary>
        public static bool InRange(this Vector2Int v, int maxX, int maxY)
        {
            return v.x >=0 && v.x < maxX 
                           && v.y >=0 && v.y < maxY;
        }

        /// <summary>From 0 to max exclusive </summary>
        public static bool InRange(this Vector2Int v, int max)
        {
            return v.x >=0 && v.x < max 
                           && v.y >=0 && v.y < max;
        }
	
        /// <summary>Max value </summary>
        public static int Max(this Vector2Int v)
        {
            return v.x > v.y ? v.x : v.y;
        }

        // min value
        public static int Min(this Vector2Int v)
        {
            return v.x < v.y ? v.x : v.y;
        }
	
        // true if this vector in bounds(inclusive min max) of argument vector, none argument check
        public static bool InRangeOfInc(this Vector2 v, Vector2 range)
        {
            return v.x >= range.x && v.y <= range.y;
            // && v.x <= range.y && v.y >= range.x
        }

        public static bool InRangeOfInc(this Vector2 v, float pos)
        {
            return v.x <= pos && pos <= v.y;
        }

        public static float ClosesdValue(this Vector2 v, float pos)
        {
            return Mathf.Abs(v.x - pos) < Mathf.Abs(v.y - pos) ? v.x : v.y;
        }

        public static Vector2 Abs(this Vector2 v)
        {
            return new Vector2(v.x < 0.0f ? -v.x : v.x, v.y < 0.0f ? -v.y : v.y);
        }

        public static Vector3 Abs(this Vector3 v)
        {
            return new Vector3(v.x < 0.0f ? -v.x : v.x, v.y < 0.0f ? -v.y : v.y, v.z < 0.0f ? -v.z : v.z);
        }

        public static Vector2 ClampLenght(this Vector2 v, float lenghtAbs)
        {
            var lenght = v.magnitude;
            if (lenght > lenghtAbs)
                v *= lenghtAbs / lenght;

            return v;
        }
    
        public static Vector3 ToVector3X(this float value)
        {
            return new Vector3(value, 0, 0);
        }
        public static Vector3 ToVector3Y(this float value)
        {
            return new Vector3(0, value, 0);
        }
        public static Vector3 ToVector3Z(this float value)
        {
            return new Vector3(0, 0, value);
        }
        
        public static float Angle(this Vector2 v)
        {
            return Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
        }

        public static Vector2Int Round(this Vector2 v)
        {
            return new Vector2Int(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y));
        }
        public static Vector2Int Ceil(this Vector2 v)
        {
            return new Vector2Int(Mathf.CeilToInt(v.x), Mathf.CeilToInt(v.y));
        }
        public static Vector2Int Floor(this Vector2 v)
        {
            return new Vector2Int(Mathf.FloorToInt(v.x), Mathf.FloorToInt(v.y));
        }
        public static Vector2Int ToVector2Int(this Vector2 v)
        {
            return new Vector2Int((int)v.x, (int)v.y);
        }

        public static Vector2 Clamp(this Vector2 v, float min, float max)
        {
            return new Vector2(Mathf.Clamp(v.x, min, max), Mathf.Clamp(v.y, min, max));
        }

        public static Vector3 Clamp(this Vector3 v, float min, float max)
        {
            return new Vector3(Mathf.Clamp(v.x, min, max), Mathf.Clamp(v.y, min, max), Mathf.Clamp(v.z, min, max));
        }

        public static Vector2Int Clamp(this Vector2Int v, int min, int max)
        {
            return new Vector2Int(Mathf.Clamp(v.x, min, max), Mathf.Clamp(v.y, min, max));
        }

        public static Vector3Int Clamp(this Vector3Int v, int min, int max)
        {
            return new Vector3Int(Mathf.Clamp(v.x, min, max), Mathf.Clamp(v.y, min, max), Mathf.Clamp(v.z, min, max));
        }
	
        public static Vector2 Normal(this float rad)
        {
            return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        }
	
	
        public static int ToInt(this Vector2Int v)
        {
            //return unchecked(v.x | (v.y << 15));
            return v.x | (v.y << 16);
        }

        public static Vector2Int ToVector2Int(this int v)
        {
            return new Vector2Int(v & 0b0000_0000_0000_0000_1111_1111_1111_1111, v >> 16);
        }

        public static int Sum(this Vector2Int v)
        {
            return v.x + v.y;
        }

        public static int SumAbs(this Vector2Int v)
        {
            return Mathf.Abs(v.x) + Mathf.Abs(v.y);
        }

        public static float Evaluate(this ParticleSystem.MinMaxCurve curve)
        {
            return curve.Evaluate(Random.value, Random.value);
        }

        public static float FinishTime(this AnimationCurve curve)
        {
            return curve.keys[curve.length - 1].time;
        }

        public static float StartTime(this AnimationCurve curve)
        {
            return curve.keys[0].time;
        }

        public static float Duration(this AnimationCurve curve)
        {
            return curve.FinishTime() - curve.StartTime();
        }

        public static Color WithA(this Color color, float a)
        {
            return new Color(color.a, color.g, color.b, a);
        }
    }

    public static class Actions
    {
        public static void Empty() { }
        public static void Empty<T>(T value) { }
        public static void Empty<T1, T2>(T1 value1, T2 value2) { }
    }

    public static class Functions
    {
        public static T Identity<T>(T value) { return value; }

        public static T Default<T>() { return default; }

        public static bool IsNull<T>(T entity) where T : class { return entity == null; }
        public static bool IsNonNull<T>(T entity) where T : class { return entity != null; }

        public static bool True<T>(T entity) { return true; }
        public static bool False<T>(T entity) { return false; }
    }
}